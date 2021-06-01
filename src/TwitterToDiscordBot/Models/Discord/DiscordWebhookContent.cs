using System.Collections.Generic;

namespace TwitterToDiscordBot.Models.Discord
{
    class DiscordWebhookContent
    {
        public string? Username { get; set; }
        public string? Content { get; set; }
        public List<Embed>? Embeds { get; set; }
    }
}
