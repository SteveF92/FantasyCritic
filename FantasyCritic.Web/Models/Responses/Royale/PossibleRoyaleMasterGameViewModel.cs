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
            AlreadyOwned = alreadyOwned;
            IsEligible = !LeagueTagExtensions.GetRoyaleClaimErrors(masterGameTags, masterGame.MasterGame, currentDate).Any();
            IsReleased = masterGame.MasterGame.IsReleased(currentDate);
            HasScore = masterGame.MasterGame.CriticScore.HasValue;
            Cost = masterGame.GetRoyaleGameCost();
        }

        public MasterGameYearViewModel MasterGame { get; }
        public bool WillReleaseInQuarter { get; }
        public decimal Cost { get; }
        public bool AlreadyOwned { get; }
        public bool IsEligible { get; }
        public bool IsReleased { get; }
        public bool HasScore { get; }
        public bool IsAvailable => !AlreadyOwned && IsEligible && !IsReleased && !HasScore && WillReleaseInQuarter;

        public string Status
        {
            get
            {

                if (AlreadyOwned)
                {
                    return "Already Owned";
                }
                if (IsReleased)
                {
                    return "Released";
                }
                if (HasScore)
                {
                    return "Has Score";
                }
                if (!IsEligible)
                {
                    return "Ineligible";
                }
                if (!WillReleaseInQuarter)
                {
                    return "Will Not Release";
                }

                return "Available";
            }
        }
    }
}
