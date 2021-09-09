# github-search-api
This is a .net core api demonstrating how to consume the GitHub api to find the most populate repositories (highest watchers count)

Source code: [GitHubSearch](https://github.com/KingsleyElder/GitHubSearch",)

# Assignment
- Submit a code repository containing a solution that would return the top 5 starred repositories from GitHub for a requested language
- The solution should store a history of recent requests and return a list to be consumed by another service
- Comment on any considerations for how the solution could be “productionized”

# My approach
## Application
- I created an API that queries the GitHub API and returns 5 repositories with the highest stargazer count
- The complete GitHub repository model is available, but the response model only contains a few interesting fields. If giving the user the option to select a valid language, need to provide input value list

## Request History Store
- Storing recent requests indicates an interest in the trending language people are querying during a window of time (not cumulative) and less important the results they viewed 
- Technical solution: Use MySQL database to store history records and valid language strings. Retrieve top 20 messages of requests sorted by creation date descending (didn’t implement paging)
- Observation: Returning a list of raw requests to a consumer may not be as valuable as a summary “list” showing top languages queried. The precision of the “star count” data may not be critical and the top 5 ranking rarely changes.

## Production Readiness
- Pipelines and config for all environment levels
- Documentation of product description, environment info and technical function
- DNS if API Gateway not available
- App and infrastructure monitoring and alerting (New Relic) response team recovery steps
