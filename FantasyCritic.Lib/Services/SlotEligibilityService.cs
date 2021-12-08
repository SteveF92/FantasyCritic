using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Results;

namespace FantasyCritic.Lib.Services
{
    public static class SlotEligibilityService
    {
        public static bool SlotIsCurrentlyValid(PublisherSlot publisherSlot, MasterGameWithEligibilityFactors eligibilityFactors)
        {
            throw new NotImplementedException();
        }

        public static bool GameIsEligibleInLeagueYear(LeagueYear leagueYear, MasterGameWithEligibilityFactors eligibilityFactors)
        {
            throw new NotImplementedException();
        }

        public static bool GameWouldBeEligibleInAnySlot(Publisher publisher, MasterGameWithEligibilityFactors eligibilityFactors)
        {
            throw new NotImplementedException();
        }

        public static PublisherSlotAcquisitionResult GetPublisherSlotAcquisitionResult(Publisher publisher, Maybe<MasterGameWithEligibilityFactors> eligibilityFactors, bool counterPick)
        {
            throw new NotImplementedException();
        }
    }
}
