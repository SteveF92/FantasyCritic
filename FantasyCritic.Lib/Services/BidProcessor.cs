using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using MoreLinq;
using NodaTime;

namespace FantasyCritic.Lib.Services
{
    public static class BidProcessor
    {
        public static BidProcessingResults ProcessPickupsIteration(SystemWideValues systemWideValues, IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> allActiveBids,
            IEnumerable<Publisher> currentPublisherStates, IClock clock)
        {
            if (!allActiveBids.Any())
            {
                return new BidProcessingResults(new List<PickupBid>(), new List<PickupBid>(), new List<LeagueAction>(), currentPublisherStates, new List<PublisherGame>());
            }

            IEnumerable<PickupBid> flatAllBids = allActiveBids.SelectMany(x => x.Value);

            var processedBids = new ProcessedBidSet();
            foreach (var leagueYear in allActiveBids)
            {
                if (!leagueYear.Value.Any())
                {
                    continue;
                }
                var processedBidsForLeagueYear = ProcessPickupsForLeagueYear(leagueYear.Key, leagueYear.Value, systemWideValues);
                processedBids = processedBids.AppendSet(processedBidsForLeagueYear);
            }

            BidProcessingResults bidProcessingResults = GetProcessingResults(processedBids.SuccessBids, processedBids.FailedBids, currentPublisherStates, clock);

            var remainingBids = flatAllBids.Except(processedBids.ProcessedBids);
            if (remainingBids.Any())
            {
                Dictionary<LeagueYear, IReadOnlyList<PickupBid>> remainingBidDictionary = remainingBids.GroupBy(x => x.LeagueYear).ToDictionary(x => x.Key, y => (IReadOnlyList<PickupBid>)y.ToList());
                var subProcessingResults = ProcessPickupsIteration(systemWideValues, remainingBidDictionary, bidProcessingResults.UpdatedPublishers, clock);
                BidProcessingResults combinedResults = bidProcessingResults.Combine(subProcessingResults);
                return combinedResults;
            }

            return bidProcessingResults;
        }

        private static ProcessedBidSet ProcessPickupsForLeagueYear(LeagueYear leagueYear, IEnumerable<PickupBid> activeBidsForLeague, SystemWideValues systemWideValues)
        {
            var noSpaceLeftBids = activeBidsForLeague.Where(x => !x.Publisher.HasRemainingGameSpot(leagueYear.Options.StandardGames));
            var insufficientFundsBids = activeBidsForLeague.Where(x => x.BidAmount > x.Publisher.Budget);

            var validBids = activeBidsForLeague.Except(noSpaceLeftBids).Except(insufficientFundsBids);
            var winnableBids = GetWinnableBids(validBids, leagueYear.Options, systemWideValues);
            var winningBids = GetWinningBids(winnableBids);

            var takenGames = winningBids.Select(x => x.MasterGame);
            var losingBids = activeBidsForLeague
                .Except(winningBids)
                .Except(noSpaceLeftBids)
                .Except(insufficientFundsBids)
                .Where(x => takenGames.Contains(x.MasterGame))
                .Select(x => new FailedPickupBid(x, "Publisher was outbid."));

            var insufficientFundsBidFailures = insufficientFundsBids.Select(x => new FailedPickupBid(x, "Not enough budget."));
            var noSpaceLeftBidFailures = noSpaceLeftBids.Select(x => new FailedPickupBid(x, "No roster spots available."));
            var failedBids = losingBids.Concat(insufficientFundsBidFailures).Concat(noSpaceLeftBidFailures);

            var processedSet = new ProcessedBidSet(winningBids, failedBids);
            return processedSet;
        }

        private static IReadOnlyList<PickupBid> GetWinnableBids(IEnumerable<PickupBid> activeBidsForLeagueYear, LeagueOptions options, SystemWideValues systemWideValues)
        {
            List<PickupBid> winnableBids = new List<PickupBid>();

            var enoughBudgetBids = activeBidsForLeagueYear.Where(x => x.BidAmount <= x.Publisher.Budget);
            var groupedByGame = enoughBudgetBids.GroupBy(x => x.MasterGame);
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
                    var bestBidsByProjectedScore = bestBids.MinBy(x => x.Publisher.GetProjectedFantasyPoints(options, systemWideValues, false));
                    bestBid = bestBidsByProjectedScore.OrderByDescending(x => x.Publisher.DraftPosition).First();
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

        private static BidProcessingResults GetProcessingResults(IEnumerable<PickupBid> successBids, IEnumerable<FailedPickupBid> failedBids, IEnumerable<Publisher> publishers, IClock clock)
        {
            List<Publisher> updatedPublishers = publishers.ToList();
            List<PublisherGame> gamesToAdd = new List<PublisherGame>();
            List<LeagueAction> leagueActions = new List<LeagueAction>();
            foreach (var successBid in successBids)
            {
                PublisherGame newPublisherGame = new PublisherGame(successBid.Publisher.PublisherID, Guid.NewGuid(), successBid.MasterGame.GameName, clock.GetCurrentInstant(),
                    false, null, null, new MasterGameYear(successBid.MasterGame, successBid.Publisher.Year), null, null, successBid.Publisher.Year);
                gamesToAdd.Add(newPublisherGame);
                var affectedPublisher = updatedPublishers.Single(x => x.PublisherID == successBid.Publisher.PublisherID);
                affectedPublisher.SpendBudget(successBid.BidAmount);

                LeagueAction leagueAction = new LeagueAction(successBid, clock.GetCurrentInstant());
                leagueActions.Add(leagueAction);
            }

            foreach (var failedBid in failedBids)
            {
                LeagueAction leagueAction = new LeagueAction(failedBid, clock.GetCurrentInstant());
                leagueActions.Add(leagueAction);
            }

            var simpleFailedBids = failedBids.Select(x => x.PickupBid);

            BidProcessingResults bidProcessingResults = new BidProcessingResults(successBids, simpleFailedBids, leagueActions, updatedPublishers, gamesToAdd);
            return bidProcessingResults;
        }
    }
}
