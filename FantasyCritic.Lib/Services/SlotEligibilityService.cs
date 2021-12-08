using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;

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
            return GetIdealSlotForGame(publisher, eligibilityFactors, false).HasValue;
        }

        public static Maybe<PublisherSlot> GetIdealSlotForGame(Publisher publisher, Maybe<MasterGameWithEligibilityFactors> eligibilityFactors, bool counterPick)
        {
            throw new NotImplementedException();
        }
    }
}
