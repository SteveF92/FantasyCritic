using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Utilities;
using Serilog;

namespace FantasyCritic.Lib.BusinessLogicFunctions;

public class ActionProcessor
{
    private static readonly ILogger _logger = Log.ForContext<ActionProcessor>();

    private readonly SystemWideValues _systemWideValues;
    private readonly Instant _processingTime;
    private readonly LocalDate _currentDate;
    private readonly IReadOnlyDictionary<Guid, MasterGameYear> _masterGameYearDictionary;

    public ActionProcessor(SystemWideValues systemWideValues, Instant processingTime, LocalDate currentDate, IReadOnlyDictionary<Guid, MasterGameYear> masterGameYearDictionary)
    {
        _systemWideValues = systemWideValues;
        _processingTime = processingTime;
        _currentDate = currentDate;
        _masterGameYearDictionary = masterGameYearDictionary;
    }

    public FinalizedActionProcessingResults ProcessActions(IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> allActiveBids,
        IReadOnlyDictionary<LeagueYear, IReadOnlyList<DropRequest>> allActiveDrops, IEnumerable<Publisher> publishers)
    {
        var bidsWithFixedPriorities = FixBidPriorities(allActiveBids);

        var publisherStateSet = new PublisherStateSet(publishers);
        var flatBids = bidsWithFixedPriorities.SelectMany(x => x.Value);
        var invalidBids = flatBids.Where(x => x.CounterPick && x.ConditionalDropPublisherGame is not null);
        if (invalidBids.Any())
        {
            throw new Exception("There are counter pick bids with conditional drops.");
        }

        string processName = $"Drop/Bid Processing ({_processingTime.ToEasternDate()})";
        Guid processSetID = Guid.NewGuid();
        if (!bidsWithFixedPriorities.Any() && !allActiveDrops.Any())
        {
            var emptyResults = ActionProcessingResults.GetEmptyResultsSet(publisherStateSet);
            return new FinalizedActionProcessingResults(processSetID, _processingTime, processName, emptyResults, new List<SpecialAuction>());
        }

        ActionProcessingResults dropResults = ProcessDrops(allActiveDrops, publisherStateSet);
        if (!bidsWithFixedPriorities.Any())
        {
            return new FinalizedActionProcessingResults(processSetID, _processingTime, processName, dropResults, new List<SpecialAuction>());
        }

        _logger.Information("Starting real pickup bid processing.");
        ActionProcessingResults bidResults = ProcessPickups(bidsWithFixedPriorities, dropResults);
        return new FinalizedActionProcessingResults(processSetID, _processingTime, processName, bidResults, new List<SpecialAuction>());
    }

    private static IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> FixBidPriorities(IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> allActiveBids)
    {
        //This was added after I discovered a bug that allowed for a publisher to have multiple bids with the same priority.

        var newDictionary = new Dictionary<LeagueYear, List<PickupBid>>();
        foreach (var leagueGroup in allActiveBids)
        {
            newDictionary[leagueGroup.Key] = new List<PickupBid>();
            var groupedByPublisher = leagueGroup.Value.GroupBy(x => x.Publisher);
            foreach (var publisherGroup in groupedByPublisher)
            {
                var bidsOrder = publisherGroup.OrderBy(x => x.Priority).ThenByDescending(x => x.BidAmount)
                    .ThenBy(x => x.Timestamp).Select((bid, index) => (bid, newPriority: index + 1)).ToList();
                var fixedPriorities = bidsOrder.Select(x => x.bid.WithNewPriority(x.newPriority)).ToList();
                newDictionary[leagueGroup.Key].AddRange(fixedPriorities);
            }
        }

        return newDictionary.SealDictionary();
    }

    public FinalizedActionProcessingResults ProcessSpecialAuctions(IReadOnlyList<LeagueYearSpecialAuctionSet> leagueYearSpecialAuctions)
    {
        var flatSpecialAuctions = leagueYearSpecialAuctions.SelectMany(x => x.SpecialAuctionsWithBids.Select(y => y.SpecialAuction)).ToList();
        var flatBids = leagueYearSpecialAuctions.SelectMany(x => x.SpecialAuctionsWithBids.SelectMany(y => y.Bids));
        var invalidBids = flatBids.Where(x => x.CounterPick && x.ConditionalDropPublisherGame is not null);
        if (invalidBids.Any())
        {
            throw new Exception("There are counter pick bids with conditional drops.");
        }

        string processName = $"Special Auction Processing ({_processingTime})";
        Guid processSetID = Guid.NewGuid();

        ActionProcessingResults bidResults = ProcessSpecialAuctionsInternal(leagueYearSpecialAuctions);
        return new FinalizedActionProcessingResults(processSetID, _processingTime, processName, bidResults, flatSpecialAuctions);
    }

