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
        }


        public MasterGameYearViewModel MasterGame { get; }
        public bool Taken { get; }
        public bool AlreadyOwned { get; }
        public bool IsEligible { get; }
    }
}
