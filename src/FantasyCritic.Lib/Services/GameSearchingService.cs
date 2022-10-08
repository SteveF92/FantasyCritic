using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Lib.Services;

public class GameSearchingService
{
    private readonly InterLeagueService _interLeagueService;
    private readonly IClock _clock;

    public GameSearchingService(InterLeagueService interLeagueService, IClock clock)
    {
        _interLeagueService = interLeagueService;
        _clock = clock;
    }

    public async Task<IReadOnlyList<PossibleMasterGameYear>> GetAllPossibleMasterGameYearsForLeagueYear(LeagueYear leagueYear, Publisher currentPublisher, int year)
    {
        HashSet<MasterGame> publisherMasterGames = leagueYear.Publishers
            .SelectMany(x => x.PublisherGames)
            .Where(x => !x.CounterPick && x.MasterGame is not null)
            .Select(x => x.MasterGame!.MasterGame)
            .ToHashSet();

        HashSet<MasterGame> myPublisherMasterGames = currentPublisher.MyMasterGames;

        IReadOnlyList<MasterGameYear> masterGames = await _interLeagueService.GetMasterGameYears(year);
        List<PossibleMasterGameYear> possibleMasterGames = new List<PossibleMasterGameYear>();
        var slots = currentPublisher.GetPublisherSlots(leagueYear.Options);
        var openNonCounterPickSlots = slots.Where(x => !x.CounterPick && x.PublisherGame is null).OrderBy(x => x.SlotNumber).ToList();

        LocalDate currentDate = _clock.GetToday();
        foreach (var masterGame in masterGames)
        {
            var eligibilityFactors = leagueYear.GetEligibilityFactorsForMasterGame(masterGame.MasterGame, currentDate);
            PossibleMasterGameYear possibleMasterGame = GetPossibleMasterGameYear(masterGame, openNonCounterPickSlots,
                publisherMasterGames, myPublisherMasterGames, eligibilityFactors, currentDate);
            possibleMasterGames.Add(possibleMasterGame);
        }

        return possibleMasterGames;
    }

    public async Task<IReadOnlyList<MasterGameYear>> SearchAllGames(string searchName, int year)
    {
        var masterGames = await _interLeagueService.GetMasterGameYears(year);
        var matchingMasterGames = MasterGameSearching.SearchMasterGameYears(searchName, masterGames);
        return matchingMasterGames;
    }

    public async Task<IReadOnlyList<PossibleMasterGameYear>> SearchGames(string searchName, LeagueYear leagueYear, Publisher currentPublisher, int year)
    {
        HashSet<MasterGame> publisherMasterGames = leagueYear.Publishers
            .SelectMany(x => x.PublisherGames)
            .Where(x => !x.CounterPick && x.MasterGame is not null)
            .Select(x => x.MasterGame!.MasterGame)
            .ToHashSet();

        HashSet<MasterGame> myPublisherMasterGames = currentPublisher.MyMasterGames;

        IReadOnlyList<MasterGameYear> masterGames = await _interLeagueService.GetMasterGameYears(year);
        IReadOnlyList<MasterGameYear> matchingMasterGames = MasterGameSearching.SearchMasterGameYears(searchName, masterGames);
        List<PossibleMasterGameYear> possibleMasterGames = new List<PossibleMasterGameYear>();
        var slots = currentPublisher.GetPublisherSlots(leagueYear.Options);
        var openNonCounterPickSlots = slots.Where(x => !x.CounterPick && x.PublisherGame is null).OrderBy(x => x.SlotNumber).ToList();

        LocalDate currentDate = _clock.GetToday();
        foreach (var masterGame in matchingMasterGames)
        {
            var eligibilityFactors = leagueYear.GetEligibilityFactorsForMasterGame(masterGame.MasterGame, currentDate);
            PossibleMasterGameYear possibleMasterGame = GetPossibleMasterGameYear(masterGame, openNonCounterPickSlots, publisherMasterGames,
                myPublisherMasterGames, eligibilityFactors, currentDate);
            possibleMasterGames.Add(possibleMasterGame);
        }

        return possibleMasterGames;
    }

