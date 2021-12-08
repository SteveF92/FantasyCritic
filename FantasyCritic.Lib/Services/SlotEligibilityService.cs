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
        public static bool SlotIsCurrentlyValid(PublisherSlot publisherSlot, MasterGameEligibilityFactors masterGame)
        {
            throw new NotImplementedException();
        }

        public static bool GameIsEligibleInLeagueYear(LeagueYear leagueYear, MasterGameEligibilityFactors masterGame)
        {
            throw new NotImplementedException();
        }

        public static bool GameWouldBeEligibleInAnySlot(Publisher publisher, MasterGameEligibilityFactors masterGame)
        {
            throw new NotImplementedException();
        }

        public static Maybe<PublisherSlot> GetIdealSlotForGame(IEnumerable<PublisherSlot> currentSlots, Maybe<MasterGame> masterGame, bool counterPick)
        {
            throw new NotImplementedException();
        }
    }
}
