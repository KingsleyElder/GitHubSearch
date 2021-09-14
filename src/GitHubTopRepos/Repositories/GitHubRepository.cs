using GitHubTopRepos.Configuration;
using GitHubTopRepos.Data.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace GitHubTopRepos.Repositories
{
    public class GitHubRepository : IGitHubRepository
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly GitHubOptions _gitHubOptions;
        private readonly IRequestTrackingRepository _requestTrackingRepository;
        private readonly ILogger<GitHubRepository> _logger;

        public GitHubRepository(IHttpClientFactory httpClientFactory,
            IOptions<GitHubOptions> gitHubOptions,
            IRequestTrackingRepository requestTrackingRepository,
            ILogger<GitHubRepository> logger)
        {
            _httpClientFactory = httpClientFactory;
            _gitHubOptions = gitHubOptions.Value;
            _requestTrackingRepository = requestTrackingRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get the top repositories based on highest star gazers count
        /// </summary>
        /// <param name="count">Number of top records to return</param>
        /// <returns>GitHub API response entity containing an array of repository objects</returns>
        //public async Task<GitHubRepoSearchResult> GetTopRepos(int count, GitHubInput input)
        public async Task<GitHubRepoSearchResult> GetTopRepos(int count, string language)
        {
            var jsonResponse = string.Empty;
            var repos = new GitHubRepoSearchResult();
            var query = "language:" + WebUtility.UrlEncode(language) + "&page=1&per_page=" + count + "&sort=stargazers_count&order=desc";

            try
            {
                var restRequest = _gitHubOptions.ApiUrl + _gitHubOptions.SearchEndpoint + _gitHubOptions.RepositoryPath + "?q=" + query;
                var request = new HttpRequestMessage(HttpMethod.Get, restRequest);
                request.Headers.Add("Accept", "application/vnd.github.preview");
                request.Headers.Add("User-Agent", "GitHubtopRepos/1.0.0");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.preview"));
                request.Headers.Add("Authorization", "Bearer " + _gitHubOptions.AccessToken);

                var client = _httpClientFactory.CreateClient("GitHub");
                var response = await client.SendAsync(request);
                jsonResponse = response.Content.ReadAsStringAsync().Result;

                response.EnsureSuccessStatusCode();

                repos = JsonConvert.DeserializeObject(jsonResponse, typeof(GitHubRepoSearchResult)) as GitHubRepoSearchResult;

            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Failed to return results calling GetTopRepos() having json response: {jsonResponse}");
            }

            var requestLog = new RequestLog
            {
                DateRequested = DateTime.Now,
                Language = language,
                RecordsReturned = repos.TotalCount > 0,
                RequestQuery = query
            };

            await _requestTrackingRepository.InsertRequest(requestLog);

            return repos;
        }

    }
}
