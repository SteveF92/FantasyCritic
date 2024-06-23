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
        var publisherStateSet = new PublisherStateSet(publishers);
        var flatBids = allActiveBids.SelectMany(x => x.Value);
        var invalidBids = flatBids.Where(x => x.CounterPick && x.ConditionalDropPublisherGame is not null);
        if (invalidBids.Any())
        {
            throw new Exception("There are counter pick bids with conditional drops.");
        }

        string processName = $"Drop/Bid Processing ({_processingTime.ToEasternDate()})";
        Guid processSetID = Guid.NewGuid();
        if (!allActiveBids.Any() && !allActiveDrops.Any())
        {
            var emptyResults = ActionProcessingResults.GetEmptyResultsSet(publisherStateSet);
            return new FinalizedActionProcessingResults(processSetID, _processingTime, processName, emptyResults, new List<SpecialAuction>());
        }

        ActionProcessingResults dropResults = ProcessDrops(allActiveDrops, publisherStateSet);
        if (!allActiveBids.Any())
        {
            return new FinalizedActionProcessingResults(processSetID, _processingTime, processName, dropResults, new List<SpecialAuction>());
        }

        var dropResultsCopy = dropResults.MakeCopy();
        ILookup<LeagueYearKey, PublisherGame> conditionalDropsThatWillSucceed = GetConditionalDropsThatWillSucceed(allActiveBids, dropResultsCopy);

        ActionProcessingResults bidResults = ProcessPickups(allActiveBids, dropResults, conditionalDropsThatWillSucceed);
        return new FinalizedActionProcessingResults(processSetID, _processingTime, processName, bidResults, new List<SpecialAuction>());
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

    private ILookup<LeagueYearKey, PublisherGame> GetConditionalDropsThatWillSucceed(IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> allActiveBids, ActionProcessingResults existingResults)
    {
        var emptyLookup = new List<(LeagueYearKey, PublisherGame)>().ToLookup(x => x.Item1, y => y.Item2);
        ActionProcessingResults bidResults = ProcessPickups(allActiveBids, existingResults, emptyLookup);
        var finalLookup = bidResults.SuccessBids
            .Where(x => x.PickupBid.ConditionalDropPublisherGame is not null)
            .Select(x => (x.PickupBid.LeagueYear.Key, x.PickupBid.ConditionalDropPublisherGame!))
            .ToLookup(x => x.Item1, y => y.Item2);
        return finalLookup;
    }

    private ActionProcessingResults ProcessPickups(IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> allActiveBids,
        ActionProcessingResults existingResults, ILookup<LeagueYearKey, PublisherGame> conditionalDropsThatWillSucceed)
    {
        var publisherStateSet = existingResults.PublisherStateSet;
        var runningActiveBids = allActiveBids;
        var runningResults = existingResults;

        while (runningActiveBids.Any())
        {
            IEnumerable<PickupBid> flatAllBids = runningActiveBids.SelectMany(x => x.Value);
            var processedBids = new ProcessedBidSet();
            foreach (var leagueYearWithBids in runningActiveBids)
            {
                if (!leagueYearWithBids.Value.Any())
                {
                    continue;
                }

                var conditionalDropsThatWillSucceedForLeague = conditionalDropsThatWillSucceed[leagueYearWithBids.Key.Key].ToList();
                var processedBidsForLeagueYear = ProcessPickupsForLeagueYear(leagueYearWithBids.Key, leagueYearWithBids.Value, publisherStateSet, false, conditionalDropsThatWillSucceedForLeague);
                processedBids = processedBids.AppendSet(processedBidsForLeagueYear);
            }

            ActionProcessingResults bidResults = GetBidProcessingResults(processedBids.SuccessBids, processedBids.FailedBids, publisherStateSet, new List<LeagueManagerAction>());
            var newResults = runningResults.Combine(bidResults);
            var remainingBids = flatAllBids.Except(processedBids.ProcessedBids);
            runningActiveBids = remainingBids.GroupToDictionary(x => x.LeagueYear);
            runningResults = runningResults.Combine(newResults);
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
        PublisherStateSet publisherStateSet, bool specialAuctions, IReadOnlyList<PublisherGame> conditionalDropsThatWillSucceed)
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

            bool counterPickWillBeConditionallyDropped = activeBid.CounterPick && conditionalDropsThatWillSucceed.ContainsGame(activeBid.MasterGame);
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
        List<SucceededPickupBid> winnableBids = new List<SucceededPickupBid>();
        var groupedByGame = activeBidsForLeagueYear.GroupToDictionary(x => x.PickupBid.MasterGame);
        foreach (var gameGroup in groupedByGame)
        {
            var bestBid = GetWinningBidForGame(gameGroup.Key, leagueYear, gameGroup.Value);
            winnableBids.Add(bestBid);
        }

        List<SucceededPickupBid> winningBids = new List<SucceededPickupBid>();
        var groupedByPublisher = winnableBids.GroupBy(x => x.PickupBid.Publisher);
        foreach (var publisherGroup in groupedByPublisher)
        {
            SucceededPickupBid winningBid = publisherGroup.WhereMin(x => x.PickupBid.Priority).First();
            winningBids.Add(winningBid);
        }

        if (!winningBids.Any())
        {
            return winningBids;
        }

        //With each iteration, we only want to take the highest priority bids as finalized winners.
        //This is because if you have a 1st priority bid that lost this iteration, and a 2nd priority bid that won this iteration, you don't want to actually take that 2nd priority game yet.
        //The reason is because you might actually win that 1st priority bid on the next iteration, because other users spent their money or slots on the last iteration where you didn't win your 1st priority game.
        var highestPriorityOverallBids = winningBids.WhereMin(x => x.PickupBid.Priority).ToList();
        return highestPriorityOverallBids;
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
            var bestBidsByProjectedScore = bestBids.WhereMin(x => x.PickupBid.Publisher.GetProjectedFantasyPoints(leagueYear, _systemWideValues, _currentDate)).ToList();
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

            var bestBidsByProjectedScore = bestBidsByBidTime.WhereMin(x => x.PickupBid.Publisher.GetProjectedFantasyPoints(leagueYear, _systemWideValues, _currentDate)).ToList();
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
