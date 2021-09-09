using System;
using System.Collections.Generic;
using System.Linq;

namespace GitHubTopRepos.Configuration
{
    [Serializable]
    public class GitHubOptions
    {
        public string ApiUrl { get; set; }
        public string SearchEndpoint { get; set; }
        public string RepositoryPath { get; set; }
        public string AccessToken { get; set; }


        public void SetConnectionString(string connectionString)
        {
            var configuration = connectionString
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s =>
                {
                    var a = s.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    return a.Length == 2 ? new KeyValuePair<string, string>(a[0].Trim(), a[1].Trim()) : default;
                }).ToDictionary(k => k.Key, v => v.Value);

            ApiUrl = configuration["apiUrl"];
            SearchEndpoint = configuration["searchPath"];
            RepositoryPath = configuration["repoPath"];
            AccessToken = configuration["accessToken"];
        }
    }
}
