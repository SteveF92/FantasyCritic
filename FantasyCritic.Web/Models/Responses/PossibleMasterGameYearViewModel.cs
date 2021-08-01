using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Web.Models.RoundTrip;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class PossibleMasterGameYearViewModel
    {
        public PossibleMasterGameYearViewModel(PossibleMasterGameYear masterGame, LocalDate currentDate)
        {
            MasterGame = new MasterGameYearViewModel(masterGame.MasterGame, currentDate);
            Taken = masterGame.Taken;
            AlreadyOwned = masterGame.AlreadyOwned;
            IsEligible = masterGame.IsEligible;
            IsReleased = masterGame.IsReleased;
            WillRelease = masterGame.WillRelease;
            HasScore = masterGame.HasScore;
            IsAvailable = masterGame.IsAvailable;
        }


        public MasterGameYearViewModel MasterGame { get; }
        public bool Taken { get; }
        public bool AlreadyOwned { get; }
        public bool IsEligible { get; }
        public bool IsReleased { get; }
        public bool WillRelease { get; }
        public bool HasScore { get; }
        public bool IsAvailable { get; }

        public string Status
        {
            get
            {
                if (Taken)
                {
                    return "Taken";
                }
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
                if (!WillRelease)
                {
                    return "Will Not Release";
                }

                return "Available";
            }
        }
    }
}
