using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs.Utilities;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class EndOfYearRolloverJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.EndOfYearRollover;
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

        foreach (var supportedYear in supportedYears)
        {
            if (supportedYear.Finished)
            {
                continue;
            }

            var endDate = new LocalDate(supportedYear.Year, 12, 31);
            if (nycNow.Date > endDate)
            {
                _logger.LogInformation($"Beginning end of year process for {supportedYear} because date/time is: {nycNow}");

                await _criticScoreRefresher.RefreshCriticInfo(cancellationToken);
                await _cacheRefresher.RefreshCaches(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                await _fantasyCriticRepo.FinishYear(supportedYear);

                //Past this point, the next run skips this year because it's finished. So the rest runs to the end regardless:
                //stopping here would leave fantasy points un-finalized and the final standings never sent.
                await _fantasyPointsUpdater.UpdateFantasyPoints(CancellationToken.None);

                var leagueYears = await _fantasyCriticRepo.GetLeagueYears(supportedYear.Year);
                await _discordPushService.SendFinalYearStandings(leagueYears, nycNow.Date);
            }
        }

        return Result.Success();
    }
}
