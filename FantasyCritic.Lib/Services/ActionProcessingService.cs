using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.LeagueActions;
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

        public ActionProcessingResults ProcessActions(SystemWideValues systemWideValues, IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> allActiveBids, 
            IReadOnlyDictionary<LeagueYear, IReadOnlyList<DropRequest>> allActiveDrops, IEnumerable<Publisher> currentPublisherStates, IClock clock)
        {
            if (!allActiveBids.Any() && !allActiveDrops.Any())
            {
                return ActionProcessingResults.GetEmptyResultsSet(currentPublisherStates);
            }

            //Do standard drops
            ActionProcessingResults currentResults = ProcessDrops(allActiveDrops, currentPublisherStates, clock);
            if (!allActiveBids.Any())
            {
                return currentResults;
            }

            return currentResults;
        }

        private ActionProcessingResults ProcessDrops(IReadOnlyDictionary<LeagueYear, IReadOnlyList<DropRequest>> allDropRequests, 
            IEnumerable<Publisher> currentPublisherStates, IClock clock)
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
                        LeagueAction leagueAction = new LeagueAction(dropRequest, dropResult, clock.GetCurrentInstant());
                        affectedPublisher.DropGame(publisherGame.Value.WillRelease());

                        leagueActions.Add(leagueAction);
                    }
                    else
                    {
                        failedDrops.Add(dropRequest);
                        LeagueAction leagueAction = new LeagueAction(dropRequest, dropResult, clock.GetCurrentInstant());
                        leagueActions.Add(leagueAction);
                    }
                }
            }

            ActionProcessingResults dropProcessingResults = ActionProcessingResults.GetResultsSetFromDropResults(successDrops, failedDrops, leagueActions, updatedPublisherStates, gamesToDelete);
            return dropProcessingResults;
        }
    }
}
