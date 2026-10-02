using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Patreon;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RefreshPatreonInfoJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.RefreshPatreonInfo;
    public static FantasyCriticJobPriority Priority => FantasyCriticJobPriority.Independent;
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
        cancellationToken.ThrowIfCancellationRequested();
        await _userStore.UpdatePatronInfo(patronInfo);

        //UpdatePatronInfo replaces every programmatic role and donor name, so these counts are the new totals.
        var plusCount = patronInfo.Count(x => x.IsPlusUser);
        var donorCount = patronInfo.Count(x => x.DonorName is not null);
        _logger.LogInformation("Updated patron info: {LinkedUserCount} linked users, {PlusCount} Plus, {DonorCount} donors.", patreonUsers.Count, plusCount, donorCount);
        await context.UpdateDetailedStatus($"Updated patron info: {patreonUsers.Count} linked users, {plusCount} Plus, {donorCount} donors.");
        return Result.Success();
    }
}
