using System.Collections.Generic;
using System.Threading.Tasks;
using GitHubTopRepos.Data.Entities;

namespace GitHubTopRepos.Repositories
{
    public interface IRequestTrackingRepository
    {
        Task<List<RequestLog>> GetRecentRequests(int count);
        Task<List<Language>> GetLanguages(bool popularOnly);
        Task InsertRequest(RequestLog request);
    }
}
