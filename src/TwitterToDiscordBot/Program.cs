
using GitHubActionSharp;

using LinqToTwitter;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using TwitterToDiscordBot.Extensions;
using TwitterToDiscordBot.Models;
using TwitterToDiscordBot.Services;

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
        static async Task Main(string[] args)
        {
            GitHubActionContext actionContext = new(args);
            actionContext.LoadParameters();

            ApplicationParameters parameters = new(
                actionContext.GetParameter(Parameters.GitHubToken),
                actionContext.GetParameter(Parameters.TwitterApiKey),
                actionContext.GetParameter(Parameters.TwitterApiSecret),
                actionContext.GetParameter(Parameters.DiscordWebhook),
                "latinonetonline"
                );

            HttpClient httpClient = new();
            GithubService githubService = new(parameters);
            TwitterService twitterService = new(parameters);
            DiscordService discordService = new(httpClient, parameters);

            Result? lastResult = await githubService.GetLastResultAsync();

            IEnumerable<Status>? statuses = await twitterService.GetTweetsAsync(lastResult);

            if (statuses.Any())
            {
                Console.WriteLine($"Se obtuvieron {statuses.Count()} nuevos tweets.");

                foreach (Status status in statuses.Reverse())
                {
                    await discordService.SendDiscordAsync(status);

                    await Task.Delay(2000);
                }

                ulong statusIdToSave = statuses.First().StatusID;

                Console.WriteLine($"Se guarda el TweetStatusId: " + statusIdToSave);

                await githubService.UploadResultAsync(new(DateTime.Now, statusIdToSave));
            }
            else
            {
                Console.WriteLine($"No hay ningún tweet nuevo.");

                await githubService.UploadResultAsync(new(DateTime.Now, lastResult.GetLastTweetStatusId()));

            }

            Console.WriteLine("Finish");
        }












    }
}
