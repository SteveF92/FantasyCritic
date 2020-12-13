using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace FantasyCritic.Lib.Utilities
{
    public static class TimeFunctions
    {
        public static (LocalDate minimumReleaseDate, LocalDate maximumReleaseDate) ParseEstimatedReleaseDate(string estimatedReleaseDate, IClock clock)
        {
            return (LocalDate.MinIsoValue, LocalDate.MaxIsoValue);
        }
    }
}
