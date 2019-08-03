using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class SystemWideValues
    {
        public SystemWideValues(decimal averageStandardGamePoints, decimal averageCounterPickPoints)
        {
            AverageStandardGamePoints = averageStandardGamePoints;
            AverageCounterPickPoints = averageCounterPickPoints;
        }

        public decimal AverageStandardGamePoints { get; }
        public decimal AverageCounterPickPoints { get; }

        public decimal GetAveragePoints(bool counterPick)
        {
            if (!counterPick)
            {
                return AverageStandardGamePoints;
            }

            return AverageCounterPickPoints;
        }
    }
}
