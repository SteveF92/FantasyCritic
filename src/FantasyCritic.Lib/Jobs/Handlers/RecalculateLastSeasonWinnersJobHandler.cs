using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RecalculateLastSeasonWinnersJobHandler : IFantasyCriticJobHandler
{
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly FantasyCriticService _fantasyCriticService;
    private readonly ILogger<RecalculateLastSeasonWinnersJobHandler> _logger;

    public RecalculateLastSeasonWinnersJobHandler(IFantasyCriticRepo fantasyCriticRepo, FantasyCriticService fantasyCriticService,
        ILogger<RecalculateLastSeasonWinnersJobHandler> logger)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _fantasyCriticService = fantasyCriticService;
        _logger = logger;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.RecalculateLastSeasonWinners;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var supportedYears = await _fantasyCriticRepo.GetSupportedYears();
        var mostRecentFinishedYear = supportedYears.Where(x => x.Finished).OrderByDescending(x => x.Year).First();
        IReadOnlyList<LeagueYear> leagueYears = await _fantasyCriticRepo.GetLeagueYears(mostRecentFinishedYear.Year);
        var calculatedStats = _fantasyCriticService.GetCalculatedStatsForYear(mostRecentFinishedYear.Year, leagueYears, true);
        cancellationToken.ThrowIfCancellationRequested();
        await _fantasyCriticRepo.UpdateLeagueWinners(calculatedStats.WinningUsers, true);

        _logger.LogInformation("Recalculated winners for {Year}: {WinnerCount} winners across {LeagueCount} leagues.",
            mostRecentFinishedYear.Year, calculatedStats.WinningUsers.Count, leagueYears.Count);
        await context.UpdateDetailedStatus($"Recalculated winners for {mostRecentFinishedYear.Year}: {calculatedStats.WinningUsers.Count} winners across {leagueYears.Count} leagues.");
        return Result.Success();
    }
}
