using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class MasterGameHypeScores
    {
        public MasterGameHypeScores(MasterGameYear masterGameYear, decimal hypeFactor, decimal dateAdjustedHypeFactor, decimal bidAdjustedHypeFactor, decimal linearRegressionHypeFactor)
        {
            MasterGameYear = masterGameYear;
            HypeFactor = hypeFactor;
            DateAdjustedHypeFactor = dateAdjustedHypeFactor;
            BidAdjustedHypeFactor = bidAdjustedHypeFactor;
            LinearRegressionHypeFactor = linearRegressionHypeFactor;
        }

        public MasterGameYear MasterGameYear { get; }
        public decimal HypeFactor { get; }
        public decimal DateAdjustedHypeFactor { get; }
        public decimal BidAdjustedHypeFactor { get; }
        public decimal LinearRegressionHypeFactor { get; }
    }
}
