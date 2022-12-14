using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Domain.Combinations;
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
    private readonly IClock _clock;
    private readonly DiscordPushService _discordPushService;

    public GameAcquisitionService(IFantasyCriticRepo fantasyCriticRepo, IMasterGameRepo masterGameRepo, IClock clock, DiscordPushService discordPushService)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _masterGameRepo = masterGameRepo;
        _clock = clock;
        _discordPushService = discordPushService;
    }

    public async Task<ClaimResult> ClaimGame(ClaimGameDomainRequest request, bool managerAction, bool draft, bool drafting)
    {
        MasterGameYear? masterGameYear = null;
        if (request.MasterGame is not null)
        {
            masterGameYear = new MasterGameYear(request.MasterGame, request.LeagueYear.Year);
        }

        ClaimResult claimResult = GameEligibilityFunctions.CanClaimGame(request, null, null, true, drafting, false, false, _clock.GetToday());
        if (!claimResult.Success)
        {
            return claimResult;
        }

        PublisherGame playerGame = new PublisherGame(request.Publisher.PublisherID, Guid.NewGuid(), request.GameName, _clock.GetCurrentInstant(), request.CounterPick, null, false, null,
            masterGameYear, claimResult.BestSlotNumber!.Value, request.DraftPosition, request.OverallDraftPosition, null, null);

        LeagueAction leagueAction = new LeagueAction(request, _clock.GetCurrentInstant(), managerAction, draft, request.AutoDraft);
        await _fantasyCriticRepo.AddLeagueAction(leagueAction);
        await _discordPushService.SendLeagueActionMessage(leagueAction);
        await _fantasyCriticRepo.AddPublisherGame(playerGame);

        return claimResult;
    }

    public async Task<ClaimResult> AssociateGame(AssociateGameDomainRequest request)
    {
        ClaimResult claimResult = GameEligibilityFunctions.CanAssociateGame(request, _clock.GetToday());

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

        var claimResult = GameEligibilityFunctions.CanClaimGame(claimRequest, nextBidTime, validDropSlot, false, false, partOfSpecialAuction, false, _clock.GetToday());
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
        var claimResult = GameEligibilityFunctions.CanClaimGame(claimRequest, null, null, false, false, false, false, _clock.GetToday());
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
        var dropResult = GameEligibilityFunctions.CanDropGame(dropRequest, leagueYear, publisher, _clock.GetToday(), _clock.GetNextBidTime());
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
        var slotResult = SlotEligibilityFunctions.GetPublisherSlotAcquisitionResult(bid.Publisher, bid.LeagueYear.Options, eligibilityFactors, bid.CounterPick, validDropSlot, false, false);
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
        var distinctBids = bidsToCount.DistinctBy(x => x.MasterGame).OrderBy(x => x.MasterGame.GameName);
        List<PublicBiddingMasterGame> masterGameYears = new List<PublicBiddingMasterGame>();
        foreach (var bid in distinctBids)
        {
            if (specialAuctionGames.Contains(bid.MasterGame))
            {
                continue;
            }

            var masterGameYear = await _masterGameRepo.GetMasterGameYearOrThrow(bid.MasterGame.MasterGameID, leagueYear.Year);
            var claimResult = GameEligibilityFunctions.GetGenericSlotMasterGameErrors(leagueYear, bid.MasterGame, leagueYear.Year, false, currentDate, dateOfPotentialAcquisition,
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

    public async Task<IReadOnlyList<LeagueYearPublicBiddingSet>> GetPublicBiddingGames(int year)
    {
        var leagueYears = await _fantasyCriticRepo.GetLeagueYears(year);
        var activeBidsByLeague = await _fantasyCriticRepo.GetActivePickupBids(year, leagueYears);

        var currentDate = _clock.GetToday();
        var dateOfPotentialAcquisition = _clock.GetNextBidTime().ToEasternDate();

        List<LeagueYearPublicBiddingSet> publicBiddingSets = new List<LeagueYearPublicBiddingSet>();
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

            var distinctBids = bidsToCount.DistinctBy(x => x.MasterGame).OrderBy(x => x.MasterGame.GameName);
            List<PublicBiddingMasterGame> masterGameYears = new List<PublicBiddingMasterGame>();
            foreach (var bid in distinctBids)
            {
                var masterGameYear = await _masterGameRepo.GetMasterGameYearOrThrow(bid.MasterGame.MasterGameID, activeBidsForLeague.Key.Year);
                var claimResult = GameEligibilityFunctions.GetGenericSlotMasterGameErrors(activeBidsForLeague.Key, bid.MasterGame, activeBidsForLeague.Key.Year,
                    false, currentDate, dateOfPotentialAcquisition, bid.CounterPick, false, false, false);
                masterGameYears.Add(new PublicBiddingMasterGame(masterGameYear, bid.CounterPick, claimResult));
            }

            publicBiddingSets.Add(new LeagueYearPublicBiddingSet(activeBidsForLeague.Key, masterGameYears));
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
        var claimErrors = SlotEligibilityFunctions.GetClaimErrorsForLeagueYear(eligibilityFactors);
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