    private ActionProcessingResults ProcessDrops(IReadOnlyDictionary<LeagueYear, IReadOnlyList<DropRequest>> allDropRequests, PublisherStateSet publisherStateSet)
    {
        List<FormerPublisherGame> gamesToDelete = new List<FormerPublisherGame>();
        List<LeagueAction> leagueActions = new List<LeagueAction>();
        List<DropRequest> successDrops = new List<DropRequest>();
        List<DropRequest> failedDrops = new List<DropRequest>();

        foreach (var leagueYearGroup in allDropRequests)
        {
            foreach (var dropRequest in leagueYearGroup.Value)
            {
                var affectedPublisher = publisherStateSet.GetPublisher(dropRequest.Publisher.PublisherID);
                var dropResult = GameEligibilityFunctions.CanDropGame(dropRequest, leagueYearGroup.Key, affectedPublisher, _currentDate, null);
                if (dropResult.Result.IsSuccess)
                {
                    successDrops.Add(dropRequest);
                    var publisherGame = dropRequest.Publisher.GetPublisherGameOrThrow(dropRequest.MasterGame, false);
                    var formerPublisherGame = publisherGame.GetFormerPublisherGame(_processingTime, "Dropped by player");
                    gamesToDelete.Add(formerPublisherGame);
                    LeagueAction leagueAction = new LeagueAction(dropRequest, dropResult, _processingTime);
                    publisherStateSet.DropGameForPublisher(affectedPublisher, publisherGame, leagueYearGroup.Key.Options, false, _currentDate);

                    leagueActions.Add(leagueAction);
                }
                else
                {
                    failedDrops.Add(dropRequest);
                    LeagueAction leagueAction = new LeagueAction(dropRequest, dropResult, _processingTime);
                    leagueActions.Add(leagueAction);
                }
            }
        }

        ActionProcessingResults dropProcessingResults = ActionProcessingResults.GetResultsSetFromDropResults(successDrops, failedDrops, leagueActions, publisherStateSet, gamesToDelete);
        return dropProcessingResults;
    }

    private ActionProcessingResults ProcessPickups(IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> allActiveBids, ActionProcessingResults existingResults)
    {
        var publisherStateSet = existingResults.PublisherStateSet;
        var runningActiveBids = allActiveBids;
        var runningResults = existingResults;

        var successfulConditionalDropsByLeague = allActiveBids.Keys.ToDictionary(x => x, _ => new List<PublisherGame>());
        var iteration = 0;
        while (runningActiveBids.Any())
        {
            var flatAllBids = runningActiveBids.SelectMany(x => x.Value).ToList();
            _logger.Information("Processing iteration {iteration}, {remainingBids} bids remaining.", iteration, flatAllBids.Count);
            var processedBids = new ProcessedBidSet();
            foreach (var leagueYearWithBids in runningActiveBids)
            {
                if (!leagueYearWithBids.Value.Any())
                {
                    continue;
                }

                var conditionalDropsThatHaveSucceededForLeague = successfulConditionalDropsByLeague[leagueYearWithBids.Key].ToList();
                var processedBidsForLeagueYear = ProcessPickupsForLeagueYear(leagueYearWithBids.Key, leagueYearWithBids.Value, publisherStateSet, false, conditionalDropsThatHaveSucceededForLeague);

                var successfulConditionalDrops = processedBidsForLeagueYear.SuccessBids
                    .Where(x => x.PickupBid.ConditionalDropPublisherGame is not null && x.PickupBid.ConditionalDropResult!.Result.IsSuccess)
                    .Select(x => x.PickupBid.ConditionalDropPublisherGame!).ToList();
                successfulConditionalDropsByLeague[leagueYearWithBids.Key].AddRange(successfulConditionalDrops);

                processedBids = processedBids.AppendSet(processedBidsForLeagueYear);
            }

            ActionProcessingResults bidResults = GetBidProcessingResults(processedBids.SuccessBids, processedBids.FailedBids, publisherStateSet, new List<LeagueManagerAction>());
            var newResults = runningResults.Combine(bidResults);
            var remainingBids = flatAllBids.Except(processedBids.ProcessedBids);
            runningActiveBids = remainingBids.GroupToDictionary(x => x.LeagueYear);
            runningResults = runningResults.Combine(newResults);
            iteration++;
        }
        
        return runningResults;
    }

