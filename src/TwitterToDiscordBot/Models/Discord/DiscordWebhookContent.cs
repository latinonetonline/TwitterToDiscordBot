using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterToDiscordBot.Models.Discord
{
    class DiscordWebhookContent
    {
        public string? Username { get; set; }
        public string? Content { get; set; }
        public List<Embed>? Embeds { get; set; }
    }
}
