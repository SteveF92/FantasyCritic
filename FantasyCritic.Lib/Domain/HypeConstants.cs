using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class HypeConstants
    {
        public HypeConstants(double baseScore, double counterPickConstant, double bidPercentileConstant, double hypeFactorConstant)
        {
            BaseScore = baseScore;
            CounterPickConstant = counterPickConstant;
            BidPercentileConstant = bidPercentileConstant;
            HypeFactorConstant = hypeFactorConstant;
        }

        public double BaseScore { get; }
        public double CounterPickConstant { get; }
        public double BidPercentileConstant { get; }
        public double HypeFactorConstant { get; }

    }
}
