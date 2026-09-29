using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class ProcessSpecialAuctionsJobHandler : IConditionalCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.ProcessSpecialAuctions;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.EveryTenMinutes;

    private readonly InterLeagueService _interLeagueService;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IMasterGameRepo _masterGameRepo;
    private readonly DiscordPushService _discordPushService;
    private readonly IClock _clock;
    private readonly ILogger<ProcessSpecialAuctionsJobHandler> _logger;

    public ProcessSpecialAuctionsJobHandler(InterLeagueService interLeagueService, IFantasyCriticRepo fantasyCriticRepo,
        IMasterGameRepo masterGameRepo, DiscordPushService discordPushService, IClock clock, ILogger<ProcessSpecialAuctionsJobHandler> logger)
    {
        _interLeagueService = interLeagueService;
        _fantasyCriticRepo = fantasyCriticRepo;
        _masterGameRepo = masterGameRepo;
        _discordPushService = discordPushService;
        _clock = clock;
        _logger = logger;
    }

    public async Task<bool> ShouldSchedule()
    {
        var allSpecialAuctions = await _fantasyCriticRepo.GetAllActiveSpecialAuctions();
        var now = _clock.GetCurrentInstant();
        return allSpecialAuctions.Any(x => x.IsLocked(now));
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        SystemWideValues systemWideValues = await _interLeagueService.GetSystemWideValues();
        var supportedYears = await _interLeagueService.GetSupportedYears();
        foreach (var supportedYear in supportedYears)
        {
            if (supportedYear.Finished || !supportedYear.OpenForPlay)
            {
                continue;
            }

            await ProcessSpecialAuctionsForYear(systemWideValues, supportedYear.Year);
        }

        return Result.Success();
    }

    private async Task ProcessSpecialAuctionsForYear(SystemWideValues systemWideValues, int year)
    {
        _logger.LogInformation($"Processing special auctions for {year}.");
        var now = _clock.GetCurrentInstant();
        IReadOnlyList<LeagueYear> allLeagueYears = await _fantasyCriticRepo.GetLeagueYears(year);
        var results = await GetSpecialAuctionResults(systemWideValues, year, now, allLeagueYears);
        if (results.IsEmpty())
        {
            return;
        }

        await _fantasyCriticRepo.SaveProcessedActionResults(results);
        await _discordPushService.SendActionProcessingSummary(results.GetLeagueActionSets());
    }

    private async Task<FinalizedActionProcessingResults> GetSpecialAuctionResults(SystemWideValues systemWideValues, int year, Instant processingTime, IReadOnlyList<LeagueYear> allLeagueYears)
    {
        var allSpecialAuctions = await _fantasyCriticRepo.GetAllActiveSpecialAuctions();
        var specialAuctionsToProcess = allSpecialAuctions.Where(x => !x.Processed && x.IsLocked(processingTime));
        var groupedByLeagueYear = specialAuctionsToProcess.GroupBy(x => x.LeagueYearKey);
        var leagueYearDictionary = allLeagueYears.ToDictionary(x => x.Key);
        IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> leaguesAndBids = await _fantasyCriticRepo.GetActivePickupBids(year, allLeagueYears);
        List<LeagueYearSpecialAuctionSet> specialAuctionSets = [];
        foreach (var leagueYearGroup in groupedByLeagueYear)
        {
            var leagueYear = leagueYearDictionary[leagueYearGroup.Key];
            var bidsForLeagueYear = leaguesAndBids[leagueYear];
            List<SpecialAuctionWithBids> specialAuctionsWithBids = [];
            foreach (var specialAuction in leagueYearGroup)
            {
                var bidsForGame = bidsForLeagueYear.Where(x => x.MasterGame.Equals(specialAuction.MasterGameYear.MasterGame)).ToList();
                specialAuctionsWithBids.Add(new SpecialAuctionWithBids(specialAuction, bidsForGame));
            }

            specialAuctionSets.Add(new LeagueYearSpecialAuctionSet(leagueYear, specialAuctionsWithBids));
        }

        var masterGameYears = await _interLeagueService.GetMasterGameYears(year);
        var masterGameYearDictionary = masterGameYears.ToDictionary(x => x.MasterGame.MasterGameID);

        var currentDate = _clock.GetToday();

        var allTags = await _masterGameRepo.GetMasterGameTags();
        var actionProcessor = new ActionProcessor(systemWideValues, processingTime, currentDate, masterGameYearDictionary, allTags);
        FinalizedActionProcessingResults results = actionProcessor.ProcessSpecialAuctions(specialAuctionSets);
        return results;
    }
}
