using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Lib.Services;

public class ActionProcessingService
{
    private readonly GameAcquisitionService _gameAcquisitionService;

    public ActionProcessingService(GameAcquisitionService gameAcquisitionService)
    {
        _gameAcquisitionService = gameAcquisitionService;
    }

    public FinalizedActionProcessingResults ProcessActions(SystemWideValues systemWideValues, IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> allActiveBids,
        IReadOnlyDictionary<LeagueYear, IReadOnlyList<DropRequest>> allActiveDrops, IEnumerable<Publisher> publishers, Instant processingTime, IReadOnlyDictionary<Guid, MasterGameYear> masterGameYearDictionary)
    {
        var publisherStateSet = new PublisherStateSet(publishers);
        var flatBids = allActiveBids.SelectMany(x => x.Value);
        var invalidBids = flatBids.Where(x => x.CounterPick && x.ConditionalDropPublisherGame.HasValue);
        if (invalidBids.Any())
        {
            throw new Exception("There are counter pick bids with conditional drops.");
        }

        string processName = $"Drop/Bid Processing ({processingTime.ToEasternDate()})";
        Guid processSetID = Guid.NewGuid();
        if (!allActiveBids.Any() && !allActiveDrops.Any())
        {
            var emptyResults = ActionProcessingResults.GetEmptyResultsSet(publisherStateSet);
            return new FinalizedActionProcessingResults(processSetID, processingTime, processName, emptyResults);
        }

        ActionProcessingResults dropResults = ProcessDrops(allActiveDrops, publisherStateSet, processingTime);
        if (!allActiveBids.Any())
        {
            return new FinalizedActionProcessingResults(processSetID, processingTime, processName, dropResults);
        }

        ActionProcessingResults bidResults = ProcessPickupsIteration(systemWideValues, allActiveBids, dropResults, processingTime, masterGameYearDictionary);
        return new FinalizedActionProcessingResults(processSetID, processingTime, processName, bidResults);
    }

