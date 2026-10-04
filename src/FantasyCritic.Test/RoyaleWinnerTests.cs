using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FantasyCritic.FakeRepo;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Jobs.Utilities;
using FantasyCritic.Lib.Royale;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class RoyaleWinnerTests
{
    private static readonly YearQuarter Q3 = new YearQuarter(2026, 3);
    private static readonly VeryMinimalFantasyCriticUser QuarterWinner = new VeryMinimalFantasyCriticUser(Guid.NewGuid(), "Winner");

    [TestCase(2026, 10, 1, false)]
    [TestCase(2026, 10, 7, false)]
    [TestCase(2026, 10, 8, true)]
    public async Task FinishedQuarter_GetsWinnerOnlyAfterGracePeriod(int year, int month, int day, bool expectWinner)
    {
        var royaleRepo = new FakeRoyaleRepo([new RoyaleYearQuarter(Q3, true, true, null)], QuarterWinner);

        var calculatedQuarters = await CalculateMissingWinners(royaleRepo, new LocalDate(year, month, day));
        var quarter = (await royaleRepo.GetYearQuarters()).Single();

        Assert.Multiple(() =>
        {
            Assert.That(calculatedQuarters.Select(x => x.YearQuarter), expectWinner ? Is.EqualTo(new[] { Q3 }) : Is.Empty);
            Assert.That(quarter.WinningUser, expectWinner ? Is.EqualTo(QuarterWinner) : Is.Null);
        });
    }

    [Test]
    public async Task UnfinishedQuarter_GetsNoWinner()
    {
        var royaleRepo = new FakeRoyaleRepo([new RoyaleYearQuarter(Q3, true, false, null)], QuarterWinner);

        var calculatedQuarters = await CalculateMissingWinners(royaleRepo, new LocalDate(2026, 10, 8));
        var quarter = (await royaleRepo.GetYearQuarters()).Single();

        Assert.Multiple(() =>
        {
            Assert.That(calculatedQuarters, Is.Empty);
            Assert.That(quarter.WinningUser, Is.Null);
        });
    }

    [Test]
    public async Task QuarterWithWinner_KeepsItsWinner()
    {
        var existingWinner = new VeryMinimalFantasyCriticUser(Guid.NewGuid(), "Existing Winner");
        var royaleRepo = new FakeRoyaleRepo([new RoyaleYearQuarter(Q3, true, true, existingWinner)], QuarterWinner);

        var calculatedQuarters = await CalculateMissingWinners(royaleRepo, new LocalDate(2026, 10, 8));
        var quarter = (await royaleRepo.GetYearQuarters()).Single();

        Assert.Multiple(() =>
        {
            Assert.That(calculatedQuarters, Is.Empty);
            Assert.That(quarter.WinningUser, Is.EqualTo(existingWinner));
        });
    }

    private static Task<IReadOnlyList<RoyaleYearQuarter>> CalculateMissingWinners(FakeRoyaleRepo royaleRepo, LocalDate today)
    {
        return RoyaleJobUtilities.CalculateMissingWinners(royaleRepo, today, NullLogger.Instance, CancellationToken.None);
    }
}
