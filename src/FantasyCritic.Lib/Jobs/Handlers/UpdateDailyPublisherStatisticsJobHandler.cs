using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class UpdateDailyPublisherStatisticsJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.UpdateDailyPublisherStatistics;
    public static FantasyCriticJobPriority Priority => FantasyCriticJobPriority.StrictlyDependant;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.AtTenPmEastern;

    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IRoyaleRepo _royaleRepo;
    private readonly IDailyStatsRepo _dailyStatsRepo;
    private readonly IClock _clock;
    private readonly ILogger<UpdateDailyPublisherStatisticsJobHandler> _logger;

    public UpdateDailyPublisherStatisticsJobHandler(IFantasyCriticRepo fantasyCriticRepo, IRoyaleRepo royaleRepo,
        IDailyStatsRepo dailyStatsRepo, IClock clock, ILogger<UpdateDailyPublisherStatisticsJobHandler> logger)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _royaleRepo = royaleRepo;
        _dailyStatsRepo = dailyStatsRepo;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating daily statistics.");
        SystemWideValues systemWideValues = await _fantasyCriticRepo.GetSystemWideValues();
        var today = _clock.GetToday();

        var supportedYears = await _fantasyCriticRepo.GetSupportedYears();
        var activeYears = supportedYears.Where(x => !x.Finished && x.OpenForPlay).ToList();
        var supportedQuarters = await _royaleRepo.GetYearQuarters();

        cancellationToken.ThrowIfCancellationRequested();
        await _dailyStatsRepo.UpdateDailyStats(activeYears, supportedQuarters, today, systemWideValues);

        var years = activeYears.Select(x => x.Year).ToList();
        _logger.LogInformation("Updated daily statistics for {Years} on {Date}.", years, today.ToISOString());
        await context.UpdateDetailedStatus($"Updated daily stats for {string.Join(", ", years)} on {today.ToISOString()}.");
        return Result.Success();
    }
}
