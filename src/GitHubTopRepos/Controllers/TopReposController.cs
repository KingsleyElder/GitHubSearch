using GitHubTopRepos.Data.Entities;
using GitHubTopRepos.Models;
using GitHubTopRepos.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitHubTopRepos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Produces("application/json")]
    public class TopReposController : ControllerBase
    {
        private readonly IGitHubRepository _gitHubRepository;
        private readonly IRequestTrackingRepository _requestTrackingRepository;
        private readonly ILogger<TopReposController> _logger;
        private const int TopRepoCount = 5;
        private const int RequestLogCount = 20;

        public TopReposController(IGitHubRepository gitHubRepository,
            IRequestTrackingRepository requestTrackingRepository,
            ILogger<TopReposController> logger)
        {
            _gitHubRepository = gitHubRepository;
            _requestTrackingRepository = requestTrackingRepository;
            _logger = logger;
        }

        /// <summary>
        /// Returns the top repositories based on Star Gazers count
        /// </summary>
        /// <returns></returns>
        //[Route("api/TopRepos/{language:string}")]
        [HttpGet]
        public async Task<IActionResult> Get(GitHubInput language)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                var repoData = await _gitHubRepository.GetTopRepos(TopRepoCount, language);
                if (repoData.TotalCount == 0)
                {
                    return NotFound();
                }
                var returnSet = new List<TopRepositoryData>();

                foreach (var repo in repoData.Items)
                {
                    returnSet.Add(new TopRepositoryData
                    {
                        Name = repo.Name,
                        FullName = repo.FullName,
                        AvatarUrl = repo.Owner.AvatarUrl,
                        Description = repo.Description,
                        Url = repo.Owner.Url,
                        StargazersCount = repo.StargazersCount,
                        Language = repo.Language
                    });
                }

                _logger.LogInformation("Insert request history here");

                return Ok(returnSet);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "TopReposController Get() threw an exception");
                return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
            }
        }

        [Route("api/TopRepos/Languages")]
        [HttpGet]
        public async Task<IActionResult> GetGitHubLanguages()
        {
            try
            {
                var repoData = await _requestTrackingRepository.GetLanguages();
                if (repoData == null)
                {
                    return NotFound();
                }
                var returnSet = new List<GitHubInput>();

                return Ok(returnSet);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "TopReposController GetGitHubLanguages() threw an exception");
                return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
            }
        }

        [Route("api/TopRepos/RecentRequests")]
        [HttpGet]
        public async Task<IActionResult> GetRecentRequests()
        {
            try
            {
                var repoData = await _requestTrackingRepository.GetRecentRequests(RequestLogCount);
                if (repoData == null)
                {
                    return NotFound();
                }
                var returnSet = new List<GitHubInput>();

                return Ok(returnSet);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "TopReposController GetGitHubLanguages() threw an exception");
                return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
            }
        }
    }
}
