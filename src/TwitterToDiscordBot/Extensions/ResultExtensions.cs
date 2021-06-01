
using TwitterToDiscordBot.Models;

namespace TwitterToDiscordBot.Extensions
{
    static class ResultExtensions
    {
        public static ulong GetLastTweetStatusId(this Result? result) => result?.LastTweetStatusId ?? 0;
    }
}
