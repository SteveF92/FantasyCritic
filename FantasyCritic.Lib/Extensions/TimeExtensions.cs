using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Utilities;
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
                .InZone(EasternTimeZone)
                .LocalDateTime.Date;
        }

        public static string ToISOString(this LocalDate date)
        {
            return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        public static readonly DateTimeZone EasternTimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull("America/New_York");
    }
}