    public async Task<IReadOnlyList<PossibleMasterGameYear>> GetTopAvailableGames(LeagueYear leagueYear, Publisher currentPublisher)
    {
        HashSet<MasterGame> publisherMasterGames = leagueYear.Publishers
            .SelectMany(x => x.PublisherGames)
            .Where(x => !x.CounterPick && x.MasterGame is not null)
            .Select(x => x.MasterGame!.MasterGame)
            .ToHashSet();

        HashSet<MasterGame> myPublisherMasterGames = currentPublisher.MyMasterGames;

        IReadOnlyList<MasterGameYear> masterGames = await _interLeagueService.GetMasterGameYears(leagueYear.Year);
        IReadOnlyList<MasterGameYear> matchingMasterGames = masterGames.OrderByDescending(x => x.DateAdjustedHypeFactor).ToList();
        List<PossibleMasterGameYear> possibleMasterGames = new List<PossibleMasterGameYear>();
        var slots = currentPublisher.GetPublisherSlots(leagueYear.Options);
        var openNonCounterPickSlots = slots.Where(x => !x.CounterPick && x.PublisherGame is null).OrderBy(x => x.SlotNumber).ToList();

        LocalDate currentDate = _clock.GetToday();
        foreach (var masterGame in matchingMasterGames)
        {
            var eligibilityFactors = leagueYear.GetEligibilityFactorsForMasterGame(masterGame.MasterGame, currentDate);
            PossibleMasterGameYear possibleMasterGame = GetPossibleMasterGameYear(masterGame, openNonCounterPickSlots,
                publisherMasterGames, myPublisherMasterGames, eligibilityFactors, currentDate);

            if (!possibleMasterGame.IsAvailable)
            {
                continue;
            }

            possibleMasterGames.Add(possibleMasterGame);
        }

        return possibleMasterGames;
    }

    public async Task<IReadOnlyList<PossibleMasterGameYear>> GetTopAvailableGamesForSlot(LeagueYear leagueYear, Publisher currentPublisher, IEnumerable<LeagueTagStatus> leagueTagRequirements)
    {
        HashSet<MasterGame> publisherMasterGames = leagueYear.Publishers
            .SelectMany(x => x.PublisherGames)
            .Where(x => !x.CounterPick && x.MasterGame is not null)
            .Select(x => x.MasterGame!.MasterGame)
            .ToHashSet();

        HashSet<MasterGame> myPublisherMasterGames = currentPublisher.MyMasterGames;

        IReadOnlyList<MasterGameYear> masterGames = await _interLeagueService.GetMasterGameYears(leagueYear.Year);
        IReadOnlyList<MasterGameYear> matchingMasterGames = masterGames.OrderByDescending(x => x.DateAdjustedHypeFactor).ToList();
        List<PossibleMasterGameYear> possibleMasterGames = new List<PossibleMasterGameYear>();
        var slots = currentPublisher.GetPublisherSlots(leagueYear.Options);
        var openNonCounterPickSlots = slots.Where(x => !x.CounterPick && x.PublisherGame is null).OrderBy(x => x.SlotNumber).ToList();

        LocalDate currentDate = _clock.GetToday();
        foreach (var masterGame in matchingMasterGames)
        {
            var eligibilityFactors = leagueYear.GetEligibilityFactorsForMasterGame(masterGame.MasterGame, currentDate);
            PossibleMasterGameYear possibleMasterGame = GetPossibleMasterGameYear(masterGame, openNonCounterPickSlots,
                publisherMasterGames, myPublisherMasterGames, eligibilityFactors, leagueTagRequirements, currentDate);

            if (!possibleMasterGame.IsAvailable)
            {
                continue;
            }

            possibleMasterGames.Add(possibleMasterGame);
        }

        return possibleMasterGames;
    }

