using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Services;

public class DraftService
{
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IClock _clock;
    private readonly GameAcquisitionService _gameAcquisitionService;
    private readonly LeagueMemberService _leagueMemberService;
    private readonly PublisherService _publisherService;
    private readonly InterLeagueService _interLeagueService;
    private readonly GameSearchingService _gameSearchingService;

    public DraftService(GameAcquisitionService gameAcquisitionService, LeagueMemberService leagueMemberService,
        PublisherService publisherService, InterLeagueService interLeagueService, IFantasyCriticRepo fantasyCriticRepo,
        GameSearchingService gameSearchingService, IClock clock)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _clock = clock;

        _leagueMemberService = leagueMemberService;
        _publisherService = publisherService;
        _interLeagueService = interLeagueService;
        _gameAcquisitionService = gameAcquisitionService;
        _gameSearchingService = gameSearchingService;
    }

    public async Task<StartDraftResult> GetStartDraftResult(LeagueYear leagueYear, IReadOnlyList<Publisher> publishersInLeague, IReadOnlyList<FantasyCriticUser> activeUsers)
    {
        if (leagueYear.PlayStatus.PlayStarted)
        {
            return new StartDraftResult(true, new List<string>());
        }

        var supportedYears = await _fantasyCriticRepo.GetSupportedYears();
        var supportedYear = supportedYears.Single(x => x.Year == leagueYear.Year);

        List<string> errors = new List<string>();

        if (activeUsers.Count() < 2)
        {
            errors.Add("You need to have at least two players in the league.");
        }

        if (activeUsers.Count() > 20)
        {
            errors.Add("You cannot have more than 20 players in the league.");
        }

        if (publishersInLeague.Count() != activeUsers.Count())
        {
            errors.Add("Not every player has created a publisher.");
        }

        if (!supportedYear.OpenForPlay)
        {
            errors.Add($"This year is not yet open for play. It will become available on {supportedYear.StartDate}.");
        }

        return new StartDraftResult(!errors.Any(), errors);
    }

    public bool LeagueIsReadyToSetDraftOrder(IEnumerable<Publisher> publishersInLeague, IEnumerable<FantasyCriticUser> activeUsers)
    {
        if (publishersInLeague.Count() != activeUsers.Count())
        {
            return false;
        }

        if (publishersInLeague.Count() < 2 || publishersInLeague.Count() > 20)
        {
            return false;
        }

        return true;
    }

    public bool LeagueIsReadyToPlay(SupportedYear supportedYear, IEnumerable<Publisher> publishersInLeague, IEnumerable<FantasyCriticUser> activeUsers)
    {
        if (!LeagueIsReadyToSetDraftOrder(publishersInLeague, activeUsers))
        {
            return false;
        }

        if (!supportedYear.OpenForPlay)
        {
            return false;
        }

        return true;
    }

    public async Task<bool> StartDraft(LeagueYear leagueYear)
    {
        await _fantasyCriticRepo.StartDraft(leagueYear);
        var updatedLeagueYear = await _fantasyCriticRepo.GetLeagueYear(leagueYear.League, leagueYear.Year);
        var autoDraftResult = await AutoDraftForLeague(updatedLeagueYear.Value, 0, 0);
        var draftComplete = await CompleteDraft(leagueYear, autoDraftResult.StandardGamesAdded, autoDraftResult.CounterPicksAdded);
        return draftComplete;
    }

    public async Task SetDraftPause(LeagueYear leagueYear, bool pause)
    {
        await _fantasyCriticRepo.SetDraftPause(leagueYear, pause);
        var updatedLeagueYear = await _fantasyCriticRepo.GetLeagueYear(leagueYear.League, leagueYear.Year);
        if (!pause)
        {
            await AutoDraftForLeague(updatedLeagueYear.Value, 0, 0);
        }
    }

    public async Task UndoLastDraftAction(IReadOnlyList<Publisher> publishers)
    {
        var publisherGames = publishers.SelectMany(x => x.PublisherGames);
        var newestGame = publisherGames.WhereMax(x => x.Timestamp).First();

        var publisher = publishers.Single(x => x.PublisherGames.Select(y => y.PublisherGameID).Contains(newestGame.PublisherGameID));

        await _publisherService.RemovePublisherGame(publisher, newestGame);
    }

    public async Task<Result> SetDraftOrder(LeagueYear leagueYear, IReadOnlyList<KeyValuePair<Publisher, int>> draftPositions)
    {
        int publishersCount = leagueYear.Publishers.Count;
        if (publishersCount != draftPositions.Count)
        {
            return Result.Failure("Not setting all publishers.");
        }

        var requiredNumbers = Enumerable.Range(1, publishersCount).ToList();
        var requestedDraftNumbers = draftPositions.Select(x => x.Value);
        bool allRequiredPresent = new HashSet<int>(requiredNumbers).SetEquals(requestedDraftNumbers);
        if (!allRequiredPresent)
        {
            return Result.Failure("Some of the positions are not valid.");
        }

        await _fantasyCriticRepo.SetDraftOrder(draftPositions);
        return Result.Success();
    }

    public Maybe<Publisher> GetNextDraftPublisher(LeagueYear leagueYear)
    {
        if (!leagueYear.PlayStatus.DraftIsActive)
        {
            return Maybe<Publisher>.None;
        }

        var phase = GetDraftPhase(leagueYear);
        if (phase.Equals(DraftPhase.StandardGames))
        {
            var publishersWithLowestNumberOfGames = leagueYear.Publishers.WhereMin(x => x.PublisherGames.Count(y => !y.CounterPick));
            var allPlayersHaveSameNumberOfGames = leagueYear.Publishers.Select(x => x.PublisherGames.Count(y => !y.CounterPick)).Distinct().Count() == 1;
            var maxNumberOfGames = leagueYear.Publishers.Max(x => x.PublisherGames.Count(y => !y.CounterPick));
            var roundNumber = maxNumberOfGames;
            if (allPlayersHaveSameNumberOfGames)
            {
                roundNumber++;
            }

            bool roundNumberIsOdd = (roundNumber % 2 != 0);
            if (roundNumberIsOdd)
            {
                var sortedPublishersOdd = publishersWithLowestNumberOfGames.OrderBy(x => x.DraftPosition);
                var firstPublisherOdd = sortedPublishersOdd.First();
                return firstPublisherOdd;
            }
            //Else round is even
            var sortedPublishersEven = publishersWithLowestNumberOfGames.OrderByDescending(x => x.DraftPosition);
            var firstPublisherEven = sortedPublishersEven.First();
            return firstPublisherEven;
        }
        if (phase.Equals(DraftPhase.CounterPicks))
        {
            var publishersWithLowestNumberOfGames = leagueYear.Publishers.WhereMin(x => x.PublisherGames.Count(y => y.CounterPick));
            var allPlayersHaveSameNumberOfGames = leagueYear.Publishers.Select(x => x.PublisherGames.Count(y => y.CounterPick)).Distinct().Count() == 1;
            var maxNumberOfGames = leagueYear.Publishers.Max(x => x.PublisherGames.Count(y => y.CounterPick));

            var roundNumber = maxNumberOfGames;
            if (allPlayersHaveSameNumberOfGames)
            {
                roundNumber++;
            }

            bool roundNumberIsOdd = (roundNumber % 2 != 0);
            if (roundNumberIsOdd)
            {
                var sortedPublishersOdd = publishersWithLowestNumberOfGames.OrderByDescending(x => x.DraftPosition);
                var firstPublisherOdd = sortedPublishersOdd.First();
                return firstPublisherOdd;
            }
            //Else round is even
            var sortedPublishersEven = publishersWithLowestNumberOfGames.OrderBy(x => x.DraftPosition);
            var firstPublisherEven = sortedPublishersEven.First();
            return firstPublisherEven;
        }

        return Maybe<Publisher>.None;
    }

    public async Task<(ClaimResult Result, bool DraftComplete)> DraftGame(ClaimGameDomainRequest request, bool managerAction)
    {
        var result = await _gameAcquisitionService.ClaimGame(request, managerAction, true, true);
        int standardGamesAdded = 0;
        if (result.Success && !request.CounterPick)
        {
            standardGamesAdded = 1;
        }
        int counterPicksAdded = 0;
        if (result.Success && request.CounterPick)
        {
            counterPicksAdded = 1;
        }

        var autoDraftResult = await AutoDraftForLeague(request.LeagueYear, standardGamesAdded, counterPicksAdded);
        var draftComplete = await CompleteDraft(request.LeagueYear, autoDraftResult.StandardGamesAdded, autoDraftResult.CounterPicksAdded);
        return (result, draftComplete);
    }

    public async Task<bool> RunAutoDraftAndCheckIfComplete(LeagueYear leagueYear)
    {
        var autoDraftResult = await AutoDraftForLeague(leagueYear, 0, 0);
        var draftComplete = await CompleteDraft(leagueYear, autoDraftResult.StandardGamesAdded, autoDraftResult.CounterPicksAdded);
        return draftComplete;
    }

    private async Task<(int StandardGamesAdded, int CounterPicksAdded)> AutoDraftForLeague(LeagueYear leagueYear, int standardGamesAdded, int counterPicksAdded)
    {
        await Task.Delay(1000);
        var today = _clock.GetToday();
        var updatedLeagueYear = (await _fantasyCriticRepo.GetLeagueYear(leagueYear.League, leagueYear.Year)).Value;
        var nextPublisher = GetNextDraftPublisher(leagueYear);
        if (nextPublisher.HasNoValue)
        {
            return (standardGamesAdded, counterPicksAdded);
        }
        if (!nextPublisher.Value.AutoDraft)
        {
            return (standardGamesAdded, counterPicksAdded);
        }

        var draftPhase = GetDraftPhase(leagueYear);
        var draftStatus = GetDraftStatus(draftPhase, leagueYear);
        if (draftPhase.Equals(DraftPhase.Complete))
        {
            return (standardGamesAdded, counterPicksAdded);
        }
        if (draftPhase.Equals(DraftPhase.StandardGames))
        {
            var publisherWatchList = await _publisherService.GetQueuedGames(nextPublisher.Value);
            var availableGames = await _gameSearchingService.GetTopAvailableGames(updatedLeagueYear, nextPublisher.Value, leagueYear.Year);
            var availableGamesEligibleInRemainingSlots = new List<PossibleMasterGameYear>();
            var openSlots = nextPublisher.Value.GetPublisherSlots(leagueYear.Options).Where(x => !x.CounterPick && x.PublisherGame.HasNoValue).ToList();
            foreach (var availableGame in availableGames)
            {
                foreach (var slot in openSlots)
                {
                    var eligibilityFactors = leagueYear.GetEligibilityFactorsForMasterGame(availableGame.MasterGame.MasterGame, today);
                    var claimErrors = SlotEligibilityService.GetClaimErrorsForSlot(slot, eligibilityFactors);
                    if (!claimErrors.Any())
                    {
                        availableGamesEligibleInRemainingSlots.Add(availableGame);
                        break;
                    }
                }
            }

            var gamesToTake = publisherWatchList.OrderBy(x => x.Rank).Select(x => x.MasterGame)
                .Concat(availableGamesEligibleInRemainingSlots.Select(x => x.MasterGame.MasterGame));

            foreach (var possibleGame in gamesToTake)
            {
                var request = new ClaimGameDomainRequest(updatedLeagueYear, nextPublisher.Value, possibleGame.GameName, false, false, false, true, possibleGame, draftStatus.DraftPosition, draftStatus.OverallDraftPosition);
                var autoDraftResult = await _gameAcquisitionService.ClaimGame(request, false, true, true);
                if (autoDraftResult.Success)
                {
                    standardGamesAdded++;
                    break;
                }
            }
        }
        else if (draftPhase.Equals(DraftPhase.CounterPicks))
        {
            var otherPublisherGames = updatedLeagueYear.Publishers.Where(x => x.PublisherID != nextPublisher.Value.PublisherID)
                .SelectMany(x => x.PublisherGames)
                .Where(x => !x.CounterPick)
                .Where(x => x.MasterGame.HasValue);
            var possibleGames = otherPublisherGames
                .Select(x => x.MasterGame.Value)
                .Where(x => x.AdjustedPercentCounterPick.HasValue)
                .OrderByDescending(x => x.AdjustedPercentCounterPick);
            foreach (var possibleGame in possibleGames)
            {
                var request = new ClaimGameDomainRequest(updatedLeagueYear, nextPublisher.Value, possibleGame.MasterGame.GameName, true, false, false, true, possibleGame.MasterGame,
                    draftStatus.DraftPosition, draftStatus.OverallDraftPosition);
                var autoDraftResult = await _gameAcquisitionService.ClaimGame(request, false, true, true);
                if (autoDraftResult.Success)
                {
                    counterPicksAdded++;
                    break;
                }
            }
        }
        else
        {
            return (standardGamesAdded, counterPicksAdded);
        }

        return await AutoDraftForLeague(leagueYear, standardGamesAdded, counterPicksAdded);
    }

    public DraftPhase GetDraftPhase(LeagueYear leagueYear)
    {
        int numberOfStandardGamesToDraft = leagueYear.Options.GamesToDraft * leagueYear.Publishers.Count;
        var allPublisherGames = leagueYear.Publishers.SelectMany(x => x.PublisherGames).ToList();
        int standardGamesDrafted = allPublisherGames.Count(x => !x.CounterPick);
        if (standardGamesDrafted < numberOfStandardGamesToDraft)
        {
            return DraftPhase.StandardGames;
        }

        int numberOfCounterPicksToDraft = leagueYear.Options.CounterPicksToDraft * leagueYear.Publishers.Count;
        int counterPicksDrafted = allPublisherGames.Count(x => x.CounterPick);
        if (counterPicksDrafted < numberOfCounterPicksToDraft)
        {
            return DraftPhase.CounterPicks;
        }

        return DraftPhase.Complete;
    }

    public (int? DraftPosition, int? OverallDraftPosition) GetDraftStatus(DraftPhase draftPhase, LeagueYear leagueYear)
    {
        var nextPublisher = GetNextDraftPublisher(leagueYear);
        if (nextPublisher.HasNoValue)
        {
            return (null, null);
        }

        if (draftPhase.Equals(DraftPhase.StandardGames))
        {
            var publisherPosition = nextPublisher.Value.PublisherGames.Count(x => !x.CounterPick) + 1;
            var overallPosition = leagueYear.Publishers.SelectMany(x => x.PublisherGames).Count(x => !x.CounterPick) + 1;
            return (publisherPosition, overallPosition);
        }
        if (draftPhase.Equals(DraftPhase.CounterPicks))
        {
            var publisherPosition = nextPublisher.Value.PublisherGames.Count(x => x.CounterPick) + 1;
            var overallPosition = leagueYear.Publishers.SelectMany(x => x.PublisherGames).Count(x => x.CounterPick) + 1;
            return (publisherPosition, overallPosition);
        }

        return (null, null);
    }

    public IReadOnlyList<PublisherGame> GetAvailableCounterPicks(LeagueYear leagueYear, Publisher nextDraftingPublisher)
    {
        IReadOnlyList<Publisher> otherPublishers = leagueYear.Publishers.Where(x => x.PublisherID != nextDraftingPublisher.PublisherID).ToList();

        IReadOnlyList<PublisherGame> gamesForYear = leagueYear.Publishers.SelectMany(x => x.PublisherGames).ToList();
        IReadOnlyList<PublisherGame> otherPlayersGames = otherPublishers.SelectMany(x => x.PublisherGames).Where(x => !x.CounterPick).ToList();

        var alreadyCounterPicked = gamesForYear.Where(x => x.CounterPick).ToList();
        List<PublisherGame> availableCounterPicks = new List<PublisherGame>();
        foreach (var otherPlayerGame in otherPlayersGames)
        {
            bool playerHasCounterPick = alreadyCounterPicked.ContainsGame(otherPlayerGame);
            if (playerHasCounterPick)
            {
                continue;
            }

            availableCounterPicks.Add(otherPlayerGame);
        }

        return availableCounterPicks;
    }

    private async Task<bool> CompleteDraft(LeagueYear leagueYear, int standardGamesAdded, int counterPicksAdded)
    {
        var publishers = leagueYear.Publishers;
        int numberOfStandardGamesToDraft = leagueYear.Options.GamesToDraft * publishers.Count;
        int standardGamesDrafted = publishers.SelectMany(x => x.PublisherGames).Count(x => !x.CounterPick);
        standardGamesDrafted += standardGamesAdded;

        if (standardGamesDrafted < numberOfStandardGamesToDraft)
        {
            return false;
        }

        int numberOfCounterPicksToDraft = leagueYear.Options.CounterPicksToDraft * publishers.Count;
        int counterPicksDrafted = publishers.SelectMany(x => x.PublisherGames).Count(x => x.CounterPick);
        counterPicksDrafted += counterPicksAdded;

        if (counterPicksDrafted < numberOfCounterPicksToDraft)
        {
            return false;
        }

        await _fantasyCriticRepo.CompleteDraft(leagueYear);
        return true;
    }

    public Task ResetDraft(LeagueYear leagueYear)
    {
        return _fantasyCriticRepo.ResetDraft(leagueYear);
    }
}
