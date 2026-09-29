using FantasyCritic.Lib.GG;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Utilities;

internal class GGInfoRefresher
{
    private readonly InterLeagueService _interLeagueService;
    private readonly IMasterGameRepo _masterGameRepo;
    private readonly IGGService _ggService;
    private readonly ILogger<GGInfoRefresher> _logger;

    public GGInfoRefresher(InterLeagueService interLeagueService, IMasterGameRepo masterGameRepo, IGGService ggService, ILogger<GGInfoRefresher> logger)
    {
        _interLeagueService = interLeagueService;
        _masterGameRepo = masterGameRepo;
        _ggService = ggService;
        _logger = logger;
    }

    public async Task RefreshGGInfo(bool deepRefresh, FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var refreshType = deepRefresh ? "deep" : "shallow";
        var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
        if (!systemWideSettings.RefreshOpenCritic)
        {
            _logger.LogInformation("Not refreshing GG data: RefreshOpenCritic is off.");
            await context.AppendDetailedStatus("GG: skipped, RefreshOpenCritic is off.");
            return;
        }

        var masterGames = await _masterGameRepo.GetMasterGames();

        //A shallow refresh only fetches games that don't have cover art yet.
        var masterGamesToCheck = masterGames
            .Where(x => x.GGToken is not null && x.SyncWithExternalAPIs)
            .Where(x => deepRefresh || string.IsNullOrWhiteSpace(x.GGCoverArtFileName))
            .ToList();
        _logger.LogInformation("Refreshing GG data ({RefreshType}) for {GameCount} games.", refreshType, masterGamesToCheck.Count);

        int gamesFetched = 0;
        int fetchFailures = 0;
        foreach (var masterGame in masterGamesToCheck)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var ggGame = await _ggService.GetGGGame(masterGame.GGToken!);
            gamesFetched++;
            if (gamesFetched % 100 == 0)
            {
                await context.AddTemporaryStatus($"GG ({refreshType}): checked {gamesFetched} of {masterGamesToCheck.Count} games.");
            }

            if (ggGame is not null)
            {
                await _masterGameRepo.UpdateGGStats(masterGame, ggGame);
            }
            else
            {
                fetchFailures++;
                _logger.LogWarning("GG returned nothing for {GameName} ({MasterGameID}), GG token {GGToken}.",
                    masterGame.GameName, masterGame.MasterGameID, masterGame.GGToken);
            }
        }

        _logger.LogInformation("Refreshed GG data ({RefreshType}): {GamesFetched} checked, {FetchFailures} failed to fetch.",
            refreshType, gamesFetched, fetchFailures);
        var failureText = fetchFailures > 0 ? $"; {fetchFailures} failed to fetch" : "";
        await context.AppendDetailedStatus($"GG ({refreshType}): checked {gamesFetched} games{failureText}.");
    }
}
