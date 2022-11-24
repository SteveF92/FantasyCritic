using FantasyCritic.Lib.Interfaces;
using Serilog;

namespace FantasyCritic.Lib.Services;
public class HypeFactorService : IHypeFactorService
{
    private static readonly ILogger _logger = Log.ForContext<AdminService>();

    private readonly IExternalHypeFactorService _externalHypeFactorService;
    private readonly IMasterGameRepo _masterGameRepo;
    private readonly InterLeagueService _interLeagueService;

    public HypeFactorService(IMasterGameRepo masterGameRepo, InterLeagueService interLeagueService, IExternalHypeFactorService externalHypeFactorService)
    {
        _masterGameRepo = masterGameRepo;
        _interLeagueService = interLeagueService;
        _externalHypeFactorService = externalHypeFactorService;
    }

    public async Task<HypeConstants> GetHypeConstants()
    {
        _logger.Information("Getting Hype Constants");
        var supportedYears = await _interLeagueService.GetSupportedYears();
        List<MasterGameYear> allMasterGameYears = new List<MasterGameYear>();

        foreach (var supportedYear in supportedYears)
        {
            if (supportedYear.Year < 2019)
            {
                continue;
            }

            var masterGamesForYear = await _masterGameRepo.GetMasterGameYears(supportedYear.Year);
            var relevantGames = masterGamesForYear.Where(x => x.IsRelevantInYear(supportedYear.Year));
            allMasterGameYears.AddRange(relevantGames);
        }

        var hypeConstants = await _externalHypeFactorService.GetHypeConstants(allMasterGameYears);
        _logger.Information($"Hype Constants: {hypeConstants}");

        return hypeConstants;
    }
}
