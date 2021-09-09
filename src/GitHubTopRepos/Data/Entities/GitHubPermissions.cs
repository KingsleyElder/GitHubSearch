using Newtonsoft.Json;

namespace GitHubTopRepos.Data.Entities
{
    public class GitHubPermissions
    {
        [JsonProperty("admin")]
        public bool Admin { get; set; }

        [JsonProperty("maintain")]
        public bool Maintain { get; set; }

        [JsonProperty("push")]
        public bool Push { get; set; }

        [JsonProperty("triage")]
        public bool Triage { get; set; }

        [JsonProperty("pull")]
        public bool Pull { get; set; }
    }
}
