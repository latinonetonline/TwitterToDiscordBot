using LinqToTwitter;
using LinqToTwitter.Common;
using LinqToTwitter.OAuth;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TwitterToDiscordBot.Extensions;
using TwitterToDiscordBot.Models;

namespace TwitterToDiscordBot.Services
{
    class TwitterService
    {
        private readonly ApplicationParameters _parameters;

        public TwitterService(ApplicationParameters parameters)
        {
            _parameters = parameters;
        }

        public async Task<IEnumerable<Status>> GetTweetsAsync(Result? lastResult)
        {
            ulong lastTweetStatusId = lastResult.GetLastTweetStatusId();

            InMemoryCredentialStore inMemoryCredentialStore = new()
            {
                ConsumerKey = _parameters.TwitterApiKey,
                ConsumerSecret = _parameters.TwitterApiSecret
            };

            XAuthAuthorizer xAuthAuthorizer = new()
            {
                CredentialStore = inMemoryCredentialStore
            };

            TwitterContext twitterCtx = new(xAuthAuthorizer);

            string searchTerm = $"from:{_parameters.TwitterUsername}";

            Search? searchResponse =
                await
                (from search in twitterCtx.Search
                 where search.Type == SearchType.Search &&
                       search.Query == searchTerm &&
                       search.IncludeEntities &&
                       search.SinceID == lastTweetStatusId &&
                       search.TweetMode == TweetMode.Extended &&
                       search.Count == 50
                 select search)
                .SingleOrDefaultAsync();

            twitterCtx.Dispose();

            return searchResponse?.Statuses ?? Enumerable.Empty<Status>();
        }
    }
}
