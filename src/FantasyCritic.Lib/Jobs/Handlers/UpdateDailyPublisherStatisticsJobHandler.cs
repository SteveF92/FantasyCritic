using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class UpdateDailyPublisherStatisticsJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.UpdateDailyPublisherStatistics;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.AtTenPmEastern;

    private readonly InterLeagueService _interLeagueService;
    private readonly RoyaleService _royaleService;
    private readonly IDailyStatsRepo _dailyStatsRepo;
    private readonly IClock _clock;
    private readonly ILogger<UpdateDailyPublisherStatisticsJobHandler> _logger;

    public UpdateDailyPublisherStatisticsJobHandler(InterLeagueService interLeagueService, RoyaleService royaleService,
        IDailyStatsRepo dailyStatsRepo, IClock clock, ILogger<UpdateDailyPublisherStatisticsJobHandler> logger)
    {
        _interLeagueService = interLeagueService;
        _royaleService = royaleService;
        _dailyStatsRepo = dailyStatsRepo;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating daily statistics.");
        SystemWideValues systemWideValues = await _interLeagueService.GetSystemWideValues();
        var today = _clock.GetToday();

        var supportedYears = await _interLeagueService.GetSupportedYears();
        var activeYears = supportedYears.Where(x => !x.Finished && x.OpenForPlay).ToList();
        var supportedQuarters = await _royaleService.GetYearQuarters();

        await _dailyStatsRepo.UpdateDailyStats(activeYears, supportedQuarters, today, systemWideValues);

        return Result.Success();
    }
}