    public async Task<IReadOnlyList<PossibleMasterGameYear>> GetQueuedPossibleGames(LeagueYear leagueYear, Publisher currentPublisher, IEnumerable<QueuedGame> queuedGames)
    {
        HashSet<MasterGame> publisherMasterGames = leagueYear.Publishers
            .SelectMany(x => x.PublisherGames)
            .Where(x => !x.CounterPick && x.MasterGame is not null)
            .Select(x => x.MasterGame!.MasterGame)
            .ToHashSet();

        HashSet<MasterGame> myPublisherMasterGames = currentPublisher.MyMasterGames;

        IReadOnlyList<MasterGameYear> masterGameYears = await _interLeagueService.GetMasterGameYears(leagueYear.Year);
        var masterGamesForThisYear = masterGameYears.Where(x => x.Year == leagueYear.Year);
        var masterGameYearDictionary = masterGamesForThisYear.ToDictionary(x => x.MasterGame.MasterGameID, y => y);
        var slots = currentPublisher.GetPublisherSlots(leagueYear.Options);
        var openNonCounterPickSlots = slots.Where(x => !x.CounterPick && x.PublisherGame is null).OrderBy(x => x.SlotNumber).ToList();

        List<PossibleMasterGameYear> possibleMasterGames = new List<PossibleMasterGameYear>();
        LocalDate currentDate = _clock.GetToday();
        foreach (var queuedGame in queuedGames)
        {
            var masterGame = masterGameYearDictionary[queuedGame.MasterGame.MasterGameID];

            var eligibilityFactors = leagueYear.GetEligibilityFactorsForMasterGame(masterGame.MasterGame, currentDate);
            PossibleMasterGameYear possibleMasterGame = GetPossibleMasterGameYear(masterGame, openNonCounterPickSlots,
                publisherMasterGames, myPublisherMasterGames, eligibilityFactors, currentDate);
            possibleMasterGames.Add(possibleMasterGame);
        }

        return possibleMasterGames;
    }

    public static PossibleMasterGameYear GetPossibleMasterGameYear(MasterGameYear masterGame, IReadOnlyList<PublisherSlot> openNonCounterPickSlots,
        HashSet<MasterGame> publisherStandardMasterGames, HashSet<MasterGame> myPublisherMasterGames,
        MasterGameWithEligibilityFactors eligibilityFactors, LocalDate currentDate)
    {
        bool isEligible = SlotEligibilityFunctions.GameIsEligibleInLeagueYear(eligibilityFactors);
        bool taken = publisherStandardMasterGames.Contains(masterGame.MasterGame);
        bool alreadyOwned = myPublisherMasterGames.Contains(masterGame.MasterGame);
        bool isReleased = masterGame.MasterGame.IsReleased(currentDate);
        bool willRelease = masterGame.WillRelease();
        bool hasScore = masterGame.MasterGame.CriticScore.HasValue;
        bool isEligibleInOpenSlot = SlotEligibilityFunctions.GameIsEligibleInOpenSlot(openNonCounterPickSlots, eligibilityFactors);

        PossibleMasterGameYear possibleMasterGame = new PossibleMasterGameYear(masterGame, taken, alreadyOwned, isEligible, isEligibleInOpenSlot, isReleased, willRelease, hasScore);
        return possibleMasterGame;
    }

    public PossibleMasterGameYear GetPossibleMasterGameYear(MasterGameYear masterGame, IReadOnlyList<PublisherSlot> openNonCounterPickSlots,
        HashSet<MasterGame> publisherStandardMasterGames, HashSet<MasterGame> myPublisherMasterGames,
        MasterGameWithEligibilityFactors eligibilityFactors, IEnumerable<LeagueTagStatus> tagsForSlot, LocalDate currentDate)
    {
        var tagsToUse = eligibilityFactors.TagOverrides.Any() ? eligibilityFactors.TagOverrides : masterGame.MasterGame.Tags;
        var claimErrors = LeagueTagExtensions.GameHasValidTags(tagsForSlot, new List<LeagueTagStatus>(), masterGame.MasterGame, tagsToUse, currentDate);
        bool isEligible = !claimErrors.Any();
        bool taken = publisherStandardMasterGames.Contains(masterGame.MasterGame);
        bool alreadyOwned = myPublisherMasterGames.Contains(masterGame.MasterGame);
        bool isReleased = masterGame.MasterGame.IsReleased(currentDate);
        bool willRelease = masterGame.WillRelease();
        bool hasScore = masterGame.MasterGame.CriticScore.HasValue;
        bool isEligibleInOpenSlot = SlotEligibilityFunctions.GameIsEligibleInOpenSlot(openNonCounterPickSlots, eligibilityFactors);

        PossibleMasterGameYear possibleMasterGame = new PossibleMasterGameYear(masterGame, taken, alreadyOwned, isEligible, isEligibleInOpenSlot, isReleased, willRelease, hasScore);
        return possibleMasterGame;
    }
}
