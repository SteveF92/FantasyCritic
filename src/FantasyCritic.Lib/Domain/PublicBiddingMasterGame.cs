using FantasyCritic.Lib.Domain.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class PublicBiddingMasterGame
    {
        public PublicBiddingMasterGame(MasterGameYear masterGameYear, bool counterPick, IEnumerable<ClaimError> claimErrors)
        {
            MasterGameYear = masterGameYear;
            CounterPick = counterPick;
            ClaimErrors = claimErrors.ToList();
        }

        public MasterGameYear MasterGameYear { get; }
        public bool CounterPick { get; }
        public IReadOnlyList<ClaimError> ClaimErrors { get; }
    }
}
