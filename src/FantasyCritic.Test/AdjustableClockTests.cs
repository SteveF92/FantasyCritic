using System;
using FantasyCritic.Lib.Utilities;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class AdjustableClockTests
{
    [Test]
    public void InitialTime_IsApproximatelyRealNow()
    {
        var clock = new AdjustableClock();
        var realNow = SystemClock.Instance.GetCurrentInstant();
        var fakeNow = clock.GetCurrentInstant();
        // Allow a 5-second window to account for slow test runners.
        Assert.That(Math.Abs((fakeNow - realNow).TotalSeconds), Is.LessThanOrEqualTo(5));
    }

    [Test]
    public void SetInitialTime_SetsTimeUnconditionally()
    {
        var clock = new AdjustableClock();
        var target = Instant.FromUtc(2024, 1, 1, 0, 0, 0);

        clock.SetInitialTime(target);

        var fakeNow = clock.GetCurrentInstant();
        // Allow a 5-second window for elapsed real time after the Set call.
        Assert.That(Math.Abs((fakeNow - target).TotalSeconds), Is.LessThanOrEqualTo(5));
    }

    [Test]
    public void SetInitialTime_AllowsGoingBackwards()
    {
        var clock = new AdjustableClock();
        var future = Instant.FromUtc(2030, 6, 1, 0, 0, 0);
        clock.SetInitialTime(future);

        var past = Instant.FromUtc(2020, 1, 1, 0, 0, 0);
        clock.SetInitialTime(past);

        var fakeNow = clock.GetCurrentInstant();
        Assert.That(Math.Abs((fakeNow - past).TotalSeconds), Is.LessThanOrEqualTo(5));
    }

    [Test]
    public void SetTime_ForwardSucceeds()
    {
        var clock = new AdjustableClock();
        var start = Instant.FromUtc(2024, 1, 1, 0, 0, 0);
        clock.SetInitialTime(start);

        var later = Instant.FromUtc(2024, 6, 1, 0, 0, 0);
        var result = clock.SetTime(later);

        Assert.That(result.IsSuccess, Is.True);
        var fakeNow = clock.GetCurrentInstant();
        Assert.That(Math.Abs((fakeNow - later).TotalSeconds), Is.LessThanOrEqualTo(5));
    }

    [Test]
    public void SetTime_BackwardsFails()
    {
        var clock = new AdjustableClock();
        var start = Instant.FromUtc(2024, 6, 1, 0, 0, 0);
        clock.SetInitialTime(start);

        var earlier = Instant.FromUtc(2024, 1, 1, 0, 0, 0);
        var result = clock.SetTime(earlier);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("past"));
    }

    [Test]
    public void SetTime_ToCurrentTime_Succeeds()
    {
        var clock = new AdjustableClock();
        var start = Instant.FromUtc(2024, 1, 1, 0, 0, 0);
        clock.SetInitialTime(start);

        // Setting to exactly the current fake time (same instant) should succeed.
        var result = clock.SetTime(start);

        Assert.That(result.IsSuccess, Is.True);
    }
}
