using Polly;
using System.Net.Http;

namespace GitHubTopRepos.Resilience
{
    public interface IExecutionPolicies
    {
        IAsyncPolicy DbPolicy { get; }
        IAsyncPolicy<HttpResponseMessage> ApiPolicy { get; }
    }
}
