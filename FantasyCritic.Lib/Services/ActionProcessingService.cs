using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Utilities;
using MoreLinq;
using NodaTime;

namespace FantasyCritic.Lib.Services
{
    public class ActionProcessingService
    {
        private readonly GameAcquisitionService _gameAcquisitionService;

        public ActionProcessingService(GameAcquisitionService gameAcquisitionService)
        {
            _gameAcquisitionService = gameAcquisitionService;
        }

        public FinalizedActionProcessingResults ProcessActions(SystemWideValues systemWideValues, IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> allActiveBids, 
            IReadOnlyDictionary<LeagueYear, IReadOnlyList<DropRequest>> allActiveDrops, IEnumerable<Publisher> currentPublisherStates, Instant processingTime)
        {
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
                var emptyResults = ActionProcessingResults.GetEmptyResultsSet(currentPublisherStates);
                return new FinalizedActionProcessingResults(processSetID, processingTime, processName, emptyResults);
            }

            ActionProcessingResults dropResults = ProcessDrops(allActiveDrops, currentPublisherStates, processingTime);
            if (!allActiveBids.Any())
            {
                return new FinalizedActionProcessingResults(processSetID, processingTime, processName, dropResults);
            }

            ActionProcessingResults bidResults = ProcessPickupsIteration(systemWideValues, allActiveBids, dropResults, processingTime);
            return new FinalizedActionProcessingResults(processSetID, processingTime, processName, bidResults);
        }

