using Newtonsoft.Json;
using System;

namespace GitHubTopRepos.Data.Entities
{
    [Serializable]
    public class GitHubRepoSearchResult
    {
        [JsonProperty("total_count")]
        public int TotalCount { get; set; }

        [JsonProperty("incomplete_results")]
        public bool IncompleteResults { get; set; }

        [JsonProperty("items")]
        public GitHubRepo[] Items { get; set; }
    }
}