    private ActionProcessingResults ProcessSpecialAuctionsInternal(IReadOnlyList<LeagueYearSpecialAuctionSet> leagueYearSpecialAuctions)
    {
        var allPublishers = leagueYearSpecialAuctions.SelectMany(x => x.LeagueYear.Publishers);
        var publisherStateSet = new PublisherStateSet(allPublishers);
        var processedBids = new ProcessedBidSet();
        List<LeagueManagerAction> noBidsActions = new List<LeagueManagerAction>();
        foreach (var singleLeagueYearSet in leagueYearSpecialAuctions)
        {
            var leagueYear = singleLeagueYearSet.LeagueYear;
            var orderedSpecialAuctions = singleLeagueYearSet.SpecialAuctionsWithBids.OrderBy(x => x.SpecialAuction.ScheduledEndTime).ToList();
            List<PickupBid> bidsToProcessForLeague = new List<PickupBid>();
            foreach (var specialAuctionWithBids in orderedSpecialAuctions)
            {
                var masterGame = specialAuctionWithBids.SpecialAuction.MasterGameYear.MasterGame;
                if (!specialAuctionWithBids.Bids.Any())
                {
                    noBidsActions.Add(new LeagueManagerAction(leagueYear.Key, _processingTime, "Special Auction Ended", $"Special Auction for: '{masterGame.GameName}' ended with no bids and no winner."));
                    continue;
                }

                bidsToProcessForLeague.AddRange(specialAuctionWithBids.Bids);
            }

            var processedBidsForLeagueYear = ProcessPickupsForLeagueYear(leagueYear, bidsToProcessForLeague, publisherStateSet, true, new List<PublisherGame>());
            processedBids = processedBids.AppendSet(processedBidsForLeagueYear);
        }

        ActionProcessingResults bidResults = GetBidProcessingResults(processedBids.SuccessBids, processedBids.FailedBids, publisherStateSet, noBidsActions);
        return bidResults;
    }

