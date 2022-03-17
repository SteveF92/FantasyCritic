using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Services;

public class GameAcquisitionService
{
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IMasterGameRepo _masterGameRepo;
    private readonly LeagueMemberService _leagueMemberService;
    private readonly IClock _clock;

    public GameAcquisitionService(IFantasyCriticRepo fantasyCriticRepo, IMasterGameRepo masterGameRepo, LeagueMemberService leagueMemberService, IClock clock)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _masterGameRepo = masterGameRepo;
        _leagueMemberService = leagueMemberService;
        _clock = clock;
    }

    public ClaimResult CanClaimGame(ClaimGameDomainRequest request, Instant? nextBidTime, int? validDropSlot, bool watchListing, bool drafting)
    {
        var currentDate = _clock.GetToday();
        var dateOfPotentialAcquisition = currentDate;
        if (nextBidTime.HasValue)
        {
            dateOfPotentialAcquisition = nextBidTime.Value.ToEasternDate();
        }

        var leagueYear = request.LeagueYear;

        List<ClaimError> claimErrors = new List<ClaimError>();

        var basicErrors = GetBasicErrors(leagueYear.League, request.Publisher);
        claimErrors.AddRange(basicErrors);

        if (request.MasterGame.HasValue)
        {
            var masterGameErrors = GetGenericSlotMasterGameErrors(leagueYear, request.MasterGame.Value, leagueYear.Year, false, currentDate,
                dateOfPotentialAcquisition, request.CounterPick, request.CounterPickedGameIsManualWillNotRelease, drafting);
            claimErrors.AddRange(masterGameErrors);
        }

        LeaguePublisherGameSet gameSet = new LeaguePublisherGameSet(request.Publisher.PublisherID, leagueYear.Publishers);
        bool thisPlayerAlreadyHas = gameSet.ThisPlayerStandardGames.ContainsGame(request);
        bool gameAlreadyClaimed = gameSet.OtherPlayerStandardGames.ContainsGame(request);
        if (!request.CounterPick)
        {
            if (gameAlreadyClaimed)
            {
                claimErrors.Add(new ClaimError("Cannot claim a game that someone already has.", false));
            }

            if (thisPlayerAlreadyHas)
            {
                claimErrors.Add(new ClaimError("Cannot claim a game that you already have.", false));
            }
        }

        if (request.CounterPick)
        {
            bool otherPlayerHasCounterPick = gameSet.OtherPlayerCounterPicks.ContainsGame(request);
            if (otherPlayerHasCounterPick)
            {
                claimErrors.Add(new ClaimError("Cannot counter-pick a game that someone else has already counter picked.", false));
            }
            bool thisPlayerHasCounterPick = gameSet.ThisPlayerCounterPicks.ContainsGame(request);
            if (thisPlayerHasCounterPick)
            {
                claimErrors.Add(new ClaimError("You already have that counter pick.", false));
            }

            bool otherPlayerHasDraftGame = gameSet.OtherPlayerStandardGames.ContainsGame(request);
            if (!otherPlayerHasDraftGame)
            {
                claimErrors.Add(new ClaimError("Cannot counter pick a game that no other player is publishing.", false));
            }
        }

        Maybe<MasterGameWithEligibilityFactors> eligibilityFactors = Maybe<MasterGameWithEligibilityFactors>.None;
        if (request.MasterGame.HasValue)
        {
            eligibilityFactors = leagueYear.GetEligibilityFactorsForMasterGame(request.MasterGame.Value, dateOfPotentialAcquisition);
        }

        var slotResult = SlotEligibilityService.GetPublisherSlotAcquisitionResult(request.Publisher, leagueYear.Options, eligibilityFactors, request.CounterPick, validDropSlot, watchListing);
        if (!slotResult.SlotNumber.HasValue)
        {
            claimErrors.AddRange(slotResult.ClaimErrors);
            return new ClaimResult(claimErrors, null);
        }

        var result = new ClaimResult(claimErrors, slotResult.SlotNumber.Value);
        if (result.Overridable && request.ManagerOverride)
        {
            return new ClaimResult(slotResult.SlotNumber.Value);
        }

        return result;
    }

    public DropResult CanDropGame(DropRequest request, LeagueYear leagueYear, Publisher publisher, IEnumerable<Publisher> otherPublishers)
    {
        List<ClaimError> dropErrors = new List<ClaimError>();

        var basicErrors = GetBasicErrors(leagueYear.League, publisher);
        dropErrors.AddRange(basicErrors);

        var currentDate = _clock.GetToday();
        var masterGameErrors = GetGenericSlotMasterGameErrors(leagueYear, request.MasterGame, leagueYear.Year, true, currentDate, currentDate, false, false, false);
        dropErrors.AddRange(masterGameErrors);

        //Drop limits
        var publisherGame = publisher.GetPublisherGame(request.MasterGame);
        if (publisherGame.HasNoValue)
        {
            return new DropResult(Result.Failure("Cannot drop a game that you do not have"));
        }
        if (dropErrors.Any())
        {
            return new DropResult(Result.Failure("Game is no longer eligible for dropping."));
        }
        bool gameWasDrafted = publisherGame.Value.OverallDraftPosition.HasValue;
        if (!gameWasDrafted && leagueYear.Options.DropOnlyDraftGames)
        {
            return new DropResult(Result.Failure("You can only drop games that you drafted due to your league settings."));
        }

        bool gameWasCounterPicked = otherPublishers
            .SelectMany(x => x.PublisherGames)
            .Where(x => x.CounterPick)
            .ContainsGame(request.MasterGame);
        if (gameWasCounterPicked && leagueYear.Options.CounterPicksBlockDrops)
        {
            return new DropResult(Result.Failure("You cannot drop that game because it was counter picked."));
        }

        bool gameWillRelease = publisherGame.Value.WillRelease();
        var dropResult = publisher.CanDropGame(gameWillRelease, leagueYear.Options);
        return new DropResult(dropResult);
    }

    public DropResult CanConditionallyDropGame(PickupBid request, LeagueYear leagueYear, Publisher publisher, IEnumerable<Publisher> otherPublishers, Instant? nextBidTime)
    {
        List<ClaimError> dropErrors = new List<ClaimError>();

        var basicErrors = GetBasicErrors(leagueYear.League, publisher);
        dropErrors.AddRange(basicErrors);

        var currentDate = _clock.GetToday();
        var dateOfPotentialAcquisition = currentDate;
        if (nextBidTime.HasValue)
        {
            dateOfPotentialAcquisition = nextBidTime.Value.ToEasternDate();
        }

        var masterGameErrors = GetGenericSlotMasterGameErrors(leagueYear, request.ConditionalDropPublisherGame.Value.MasterGame.Value.MasterGame, leagueYear.Year,
            true, currentDate, dateOfPotentialAcquisition, false, false, false);
        dropErrors.AddRange(masterGameErrors);

        //Drop limits
        var publisherGame = publisher.GetPublisherGameByPublisherGameID(request.ConditionalDropPublisherGame.Value.PublisherGameID);
        if (publisherGame.HasNoValue)
        {
            return new DropResult(Result.Failure("Cannot drop a game that you do not have"));
        }
        if (dropErrors.Any())
        {
            return new DropResult(Result.Failure("Game is no longer eligible for dropping."));
        }
        bool gameWasDrafted = publisherGame.Value.OverallDraftPosition.HasValue;
        if (!gameWasDrafted && leagueYear.Options.DropOnlyDraftGames)
        {
            return new DropResult(Result.Failure("You can only drop games that you drafted due to your league settings."));
        }

        bool gameWasCounterPicked = otherPublishers
            .SelectMany(x => x.PublisherGames)
            .Where(x => x.CounterPick)
            .ContainsGame(request.ConditionalDropPublisherGame.Value.MasterGame.Value.MasterGame);
        if (gameWasCounterPicked && leagueYear.Options.CounterPicksBlockDrops)
        {
            return new DropResult(Result.Failure("You cannot drop that game because it was counter picked."));
        }

        bool gameWillRelease = publisherGame.Value.WillRelease();
        var dropResult = publisher.CanDropGame(gameWillRelease, leagueYear.Options);
        return new DropResult(dropResult);
    }

    public async Task<ClaimResult> CanAssociateGame(AssociateGameDomainRequest request)
    {
        List<ClaimError> associationErrors = new List<ClaimError>();
        var basicErrors = GetBasicErrors(request.LeagueYear.League, request.Publisher);
        associationErrors.AddRange(basicErrors);
        var leagueYear = request.LeagueYear;

        var currentDate = _clock.GetToday();
        var dateOfPotentialAcquisition = currentDate;

        IReadOnlyList<ClaimError> masterGameErrors = GetGenericSlotMasterGameErrors(leagueYear, request.MasterGame, leagueYear.Year, false, currentDate,
            dateOfPotentialAcquisition, request.PublisherGame.CounterPick, false, false);
        associationErrors.AddRange(masterGameErrors);

        LeaguePublisherGameSet gameSet = new LeaguePublisherGameSet(request.Publisher.PublisherID, request.LeagueYear.Publishers);

        bool thisPlayerAlreadyHas = gameSet.ThisPlayerStandardGames.ContainsGame(request.MasterGame);
        bool gameAlreadyClaimed = gameSet.OtherPlayerStandardGames.ContainsGame(request.MasterGame);
        if (!request.PublisherGame.CounterPick)
        {
            if (gameAlreadyClaimed)
            {
                associationErrors.Add(new ClaimError("Cannot claim a game that someone already has.", false));
            }

            if (thisPlayerAlreadyHas)
            {
                associationErrors.Add(new ClaimError("Cannot claim a game that you already have.", false));
            }
        }

        if (request.PublisherGame.CounterPick)
        {
            bool otherPlayerHasCounterPick = gameSet.OtherPlayerCounterPicks.ContainsGame(request.MasterGame);
            if (otherPlayerHasCounterPick)
            {
                associationErrors.Add(new ClaimError("Cannot counter-pick a game that someone else has already counter picked.", false));
            }
            bool thisPlayerHasCounterPick = gameSet.ThisPlayerCounterPicks.ContainsGame(request.MasterGame);
            if (thisPlayerHasCounterPick)
            {
                associationErrors.Add(new ClaimError("You already have that counter pick.", false));
            }

            bool otherPlayerHasDraftGame = gameSet.OtherPlayerStandardGames.ContainsGame(request.MasterGame);
            if (!otherPlayerHasDraftGame)
            {
                associationErrors.Add(new ClaimError("Cannot counter pick a game that no other player is publishing.", false));
            }
        }

        var result = new ClaimResult(associationErrors, request.PublisherGame.SlotNumber);
        if (result.Overridable && request.ManagerOverride)
        {
            return new ClaimResult(request.PublisherGame.SlotNumber);
        }

        return result;
    }

    private static IReadOnlyList<ClaimError> GetBasicErrors(League league, Publisher publisher)
    {
        List<ClaimError> claimErrors = new List<ClaimError>();

        bool isInLeague = (publisher.LeagueYearKey.LeagueID == league.LeagueID);
        if (!isInLeague)
        {
            claimErrors.Add(new ClaimError("User is not in that league.", false));
        }

        if (!league.Years.Contains(publisher.LeagueYearKey.Year))
        {
            claimErrors.Add(new ClaimError("League is not active for that year.", false));
        }

        return claimErrors;
    }

    private IReadOnlyList<ClaimError> GetGenericSlotMasterGameErrors(LeagueYear leagueYear, MasterGame masterGame, int year, bool dropping,
        LocalDate currentDate, LocalDate dateOfPotentialAcquisition, bool counterPick, bool counterPickedGameIsManualWillNotRelease, bool drafting)
    {
        MasterGameWithEligibilityFactors eligibilityFactors = leagueYear.GetEligibilityFactorsForMasterGame(masterGame, dateOfPotentialAcquisition);
        List<ClaimError> claimErrors = new List<ClaimError>();

        bool manuallyEligible = eligibilityFactors.OverridenEligibility.HasValue && eligibilityFactors.OverridenEligibility.Value;
        bool released = masterGame.IsReleased(currentDate);
        if (released)
        {
            claimErrors.Add(new ClaimError("That game has already been released.", true));
        }

        if (currentDate != dateOfPotentialAcquisition)
        {
            bool releaseBeforeNextBids = masterGame.IsReleased(dateOfPotentialAcquisition);
            if (releaseBeforeNextBids)
            {
                if (!dropping)
                {
                    claimErrors.Add(new ClaimError("That game will release before bids are processed.", true));
                }
                else
                {
                    claimErrors.Add(new ClaimError("That game will release before drops are processed.", true));
                }
            }
        }

        if (released && masterGame.ReleaseDate.HasValue && masterGame.ReleaseDate.Value.Year < year)
        {
            claimErrors.Add(new ClaimError($"That game was released prior to the start of {year}.", false));
        }

        if (counterPick && !drafting && masterGame.DelayContention)
        {
            claimErrors.Add(new ClaimError($"That game is in 'delay contention', and therefore cannot be counter picked.", true));
        }

        bool willRelease = masterGame.MinimumReleaseDate.Year == year && !counterPickedGameIsManualWillNotRelease;
        if (!dropping && !released && !willRelease && !manuallyEligible)
        {
            claimErrors.Add(new ClaimError($"That game is not scheduled to be released in {year}.", true));
        }

        bool hasScore = masterGame.CriticScore.HasValue;
        if (hasScore && !manuallyEligible)
        {
            claimErrors.Add(new ClaimError("That game already has a score.", true));
        }

        return claimErrors;
    }

    public async Task<ClaimResult> ClaimGame(ClaimGameDomainRequest request, bool managerAction, bool draft, bool drafting)
    {
        Maybe<MasterGameYear> masterGameYear = Maybe<MasterGameYear>.None;
        if (request.MasterGame.HasValue)
        {
            masterGameYear = new MasterGameYear(request.MasterGame.Value, request.LeagueYear.Year);
        }

        LeagueYear leagueYear = request.LeagueYear;
        ClaimResult claimResult = CanClaimGame(request, null, null, false, drafting);
        if (!claimResult.Success)
        {
            return claimResult;
        }

        PublisherGame playerGame = new PublisherGame(request.Publisher.PublisherID, Guid.NewGuid(), request.GameName, _clock.GetCurrentInstant(), request.CounterPick, null, false, null,
            masterGameYear, claimResult.BestSlotNumber.Value, request.DraftPosition, request.OverallDraftPosition, null, null);

        LeagueAction leagueAction = new LeagueAction(request, _clock.GetCurrentInstant(), managerAction, draft, request.AutoDraft);
        await _fantasyCriticRepo.AddLeagueAction(leagueAction);
        await _fantasyCriticRepo.AddPublisherGame(playerGame);

        return claimResult;
    }

    public async Task<ClaimResult> AssociateGame(AssociateGameDomainRequest request)
    {
        ClaimResult claimResult = await CanAssociateGame(request);

        if (!claimResult.Success)
        {
            return claimResult;
        }

        LeagueAction leagueAction = new LeagueAction(request, _clock.GetCurrentInstant());
        await _fantasyCriticRepo.AddLeagueAction(leagueAction);
        await _fantasyCriticRepo.AssociatePublisherGame(request.Publisher, request.PublisherGame, request.MasterGame);

        return claimResult;
    }

    public async Task<ClaimResult> MakePickupBid(LeagueYear leagueYear, Publisher publisher, MasterGame masterGame, Maybe<PublisherGame> conditionalDropPublisherGame, bool counterPick, uint bidAmount)
    {
        if (bidAmount < leagueYear.Options.MinimumBidAmount)
        {
            return new ClaimResult(new List<ClaimError>() { new ClaimError("That bid does not meet the league's minimum bid.", false) }, null);
        }

        if (bidAmount > publisher.Budget)
        {
            return new ClaimResult(new List<ClaimError>() { new ClaimError("You do not have enough budget to make that bid.", false) }, null);
        }

        IReadOnlyList<PickupBid> pickupBids = await _fantasyCriticRepo.GetActivePickupBids(publisher);
        bool alreadyBidFor = pickupBids.Select(x => x.MasterGame.MasterGameID).Contains(masterGame.MasterGameID);
        if (alreadyBidFor)
        {
            return new ClaimResult(new List<ClaimError>() { new ClaimError("You cannot have two active bids for the same game.", false) }, null);
        }

        bool counterPickedGameIsManualWillNotRelease = false;
        if (counterPick)
        {
            var gameBeingCounterPickedOptions = leagueYear.Publishers.Select(x => x.GetPublisherGame(masterGame))
                .Where(x => x.HasValue && !x.Value.CounterPick).ToList();

            if (gameBeingCounterPickedOptions.Count != 1)
            {
                throw new Exception($"Something very strange has happened with bid processing for publisher: {publisher.PublisherID}");
            }

            counterPickedGameIsManualWillNotRelease = gameBeingCounterPickedOptions.Single().Value.ManualWillNotRelease;
        }

        var claimRequest = new ClaimGameDomainRequest(leagueYear, publisher, masterGame.GameName, counterPick,
            counterPickedGameIsManualWillNotRelease, false, false, masterGame, null, null);

        Instant nextBidTime = _clock.GetNextBidTime();

        int? validDropSlot = null;
        if (conditionalDropPublisherGame.HasValue)
        {
            if (counterPick)
            {
                return new ClaimResult("Cannot make a counter pick bid with a conditional drop.", null);
            }

            var dropResult = await MakeDropRequest(leagueYear, publisher, conditionalDropPublisherGame.Value, true);
            if (dropResult.Result.IsFailure)
            {
                return new ClaimResult(dropResult.Result.Error, null);
            }

            validDropSlot = conditionalDropPublisherGame.Value.SlotNumber;
        }

        var claimResult = CanClaimGame(claimRequest, nextBidTime, validDropSlot, false, false);
        if (!claimResult.Success)
        {
            return claimResult;
        }

        var nextPriority = pickupBids.Count + 1;
        PickupBid currentBid = new PickupBid(Guid.NewGuid(), publisher, leagueYear, masterGame, conditionalDropPublisherGame, counterPick,
            bidAmount, nextPriority, _clock.GetCurrentInstant(), null, null, Maybe<string>.None, null);
        await _fantasyCriticRepo.CreatePickupBid(currentBid);

        return claimResult;
    }

    public async Task<ClaimResult> QueueGame(LeagueYear leagueYear, Publisher publisher, MasterGame masterGame)
    {
        IReadOnlyList<QueuedGame> queuedGames = await _fantasyCriticRepo.GetQueuedGames(publisher);
        bool alreadyQueued = queuedGames.Select(x => x.MasterGame.MasterGameID).Contains(masterGame.MasterGameID);
        if (alreadyQueued)
        {
            return new ClaimResult(new List<ClaimError>() { new ClaimError("You already have that game queued.", false) }, null);
        }

        var claimRequest = new ClaimGameDomainRequest(leagueYear, publisher, masterGame.GameName, false, false, false, false, masterGame, null, null);
        var claimResult = CanClaimGame(claimRequest, null, null, true, false);
        if (!claimResult.Success)
        {
            return claimResult;
        }

        var nextRank = queuedGames.Count + 1;
        QueuedGame queuedGame = new QueuedGame(publisher, masterGame, nextRank);
        await _fantasyCriticRepo.QueueGame(queuedGame);

        return claimResult;
    }

    public Task RemoveQueuedGame(QueuedGame queuedGame)
    {
        return _fantasyCriticRepo.RemoveQueuedGame(queuedGame);
    }

    public async Task<DropResult> MakeDropRequest(LeagueYear leagueYear, Publisher publisher, PublisherGame publisherGame, bool justCheck)
    {
        if (publisherGame.CounterPick)
        {
            return new DropResult(Result.Failure("You can't drop a counter pick."));
        }

        if (publisherGame.MasterGame.HasNoValue)
        {
            return new DropResult(Result.Failure("You can't drop a game that is not linked to a master game. Please see the FAQ section on dropping games."));
        }

        MasterGame masterGame = publisherGame.MasterGame.Value.MasterGame;
        IReadOnlyList<DropRequest> dropRequests = await _fantasyCriticRepo.GetActiveDropRequests(publisher);
        bool alreadyDropping = dropRequests.Select(x => x.MasterGame.MasterGameID).Contains(masterGame.MasterGameID);
        if (alreadyDropping)
        {
            return new DropResult(Result.Failure("You cannot have two active drop requests for the same game."));
        }

        DropRequest dropRequest = new DropRequest(Guid.NewGuid(), publisher, leagueYear, masterGame, _clock.GetCurrentInstant(), null, null);
        var publishersInLeague = leagueYear.Publishers;
        var otherPublishers = publishersInLeague.Except(new List<Publisher>() { publisher });

        var dropResult = CanDropGame(dropRequest, leagueYear, publisher, otherPublishers);
        if (dropResult.Result.IsFailure)
        {
            return dropResult;
        }

        if (!justCheck)
        {
            await _fantasyCriticRepo.CreateDropRequest(dropRequest);
        }

        return dropResult;
    }

    public Task<IReadOnlyList<PickupBid>> GetActiveAcquisitionBids(Publisher publisher)
    {
        return _fantasyCriticRepo.GetActivePickupBids(publisher);
    }

    public Task<IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>>> GetActiveAcquisitionBids(SupportedYear supportedYear, IReadOnlyList<LeagueYear> allLeagueYears, IReadOnlyList<Publisher> allPublishersForYear)
    {
        return _fantasyCriticRepo.GetActivePickupBids(supportedYear.Year, allLeagueYears, allPublishersForYear);
    }

    public Task<IReadOnlyList<DropRequest>> GetActiveDropRequests(Publisher publisher)
    {
        return _fantasyCriticRepo.GetActiveDropRequests(publisher);
    }

    public Task<IReadOnlyDictionary<LeagueYear, IReadOnlyList<DropRequest>>> GetActiveDropRequests(SupportedYear supportedYear, IReadOnlyList<LeagueYear> allLeagueYears, IReadOnlyList<Publisher> allPublishersForYear)
    {
        return _fantasyCriticRepo.GetActiveDropRequests(supportedYear.Year, allLeagueYears, allPublishersForYear);
    }

    public Task<Maybe<PickupBid>> GetPickupBid(Guid bidID)
    {
        return _fantasyCriticRepo.GetPickupBid(bidID);
    }

    public Task<Maybe<DropRequest>> GetDropRequest(Guid dropRequest)
    {
        return _fantasyCriticRepo.GetDropRequest(dropRequest);
    }

    public async Task<ClaimResult> EditPickupBid(PickupBid bid, Maybe<PublisherGame> conditionalDropPublisherGame, uint bidAmount)
    {
        if (bid.Successful != null)
        {
            return new ClaimResult(new List<ClaimError>() { new ClaimError("Bid has already been processed", false) }, null);
        }

        if (bidAmount < bid.LeagueYear.Options.MinimumBidAmount)
        {
            return new ClaimResult(new List<ClaimError>() { new ClaimError("That bid does not meet the league's minimum bid.", false) }, null);
        }

        if (bidAmount > bid.Publisher.Budget)
        {
            return new ClaimResult(new List<ClaimError>() { new ClaimError("You do not have enough budget to make that bid.", false) }, null);
        }

        int? validDropSlot = null;
        if (conditionalDropPublisherGame.HasValue)
        {
            if (bid.CounterPick)
            {
                return new ClaimResult("Cannot make a counter pick bid with a conditional drop.", null);
            }

            var dropResult = await MakeDropRequest(bid.LeagueYear, bid.Publisher, conditionalDropPublisherGame.Value, true);
            if (dropResult.Result.IsFailure)
            {
                return new ClaimResult(dropResult.Result.Error, null);
            }

            validDropSlot = conditionalDropPublisherGame.Value.SlotNumber;
        }

        await _fantasyCriticRepo.EditPickupBid(bid, conditionalDropPublisherGame, bidAmount);

        var currentDate = _clock.GetToday();
        MasterGameWithEligibilityFactors eligibilityFactors = bid.LeagueYear.GetEligibilityFactorsForMasterGame(bid.MasterGame, currentDate);
        var slotResult = SlotEligibilityService.GetPublisherSlotAcquisitionResult(bid.Publisher, bid.LeagueYear.Options, eligibilityFactors, bid.CounterPick, validDropSlot, false);
        if (!slotResult.SlotNumber.HasValue)
        {
            return new ClaimResult(slotResult.ClaimErrors, null);
        }

        return new ClaimResult(slotResult.SlotNumber.Value);
    }

    public async Task<Result> RemovePickupBid(PickupBid bid)
    {
        if (bid.Successful != null)
        {
            return Result.Failure("Bid has already been processed");
        }

        await _fantasyCriticRepo.RemovePickupBid(bid);
        return Result.Success();
    }

    public async Task<Result> RemoveDropRequest(DropRequest dropRequest)
    {
        if (dropRequest.Successful != null)
        {
            return Result.Failure("Drop request has already been processed");
        }

        await _fantasyCriticRepo.RemoveDropRequest(dropRequest);
        return Result.Success();
    }

    public async Task<Maybe<IReadOnlyList<PublicBiddingMasterGame>>> GetPublicBiddingGames(LeagueYear leagueYear)
    {
        var isInPublicWindow = IsInPublicBiddingWindow(leagueYear);
        if (!isInPublicWindow)
        {
            return Maybe<IReadOnlyList<PublicBiddingMasterGame>>.None;
        }

        var currentDate = _clock.GetToday();
        var dateOfPotentialAcquisition = _clock.GetNextBidTime().ToEasternDate();

        var activeBidsForLeague = await _fantasyCriticRepo.GetActivePickupBids(leagueYear);

        var bidsToCount = activeBidsForLeague;
        if (leagueYear.Options.PickupSystem.Equals(PickupSystem.SemiPublicBiddingSecretCounterPicks))
        {
            bidsToCount = bidsToCount.Where(x => !x.CounterPick).ToList();
        }

        var distinctBids = bidsToCount.DistinctBy(x => x.MasterGame);
        List<PublicBiddingMasterGame> masterGameYears = new List<PublicBiddingMasterGame>();
        foreach (var bid in distinctBids)
        {
            var masterGameYear = await _masterGameRepo.GetMasterGameYear(bid.MasterGame.MasterGameID, leagueYear.Year);
            var claimResult = GetGenericSlotMasterGameErrors(leagueYear, bid.MasterGame, leagueYear.Year, false, currentDate, dateOfPotentialAcquisition, bid.CounterPick, false, false);
            masterGameYears.Add(new PublicBiddingMasterGame(masterGameYear.Value, bid.CounterPick, claimResult));
        }

        return masterGameYears;
    }

    public bool PublicBidIsValid(LeagueYear leagueYear, MasterGame masterGame, bool counterPick, Maybe<IReadOnlyList<PublicBiddingMasterGame>> publicBiddingMasterGames)
    {
        if (publicBiddingMasterGames.HasNoValue)
        {
            return true;
        }

        if (counterPick && leagueYear.Options.PickupSystem.Equals(PickupSystem.SemiPublicBiddingSecretCounterPicks))
        {
            return true;
        }

        return publicBiddingMasterGames.Value.Select(x => x.MasterGameYear.MasterGame).Contains(masterGame);
    }

    public bool CanCancelBid(LeagueYear leagueYear, bool counterPick)
    {
        bool inPublicBidWindow = IsInPublicBiddingWindow(leagueYear);
        if (!inPublicBidWindow)
        {
            return true;
        }

        if (leagueYear.Options.PickupSystem.Equals(PickupSystem.SemiPublicBidding))
        {
            return false;
        }

        if (counterPick && leagueYear.Options.PickupSystem.Equals(PickupSystem.SemiPublicBiddingSecretCounterPicks))
        {
            return true;
        }

        return false;
    }

    public async Task<IReadOnlyList<PublicBiddingSet>> GetPublicBiddingGames(int year)
    {
        var leagueYears = await _fantasyCriticRepo.GetLeagueYears(year);
        var activeBidsByLeague = await _fantasyCriticRepo.GetActivePickupBids(year, leagueYears);

        var currentDate = _clock.GetToday();
        var dateOfPotentialAcquisition = _clock.GetNextBidTime().ToEasternDate();

        List<PublicBiddingSet> publicBiddingSets = new List<PublicBiddingSet>();
        foreach (var activeBidsForLeague in activeBidsByLeague)
        {
            if (!activeBidsForLeague.Key.PlayStatus.DraftFinished || !activeBidsForLeague.Key.Options.PickupSystem.HasPublicBiddingWindow)
            {
                continue;
            }

            var bidsToCount = activeBidsForLeague.Value;
            if (activeBidsForLeague.Key.Options.PickupSystem.Equals(PickupSystem.SemiPublicBiddingSecretCounterPicks))
            {
                bidsToCount = bidsToCount.Where(x => !x.CounterPick).ToList();
            }

            var distinctBids = bidsToCount.DistinctBy(x => x.MasterGame);
            List<PublicBiddingMasterGame> masterGameYears = new List<PublicBiddingMasterGame>();
            foreach (var bid in distinctBids)
            {
                var masterGameYear = await _masterGameRepo.GetMasterGameYear(bid.MasterGame.MasterGameID, activeBidsForLeague.Key.Year);
                var claimResult = GetGenericSlotMasterGameErrors(activeBidsForLeague.Key, bid.MasterGame, activeBidsForLeague.Key.Year, false, currentDate, dateOfPotentialAcquisition, bid.CounterPick, false, false);
                masterGameYears.Add(new PublicBiddingMasterGame(masterGameYear.Value, bid.CounterPick, claimResult));
            }

            publicBiddingSets.Add(new PublicBiddingSet(activeBidsForLeague.Key, masterGameYears));
        }

        return publicBiddingSets;
    }

    public bool IsInPublicBiddingWindow(LeagueYear leagueYear)
    {
        if (!leagueYear.Options.PickupSystem.HasPublicBiddingWindow)
        {
            return false;
        }

        if (!leagueYear.PlayStatus.DraftFinished)
        {
            return false;
        }

        var currentTime = _clock.GetCurrentInstant();
        var previousBidTime = _clock.GetPreviousBidTime();
        LocalDate previousBidDate = previousBidTime.InZone(TimeExtensions.EasternTimeZone).LocalDateTime.Date;
        var publicBidDate = previousBidDate.Next(TimeExtensions.PublicBiddingRevealDay);
        var publicBidDateTime = (publicBidDate + TimeExtensions.PublicBiddingRevealTime)
            .InZoneStrictly(TimeExtensions.EasternTimeZone)
            .ToInstant();

        return currentTime > publicBidDateTime;
    }
}