    private ActionProcessingResults ProcessDrops(IReadOnlyDictionary<LeagueYear, IReadOnlyList<DropRequest>> allDropRequests, PublisherStateSet publisherStateSet, Instant processingTime)
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
                var dropResult = _gameAcquisitionService.CanDropGame(dropRequest, leagueYearGroup.Key, affectedPublisher);
                if (dropResult.Result.IsSuccess)
                {
                    successDrops.Add(dropRequest);
                    var publisherGame = dropRequest.Publisher.GetPublisherGame(dropRequest.MasterGame);
                    var formerPublisherGame = publisherGame.Value.GetFormerPublisherGame(processingTime, "Dropped by player");
                    gamesToDelete.Add(formerPublisherGame);
                    LeagueAction leagueAction = new LeagueAction(dropRequest, dropResult, processingTime);
                    publisherStateSet.DropGameForPublisher(affectedPublisher, publisherGame.Value, leagueYearGroup.Key.Options);

                    leagueActions.Add(leagueAction);
                }
                else
                {
                    failedDrops.Add(dropRequest);
                    LeagueAction leagueAction = new LeagueAction(dropRequest, dropResult, processingTime);
                    leagueActions.Add(leagueAction);
                }
            }
        }

        ActionProcessingResults dropProcessingResults = ActionProcessingResults.GetResultsSetFromDropResults(successDrops, failedDrops, leagueActions, publisherStateSet, gamesToDelete);
        return dropProcessingResults;
    }

    private ActionProcessingResults ProcessPickupsIteration(SystemWideValues systemWideValues, IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> allActiveBids,
        ActionProcessingResults existingResults, Instant processingTime, IReadOnlyDictionary<Guid, MasterGameYear> masterGameYearDictionary)
    {
        IEnumerable<PickupBid> flatAllBids = allActiveBids.SelectMany(x => x.Value);

        var publisherStateSet = existingResults.PublisherStateSet;
        var processedBids = new ProcessedBidSet();
        foreach (var leagueYear in allActiveBids)
        {
            if (!leagueYear.Value.Any())
            {
                continue;
            }

            var processedBidsForLeagueYear = ProcessPickupsForLeagueYear(leagueYear.Key, leagueYear.Value, publisherStateSet, systemWideValues, processingTime);
            processedBids = processedBids.AppendSet(processedBidsForLeagueYear);
        }

        ActionProcessingResults bidResults = GetBidProcessingResults(processedBids.SuccessBids, processedBids.FailedBids, publisherStateSet, processingTime, masterGameYearDictionary);
        var newResults = existingResults.Combine(bidResults);
        var remainingBids = flatAllBids.Except(processedBids.ProcessedBids);
        if (remainingBids.Any())
        {
            IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> remainingBidDictionary = remainingBids.GroupToDictionary(x => x.LeagueYear);
            var subProcessingResults = ProcessPickupsIteration(systemWideValues, remainingBidDictionary, newResults, processingTime, masterGameYearDictionary);
            ActionProcessingResults combinedResults = newResults.Combine(subProcessingResults);
            return combinedResults;
        }

        return newResults;
    }

    private ProcessedBidSet ProcessPickupsForLeagueYear(LeagueYear leagueYear, IReadOnlyList<PickupBid> activeBidsForLeague,
        PublisherStateSet publisherStateSet, SystemWideValues systemWideValues, Instant processingTime)
    {
        var gamesGroupedByPublisherAndGame = activeBidsForLeague.GroupBy(x => (x.Publisher.PublisherID, x.MasterGame.MasterGameID));
        var duplicateBidGroups = gamesGroupedByPublisherAndGame.Where(x => x.Count() > 1).ToList();
        List<PickupBid> duplicateBids = new List<PickupBid>();
        foreach (var duplicateBidGroup in duplicateBidGroups)
        {
            var bestBid = duplicateBidGroup.WhereMax(x => x.BidAmount).WhereMin(x => x.Timestamp).FirstOrDefault();
            var otherBids = duplicateBidGroup.Except(new List<PickupBid>() { bestBid });
            duplicateBids.AddRange(otherBids);
        }

        var nonDuplicateBids = activeBidsForLeague.Except(duplicateBids);

        List<PickupBid> noSpaceLeftBids = new List<PickupBid>();
        List<PickupBid> insufficientFundsBids = new List<PickupBid>();
        List<PickupBid> belowMinimumBids = new List<PickupBid>();
        List<KeyValuePair<PickupBid, string>> invalidGameBids = new List<KeyValuePair<PickupBid, string>>();

        var currentDate = processingTime.ToEasternDate();

        List<ValidPickupBid> validPickupBids = new List<ValidPickupBid>();
        foreach (var activeBid in nonDuplicateBids)
        {
            Publisher bidPublisher = publisherStateSet.GetPublisher(activeBid.Publisher.PublisherID);
            bool counterPickedGameIsManualWillNotRelease = PlayerGameExtensions.CounterPickedGameIsManualWillNotRelease(leagueYear, activeBid.CounterPick, activeBid.MasterGame, true);
            var gameRequest = new ClaimGameDomainRequest(leagueYear, bidPublisher, activeBid.MasterGame.GameName, activeBid.CounterPick, counterPickedGameIsManualWillNotRelease, false, false, activeBid.MasterGame, null, null);

            int? validConditionalDropSlot = null;
            if (activeBid.ConditionalDropPublisherGame.HasValue)
            {
                activeBid.ConditionalDropResult = _gameAcquisitionService.CanConditionallyDropGame(activeBid, leagueYear, bidPublisher, processingTime);
                if (activeBid.ConditionalDropResult.Result.IsSuccess)
                {
                    validConditionalDropSlot = activeBid.ConditionalDropPublisherGame.Value.SlotNumber;
                }
            }

            var claimResult = _gameAcquisitionService.CanClaimGame(gameRequest, null, validConditionalDropSlot, false, false);
            if (claimResult.NoSpaceError)
            {
                noSpaceLeftBids.Add(activeBid);
                continue;
            }

            if (!claimResult.Success)
            {
                invalidGameBids.Add(new KeyValuePair<PickupBid, string>(activeBid, string.Join(" AND ", claimResult.Errors.Select(x => x.Error))));
                continue;
            }

            if (activeBid.BidAmount > bidPublisher.Budget)
            {
                insufficientFundsBids.Add(activeBid);
                continue;
            }

            if (activeBid.BidAmount < leagueYear.Options.MinimumBidAmount)
            {
                belowMinimumBids.Add(activeBid);
                continue;
            }

            validPickupBids.Add(new ValidPickupBid(activeBid, claimResult.BestSlotNumber.Value));
        }

        var winnableBids = GetWinnableBids(leagueYear, validPickupBids, systemWideValues, currentDate);
        var winningBids = GetWinningBids(winnableBids);

        var takenGames = winningBids.Select(x => x.PickupBid.MasterGame);
        var losingBids = activeBidsForLeague
            .Except(winningBids.Select(x => x.PickupBid))
            .Except(duplicateBids)
            .Except(noSpaceLeftBids)
            .Except(insufficientFundsBids)
            .Except(belowMinimumBids)
            .Except(invalidGameBids.Select(x => x.Key))
            .Where(x => takenGames.Contains(x.MasterGame))
            .Select(x => new FailedPickupBid(x, "Publisher was outbid.", systemWideValues, currentDate));

        var duplicateBidFailures = duplicateBids.Select(x => new FailedPickupBid(x, "You cannot have multiple bids for the same game. This bid has been ignored.", systemWideValues, currentDate));
        var invalidGameBidFailures = invalidGameBids.Select(x => new FailedPickupBid(x.Key, "Game is no longer eligible: " + x.Value, systemWideValues, currentDate));
        var insufficientFundsBidFailures = insufficientFundsBids.Select(x => new FailedPickupBid(x, "Not enough budget.", systemWideValues, currentDate));
        var belowMinimumBidFailures = belowMinimumBids.Select(x => new FailedPickupBid(x, "Bid is below the minimum bid amount.", systemWideValues, currentDate));
        List<FailedPickupBid> noSpaceLeftBidFailures = new List<FailedPickupBid>();
        foreach (var noSpaceLeftBid in noSpaceLeftBids)
        {
            FailedPickupBid failedBid;
            if (noSpaceLeftBid.ConditionalDropPublisherGame.HasValue && noSpaceLeftBid.ConditionalDropResult.Result.IsFailure)
            {
                failedBid = new FailedPickupBid(noSpaceLeftBid, "No roster spots available. Attempted to conditionally drop game: " +
                                                                $"{noSpaceLeftBid.ConditionalDropPublisherGame.Value.MasterGame.Value.MasterGame.GameName} " +
                                                                $"but failed because: {noSpaceLeftBid.ConditionalDropResult.Result.Error}", systemWideValues, currentDate);
            }
            else
            {
                failedBid = new FailedPickupBid(noSpaceLeftBid, "No roster spots available.", systemWideValues, currentDate);
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

    private IReadOnlyList<SucceededPickupBid> GetWinnableBids(LeagueYear leagueYear, IReadOnlyList<ValidPickupBid> activeBidsForLeagueYear, SystemWideValues systemWideValues, LocalDate currentDate)
    {
        List<SucceededPickupBid> winnableBids = new List<SucceededPickupBid>();
        var groupedByGame = activeBidsForLeagueYear.GroupBy(x => x.PickupBid.MasterGame);
        foreach (var gameGroup in groupedByGame)
        {
            var bestBid = GetWinningBidForGame(gameGroup.Key, leagueYear, gameGroup, systemWideValues, currentDate);
            winnableBids.Add(bestBid);
        }

        return winnableBids;
    }

    private SucceededPickupBid GetWinningBidForGame(MasterGame masterGame, LeagueYear leagueYear, IEnumerable<ValidPickupBid> bidsForGame, SystemWideValues systemWideValues, LocalDate currentDate)
    {
        if (bidsForGame.Count() == 1)
        {
            var singleBid = bidsForGame.First();
            return new SucceededPickupBid(singleBid.PickupBid, singleBid.SlotNumber, "No competing bids for this game.", systemWideValues, currentDate);
        }

        var bestBids = bidsForGame.WhereMax(x => x.PickupBid.BidAmount).ToList();
        if (bestBids.Count == 1)
        {
            var singleBid = bestBids.Single();
            return new SucceededPickupBid(singleBid.PickupBid, singleBid.SlotNumber, "This bid was the highest bid.", systemWideValues, currentDate);
        }

        IEnumerable<ValidPickupBid> remainingGamesAfterTiebreaks;
        if (leagueYear.Options.TiebreakSystem.Equals(TiebreakSystem.LowestProjectedPoints))
        {
            bool countIneligibleGames = leagueYear.Options.HasSpecialSlots();
            var bestBidsByProjectedScore = bestBids.WhereMin(x => x.PickupBid.Publisher.GetProjectedFantasyPoints(systemWideValues, false, currentDate, countIneligibleGames, leagueYear)).ToList();
            if (bestBidsByProjectedScore.Count == 1)
            {
                var singleBid = bestBidsByProjectedScore.Single();
                return new SucceededPickupBid(singleBid.PickupBid, singleBid.SlotNumber, "This publisher has the lowest projected points. (Not including this game)", systemWideValues, currentDate);
            }

            var bestBidsByBidTime = bestBidsByProjectedScore.WhereMin(x => x.PickupBid.Timestamp).ToList();
            if (bestBidsByBidTime.Count == 1)
            {
                var singleBid = bestBidsByBidTime.Single();
                return new SucceededPickupBid(singleBid.PickupBid, singleBid.SlotNumber, "This bid was placed earliest. (Projected points were tied)", systemWideValues, currentDate);
            }

            remainingGamesAfterTiebreaks = bestBidsByBidTime;
        }
        else if (leagueYear.Options.TiebreakSystem.Equals(TiebreakSystem.EarliestBid))
        {
            var bestBidsByBidTime = bestBids.WhereMin(x => x.PickupBid.Timestamp).ToList();
            if (bestBidsByBidTime.Count == 1)
            {
                var singleBid = bestBidsByBidTime.Single();
                return new SucceededPickupBid(singleBid.PickupBid, singleBid.SlotNumber, "This bid was placed earliest.", systemWideValues, currentDate);
            }

            bool countIneligibleGames = leagueYear.Options.HasSpecialSlots();
            var bestBidsByProjectedScore = bestBidsByBidTime.WhereMin(x => x.PickupBid.Publisher.GetProjectedFantasyPoints(systemWideValues, false, currentDate, countIneligibleGames, leagueYear)).ToList();
            if (bestBidsByProjectedScore.Count == 1)
            {
                var singleBid = bestBidsByProjectedScore.Single();
                return new SucceededPickupBid(singleBid.PickupBid, singleBid.SlotNumber, "This publisher has the lowest projected points. (Not including this game) (Bid placement time was tied)", systemWideValues, currentDate);
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
            return new SucceededPickupBid(singleBid.PickupBid, singleBid.SlotNumber, "This publisher had the latest draft position (Projected points and Bid placement time were tied", systemWideValues, currentDate);
        }

        throw new Exception($"Inconceivable tie situation for game: {masterGame.GameName} for league year: {leagueYear}");
    }

    private static IReadOnlyList<SucceededPickupBid> GetWinningBids(IReadOnlyList<SucceededPickupBid> winnableBids)
    {
        List<SucceededPickupBid> winningBids = new List<SucceededPickupBid>();
        var groupedByPublisher = winnableBids.GroupBy(x => x.PickupBid.Publisher);
        foreach (var publisherGroup in groupedByPublisher)
        {
            SucceededPickupBid winningBid = publisherGroup.WhereMin(x => x.PickupBid.Priority).First();
            winningBids.Add(winningBid);
        }

        return winningBids;
    }

    private static ActionProcessingResults GetBidProcessingResults(IReadOnlyList<SucceededPickupBid> successBids, IReadOnlyList<FailedPickupBid> failedBids, PublisherStateSet publisherStateSet,
        Instant processingTime, IReadOnlyDictionary<Guid, MasterGameYear> masterGameYearDictionary)
    {
        List<PublisherGame> gamesToAdd = new List<PublisherGame>();
        List<LeagueAction> leagueActions = new List<LeagueAction>();
        foreach (var successBid in successBids)
        {
            var masterGameYear = masterGameYearDictionary[successBid.PickupBid.MasterGame.MasterGameID];
            PublisherGame newPublisherGame = new PublisherGame(successBid.PickupBid.Publisher.PublisherID, Guid.NewGuid(), successBid.PickupBid.MasterGame.GameName, processingTime,
                successBid.PickupBid.CounterPick, null, false, null, masterGameYear, successBid.SlotNumber, null, null, successBid.PickupBid.BidAmount, null);
            gamesToAdd.Add(newPublisherGame);
            var affectedPublisher = publisherStateSet.GetPublisher(successBid.PickupBid.Publisher.PublisherID);
            publisherStateSet.AcquireGameForPublisher(affectedPublisher, newPublisherGame, successBid.PickupBid.BidAmount);

            LeagueAction leagueAction = new LeagueAction(successBid.PickupBid, processingTime);
            leagueActions.Add(leagueAction);
        }

        foreach (var failedBid in failedBids)
        {
            LeagueAction leagueAction = new LeagueAction(failedBid, processingTime);
            leagueActions.Add(leagueAction);
        }

        List<FormerPublisherGame> conditionalDroppedGames = new List<FormerPublisherGame>();
        var successfulConditionalDrops = successBids.Where(x => x.PickupBid.ConditionalDropPublisherGame.HasValue && x.PickupBid.ConditionalDropResult.Result.IsSuccess);
        foreach (var successfulConditionalDrop in successfulConditionalDrops)
        {
            var affectedPublisher = publisherStateSet.GetPublisher(successfulConditionalDrop.PickupBid.Publisher.PublisherID);
            publisherStateSet.DropGameForPublisher(affectedPublisher, successfulConditionalDrop.PickupBid.ConditionalDropPublisherGame.Value, successfulConditionalDrop.PickupBid.LeagueYear.Options);
            var formerPublisherGame = successfulConditionalDrop.PickupBid.ConditionalDropPublisherGame.Value.GetFormerPublisherGame(processingTime, $"Conditionally dropped while picking up: {successfulConditionalDrop.PickupBid.MasterGame.GameName}");
            conditionalDroppedGames.Add(formerPublisherGame);
        }

        ActionProcessingResults bidProcessingResults = ActionProcessingResults.GetResultsSetFromBidResults(successBids, failedBids,
            leagueActions, publisherStateSet, gamesToAdd, conditionalDroppedGames);
        return bidProcessingResults;
    }
}
