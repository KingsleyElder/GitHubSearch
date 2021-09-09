using GitHubTopRepos.Configuration;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Options;
using Polly;
using System;
using System.Collections.Immutable;
using System.Net;
using System.Net.Http;

namespace GitHubTopRepos.Resilience
{

    public class ExecutionPolicies : IExecutionPolicies
    {
        /// <summary>
        /// Array of sql error numbers that are considered transient and thus should potentially be retried
        /// </summary>
        /// <remarks>
        /// https://docs.microsoft.com/en-us/sql/relational-databases/errors-events/database-engine-events-and-errors?view=sql-server-ver15
        /// </remarks>
        private static readonly ImmutableHashSet<int> TransientSqlErrorNumbers = ImmutableHashSet.Create(-2, -1, 2, 53);

        /// <summary>
        /// Array of http status codes that are considered transient and thus should potentially be retried
        /// </summary>
        /// <remarks>
        /// https://docs.microsoft.com/en-us/dotnet/api/system.net.httpstatuscode?view=netcore-3.1
        /// </remarks>
        private static readonly ImmutableHashSet<int> TransientHttpStatusCodes = ImmutableHashSet.Create(408, 416, 418, 420, 423, 426, 429, 500, 502, 503, 504, 598);

        public static bool IsTransientSqlException(Exception exception)
        {
            var e = exception;
            while (e != null && !(e is MySqlException))
            {
                e = e.InnerException;
            }

            return e is MySqlException sqlException && TransientSqlErrorNumbers.Contains(sqlException.Number);
        }

        public static bool IsTransientHttpException(Exception exception)
        {
            var e = exception;
            while (e != null && !(e is WebException))
            {
                e = e.InnerException;
            }

            return e is WebException webException && webException.Response is HttpWebResponse httpWebResponse && TransientHttpStatusCodes.Contains((int)httpWebResponse.StatusCode);
        }

        public ExecutionPolicies(IOptions<ExecutionPolicyOptions> policyOptions)
        {
            var dbPolicyBuilder = Policy.Handle<Exception>(ex => IsTransientSqlException(ex));
            DbPolicy = dbPolicyBuilder
                .CircuitBreakerAsync(policyOptions.Value.DbCircuitBreakerErrorCount, TimeSpan.FromMilliseconds(policyOptions.Value.ApiCircuitBreakerDelay))
                .WrapAsync(dbPolicyBuilder.WaitAndRetryAsync(policyOptions.Value.DbRetryCount, attempt => TimeSpan.FromMilliseconds(policyOptions.Value.DbRetryBaseDelay * attempt)));

            var apiPolicyBuilder = Policy<HttpResponseMessage>.Handle<Exception>(ex => IsTransientHttpException(ex));
            ApiPolicy = apiPolicyBuilder
                .CircuitBreakerAsync(policyOptions.Value.ApiCircuitBreakerErrorCount, TimeSpan.FromMilliseconds(policyOptions.Value.ApiCircuitBreakerDelay))
                .WrapAsync<HttpResponseMessage>(apiPolicyBuilder.WaitAndRetryAsync(policyOptions.Value.ApiRetryCount, attempt => TimeSpan.FromMilliseconds(policyOptions.Value.ApiRetryBaseDelay * attempt)));
        }

        public IAsyncPolicy DbPolicy { get; }
        public IAsyncPolicy<HttpResponseMessage> ApiPolicy { get; }
    }
}
