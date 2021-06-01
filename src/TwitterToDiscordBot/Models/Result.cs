using System;

namespace TwitterToDiscordBot.Models
{
    record Result(DateTime Date, ulong LastTweetStatusId);
}
