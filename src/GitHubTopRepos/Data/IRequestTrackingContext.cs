using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace GitHubTopRepos.Data.Entities
{
   public interface IRequestTrackingContext
    {
        Task<int> ExecuteNonQueryAsync(string commandText, CommandType commandType, IEnumerable<DbParameter> parameters);
        Task<TResult> ExecuteReaderAsync<TResult>(string commandText, CommandType commandType, IEnumerable<DbParameter> parameters, Func<DbDataReader, TResult> function);
    }
}
