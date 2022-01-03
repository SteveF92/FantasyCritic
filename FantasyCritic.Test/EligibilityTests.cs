using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Extensions;
using NodaTime;
using NodaTime.Testing;
using NodaTime.Text;
using NUnit.Framework;

namespace FantasyCritic.Test
{
    [TestFixture]
    public class EligibilityTests
    {
        private static Dictionary<string, MasterGameTag> _tagDictionary = new List<MasterGameTag>()
        {
            new MasterGameTag("Cancelled", "Cancelled", "CNCL", null, false, false, "", new List<string>(), ""),
            new MasterGameTag("CurrentlyInEarlyAccess", "Currently in Early Access", "C-EA",null, true, false, "", new List<string>(), ""),
            new MasterGameTag("DirectorsCut", "Director's Cut", "DC", null, false, false, "", new List<string>(), ""),
            new MasterGameTag("ExpansionPack", "Expansion Pack", "EXP", null, false, false, "", new List<string>(), ""),
            new MasterGameTag("FreeToPlay", "Free to Play", "FTP", null, false, false, "", new List<string>(), ""),
            new MasterGameTag("NewGame", "New Game", "NG", null, false, false, "", new List<string>(), ""),
            new MasterGameTag("NewGamingFranchise", "New Gaming Franchise", "NGF", null, false, false, "", new List<string>(), ""),
            new MasterGameTag("PartialRemake", "Partial Remake", "P-RMKE", null, false, false, "", new List<string>(), ""),
            new MasterGameTag("PlannedForEarlyAccess", "Planned for Early Access", "P-EA",null, true, false, "", new List<string>(), ""),
            new MasterGameTag("Port", "Port", "PRT", null, false, false, "", new List<string>(), ""),
            new MasterGameTag("Reimagining", "Reimagining", "RIMG", null, false, false, "", new List<string>(), ""),
            new MasterGameTag("ReleasedInternationally", "Released Internationally", "R-INT",null, true, false, "", new List<string>(), ""),
            new MasterGameTag("Remake", "Remake", "RMKE", null, false, false, "", new List<string>(), ""),
            new MasterGameTag("Remaster", "Remaster", "RMSTR", null, false, false, "", new List<string>(), ""),
            new MasterGameTag("UnannouncedGame", "Unannounced Game", "UNA", null, false, false, "", new List<string>(), ""),
            new MasterGameTag("VirtualReality", "Virtual Reality", "VR", null, false, false, "", new List<string>(), ""),
            new MasterGameTag("WillReleaseInternationallyFirst", "Will Release Internationally First", "W-INT",null, true, false, "", new List<string>(), ""),
            new MasterGameTag("YearlyInstallment", "Yearly Installment", "YI", null, false, false, "", new List<string>(), ""),
        }.ToDictionary(x => x.ShortName);

        private static MasterGame CreateBasicMasterGame(string name, LocalDate releaseDate, MasterGameTag tag)
        {
            return new MasterGame(Guid.NewGuid(), name, releaseDate.ToISOString(), releaseDate, releaseDate, null, null,
                releaseDate, null, null, "", null, null, false, false, false, Instant.MinValue,
                new List<MasterSubGame>(), new List<MasterGameTag>(){ tag });
        }

        [Test]
        public void SimpleEligibleTest()
        {
            Instant nowTime = InstantPattern.ExtendedIso.Parse("2021-01-02T20:49:24Z").GetValueOrThrow();
            IClock fakeClock = new FakeClock(nowTime);
            var fakeToday = fakeClock.GetToday();

            MasterGame masterGame = CreateBasicMasterGame("Elden Ring", new LocalDate(2022, 2, 25), _tagDictionary["NGF"]);

            var leagueTags = new List<LeagueTagStatus>()
            {
                new LeagueTagStatus(_tagDictionary["PRT"], TagStatus.Banned)
            };

            var slotTags = new List<LeagueTagStatus>();

            var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, fakeToday);
            Assert.AreEqual(0, claimErrors.Count);
        }

        [Test]
        public void SimpleInEligibleTest()
        {
            Instant nowTime = InstantPattern.ExtendedIso.Parse("2021-01-02T20:49:24Z").GetValueOrThrow();
            IClock fakeClock = new FakeClock(nowTime);
            var fakeToday = fakeClock.GetToday();

            MasterGame masterGame = CreateBasicMasterGame("GTA 5 (PS5)", new LocalDate(2022, 2, 25), _tagDictionary["PRT"]);

            var leagueTags = new List<LeagueTagStatus>()
            {
                new LeagueTagStatus(_tagDictionary["PRT"], TagStatus.Banned)
            };

            var slotTags = new List<LeagueTagStatus>();

            var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, fakeToday);
            Assert.AreEqual(1, claimErrors.Count);
            Assert.AreEqual("That game is not eligible because the Port tag has been banned.", claimErrors[0].Error);
        }

        [Test]
        public void SlotEligibleTest()
        {
            Instant nowTime = InstantPattern.ExtendedIso.Parse("2021-01-02T20:49:24Z").GetValueOrThrow();
            IClock fakeClock = new FakeClock(nowTime);
            var fakeToday = fakeClock.GetToday();

            MasterGame masterGame = CreateBasicMasterGame("Elden Ring", new LocalDate(2022, 2, 25), _tagDictionary["NGF"]);

            var leagueTags = new List<LeagueTagStatus>()
            {
                new LeagueTagStatus(_tagDictionary["PRT"], TagStatus.Banned)
            };

            var slotTags = new List<LeagueTagStatus>()
            {
                new LeagueTagStatus(_tagDictionary["NGF"], TagStatus.Required)
            };

            var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, fakeToday);
            Assert.AreEqual(0, claimErrors.Count);
        }

        [Test]
        public void SlotInEligibleTest()
        {
            Instant nowTime = InstantPattern.ExtendedIso.Parse("2021-01-02T20:49:24Z").GetValueOrThrow();
            IClock fakeClock = new FakeClock(nowTime);
            var fakeToday = fakeClock.GetToday();

            MasterGame masterGame = CreateBasicMasterGame("Horizon Forbidden West", new LocalDate(2022, 2, 25), _tagDictionary["NG"]);

            var leagueTags = new List<LeagueTagStatus>()
            {
                new LeagueTagStatus(_tagDictionary["PRT"], TagStatus.Banned)
            };

            var slotTags = new List<LeagueTagStatus>()
            {
                new LeagueTagStatus(_tagDictionary["NGF"], TagStatus.Required)
            };

            var claimErrors = LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, masterGame, masterGame.Tags, fakeToday);
            Assert.AreEqual(1, claimErrors.Count);
            Assert.AreEqual("That game is not eligible because it does not have any of the following required tags: (New Gaming Franchise)", claimErrors[0].Error);
        }
    }
}
