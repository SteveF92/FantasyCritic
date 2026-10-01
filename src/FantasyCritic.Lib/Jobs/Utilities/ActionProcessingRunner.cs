using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Utilities;

internal class ActionProcessingRunner
{
    private readonly InterLeagueService _interLeagueService;
    private readonly AdminService _adminService;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly DiscordPushService _discordPushService;
    private readonly TopBidsAndDropsUpdater _topBidsAndDropsUpdater;
    private readonly IClock _clock;

    public ActionProcessingRunner(InterLeagueService interLeagueService, AdminService adminService, IFantasyCriticRepo fantasyCriticRepo,
        DiscordPushService discordPushService, TopBidsAndDropsUpdater topBidsAndDropsUpdater, IClock clock)
    {
        _interLeagueService = interLeagueService;
        _adminService = adminService;
        _fantasyCriticRepo = fantasyCriticRepo;
        _discordPushService = discordPushService;
        _topBidsAndDropsUpdater = topBidsAndDropsUpdater;
        _clock = clock;
    }

    //Callers check AdminService.CanProcessActions first.
    public Task ProcessActions(FantasyCriticJobContext context)
    {
        return Task.CompletedTask;
    }

    private Task ProcessActionsForYear(SystemWideValues systemWideValues, int year)
    {
        return Task.CompletedTask;
    }
}