    private ProcessedBidSet ProcessPickupsForLeagueYear(LeagueYear leagueYear, IReadOnlyList<PickupBid> activeBidsForLeague,
        PublisherStateSet publisherStateSet, bool specialAuctions, IReadOnlyList<PublisherGame> conditionalDropsThatHaveSucceeded)
    {
        LeagueYear updatedLeagueYear = publisherStateSet.GetUpdatedLeagueYear(leagueYear);
        var gamesGroupedByPublisherAndGame = activeBidsForLeague.GroupBy(x => (x.Publisher.PublisherID, x.MasterGame.MasterGameID));
        var duplicateBidGroups = gamesGroupedByPublisherAndGame.Where(x => x.Count() > 1).ToList();
        List<PickupBid> duplicateBids = new List<PickupBid>();
        foreach (var duplicateBidGroup in duplicateBidGroups)
        {
            var bestBid = duplicateBidGroup.WhereMax(x => x.BidAmount).WhereMin(x => x.Timestamp).First();
            var otherBids = duplicateBidGroup.Except(new List<PickupBid>() { bestBid });
            duplicateBids.AddRange(otherBids);
        }

        var nonDuplicateBids = activeBidsForLeague.Except(duplicateBids).ToList();

        List<PickupBid> noSpaceLeftBids = new List<PickupBid>();
        List<PickupBid> noEligibleSpaceLeftBids = new List<PickupBid>();
        List<PickupBid> insufficientFundsBids = new List<PickupBid>();
        List<PickupBid> belowMinimumBids = new List<PickupBid>();
        List<KeyValuePair<PickupBid, string>> invalidGameBids = new List<KeyValuePair<PickupBid, string>>();

        List<ValidPickupBid> validPickupBids = new List<ValidPickupBid>();
        foreach (var activeBid in nonDuplicateBids)
        {
            Publisher bidPublisher = publisherStateSet.GetPublisher(activeBid.Publisher.PublisherID);
            bool counterPickedGameIsManualWillNotRelease = PlayerGameExtensions.CounterPickedGameIsManualWillNotRelease(updatedLeagueYear, activeBid.CounterPick, activeBid.MasterGame, true);
            var gameRequest = new ClaimGameDomainRequest(updatedLeagueYear, bidPublisher, activeBid.MasterGame.GameName, activeBid.CounterPick,
                counterPickedGameIsManualWillNotRelease, false, false, activeBid.MasterGame, null, null);

            PickupBid pickupBidWithConditionalDropResult = activeBid;
            int? validConditionalDropSlot = null;
            if (activeBid.ConditionalDropPublisherGame is not null)
            {
                var conditionalDropResult = GameEligibilityFunctions.CanConditionallyDropGame(activeBid, updatedLeagueYear, bidPublisher, _processingTime, _currentDate);
                pickupBidWithConditionalDropResult = activeBid.WithConditionalDropResult(conditionalDropResult);
                if (conditionalDropResult.Result.IsSuccess)
                {
                    validConditionalDropSlot = activeBid.ConditionalDropPublisherGame.SlotNumber;
                }
            }

            bool counterPickWillBeConditionallyDropped = activeBid.CounterPick && conditionalDropsThatHaveSucceeded.ContainsGame(activeBid.MasterGame);
            var claimResult = GameEligibilityFunctions.CanClaimGame(gameRequest, null, validConditionalDropSlot, true, false, specialAuctions, counterPickWillBeConditionallyDropped, _currentDate, activeBid.AllowIneligibleSlot);
            if (claimResult.NoSpaceError)
            {
                noSpaceLeftBids.Add(pickupBidWithConditionalDropResult);
                continue;
            }

            if (claimResult.NoEligibleSpaceError)
            {
                noEligibleSpaceLeftBids.Add(pickupBidWithConditionalDropResult);
                continue;
            }

            if (!claimResult.Success)
            {
                invalidGameBids.Add(new KeyValuePair<PickupBid, string>(pickupBidWithConditionalDropResult, string.Join(" AND ", claimResult.Errors.Select(x => x.Error))));
                continue;
            }

            if (pickupBidWithConditionalDropResult.BidAmount > bidPublisher.Budget)
            {
                insufficientFundsBids.Add(pickupBidWithConditionalDropResult);
                continue;
            }

            if (pickupBidWithConditionalDropResult.BidAmount < updatedLeagueYear.Options.MinimumBidAmount)
            {
                belowMinimumBids.Add(pickupBidWithConditionalDropResult);
                continue;
            }

            validPickupBids.Add(new ValidPickupBid(pickupBidWithConditionalDropResult, claimResult.BestSlotNumber!.Value));
        }

        var processDate = _processingTime.ToEasternDate();
        var winningBids = GetWinningBids(updatedLeagueYear, validPickupBids);

        var takenGames = winningBids.Select(x => x.PickupBid.MasterGame).ToHashSet();
        var losingBids = activeBidsForLeague
            .Except(winningBids.Select(x => x.PickupBid))
            .Except(duplicateBids)
            .Except(noSpaceLeftBids)
            .Except(noEligibleSpaceLeftBids)
            .Except(insufficientFundsBids)
            .Except(belowMinimumBids)
            .Except(invalidGameBids.Select(x => x.Key))
            .Where(x => takenGames.Contains(x.MasterGame))
            .Select(x => new FailedPickupBid(x, GetLostBidMessage(x, winningBids), _systemWideValues, processDate))
            .ToList();

        var duplicateBidFailures = duplicateBids.Select(x => new FailedPickupBid(x, "You cannot have multiple bids for the same game. This bid has been ignored.", _systemWideValues, processDate)).ToList();
        var invalidGameBidFailures = invalidGameBids.Select(x => new FailedPickupBid(x.Key, "Game is no longer eligible: " + x.Value, _systemWideValues, processDate)).ToList();
        var insufficientFundsBidFailures = insufficientFundsBids.Select(x => new FailedPickupBid(x, "Not enough budget.", _systemWideValues, processDate)).ToList();
        var belowMinimumBidFailures = belowMinimumBids.Select(x => new FailedPickupBid(x, "Bid is below the minimum bid amount.", _systemWideValues, processDate)).ToList();
        List<FailedPickupBid> noSpaceLeftBidFailures = new List<FailedPickupBid>();
        foreach (var noSpaceLeftBid in noSpaceLeftBids)
        {
            FailedPickupBid failedBid;
            if (noSpaceLeftBid.ConditionalDropPublisherGame is not null && noSpaceLeftBid.ConditionalDropResult!.Result.IsFailure)
            {
                failedBid = new FailedPickupBid(noSpaceLeftBid, "No roster spots available. Attempted to conditionally drop game: " +
                                                                $"{noSpaceLeftBid.ConditionalDropPublisherGame.MasterGame!.MasterGame.GameName} " +
                                                                $"but failed because: {noSpaceLeftBid.ConditionalDropResult.Result.Error}", _systemWideValues, processDate);
            }
            else
            {
                failedBid = new FailedPickupBid(noSpaceLeftBid, "No roster spots available.", _systemWideValues, processDate);
            }
            noSpaceLeftBidFailures.Add(failedBid);
        }

        foreach (var noEligibleSpaceLeftBid in noEligibleSpaceLeftBids)
        {
            FailedPickupBid failedBid;
            if (noEligibleSpaceLeftBid.ConditionalDropPublisherGame is not null && noEligibleSpaceLeftBid.ConditionalDropResult!.Result.IsFailure)
            {
                failedBid = new FailedPickupBid(noEligibleSpaceLeftBid, "No eligible roster spots available. Attempted to conditionally drop game: " +
                                                                        $"{noEligibleSpaceLeftBid.ConditionalDropPublisherGame.MasterGame!.MasterGame.GameName} " +
                                                                        $"but failed because: {noEligibleSpaceLeftBid.ConditionalDropResult.Result.Error}", _systemWideValues, processDate);
            }
            else
            {
                failedBid = new FailedPickupBid(noEligibleSpaceLeftBid, "No eligible roster spots available.", _systemWideValues, processDate);
            }
            noSpaceLeftBidFailures.Add(failedBid);
        }

        var failedBids = losingBids
            .Concat(insufficientFundsBidFailures)
            .Concat(belowMinimumBidFailures)
            .Concat(noSpaceLeftBidFailures)
            .Concat(duplicateBidFailures)
            .Concat(invalidGameBidFailures);

        var processedSet = new ProcessedBidSet(winningBids, failedBids);
        return processedSet;
    }

