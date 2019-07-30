using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class HypeConstants
    {
        public HypeConstants(decimal baseScore, decimal standardGameConstant, decimal counterPickConstant, decimal averageDraftPositionConstant, decimal averageBidAmountConstant)
        {
            BaseScore = baseScore;
            StandardGameConstant = standardGameConstant;
            CounterPickConstant = counterPickConstant;
            AverageDraftPositionConstant = averageDraftPositionConstant;
            AverageBidAmountConstant = averageBidAmountConstant;
        }

        public decimal BaseScore { get; }
        public decimal StandardGameConstant { get; }
        public decimal CounterPickConstant { get; }
        public decimal AverageDraftPositionConstant { get; }
        public decimal AverageBidAmountConstant { get; }
    }
}
