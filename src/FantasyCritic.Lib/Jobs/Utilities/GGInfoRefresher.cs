using FantasyCritic.Lib.GG;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using Serilog;

namespace FantasyCritic.Lib.Jobs.Utilities;

internal class GGInfoRefresher
{
    private static readonly ILogger _logger = Log.ForContext<GGInfoRefresher>();

    private readonly InterLeagueService _interLeagueService;
    private readonly IMasterGameRepo _masterGameRepo;
    private readonly IGGService _ggService;

    public GGInfoRefresher(InterLeagueService interLeagueService, IMasterGameRepo masterGameRepo, IGGService ggService)
    {
        _interLeagueService = interLeagueService;
        _masterGameRepo = masterGameRepo;
        _ggService = ggService;
    }

    public async Task RefreshGGInfo(bool deepRefresh)
    {
        var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
        if (!systemWideSettings.RefreshOpenCritic)
        {
            _logger.Information("Not refreshing GG data as the flag is turned off.");
            return;
        }

        _logger.Information("Refreshing GG Info. Deep:{deepRefresh}", deepRefresh);
        var masterGames = await _masterGameRepo.GetMasterGames();

        var masterGamesToUpdate = masterGames.Where(x => x.GGToken is not null && x.SyncWithExternalAPIs).ToList();
        foreach (var masterGame in masterGamesToUpdate)
        {
            if (!string.IsNullOrWhiteSpace(masterGame.GGCoverArtFileName) && !deepRefresh)
            {
                continue;
            }

            var ggGame = await _ggService.GetGGGame(masterGame.GGToken!);
            if (ggGame is not null)
            {
                await _masterGameRepo.UpdateGGStats(masterGame, ggGame);
            }
            else
            {
                _logger.Warning($"Getting an GG| game failed (empty return): {masterGame.GameName} | [{masterGame.GGToken}]");
            }
        }

        _logger.Information("Done Refreshing GG Info");
    }
}
