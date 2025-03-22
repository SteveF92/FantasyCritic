using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Statistics;
using MathNet.Numerics.LinearRegression;
using Serilog;

namespace FantasyCritic.Lib.Services;
public class HypeFactorService : IHypeFactorService
{
    private static readonly ILogger _logger = Log.ForContext<AdminService>();

    private readonly IMasterGameRepo _masterGameRepo;
    private readonly InterLeagueService _interLeagueService;

    public HypeFactorService(IMasterGameRepo masterGameRepo, InterLeagueService interLeagueService)
    {
        _masterGameRepo = masterGameRepo;
        _interLeagueService = interLeagueService;
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
            var relevantGames = masterGamesForYear.Where(x => x.IsRelevantInYear(supportedYear.Year, false));
            allMasterGameYears.AddRange(relevantGames);
        }

        var hypeConstants = RunRegression(allMasterGameYears);
        _logger.Information($"Hype Constants: {hypeConstants}");

        return hypeConstants;
    }

    private static HypeConstants RunRegression(IReadOnlyList<MasterGameYear> allMasterGameYears)
    {
        var models = allMasterGameYears.Select(x => new MasterGameYearScriptInput(x)).Where(x => x.CriticScore.HasValue).ToList();

        var xData = models.Select(d => new[]
        {
            d.EligiblePercentStandardGame,
            d.AdjustedPercentCounterPick,
            d.DateAdjustedHypeFactor
        }).ToArray();

        var yData = models.Select(d => d.CriticScore!.Value).ToArray();

        var coefficients = MultipleRegression.NormalEquations(xData, yData, intercept: true);

        return new HypeConstants(coefficients[0], coefficients[1], coefficients[2], coefficients[3]);
    }
}
