using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class PushGameReleaseMessagesJobHandler : IFantasyCriticCronJobHandler
{
    private readonly InterLeagueService _interLeagueService;
    private readonly DiscordPushService _discordPushService;
    private readonly IClock _clock;

    public PushGameReleaseMessagesJobHandler(InterLeagueService interLeagueService, DiscordPushService discordPushService, IClock clock)
    {
        _interLeagueService = interLeagueService;
        _discordPushService = discordPushService;
        _clock = clock;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.PushGameReleaseMessages;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.Cron("0 0 * * *");

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        //Runs at Eastern midnight, so "today" is the day that just started.
        var today = _clock.GetToday();
        var allMasterGames = await _interLeagueService.GetMasterGameYears(today.Year);
        var masterGamesReleasingToday = allMasterGames.Where(x => x.MasterGame.ReleaseDate.HasValue && x.MasterGame.ReleaseDate.Value == today).ToList();
        if (!masterGamesReleasingToday.Any())
        {
            await context.UpdateDetailedStatus($"No games release on {today.ToISOString()}.");
            return Result.Success();
        }

        await _discordPushService.SendGameReleaseUpdates(masterGamesReleasingToday);
        await context.UpdateDetailedStatus($"Sent release messages for {masterGamesReleasingToday.Count} games.");
        return Result.Success();
    }
}
