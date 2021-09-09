using System.Threading.Tasks;
using GitHubTopRepos.Data.Entities;

namespace GitHubTopRepos.Repositories
{
    public interface IRequestTrackingRepository
    {
        Task<RequestLog> GetRecentRequests(int count);
        Task<Language> GetLanguages();
    }
}
