using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class EligibilityOverride
    {
        public EligibilityOverride(MasterGame masterGame, bool eligible)
        {
            MasterGame = masterGame;
            Eligible = eligible;
        }

        public MasterGame MasterGame { get; }
        public bool Eligible { get; }
    }
}
