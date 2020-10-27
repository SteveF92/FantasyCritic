using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using NodaTime;
using NodaTime.Testing;
using NodaTime.Text;
using NUnit.Framework;

namespace FantasyCritic.Test
{
    [TestFixture]
    public class StandardScoringTests
    {
        private static readonly EligibilityLevel StandardEligibilityLevel = new EligibilityLevel(0, "", "", new List<string>());
        private static readonly SystemWideValues SystemWideValues = new SystemWideValues(10, -5);

        [Test]
        public void BasicScoreTest()
        {
            Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
            Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
            IClock fakeClock = new FakeClock(nowTime);
            ScoringSystem standardScoring = ScoringSystem.GetScoringSystem("Standard");
            var eligibilitySettings = new EligibilitySettings(StandardEligibilityLevel, false, false, false, false, false, false);

            MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "", new LocalDate(2018, 7, 13), new LocalDate(2018, 7, 13), null, 84.8095m, new LocalDate(2018,1,1), eligibilitySettings, "", "", 
                fakeClock.GetCurrentInstant(), false, false, false, fakeClock.GetCurrentInstant());
            PublisherGame testGame = new PublisherGame(Guid.NewGuid(), Guid.NewGuid(), "", pickupTime, false, null, false, null, new MasterGameYear(masterGame, 2018), null, null);

            decimal? fantasyPoints = testGame.CalculateFantasyPoints(standardScoring, fakeClock);

            Assert.AreEqual(14.8095m, fantasyPoints);
        }

        [Test]
        public void ManualScoreTest()
        {
            Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
            Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
            IClock fakeClock = new FakeClock(nowTime);
            ScoringSystem standardScoring = ScoringSystem.GetScoringSystem("Standard");
            var eligibilitySettings = new EligibilitySettings(StandardEligibilityLevel, false, false, false, false, false, false);

            MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "", new LocalDate(2018, 7, 13), new LocalDate(2018, 7, 13), null, null, new LocalDate(2018, 1, 1), eligibilitySettings, "", "", 
                fakeClock.GetCurrentInstant(), false, false, false, fakeClock.GetCurrentInstant());
            PublisherGame testGame = new PublisherGame(Guid.NewGuid(), Guid.NewGuid(), "", pickupTime, false, 83.8095m, false, null, new MasterGameYear(masterGame, 2018), null, null);

            decimal? fantasyPoints = testGame.CalculateFantasyPoints(standardScoring, fakeClock);

            Assert.AreEqual(13.8095m, fantasyPoints);
        }

        [Test]
        public void Over90ScoreTest()
        {
            Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
            Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
            IClock fakeClock = new FakeClock(nowTime);
            ScoringSystem standardScoring = ScoringSystem.GetScoringSystem("Standard");
            var eligibilitySettings = new EligibilitySettings(StandardEligibilityLevel, false, false, false, false, false, false);

            MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "", new LocalDate(2018, 4, 20), new LocalDate(2018, 4, 20), null, 94.8125m, new LocalDate(2018, 1, 1), eligibilitySettings, "", "", 
                fakeClock.GetCurrentInstant(), false, false, false, fakeClock.GetCurrentInstant());
            PublisherGame testGame = new PublisherGame(Guid.NewGuid(), Guid.NewGuid(), "", pickupTime, false, null, false, null, new MasterGameYear(masterGame, 2018), null, null);

            decimal? fantasyPoints = testGame.CalculateFantasyPoints(standardScoring, fakeClock);

            Assert.AreEqual(29.625m, fantasyPoints);
        }

        [Test]
        public void Under70ScoreTest()
        {
            Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
            Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
            IClock fakeClock = new FakeClock(nowTime);
            ScoringSystem standardScoring = ScoringSystem.GetScoringSystem("Standard");
            var eligibilitySettings = new EligibilitySettings(StandardEligibilityLevel, false, false, false, false, false, false);

            MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "", new LocalDate(2018, 4, 20), new LocalDate(2018, 4, 20), null, 65.8559m, new LocalDate(2018, 1, 1), eligibilitySettings, "", "", 
                fakeClock.GetCurrentInstant(), false, false, false, fakeClock.GetCurrentInstant());
            PublisherGame testGame = new PublisherGame(Guid.NewGuid(), Guid.NewGuid(), "", pickupTime, false, null, false, null, new MasterGameYear(masterGame, 2018), null, null);

            decimal? fantasyPoints = testGame.CalculateFantasyPoints(standardScoring, fakeClock);

            Assert.AreEqual(-4.1441m, fantasyPoints);
        }

        [Test]
        public void UnreleasedGameTest()
        {
            Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
            Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
            IClock fakeClock = new FakeClock(nowTime);
            ScoringSystem standardScoring = ScoringSystem.GetScoringSystem("Standard");
            var eligibilitySettings = new EligibilitySettings(StandardEligibilityLevel, false, false, false, false, false, false);

            MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "", new LocalDate(2018, 10, 20), new LocalDate(2018, 10, 20), null, null, new LocalDate(2018, 1, 1), eligibilitySettings, "", "", 
                fakeClock.GetCurrentInstant(), false, false, false, fakeClock.GetCurrentInstant());
            PublisherGame testGame = new PublisherGame(Guid.NewGuid(), Guid.NewGuid(), "", pickupTime, false, null, false, null, new MasterGameYear(masterGame, 2018), null, null);

            decimal? fantasyPoints = testGame.CalculateFantasyPoints(standardScoring, fakeClock);

            Assert.AreEqual(null, fantasyPoints);
        }

        [Test]
        public void WillNotReleaseGameTest()
        {
            Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
            Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
            IClock fakeClock = new FakeClock(nowTime);
            ScoringSystem standardScoring = ScoringSystem.GetScoringSystem("Standard");
            var eligibilitySettings = new EligibilitySettings(StandardEligibilityLevel, false, false, false, false, false, false);

            MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "", new LocalDate(2019, 10, 20), new LocalDate(2019, 10, 20), null, null, new LocalDate(2019, 1, 1), eligibilitySettings, "", "", 
                fakeClock.GetCurrentInstant(), false, false, false, fakeClock.GetCurrentInstant());
            PublisherGame testGame = new PublisherGame(Guid.NewGuid(), Guid.NewGuid(), "", pickupTime, false, null, false, null, new MasterGameYear(masterGame, 2018), null, null);

            decimal? fantasyPoints = testGame.CalculateFantasyPoints(standardScoring, fakeClock);

            Assert.AreEqual(0m, fantasyPoints);
        }

        [Test]
        public void CounterPickGameTest()
        {
            Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
            Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
            IClock fakeClock = new FakeClock(nowTime);
            ScoringSystem standardScoring = ScoringSystem.GetScoringSystem("Standard");
            var eligibilitySettings = new EligibilitySettings(StandardEligibilityLevel, false, false, false, false, false, false);

            MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "", new LocalDate(2018, 4, 20), new LocalDate(2018, 4, 20), null, 65.8559m, new LocalDate(2018, 1, 1), eligibilitySettings, "", "", 
                fakeClock.GetCurrentInstant(), false, false, false, fakeClock.GetCurrentInstant());
            PublisherGame testGame = new PublisherGame(Guid.NewGuid(), Guid.NewGuid(), "", pickupTime, true, null, false, null, new MasterGameYear(masterGame, 2018), null, null);

            decimal? fantasyPoints = testGame.CalculateFantasyPoints(standardScoring, fakeClock);

            Assert.AreEqual(4.1441m, fantasyPoints);
        }
    }
}