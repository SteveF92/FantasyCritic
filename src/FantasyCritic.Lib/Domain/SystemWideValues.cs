using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class SystemWideValues
    {
        public SystemWideValues(decimal averageStandardGamePoints, decimal averagePickupOnlyStandardGamePoints, decimal averageCounterPickPoints)
        {
            AverageStandardGamePoints = averageStandardGamePoints;
            AveragePickupOnlyStandardGamePoints = averagePickupOnlyStandardGamePoints;
            AverageCounterPickPoints = averageCounterPickPoints;
        }

        public decimal AverageStandardGamePoints { get; }
        public decimal AveragePickupOnlyStandardGamePoints { get; }
        public decimal AverageCounterPickPoints { get; }

        public decimal GetAveragePoints(bool pickupOnly, bool counterPick)
        {
            if (counterPick)
            {
                return AverageCounterPickPoints;
            }

            if (pickupOnly)
            {
                return AveragePickupOnlyStandardGamePoints;
            }

            return AverageStandardGamePoints;
        }
    }
}
