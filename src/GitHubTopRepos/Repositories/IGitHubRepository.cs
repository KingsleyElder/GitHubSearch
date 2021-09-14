using GitHubTopRepos.Data.Entities;
using System.Threading.Tasks;

namespace GitHubTopRepos.Repositories
{
    public interface IGitHubRepository
    {
        Task<GitHubRepoSearchResult> GetTopRepos(int count, string language);
    }
}