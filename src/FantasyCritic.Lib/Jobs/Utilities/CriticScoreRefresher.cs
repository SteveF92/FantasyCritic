using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Services;
using Serilog;

namespace FantasyCritic.Lib.Jobs.Utilities;

internal class CriticScoreRefresher
{
    private static readonly ILogger _logger = Log.ForContext<CriticScoreRefresher>();

    private readonly InterLeagueService _interLeagueService;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IMasterGameRepo _masterGameRepo;
    private readonly IOpenCriticService _openCriticService;
    private readonly DiscordPushService _discordPushService;
    private readonly IClock _clock;

    public CriticScoreRefresher(InterLeagueService interLeagueService, IFantasyCriticRepo fantasyCriticRepo, IMasterGameRepo masterGameRepo,
        IOpenCriticService openCriticService, DiscordPushService discordPushService, IClock clock)
    {
        _interLeagueService = interLeagueService;
        _fantasyCriticRepo = fantasyCriticRepo;
        _masterGameRepo = masterGameRepo;
        _openCriticService = openCriticService;
        _discordPushService = discordPushService;
        _clock = clock;
    }

    public async Task RefreshCriticInfo()
    {
        _logger.Information("Refreshing critic scores");
        var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
        if (!systemWideSettings.RefreshOpenCritic)
        {
            _logger.Information("Not refreshing Open Critic scores as the flag is turned off.");
            return;
        }

        var supportedYears = await _fantasyCriticRepo.GetSupportedYears();
        var masterGames = await _masterGameRepo.GetMasterGames();

        var currentDate = _clock.GetToday();
        var masterGamesToUpdate = masterGames.Where(x => x.OpenCriticID.HasValue && x.SyncWithExternalAPIs).ToList();
        int gamesFetched = 0;
        foreach (var masterGame in masterGamesToUpdate)
        {
            if (masterGame.IsReleased(currentDate) && masterGame.ReleaseDate.HasValue)
            {
                var year = masterGame.ReleaseDate.Value.Year;
                var supportedYear = supportedYears.SingleOrDefault(x => x.Year == year);
                if (supportedYear != null && supportedYear.Finished)
                {
                    continue;
                }
            }

            var openCriticGame = await _openCriticService.GetOpenCriticGame(masterGame.OpenCriticID!.Value);
            gamesFetched++;
            if (gamesFetched % 100 == 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            if (openCriticGame is not null)
            {
                if (openCriticGame.Score.HasValue && !masterGame.CriticScore.HasValue)
                {
                    _logger.Information($"Game {masterGame.GameName} has just recieved an OpenCritic score of: {openCriticGame.Score})");
                }

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
                _discordPushService.QueueGameCriticScoreUpdateMessage(masterGame, masterGame.CriticScore, openCriticGame.Score);
            }
            else
            {
                _logger.Warning($"Getting an open critic game failed (empty return): {masterGame.GameName} | [{masterGame.OpenCriticID.Value}]");
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

        _logger.Information("Done refreshing critic scores");
    }
}
