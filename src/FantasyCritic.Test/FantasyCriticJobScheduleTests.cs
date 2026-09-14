using System;
using System.Collections.Generic;
using System.Linq;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Jobs;
using NodaTime;
using NodaTime.Testing;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class FantasyCriticJobScheduleTests
{
    private static readonly FantasyCriticJobRegistry Registry = FantasyCriticJobRegistry.Create();

    [Test]
    public void Weekly_BuildsCronExpressionWithSundayAsZero()
    {
        Assert.Multiple(() =>
        {
            Assert.That(FantasyCriticJobSchedule.Weekly(IsoDayOfWeek.Thursday, new LocalTime(20, 0)).Expression, Is.EqualTo("0 20 * * 4"));
            Assert.That(FantasyCriticJobSchedule.Weekly(IsoDayOfWeek.Sunday, new LocalTime(20, 0)).Expression, Is.EqualTo("0 20 * * 0"));
            Assert.That(FantasyCriticJobSchedule.Weekly(IsoDayOfWeek.Monday, new LocalTime(9, 30)).Expression, Is.EqualTo("30 9 * * 1"));
        });
    }

    [Test]
    public void Weekly_RejectsTimesThatAreNotWholeMinutes()
    {
        Assert.Throws<ArgumentException>(() => FantasyCriticJobSchedule.Weekly(IsoDayOfWeek.Thursday, new LocalTime(20, 0, 30)));
    }

    //The point of evaluating in America/New_York: the same 20:00 local is a different UTC hour either side of DST.
    [TestCase(2026, 7, 16, 0)]
    [TestCase(2026, 1, 15, 1)]
    public void GetNextOccurrence_IsEasternTimeAcrossDaylightSaving(int year, int month, int thursday, int expectedUtcHour)
    {
        var schedule = FantasyCriticJobSchedule.Cron("0 20 * * THU");
        var thursdayMorningUtc = Instant.FromUtc(year, month, thursday, 12, 0);

        var next = schedule.GetNextOccurrence(thursdayMorningUtc);

        Assert.That(next, Is.EqualTo(Instant.FromUtc(year, month, thursday + 1, expectedUtcHour, 0)));
    }

    [Test]
    public void GetNextOccurrence_IsExclusiveOfTheStartingInstant()
    {
        var schedule = FantasyCriticJobSchedule.Cron("*/10 * * * *");
        var slot = Instant.FromUtc(2026, 9, 13, 14, 10);

        Assert.That(schedule.GetNextOccurrence(slot), Is.EqualTo(Instant.FromUtc(2026, 9, 13, 14, 20)));
    }

    [Test]
    public void GetLatestOccurrence_CollapsesMissedSlotsIntoTheMostRecent()
    {
        var schedule = FantasyCriticJobSchedule.Cron("*/10 * * * *");
        var lastScheduled = Instant.FromUtc(2026, 9, 13, 10, 0);
        var now = Instant.FromUtc(2026, 9, 13, 11, 25);

        Assert.That(schedule.GetLatestOccurrence(lastScheduled, now), Is.EqualTo(Instant.FromUtc(2026, 9, 13, 11, 20)));
    }

    [Test]
    public void GetLatestOccurrence_ExcludesTheLastScheduledSlotAndIncludesNow()
    {
        var schedule = FantasyCriticJobSchedule.Cron("*/10 * * * *");
        var lastScheduled = Instant.FromUtc(2026, 9, 13, 10, 0);

        Assert.Multiple(() =>
        {
            Assert.That(schedule.GetLatestOccurrence(lastScheduled, Instant.FromUtc(2026, 9, 13, 10, 9, 59)), Is.Null);
            Assert.That(schedule.GetLatestOccurrence(lastScheduled, Instant.FromUtc(2026, 9, 13, 10, 10)), Is.EqualTo(Instant.FromUtc(2026, 9, 13, 10, 10)));
            Assert.That(schedule.GetLatestOccurrence(lastScheduled, lastScheduled), Is.Null);
            Assert.That(schedule.GetLatestOccurrence(lastScheduled, Instant.FromUtc(2026, 9, 13, 9, 0)), Is.Null);
        });
    }

    [Test]
    public void IsActiveAt_IsAlwaysTrueWithoutACalendarGuard()
    {
        var schedule = FantasyCriticJobSchedule.Cron("*/10 * * * *");

        Assert.That(schedule.IsActiveAt(Instant.FromUtc(2026, 1, 1, 0, 0)), Is.True);
    }

    [Test]
    public void WithCalendarGuard_KeepsTheExpressionAndLeavesTheOriginalUnguarded()
    {
        var original = FantasyCriticJobSchedule.Cron("*/10 * * * *");
        var guarded = original.WithCalendarGuard(_ => false);
        var slot = Instant.FromUtc(2026, 1, 1, 0, 0);

        Assert.Multiple(() =>
        {
            Assert.That(guarded.Expression, Is.EqualTo(original.Expression));
            Assert.That(guarded.IsActiveAt(slot), Is.False);
            Assert.That(original.IsActiveAt(slot), Is.True);
        });
    }

    //September 1st midnight Eastern is 04:00 UTC, during daylight saving time.
    [TestCase(2026, 9, 1, 3, 50, false)]
    [TestCase(2026, 9, 1, 4, 0, true)]
    [TestCase(2026, 12, 31, 12, 0, true)]
    [TestCase(2027, 1, 1, 12, 0, false)]
    public void GrantSuperDropsSchedule_IsActiveFromSeptemberFirstEastern(int year, int month, int day, int utcHour, int utcMinute, bool expectedActive)
    {
        var schedule = Registry.Schedules[FantasyCriticJobType.GrantSuperDrops];

        Assert.That(schedule.IsActiveAt(Instant.FromUtc(year, month, day, utcHour, utcMinute)), Is.EqualTo(expectedActive));
    }

    //If the cron and the site's "next public reveal" display ever disagree, this fails instead of a post going out at the wrong time.
    [TestCase("PushPublicBiddingMessages")]
    [TestCase("SendPublicBiddingEmails")]
    public void PublicBiddingSchedules_MatchNextPublicRevealTime(string jobTypeName)
    {
        var schedule = Registry.Schedules[FantasyCriticJobType.FromValue(jobTypeName)];

        foreach (var now in InstantsAcrossAYear())
        {
            var clock = new FakeClock(now);
            Assert.That(schedule.GetNextOccurrence(now), Is.EqualTo(clock.GetNextPublicRevealTime()), $"Disagreement at {now}");
        }
    }

    [Test]
    public void ReleasingThisWeekSchedule_MatchesReleasingThisWeekConstants()
    {
        var schedule = Registry.Schedules[FantasyCriticJobType.SendReleasingThisWeekUpdate];

        foreach (var now in InstantsAcrossAYear())
        {
            var next = schedule.GetNextOccurrence(now).ToEasternDateTime();
            Assert.That((next.DayOfWeek, next.TimeOfDay), Is.EqualTo((TimeExtensions.ReleasingThisWeekNewsDay, TimeExtensions.ReleasingThisWeekNewsTime)),
                $"Disagreement at {now}");
        }
    }

    [Test]
    public void OnlyTheConvertedTaskJobTypesHaveSchedules()
    {
        var scheduled = Registry.Schedules.ToDictionary(x => x.Key.Value, x => x.Value.Expression);

        Assert.That(scheduled, Is.EquivalentTo(new Dictionary<string, string>
        {
            ["ExpireTrades"] = "*/10 * * * *",
            ["ProcessSpecialAuctions"] = "*/10 * * * *",
            ["GrantSuperDrops"] = "*/10 * * * *",
            ["RefreshPatreonInfo"] = "0 * * * *",
            ["SetTimeFlags"] = "0 * * * *",
            ["FullDataRefresh"] = "0 */2 * * *",
            ["UpdateDailyPublisherStatistics"] = "0 22 * * *",
            ["PushGameReleaseMessages"] = "0 0 * * *",
            ["PushPublicBiddingMessages"] = "0 20 * * 4",
            ["SendPublicBiddingEmails"] = "0 20 * * 4",
            ["SendReleasingThisWeekUpdate"] = "0 20 * * 0",
        }));
    }

    //A 61-minute step walks through every minute-of-hour and hour-of-day across both DST transitions. The exact slots, winter and summer,
    //are added explicitly, since that boundary is where an inclusive/exclusive mismatch would hide.
    private static IEnumerable<Instant> InstantsAcrossAYear()
    {
        yield return Instant.FromUtc(2026, 1, 16, 1, 0);
        yield return Instant.FromUtc(2026, 7, 17, 0, 0);
        yield return Instant.FromUtc(2026, 1, 19, 1, 0);
        yield return Instant.FromUtc(2026, 7, 20, 0, 0);

        var start = Instant.FromUtc(2026, 1, 1, 0, 0);
        var end = Instant.FromUtc(2027, 1, 1, 0, 0);
        for (var now = start; now < end; now += Duration.FromMinutes(61))
        {
            yield return now;
        }
    }
}