    private static string GetLostBidMessage(PickupBid pickupBid, IReadOnlyList<SucceededPickupBid> winningBids)
    {
        var winningBid = winningBids.SingleOrDefault(x =>
            x.PickupBid.LeagueYear.Key == pickupBid.LeagueYear.Key &&
            x.PickupBid.MasterGame.MasterGameID == pickupBid.MasterGame.MasterGameID);

        if (winningBid is null)
        {
            _logger.Warning("Impossible situation with bids: {leagueID} {masterGame}", pickupBid.LeagueYear.League.LeagueID, pickupBid.MasterGame.GameName);
            return "Publisher was outbid.";
        }

        if (pickupBid.BidAmount == winningBid.PickupBid.BidAmount)
        {
            return "Bid lost on tiebreakers.";
        }

        return "Publisher was outbid.";
    }

    private IReadOnlyList<SucceededPickupBid> GetWinningBids(LeagueYear leagueYear, IReadOnlyList<ValidPickupBid> activeBidsForLeagueYear)
    {
        var bidsGroupedByPublisher = activeBidsForLeagueYear.GroupBy(x => x.PickupBid.Publisher);
        var highestPriorityPerPublisher = new Dictionary<Publisher, int>();
        foreach (var bidGroup in bidsGroupedByPublisher)
        {
            var highestPriority = bidGroup.Select(x => x.PickupBid.Priority).Min();
            highestPriorityPerPublisher[bidGroup.Key] = highestPriority;
        }

        List<SucceededPickupBid> winnableBids = new List<SucceededPickupBid>();
        var groupedByGame = activeBidsForLeagueYear.GroupToDictionary(x => x.PickupBid.MasterGame);
        foreach (var gameGroup in groupedByGame)
        {
            var bestBid = GetWinningBidForGame(gameGroup.Key, leagueYear, gameGroup.Value);
            winnableBids.Add(bestBid);
        }

        if (!winnableBids.Any())
        {
            return new List<SucceededPickupBid>();
        }

        var winnableCounterPickBids = winnableBids.Where(x => x.PickupBid.CounterPick).ToList();
        var winnableBidsWithConditionalDrop = winnableBids.Where(x => x.PickupBid.ConditionalDropPublisherGame is not null && x.PickupBid.ConditionalDropPublisherGame.MasterGame is not null).ToList();
        var gamesThatCouldGetCounterPicked = winnableCounterPickBids.Select(x => x.PickupBid.MasterGame).Distinct().ToList();
        var gamesThatCouldGetDropped = winnableBidsWithConditionalDrop.Select(x => x.PickupBid.ConditionalDropPublisherGame!.MasterGame!.MasterGame).Distinct().ToList();
        var conflictedGames = gamesThatCouldGetCounterPicked.Intersect(gamesThatCouldGetDropped).ToHashSet();

        var winningBids = winnableBids.Where(x => x.PickupBid.Priority == highestPriorityPerPublisher[x.PickupBid.Publisher]).ToList();

        var unconflictedWinningBids = winningBids.Where(x => !x.PickupBid.CounterPick || !conflictedGames.Contains(x.PickupBid.MasterGame)).ToList();
        if (unconflictedWinningBids.Any())
        {
            return unconflictedWinningBids;
        }

        if (winningBids.Any())
        {
            return winningBids;
        }

        var winnableBidsGroupedByPublisher = winnableBids.GroupBy(x => x.PickupBid.Publisher);
        var highestPriorityWinnablePerPublisher = new Dictionary<Publisher, int>();
        foreach (var bidGroup in winnableBidsGroupedByPublisher)
        {
            var highestPriority = bidGroup.Select(x => x.PickupBid.Priority).Min();
            highestPriorityWinnablePerPublisher[bidGroup.Key] = highestPriority;
        }

        List<SucceededPickupBid> winningBidsSecondAttempt = winnableBids.Where(x => x.PickupBid.Priority == highestPriorityWinnablePerPublisher[x.PickupBid.Publisher]).ToList();
        return winningBidsSecondAttempt;
    }

