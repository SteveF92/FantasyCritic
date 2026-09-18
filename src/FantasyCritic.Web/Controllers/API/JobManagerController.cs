using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs;
using FantasyCritic.Web.Models.Requests.JobManager;
using FantasyCritic.Web.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Web.Controllers.API;

[Route("api/[controller]/[action]")]
[Authorize("JobManager")]
public class JobManagerController : FantasyCriticController
{
    private readonly IClock _clock;
    private readonly ILogger _logger;
    private readonly IJobRepo _jobRepo;

    public JobManagerController(IClock clock, ILogger<JobManagerController> logger, FantasyCriticUserManager userManager, IJobRepo jobRepo)
        : base(userManager)
    {
        _clock = clock;
        _logger = logger;
        _jobRepo = jobRepo;
    }

    [HttpGet]
    [ProducesResponseType<List<FantasyCriticJobViewModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<FantasyCriticJobViewModel>>> GetJobs([FromQuery] int page, [FromQuery] int count, [FromQuery] string? jobType)
    {
        if (page < 1 || count < 1)
        {
            return BadRequest("page and count must both be at least 1.");
        }

        FantasyCriticJobType? parsedJobType = null;
        if (!string.IsNullOrWhiteSpace(jobType))
        {
            parsedJobType = FantasyCriticJobType.TryFromValue(jobType);
            if (parsedJobType is null)
            {
                return BadRequest($"Unknown job type '{jobType}'.");
            }
        }

        var jobs = await _jobRepo.GetJobs(page, count, parsedJobType);
        return jobs.Select(x => new FantasyCriticJobViewModel(x)).ToList();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelJob([FromBody] CancelJobRequest request)
    {
        var job = await _jobRepo.GetJob(request.JobID);
        if (job is null)
        {
            return NotFound();
        }

        var isCancellable = job.Status.Equals(FantasyCriticJobStatus.Queued) || job.Status.Equals(FantasyCriticJobStatus.Running);
        if (!isCancellable)
        {
            return BadRequest($"Job is {job.Status.Value}, which cannot be cancelled.");
        }

        var currentUser = await GetCurrentUserOrThrow();
        await _jobRepo.RequestCancellation(job, currentUser, _clock.GetCurrentInstant());

        _logger.LogWarning("{User} requested cancellation of job {Job}.", currentUser.UserName, job);

        return Ok();
    }
}
