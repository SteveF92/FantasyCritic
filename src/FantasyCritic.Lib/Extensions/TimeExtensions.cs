using System.Globalization;

namespace FantasyCritic.Lib.Extensions;

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

    public static string ToISOString(this LocalDate date, string? wrapWith = null)
    {
        var value = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return WrapWith(value, wrapWith);
    }

    public static string ToLongDate(this LocalDate date, string? wrapWith = null)
    {
        var value = date.ToString("dddd, MMMM dd, yyyy", CultureInfo.InvariantCulture);
        return WrapWith(value, wrapWith);
    }

    public static string ToNullableISOString(this LocalDate? date, string? wrapWith = null)
    {
        var value = date?.ToISOString() ?? "No Date";
        return WrapWith(value, wrapWith);
    }

    public static string ToNullableLongDate(this LocalDate? date, string? wrapWith = null)
    {
        var value = date?.ToLongDate() ?? "No Date";
        return WrapWith(value, wrapWith);
    }

    private static string WrapWith(string value, string? wrapWith = null)
    {
        if (wrapWith is null)
        {
            return value;
        }

        return $"{wrapWith}{value}{wrapWith}";
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

    public static bool ShouldGrantSuperDrops(this IClock clock)
    {
        var now = clock.GetCurrentInstant();
        var date = now.ToEasternDate();
        var superDropsGrantTime = GetSuperDropsGrantTime(date.Year);
        return now >= superDropsGrantTime;
    }

    public static Instant GetSuperDropsGrantTime(this IClock clock)
    {
        var currentDate = clock.GetToday();
        return GetSuperDropsGrantTime(currentDate.Year);
    }

    private static Instant GetSuperDropsGrantTime(int year)
    {
        var superDropsDate = SuperDropsDate.InYear(year);
        return superDropsDate.AtStartOfDayInZone(EasternTimeZone).ToInstant();
    }

    public static readonly DateTimeZone EasternTimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull("America/New_York")!;
    public static readonly IsoDayOfWeek PublicBiddingRevealDay = IsoDayOfWeek.Thursday;
    public static readonly LocalTime PublicBiddingRevealTime = new LocalTime(20, 0);
    public static readonly IsoDayOfWeek ActionProcessingDay = IsoDayOfWeek.Saturday;
    public static readonly LocalTime ActionProcessingTime = new LocalTime(20, 0);
    public static readonly AnnualDate SuperDropsDate = new AnnualDate(9, 1);
}
