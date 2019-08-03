using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace FantasyCritic.Lib.Extensions
{
    public static class TimeExtensions
    {
        public static LocalDate GetToday(this IClock clock)
        {
            Instant instant = clock.GetCurrentInstant();
            return instant.ToEasternDate();
        }

        public static LocalDate ToEasternDate(this Instant instant)
        {
            return instant
                .InZone(DateTimeZoneProviders.Tzdb.GetZoneOrNull("America/New_York"))
                .LocalDateTime.Date;
        }
    }
}
