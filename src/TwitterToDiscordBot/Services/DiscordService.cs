using LinqToTwitter.Common.Entities;

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using TwitterToDiscordBot.Models;
using TwitterToDiscordBot.Models.Discord;

using TwitterStatus = LinqToTwitter.Status;

namespace TwitterToDiscordBot.Services
{
    class DiscordService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationParameters _parameters;

        public DiscordService(HttpClient httpClient, ApplicationParameters parameters)
        {
            _httpClient = httpClient;
            _parameters = parameters;
        }

        public async Task SendDiscordAsync(TwitterStatus status)
        {

            string username = status.User?.ScreenNameResponse ?? _parameters.TwitterUsername;


            if (status.FullText is not null)
            {
                string fullText = status.FullText;

                if (status.Entities?.UrlEntities is not null)
                {
                    foreach (UrlEntity entity in status.Entities.UrlEntities)
                    {
                        if (entity.Url is not null)
                        {
                            fullText = fullText.Replace(entity.Url, entity.ExpandedUrl);
                        }
                    }
                }

                if (status.Entities?.MediaEntities is not null)
                {
                    foreach (MediaEntity entity in status.Entities.MediaEntities)
                    {
                        if (entity.Url is not null)
                        {
                            fullText = fullText.Replace(entity.Url, entity.ExpandedUrl);
                        }
                    }
                }



                DiscordWebhookContent webhookContent = new()
                {
                    Username = "Twitter Bot",

                    Content = "https://twitter.com/LatinoNetOnline/status/" + status.StatusID,

                    Embeds = new()
                    {
                        new()
                        {
                            Description = fullText,

                            Author = new()
                            {
                                Name = status.User?.Name ?? "Latino .Net Online",

                                Url = new($"https://twitter.com/" + username),

                                Icon_url = null
                            },

                            Image = null,

                            Color = 8716463,

                            Url = new("https://twitter.com/latinonetonline/status/" + status.StatusID),

                            Timestamp = status.CreatedAt,

                            Footer = new()
                            {
                                Text = "Twitter",

                                Icon_url = new("https://abs.twimg.com/icons/apple-touch-icon-192x192.png")
                            }
                        }
                    }

                };


                if (!string.IsNullOrWhiteSpace(status.User?.ProfileImageUrlHttps))
                {
                    Models.Discord.Author? author = webhookContent.Embeds.First().Author;

                    if (author is not null)
                    {
                        author.Icon_url = new(status.User.ProfileImageUrlHttps);
                    }
                }


                if (status.Entities?.MediaEntities is not null)
                {
                    if (status.Entities.MediaEntities.Any())
                    {
                        MediaEntity mediaEntity = status.Entities.MediaEntities.First();

                        if (!string.IsNullOrWhiteSpace(mediaEntity.MediaUrlHttps))
                        {
                            webhookContent.Embeds.First().Image = new()
                            {
                                Url = new(mediaEntity.MediaUrlHttps)
                            };
                        }
                    }

                    if (status.Entities.MediaEntities.Count > 1)
                    {
                        for (int i = 1; i < status.Entities.MediaEntities.Count; i++)
                        {
                            MediaEntity mediaEntity = status.Entities.MediaEntities[i];

                            if (!string.IsNullOrWhiteSpace(mediaEntity.MediaUrlHttps))
                            {
                                webhookContent.Embeds.Add(new Embed
                                {
                                    Image = new()
                                    {
                                        Url = new(mediaEntity.MediaUrlHttps)
                                    }
                                });
                            }
                        }
                    }
                }

                Console.WriteLine($"Enviando a Discord el Tweet {status.StatusID}");

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_parameters.DiscordWebhook, webhookContent);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Se envió correctamente.");
                }
                else
                {
                    Console.WriteLine($"Falló envio.");
                }
            }
        }
    }
}
