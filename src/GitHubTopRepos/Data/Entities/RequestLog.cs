using System;

#nullable disable
namespace GitHubTopRepos.Data.Entities
{
    public partial class RequestLog
    {
        public DateTime DateRequested { get; set; }
        public string Language { get; set; }
        public bool RecordsReturned { get; set; }
        public string RequestQuery { get; set; }
    }
}
