using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Patreon;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RefreshPatreonInfoJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.RefreshPatreonInfo;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.Hourly;

    private readonly IFantasyCriticUserStore _userStore;
    private readonly PatreonService _patreonService;
    private readonly ILogger<RefreshPatreonInfoJobHandler> _logger;

    public RefreshPatreonInfoJobHandler(IFantasyCriticUserStore userStore, PatreonService patreonService,
        ILogger<RefreshPatreonInfoJobHandler> logger)
    {
        _userStore = userStore;
        _patreonService = patreonService;
        _logger = logger;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var patreonUsers = await _userStore.GetUsersWithExternalLogin("Patreon");
        var patronInfo = await _patreonService.GetPatronInfo(patreonUsers);
        await _userStore.UpdatePatronInfo(patronInfo);

        return Result.Success();
    }
}