    private SucceededPickupBid GetWinningBidForGame(MasterGame masterGame, LeagueYear leagueYear, IEnumerable<ValidPickupBid> bidsForGame)
    {
        if (bidsForGame.Count() == 1)
        {
            var singleBid = bidsForGame.First();
            return new SucceededPickupBid(singleBid.PickupBid, singleBid.SlotNumber, "No competing bids for this game.", _systemWideValues, _currentDate);
        }

        var bestBids = bidsForGame.WhereMax(x => x.PickupBid.BidAmount).ToList();
        if (bestBids.Count == 1)
        {
            var singleBid = bestBids.Single();
            return new SucceededPickupBid(singleBid.PickupBid, singleBid.SlotNumber, "This bid was the highest bid.", _systemWideValues, _currentDate);
        }

        IEnumerable<ValidPickupBid> remainingGamesAfterTiebreaks;
        if (leagueYear.Options.TiebreakSystem.Equals(TiebreakSystem.LowestProjectedPoints))
        {
            var bestBidsByProjectedScore = bestBids.WhereMin(x => x.PickupBid.Publisher.GetProjectedFantasyPoints(leagueYear, _systemWideValues)).ToList();
            if (bestBidsByProjectedScore.Count == 1)
            {
                var singleBid = bestBidsByProjectedScore.Single();
                return new SucceededPickupBid(singleBid.PickupBid, singleBid.SlotNumber, "This publisher has the lowest projected points. (Not including this game)", _systemWideValues, _currentDate);
            }

            var bestBidsByBidTime = bestBidsByProjectedScore.WhereMin(x => x.PickupBid.Timestamp).ToList();
            if (bestBidsByBidTime.Count == 1)
            {
                var singleBid = bestBidsByBidTime.Single();
                return new SucceededPickupBid(singleBid.PickupBid, singleBid.SlotNumber, "This bid was placed earliest. (Projected points were tied)", _systemWideValues, _currentDate);
            }

            remainingGamesAfterTiebreaks = bestBidsByBidTime;
        }
        else if (leagueYear.Options.TiebreakSystem.Equals(TiebreakSystem.EarliestBid))
        {
            var bestBidsByBidTime = bestBids.WhereMin(x => x.PickupBid.Timestamp).ToList();
            if (bestBidsByBidTime.Count == 1)
            {
                var singleBid = bestBidsByBidTime.Single();
                return new SucceededPickupBid(singleBid.PickupBid, singleBid.SlotNumber, "This bid was placed earliest.", _systemWideValues, _currentDate);
            }

            var bestBidsByProjectedScore = bestBidsByBidTime.WhereMin(x => x.PickupBid.Publisher.GetProjectedFantasyPoints(leagueYear, _systemWideValues)).ToList();
            if (bestBidsByProjectedScore.Count == 1)
            {
                var singleBid = bestBidsByProjectedScore.Single();
                return new SucceededPickupBid(singleBid.PickupBid, singleBid.SlotNumber, "This publisher has the lowest projected points. (Not including this game) (Bid placement time was tied)", _systemWideValues, _currentDate);
            }

            remainingGamesAfterTiebreaks = bestBidsByProjectedScore;
        }
        else
        {
            throw new NotImplementedException($"Unknown tiebreak system: {leagueYear.Options.TiebreakSystem}");
        }

        var bestBidsByDraftPosition = remainingGamesAfterTiebreaks.OrderByDescending(x => x.PickupBid.Publisher.DraftPosition).ToList();
        if (bestBidsByDraftPosition.Count == 1)
        {
            var singleBid = bestBidsByDraftPosition.Single();
            return new SucceededPickupBid(singleBid.PickupBid, singleBid.SlotNumber, "This publisher had the latest draft position (Projected points and Bid placement time were tied)", _systemWideValues, _currentDate);
        }

        throw new Exception($"Inconceivable tie situation for game: {masterGame.GameName} for league year: {leagueYear}");
    }

