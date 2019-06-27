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
        public PossibleMasterGameYearViewModel(MasterGameYear masterGame, IClock clock, bool taken, bool isEligible)
        {
            MasterGame = new MasterGameYearViewModel(masterGame, clock);
            Taken = taken;
            IsEligible = isEligible;
        }

        public PossibleMasterGameYearViewModel(MasterSubGame masterSubGame, MasterGameYear masterGame, IClock clock, bool taken, bool isEligible)
        {
            MasterGame = new MasterGameYearViewModel(masterSubGame, masterGame, clock);
            Taken = taken;
            IsEligible = isEligible;
        }

        public MasterGameYearViewModel MasterGame { get; }
        public bool Taken { get; }
        public bool IsEligible { get; }

    }
}
