
using AivenEcommerce.V1.Modules.GitHub.Services;

using GitHubActionSharp;

using LinqToTwitter;
using LinqToTwitter.Common;
using LinqToTwitter.Common.Entities;
using LinqToTwitter.OAuth;

using Octokit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

using TwitterToDiscordBot.Models;
using TwitterToDiscordBot.Models.Discord;

namespace TwitterToDiscordBot
{
    enum Parameters
    {
        [Parameter("gh-token")]
        GitHubToken,

        [Parameter("tw-api-key")]
        TwitterApiKey,

        [Parameter("tw-api-secret")]
        TwitterApiSecret,

        [Parameter("discord-webhook")]
        DiscordWebhook
    }


    class Program
    {
        const long RepositoryId = 251758832;

        static async Task Main(string[] args)
        {
            GitHubActionContext actionContext = new(args);
            actionContext.LoadParameters();

            string ghToken = actionContext.GetParameter(Parameters.GitHubToken);
            string twitterApiKey = actionContext.GetParameter(Parameters.TwitterApiKey);
            string twitterApisecret = actionContext.GetParameter(Parameters.TwitterApiSecret);
            string discordWebhook = actionContext.GetParameter(Parameters.DiscordWebhook);

            Result? result = await GetLastResutlAsync(ghToken);

            ulong lastTweetStatusId = result?.LastTweetStatusId ?? 0;

            IEnumerable<Status>? status = await GetTweets(twitterApiKey, twitterApisecret, lastTweetStatusId);

            if (status.Any())
            {
                Console.WriteLine($"Se obtuvieron {status.Count()} nuevos tweets.");

                await SendDiscord(discordWebhook, status);

                await UploadResultAsync(new(DateTime.Now, status.First().StatusID), ghToken);
            }
            else
            {
                Console.WriteLine($"No hay ningún tweet nuevo.");

                await UploadResultAsync(new(DateTime.Now, lastTweetStatusId), ghToken);

            }

            Console.WriteLine("Finish");
        }


        static async Task SendDiscord(string discordWebhook, IEnumerable<Status> status)
        {
            HttpClient httpClient = new();

            foreach (Status item in status)
            {

                if (item.FullText is not null)
                {
                    string fullText = item.FullText;

                    if (item.Entities?.UrlEntities is not null)
                    {
                        foreach (UrlEntity entity in item.Entities.UrlEntities)
                        {
                            if (entity.Url is not null)
                                fullText = fullText.Replace(entity.Url, entity.ExpandedUrl);
                        }
                    }

                    if (item.Entities?.MediaEntities is not null)
                    {
                        foreach (MediaEntity entity in item.Entities.MediaEntities)
                        {
                            if (entity.Url is not null)
                                fullText = fullText.Replace(entity.Url, entity.ExpandedUrl);
                        }
                    }



                    DiscordWebhookContent webhookContent = new()
                    {
                        Username = "Twitter Bot",
                        Content = $"https://twitter.com/LauchaCarro/status/{item.StatusID}",
                        Embeds = new List<Embed>()
                    {
                        new()
                        {
                            Description = fullText,
                            Author = new()
                            {
                                Name = item.User?.Name ?? "Latino .Net Online",
                                Url = $"https://twitter.com/{item.User?.ScreenNameResponse ?? "latinonetonline"}",
                                Icon_url = item.User?.ProfileImageUrlHttps
                            },
                            Color = 8716463,
                            Url = "https://twitter.com/latinonetonline/status/{item.StatusID}"
                        }
                    }

                    };


                    if (item.Entities?.MediaEntities is not null)
                    {
                        if (item.Entities.MediaEntities.Count == 1)
                        {
                            webhookContent.Embeds.First().Image = new()
                            {
                                Url = item.Entities.MediaEntities.First().MediaUrlHttps
                            };
                        }

                        if (item.Entities.MediaEntities.Count > 1)
                        {
                            for (int i = 1; i < item.Entities.MediaEntities.Count; i++)
                            {
                                webhookContent.Embeds.Add(new Embed
                                {
                                    Image = new()
                                    {
                                        Url = item.Entities.MediaEntities[i].MediaUrlHttps
                                    }
                                });
                            }
                        }
                    }

                    Console.WriteLine($"Sending to Discord...");

                    HttpResponseMessage response = await httpClient.PostAsJsonAsync(discordWebhook, webhookContent);

                    await Task.Delay(2000);
                }
            }
        }


        static async Task<IEnumerable<Status>> GetTweets(string twitterApiKey, string twitterApisecret, ulong lastTweetStatusId)
        {
            InMemoryCredentialStore inMemoryCredentialStore = new();

            inMemoryCredentialStore.ConsumerKey = twitterApiKey;
            inMemoryCredentialStore.ConsumerSecret = twitterApisecret;


            XAuthAuthorizer xAuthAuthorizer = new();
            xAuthAuthorizer.CredentialStore = inMemoryCredentialStore;

            TwitterContext twitterCtx = new(xAuthAuthorizer);

            string searchTerm = "from:latinonetonline";

            Search? searchResponse =
                await
                (from search in twitterCtx.Search
                 where search.Type == SearchType.Search &&
                       search.Query == searchTerm &&
                       search.IncludeEntities == true &&
                       search.SinceID == lastTweetStatusId &&
                       search.TweetMode == TweetMode.Extended &&
                       search.Count == 50
                 select search)
                .SingleOrDefaultAsync();

            return searchResponse?.Statuses ?? Enumerable.Empty<Status>();
        }



        static async Task UploadResultAsync(Result result, string githubToken)
        {

            GitHubClient githubClient = new(new Octokit.ProductHeaderValue(nameof(TwitterToDiscordBot)));

            Octokit.Credentials basicAuth = new(githubToken);

            githubClient.Credentials = basicAuth;

            IGitHubService gitHubService = new GitHubService(githubClient);

            string path = nameof(TwitterToDiscordBot);
            string fileName = "result.json";

            bool fileExist = await gitHubService.ExistFileAsync(RepositoryId, path, fileName);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            if (fileExist)
            {
                await gitHubService.UpdateFileAsync(RepositoryId, path, fileName, JsonSerializer.Serialize(result, options));
            }
            else
            {
                await gitHubService.CreateFileAsync(RepositoryId, path, fileName, JsonSerializer.Serialize(result, options));
            }
        }

        static async Task<Result?> GetLastResutlAsync(string githubToken)
        {

            GitHubClient githubClient = new(new Octokit.ProductHeaderValue(nameof(TwitterToDiscordBot)));

            Octokit.Credentials basicAuth = new(githubToken);

            githubClient.Credentials = basicAuth;

            IGitHubService gitHubService = new GitHubService(githubClient);

            string path = nameof(TwitterToDiscordBot);
            string fileName = "result.json";

            bool fileExist = await gitHubService.ExistFileAsync(RepositoryId, path, fileName);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            if (fileExist)
            {
                var fileContent = await gitHubService.GetFileContentAsync(RepositoryId, path, fileName);
                return JsonSerializer.Deserialize<Result>(fileContent.Content, options);
            }
            else
            {
                return null;
            }
        }
    }
}
