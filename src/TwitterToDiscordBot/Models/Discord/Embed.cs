using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterToDiscordBot.Models.Discord
{
    class Embed
    {
        public string? Description { get; set; }
        public string? Url { get; set; }
        public int Color { get; set; }
        public Author? Author { get; set; }
        public Image? Image { get; set; }
    }
}
