using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class LeagueWideValues
    {
        public LeagueWideValues(decimal averageStandardGamePoints, decimal averageCounterPickPoints)
        {
            AverageStandardGamePoints = averageStandardGamePoints;
            AverageCounterPickPoints = averageCounterPickPoints;
        }

        public decimal AverageStandardGamePoints { get; }
        public decimal AverageCounterPickPoints { get; }
    }
}
