namespace GitHubTopRepos.Data.Entities
{
    public class TopRepositoryData
    {
        /// <summary>
        /// Repository Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Full repository name
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// URL to repository avatar
        /// </summary>
        public string AvatarUrl { get; set; }

        /// <summary>
        /// Repository description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Url of repository
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Star gazers count (stargazers_count and watchers have the same value as watchers_count)
        /// </summary>
        public int StargazersCount { get; set; }

        /// <summary>
        /// Programing language
        /// </summary>
        public string Language { get; set; }


    }
}
