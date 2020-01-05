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
        public PossibleMasterGameYearViewModel(PossibleMasterGameYear masterGame, IClock clock)
        {
            MasterGame = new MasterGameYearViewModel(masterGame.MasterGame, clock);
            Taken = masterGame.Taken;
            AlreadyOwned = masterGame.AlreadyOwned;
            IsEligible = masterGame.IsEligible;
            IsReleased = masterGame.IsReleased;
            WillRelease = masterGame.WillRelease;
            HasScore = masterGame.IsEligible;
        }


        public MasterGameYearViewModel MasterGame { get; }
        public bool Taken { get; }
        public bool AlreadyOwned { get; }
        public bool IsEligible { get; }
        public bool IsReleased { get; }
        public bool WillRelease { get; }
        public bool HasScore { get; }
    }
}
