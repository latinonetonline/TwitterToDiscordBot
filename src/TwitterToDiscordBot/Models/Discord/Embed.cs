using System;

namespace TwitterToDiscordBot.Models.Discord
{
    class Embed
    {
        public string? Description { get; set; }
        public string? Url { get; set; }
        public int Color { get; set; }
        public Author? Author { get; set; }
        public Image? Image { get; set; }
        public Footer? Footer { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
