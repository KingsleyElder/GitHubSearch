using GitHubTopRepos.Controllers;
using GitHubTopRepos.Data.Entities;
using GitHubTopRepos.Models;
using GitHubTopRepos.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace GitHubTopRepos.Test.Controllers
{
    public class TopReposControllerTests
    {
        private readonly Mock<IGitHubRepository> _gitHubRepository;
        private readonly Mock<IRequestTrackingRepository> _requestTrackingRepository;
        private readonly Mock<ILogger<TopReposController>> _logger;
        private readonly TopReposController _controller;

        public TopReposControllerTests()
        {
            _gitHubRepository = new Mock<IGitHubRepository>();
            _requestTrackingRepository = new Mock<IRequestTrackingRepository>();
            _logger = new Mock<ILogger<TopReposController>>();
            _controller = new TopReposController(_gitHubRepository.Object, _requestTrackingRepository.Object, _logger.Object)
            {
                ControllerContext = new ControllerContext()
            };
        }

        [Fact]
        public async Task Get_NoDataRetrieved_ReturnsNotFound()
        {
            // Arrange
            var selectedLanguage = new GitHubInput() { Language = "C#" };
            _gitHubRepository.Setup(r => r.GetTopRepos(It.IsAny<int>(), It.IsAny<GitHubInput>())).Returns(Task.FromResult(new GitHubRepoSearchResult
            {
                IncompleteResults = true,
                TotalCount = 0,
                Items = new GitHubRepo[0]
            }
            ));

            // Act
            var result = await _controller.Get(selectedLanguage) as NotFoundResult;

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Get_DataRetrieved_RecordsReturned()
        {
            // Arrange
            var selectedLanguage = new GitHubInput() { Language = "C#" };
            _gitHubRepository.Setup(r => r.GetTopRepos(It.IsAny<int>(), It.IsAny<GitHubInput>())).Returns(Task.FromResult(new GitHubRepoSearchResult
            {
                IncompleteResults = true,
                TotalCount = 3027156,
                Items = BuildResultSet()
            }
            ));

            // Act
            var result = await _controller.Get(selectedLanguage) as ActionResult;
            var okResult = result as OkObjectResult;
            var records = okResult.Value as List<TopRepositoryData>;

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(records);
            Assert.Equal("PowerToys", records[0].Name);
            _logger.Verify(lg => lg.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => "Insert request history here".Equals(v.ToString())),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public async Task Get_CastingException_ReturnsServiceUnavailable()
        {
            // Arrange
            var selectedLanguage = new GitHubInput() { Language = "C#" };
            var expectedLog = "TopReposController Get() threw an exception";
            var exceptionMessage = "Object reference not set to an instance of an object.";
            _gitHubRepository.Setup(r => r.GetTopRepos(It.IsAny<int>(), It.IsAny<GitHubInput>())).Returns(Task.FromResult(new GitHubRepoSearchResult
            {
                IncompleteResults = true,
                TotalCount = 1,
                Items = new GitHubRepo[] { new GitHubRepo
                {
                    Name = "NoOwner",
                    FullName = null,
                    Url = "https://api.github.com/users/microsoft",
                    StargazersCount = 1
                }}
            }
            ));

            // Act
            var result = await _controller.Get(selectedLanguage) as StatusCodeResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, result.StatusCode);
            _logger.Verify(lg => lg.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Equals(expectedLog)),
                It.Is<Exception>(exception => exception.Message == exceptionMessage),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        private GitHubRepo[] BuildResultSet()
        {
            return new GitHubRepo[] {
                new GitHubRepo
                {
                    Id = 184456251,
                    Name = "PowerToys",
                    FullName = "microsoft / PowerToys",
                    Description = "Windows system utilities to maximize productivity",
                    Language = "C#",
                    Owner = new GitHubOwner
                    {
                        AvatarUrl = "https://avatars.githubusercontent.com/u/6154722?v=4",
                    },
                    Url = "https://api.github.com/users/microsoft",
                    StargazersCount = 59137
                }, new GitHubRepo
                {
                    Id = 7600409,
                    Name = "shadowsocks-windows",
                    FullName = "shadowsocks/shadowsocks-windows",
                    Description = "A C# port of shadowsocks",
                    Language = "C#",
                    Owner = new GitHubOwner
                    {
                        AvatarUrl = "ttps://avatars.githubusercontent.com/u/3006190?v=4",
                    },
                    Url = "https://api.github.com/repos/shadowsocks/shadowsocks-windows",
                    StargazersCount = 52858
                }, new GitHubRepo
                {
                    Id = 49609581,
                    Name = "PowerShell",
                    FullName = "PowerShell/PowerShell",
                    Description = "PowerShell for every system!",
                    Language = "C#",
                    Owner = new GitHubOwner
                    {
                        AvatarUrl = "https://avatars.githubusercontent.com/u/11524380?v=4",
                    },
                    Url = "https://api.github.com/repos/PowerShell/PowerShell",
                    StargazersCount = 29368
                }, new GitHubRepo
                {
                    Id = 17620347,
                    Name = "aspnetcore",
                    FullName = "dotnet/aspnetcore",
                    Description = "ASP.NET Core is a cross-platform .NET framework for building modern cloud-based web applications on Windows, Mac, or Linux.",
                    Language = "C#",
                    Owner = new GitHubOwner
                    {
                        AvatarUrl = "https://avatars.githubusercontent.com/u/9141961?v=4",
                    },
                    Url = "https://api.github.com/repos/dotnet/aspnetcore",
                    StargazersCount = 25697
                }, new GitHubRepo
                {
                    Id = 11620669,
                    Name = "CodeHub",
                    FullName = "CodeHubApp/CodeHub",
                    Description = "CodeHub is an iOS application written using Xamarin",
                    Language = "C#",
                    Owner = new GitHubOwner
                    {
                        AvatarUrl = "https://avatars.githubusercontent.com/u/31394933?v=4",
                    },
                    Url = "https://api.github.com/repos/CodeHubApp/CodeHub",
                    StargazersCount = 23345
                }
            };
        }
    }
}
