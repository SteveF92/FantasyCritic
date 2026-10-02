using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs.Utilities;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

//The rollover sends these itself. This is for sending them again when that send didn't reach Discord.
internal class SendFinalYearStandingsJobHandler : IFantasyCriticJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.SendFinalYearStandings;

    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly DiscordPushService _discordPushService;
    private readonly IClock _clock;
    private readonly ILogger<SendFinalYearStandingsJobHandler> _logger;

    public SendFinalYearStandingsJobHandler(IFantasyCriticRepo fantasyCriticRepo, DiscordPushService discordPushService, IClock clock,
        ILogger<SendFinalYearStandingsJobHandler> logger)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _discordPushService = discordPushService;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var supportedYears = await _fantasyCriticRepo.GetSupportedYears();
        var mostRecentFinishedYear = supportedYears.Where(x => x.Finished).MaxBy(x => x.Year);
        if (mostRecentFinishedYear is null)
        {
            return Result.Failure("No year is finished.");
        }

        IReadOnlyList<LeagueYear> leagueYears = await _fantasyCriticRepo.GetLeagueYears(mostRecentFinishedYear.Year);
        var easternToday = _clock.GetCurrentInstant().InZone(TimeExtensions.EasternTimeZone).Date;
        cancellationToken.ThrowIfCancellationRequested();
        var result = await _discordPushService.SendFinalYearStandings(leagueYears, easternToday);

        FinalYearStandingsJobUtilities.LogResult(_logger, mostRecentFinishedYear.Year, result);
        await context.UpdateDetailedStatus(FinalYearStandingsJobUtilities.Describe(mostRecentFinishedYear.Year, result));
        return Result.Success();
    }
}
