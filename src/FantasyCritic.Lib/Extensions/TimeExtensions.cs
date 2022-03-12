using System.Globalization;

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

        public static Instant GetNextPublicRevealTime(this IClock clock) => GetNextTime(clock, PublicBiddingRevealDay, PublicBiddingRevealTime);
        public static Instant GetNextBidTime(this IClock clock) => GetNextTime(clock, ActionProcessingDay, ActionProcessingTime);

        private static Instant GetNextTime(this IClock clock, IsoDayOfWeek dayOfWeek, LocalTime timeOnDay)
        {
            var currentTime = clock.GetCurrentInstant();
            var nyc = EasternTimeZone;
            var localDateTime = currentTime.InZone(nyc).LocalDateTime;
            LocalDate currentDate = localDateTime.Date;
            LocalTime currentLocalTime = localDateTime.TimeOfDay;
            LocalDate nextPublicRevealDate;
            if (currentDate.DayOfWeek == dayOfWeek)
            {
                nextPublicRevealDate = currentLocalTime >= timeOnDay ? currentDate.Next(dayOfWeek) : currentDate;
            }
            else
            {
                nextPublicRevealDate = currentDate.Next(dayOfWeek);
            }

            LocalDateTime dateTime = nextPublicRevealDate + timeOnDay;
            return dateTime.InZoneStrictly(nyc).ToInstant();
        }

        public static Instant GetPreviousBidTime(this IClock clock)
        {
            var currentTime = clock.GetCurrentInstant();
            var nyc = EasternTimeZone;
            var currentDateTime = currentTime.InZone(nyc).LocalDateTime;
            LocalDate previousBidDate;
            if (currentDateTime.Date.DayOfWeek == ActionProcessingDay && currentDateTime.TimeOfDay > ActionProcessingTime)
            {
                previousBidDate = currentDateTime.Date;
            }
            else
            {
                previousBidDate = currentDateTime.Date.Previous(ActionProcessingDay);
            }

            LocalDateTime dateTime = previousBidDate + ActionProcessingTime;
            return dateTime.InZoneStrictly(nyc).ToInstant();
        }

        public static readonly DateTimeZone EasternTimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull("America/New_York");
        public static readonly IsoDayOfWeek PublicBiddingRevealDay = IsoDayOfWeek.Thursday;
        public static readonly LocalTime PublicBiddingRevealTime = new LocalTime(20, 0);
        public static readonly IsoDayOfWeek ActionProcessingDay = IsoDayOfWeek.Saturday;
        public static readonly LocalTime ActionProcessingTime = new LocalTime(20, 0);
    }
}
