using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.MySQL.Entities
{
    public class SystemWideValuesEntity
    {
        public SystemWideValuesEntity()
        {

        }

        public SystemWideValuesEntity(SystemWideValues domain)
        {
            AverageStandardGamePoints = domain.AverageStandardGamePoints;
            AverageCounterPickPoints = domain.AverageCounterPickPoints;
        }

        public decimal AverageStandardGamePoints { get; set; }
        public decimal AverageCounterPickPoints { get; set; }

        public SystemWideValues ToDomain()
        {
            return new SystemWideValues(AverageStandardGamePoints, AverageCounterPickPoints);
        }
    }
}
