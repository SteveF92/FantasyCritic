using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs.Utilities;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class EndOfYearRolloverJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.EndOfYearRollover;
    public static FantasyCriticJobPriority Priority => FantasyCriticJobPriority.TimeCritical;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.Cron("0 0 1 1 *");

    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly DiscordPushService _discordPushService;
    private readonly IClock _clock;
    private readonly CriticScoreRefresher _criticScoreRefresher;
    private readonly CacheRefresher _cacheRefresher;
    private readonly FantasyPointsUpdater _fantasyPointsUpdater;
    private readonly ILogger<EndOfYearRolloverJobHandler> _logger;

    public EndOfYearRolloverJobHandler(IFantasyCriticRepo fantasyCriticRepo, DiscordPushService discordPushService, IClock clock,
        CriticScoreRefresher criticScoreRefresher, CacheRefresher cacheRefresher, FantasyPointsUpdater fantasyPointsUpdater, ILogger<EndOfYearRolloverJobHandler> logger)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _discordPushService = discordPushService;
        _clock = clock;
        _criticScoreRefresher = criticScoreRefresher;
        _cacheRefresher = cacheRefresher;
        _fantasyPointsUpdater = fantasyPointsUpdater;
        _logger = logger;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var supportedYears = await _fantasyCriticRepo.GetSupportedYears();
        var nycNow = _clock.GetCurrentInstant().InZone(TimeExtensions.EasternTimeZone);

        bool anyYearFinished = false;
        foreach (var supportedYear in supportedYears)
        {
            if (supportedYear.Finished)
            {
                continue;
            }

            var endDate = new LocalDate(supportedYear.Year, 12, 31);
            if (nycNow.Date > endDate)
            {
                _logger.LogInformation("Beginning end of year process for {Year}: it ended on {EndDate} and the Eastern time is {EasternTime}.",
                    supportedYear.Year, endDate.ToISOString(), nycNow.ToString());

                await _criticScoreRefresher.RefreshCriticInfo(context, cancellationToken);
                await _cacheRefresher.RefreshCaches(context, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                await _fantasyCriticRepo.FinishYear(supportedYear);
                _logger.LogInformation("Finished {Year}.", supportedYear.Year);
                await context.AppendDetailedStatus($"Finished {supportedYear.Year}.");

                //Past this point, the next run skips this year because it's finished. So the rest runs to the end regardless:
                //stopping here would leave the final standings unsent.
                //This writes the finished year's winners, which the next refresh would also do. It doesn't update that year's points,
                //since only unfinished years get those, so the year ends on the points from the last refresh before midnight.
                await _fantasyPointsUpdater.UpdateFantasyPoints(context, CancellationToken.None);

                var leagueYears = await _fantasyCriticRepo.GetLeagueYears(supportedYear.Year);
                var standingsResult = await _discordPushService.SendFinalYearStandings(leagueYears, nycNow.Date);
                FinalYearStandingsJobUtilities.LogResult(_logger, supportedYear.Year, standingsResult);
                await context.AppendDetailedStatus(FinalYearStandingsJobUtilities.Describe(supportedYear.Year, standingsResult));
                anyYearFinished = true;
            }
        }

        if (!anyYearFinished)
        {
            _logger.LogDebug("No years to finish: the Eastern date is {EasternDate}.", nycNow.Date.ToISOString());
            await context.AppendDetailedStatus("No years to finish.");
        }

        return Result.Success();
    }
}
