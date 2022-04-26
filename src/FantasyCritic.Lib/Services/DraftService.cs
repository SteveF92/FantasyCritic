using FantasyCritic.Lib.Domain.Draft;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Extensions;
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

    public async Task<bool> StartDraft(LeagueYear leagueYear)
    {
        await _fantasyCriticRepo.StartDraft(leagueYear);
        var updatedLeagueYear = await _fantasyCriticRepo.GetLeagueYearOrThrow(leagueYear.League, leagueYear.Year);
        var autoDraftResult = await AutoDraftForLeague(updatedLeagueYear, 0, 0);
        var draftComplete = await CompleteDraft(updatedLeagueYear, autoDraftResult.StandardGamesAdded, autoDraftResult.CounterPicksAdded);
        return draftComplete;
    }

    public async Task SetDraftPause(LeagueYear leagueYear, bool pause)
    {
        await _fantasyCriticRepo.SetDraftPause(leagueYear, pause);
        var updatedLeagueYear = await _fantasyCriticRepo.GetLeagueYearOrThrow(leagueYear.League, leagueYear.Year);
        if (!pause)
        {
            await AutoDraftForLeague(updatedLeagueYear, 0, 0);
        }
    }

    public async Task UndoLastDraftAction(LeagueYear leagueYear)
    {
        var publisherGames = leagueYear.Publishers.SelectMany(x => x.PublisherGames);
        var newestGame = publisherGames.WhereMax(x => x.Timestamp).First();

        var publisher = leagueYear.Publishers.Single(x => x.PublisherGames.Select(y => y.PublisherGameID).Contains(newestGame.PublisherGameID));

        await _publisherService.RemovePublisherGame(leagueYear, publisher, newestGame);
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
        var today = _clock.GetToday();
        var updatedLeagueYear = await _fantasyCriticRepo.GetLeagueYearOrThrow(leagueYear.League, leagueYear.Year);
        var draftStatus = DraftFunctions.GetDraftStatus(updatedLeagueYear);
        if (draftStatus is null)
        {
            return (standardGamesAdded, counterPicksAdded);
        }

        var nextPublisher = draftStatus.NextDraftPublisher;
        if (!nextPublisher.AutoDraft)
        {
            return (standardGamesAdded, counterPicksAdded);
        }

        if (draftStatus.DraftPhase.Equals(DraftPhase.Complete))
        {
            return (standardGamesAdded, counterPicksAdded);
        }
        if (draftStatus.DraftPhase.Equals(DraftPhase.StandardGames))
        {
            var publisherWatchList = await _publisherService.GetQueuedGames(nextPublisher);
            var availableGames = await _gameSearchingService.GetTopAvailableGames(updatedLeagueYear, nextPublisher);
            var availableGamesEligibleInRemainingSlots = new List<PossibleMasterGameYear>();
            var openSlots = nextPublisher.GetPublisherSlots(updatedLeagueYear.Options).Where(x => !x.CounterPick && x.PublisherGame is null).ToList();
            foreach (var availableGame in availableGames)
            {
                foreach (var slot in openSlots)
                {
                    var eligibilityFactors = updatedLeagueYear.GetEligibilityFactorsForMasterGame(availableGame.MasterGame.MasterGame, today);
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
                var request = new ClaimGameDomainRequest(updatedLeagueYear, nextPublisher, possibleGame.GameName, false, false, false, true, possibleGame, draftStatus.DraftPosition, draftStatus.OverallDraftPosition);
                var autoDraftResult = await _gameAcquisitionService.ClaimGame(request, false, true, true);
                if (autoDraftResult.Success)
                {
                    standardGamesAdded++;
                    break;
                }
            }
        }
        else if (draftStatus.DraftPhase.Equals(DraftPhase.CounterPicks))
        {
            var otherPublisherGames = updatedLeagueYear.GetAllPublishersExcept(nextPublisher)
                .SelectMany(x => x.PublisherGames)
                .Where(x => !x.CounterPick)
                .Where(x => x.MasterGame is not null);
            var possibleGames = otherPublisherGames
                .Select(x => x.MasterGame!)
                .Where(x => x.AdjustedPercentCounterPick.HasValue)
                .OrderByDescending(x => x.AdjustedPercentCounterPick);
            foreach (var possibleGame in possibleGames)
            {
                var request = new ClaimGameDomainRequest(updatedLeagueYear, nextPublisher, possibleGame.MasterGame.GameName, true, false, false, true, possibleGame.MasterGame,
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

        return await AutoDraftForLeague(updatedLeagueYear, standardGamesAdded, counterPicksAdded);
    }

    public IReadOnlyList<PublisherGame> GetAvailableCounterPicks(LeagueYear leagueYear, Publisher publisherMakingCounterPick)
    {
        IReadOnlyList<Publisher> otherPublishers = leagueYear.GetAllPublishersExcept(publisherMakingCounterPick);
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
        return _fantasyCriticRepo.ResetDraft(leagueYear, _clock.GetCurrentInstant());
    }
}
