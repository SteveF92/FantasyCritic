using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Extensions;
using NodaTime;
using NodaTime.Testing;
using NodaTime.Text;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class StandardScoringTests
{
    private static readonly ScoringSystem _scoringSystem = ScoringSystem.GetScoringSystem("Standard");

    [Test]
    public void BasicScoreTest()
    {
        Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
        Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
        IClock fakeClock = new FakeClock(nowTime);
        var fakeToday = fakeClock.GetToday();

        MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "",
            new LocalDate(2018, 7, 13), new LocalDate(2018, 7, 13), null, null, null, new LocalDate(2018, 7, 13),
            null, null, 84.8095m, "", "", "", fakeClock.GetCurrentInstant(), false, false, false, false,
            fakeClock.GetCurrentInstant(), new List<MasterSubGame>(), new List<MasterGameTag>());

        PublisherGame testGame = new PublisherGame(Guid.NewGuid(), Guid.NewGuid(), "", pickupTime, false, null, false, null, new MasterGameYear(masterGame, 2018), 1, null, null, null, null);
        PublisherSlot testSlot = new PublisherSlot(1, 1, false, Maybe<SpecialGameSlot>.None, testGame);
        decimal? fantasyPoints = testSlot.CalculateFantasyPoints(true, _scoringSystem, fakeToday);

        Assert.AreEqual(14.8095m, fantasyPoints);
    }

    [Test]
    public void ManualScoreTest()
    {
        Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
        Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
        IClock fakeClock = new FakeClock(nowTime);
        var fakeToday = fakeClock.GetToday();

        MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "",
            new LocalDate(2018, 7, 13), new LocalDate(2018, 7, 13), null, null, null, new LocalDate(2018, 7, 13),
            null, null, 84.8095m, "", "", "", fakeClock.GetCurrentInstant(), false, false, false, false,
            fakeClock.GetCurrentInstant(), new List<MasterSubGame>(), new List<MasterGameTag>());

        PublisherGame testGame = new PublisherGame(Guid.NewGuid(), Guid.NewGuid(), "", pickupTime, false, 83.8095m, false, null, new MasterGameYear(masterGame, 2018), 1, null, null, null, null);
        PublisherSlot testSlot = new PublisherSlot(1, 1, false, Maybe<SpecialGameSlot>.None, testGame);
        decimal? fantasyPoints = testSlot.CalculateFantasyPoints(true, _scoringSystem, fakeToday);

        Assert.AreEqual(13.8095m, fantasyPoints);
    }

    [Test]
    public void Over90ScoreTest()
    {
        Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
        Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
        IClock fakeClock = new FakeClock(nowTime);
        var fakeToday = fakeClock.GetToday();

        MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "",
            new LocalDate(2018, 4, 20), new LocalDate(2018, 4, 20), null, null, null, new LocalDate(2018, 4, 20),
            null, null, 94.8125m, "", "", "", fakeClock.GetCurrentInstant(), false, false, false, false,
            fakeClock.GetCurrentInstant(), new List<MasterSubGame>(), new List<MasterGameTag>());

        PublisherGame testGame = new PublisherGame(Guid.NewGuid(), Guid.NewGuid(), "", pickupTime, false, null, false, null, new MasterGameYear(masterGame, 2018), 1, null, null, null, null);
        PublisherSlot testSlot = new PublisherSlot(1, 1, false, Maybe<SpecialGameSlot>.None, testGame);
        decimal? fantasyPoints = testSlot.CalculateFantasyPoints(true, _scoringSystem, fakeToday);

        Assert.AreEqual(29.625m, fantasyPoints);
    }

    [Test]
    public void Under70ScoreTest()
    {
        Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
        Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
        IClock fakeClock = new FakeClock(nowTime);
        var fakeToday = fakeClock.GetToday();

        MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "",
            new LocalDate(2018, 4, 20), new LocalDate(2018, 4, 20), null, null, null, new LocalDate(2018, 4, 20),
            null, null, 65.8559m, "", "", "", fakeClock.GetCurrentInstant(), false, false, false, false,
            fakeClock.GetCurrentInstant(), new List<MasterSubGame>(), new List<MasterGameTag>());

        PublisherGame testGame = new PublisherGame(Guid.NewGuid(), Guid.NewGuid(), "", pickupTime, false, null, false, null, new MasterGameYear(masterGame, 2018), 1, null, null, null, null);
        PublisherSlot testSlot = new PublisherSlot(1, 1, false, Maybe<SpecialGameSlot>.None, testGame);
        decimal? fantasyPoints = testSlot.CalculateFantasyPoints(true, _scoringSystem, fakeToday);

        Assert.AreEqual(-4.1441m, fantasyPoints);
    }

    [Test]
    public void Under60ScoreTest()
    {
        Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
        Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
        IClock fakeClock = new FakeClock(nowTime);
        var fakeToday = fakeClock.GetToday();

        MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "",
            new LocalDate(2018, 4, 20), new LocalDate(2018, 4, 20), null, null, null, new LocalDate(2018, 4, 20),
            null, null, 55.8559m, "", "", "", fakeClock.GetCurrentInstant(), false, false, false, false,
            fakeClock.GetCurrentInstant(), new List<MasterSubGame>(), new List<MasterGameTag>());

        PublisherGame testGame = new PublisherGame(Guid.NewGuid(), Guid.NewGuid(), "", pickupTime, false, null, false, null, new MasterGameYear(masterGame, 2018), 1, null, null, null, null);
        PublisherSlot testSlot = new PublisherSlot(1, 1, false, Maybe<SpecialGameSlot>.None, testGame);
        decimal? fantasyPoints = testSlot.CalculateFantasyPoints(true, _scoringSystem, fakeToday);

        Assert.AreEqual(-14.1441m, fantasyPoints);
    }

    [Test]
    public void Under50ScoreTest()
    {
        Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
        Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
        IClock fakeClock = new FakeClock(nowTime);
        var fakeToday = fakeClock.GetToday();

        MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "",
            new LocalDate(2018, 4, 20), new LocalDate(2018, 4, 20), null, null, null, new LocalDate(2018, 4, 20),
            null, null, 45.8559m, "", "", "", fakeClock.GetCurrentInstant(), false, false, false, false,
            fakeClock.GetCurrentInstant(), new List<MasterSubGame>(), new List<MasterGameTag>());

        PublisherGame testGame = new PublisherGame(Guid.NewGuid(), Guid.NewGuid(), "", pickupTime, false, null, false, null, new MasterGameYear(masterGame, 2018), 1, null, null, null, null);
        PublisherSlot testSlot = new PublisherSlot(1, 1, false, Maybe<SpecialGameSlot>.None, testGame);
        decimal? fantasyPoints = testSlot.CalculateFantasyPoints(true, _scoringSystem, fakeToday);

        Assert.AreEqual(-24.1441m, fantasyPoints);
    }

    [Test]
    public void Under40ScoreTest()
    {
        Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
        Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
        IClock fakeClock = new FakeClock(nowTime);
        var fakeToday = fakeClock.GetToday();

        MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "",
            new LocalDate(2018, 4, 20), new LocalDate(2018, 4, 20), null, null, null, new LocalDate(2018, 4, 20),
            null, null, 35.8559m, "", "", "", fakeClock.GetCurrentInstant(), false, false, false, false,
            fakeClock.GetCurrentInstant(), new List<MasterSubGame>(), new List<MasterGameTag>());

        PublisherGame testGame = new PublisherGame(Guid.NewGuid(), Guid.NewGuid(), "", pickupTime, false, null, false, null, new MasterGameYear(masterGame, 2018), 1, null, null, null, null);
        PublisherSlot testSlot = new PublisherSlot(1, 1, false, Maybe<SpecialGameSlot>.None, testGame);
        decimal? fantasyPoints = testSlot.CalculateFantasyPoints(true, _scoringSystem, fakeToday);

        Assert.AreEqual(-34.1441m, fantasyPoints);
    }

    [Test]
    public void Under30ScoreTest()
    {
        Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
        Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
        IClock fakeClock = new FakeClock(nowTime);
        var fakeToday = fakeClock.GetToday();

        MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "",
            new LocalDate(2018, 4, 20), new LocalDate(2018, 4, 20), null, null, null, new LocalDate(2018, 4, 20),
            null, null, 25.8559m, "", "", "", fakeClock.GetCurrentInstant(), false, false, false, false,
            fakeClock.GetCurrentInstant(), new List<MasterSubGame>(), new List<MasterGameTag>());

        PublisherGame testGame = new PublisherGame(Guid.NewGuid(), Guid.NewGuid(), "", pickupTime, false, null, false, null, new MasterGameYear(masterGame, 2018), 1, null, null, null, null);
        PublisherSlot testSlot = new PublisherSlot(1, 1, false, Maybe<SpecialGameSlot>.None, testGame);
        decimal? fantasyPoints = testSlot.CalculateFantasyPoints(true, _scoringSystem, fakeToday);

        Assert.AreEqual(-44.1441m, fantasyPoints);
    }

    [Test]
    public void Under20ScoreTest()
    {
        Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
        Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
        IClock fakeClock = new FakeClock(nowTime);
        var fakeToday = fakeClock.GetToday();

        MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "",
            new LocalDate(2018, 4, 20), new LocalDate(2018, 4, 20), null, null, null, new LocalDate(2018, 4, 20),
            null, null, 15.8559m, "", "", "", fakeClock.GetCurrentInstant(), false, false, false, false,
            fakeClock.GetCurrentInstant(), new List<MasterSubGame>(), new List<MasterGameTag>());

        PublisherGame testGame = new PublisherGame(Guid.NewGuid(), Guid.NewGuid(), "", pickupTime, false, null, false, null, new MasterGameYear(masterGame, 2018), 1, null, null, null, null);
        PublisherSlot testSlot = new PublisherSlot(1, 1, false, Maybe<SpecialGameSlot>.None, testGame);
        decimal? fantasyPoints = testSlot.CalculateFantasyPoints(true, _scoringSystem, fakeToday);

        Assert.AreEqual(-54.1441m, fantasyPoints);
    }

    [Test]
    public void Under10ScoreTest()
    {
        Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
        Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
        IClock fakeClock = new FakeClock(nowTime);
        var fakeToday = fakeClock.GetToday();

        MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "",
            new LocalDate(2018, 4, 20), new LocalDate(2018, 4, 20), null, null, null, new LocalDate(2018, 4, 20),
            null, null, 5.8559m, "", "", "", fakeClock.GetCurrentInstant(), false, false, false, false,
            fakeClock.GetCurrentInstant(), new List<MasterSubGame>(), new List<MasterGameTag>());

        PublisherGame testGame = new PublisherGame(Guid.NewGuid(), Guid.NewGuid(), "", pickupTime, false, null, false, null, new MasterGameYear(masterGame, 2018), 1, null, null, null, null);
        PublisherSlot testSlot = new PublisherSlot(1, 1, false, Maybe<SpecialGameSlot>.None, testGame);
        decimal? fantasyPoints = testSlot.CalculateFantasyPoints(true, _scoringSystem, fakeToday);

        Assert.AreEqual(-64.1441m, fantasyPoints);
    }

    [Test]
    public void UnreleasedGameTest()
    {
        Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
        Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
        IClock fakeClock = new FakeClock(nowTime);
        var fakeToday = fakeClock.GetToday();

        MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "",
            new LocalDate(2018, 10, 20), new LocalDate(2018, 10, 20), null, null, null, new LocalDate(2018, 10, 20),
            null, null, null, "", "", "", fakeClock.GetCurrentInstant(), false, false, false, false,
            fakeClock.GetCurrentInstant(), new List<MasterSubGame>(), new List<MasterGameTag>());

        PublisherGame testGame = new PublisherGame(Guid.NewGuid(), Guid.NewGuid(), "", pickupTime, false, null, false, null, new MasterGameYear(masterGame, 2018), 1, null, null, null, null);
        PublisherSlot testSlot = new PublisherSlot(1, 1, false, Maybe<SpecialGameSlot>.None, testGame);
        decimal? fantasyPoints = testSlot.CalculateFantasyPoints(true, _scoringSystem, fakeToday);

        Assert.AreEqual(null, fantasyPoints);
    }

    [Test]
    public void WillNotReleaseGameTest()
    {
        Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
        Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
        IClock fakeClock = new FakeClock(nowTime);
        var fakeToday = fakeClock.GetToday();

        MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "",
            new LocalDate(2019, 10, 20), new LocalDate(2019, 10, 20), null, null, null, new LocalDate(2019, 10, 20),
            null, null, null, "", "", "", fakeClock.GetCurrentInstant(), false, false, false, false,
            fakeClock.GetCurrentInstant(), new List<MasterSubGame>(), new List<MasterGameTag>());

        PublisherGame testGame = new PublisherGame(Guid.NewGuid(), Guid.NewGuid(), "", pickupTime, false, null, false, null, new MasterGameYear(masterGame, 2018), 1, null, null, null, null);
        PublisherSlot testSlot = new PublisherSlot(1, 1, false, Maybe<SpecialGameSlot>.None, testGame);
        decimal? fantasyPoints = testSlot.CalculateFantasyPoints(true, _scoringSystem, fakeToday);

        Assert.AreEqual(0m, fantasyPoints);
    }

    [Test]
    public void CounterPickGameTest()
    {
        Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
        Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
        IClock fakeClock = new FakeClock(nowTime);
        var fakeToday = fakeClock.GetToday();

        MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "",
            new LocalDate(2018, 4, 20), new LocalDate(2018, 4, 20), null, null, null, new LocalDate(2018, 4, 20),
            null, null, 65.8559m, "", "", "", fakeClock.GetCurrentInstant(), false, false, false, false,
            fakeClock.GetCurrentInstant(), new List<MasterSubGame>(), new List<MasterGameTag>());

        PublisherGame testGame = new PublisherGame(Guid.NewGuid(), Guid.NewGuid(), "", pickupTime, true, null, false, null, new MasterGameYear(masterGame, 2018), 1, null, null, null, null);
        PublisherSlot testSlot = new PublisherSlot(1, 1, true, Maybe<SpecialGameSlot>.None, testGame);
        decimal? fantasyPoints = testSlot.CalculateFantasyPoints(true, _scoringSystem, fakeToday);

        Assert.AreEqual(4.1441m, fantasyPoints);
    }
}