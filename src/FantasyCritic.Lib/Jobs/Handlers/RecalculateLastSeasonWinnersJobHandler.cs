using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RecalculateLastSeasonWinnersJobHandler : IFantasyCriticJobHandler
{
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly FantasyCriticService _fantasyCriticService;

    public RecalculateLastSeasonWinnersJobHandler(IFantasyCriticRepo fantasyCriticRepo, FantasyCriticService fantasyCriticService)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _fantasyCriticService = fantasyCriticService;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.RecalculateLastSeasonWinners;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var supportedYears = await _fantasyCriticRepo.GetSupportedYears();
        var mostRecentFinishedYear = supportedYears.Where(x => x.Finished).OrderByDescending(x => x.Year).First();
        IReadOnlyList<LeagueYear> leagueYears = await _fantasyCriticRepo.GetLeagueYears(mostRecentFinishedYear.Year);
        var calculatedStats = _fantasyCriticService.GetCalculatedStatsForYear(mostRecentFinishedYear.Year, leagueYears, true);
        await _fantasyCriticRepo.UpdateLeagueWinners(calculatedStats.WinningUsers, true);

        return Result.Success();
    }
}
