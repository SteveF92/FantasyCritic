using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        [Test]
        public void BasicScoreTest()
        {
            Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
            Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
            IClock fakeClock = new FakeClock(nowTime);
            ScoringSystem standardScoring = ScoringSystem.GetScoringSystem("Standard");

            MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "", new LocalDate(2018, 7, 13), null, 84.8095m, 2018, StandardEligibilityLevel, false, false);
            PublisherGame testGame = new PublisherGame(Guid.NewGuid(), "", pickupTime, false, false, null, masterGame, 2018);

            decimal? fantasyPoints = standardScoring.GetPointsForGame(testGame, fakeClock);

            Assert.AreEqual(14.8095m, fantasyPoints);
        }

        [Test]
        public void Over90ScoreTest()
        {
            Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
            Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
            IClock fakeClock = new FakeClock(nowTime);
            ScoringSystem standardScoring = ScoringSystem.GetScoringSystem("Standard");

            MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "", new LocalDate(2018, 4, 20), null, 94.8125m, 2018, StandardEligibilityLevel, false, false);
            PublisherGame testGame = new PublisherGame(Guid.NewGuid(), "", pickupTime, false, false, null, masterGame, 2018);

            decimal? fantasyPoints = standardScoring.GetPointsForGame(testGame, fakeClock);

            Assert.AreEqual(29.625m, fantasyPoints);
        }

        [Test]
        public void Under70ScoreTest()
        {
            Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
            Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
            IClock fakeClock = new FakeClock(nowTime);
            ScoringSystem standardScoring = ScoringSystem.GetScoringSystem("Standard");

            MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "", new LocalDate(2018, 4, 20), null, 65.8559m, 2018, StandardEligibilityLevel, false, false);
            PublisherGame testGame = new PublisherGame(Guid.NewGuid(), "", pickupTime, false, false, null, masterGame, 2018);

            decimal? fantasyPoints = standardScoring.GetPointsForGame(testGame, fakeClock);

            Assert.AreEqual(-4.1441m, fantasyPoints);
        }

        [Test]
        public void UnreleasedGameTest()
        {
            Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
            Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
            IClock fakeClock = new FakeClock(nowTime);
            ScoringSystem standardScoring = ScoringSystem.GetScoringSystem("Standard");

            MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "", new LocalDate(2018, 10, 20), null, null, 2018, StandardEligibilityLevel, false, false);
            PublisherGame testGame = new PublisherGame(Guid.NewGuid(), "", pickupTime, false, false, null, masterGame, 2018);

            decimal? fantasyPoints = standardScoring.GetPointsForGame(testGame, fakeClock);

            Assert.AreEqual(null, fantasyPoints);
        }

        [Test]
        public void WillNotReleaseGameTest()
        {
            Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
            Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
            IClock fakeClock = new FakeClock(nowTime);
            ScoringSystem standardScoring = ScoringSystem.GetScoringSystem("Standard");

            MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "", new LocalDate(2018, 10, 20), null, null, 2019, StandardEligibilityLevel, false, false);
            PublisherGame testGame = new PublisherGame(Guid.NewGuid(), "", pickupTime, false, false, null, masterGame, 2018);

            decimal? fantasyPoints = standardScoring.GetPointsForGame(testGame, fakeClock);

            Assert.AreEqual(0m, fantasyPoints);
        }

        [Test]
        public void CounterPickGameTest()
        {
            Instant pickupTime = InstantPattern.ExtendedIso.Parse("2018-01-02T12:34:24Z").GetValueOrThrow();
            Instant nowTime = InstantPattern.ExtendedIso.Parse("2018-08-02T12:34:24Z").GetValueOrThrow();
            IClock fakeClock = new FakeClock(nowTime);
            ScoringSystem standardScoring = ScoringSystem.GetScoringSystem("Standard");

            MasterGame masterGame = new MasterGame(Guid.NewGuid(), "", "", new LocalDate(2018, 4, 20), null, 65.8559m, 2018, StandardEligibilityLevel, false, false);
            PublisherGame testGame = new PublisherGame(Guid.NewGuid(), "", pickupTime, false, true, null, masterGame, 2018);

            decimal? fantasyPoints = standardScoring.GetPointsForGame(testGame, fakeClock);

            Assert.AreEqual(4.1441m, fantasyPoints);
        }
    }
}