﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Extensions;
using MoreLinq;
using NodaTime;

namespace FantasyCritic.Lib.Services
{
    public class ActionProcessingService
    {
        private readonly GameAcquisitionService _gameAcquisitionService;
        private readonly IClock _clock;

        public ActionProcessingService(GameAcquisitionService gameAcquisitionService, IClock clock)
        {
            _gameAcquisitionService = gameAcquisitionService;
            _clock = clock;
        }

        public ActionProcessingResults ProcessPickupsIteration(SystemWideValues systemWideValues, IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> allActiveBids,
            IEnumerable<Publisher> currentPublisherStates, IClock clock, IEnumerable<SupportedYear> supportedYears)
        {
            if (!allActiveBids.Any())
            {
                return new ActionProcessingResults(new List<PickupBid>(), new List<PickupBid>(), new List<LeagueAction>(), currentPublisherStates, new List<PublisherGame>());
            }

            IEnumerable<PickupBid> flatAllBids = allActiveBids.SelectMany(x => x.Value);

            var processedBids = new ProcessedBidSet();
            foreach (var leagueYear in allActiveBids)
            {
                if (!leagueYear.Value.Any())
                {
                    continue;
                }

                var processedBidsForLeagueYear = ProcessPickupsForLeagueYear(leagueYear.Key, leagueYear.Value, currentPublisherStates, systemWideValues, supportedYears);
                processedBids = processedBids.AppendSet(processedBidsForLeagueYear);
            }

            ActionProcessingResults actionProcessingResults = GetProcessingResults(processedBids.SuccessBids, processedBids.FailedBids, currentPublisherStates, clock);

            var remainingBids = flatAllBids.Except(processedBids.ProcessedBids);
            if (remainingBids.Any())
            {
                Dictionary<LeagueYear, IReadOnlyList<PickupBid>> remainingBidDictionary = remainingBids.GroupBy(x => x.LeagueYear).ToDictionary(x => x.Key, y => (IReadOnlyList<PickupBid>)y.ToList());
                var subProcessingResults = ProcessPickupsIteration(systemWideValues, remainingBidDictionary, actionProcessingResults.UpdatedPublishers, clock, supportedYears);
                ActionProcessingResults combinedResults = actionProcessingResults.Combine(subProcessingResults);
                return combinedResults;
            }

            return actionProcessingResults;
        }

        private ProcessedBidSet ProcessPickupsForLeagueYear(LeagueYear leagueYear, IEnumerable<PickupBid> activeBidsForLeague,
            IEnumerable<Publisher> currentPublisherStates, SystemWideValues systemWideValues, IEnumerable<SupportedYear> supportedYears)
        {
            List<PickupBid> noSpaceLeftBids = new List<PickupBid>();
            List<PickupBid> insufficientFundsBids = new List<PickupBid>();
            List<PickupBid> belowMinimumBids = new List<PickupBid>();
            List<KeyValuePair<PickupBid, string>> invalidGameBids = new List<KeyValuePair<PickupBid, string>>();

            foreach (var activeBid in activeBidsForLeague)
            {
                Publisher publisher = currentPublisherStates.Single(x => x.PublisherID == activeBid.Publisher.PublisherID);

                var gameRequest = new ClaimGameDomainRequest(publisher, activeBid.MasterGame.GameName, false, false, false, activeBid.MasterGame, null, null);
                var publishersForLeagueAndYear = currentPublisherStates.Where(x => x.LeagueYear.League.LeagueID == leagueYear.League.LeagueID && x.LeagueYear.Year == leagueYear.Year);
                var claimResult = _gameAcquisitionService.CanClaimGame(gameRequest, supportedYears, leagueYear, publishersForLeagueAndYear, null, false);

                if (!publisher.HasRemainingGameSpot(leagueYear.Options.StandardGames))
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
            }

            var validBids = activeBidsForLeague
                .Except(noSpaceLeftBids)
                .Except(insufficientFundsBids)
                .Except(belowMinimumBids)
                .Except(invalidGameBids.Select(x => x.Key));
            var winnableBids = GetWinnableBids(validBids, leagueYear.Options, systemWideValues);
            var winningBids = GetWinningBids(winnableBids);

            var takenGames = winningBids.Select(x => x.MasterGame);
            var losingBids = activeBidsForLeague
                .Except(winningBids)
                .Except(noSpaceLeftBids)
                .Except(insufficientFundsBids)
                .Except(belowMinimumBids)
                .Except(invalidGameBids.Select(x => x.Key))
                .Where(x => takenGames.Contains(x.MasterGame))
                .Select(x => new FailedPickupBid(x, "Publisher was outbid."));

            var invalidGameBidFailures = invalidGameBids.Select(x => new FailedPickupBid(x.Key, "Game is no longer eligible: " + x.Value));
            var insufficientFundsBidFailures = insufficientFundsBids.Select(x => new FailedPickupBid(x, "Not enough budget."));
            var belowMinimumBidFailures = belowMinimumBids.Select(x => new FailedPickupBid(x, "Bid is below the minimum bid amount."));
            var noSpaceLeftBidFailures = noSpaceLeftBids.Select(x => new FailedPickupBid(x, "No roster spots available."));
            var failedBids = losingBids
                .Concat(insufficientFundsBidFailures)
                .Concat(belowMinimumBidFailures)
                .Concat(noSpaceLeftBidFailures)
                .Concat(invalidGameBidFailures);

            var processedSet = new ProcessedBidSet(winningBids, failedBids);
            return processedSet;
        }

