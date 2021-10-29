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

        public ActionProcessingResults ProcessActionsIteration(SystemWideValues systemWideValues, IReadOnlyDictionary<LeagueYear, GameActionSet> allActiveActions, IEnumerable<PublisherActionStatus> currentPublisherStates, IClock clock, IEnumerable<SupportedYear> supportedYears)
        {
            if (allActiveActions.All(x => !x.Value.Any()))
            {
                return ActionProcessingResults.GetEmptyResultsSet(currentPublisherStates);
            }

            //Do standard drops

            return ActionProcessingResults.GetEmptyResultsSet(currentPublisherStates);
        }
    }
}
