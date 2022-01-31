using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class PublicBiddingMasterGame
    {
        public PublicBiddingMasterGame(MasterGameYear masterGameYear, bool counterPick)
        {
            MasterGameYear = masterGameYear;
            CounterPick = counterPick;
        }

        public MasterGameYear MasterGameYear { get; }
        public bool CounterPick { get; }
    }
}
