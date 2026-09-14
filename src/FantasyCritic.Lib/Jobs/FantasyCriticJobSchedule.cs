using Cronos;
using FantasyCritic.Lib.Extensions;

namespace FantasyCritic.Lib.Jobs;

//A cron expression evaluated in America/New_York, because the rules of Fantasy Critic are, whatever the servers run.
//Cronos works in DateTime and TimeZoneInfo; this is the one place that converts, so callers deal only in Instants.
public class FantasyCriticJobSchedule
{
    //Cronos needs a TimeZoneInfo, which NodaTime can't produce, so the zone is looked up by the same IANA ID as TimeExtensions.EasternTimeZone.
    //A container image without tzdata fails here, loudly, rather than scheduling in the wrong zone.
    private static readonly TimeZoneInfo EasternTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(TimeExtensions.EasternTimeZone.Id);

    private readonly CronExpression _cronExpression;

    private FantasyCriticJobSchedule(string expression)
    {
        Expression = expression;
        _cronExpression = CronExpression.Parse(expression);
    }

    public string Expression { get; }

    public static readonly FantasyCriticJobSchedule EveryTenMinutes = Cron("*/10 * * * *");
    public static readonly FantasyCriticJobSchedule Hourly = Cron("0 * * * *");

    public static FantasyCriticJobSchedule Cron(string expression) => new(expression);

    //Derived from the same constants that drive the site's "next reveal" display, so the two cannot disagree.
    public static FantasyCriticJobSchedule Weekly(IsoDayOfWeek dayOfWeek, LocalTime timeOfDay)
    {
        if (timeOfDay.Second != 0 || timeOfDay.NanosecondOfSecond != 0)
        {
            throw new ArgumentException($"A cron schedule can only express whole minutes, not {timeOfDay}.", nameof(timeOfDay));
        }

        //Cron counts Sunday as 0; IsoDayOfWeek counts it as 7.
        var cronDayOfWeek = (int)dayOfWeek % 7;
        return new FantasyCriticJobSchedule($"{timeOfDay.Minute} {timeOfDay.Hour} * * {cronDayOfWeek}");
    }

    public Instant GetNextOccurrence(Instant after)
    {
        var next = _cronExpression.GetNextOccurrence(after.ToDateTimeUtc(), EasternTimeZoneInfo);
        if (next is null)
        {
            //Only possible for an expression that can never fire again, which none of ours are.
            throw new InvalidOperationException($"Cron expression '{Expression}' has no occurrence after {after}.");
        }

        return Instant.FromDateTimeUtc(next.Value);
    }

    //The most recent occurrence in (after, upTo], or null if none has come due. Missed occurrences before it are deliberately skipped:
    //a worker that was down for a day should run a ten-minute job once, not 144 times.
    public Instant? GetLatestOccurrence(Instant after, Instant upTo)
    {
        if (upTo <= after)
        {
            return null;
        }

        var latest = _cronExpression
            .GetOccurrencesDescending(upTo.ToDateTimeUtc(), after.ToDateTimeUtc(), EasternTimeZoneInfo, fromInclusive: true, toInclusive: false)
            .Select(x => (DateTime?)x)
            .FirstOrDefault();

        return latest.HasValue ? Instant.FromDateTimeUtc(latest.Value) : null;
    }

    public override string ToString() => Expression;
}
