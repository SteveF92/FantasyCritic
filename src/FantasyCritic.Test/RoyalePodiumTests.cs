using System;
using System.Collections.Generic;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Royale;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class RoyalePodiumTests
{
    private static readonly YearQuarter Q3 = new YearQuarter(2026, 3);
    private static readonly YearQuarter Q4 = new YearQuarter(2026, 4);
    private static readonly VeryMinimalFantasyCriticUser QuarterWinner = new VeryMinimalFantasyCriticUser(Guid.NewGuid(), "Winner");

    [Test]
    public void QuarterWithWinner_ShowsItsOwnPodium()
    {
        var q3 = new RoyaleYearQuarter(Q3, true, true, QuarterWinner);
        var q4 = new RoyaleYearQuarter(Q4, true, false, null);

        var podiumYearQuarter = BuildData([q3, q4], q3).GetPodiumYearQuarter();

        Assert.That(podiumYearQuarter, Is.EqualTo(q3));
    }

    [Test]
    public void FinishedQuarterInGracePeriod_ShowsNoPodium()
    {
        var q3 = new RoyaleYearQuarter(Q3, true, true, null);
        var q4 = new RoyaleYearQuarter(Q4, true, false, null);

        var podiumYearQuarter = BuildData([q3, q4], q3).GetPodiumYearQuarter();

        Assert.That(podiumYearQuarter, Is.Null);
    }

    [Test]
    public void ActiveQuarter_ShowsPreviousQuarterPodium()
    {
        var q3 = new RoyaleYearQuarter(Q3, true, true, QuarterWinner);
        var q4 = new RoyaleYearQuarter(Q4, true, false, null);

        var podiumYearQuarter = BuildData([q3, q4], q4).GetPodiumYearQuarter();

        Assert.That(podiumYearQuarter, Is.EqualTo(q3));
    }

    [Test]
    public void ActiveQuarter_WhilePreviousQuarterInGracePeriod_ShowsNoPodium()
    {
        var q3 = new RoyaleYearQuarter(Q3, true, true, null);
        var q4 = new RoyaleYearQuarter(Q4, true, false, null);

        var podiumYearQuarter = BuildData([q3, q4], q4).GetPodiumYearQuarter();

        Assert.That(podiumYearQuarter, Is.Null);
    }

    [Test]
    public void FirstQuarter_ShowsNoPodium()
    {
        var q4 = new RoyaleYearQuarter(Q4, true, false, null);

        var podiumYearQuarter = BuildData([q4], q4).GetPodiumYearQuarter();

        Assert.That(podiumYearQuarter, Is.Null);
    }

    private static RoyaleYearQuarterData BuildData(IReadOnlyList<RoyaleYearQuarter> allYearQuarters, RoyaleYearQuarter activeYearQuarter)
    {
        return new RoyaleYearQuarterData(allYearQuarters, activeYearQuarter, [], []);
    }
}
