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

    public ClaimResult CanClaimGame(ClaimGameDomainRequest request, Instant? nextBidTime, int? validDropSlot, bool acquiringNow, bool drafting, bool partOfSpecialAuction)
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

        if (request.MasterGame is not null)
        {
            var masterGameErrors = GetGenericSlotMasterGameErrors(leagueYear, request.MasterGame, leagueYear.Year, false, currentDate,
                dateOfPotentialAcquisition, request.CounterPick, request.CounterPickedGameIsManualWillNotRelease, drafting, partOfSpecialAuction);
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

        MasterGameWithEligibilityFactors? eligibilityFactors = null;
        if (request.MasterGame is not null)
        {
            eligibilityFactors = leagueYear.GetEligibilityFactorsForMasterGame(request.MasterGame, dateOfPotentialAcquisition);
        }

        var slotResult = SlotEligibilityService.GetPublisherSlotAcquisitionResult(request.Publisher, leagueYear.Options, eligibilityFactors, request.CounterPick, validDropSlot, acquiringNow);
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

    public DropResult CanDropGame(DropRequest request, LeagueYear leagueYear, Publisher publisher)
    {
        List<ClaimError> dropErrors = new List<ClaimError>();

        var basicErrors = GetBasicErrors(leagueYear.League, publisher);
        dropErrors.AddRange(basicErrors);

        var currentDate = _clock.GetToday();
        var masterGameErrors = GetGenericSlotMasterGameErrors(leagueYear, request.MasterGame, leagueYear.Year, true, currentDate, currentDate, false, false, false, false);
        dropErrors.AddRange(masterGameErrors);

        //Drop limits
        var publisherGame = publisher.GetPublisherGame(request.MasterGame);
        if (publisherGame is null)
        {
            return new DropResult(Result.Failure("Cannot drop a game that you do not have"));
        }
        if (dropErrors.Any())
        {
            return new DropResult(Result.Failure("Game is no longer eligible for dropping."));
        }
        bool gameWasDrafted = publisherGame.OverallDraftPosition.HasValue;
        if (!gameWasDrafted && leagueYear.Options.DropOnlyDraftGames)
        {
            return new DropResult(Result.Failure("You can only drop games that you drafted due to your league settings."));
        }

        var otherPublishers = leagueYear.GetAllPublishersExcept(publisher);
        bool gameWasCounterPicked = otherPublishers
            .SelectMany(x => x.PublisherGames)
            .Where(x => x.CounterPick)
            .ContainsGame(request.MasterGame);
        if (gameWasCounterPicked && leagueYear.Options.CounterPicksBlockDrops)
        {
            return new DropResult(Result.Failure("You cannot drop that game because it was counter picked."));
        }

        bool gameWillRelease = publisherGame.WillRelease();
        var dropResult = publisher.CanDropGame(gameWillRelease, leagueYear.Options, false);
        return new DropResult(dropResult);
    }

    public DropResult CanConditionallyDropGame(PickupBid request, LeagueYear leagueYear, Publisher publisher, Instant? nextBidTime)
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

        if (request.ConditionalDropPublisherGame?.MasterGame is null)
        {
            throw new Exception($"Invalid conditional drop for bid: {request.BidID}");
        }

        var masterGameErrors = GetGenericSlotMasterGameErrors(leagueYear, request.ConditionalDropPublisherGame.MasterGame.MasterGame, leagueYear.Year,
            true, currentDate, dateOfPotentialAcquisition, false, false, false, false);
        dropErrors.AddRange(masterGameErrors);

        //Drop limits
        var publisherGame = publisher.GetPublisherGameByPublisherGameID(request.ConditionalDropPublisherGame.PublisherGameID);
        if (publisherGame is null)
        {
            return new DropResult(Result.Failure("Cannot drop a game that you do not have"));
        }
        if (dropErrors.Any())
        {
            return new DropResult(Result.Failure("Game is no longer eligible for dropping."));
        }
        bool gameWasDrafted = publisherGame.OverallDraftPosition.HasValue;
        if (!gameWasDrafted && leagueYear.Options.DropOnlyDraftGames)
        {
            return new DropResult(Result.Failure("You can only drop games that you drafted due to your league settings."));
        }

        var otherPublishers = leagueYear.GetAllPublishersExcept(publisher);
        bool gameWasCounterPicked = otherPublishers
            .SelectMany(x => x.PublisherGames)
            .Where(x => x.CounterPick)
            .ContainsGame(request.ConditionalDropPublisherGame.MasterGame.MasterGame);
        if (gameWasCounterPicked && leagueYear.Options.CounterPicksBlockDrops)
        {
            return new DropResult(Result.Failure("You cannot drop that game because it was counter picked."));
        }

        bool gameWillRelease = publisherGame.WillRelease();
        var dropResult = publisher.CanDropGame(gameWillRelease, leagueYear.Options, false);
        return new DropResult(dropResult);
    }

    public ClaimResult CanAssociateGame(AssociateGameDomainRequest request)
    {
        List<ClaimError> associationErrors = new List<ClaimError>();
        var basicErrors = GetBasicErrors(request.LeagueYear.League, request.Publisher);
        associationErrors.AddRange(basicErrors);
        var leagueYear = request.LeagueYear;

        var currentDate = _clock.GetToday();
        var dateOfPotentialAcquisition = currentDate;

        IReadOnlyList<ClaimError> masterGameErrors = GetGenericSlotMasterGameErrors(leagueYear, request.MasterGame, leagueYear.Year, false, currentDate,
            dateOfPotentialAcquisition, request.PublisherGame.CounterPick, false, false, false);
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
        LocalDate currentDate, LocalDate dateOfPotentialAcquisition, bool counterPick, bool counterPickedGameIsManualWillNotRelease,
        bool drafting, bool partOfSpecialAuction)
    {
        MasterGameWithEligibilityFactors eligibilityFactors = leagueYear.GetEligibilityFactorsForMasterGame(masterGame, dateOfPotentialAcquisition);
        List<ClaimError> claimErrors = new List<ClaimError>();

        bool manuallyEligible = eligibilityFactors.OverridenEligibility.HasValue && eligibilityFactors.OverridenEligibility.Value;
        bool released = masterGame.IsReleased(currentDate);
        if (released && !partOfSpecialAuction)
        {
            claimErrors.Add(new ClaimError("That game has already been released.", true));
        }

        if (currentDate != dateOfPotentialAcquisition && !partOfSpecialAuction)
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

        bool willRelease = masterGame.MinimumReleaseDate.Year == year && !counterPickedGameIsManualWillNotRelease;
        if (!dropping && !released && !willRelease && !manuallyEligible)
        {
            claimErrors.Add(new ClaimError($"That game is not scheduled to be released in {year}.", true));
        }

        if (counterPick && !drafting)
        {
            if (masterGame.DelayContention)
            {
                claimErrors.Add(new ClaimError("That game is in 'delay contention', and therefore cannot be counter picked.", false));
            }

            bool confirmedWillRelease = masterGame.ReleaseDate.HasValue && masterGame.ReleaseDate.Value.Year == year;
            bool acquiringAfterDeadline = dateOfPotentialAcquisition >= leagueYear.CounterPickDeadline;
            if (!confirmedWillRelease && acquiringAfterDeadline && willRelease)
            {
                claimErrors.Add(new ClaimError($"That game does not have a confirmed release date in {year}, and the 'counter pick deadline' has already passed (or will have by the time bids process).", false));
            }
        }

        bool hasScore = masterGame.CriticScore.HasValue;
        if (hasScore && !manuallyEligible && !partOfSpecialAuction)
        {
            claimErrors.Add(new ClaimError("That game already has a score.", true));
        }

        if (!hasScore && masterGame.HasAnyReviews && !manuallyEligible && !partOfSpecialAuction)
        {
            claimErrors.Add(new ClaimError("That game already has reviews.", true));
        }

        return claimErrors;
    }

    public async Task<ClaimResult> ClaimGame(ClaimGameDomainRequest request, bool managerAction, bool draft, bool drafting)
    {
        MasterGameYear? masterGameYear = null;
        if (request.MasterGame is not null)
        {
            masterGameYear = new MasterGameYear(request.MasterGame, request.LeagueYear.Year);
        }

        ClaimResult claimResult = CanClaimGame(request, null, null, true, drafting, false);
        if (!claimResult.Success)
        {
            return claimResult;
        }

        PublisherGame playerGame = new PublisherGame(request.Publisher.PublisherID, Guid.NewGuid(), request.GameName, _clock.GetCurrentInstant(), request.CounterPick, null, false, null,
            masterGameYear, claimResult.BestSlotNumber!.Value, request.DraftPosition, request.OverallDraftPosition, null, null);

        LeagueAction leagueAction = new LeagueAction(request, _clock.GetCurrentInstant(), managerAction, draft, request.AutoDraft);
        await _fantasyCriticRepo.AddLeagueAction(leagueAction);
        await _fantasyCriticRepo.AddPublisherGame(playerGame);

        return claimResult;
    }

    public async Task<ClaimResult> AssociateGame(AssociateGameDomainRequest request)
    {
        ClaimResult claimResult = CanAssociateGame(request);

        if (!claimResult.Success)
        {
            return claimResult;
        }

        LeagueAction leagueAction = new LeagueAction(request, _clock.GetCurrentInstant());
        await _fantasyCriticRepo.AddLeagueAction(leagueAction);
        await _fantasyCriticRepo.AssociatePublisherGame(request.Publisher, request.PublisherGame, request.MasterGame);

        return claimResult;
    }

    public async Task<ClaimResult> MakePickupBid(LeagueYear leagueYear, Publisher publisher, MasterGame masterGame,
        PublisherGame? conditionalDropPublisherGame, bool counterPick, uint bidAmount)
    {
        if (bidAmount < leagueYear.Options.MinimumBidAmount)
        {
            return new ClaimResult(new List<ClaimError>() { new ClaimError("That bid does not meet the league's minimum bid.", false) }, null);
        }

        if (bidAmount > publisher.Budget)
        {
            return new ClaimResult(new List<ClaimError>() { new ClaimError("You do not have enough budget to make that bid.", false) }, null);
        }

        IReadOnlyList<PickupBid> pickupBids = await _fantasyCriticRepo.GetActivePickupBids(leagueYear, publisher);
        bool alreadyBidFor = pickupBids.Select(x => x.MasterGame.MasterGameID).Contains(masterGame.MasterGameID);
        if (alreadyBidFor)
        {
            return new ClaimResult(new List<ClaimError>() { new ClaimError("You cannot have two active bids for the same game.", false) }, null);
        }

        bool counterPickedGameIsManualWillNotRelease = false;
        if (counterPick)
        {
            var gameBeingCounterPickedOptions = leagueYear.Publishers.Select(x => x.GetPublisherGame(masterGame))
                .Where(x => x is not null && !x.CounterPick).SelectNotNull().ToList();

            if (gameBeingCounterPickedOptions.Count != 1)
            {
                throw new Exception($"Something very strange has happened with bid processing for publisher: {publisher.PublisherID}");
            }

            counterPickedGameIsManualWillNotRelease = gameBeingCounterPickedOptions.Single().ManualWillNotRelease;
        }

        var claimRequest = new ClaimGameDomainRequest(leagueYear, publisher, masterGame.GameName, counterPick,
            counterPickedGameIsManualWillNotRelease, false, false, masterGame, null, null);

        Instant nextBidTime = _clock.GetNextBidTime();
        var specialAuctions = await _fantasyCriticRepo.GetSpecialAuctions(leagueYear);
        var activeSpecialAuctionForGame = specialAuctions.SingleOrDefault(x => !x.Processed && x.MasterGameYear.MasterGame.Equals(masterGame));
        bool partOfSpecialAuction = activeSpecialAuctionForGame is not null;

        int? validDropSlot = null;
        if (conditionalDropPublisherGame is not null)
        {
            if (counterPick)
            {
                return new ClaimResult("Cannot make a counter pick bid with a conditional drop.");
            }

            var dropResult = await MakeDropRequest(leagueYear, publisher, conditionalDropPublisherGame, true);
            if (dropResult.Result.IsFailure)
            {
                return new ClaimResult(dropResult.Result.Error);
            }

            validDropSlot = conditionalDropPublisherGame.SlotNumber;
        }

        var claimResult = CanClaimGame(claimRequest, nextBidTime, validDropSlot, false, false, partOfSpecialAuction);
        if (!claimResult.Success)
        {
            return claimResult;
        }

        var nextPriority = pickupBids.Count + 1;
        PickupBid currentBid = new PickupBid(Guid.NewGuid(), publisher, leagueYear, masterGame, conditionalDropPublisherGame, counterPick,
            bidAmount, nextPriority, _clock.GetCurrentInstant(), null, null, null, null);
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
        var claimResult = CanClaimGame(claimRequest, null, null, false, false, false);
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

        if (publisherGame.MasterGame is null)
        {
            return new DropResult(Result.Failure("You can't drop a game that is not linked to a master game. Please see the FAQ section on dropping games."));
        }

        MasterGame masterGame = publisherGame.MasterGame.MasterGame;
        IReadOnlyList<DropRequest> dropRequests = await _fantasyCriticRepo.GetActiveDropRequests(leagueYear, publisher);
        bool alreadyDropping = dropRequests.Select(x => x.MasterGame.MasterGameID).Contains(masterGame.MasterGameID);
        if (alreadyDropping)
        {
            return new DropResult(Result.Failure("You cannot have two active drop requests for the same game."));
        }

        DropRequest dropRequest = new DropRequest(Guid.NewGuid(), publisher, leagueYear, masterGame, _clock.GetCurrentInstant(), null, null);
        var dropResult = CanDropGame(dropRequest, leagueYear, publisher);
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

    public async Task<DropResult> UseSuperDrop(LeagueYear leagueYear, Publisher publisher, PublisherGame publisherGame)
    {
        if (publisher.SuperDropsAvailable < 1)
        {
            return new DropResult(Result.Failure("You do not have any super drops available"));
        }

        var now = _clock.GetCurrentInstant();
        var action = new LeagueAction(publisher, now, "Super Dropped Game", $"Super dropped game '{publisherGame.GameName}'", false);
        var formerPublisherGame = publisherGame.GetFormerPublisherGame(now, "Super dropped by player");
        await _fantasyCriticRepo.SuperDropGame(leagueYear, publisher, publisherGame, formerPublisherGame, action);
        return new DropResult(Result.Success());
    }

    public Task<IReadOnlyList<PickupBid>> GetActiveAcquisitionBids(LeagueYear leagueYear, Publisher publisher)
    {
        return _fantasyCriticRepo.GetActivePickupBids(leagueYear, publisher);
    }

    public Task<IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>>> GetActiveAcquisitionBids(SupportedYear supportedYear, IReadOnlyList<LeagueYear> allLeagueYears)
    {
        return _fantasyCriticRepo.GetActivePickupBids(supportedYear.Year, allLeagueYears);
    }

    public Task<IReadOnlyList<DropRequest>> GetActiveDropRequests(LeagueYear leagueYear, Publisher publisher)
    {
        return _fantasyCriticRepo.GetActiveDropRequests(leagueYear, publisher);
    }

    public Task<IReadOnlyDictionary<LeagueYear, IReadOnlyList<DropRequest>>> GetActiveDropRequests(SupportedYear supportedYear, IReadOnlyList<LeagueYear> allLeagueYears)
    {
        return _fantasyCriticRepo.GetActiveDropRequests(supportedYear.Year, allLeagueYears);
    }

    public Task<PickupBid?> GetPickupBid(Guid bidID)
    {
        return _fantasyCriticRepo.GetPickupBid(bidID);
    }

    public Task<DropRequest?> GetDropRequest(Guid dropRequest)
    {
        return _fantasyCriticRepo.GetDropRequest(dropRequest);
    }

    public async Task<ClaimResult> EditPickupBid(PickupBid bid, PublisherGame? conditionalDropPublisherGame, uint bidAmount)
    {
        if (bid.Successful != null)
        {
            return new ClaimResult(new ClaimError("Bid has already been processed", false));
        }

        if (bidAmount < bid.LeagueYear.Options.MinimumBidAmount)
        {
            return new ClaimResult(new ClaimError("That bid does not meet the league's minimum bid.", false));
        }

        if (bidAmount > bid.Publisher.Budget)
        {
            return new ClaimResult(new ClaimError("You do not have enough budget to make that bid.", false));
        }

        var now = _clock.GetCurrentInstant();
        var activeSpecialAuctionForGame = await GetActiveSpecialAuctionForGame(bid.LeagueYear, bid.MasterGame);
        if (activeSpecialAuctionForGame is not null && activeSpecialAuctionForGame.IsLocked(now))
        {
            return new ClaimResult(new ClaimError("Can't edit a bid after the special auction has locked.", false));
        }

        int? validDropSlot = null;
        if (conditionalDropPublisherGame is not null)
        {
            if (bid.CounterPick)
            {
                return new ClaimResult("Cannot make a counter pick bid with a conditional drop.");
            }

            var dropResult = await MakeDropRequest(bid.LeagueYear, bid.Publisher, conditionalDropPublisherGame, true);
            if (dropResult.Result.IsFailure)
            {
                return new ClaimResult(dropResult.Result.Error);
            }

            validDropSlot = conditionalDropPublisherGame.SlotNumber;
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
        bool canCancelBid = CanCancelBid(bid.LeagueYear, bid.CounterPick);
        if (!canCancelBid)
        {
            return Result.Failure("Can't cancel a bid when in the public bidding window.");
        }

        var now = _clock.GetCurrentInstant();
        var activeSpecialAuctionForGame = await GetActiveSpecialAuctionForGame(bid.LeagueYear, bid.MasterGame);
        if (activeSpecialAuctionForGame is not null && activeSpecialAuctionForGame.IsLocked(now))
        {
            return Result.Failure("Can't cancel a bid after the special auction has locked.");
        }

        if (bid.Successful != null)
        {
            return Result.Failure("Bid has already been processed");
        }

        await _fantasyCriticRepo.RemovePickupBid(bid);
        return Result.Success();
    }

    private bool CanCancelBid(LeagueYear leagueYear, bool counterPick)
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

    public async Task<Result> RemoveDropRequest(DropRequest dropRequest)
    {
        if (dropRequest.Successful != null)
        {
            return Result.Failure("Drop request has already been processed");
        }

        await _fantasyCriticRepo.RemoveDropRequest(dropRequest);
        return Result.Success();
    }

    public async Task<PublicBiddingSet?> GetPublicBiddingGames(LeagueYear leagueYear, IReadOnlyList<SpecialAuction> activeSpecialAuctions)
    {
        var isInPublicWindow = IsInPublicBiddingWindow(leagueYear);
        if (!isInPublicWindow)
        {
            return null;
        }

        var currentDate = _clock.GetToday();
        var dateOfPotentialAcquisition = _clock.GetNextBidTime().ToEasternDate();

        var activeBidsForLeague = await _fantasyCriticRepo.GetActivePickupBids(leagueYear);
        var bidsToCount = activeBidsForLeague;
        if (leagueYear.Options.PickupSystem.Equals(PickupSystem.SemiPublicBiddingSecretCounterPicks))
        {
            bidsToCount = bidsToCount.Where(x => !x.CounterPick).ToList();
        }

        var specialAuctionGames = activeSpecialAuctions.Select(x => x.MasterGameYear.MasterGame).ToHashSet();
        var distinctBids = bidsToCount.DistinctBy(x => x.MasterGame);
        List<PublicBiddingMasterGame> masterGameYears = new List<PublicBiddingMasterGame>();
        foreach (var bid in distinctBids)
        {
            if (specialAuctionGames.Contains(bid.MasterGame))
            {
                continue;
            }

            var masterGameYear = await _masterGameRepo.GetMasterGameYearOrThrow(bid.MasterGame.MasterGameID, leagueYear.Year);
            var claimResult = GetGenericSlotMasterGameErrors(leagueYear, bid.MasterGame, leagueYear.Year, false, currentDate, dateOfPotentialAcquisition,
                bid.CounterPick, false, false, false);
            masterGameYears.Add(new PublicBiddingMasterGame(masterGameYear, bid.CounterPick, claimResult));
        }

        var publicBidTime = GetCurrentWeekPublicBidTime();
        return new PublicBiddingSet(masterGameYears, publicBidTime);
    }

    public bool PublicBidIsValid(LeagueYear leagueYear, MasterGame masterGame, bool counterPick, IReadOnlyList<PublicBiddingMasterGame>? publicBiddingMasterGames, IEnumerable<SpecialAuction> activeSpecialAuctions)
    {
        if (publicBiddingMasterGames is null)
        {
            return true;
        }

        if (counterPick && leagueYear.Options.PickupSystem.Equals(PickupSystem.SemiPublicBiddingSecretCounterPicks))
        {
            return true;
        }

        if (activeSpecialAuctions.Select(x => x.MasterGameYear.MasterGame).Contains(masterGame))
        {
            return true;
        }

        return publicBiddingMasterGames.Select(x => x.MasterGameYear.MasterGame).Contains(masterGame);
    }

    public async Task<IReadOnlyList<EmailPublicBiddingSet>> GetPublicBiddingGames(int year)
    {
        var leagueYears = await _fantasyCriticRepo.GetLeagueYears(year);
        var activeBidsByLeague = await _fantasyCriticRepo.GetActivePickupBids(year, leagueYears);

        var currentDate = _clock.GetToday();
        var dateOfPotentialAcquisition = _clock.GetNextBidTime().ToEasternDate();

        List<EmailPublicBiddingSet> publicBiddingSets = new List<EmailPublicBiddingSet>();
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
                var masterGameYear = await _masterGameRepo.GetMasterGameYearOrThrow(bid.MasterGame.MasterGameID, activeBidsForLeague.Key.Year);
                var claimResult = GetGenericSlotMasterGameErrors(activeBidsForLeague.Key, bid.MasterGame, activeBidsForLeague.Key.Year, false, currentDate, dateOfPotentialAcquisition,
                    bid.CounterPick, false, false, false);
                masterGameYears.Add(new PublicBiddingMasterGame(masterGameYear, bid.CounterPick, claimResult));
            }

            publicBiddingSets.Add(new EmailPublicBiddingSet(activeBidsForLeague.Key, masterGameYears));
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
        var publicBidDateTime = GetCurrentWeekPublicBidTime();

        return currentTime > publicBidDateTime;
    }

    public Instant GetCurrentWeekPublicBidTime()
    {
        var previousBidTime = _clock.GetPreviousBidTime();
        LocalDate previousBidDate = previousBidTime.InZone(TimeExtensions.EasternTimeZone).LocalDateTime.Date;
        var publicBidDate = previousBidDate.Next(TimeExtensions.PublicBiddingRevealDay);
        var publicBidDateTime = (publicBidDate + TimeExtensions.PublicBiddingRevealTime)
            .InZoneStrictly(TimeExtensions.EasternTimeZone)
            .ToInstant();
        return publicBidDateTime;
    }

    public async Task<Result> CreateSpecialAuction(LeagueYear leagueYear, MasterGameYear masterGameYear, Instant scheduledEndTime)
    {
        var now = _clock.GetCurrentInstant();
        var today = now.ToEasternDate();
        var masterGame = masterGameYear.MasterGame;

        var nycEndDate = scheduledEndTime.ToEasternDate();
        if (masterGame.ReleaseDate != today)
        {
            if (masterGame.IsReleased(today))
            {
                return Result.Failure("That game is already released.");
            }

            if (masterGame.IsReleased(nycEndDate))
            {
                return Result.Failure("That game will be released before the end time you specified.");
            }
        }
        else
        {
            if (nycEndDate != today)
            {
                return Result.Failure("That game will be released before the end time you specified.");
            }
        }
        
        var nextBidTime = _clock.GetNextBidTime();
        if (scheduledEndTime > nextBidTime)
        {
            return Result.Failure("The end time must be before the next time that bids process.");
        }

        var closeToNextBidTime = nextBidTime.Minus(Duration.FromHours(1));
        if (scheduledEndTime > closeToNextBidTime)
        {
            return Result.Failure("The end time must be at least an hour before the next time that bids process.");
        }

        var oneHourAway = now.Plus(Duration.FromHours(1));
        if (scheduledEndTime < oneHourAway)
        {
            return Result.Failure("The end time must be at least one hour from now.");
        }

        var allCurrentPublisherGames = leagueYear.Publishers
            .SelectMany(x => x.PublisherGames)
            .Where(x => !x.CounterPick && x.MasterGame is not null)
            .Select(x => x.MasterGame!.MasterGame)
            .Distinct()
            .ToHashSet();

        if (allCurrentPublisherGames.Contains(masterGame))
        {
            return Result.Failure("A player in the league already has that game.");
        }

        var existingSpecialAuction = await GetActiveSpecialAuctionForGame(leagueYear, masterGame);
        if (existingSpecialAuction is not null)
        {
            return Result.Failure("There is already a special auction for that game.");
        }

        MasterGameWithEligibilityFactors eligibilityFactors = leagueYear.GetEligibilityFactorsForMasterGame(masterGame, nycEndDate);
        var claimErrors = SlotEligibilityService.GetClaimErrorsForLeagueYear(eligibilityFactors);
        if (claimErrors.Any())
        {
            string joinedString = string.Join("\n", claimErrors.Select(x => $"• {x.Error}"));
            return Result.Failure(joinedString);
        }

        var specialAuction = new SpecialAuction(Guid.NewGuid(), leagueYear.Key, masterGameYear, now, scheduledEndTime, false);

        var managerPublisher = leagueYear.GetManagerPublisherOrThrow();
        string actionDescription = $"Created special auction for '{masterGame.GameName}'.";
        LeagueAction action = new LeagueAction(managerPublisher, now, "Created Special Auction", actionDescription, true);

        await _fantasyCriticRepo.CreateSpecialAuction(specialAuction, action);

        return Result.Success();
    }

    public async Task<IReadOnlyList<SpecialAuction>> GetActiveSpecialAuctionsForLeague(LeagueYear leagueYear)
    {
        var allSpecialAuctions = await _fantasyCriticRepo.GetSpecialAuctions(leagueYear);
        var activeSpecialAuctions = allSpecialAuctions.Where(x => !x.Processed).ToList();
        return activeSpecialAuctions;
    }

    public Task<SpecialAuction?> GetActiveSpecialAuctionForGame(LeagueYear leagueYear, MasterGame masterGame)
    {
        return GetActiveSpecialAuctionForGame(leagueYear, masterGame.MasterGameID);
    }

    public async Task<SpecialAuction?> GetActiveSpecialAuctionForGame(LeagueYear leagueYear, Guid masterGameID)
    {
        var activeSpecialAuctions = await GetActiveSpecialAuctionsForLeague(leagueYear);
        return activeSpecialAuctions.SingleOrDefault(x => x.MasterGameYear.MasterGame.MasterGameID.Equals(masterGameID));
    }

    public async Task<Result> CancelSpecialAuction(LeagueYear leagueYear, SpecialAuction specialAuction)
    {
        var now = _clock.GetCurrentInstant();
        if (specialAuction.Processed || specialAuction.IsLocked(now))
        {
            return Result.Failure("That special auction has already ended.");
        }

        var cutoff = specialAuction.ScheduledEndTime.Minus(Duration.FromMinutes(10));
        if (now > cutoff)
        {
            return Result.Failure("You can't cancel a special auction within 10 minutes of it ending.");
        }

        var managerPublisher = leagueYear.GetManagerPublisherOrThrow();
        string actionDescription = $"Cancelled special auction for '{specialAuction.MasterGameYear.MasterGame.GameName}'.";
        LeagueAction action = new LeagueAction(managerPublisher, now, "Cancelled Special Auction", actionDescription, true);

        await _fantasyCriticRepo.CancelSpecialAuction(specialAuction, action);
        return Result.Success();
    }
}
