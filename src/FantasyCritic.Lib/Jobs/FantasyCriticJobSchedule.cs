using Cronos;
using FantasyCritic.Lib.Extensions;

namespace FantasyCritic.Lib.Jobs;

public class FantasyCriticJobSchedule
{
    //Cronos needs a TimeZoneInfo, which NodaTime can't produce, so the zone is looked up by the same IANA ID as TimeExtensions.EasternTimeZone.
    private static readonly TimeZoneInfo EasternTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(TimeExtensions.EasternTimeZone.Id);

    private readonly CronExpression _cronExpression;
    private readonly Func<Instant, bool>? _calendarGuard;

    private FantasyCriticJobSchedule(string expression, Func<Instant, bool>? calendarGuard)
    {
        Expression = expression;
        _cronExpression = CronExpression.Parse(expression);
        _calendarGuard = calendarGuard;
    }

    public string Expression { get; }

    public static readonly FantasyCriticJobSchedule EveryTenMinutes = Cron("*/10 * * * *");
    public static readonly FantasyCriticJobSchedule Hourly = Cron("0 * * * *");
    public static readonly FantasyCriticJobSchedule EveryTwoHours = Cron("0 */2 * * *");
    public static readonly FantasyCriticJobSchedule AtTenPmEastern = Cron("0 22 * * *");
    public static readonly FantasyCriticJobSchedule AtOnePastMidnightEastern = Cron("1 0 * * *");

    public static FantasyCriticJobSchedule Weekly(IsoDayOfWeek dayOfWeek, LocalTime timeOfDay)
    {
        //We could do this with plain cron, but this allows us to align with the constants in the TimeExtensions file.
        if (timeOfDay.Second != 0 || timeOfDay.NanosecondOfSecond != 0)
        {
            throw new ArgumentException($"A cron schedule can only express whole minutes, not {timeOfDay}.", nameof(timeOfDay));
        }

        //Cron counts Sunday as 0; IsoDayOfWeek counts it as 7.
        var cronDayOfWeek = (int)dayOfWeek % 7;
        return new FantasyCriticJobSchedule($"{timeOfDay.Minute} {timeOfDay.Hour} * * {cronDayOfWeek}", null);
    }

    public static FantasyCriticJobSchedule Cron(string expression) => new(expression, null);

    //Used for more complex cases like Grant Super Drops, which needs to only start happening after September 1st
    public FantasyCriticJobSchedule WithCalendarGuard(Func<Instant, bool> calendarGuard) => new(Expression, calendarGuard);

    public override string ToString() => Expression;

    public Instant? GetNextOccurrence(Instant after)
    {
        var next = _cronExpression.GetNextOccurrence(after.ToDateTimeUtc(), EasternTimeZoneInfo);
        if (next is null)
        {
            throw new InvalidOperationException($"Cron expression '{Expression}' has no occurrence after {after}.");
        }

        var nextOccurrence = Instant.FromDateTimeUtc(next.Value);
        if (_calendarGuard is not null && !_calendarGuard.Invoke(nextOccurrence))
        {
            return null;
        }

        return nextOccurrence;
    }
}
