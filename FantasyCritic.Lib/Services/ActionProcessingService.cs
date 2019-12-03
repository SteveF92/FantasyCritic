using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Requests;
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

        public BidProcessingResults ProcessPickupsIteration(SystemWideValues systemWideValues, IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> allActiveBids,
            IEnumerable<Publisher> currentPublisherStates, IClock clock, IEnumerable<SupportedYear> supportedYears)
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

                var processedBidsForLeagueYear = ProcessPickupsForLeagueYear(leagueYear.Key, leagueYear.Value, currentPublisherStates, systemWideValues, supportedYears);
                processedBids = processedBids.AppendSet(processedBidsForLeagueYear);
            }

            BidProcessingResults bidProcessingResults = GetProcessingResults(processedBids.SuccessBids, processedBids.FailedBids, currentPublisherStates, clock);

            var remainingBids = flatAllBids.Except(processedBids.ProcessedBids);
            if (remainingBids.Any())
            {
                Dictionary<LeagueYear, IReadOnlyList<PickupBid>> remainingBidDictionary = remainingBids.GroupBy(x => x.LeagueYear).ToDictionary(x => x.Key, y => (IReadOnlyList<PickupBid>)y.ToList());
                var subProcessingResults = ProcessPickupsIteration(systemWideValues, remainingBidDictionary, bidProcessingResults.UpdatedPublishers, clock, supportedYears);
                BidProcessingResults combinedResults = bidProcessingResults.Combine(subProcessingResults);
                return combinedResults;
            }

            return bidProcessingResults;
        }

        private ProcessedBidSet ProcessPickupsForLeagueYear(LeagueYear leagueYear, IEnumerable<PickupBid> activeBidsForLeague,
            IEnumerable<Publisher> currentPublisherStates, SystemWideValues systemWideValues, IEnumerable<SupportedYear> supportedYears)
        {
            List<PickupBid> noSpaceLeftBids = new List<PickupBid>();
            List<PickupBid> insufficientFundsBids = new List<PickupBid>();
            List<PickupBid> invalidGameBids = new List<PickupBid>();

            foreach (var activeBid in activeBidsForLeague)
            {
                Publisher publisher = currentPublisherStates.Single(x => x.PublisherID == activeBid.Publisher.PublisherID);

                var gameRequest = new ClaimGameDomainRequest(publisher, activeBid.MasterGame.GameName, false, false, activeBid.MasterGame, null, null);
                var publishersForLeagueAndYear = currentPublisherStates.Where(x => x.LeagueYear.League.LeagueID == leagueYear.League.LeagueID && x.LeagueYear.Year == leagueYear.Year);
                var claimResult = _gameAcquisitionService.CanClaimGame(gameRequest, supportedYears, leagueYear, publishersForLeagueAndYear);

                if (!publisher.HasRemainingGameSpot(leagueYear.Options.StandardGames))
                {
                    noSpaceLeftBids.Add(activeBid);
                    continue;
                }

                if (!claimResult.Success)
                {
                    invalidGameBids.Add(activeBid);
                    continue;
                }
                
                if (activeBid.BidAmount > publisher.Budget)
                {
                    insufficientFundsBids.Add(activeBid);
                }
            }

            var validBids = activeBidsForLeague.Except(noSpaceLeftBids).Except(insufficientFundsBids).Except(invalidGameBids);
            var winnableBids = GetWinnableBids(validBids, leagueYear.Options, systemWideValues);
            var winningBids = GetWinningBids(winnableBids);

            var takenGames = winningBids.Select(x => x.MasterGame);
            var losingBids = activeBidsForLeague
                .Except(winningBids)
                .Except(noSpaceLeftBids)
                .Except(insufficientFundsBids)
                .Except(invalidGameBids)
                .Where(x => takenGames.Contains(x.MasterGame))
                .Select(x => new FailedPickupBid(x, "Publisher was outbid."));

            var invalidGameBidFailures = invalidGameBids.Select(x => new FailedPickupBid(x, "Game is no longer eligible (it probably has been released or at least has reviews)."));
            var insufficientFundsBidFailures = insufficientFundsBids.Select(x => new FailedPickupBid(x, "Not enough budget."));
            var noSpaceLeftBidFailures = noSpaceLeftBids.Select(x => new FailedPickupBid(x, "No roster spots available."));
            var failedBids = losingBids.Concat(insufficientFundsBidFailures).Concat(noSpaceLeftBidFailures).Concat(invalidGameBidFailures);

            var processedSet = new ProcessedBidSet(winningBids, failedBids);
            return processedSet;
        }

        private IReadOnlyList<PickupBid> GetWinnableBids(IEnumerable<PickupBid> activeBidsForLeagueYear, LeagueOptions options, SystemWideValues systemWideValues)
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
                    //TODO change projections
                    var bestBidsByProjectedScore = bestBids.MinBy(x => x.Publisher.GetProjectedFantasyPoints(options, systemWideValues, false, true, _clock));
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
                    false, null, null, new MasterGameYear(successBid.MasterGame, successBid.Publisher.LeagueYear.Year), null, null);
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

            BidProcessingResults bidProcessingResults = new BidProcessingResults(successBids, simpleFailedBids, leagueActions, updatedPublishers, gamesToAdd);
            return bidProcessingResults;
        }

        public DropProcessingResults ProcessDropsIteration(SystemWideValues systemWideValues, IReadOnlyDictionary<LeagueYear, IReadOnlyList<DropRequest>> allDropRequests, IReadOnlyList<Publisher> allPublishers, 
            IClock clock, IReadOnlyList<SupportedYear> supportedYears)
        {
            throw new NotImplementedException();
        }
    }
}