        private ActionProcessingResults ProcessDrops(IReadOnlyDictionary<LeagueYear, IReadOnlyList<DropRequest>> allDropRequests, 
            IEnumerable<Publisher> currentPublisherStates, Instant processingTime)
        {
            List<Publisher> updatedPublisherStates = currentPublisherStates.ToList();
            List<PublisherGame> gamesToDelete = new List<PublisherGame>();
            List<LeagueAction> leagueActions = new List<LeagueAction>();
            List<DropRequest> successDrops = new List<DropRequest>();
            List<DropRequest> failedDrops = new List<DropRequest>();

            foreach (var leagueYearGroup in allDropRequests)
            {
                foreach (var dropRequest in leagueYearGroup.Value)
                {
                    var affectedPublisher = updatedPublisherStates.Single(x => x.PublisherID == dropRequest.Publisher.PublisherID);
                    var publishersInLeague = updatedPublisherStates.Where(x => x.LeagueYear.Equals(affectedPublisher.LeagueYear));
                    var otherPublishersInLeague = publishersInLeague.Except(new List<Publisher>() { affectedPublisher });

                    var dropResult = _gameAcquisitionService.CanDropGame(dropRequest, leagueYearGroup.Key, affectedPublisher, otherPublishersInLeague);
                    if (dropResult.Result.IsSuccess)
                    {
                        successDrops.Add(dropRequest);
                        var publisherGame = dropRequest.Publisher.GetPublisherGame(dropRequest.MasterGame);
                        gamesToDelete.Add(publisherGame.Value);
                        LeagueAction leagueAction = new LeagueAction(dropRequest, dropResult, processingTime);
                        affectedPublisher.DropGame(publisherGame.Value);

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

            ActionProcessingResults dropProcessingResults = ActionProcessingResults.GetResultsSetFromDropResults(successDrops, failedDrops, leagueActions, updatedPublisherStates, gamesToDelete);
            return dropProcessingResults;
        }

        private ActionProcessingResults ProcessPickupsIteration(SystemWideValues systemWideValues, IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> allActiveBids,
            ActionProcessingResults existingResults, Instant processingTime)
        {
            IEnumerable<PickupBid> flatAllBids = allActiveBids.SelectMany(x => x.Value);

            var currentPublisherStates = existingResults.UpdatedPublishers;
            var processedBids = new ProcessedBidSet();
            foreach (var leagueYear in allActiveBids)
            {
                if (!leagueYear.Value.Any())
                {
                    continue;
                }

                var processedBidsForLeagueYear = ProcessPickupsForLeagueYear(leagueYear.Key, leagueYear.Value, currentPublisherStates, systemWideValues, processingTime);
                processedBids = processedBids.AppendSet(processedBidsForLeagueYear);
            }

            ActionProcessingResults bidResults = GetBidProcessingResults(processedBids.SuccessBids, processedBids.FailedBids, currentPublisherStates, processingTime);
            var newResults = existingResults.Combine(bidResults);
            var remainingBids = flatAllBids.Except(processedBids.ProcessedBids);
            if (remainingBids.Any())
            {
                IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> remainingBidDictionary = remainingBids.GroupToDictionary(x => x.LeagueYear);
                var subProcessingResults = ProcessPickupsIteration(systemWideValues, remainingBidDictionary, newResults, processingTime);
                ActionProcessingResults combinedResults = newResults.Combine(subProcessingResults);
                return combinedResults;
            }

            return newResults;
        }

        private ProcessedBidSet ProcessPickupsForLeagueYear(LeagueYear leagueYear, IReadOnlyList<PickupBid> activeBidsForLeague,
            IReadOnlyList<Publisher> currentPublisherStates, SystemWideValues systemWideValues, Instant processingTime)
        {
            var gamesGroupedByPublisherAndGame = activeBidsForLeague.GroupBy(x => (x.Publisher.PublisherID, x.MasterGame.MasterGameID));
            var duplicateBidGroups = gamesGroupedByPublisherAndGame.Where(x => x.Count() > 1).ToList();
            List<PickupBid> duplicateBids = new List<PickupBid>();
            foreach (var duplicateBidGroup in duplicateBidGroups)
            {
                var bestBid = duplicateBidGroup.MaxBy(x => x.BidAmount).MinBy(x => x.Timestamp).FirstOrDefault();
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
                var currentPublisherStatesForLeagueYear = currentPublisherStates.Where(x => x.LeagueYear.Key.Equals(leagueYear.Key)).ToList();
                Publisher publisher = currentPublisherStatesForLeagueYear.Single(x => x.PublisherID == activeBid.Publisher.PublisherID);
                bool counterPickedGameIsManualWillNotRelease = PlayerGameExtensions.CounterPickedGameIsManualWillNotRelease(leagueYear, currentPublisherStatesForLeagueYear, activeBid.CounterPick, activeBid.MasterGame, true);
                var gameRequest = new ClaimGameDomainRequest(publisher, activeBid.MasterGame.GameName, activeBid.CounterPick, counterPickedGameIsManualWillNotRelease, false, false, activeBid.MasterGame, null, null);

                int? validConditionalDropSlot = null;
                if (activeBid.ConditionalDropPublisherGame.HasValue)
                {
                    var otherPublishersInLeague = currentPublisherStatesForLeagueYear.Except(new List<Publisher>() { publisher });
                    activeBid.ConditionalDropResult = _gameAcquisitionService.CanConditionallyDropGame(activeBid, leagueYear, publisher, otherPublishersInLeague, processingTime);
                    if (activeBid.ConditionalDropResult.Result.IsSuccess)
                    {
                        validConditionalDropSlot = activeBid.ConditionalDropPublisherGame.Value.SlotNumber;
                    }
                }

                var claimResult = _gameAcquisitionService.CanClaimGame(gameRequest, leagueYear, currentPublisherStatesForLeagueYear, null, validConditionalDropSlot, false);
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

                if (activeBid.BidAmount > publisher.Budget)
                {
                    insufficientFundsBids.Add(activeBid);
                }

                if (activeBid.BidAmount < leagueYear.Options.MinimumBidAmount)
                {
                    belowMinimumBids.Add(activeBid);
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

            var duplicateBidFailures = duplicateBids.Select(x => new FailedPickupBid(x, "You cannot have multiple bids for the same game.", systemWideValues, currentDate));
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

            var enoughBudgetBids = activeBidsForLeagueYear.Where(x => x.PickupBid.BidAmount <= x.PickupBid.Publisher.Budget);
            var groupedByGame = enoughBudgetBids.GroupBy(x => x.PickupBid.MasterGame);
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

            var bestBids = bidsForGame.MaxBy(x => x.PickupBid.BidAmount).ToList();
            if (bestBids.Count == 1)
            {
                var singleBid = bestBids.Single();
                return new SucceededPickupBid(singleBid.PickupBid, singleBid.SlotNumber, "This bid was the highest bid.", systemWideValues, currentDate);
            }

            bool countIneligibleGames = leagueYear.Options.HasSpecialSlots();
            var bestBidsByProjectedScore = bestBids.MinBy(x => x.PickupBid.Publisher.GetProjectedFantasyPoints(systemWideValues, false, currentDate, countIneligibleGames)).ToList();
            if (bestBidsByProjectedScore.Count == 1)
            {
                var singleBid = bestBidsByProjectedScore.Single();
                return new SucceededPickupBid(singleBid.PickupBid, singleBid.SlotNumber, "This publisher has the lowest projected points. (Before this round of bids)", systemWideValues, currentDate);
            }

            var bestBidsByBidTime = bestBidsByProjectedScore.OrderBy(x => x.PickupBid.Timestamp).ToList();
            if (bestBidsByBidTime.Count == 1)
            {
                var singleBid = bestBidsByBidTime.Single();
                return new SucceededPickupBid(singleBid.PickupBid, singleBid.SlotNumber, "This bid was placed earliest. (Projected points were tied)", systemWideValues, currentDate);
            }

            var bestBidsByDraftPosition = bestBidsByProjectedScore.OrderByDescending(x => x.PickupBid.Publisher.DraftPosition).ToList();
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
                SucceededPickupBid winningBid = publisherGroup.MinBy(x => x.PickupBid.Priority).First();
                winningBids.Add(winningBid);
            }

            return winningBids;
        }

        private static ActionProcessingResults GetBidProcessingResults(IReadOnlyList<SucceededPickupBid> successBids, IReadOnlyList<FailedPickupBid> failedBids, IReadOnlyList<Publisher> publishers, Instant processingTime)
        {
            Dictionary<Guid, Publisher> publisherDictionary = publishers.ToDictionary(x => x.PublisherID);
            List<PublisherGame> gamesToAdd = new List<PublisherGame>();
            List<LeagueAction> leagueActions = new List<LeagueAction>();
            foreach (var successBid in successBids)
            {
                PublisherGame newPublisherGame = new PublisherGame(successBid.PickupBid.Publisher.PublisherID, Guid.NewGuid(), successBid.PickupBid.MasterGame.GameName, processingTime,
                    successBid.PickupBid.CounterPick, null, false, null, new MasterGameYear(successBid.PickupBid.MasterGame, successBid.PickupBid.Publisher.LeagueYear.Year), successBid.SlotNumber, null, null);
                gamesToAdd.Add(newPublisherGame);
                var affectedPublisher = publisherDictionary[successBid.PickupBid.Publisher.PublisherID];
                affectedPublisher.AcquireGame(newPublisherGame, successBid.PickupBid.BidAmount);

                LeagueAction leagueAction = new LeagueAction(successBid.PickupBid, processingTime);
                leagueActions.Add(leagueAction);
            }

            foreach (var failedBid in failedBids)
            {
                LeagueAction leagueAction = new LeagueAction(failedBid, processingTime);
                leagueActions.Add(leagueAction);
            }

            List<PublisherGame> conditionalDroppedGames = new List<PublisherGame>();
            var successfulConditionalDrops = successBids.Where(x => x.PickupBid.ConditionalDropPublisherGame.HasValue && x.PickupBid.ConditionalDropResult.Result.IsSuccess);
            foreach (var successfulConditionalDrop in successfulConditionalDrops)
            {
                var affectedPublisher = publisherDictionary[successfulConditionalDrop.PickupBid.Publisher.PublisherID];
                affectedPublisher.DropGame(successfulConditionalDrop.PickupBid.ConditionalDropPublisherGame.Value);
                conditionalDroppedGames.Add(successfulConditionalDrop.PickupBid.ConditionalDropPublisherGame.Value);
            }

            var updatedPublishers = publisherDictionary.Values.ToList();
            ActionProcessingResults bidProcessingResults = ActionProcessingResults.GetResultsSetFromBidResults(successBids, failedBids, 
                leagueActions, updatedPublishers, gamesToAdd, conditionalDroppedGames);
            return bidProcessingResults;
        }
    }
}
