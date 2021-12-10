using System.Collections.Generic;
using System.Linq;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Royale;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses.Royale
{
    public class PossibleRoyaleMasterGameViewModel
    {
        public PossibleRoyaleMasterGameViewModel(MasterGameYear masterGame, LocalDate currentDate, RoyaleYearQuarter yearQuarter, bool alreadyOwned, IEnumerable<MasterGameTag> masterGameTags)
        {
            MasterGame = new MasterGameYearViewModel(masterGame, currentDate);
            WillReleaseInQuarter = masterGame.WillReleaseInQuarter(yearQuarter.YearQuarter);
            IsEligible = LeagueTagExtensions.GameIsRoyaleEligible(masterGameTags, masterGame.MasterGame, currentDate).Any();
            AlreadyOwned = alreadyOwned;
            Cost = masterGame.GetRoyaleGameCost();
        }

        public MasterGameYearViewModel MasterGame { get; }
        public bool WillReleaseInQuarter { get; }
        public bool IsEligible { get; }
        public bool AlreadyOwned { get; }
        public decimal Cost { get; }
        public bool IsAvailable => WillReleaseInQuarter && !AlreadyOwned && IsEligible;

        public string Status
        {
            get
            {
                if (AlreadyOwned)
                {
                    return "Already Owned";
                }
                if (!WillReleaseInQuarter)
                {
                    return "Will Not Release";
                }
                if (!IsEligible)
                {
                    return "Ineligible";
                }
                
                return "Available";
            }
        }
    }
}
