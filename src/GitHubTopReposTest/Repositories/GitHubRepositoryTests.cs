using GitHubTopRepos.Configuration;
using GitHubTopRepos.Models;
using GitHubTopRepos.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GitHubTopRepos.Test.Repositories
{
    public class GitHubRepositoryTests
    {
        private Mock<IHttpClientFactory> _httpClientFactory;
        private OptionsWrapper<GitHubOptions> _gitHubOptions;
        private readonly Mock<ILogger<GitHubRepository>> _logger;
        private string _apiUrl = "https://api.github.com";

        public GitHubRepositoryTests()
        {
            _logger = new Mock<ILogger<GitHubRepository>>();
        }

        private void Setup()
        {
            _httpClientFactory = new Mock<IHttpClientFactory>();
            var options = new GitHubOptions
            {
                AccessToken = "MyToken",
                SearchEndpoint = "/search",
                ApiUrl = _apiUrl,
                RepositoryPath = "/repositories"
            };
            _gitHubOptions = new OptionsWrapper<GitHubOptions>(options);
        }

        [Fact]
        public async Task GetTopRepos_InvalidToken_NonNullResultAndLogging()
        {
            // Arrange
            var language = new GitHubInput() { Language = "C++" };
            var responseJson = "{\"message\": \"Bad credentials\",\"documentation_url\": \"https://docs.github.com/rest\"}";
            var expectedLog = "Failed to return results calling GetTopRepos() having json response: " + responseJson;
            var exceptionMessage = "Response status code does not indicate success: 401 (Unauthorized).";
            Setup();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Content = new StringContent(responseJson)
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactory.Setup(_ => _.CreateClient("GitHub")).Returns(client);

            // Act
            var repository = new GitHubRepository(_httpClientFactory.Object, _gitHubOptions, _logger.Object);
            var result = await repository.GetTopRepos(5, language);

            // Assert
            Assert.NotNull(result);
            _logger.Verify(lg => lg.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedLog)),
                It.Is<Exception>(exception => exception.Message == exceptionMessage),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public async Task GetTopRepos_BadSettingsUrlThrowsException_EmptyReturnSet()
        {
            // Arrange
            var language = new GitHubInput() { Language = "C++" };
            var expectedLog = "Failed to return results calling GetTopRepos() having json response: ";
            var exceptionMessage = "Only 'http' and 'https' schemes are allowed. (Parameter 'requestUri')";
            _apiUrl = "XXXhttps://api.github.com/search/butter";
            Setup();

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent(string.Empty)
                });

            // Act
            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactory.Setup(_ => _.CreateClient("GitHub")).Returns(client);

            var repository = new GitHubRepository(_httpClientFactory.Object, _gitHubOptions, _logger.Object);
            var result = await repository.GetTopRepos(5, language);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
            _logger.Verify(lg => lg.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedLog)),
                It.Is<Exception>(exception => exception.Message == exceptionMessage),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }


        [Fact]
        public async Task GetTopRepos_ValidConditions_ReturnsResults()
        {
            // Arrange
            var language = new GitHubInput() { Language = "C++" };
            var expectedTotalCount = 3013541;
            var expectedItemsCount = 5;
            Setup();

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = BuildSearchContents()
                });

            // Act
            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactory.Setup(_ => _.CreateClient("GitHub")).Returns(client);

            var repository = new GitHubRepository(_httpClientFactory.Object, _gitHubOptions, _logger.Object);
            var result = await repository.GetTopRepos(5, language);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTotalCount, result.TotalCount);
            Assert.Equal(expectedItemsCount, result.Items.Length);
        }


        private StringContent BuildSearchContents()
        {
            return new StringContent("{\"total_count\":3013541,\"incomplete_results\":true,\"items\":[{\"id\":184456251,\"node_id\":\"MDEwOlJlcG9zaXRvcnkxODQ0NTYyNTE=\",\"name\":\"PowerToys\",\"full_name\":\"microsoft/PowerToys\",\"private\":false,\"owner\":{\"login\":\"microsoft\",\"id\":6154722,\"node_id\":\"MDEyOk9yZ2FuaXphdGlvbjYxNTQ3MjI=\",\"avatar_url\":\"https://avatars.githubusercontent.com/u/6154722?v=4\",\"gravatar_id\":\"\",\"url\":\"https://api.github.com/users/microsoft\",\"html_url\":\"https://github.com/microsoft\",\"followers_url\":\"https://api.github.com/users/microsoft/followers\",\"following_url\":\"https://api.github.com/users/microsoft/following{/other_user}\",\"gists_url\":\"https://api.github.com/users/microsoft/gists{/gist_id}\",\"starred_url\":\"https://api.github.com/users/microsoft/starred{/owner}{/repo}\",\"subscriptions_url\":\"https://api.github.com/users/microsoft/subscriptions\",\"organizations_url\":\"https://api.github.com/users/microsoft/orgs\",\"repos_url\":\"https://api.github.com/users/microsoft/repos\",\"events_url\":\"https://api.github.com/users/microsoft/events{/privacy}\",\"received_events_url\":\"https://api.github.com/users/microsoft/received_events\",\"type\":\"Organization\",\"site_admin\":false},\"html_url\":\"https://github.com/microsoft/PowerToys\",\"description\":\"Windowssystemutilitiestomaximizeproductivity\",\"fork\":false,\"url\":\"https://api.github.com/repos/microsoft/PowerToys\",\"forks_url\":\"https://api.github.com/repos/microsoft/PowerToys/forks\",\"keys_url\":\"https://api.github.com/repos/microsoft/PowerToys/keys{/key_id}\",\"collaborators_url\":\"https://api.github.com/repos/microsoft/PowerToys/collaborators{/collaborator}\",\"teams_url\":\"https://api.github.com/repos/microsoft/PowerToys/teams\",\"hooks_url\":\"https://api.github.com/repos/microsoft/PowerToys/hooks\",\"issue_events_url\":\"https://api.github.com/repos/microsoft/PowerToys/issues/events{/number}\",\"events_url\":\"https://api.github.com/repos/microsoft/PowerToys/events\",\"assignees_url\":\"https://api.github.com/repos/microsoft/PowerToys/assignees{/user}\",\"branches_url\":\"https://api.github.com/repos/microsoft/PowerToys/branches{/branch}\",\"tags_url\":\"https://api.github.com/repos/microsoft/PowerToys/tags\",\"blobs_url\":\"https://api.github.com/repos/microsoft/PowerToys/git/blobs{/sha}\",\"git_tags_url\":\"https://api.github.com/repos/microsoft/PowerToys/git/tags{/sha}\",\"git_refs_url\":\"https://api.github.com/repos/microsoft/PowerToys/git/refs{/sha}\",\"trees_url\":\"https://api.github.com/repos/microsoft/PowerToys/git/trees{/sha}\",\"statuses_url\":\"https://api.github.com/repos/microsoft/PowerToys/statuses/{sha}\",\"languages_url\":\"https://api.github.com/repos/microsoft/PowerToys/languages\",\"stargazers_url\":\"https://api.github.com/repos/microsoft/PowerToys/stargazers\",\"contributors_url\":\"https://api.github.com/repos/microsoft/PowerToys/contributors\",\"subscribers_url\":\"https://api.github.com/repos/microsoft/PowerToys/subscribers\",\"subscription_url\":\"https://api.github.com/repos/microsoft/PowerToys/subscription\",\"commits_url\":\"https://api.github.com/repos/microsoft/PowerToys/commits{/sha}\",\"git_commits_url\":\"https://api.github.com/repos/microsoft/PowerToys/git/commits{/sha}\",\"comments_url\":\"https://api.github.com/repos/microsoft/PowerToys/comments{/number}\",\"issue_comment_url\":\"https://api.github.com/repos/microsoft/PowerToys/issues/comments{/number}\",\"contents_url\":\"https://api.github.com/repos/microsoft/PowerToys/contents/{+path}\",\"compare_url\":\"https://api.github.com/repos/microsoft/PowerToys/compare/{base}...{head}\",\"merges_url\":\"https://api.github.com/repos/microsoft/PowerToys/merges\",\"archive_url\":\"https://api.github.com/repos/microsoft/PowerToys/{archive_format}{/ref}\",\"downloads_url\":\"https://api.github.com/repos/microsoft/PowerToys/downloads\",\"issues_url\":\"https://api.github.com/repos/microsoft/PowerToys/issues{/number}\",\"pulls_url\":\"https://api.github.com/repos/microsoft/PowerToys/pulls{/number}\",\"milestones_url\":\"https://api.github.com/repos/microsoft/PowerToys/milestones{/number}\",\"notifications_url\":\"https://api.github.com/repos/microsoft/PowerToys/notifications{?since,all,participating}\",\"labels_url\":\"https://api.github.com/repos/microsoft/PowerToys/labels{/name}\",\"releases_url\":\"https://api.github.com/repos/microsoft/PowerToys/releases{/id}\",\"deployments_url\":\"https://api.github.com/repos/microsoft/PowerToys/deployments\",\"created_at\":\"2019-05-01T17:44:02Z\",\"updated_at\":\"2021-09-01T01:39:22Z\",\"pushed_at\":\"2021-08-31T19:19:33Z\",\"git_url\":\"git://github.com/microsoft/PowerToys.git\",\"ssh_url\":\"git@github.com:microsoft/PowerToys.git\",\"clone_url\":\"https://github.com/microsoft/PowerToys.git\",\"svn_url\":\"https://github.com/microsoft/PowerToys\",\"homepage\":\"\",\"size\":283691,\"stargazers_count\":59169,\"watchers_count\":59169,\"language\":\"C#\",\"has_issues\":true,\"has_projects\":true,\"has_downloads\":true,\"has_wiki\":true,\"has_pages\":false,\"forks_count\":3246,\"mirror_url\":null,\"archived\":false,\"disabled\":false,\"open_issues_count\":1934,\"license\":{\"key\":\"mit\",\"name\":\"MITLicense\",\"spdx_id\":\"MIT\",\"url\":\"https://api.github.com/licenses/mit\",\"node_id\":\"MDc6TGljZW5zZTEz\"},\"forks\":3246,\"open_issues\":1934,\"watchers\":59169,\"default_branch\":\"master\",\"permissions\":{\"admin\":false,\"maintain\":false,\"push\":false,\"triage\":false,\"pull\":true},\"score\":1.0},{\"id\":7600409,\"node_id\":\"MDEwOlJlcG9zaXRvcnk3NjAwNDA5\",\"name\":\"shadowsocks-windows\",\"full_name\":\"shadowsocks/shadowsocks-windows\",\"private\":false,\"owner\":{\"login\":\"shadowsocks\",\"id\":3006190,\"node_id\":\"MDEyOk9yZ2FuaXphdGlvbjMwMDYxOTA=\",\"avatar_url\":\"https://avatars.githubusercontent.com/u/3006190?v=4\",\"gravatar_id\":\"\",\"url\":\"https://api.github.com/users/shadowsocks\",\"html_url\":\"https://github.com/shadowsocks\",\"followers_url\":\"https://api.github.com/users/shadowsocks/followers\",\"following_url\":\"https://api.github.com/users/shadowsocks/following{/other_user}\",\"gists_url\":\"https://api.github.com/users/shadowsocks/gists{/gist_id}\",\"starred_url\":\"https://api.github.com/users/shadowsocks/starred{/owner}{/repo}\",\"subscriptions_url\":\"https://api.github.com/users/shadowsocks/subscriptions\",\"organizations_url\":\"https://api.github.com/users/shadowsocks/orgs\",\"repos_url\":\"https://api.github.com/users/shadowsocks/repos\",\"events_url\":\"https://api.github.com/users/shadowsocks/events{/privacy}\",\"received_events_url\":\"https://api.github.com/users/shadowsocks/received_events\",\"type\":\"Organization\",\"site_admin\":false},\"html_url\":\"https://github.com/shadowsocks/shadowsocks-windows\",\"description\":\"AC#portofshadowsocks\",\"fork\":false,\"url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows\",\"forks_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/forks\",\"keys_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/keys{/key_id}\",\"collaborators_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/collaborators{/collaborator}\",\"teams_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/teams\",\"hooks_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/hooks\",\"issue_events_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/issues/events{/number}\",\"events_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/events\",\"assignees_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/assignees{/user}\",\"branches_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/branches{/branch}\",\"tags_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/tags\",\"blobs_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/git/blobs{/sha}\",\"git_tags_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/git/tags{/sha}\",\"git_refs_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/git/refs{/sha}\",\"trees_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/git/trees{/sha}\",\"statuses_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/statuses/{sha}\",\"languages_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/languages\",\"stargazers_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/stargazers\",\"contributors_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/contributors\",\"subscribers_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/subscribers\",\"subscription_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/subscription\",\"commits_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/commits{/sha}\",\"git_commits_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/git/commits{/sha}\",\"comments_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/comments{/number}\",\"issue_comment_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/issues/comments{/number}\",\"contents_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/contents/{+path}\",\"compare_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/compare/{base}...{head}\",\"merges_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/merges\",\"archive_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/{archive_format}{/ref}\",\"downloads_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/downloads\",\"issues_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/issues{/number}\",\"pulls_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/pulls{/number}\",\"milestones_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/milestones{/number}\",\"notifications_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/notifications{?since,all,participating}\",\"labels_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/labels{/name}\",\"releases_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/releases{/id}\",\"deployments_url\":\"https://api.github.com/repos/shadowsocks/shadowsocks-windows/deployments\",\"created_at\":\"2013-01-14T07:54:16Z\",\"updated_at\":\"2021-09-01T01:36:15Z\",\"pushed_at\":\"2021-07-26T10:04:32Z\",\"git_url\":\"git://github.com/shadowsocks/shadowsocks-windows.git\",\"ssh_url\":\"git@github.com:shadowsocks/shadowsocks-windows.git\",\"clone_url\":\"https://github.com/shadowsocks/shadowsocks-windows.git\",\"svn_url\":\"https://github.com/shadowsocks/shadowsocks-windows\",\"homepage\":\"\",\"size\":13482,\"stargazers_count\":52865,\"watchers_count\":52865,\"language\":\"C#\",\"has_issues\":true,\"has_projects\":true,\"has_downloads\":true,\"has_wiki\":true,\"has_pages\":false,\"forks_count\":16101,\"mirror_url\":null,\"archived\":false,\"disabled\":false,\"open_issues_count\":42,\"license\":{\"key\":\"gpl-3.0\",\"name\":\"GNUGeneralPublicLicensev3.0\",\"spdx_id\":\"GPL-3.0\",\"url\":\"https://api.github.com/licenses/gpl-3.0\",\"node_id\":\"MDc6TGljZW5zZTk=\"},\"forks\":16101,\"open_issues\":42,\"watchers\":52865,\"default_branch\":\"main\",\"permissions\":{\"admin\":false,\"maintain\":false,\"push\":false,\"triage\":false,\"pull\":true},\"score\":1.0},{\"id\":49609581,\"node_id\":\"MDEwOlJlcG9zaXRvcnk0OTYwOTU4MQ==\",\"name\":\"PowerShell\",\"full_name\":\"PowerShell/PowerShell\",\"private\":false,\"owner\":{\"login\":\"PowerShell\",\"id\":11524380,\"node_id\":\"MDEyOk9yZ2FuaXphdGlvbjExNTI0Mzgw\",\"avatar_url\":\"https://avatars.githubusercontent.com/u/11524380?v=4\",\"gravatar_id\":\"\",\"url\":\"https://api.github.com/users/PowerShell\",\"html_url\":\"https://github.com/PowerShell\",\"followers_url\":\"https://api.github.com/users/PowerShell/followers\",\"following_url\":\"https://api.github.com/users/PowerShell/following{/other_user}\",\"gists_url\":\"https://api.github.com/users/PowerShell/gists{/gist_id}\",\"starred_url\":\"https://api.github.com/users/PowerShell/starred{/owner}{/repo}\",\"subscriptions_url\":\"https://api.github.com/users/PowerShell/subscriptions\",\"organizations_url\":\"https://api.github.com/users/PowerShell/orgs\",\"repos_url\":\"https://api.github.com/users/PowerShell/repos\",\"events_url\":\"https://api.github.com/users/PowerShell/events{/privacy}\",\"received_events_url\":\"https://api.github.com/users/PowerShell/received_events\",\"type\":\"Organization\",\"site_admin\":false},\"html_url\":\"https://github.com/PowerShell/PowerShell\",\"description\":\"PowerShellforeverysystem!\",\"fork\":false,\"url\":\"https://api.github.com/repos/PowerShell/PowerShell\",\"forks_url\":\"https://api.github.com/repos/PowerShell/PowerShell/forks\",\"keys_url\":\"https://api.github.com/repos/PowerShell/PowerShell/keys{/key_id}\",\"collaborators_url\":\"https://api.github.com/repos/PowerShell/PowerShell/collaborators{/collaborator}\",\"teams_url\":\"https://api.github.com/repos/PowerShell/PowerShell/teams\",\"hooks_url\":\"https://api.github.com/repos/PowerShell/PowerShell/hooks\",\"issue_events_url\":\"https://api.github.com/repos/PowerShell/PowerShell/issues/events{/number}\",\"events_url\":\"https://api.github.com/repos/PowerShell/PowerShell/events\",\"assignees_url\":\"https://api.github.com/repos/PowerShell/PowerShell/assignees{/user}\",\"branches_url\":\"https://api.github.com/repos/PowerShell/PowerShell/branches{/branch}\",\"tags_url\":\"https://api.github.com/repos/PowerShell/PowerShell/tags\",\"blobs_url\":\"https://api.github.com/repos/PowerShell/PowerShell/git/blobs{/sha}\",\"git_tags_url\":\"https://api.github.com/repos/PowerShell/PowerShell/git/tags{/sha}\",\"git_refs_url\":\"https://api.github.com/repos/PowerShell/PowerShell/git/refs{/sha}\",\"trees_url\":\"https://api.github.com/repos/PowerShell/PowerShell/git/trees{/sha}\",\"statuses_url\":\"https://api.github.com/repos/PowerShell/PowerShell/statuses/{sha}\",\"languages_url\":\"https://api.github.com/repos/PowerShell/PowerShell/languages\",\"stargazers_url\":\"https://api.github.com/repos/PowerShell/PowerShell/stargazers\",\"contributors_url\":\"https://api.github.com/repos/PowerShell/PowerShell/contributors\",\"subscribers_url\":\"https://api.github.com/repos/PowerShell/PowerShell/subscribers\",\"subscription_url\":\"https://api.github.com/repos/PowerShell/PowerShell/subscription\",\"commits_url\":\"https://api.github.com/repos/PowerShell/PowerShell/commits{/sha}\",\"git_commits_url\":\"https://api.github.com/repos/PowerShell/PowerShell/git/commits{/sha}\",\"comments_url\":\"https://api.github.com/repos/PowerShell/PowerShell/comments{/number}\",\"issue_comment_url\":\"https://api.github.com/repos/PowerShell/PowerShell/issues/comments{/number}\",\"contents_url\":\"https://api.github.com/repos/PowerShell/PowerShell/contents/{+path}\",\"compare_url\":\"https://api.github.com/repos/PowerShell/PowerShell/compare/{base}...{head}\",\"merges_url\":\"https://api.github.com/repos/PowerShell/PowerShell/merges\",\"archive_url\":\"https://api.github.com/repos/PowerShell/PowerShell/{archive_format}{/ref}\",\"downloads_url\":\"https://api.github.com/repos/PowerShell/PowerShell/downloads\",\"issues_url\":\"https://api.github.com/repos/PowerShell/PowerShell/issues{/number}\",\"pulls_url\":\"https://api.github.com/repos/PowerShell/PowerShell/pulls{/number}\",\"milestones_url\":\"https://api.github.com/repos/PowerShell/PowerShell/milestones{/number}\",\"notifications_url\":\"https://api.github.com/repos/PowerShell/PowerShell/notifications{?since,all,participating}\",\"labels_url\":\"https://api.github.com/repos/PowerShell/PowerShell/labels{/name}\",\"releases_url\":\"https://api.github.com/repos/PowerShell/PowerShell/releases{/id}\",\"deployments_url\":\"https://api.github.com/repos/PowerShell/PowerShell/deployments\",\"created_at\":\"2016-01-13T23:41:35Z\",\"updated_at\":\"2021-09-01T00:53:27Z\",\"pushed_at\":\"2021-08-31T22:58:21Z\",\"git_url\":\"git://github.com/PowerShell/PowerShell.git\",\"ssh_url\":\"git@github.com:PowerShell/PowerShell.git\",\"clone_url\":\"https://github.com/PowerShell/PowerShell.git\",\"svn_url\":\"https://github.com/PowerShell/PowerShell\",\"homepage\":\"https://microsoft.com/PowerShell\",\"size\":80459,\"stargazers_count\":29374,\"watchers_count\":29374,\"language\":\"C#\",\"has_issues\":true,\"has_projects\":true,\"has_downloads\":true,\"has_wiki\":false,\"has_pages\":false,\"forks_count\":4588,\"mirror_url\":null,\"archived\":false,\"disabled\":false,\"open_issues_count\":3083,\"license\":{\"key\":\"mit\",\"name\":\"MITLicense\",\"spdx_id\":\"MIT\",\"url\":\"https://api.github.com/licenses/mit\",\"node_id\":\"MDc6TGljZW5zZTEz\"},\"forks\":4588,\"open_issues\":3083,\"watchers\":29374,\"default_branch\":\"master\",\"permissions\":{\"admin\":false,\"maintain\":false,\"push\":false,\"triage\":false,\"pull\":true},\"score\":1.0},{\"id\":17620347,\"node_id\":\"MDEwOlJlcG9zaXRvcnkxNzYyMDM0Nw==\",\"name\":\"aspnetcore\",\"full_name\":\"dotnet/aspnetcore\",\"private\":false,\"owner\":{\"login\":\"dotnet\",\"id\":9141961,\"node_id\":\"MDEyOk9yZ2FuaXphdGlvbjkxNDE5NjE=\",\"avatar_url\":\"https://avatars.githubusercontent.com/u/9141961?v=4\",\"gravatar_id\":\"\",\"url\":\"https://api.github.com/users/dotnet\",\"html_url\":\"https://github.com/dotnet\",\"followers_url\":\"https://api.github.com/users/dotnet/followers\",\"following_url\":\"https://api.github.com/users/dotnet/following{/other_user}\",\"gists_url\":\"https://api.github.com/users/dotnet/gists{/gist_id}\",\"starred_url\":\"https://api.github.com/users/dotnet/starred{/owner}{/repo}\",\"subscriptions_url\":\"https://api.github.com/users/dotnet/subscriptions\",\"organizations_url\":\"https://api.github.com/users/dotnet/orgs\",\"repos_url\":\"https://api.github.com/users/dotnet/repos\",\"events_url\":\"https://api.github.com/users/dotnet/events{/privacy}\",\"received_events_url\":\"https://api.github.com/users/dotnet/received_events\",\"type\":\"Organization\",\"site_admin\":false},\"html_url\":\"https://github.com/dotnet/aspnetcore\",\"description\":\"ASP.NETCoreisacross-platform.NETframeworkforbuildingmoderncloud-basedwebapplicationsonWindows,Mac,orLinux.\",\"fork\":false,\"url\":\"https://api.github.com/repos/dotnet/aspnetcore\",\"forks_url\":\"https://api.github.com/repos/dotnet/aspnetcore/forks\",\"keys_url\":\"https://api.github.com/repos/dotnet/aspnetcore/keys{/key_id}\",\"collaborators_url\":\"https://api.github.com/repos/dotnet/aspnetcore/collaborators{/collaborator}\",\"teams_url\":\"https://api.github.com/repos/dotnet/aspnetcore/teams\",\"hooks_url\":\"https://api.github.com/repos/dotnet/aspnetcore/hooks\",\"issue_events_url\":\"https://api.github.com/repos/dotnet/aspnetcore/issues/events{/number}\",\"events_url\":\"https://api.github.com/repos/dotnet/aspnetcore/events\",\"assignees_url\":\"https://api.github.com/repos/dotnet/aspnetcore/assignees{/user}\",\"branches_url\":\"https://api.github.com/repos/dotnet/aspnetcore/branches{/branch}\",\"tags_url\":\"https://api.github.com/repos/dotnet/aspnetcore/tags\",\"blobs_url\":\"https://api.github.com/repos/dotnet/aspnetcore/git/blobs{/sha}\",\"git_tags_url\":\"https://api.github.com/repos/dotnet/aspnetcore/git/tags{/sha}\",\"git_refs_url\":\"https://api.github.com/repos/dotnet/aspnetcore/git/refs{/sha}\",\"trees_url\":\"https://api.github.com/repos/dotnet/aspnetcore/git/trees{/sha}\",\"statuses_url\":\"https://api.github.com/repos/dotnet/aspnetcore/statuses/{sha}\",\"languages_url\":\"https://api.github.com/repos/dotnet/aspnetcore/languages\",\"stargazers_url\":\"https://api.github.com/repos/dotnet/aspnetcore/stargazers\",\"contributors_url\":\"https://api.github.com/repos/dotnet/aspnetcore/contributors\",\"subscribers_url\":\"https://api.github.com/repos/dotnet/aspnetcore/subscribers\",\"subscription_url\":\"https://api.github.com/repos/dotnet/aspnetcore/subscription\",\"commits_url\":\"https://api.github.com/repos/dotnet/aspnetcore/commits{/sha}\",\"git_commits_url\":\"https://api.github.com/repos/dotnet/aspnetcore/git/commits{/sha}\",\"comments_url\":\"https://api.github.com/repos/dotnet/aspnetcore/comments{/number}\",\"issue_comment_url\":\"https://api.github.com/repos/dotnet/aspnetcore/issues/comments{/number}\",\"contents_url\":\"https://api.github.com/repos/dotnet/aspnetcore/contents/{+path}\",\"compare_url\":\"https://api.github.com/repos/dotnet/aspnetcore/compare/{base}...{head}\",\"merges_url\":\"https://api.github.com/repos/dotnet/aspnetcore/merges\",\"archive_url\":\"https://api.github.com/repos/dotnet/aspnetcore/{archive_format}{/ref}\",\"downloads_url\":\"https://api.github.com/repos/dotnet/aspnetcore/downloads\",\"issues_url\":\"https://api.github.com/repos/dotnet/aspnetcore/issues{/number}\",\"pulls_url\":\"https://api.github.com/repos/dotnet/aspnetcore/pulls{/number}\",\"milestones_url\":\"https://api.github.com/repos/dotnet/aspnetcore/milestones{/number}\",\"notifications_url\":\"https://api.github.com/repos/dotnet/aspnetcore/notifications{?since,all,participating}\",\"labels_url\":\"https://api.github.com/repos/dotnet/aspnetcore/labels{/name}\",\"releases_url\":\"https://api.github.com/repos/dotnet/aspnetcore/releases{/id}\",\"deployments_url\":\"https://api.github.com/repos/dotnet/aspnetcore/deployments\",\"created_at\":\"2014-03-11T06:09:42Z\",\"updated_at\":\"2021-09-01T01:37:06Z\",\"pushed_at\":\"2021-09-01T02:17:50Z\",\"git_url\":\"git://github.com/dotnet/aspnetcore.git\",\"ssh_url\":\"git@github.com:dotnet/aspnetcore.git\",\"clone_url\":\"https://github.com/dotnet/aspnetcore.git\",\"svn_url\":\"https://github.com/dotnet/aspnetcore\",\"homepage\":\"https://asp.net\",\"size\":267563,\"stargazers_count\":25708,\"watchers_count\":25708,\"language\":\"C#\",\"has_issues\":true,\"has_projects\":true,\"has_downloads\":true,\"has_wiki\":true,\"has_pages\":false,\"forks_count\":6778,\"mirror_url\":null,\"archived\":false,\"disabled\":false,\"open_issues_count\":2729,\"license\":{\"key\":\"mit\",\"name\":\"MITLicense\",\"spdx_id\":\"MIT\",\"url\":\"https://api.github.com/licenses/mit\",\"node_id\":\"MDc6TGljZW5zZTEz\"},\"forks\":6778,\"open_issues\":2729,\"watchers\":25708,\"default_branch\":\"main\",\"permissions\":{\"admin\":false,\"maintain\":false,\"push\":false,\"triage\":false,\"pull\":true},\"score\":1.0},{\"id\":11620669,\"node_id\":\"MDEwOlJlcG9zaXRvcnkxMTYyMDY2OQ==\",\"name\":\"CodeHub\",\"full_name\":\"CodeHubApp/CodeHub\",\"private\":false,\"owner\":{\"login\":\"CodeHubApp\",\"id\":31394933,\"node_id\":\"MDEyOk9yZ2FuaXphdGlvbjMxMzk0OTMz\",\"avatar_url\":\"https://avatars.githubusercontent.com/u/31394933?v=4\",\"gravatar_id\":\"\",\"url\":\"https://api.github.com/users/CodeHubApp\",\"html_url\":\"https://github.com/CodeHubApp\",\"followers_url\":\"https://api.github.com/users/CodeHubApp/followers\",\"following_url\":\"https://api.github.com/users/CodeHubApp/following{/other_user}\",\"gists_url\":\"https://api.github.com/users/CodeHubApp/gists{/gist_id}\",\"starred_url\":\"https://api.github.com/users/CodeHubApp/starred{/owner}{/repo}\",\"subscriptions_url\":\"https://api.github.com/users/CodeHubApp/subscriptions\",\"organizations_url\":\"https://api.github.com/users/CodeHubApp/orgs\",\"repos_url\":\"https://api.github.com/users/CodeHubApp/repos\",\"events_url\":\"https://api.github.com/users/CodeHubApp/events{/privacy}\",\"received_events_url\":\"https://api.github.com/users/CodeHubApp/received_events\",\"type\":\"Organization\",\"site_admin\":false},\"html_url\":\"https://github.com/CodeHubApp/CodeHub\",\"description\":\"CodeHubisaniOSapplicationwrittenusingXamarin\",\"fork\":false,\"url\":\"https://api.github.com/repos/CodeHubApp/CodeHub\",\"forks_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/forks\",\"keys_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/keys{/key_id}\",\"collaborators_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/collaborators{/collaborator}\",\"teams_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/teams\",\"hooks_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/hooks\",\"issue_events_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/issues/events{/number}\",\"events_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/events\",\"assignees_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/assignees{/user}\",\"branches_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/branches{/branch}\",\"tags_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/tags\",\"blobs_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/git/blobs{/sha}\",\"git_tags_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/git/tags{/sha}\",\"git_refs_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/git/refs{/sha}\",\"trees_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/git/trees{/sha}\",\"statuses_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/statuses/{sha}\",\"languages_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/languages\",\"stargazers_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/stargazers\",\"contributors_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/contributors\",\"subscribers_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/subscribers\",\"subscription_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/subscription\",\"commits_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/commits{/sha}\",\"git_commits_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/git/commits{/sha}\",\"comments_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/comments{/number}\",\"issue_comment_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/issues/comments{/number}\",\"contents_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/contents/{+path}\",\"compare_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/compare/{base}...{head}\",\"merges_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/merges\",\"archive_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/{archive_format}{/ref}\",\"downloads_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/downloads\",\"issues_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/issues{/number}\",\"pulls_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/pulls{/number}\",\"milestones_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/milestones{/number}\",\"notifications_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/notifications{?since,all,participating}\",\"labels_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/labels{/name}\",\"releases_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/releases{/id}\",\"deployments_url\":\"https://api.github.com/repos/CodeHubApp/CodeHub/deployments\",\"created_at\":\"2013-07-23T22:19:57Z\",\"updated_at\":\"2021-08-31T16:22:06Z\",\"pushed_at\":\"2021-04-05T02:45:57Z\",\"git_url\":\"git://github.com/CodeHubApp/CodeHub.git\",\"ssh_url\":\"git@github.com:CodeHubApp/CodeHub.git\",\"clone_url\":\"https://github.com/CodeHubApp/CodeHub.git\",\"svn_url\":\"https://github.com/CodeHubApp/CodeHub\",\"homepage\":\"http://www.codehub-app.com\",\"size\":75626,\"stargazers_count\":23344,\"watchers_count\":23344,\"language\":\"C#\",\"has_issues\":true,\"has_projects\":false,\"has_downloads\":true,\"has_wiki\":true,\"has_pages\":true,\"forks_count\":642,\"mirror_url\":null,\"archived\":false,\"disabled\":false,\"open_issues_count\":235,\"license\":null,\"forks\":642,\"open_issues\":235,\"watchers\":23344,\"default_branch\":\"master\",\"permissions\":{\"admin\":false,\"maintain\":false,\"push\":false,\"triage\":false,\"pull\":true},\"score\":1.0}]}");
        }
    }
}
