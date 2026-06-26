using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Domain.Draft;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using Serilog;

namespace FantasyCritic.Lib.Services;

public class DraftService
{
    private static readonly ILogger _logger = Log.ForContext<DraftService>();

    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly ICombinedDataRepo _combinedDataRepo;
    private readonly IClock _clock;
    private readonly GameAcquisitionService _gameAcquisitionService;
    private readonly PublisherService _publisherService;
    private readonly GameSearchingService _gameSearchingService;
    private readonly DiscordPushService _discordPushService;

    public DraftService(GameAcquisitionService gameAcquisitionService, PublisherService publisherService,
        IFantasyCriticRepo fantasyCriticRepo, ICombinedDataRepo combinedDataRepo, GameSearchingService gameSearchingService, IClock clock,
        DiscordPushService discordPushService)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _combinedDataRepo = combinedDataRepo;
        _clock = clock;
        _publisherService = publisherService;
        _gameAcquisitionService = gameAcquisitionService;
        _gameSearchingService = gameSearchingService;
        _discordPushService = discordPushService;
    }

    public async Task<StartDraftResult> StartDraft(LeagueYear leagueYear)
    {
        if (leagueYear.PendingDraft is null)
        {
            throw new Exception($"No pending draft found for league: {leagueYear.Key}");
        }
        await _fantasyCriticRepo.StartDraft(leagueYear, leagueYear.PendingDraft);
        var updatedLeagueYear = await _combinedDataRepo.GetLeagueYearOrThrow(leagueYear.League.LeagueID, leagueYear.Year);
        var autoDraftResult = await AutoDraftForLeague(updatedLeagueYear, 0, 0);
        var draftComplete = await CompleteDraft(updatedLeagueYear, autoDraftResult.StandardGamesAdded, autoDraftResult.CounterPicksAdded);
        return new StartDraftResult(autoDraftResult, draftComplete);
    }

    public async Task SetDraftPause(LeagueYear leagueYear, bool pause)
    {
        if (leagueYear.ActiveDraft is null)
        {
            throw new Exception($"No active draft found for league: {leagueYear.Key}");
        }
        await _fantasyCriticRepo.SetDraftPause(leagueYear, leagueYear.ActiveDraft, pause);
        var updatedLeagueYear = await _combinedDataRepo.GetLeagueYearOrThrow(leagueYear.League.LeagueID, leagueYear.Year);
        if (!pause)
        {
            await AutoDraftForLeague(updatedLeagueYear, 0, 0);
        }
    }

    public async Task<Result> UndoLastDraftAction(LeagueYear leagueYear)
    {
        if (leagueYear.ActiveDraft is null)
        {
            throw new Exception($"No active draft found for league: {leagueYear.Key}");
        }

        var publisherGames = leagueYear.Publishers.SelectMany(x => x.PublisherGames).ToList();
        if (!leagueYear.ActiveDraft.PlayStatus.PlayStarted || leagueYear.ActiveDraft.PlayStatus.DraftFinished)
        {
            return Result.Failure("Cannot undo a draft game when the draft is not active.");
        }

        if (!publisherGames.Any())
        {
            return Result.Failure("Cannot undo a draft game when no games have been drafted yet.");
        }

        var counterPicks = publisherGames.Where(x => x.CounterPick).ToList();

        PublisherGame publisherGameToUndo;
        if (!counterPicks.Any())
        {
            publisherGameToUndo = publisherGames.MaxBy(x => x.OverallDraftPosition)!;
        }
        else
        {
            publisherGameToUndo = counterPicks.MaxBy(x => x.OverallDraftPosition)!;
        }

        var publisher = leagueYear.Publishers.Single(x => x.PublisherID == publisherGameToUndo.PublisherID);
        await _publisherService.RemovePublisherGame(leagueYear, publisher, publisherGameToUndo);

        return Result.Success();
    }

    public async Task<Result> SetDraftOrder(LeagueYear leagueYear, Guid draftID, DraftOrderType draftOrderType, IReadOnlyList<KeyValuePair<Publisher, int>> draftPositions)
    {
        var draft = leagueYear.Drafts.SingleOrDefault(d => d.DraftID == draftID);
        if (draft is null)
        {
            return Result.Failure("Draft not found.");
        }

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

        string draftOrderDescription = string.Join("\n", draftPositions.Select(x => $"• {x.Value}: {x.Key.GetPublisherAndUserDisplayName()}"));
        string actionDescription = $"{draftOrderType.ActionDescription} \n {draftOrderDescription}";
        LeagueManagerAction draftSetAction = new LeagueManagerAction(leagueYear.Key, _clock.GetCurrentInstant(), "Set Draft Order", actionDescription);
        await _discordPushService.SendLeagueActionMessage(draftSetAction);

        await _fantasyCriticRepo.SetDraftOrder(draftPositions, draft, draftSetAction);
        return Result.Success();
    }

    public async Task<DraftResult> DraftGame(ClaimGameDomainRequest request, LeagueDraft draft, bool managerAction, bool allowIneligibleSlot)
    {
        var result = await _gameAcquisitionService.ClaimGame(request, managerAction, draft.DraftID, allowIneligibleSlot);
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
        return new DraftResult(result, autoDraftResult, draftComplete);
    }

    public async Task<bool> RunAutoDraftAndCheckIfComplete(LeagueYear leagueYear)
    {
        if (leagueYear.ActiveDraft is null)
        {
            return false;
        }

        var autoDraftResult = await AutoDraftForLeague(leagueYear, 0, 0);
        var draftComplete = await CompleteDraft(leagueYear, autoDraftResult.StandardGamesAdded, autoDraftResult.CounterPicksAdded);
        return draftComplete;
    }

    private async Task<AutoDraftResult> AutoDraftForLeague(LeagueYear leagueYear, int standardGamesAdded, int counterPicksAdded)
    {
        int depth = 0;
        while (true)
        {
            _logger.Debug($"Autodrafting for league: {leagueYear.League} at depth: {depth}");
            var today = _clock.GetToday();
            var updatedLeagueYear = await _combinedDataRepo.GetLeagueYearOrThrow(leagueYear.League.LeagueID, leagueYear.Year);
            var draftStatus = DraftFunctions.GetDraftStatus(updatedLeagueYear);
            if (draftStatus is null)
            {
                return new AutoDraftResult(updatedLeagueYear, standardGamesAdded, counterPicksAdded);
            }

            var nextPublisher = draftStatus.NextDraftPublisher;

            if (draftStatus.DraftPhase.Equals(DraftPhase.Complete))
            {
                return new AutoDraftResult(updatedLeagueYear, standardGamesAdded, counterPicksAdded);
            }

            // Auto-skip publishers who have no open slots in the active draft (e.g. filled via bids between drafts).
            var activeDraft = updatedLeagueYear.ActiveDraft;
            if (activeDraft is null)
            {
                throw new Exception($"Draft is not active for league: {leagueYear.Key}");
            }

            bool isCounterPickPhase = draftStatus.DraftPhase.Equals(DraftPhase.CounterPicks);
            int gamesInActiveDraft = nextPublisher.PublisherGames.Count(g =>
                g.DraftID == activeDraft.DraftID && g.CounterPick == isCounterPickPhase);
            int slotsInActiveDraft = isCounterPickPhase ? activeDraft.CounterPicksToDraft : activeDraft.GamesToDraft;
            if (gamesInActiveDraft >= slotsInActiveDraft)
            {
                string slotType = isCounterPickPhase ? "counter-pick" : "standard game";
                string description = $"{nextPublisher.GetPublisherAndUserDisplayName()} was skipped (no open {slotType} slots in {activeDraft.Name}).";
                var skipAction = new LeagueManagerAction(updatedLeagueYear.Key, _clock.GetCurrentInstant(), "SkippedDraftTurn", description);
                await _fantasyCriticRepo.AddLeagueManagerAction(skipAction);
                depth++;
                continue;
            }

            if (nextPublisher.AutoDraftMode.Equals(AutoDraftMode.Off))
            {
                return new AutoDraftResult(updatedLeagueYear, standardGamesAdded, counterPicksAdded);
            }

            if (draftStatus.DraftPhase.Equals(DraftPhase.StandardGames))
            {
                var publisherWatchList = await _publisherService.GetQueuedGames(nextPublisher);
                var availableGames = await _gameSearchingService.GetTopAvailableGames(updatedLeagueYear, nextPublisher);
                var availableGamesEligibleInRemainingSlots = new List<PossibleMasterGameYear>();
                var openSlots = nextPublisher.GetPublisherSlots(updatedLeagueYear).Where(x => !x.CounterPick && x.PublisherGame is null).ToList();
                foreach (var availableGame in availableGames)
                {
                    foreach (var slot in openSlots)
                    {
                        var eligibilityFactors = updatedLeagueYear.GetEligibilityFactorsForMasterGame(availableGame.MasterGame.MasterGame, today);
                        var claimErrors = SlotEligibilityFunctions.GetClaimErrorsForSlot(slot, eligibilityFactors);
                        if (!claimErrors.Any())
                        {
                            availableGamesEligibleInRemainingSlots.Add(availableGame);
                            break;
                        }
                    }
                }

                var watchlistGames = publisherWatchList.OrderBy(x => x.Rank).Select(x => x.MasterGame);
                var gamesToTake = (nextPublisher.AutoDraftSettings.OnlyDraftFromWatchlist
                    ? watchlistGames
                    : watchlistGames.Concat(availableGamesEligibleInRemainingSlots.Select(x => x.MasterGame.MasterGame))).ToList();

                bool addedAGame = false;
                foreach (var possibleGame in gamesToTake)
                {
                    var request = new ClaimGameDomainRequest(updatedLeagueYear, nextPublisher, possibleGame.GameName, false, false, false, true, possibleGame, draftStatus.DraftPosition, draftStatus.OverallDraftPosition);
                    var autoDraftResult = await _gameAcquisitionService.ClaimGame(request, false, draftStatus.Draft.DraftID, false);
                    if (autoDraftResult.Success)
                    {
                        standardGamesAdded++;
                        addedAGame = true;
                        break;
                    }
                }

                if (!addedAGame)
                {
                    return new AutoDraftResult(updatedLeagueYear, standardGamesAdded, counterPicksAdded);
                }
            }
            else if (draftStatus.DraftPhase.Equals(DraftPhase.CounterPicks))
            {
                if (nextPublisher.AutoDraftMode.Equals(AutoDraftMode.StandardGamesOnly))
                {
                    return new AutoDraftResult(updatedLeagueYear, standardGamesAdded, counterPicksAdded);
                }

                var availableCounterPicks = GetAvailableCounterPicks(updatedLeagueYear, nextPublisher)
                    .Where(x => x.MasterGame is not null)
                    .OrderByDescending(x => x.MasterGame!.AdjustedPercentCounterPick ?? 0);

                bool addedAGame = false;
                foreach (var publisherGame in availableCounterPicks)
                {
                    var masterGame = publisherGame.MasterGame!.MasterGame;
                    var request = new ClaimGameDomainRequest(updatedLeagueYear, nextPublisher, masterGame.GameName, true, false, false, true, masterGame, draftStatus.DraftPosition, draftStatus.OverallDraftPosition);
                    var autoDraftResult = await _gameAcquisitionService.ClaimGame(request, false, draftStatus.Draft.DraftID, false);
                    if (autoDraftResult.Success)
                    {
                        counterPicksAdded++;
                        addedAGame = true;
                        break;
                    }
                }

                if (!addedAGame)
                {
                    return new AutoDraftResult(updatedLeagueYear, standardGamesAdded, counterPicksAdded);
                }
            }
            else
            {
                return new AutoDraftResult(updatedLeagueYear, standardGamesAdded, counterPicksAdded);
            }

            leagueYear = updatedLeagueYear;
            depth++;
        }
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
        var activeDraft = leagueYear.ActiveDraft;
        if (activeDraft is null)
        {
            return false;
        }

        var publishers = leagueYear.Publishers;
        int numberOfStandardGamesToDraft = activeDraft.GamesToDraft * publishers.Count;
        int standardGamesDrafted = publishers.SelectMany(x => x.PublisherGames).Count(x => !x.CounterPick && x.DraftID == activeDraft.DraftID);
        standardGamesDrafted += standardGamesAdded;

        if (standardGamesDrafted < numberOfStandardGamesToDraft)
        {
            return false;
        }

        int numberOfCounterPicksToDraft = activeDraft.CounterPicksToDraft * publishers.Count;
        int counterPicksDrafted = publishers.SelectMany(x => x.PublisherGames).Count(x => x.CounterPick && x.DraftID == activeDraft.DraftID);
        counterPicksDrafted += counterPicksAdded;

        if (counterPicksDrafted < numberOfCounterPicksToDraft)
        {
            return false;
        }

        await _fantasyCriticRepo.CompleteDraft(leagueYear, activeDraft);
        return true;
    }

    public Task ResetDraft(LeagueYear leagueYear)
    {
        if (leagueYear.ActiveDraft is null)
        {
            throw new Exception($"No active draft found for league: {leagueYear.Key}");
        }
        return _fantasyCriticRepo.ResetDraft(leagueYear, leagueYear.ActiveDraft, _clock.GetCurrentInstant());
    }

    public async Task<Result> CreateDraft(LeagueYear leagueYear, CreateLeagueDraftParameters domainRequest)
    {
        if (leagueYear.IsAnyDraftInProgress)
        {
            return Result.Failure("Cannot create a draft while one is in progress.");
        }

        if (domainRequest.NewSpecialGameSlots.Count > domainRequest.AdditionalStandardGames)
        {
            return Result.Failure("You must add at least as many 'Additional Standard Games' as new special slots. " +
                                  "Otherwise the new special slots would convert existing standard slots into special slots.");
        }

        int nextDraftNumber = leagueYear.Drafts.Max(d => d.DraftNumber) + 1;
        var newPublisherDraftInfo = new List<PublisherDraftInfo>();
        foreach (var publisher in leagueYear.Publishers)
        {
            var startingDraftPosition = publisher.LastDraftInfo.DraftPosition;
            var draftInfoForPublisher = new PublisherDraftInfo(Guid.NewGuid(), nextDraftNumber, publisher.PublisherID, startingDraftPosition);
            newPublisherDraftInfo.Add(draftInfoForPublisher);
        }

        var draft = new LeagueDraft(Guid.NewGuid(), leagueYear.Key, nextDraftNumber, domainRequest.Name, domainRequest.ScheduledDate,
            domainRequest.GamesToDraft, domainRequest.CounterPicksToDraft, false, PlayStatus.NotStartedDraft, newPublisherDraftInfo, null);

        var timestamp = _clock.GetCurrentInstant();
        string description = $"Scheduled new draft: {domainRequest.Name}";
        var newDraftAction = new LeagueManagerAction(leagueYear.Key, timestamp, "Create Draft", description);

        NewDraftLeagueSettingsChanges? settingsToChange = leagueYear.Options.WithNewDraftOptions(domainRequest, timestamp);
        await _fantasyCriticRepo.CreateLeagueDraft(draft, newDraftAction, settingsToChange);

        return Result.Success();
    }

    public async Task<Result> EditDraft(LeagueYear leagueYear, Guid draftID, EditLeagueDraftParameters domainRequest)
    {
        var draft = leagueYear.Drafts.SingleOrDefault(d => d.DraftID == draftID);
        if (draft is null)
        {
            return Result.Failure("Draft not found.");
        }

        bool isStarted = draft.PlayStatus.PlayStarted;

        // Name is always editable; other fields only if not started
        if (isStarted && (
                (domainRequest.ScheduledDate != draft.ScheduledDate) ||
                (domainRequest.GamesToDraft != draft.GamesToDraft) ||
                (domainRequest.CounterPicksToDraft != draft.CounterPicksToDraft)
            ))
        {
            return Result.Failure("Cannot edit draft settings once a draft has started. Reset the draft first.");
        }

        var updatedDraft = new LeagueDraft(draft.DraftID, draft.LeagueYearKey, draft.DraftNumber, domainRequest.Name,
            domainRequest.ScheduledDate,
            domainRequest.GamesToDraft,
            domainRequest.CounterPicksToDraft,
            draft.DraftOrderSet, draft.PlayStatus, draft.PublisherDraftInfo, draft.DraftStartedTimestamp);

        var differences = updatedDraft.GetDifferences(draft);
        if (!differences.HasChanges)
        {
            return Result.Success();
        }

        var timestamp = _clock.GetCurrentInstant();
        var managerAction = new LeagueManagerAction(leagueYear.Key, timestamp, "Edit Draft", differences.ToString());
        await _fantasyCriticRepo.EditLeagueDraft(updatedDraft, managerAction);
        return Result.Success();
    }

    public async Task<Result> DeleteDraft(LeagueYear leagueYear, Guid draftID)
    {
        var draft = leagueYear.Drafts.SingleOrDefault(d => d.DraftID == draftID);
        if (draft is null)
        {
            return Result.Failure("Draft not found.");
        }

        if (draft.DraftNumber == 1)
        {
            return Result.Failure("Cannot delete the first draft. Every league must have at least one draft.");
        }

        if (!draft.PlayStatus.Equals(PlayStatus.NotStartedDraft))
        {
            return Result.Failure("Cannot delete a draft that has already started. Reset the draft first.");
        }

        var timestamp = _clock.GetCurrentInstant();
        string description = $"Deleted draft: {draft.Name}";
        var managerAction = new LeagueManagerAction(leagueYear.Key, timestamp, "Delete Draft", description);
        await _fantasyCriticRepo.DeleteLeagueDraft(draft, managerAction);
        return Result.Success();
    }
}
