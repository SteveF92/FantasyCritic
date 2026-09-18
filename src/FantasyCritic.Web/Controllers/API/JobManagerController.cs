using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Web.Models.Responses;
using Microsoft.AspNetCore.Authorization;
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


    public JobManagerController(IClock clock, ILogger<AdminController> logger, FantasyCriticUserManager userManager, IJobRepo jobRepo)
        : base(userManager)
    {
        _clock = clock;
        _logger = logger;
        _jobRepo = jobRepo;
    }

    [HttpGet]
    public Action<List<FantasyCriticJobViewModel>> GetJobs(int page, int count)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    public Task<ActionResult> CancelJob()
    {
        throw new NotImplementedException();
    }
}
