using FantasyCritic.Lib.Extensions;
using NodaTime.Text;
using NodaTime;
using NUnit.Framework;
using NodaTime.Testing;

namespace FantasyCritic.Test;

[TestFixture]
public class TimingTests
{
    private static Instant GetInstantFromEasternString(string dateTimeString)
    {
        var localDateTime = LocalDateTimePattern.GeneralIso.Parse(dateTimeString).GetValueOrThrow();
        var instant = localDateTime.InZoneStrictly(TimeExtensions.EasternTimeZone).ToInstant();
        return instant;
    }
    
    [TestCase("2023-09-15T19:00:00", false)]
    [TestCase("2023-09-16T19:00:00", false)]
    [TestCase("2023-09-16T19:59:00", false)]
    [TestCase("2023-09-16T20:00:00", true)]
    [TestCase("2023-09-16T20:01:00", true)]
    [TestCase("2023-09-16T20:02:00", true)]
    [TestCase("2023-09-16T20:02:59", true)]
    [TestCase("2023-09-16T20:03:00", true)]
    [TestCase("2023-09-16T20:04:00", false)]
    [TestCase("2023-09-17T20:04:00", false)]
    public void BidLockTimeTest(string instantString, bool expected)
    {
        IClock fakeClock = new FakeClock(GetInstantFromEasternString(instantString));
        var isBidLock = fakeClock.IsBidLockWindow();
        Assert.That(isBidLock, Is.EqualTo(expected));
    }
}
