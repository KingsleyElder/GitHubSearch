using GitHubTopRepos.Data.Entities;
using GitHubTopRepos.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitHubTopRepos.Repositories
{
    public interface IGitHubRepository
    {
        Task<GitHubRepoSearchResult> GetTopRepos(int count, GitHubInput input);

        Task<List<GitHubInput>> GetGitHubLanguages();
    }
}