        private IReadOnlyList<PickupBid> GetWinnableBids(IEnumerable<PickupBid> activeBidsForLeagueYear, LeagueOptions options, SystemWideValues systemWideValues)
        {
            List<PickupBid> winnableBids = new List<PickupBid>();

            var enoughBudgetBids = activeBidsForLeagueYear.Where(x => x.BidAmount <= x.Publisher.Budget);
            var groupedByGame = enoughBudgetBids.GroupBy(x => x.MasterGame);
            var currentDate = _clock.GetToday();
            foreach (var gameGroup in groupedByGame)
            {
                PickupBid bestBid;
                if (gameGroup.Count() == 1)
                {
                    bestBid = gameGroup.First();
                }
                else
                {
                    var bestBids = gameGroup.MaxBy(x => x.BidAmount);
                    var bestBidsByProjectedScore = bestBids.MinBy(x => x.Publisher.GetProjectedFantasyPoints(options, systemWideValues, false, false, currentDate));
                    bestBid = bestBidsByProjectedScore.OrderBy(x => x.Timestamp).ThenByDescending(x => x.Publisher.DraftPosition).First();
                }

                winnableBids.Add(bestBid);
            }

            return winnableBids;
        }

        private static IReadOnlyList<PickupBid> GetWinningBids(IEnumerable<PickupBid> winnableBids)
        {
            List<PickupBid> winningBids = new List<PickupBid>();
            var groupedByPublisher = winnableBids.GroupBy(x => x.Publisher);
            foreach (var publisherGroup in groupedByPublisher)
            {
                PickupBid winningBid = publisherGroup.MinBy(x => x.Priority).First();
                winningBids.Add(winningBid);
            }

            return winningBids;
        }

        private static ActionProcessingResults GetProcessingResults(IEnumerable<PickupBid> successBids, IEnumerable<FailedPickupBid> failedBids, IEnumerable<Publisher> publishers, IClock clock)
        {
            List<Publisher> updatedPublishers = publishers.ToList();
            List<PublisherGame> gamesToAdd = new List<PublisherGame>();
            List<LeagueAction> leagueActions = new List<LeagueAction>();
            foreach (var successBid in successBids)
            {
                PublisherGame newPublisherGame = new PublisherGame(successBid.Publisher.PublisherID, Guid.NewGuid(), successBid.MasterGame.GameName, clock.GetCurrentInstant(),
                    false, null, false, null, new MasterGameYear(successBid.MasterGame, successBid.Publisher.LeagueYear.Year), null, null, false);
                gamesToAdd.Add(newPublisherGame);
                var affectedPublisher = updatedPublishers.Single(x => x.PublisherID == successBid.Publisher.PublisherID);
                affectedPublisher.AcquireGame(newPublisherGame, successBid.BidAmount);

                LeagueAction leagueAction = new LeagueAction(successBid, clock.GetCurrentInstant());
                leagueActions.Add(leagueAction);
            }

            foreach (var failedBid in failedBids)
            {
                LeagueAction leagueAction = new LeagueAction(failedBid, clock.GetCurrentInstant());
                leagueActions.Add(leagueAction);
            }

            var simpleFailedBids = failedBids.Select(x => x.PickupBid);

            ActionProcessingResults actionProcessingResults = new ActionProcessingResults(successBids, simpleFailedBids, leagueActions, updatedPublishers, gamesToAdd);
            return actionProcessingResults;
        }
    }
}
