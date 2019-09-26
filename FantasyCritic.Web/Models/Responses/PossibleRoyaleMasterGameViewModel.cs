using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Royale;
using FantasyCritic.Web.Models.RoundTrip;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class PossibleRoyaleMasterGameViewModel
    {
        public PossibleRoyaleMasterGameViewModel(MasterGameYear masterGame, IClock clock, RoyaleYearQuarter yearQuarter, bool alreadyOwned)
        {
            MasterGame = new MasterGameYearViewModel(masterGame, clock);
            WillReleaseInQuarter = masterGame.WillReleaseInQuarter(yearQuarter.YearQuarter);
            IsEligible = !EligibilitySettings.GetRoyaleEligibilitySettings().GameIsEligible(masterGame.MasterGame).Any();
            AlreadyOwned = alreadyOwned;
            Cost = masterGame.GetRoyaleGameCost();
        }

        public MasterGameYearViewModel MasterGame { get; }
        public bool WillReleaseInQuarter { get; }
        public bool IsEligible { get; }
        public bool AlreadyOwned { get; }
        public decimal Cost { get; }
    }
}
