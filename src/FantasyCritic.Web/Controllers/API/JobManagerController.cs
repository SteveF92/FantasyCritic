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
    public async Task<ActionResult<List<FantasyCriticJobViewModel>>> GetJobs([FromQuery] GetJobsRequest request)
    {
        if (request.Page < 1 || request.Count < 1)
        {
            return BadRequest("page and count must both be at least 1.");
        }

        var filter = request.ToDomain();
        if (filter.IsFailure)
        {
            return BadRequest(filter.Error);
        }

        var jobs = await _jobRepo.GetJobs(request.Page, request.Count, filter.Value);
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
