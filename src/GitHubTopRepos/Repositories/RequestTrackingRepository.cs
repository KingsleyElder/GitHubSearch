using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GitHubTopRepos.Data.Entities;
using GitHubTopRepos.Resilience;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

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

        private readonly Func<DbDataReader, List<RequestLog>> CreateGetRequestLogs = reader =>
        {
            var requests = new List<RequestLog>();
            while (reader.Read())
            {
                requests.Add(new RequestLog
                {
                    DateRequested = Convert.ToDateTime(reader[nameof(RequestLog.DateRequested)]),
                    Language = reader[nameof(RequestLog.Language)].ToString().Trim(),
                    RecordsReturned = Convert.ToBoolean(reader[nameof(RequestLog.RecordsReturned)]),
                    RequestQuery = reader[nameof(RequestLog.RequestQuery)].ToString().Trim()
                });
            }
            return requests;
        };

        /// <summary>
        /// Returns set of recent api calls
        /// </summary>
        /// <param name="count">Number of records to return</param>
        /// <returns>Record set of log requests</returns>
        public async Task<List<RequestLog>> GetRecentRequests(int count)
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

        private readonly Func<DbDataReader, List<Language>> CreateLanguageResponse = reader =>
        {
            var languages = new List<Language>();
            while (reader.Read())
            {
                languages.Add(new Language
                {
                    Name = WebUtility.UrlEncode(reader[nameof(Language.Name)].ToString().Trim()),
                    Popular = Convert.ToBoolean(reader[nameof(Language.Popular)]),
                });
            }
            return languages;
        };

        /// <summary>
        /// Returns list of valid GitHub languages
        /// </summary>
        /// <returns>List of languages and indication if they are popular according to GitHub</returns>
        public async Task<List<Language>> GetLanguages(bool popularOnly)
        {
            var sql = "select name, popular from language" + (popularOnly ? " where popular = 1" : "");
            var policyResult = await _executionPolicies.DbPolicy.ExecuteAndCaptureAsync(() => _context.ExecuteReaderAsync(sql, CommandType.Text, null, CreateLanguageResponse));

            if (policyResult.FinalException != null)
            {
                _logger.LogError(policyResult.FinalException, $"Unexpected exception when retrieving language data");
                return null;
            }

            return policyResult.Result;
        }

        public async Task InsertRequest(RequestLog request)
        {
            const string sql = "insert into request_log (date_requested, language, records_returned, request_query) values (@date, @language, @recordsreturned, @requestquery)";
            var parameters = new MySqlConnector.MySqlParameter[]
            {
                new MySqlConnector.MySqlParameter("@date", request.DateRequested),
                new MySqlConnector.MySqlParameter("@language", request.Language),
                new MySqlConnector.MySqlParameter("@recordsreturned", request.RecordsReturned),
                new MySqlConnector.MySqlParameter("@requestquery", request.RequestQuery)
            };

            var policyResult = await _executionPolicies.DbPolicy.ExecuteAndCaptureAsync(() => _context.ExecuteNonQueryAsync(sql, CommandType.Text, parameters));

            if (policyResult.FinalException != null)
            {
                var parms = new StringBuilder();
                foreach (var p in parameters)
                {
                    parms.Append(p.ParameterName.ToString() + "='" + p.Value.ToString() + "'; ");
                }
                _logger.LogError(policyResult.FinalException, $"Unexpected exception when InsertRequest language data: {parms}");
            }
        }
    }
}
