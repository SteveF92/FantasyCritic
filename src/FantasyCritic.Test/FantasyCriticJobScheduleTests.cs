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

    [Test]
    public void NamedSchedules_HaveTheExpectedExpressions()
    {
        Assert.Multiple(() =>
        {
            Assert.That(FantasyCriticJobSchedule.EveryTenMinutes.Expression, Is.EqualTo("*/10 * * * *"));
            Assert.That(FantasyCriticJobSchedule.Hourly.Expression, Is.EqualTo("0 * * * *"));
            Assert.That(FantasyCriticJobSchedule.EveryTwoHours.Expression, Is.EqualTo("0 */2 * * *"));
            Assert.That(FantasyCriticJobSchedule.AtTenPmEastern.Expression, Is.EqualTo("0 22 * * *"));
            Assert.That(FantasyCriticJobSchedule.AtOnePastMidnightEastern.Expression, Is.EqualTo("1 0 * * *"));
        });
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
    public void GetNextOccurrence_IsNeverNullWithoutACalendarGuard()
    {
        var schedule = FantasyCriticJobSchedule.Cron("*/10 * * * *");

        Assert.That(schedule.GetNextOccurrence(Instant.FromUtc(2026, 1, 1, 0, 0)), Is.EqualTo(Instant.FromUtc(2026, 1, 1, 0, 10)));
    }

    [Test]
    public void WithCalendarGuard_KeepsTheExpressionAndLeavesTheOriginalUnguarded()
    {
        var original = FantasyCriticJobSchedule.Cron("*/10 * * * *");
        var guarded = original.WithCalendarGuard(_ => false);
        var after = Instant.FromUtc(2026, 1, 1, 0, 0);

        Assert.Multiple(() =>
        {
            Assert.That(guarded.Expression, Is.EqualTo(original.Expression));
            Assert.That(guarded.GetNextOccurrence(after), Is.Null);
            Assert.That(original.GetNextOccurrence(after), Is.EqualTo(Instant.FromUtc(2026, 1, 1, 0, 10)));
        });
    }

    [Test]
    public void WithCalendarGuard_IsGivenTheOccurrenceNotTheStartingInstant()
    {
        var after = Instant.FromUtc(2026, 1, 1, 0, 0);
        var expectedOccurrence = Instant.FromUtc(2026, 1, 1, 0, 10);
        var guardedInstants = new List<Instant>();
        var schedule = FantasyCriticJobSchedule.EveryTenMinutes.WithCalendarGuard(x =>
        {
            guardedInstants.Add(x);
            return true;
        });

        Assert.Multiple(() =>
        {
            Assert.That(schedule.GetNextOccurrence(after), Is.EqualTo(expectedOccurrence));
            Assert.That(guardedInstants, Is.EqualTo(new[] { expectedOccurrence }));
        });
    }

    //September 1st midnight Eastern is 04:00 UTC, during daylight saving time. Each case starts one hour before the slot being guarded.
    [TestCase(2026, 9, 1, 2, false)]
    [TestCase(2026, 9, 1, 3, true)]
    [TestCase(2026, 12, 31, 12, true)]
    [TestCase(2027, 1, 1, 12, false)]
    public void GrantSuperDropsSchedule_IsActiveFromSeptemberFirstEastern(int year, int month, int day, int utcHour, bool expectedActive)
    {
        var schedule = Registry.Schedules[FantasyCriticJobType.GrantSuperDrops];
        var after = Instant.FromUtc(year, month, day, utcHour, 0);

        var next = schedule.GetNextOccurrence(after);

        Assert.That(next, expectedActive ? Is.EqualTo(after + Duration.FromHours(1)) : Is.Null);
    }

    //If the cron and the site's "next public reveal" display ever disagree, this fails instead of a post going out at the wrong time.
    [Test]
    public void PublicBiddingSchedule_MatchesNextPublicRevealTime()
    {
        var schedule = Registry.Schedules[FantasyCriticJobType.SendAllPublicBiddingMessages];

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
            var next = schedule.GetNextOccurrence(now);
            Assert.That(next, Is.Not.Null, $"No occurrence after {now}");
            var nextEastern = next!.Value.ToEasternDateTime();
            Assert.That((nextEastern.DayOfWeek, nextEastern.TimeOfDay), Is.EqualTo((TimeExtensions.ReleasingThisWeekNewsDay, TimeExtensions.ReleasingThisWeekNewsTime)),
                $"Disagreement at {now}");
        }
    }

    [Test]
    public void OnlyTheCronJobTypesHaveSchedules()
    {
        var scheduled = Registry.Schedules.ToDictionary(x => x.Key.Value, x => x.Value.Expression);

        Assert.That(scheduled, Is.EquivalentTo(new Dictionary<string, string>
        {
            ["ExpireTrades"] = "0 * * * *",
            ["ProcessSpecialAuctions"] = "*/10 * * * *",
            ["GrantSuperDrops"] = "0 * * * *",
            ["RefreshPatreonInfo"] = "0 * * * *",
            ["FullDataRefresh"] = "0 */2 * * *",
            ["UpdateDailyPublisherStatistics"] = "0 22 * * *",
            ["PushGameReleaseMessages"] = "1 0 * * *",
            ["EndOfYearRollover"] = "0 0 1 1 *",
            ["AdvanceRoyaleQuarters"] = "1 0 * * *",
            ["PrepareForActionProcessing"] = "0 20 * * 6",
            ["SendAllPublicBiddingMessages"] = "0 20 * * 4",
            ["SendReleasingThisWeekUpdate"] = "0 20 * * 0",
        }));
    }

    //FullDataRefresh only skips in favor of the rollover when both are due in one scheduler wake, so their slots must coincide exactly.
    [TestCase(2026, 12, 31, 12)]
    [TestCase(2026, 7, 1, 0)]
    public void EndOfYearRollover_SharesASlotWithFullDataRefreshAtMidnightOnJanuaryFirst(int year, int month, int day, int utcHour)
    {
        var rollover = Registry.Schedules[FantasyCriticJobType.EndOfYearRollover];
        var refresh = Registry.Schedules[FantasyCriticJobType.FullDataRefresh];
        var januaryFirstMidnightEastern = Instant.FromUtc(2027, 1, 1, 5, 0);
        var justBeforeMidnight = januaryFirstMidnightEastern - Duration.FromMinutes(1);

        Assert.Multiple(() =>
        {
            Assert.That(rollover.GetNextOccurrence(Instant.FromUtc(year, month, day, utcHour, 0)), Is.EqualTo(januaryFirstMidnightEastern));
            Assert.That(refresh.GetNextOccurrence(justBeforeMidnight), Is.EqualTo(januaryFirstMidnightEastern));
        });
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
