using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Services;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Utilities;

internal class CriticScoreRefresher
{
    private readonly InterLeagueService _interLeagueService;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IMasterGameRepo _masterGameRepo;
    private readonly IOpenCriticService _openCriticService;
    private readonly IClock _clock;
    private readonly ILogger<CriticScoreRefresher> _logger;

    public CriticScoreRefresher(InterLeagueService interLeagueService, IFantasyCriticRepo fantasyCriticRepo, IMasterGameRepo masterGameRepo,
        IOpenCriticService openCriticService, IClock clock, ILogger<CriticScoreRefresher> logger)
    {
        _interLeagueService = interLeagueService;
        _fantasyCriticRepo = fantasyCriticRepo;
        _masterGameRepo = masterGameRepo;
        _openCriticService = openCriticService;
        _clock = clock;
        _logger = logger;
    }

    public async Task RefreshCriticInfo(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
        if (!systemWideSettings.RefreshOpenCritic)
        {
            _logger.LogInformation("Not refreshing critic scores: RefreshOpenCritic is off.");
            await context.AppendDetailedStatus("Critic scores: skipped, RefreshOpenCritic is off.");
            return;
        }

        var supportedYears = await _fantasyCriticRepo.GetSupportedYears();
        var masterGames = await _masterGameRepo.GetMasterGames();

        //A game released in a finished year can't change that year's standings, so it isn't fetched.
        var currentDate = _clock.GetToday();
        var finishedYears = supportedYears.Where(x => x.Finished).Select(x => x.Year).ToHashSet();
        var masterGamesToCheck = masterGames
            .Where(x => x.OpenCriticID.HasValue && x.SyncWithExternalAPIs)
            .Where(x => !(x.IsReleased(currentDate) && x.ReleaseDate.HasValue && finishedYears.Contains(x.ReleaseDate.Value.Year)))
            .ToList();
        _logger.LogInformation("Refreshing critic scores for {GameCount} games.", masterGamesToCheck.Count);

        int gamesFetched = 0;
        int scoresChanged = 0;
        List<string> newlyScoredGames = [];
        int fetchFailures = 0;
        foreach (var masterGame in masterGamesToCheck)
        {
            //Each game is fetched then written, so a stop lands between games, and the next run just fetches that game again.
            cancellationToken.ThrowIfCancellationRequested();

            var openCriticGame = await _openCriticService.GetOpenCriticGame(masterGame.OpenCriticID!.Value);
            gamesFetched++;
            if (gamesFetched % 100 == 0)
            {
                await context.AddTemporaryStatus($"Critic scores: checked {gamesFetched} of {masterGamesToCheck.Count} games.");
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }

            if (openCriticGame is not null)
            {
                var currentCriticScore = masterGame.CriticScore;
                var newCriticScore = openCriticGame.Score;
                if (!currentCriticScore.HasValue && !newCriticScore.HasValue)
                {
                    continue;
                }

                if (currentCriticScore.HasValue && newCriticScore.HasValue)
                {
                    bool scoreChangedOverThreshold = Math.Abs(newCriticScore.Value - currentCriticScore.Value) >= 0.0002m;
                    if (!scoreChangedOverThreshold)
                    {
                        continue;
                    }
                }

                await _masterGameRepo.UpdateCriticStats(masterGame, openCriticGame);
                //No cancellation check between these two: the update carries the old score, which the save just overwrote.
                await _masterGameRepo.AddPendingScoreUpdate(new GameCriticScoreUpdateMessage(masterGame, masterGame.CriticScore, openCriticGame.Score));
                scoresChanged++;
                if (!currentCriticScore.HasValue)
                {
                    newlyScoredGames.Add(masterGame.GameName);
                    _logger.LogInformation("{GameName} ({MasterGameID}) received its first OpenCritic score: {NewScore}.",
                        masterGame.GameName, masterGame.MasterGameID, newCriticScore);
                }
                else
                {
                    _logger.LogInformation("{GameName} ({MasterGameID}) OpenCritic score changed from {OldScore} to {NewScore}.",
                        masterGame.GameName, masterGame.MasterGameID, currentCriticScore, newCriticScore);
                }
            }
            else
            {
                fetchFailures++;
                _logger.LogWarning("OpenCritic returned nothing for {GameName} ({MasterGameID}), OpenCritic ID {OpenCriticID}.",
                    masterGame.GameName, masterGame.MasterGameID, masterGame.OpenCriticID.Value);
            }

            foreach (var subGame in masterGame.SubGames)
            {
                if (!subGame.OpenCriticID.HasValue)
                {
                    continue;
                }

                var subGameOpenCriticGame = await _openCriticService.GetOpenCriticGame(subGame.OpenCriticID.Value);
                if (subGameOpenCriticGame is not null)
                {
                    await _masterGameRepo.UpdateCriticStats(subGame, subGameOpenCriticGame);
                }
            }
        }

        _masterGameRepo.ClearMasterGameCache();

        _logger.LogInformation("Refreshed critic scores: {GamesFetched} checked, {ScoresChanged} changed, {NewlyScoredCount} newly scored, {FetchFailures} failed to fetch.",
            gamesFetched, scoresChanged, newlyScoredGames.Count, fetchFailures);
        await context.AppendDetailedStatus(DescribeRefresh(gamesFetched, scoresChanged, newlyScoredGames, fetchFailures));
    }

    private static string DescribeRefresh(int gamesFetched, int scoresChanged, IReadOnlyList<string> newlyScoredGames, int fetchFailures)
    {
        List<string> parts = [$"checked {gamesFetched} games", $"{scoresChanged} changed"];
        if (newlyScoredGames.Count > 0)
        {
            parts.Add($"{newlyScoredGames.Count} newly scored ({string.Join(", ", newlyScoredGames)})");
        }

        if (fetchFailures > 0)
        {
            parts.Add($"{fetchFailures} failed to fetch");
        }

        return $"Critic scores: {string.Join("; ", parts)}.";
    }
}
