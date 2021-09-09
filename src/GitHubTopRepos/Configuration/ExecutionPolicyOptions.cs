namespace GitHubTopRepos.Configuration
{
    public class ExecutionPolicyOptions
    {
        public int DbCircuitBreakerErrorCount { get; set; }
        public int DbCircuitBreakerDelay { get; set; }
        public int DbRetryCount { get; set; }
        public int DbRetryBaseDelay { get; set; }
        public int ApiCircuitBreakerErrorCount { get; set; }
        public int ApiCircuitBreakerDelay { get; set; }
        public int ApiRetryCount { get; set; }
        public int ApiRetryBaseDelay { get; set; }
    }
}
