using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using GitHubTopRepos.Data.Entities;
using GitHubTopRepos.Resilience;

namespace GitHubTopRepos.Repositories
{
    public class RequestTrackingRepository : IRequestTrackingRepository
    {
        private readonly IRequestTrackingContext _context;
        private readonly IExecutionPolicies _executionPolicies;
        private readonly ILogger<RequestTrackingRepository> _logger;

        public RequestTrackingRepository(IRequestTrackingContext context,
            IExecutionPolicies executionPolicies,
            ILogger<RequestTrackingRepository> logger)
        {
            _context = context;
            _executionPolicies = executionPolicies;
            _logger = logger;
        }

        private readonly Func<DbDataReader, RequestLog> CreateGetRequestLogs = reader =>
        {
            return new RequestLog
            {
                DateRequested = Convert.ToDateTime(reader[nameof(RequestLog.DateRequested)]),
                Language = reader[nameof(RequestLog.Language)].ToString().Trim(),
                RecordsReturned  = Convert.ToBoolean(reader[nameof(RequestLog.RecordsReturned)]),
                RequestQuery = reader[nameof(RequestLog.RequestQuery)].ToString().Trim()
            };
        };

        /// <summary>
        /// Returns set of recent api calls
        /// </summary>
        /// <param name="count">Number of records to return</param>
        /// <returns></returns>
        public async Task<RequestLog> GetRecentRequests(int count)
        {
            var sql = string.Format("select date_requested, language, records_returned, request_query from request_log order by date_requested desc limit {0}", count);
            var policyResult = await _executionPolicies.DbPolicy.ExecuteAndCaptureAsync(() => _context.ExecuteReaderAsync(sql, CommandType.Text, null, CreateGetRequestLogs));

            if (policyResult.FinalException != null)
            {
                _logger.LogError(policyResult.FinalException, $"Unexpected exception when retrieving request_log data");
                return null;
            }

            return policyResult.Result;
        }

        private readonly Func<DbDataReader, Language> CreateLanguageResponse = reader =>
        {
            return new Language
            {
                Name = reader[nameof(Language.Name)].ToString().Trim(),
                Popular = Convert.ToBoolean(reader[nameof(Language.Popular)]),
            };
        };

        /// <summary>
        /// Returns list of valid GitHub languages
        /// </summary>
        /// <returns></returns>
        public async Task<Language> GetLanguages()
        {
            var sql = "select name, popular from language";
            var policyResult = await _executionPolicies.DbPolicy.ExecuteAndCaptureAsync(() => _context.ExecuteReaderAsync(sql, CommandType.Text, null, CreateLanguageResponse));

            if (policyResult.FinalException != null)
            {
                _logger.LogError(policyResult.FinalException, $"Unexpected exception when retrieving language data");
                return null;
            }

            return policyResult.Result;
        }
    }
}
