using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.MySQL.Entities
{
    public class HypeConstantsEntity
    {
        public double BaseScore { get; set;  }
        public double CounterPickConstant { get; set; }
        public double BidPercentileConstant { get; set; }
        public double HypeFactorConstant { get; set; }

        public HypeConstants ToDomain()
        {
            return new HypeConstants(BaseScore, CounterPickConstant, BidPercentileConstant, HypeFactorConstant);
        }
    }
}
