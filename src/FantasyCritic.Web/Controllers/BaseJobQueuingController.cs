using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs;
using FantasyCritic.Web.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FantasyCritic.Web.Controllers;

public abstract class BaseJobQueuingController : FantasyCriticController
{
    protected readonly IJobRepo _jobRepo;
    protected readonly IClock _clock;

    protected BaseJobQueuingController(FantasyCriticUserManager userManager, IJobRepo jobRepo, IClock clock)
        : base(userManager)
    {
        _jobRepo = jobRepo;
        _clock = clock;
    }

    //For the admin console's job buttons. Checks specific to one job type belong in the action, before this.
    protected async Task<ActionResult<FantasyCriticJobViewModel>> EnqueueJob(FantasyCriticJobType jobType)
    {
        var currentUser = await GetCurrentUserOrThrow();
        var result = await _jobRepo.EnqueueJob(jobType, currentUser, _clock.GetCurrentInstant());
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return new FantasyCriticJobViewModel(result.Value);
    }
}
