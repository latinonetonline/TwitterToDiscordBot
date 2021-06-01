using AivenEcommerce.V1.Modules.GitHub.Services;

using Octokit;

using System.Text.Json;
using System.Threading.Tasks;

using TwitterToDiscordBot.Models;

namespace TwitterToDiscordBot.Services
{
    class GithubService
    {

        private const long RepositoryId = 251758832;
        private const string FileName = "result.json";
        private const string Path = nameof(TwitterToDiscordBot);

        private static readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        private readonly ApplicationParameters _parameters;

        public GithubService(ApplicationParameters parameters)
        {
            _parameters = parameters;
        }

        public async Task UploadResultAsync(Result result)
        {
            IGitHubService gitHubService = GetService();

            bool fileExist = await gitHubService.ExistFileAsync(RepositoryId, Path, FileName);

            if (fileExist)
            {
                await gitHubService.UpdateFileAsync(RepositoryId, Path, FileName, JsonSerializer.Serialize(result, jsonOptions));
            }
            else
            {
                await gitHubService.CreateFileAsync(RepositoryId, Path, FileName, JsonSerializer.Serialize(result, jsonOptions));
            }
        }


        public async Task<Result?> GetLastResultAsync()
        {
            IGitHubService gitHubService = GetService();

            bool fileExist = await gitHubService.ExistFileAsync(RepositoryId, Path, FileName);

            if (fileExist)
            {
                var fileContent = await gitHubService.GetFileContentAsync(RepositoryId, Path, FileName);
                return JsonSerializer.Deserialize<Result>(fileContent.Content, jsonOptions);
            }
            else
            {
                return null;
            }
        }


        private IGitHubService GetService()
        {
            GitHubClient githubClient = new(new Octokit.ProductHeaderValue(nameof(TwitterToDiscordBot)));

            Octokit.Credentials basicAuth = new(_parameters.GitHubToken);

            githubClient.Credentials = basicAuth;

            IGitHubService gitHubService = new GitHubService(githubClient);

            return gitHubService;
        }
    }
}
