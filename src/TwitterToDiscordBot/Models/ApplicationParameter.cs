namespace TwitterToDiscordBot.Models
{
    record ApplicationParameters(string GitHubToken, string TwitterApiKey, string TwitterApiSecret, string DiscordWebhook, string TwitterUsername);
}
