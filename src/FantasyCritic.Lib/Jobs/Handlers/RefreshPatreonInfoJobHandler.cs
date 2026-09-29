using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Patreon;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RefreshPatreonInfoJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.RefreshPatreonInfo;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.Hourly;

    private readonly FantasyCriticUserManager _userManager;
    private readonly PatreonService _patreonService;
    private readonly ILogger<RefreshPatreonInfoJobHandler> _logger;

    public RefreshPatreonInfoJobHandler(FantasyCriticUserManager userManager, PatreonService patreonService,
        ILogger<RefreshPatreonInfoJobHandler> logger)
    {
        _userManager = userManager;
        _patreonService = patreonService;
        _logger = logger;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var patreonUsers = await _userManager.GetAllPatreonUsers();
        var patronInfo = await _patreonService.GetPatronInfo(patreonUsers);
        await _userManager.UpdatePatronInfo(patronInfo);

        return Result.Success();
    }
}
