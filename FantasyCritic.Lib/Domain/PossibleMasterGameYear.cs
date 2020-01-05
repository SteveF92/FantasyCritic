using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class PossibleMasterGameYear
    {
        public PossibleMasterGameYear(MasterGameYear masterGame, bool taken, bool alreadyOwned, bool isEligible)
        {
            MasterGame = masterGame;
            Taken = taken;
            AlreadyOwned = alreadyOwned;
            IsEligible = isEligible;
        }

        public MasterGameYear MasterGame { get; }
        public bool Taken { get; }
        public bool AlreadyOwned { get; }
        public bool IsEligible { get; }
    }
}