    private ActionProcessingResults GetBidProcessingResults(IReadOnlyList<SucceededPickupBid> successBids, IReadOnlyList<FailedPickupBid> failedBids, PublisherStateSet publisherStateSet,
        IEnumerable<LeagueManagerAction> leagueManagerActions)
    {
        List<PublisherGame> gamesToAdd = new List<PublisherGame>();
        List<LeagueAction> leagueActions = new List<LeagueAction>();
        foreach (var successBid in successBids)
        {
            var masterGameYear = _masterGameYearDictionary[successBid.PickupBid.MasterGame.MasterGameID];
            PublisherGame newPublisherGame = new PublisherGame(successBid.PickupBid.Publisher.PublisherID, Guid.NewGuid(), successBid.PickupBid.MasterGame.GameName, _processingTime,
                successBid.PickupBid.CounterPick, null, false, null, masterGameYear, successBid.SlotNumber, null, null, successBid.PickupBid.BidAmount, null);
            gamesToAdd.Add(newPublisherGame);
            var affectedPublisher = publisherStateSet.GetPublisher(successBid.PickupBid.Publisher.PublisherID);
            publisherStateSet.AcquireGameForPublisher(affectedPublisher, newPublisherGame, successBid.PickupBid.BidAmount);

            LeagueAction leagueAction = new LeagueAction(successBid.PickupBid, _processingTime);
            leagueActions.Add(leagueAction);
        }

        foreach (var failedBid in failedBids)
        {
            LeagueAction leagueAction = new LeagueAction(failedBid, _processingTime);
            leagueActions.Add(leagueAction);
        }

        List<FormerPublisherGame> conditionalDroppedGames = new List<FormerPublisherGame>();
        var successfulConditionalDrops = successBids.Where(x => x.PickupBid.ConditionalDropPublisherGame is not null && x.PickupBid.ConditionalDropResult!.Result.IsSuccess);
        foreach (var successfulConditionalDrop in successfulConditionalDrops)
        {
            var affectedPublisher = publisherStateSet.GetPublisher(successfulConditionalDrop.PickupBid.Publisher.PublisherID);
            publisherStateSet.DropGameForPublisher(affectedPublisher, successfulConditionalDrop.PickupBid.ConditionalDropPublisherGame!, successfulConditionalDrop.PickupBid.LeagueYear.Options, false, _currentDate);
            var formerPublisherGame = successfulConditionalDrop.PickupBid.ConditionalDropPublisherGame!.GetFormerPublisherGame(_processingTime, $"Conditionally dropped while picking up: {successfulConditionalDrop.PickupBid.MasterGame.GameName}");
            conditionalDroppedGames.Add(formerPublisherGame);
        }

        ActionProcessingResults bidProcessingResults = ActionProcessingResults.GetResultsSetFromBidResults(successBids, failedBids,
            leagueActions, publisherStateSet, gamesToAdd, conditionalDroppedGames, leagueManagerActions);
        return bidProcessingResults;
    }
}
