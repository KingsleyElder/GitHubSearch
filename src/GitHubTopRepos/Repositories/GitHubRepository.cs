using GitHubTopRepos.Configuration;
using GitHubTopRepos.Data.Entities;
using GitHubTopRepos.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GitHubTopRepos.Repositories
{
    public class GitHubRepository : IGitHubRepository
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly GitHubOptions _gitHubOptions;
        private readonly ILogger<GitHubRepository> _logger;
        private readonly Assembly _assembly;
        private readonly string[] _assemblyResources;
        private const string LanguageDataFile = "GitHubLanguages.json";

        public GitHubRepository(IHttpClientFactory httpClientFactory,
            IOptions<GitHubOptions> gitHubOptions,
            ILogger<GitHubRepository> logger)
        {
            _httpClientFactory = httpClientFactory;
            _gitHubOptions = gitHubOptions.Value;
            _logger = logger;
            _assembly = Assembly.GetAssembly(this.GetType());
            _assemblyResources = _assembly.GetManifestResourceNames();
        }

        /// <summary>
        /// Get the top repositories based on highest star gazers count
        /// </summary>
        /// <param name="count">Number of top records to return</param>
        /// <returns>GitHub API response entity containing an array of repository objects</returns>
        public async Task<GitHubRepoSearchResult> GetTopRepos(int count, GitHubInput input)
        {
            var jsonResponse = string.Empty;
            var repos = new GitHubRepoSearchResult();
            try
            {
                var query = "language:" + WebUtility.UrlEncode(input.Language) + "&page=1&per_page=" + count + "&sort=stargazers_count&order=desc";
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
            return repos;
        }

        /// <summary>
        /// Returns list of valid GitHub languages
        /// </summary>
        /// <returns></returns>
        public async Task<List<GitHubInput>> GetGitHubLanguages()
        {
            var resourceName = _assemblyResources.Single(x => x.EndsWith(LanguageDataFile));
            var resourceFileStream = _assembly.GetManifestResourceStream(resourceName);

            using (var r = new StreamReader(resourceFileStream, Encoding.UTF8))
            {
                var json = await r.ReadToEndAsync();
                return JsonConvert.DeserializeObject<List<GitHubInput>>(json);
            }
        }

    }
}
