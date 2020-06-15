using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.FakeRepo;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Utilities;
using NodaTime;
using NodaTime.Testing;
using NodaTime.Text;
using NUnit.Framework;

namespace FantasyCritic.Test
{
    [TestFixture]
    public class MasterGameSearchingTests
    {
        private static async Task<IReadOnlyList<MasterGame>> SearchGames(string searchString)
        {
            var fakeRepo = new FakeMasterGameRepo(new FakeFantasyCriticUserStore(FakeClock.FromUtc(2019, 1, 1)));
            var masterGames = await fakeRepo.GetMasterGames();
            var searchResults = MasterGameSearching.SearchMasterGames(searchString, masterGames);
            return searchResults;
        }

        [Test]
        public async Task ExactSekiroTest()
        {
            var searchResults = await SearchGames("Sekiro: Shadows Die Twice");

            Assert.GreaterOrEqual(searchResults.Count, 1);
            Assert.AreEqual("96f5e8e3-672b-4626-b47e-4bff3a6c4430", searchResults[0].MasterGameID.ToString());
        }

        [Test]
        public async Task PartOfStringSekiroTest()
        {
            var searchResults = await SearchGames("Sekiro");

            Assert.GreaterOrEqual(searchResults.Count, 1);
            Assert.AreEqual("96f5e8e3-672b-4626-b47e-4bff3a6c4430", searchResults[0].MasterGameID.ToString());
        }

        [Test]
        public async Task MispelledStringSekiroTest()
        {
            var searchResults = await SearchGames("Sakiro: Shadows Die Twice");

            Assert.GreaterOrEqual(searchResults.Count, 1);
            Assert.AreEqual("96f5e8e3-672b-4626-b47e-4bff3a6c4430", searchResults[0].MasterGameID.ToString());
        }

        [Test]
        public async Task MispelledPartStringSekiroTest()
        {
            var searchResults = await SearchGames("Sakiro");

            Assert.GreaterOrEqual(searchResults.Count, 1);
            Assert.AreEqual("96f5e8e3-672b-4626-b47e-4bff3a6c4430", searchResults[0].MasterGameID.ToString());
        }

        [Test]
        public async Task ExactMetroidTest()
        {
            var searchResults = await SearchGames("Metroid Prime 4");

            Assert.GreaterOrEqual(searchResults.Count, 1);
            Assert.AreEqual("914fc4e8-1013-46a4-b7e1-c1ca0d60ab96", searchResults[0].MasterGameID.ToString());
        }

        [Test]
        public async Task PartOfStringMetroidTest()
        {
            var searchResults = await SearchGames("Metroid");

            Assert.GreaterOrEqual(searchResults.Count, 1);
            Assert.AreEqual("914fc4e8-1013-46a4-b7e1-c1ca0d60ab96", searchResults[0].MasterGameID.ToString());
        }

        [Test]
        public async Task MispelledStringMetroidTest()
        {
            var searchResults = await SearchGames("Matroid Prime 4");

            Assert.GreaterOrEqual(searchResults.Count, 1);
            Assert.AreEqual("914fc4e8-1013-46a4-b7e1-c1ca0d60ab96", searchResults[0].MasterGameID.ToString());
        }

        [Test]
        public async Task MispelledPartStringMetroidTest()
        {
            var searchResults = await SearchGames("Matroid");

            Assert.GreaterOrEqual(searchResults.Count, 1);
            Assert.AreEqual("914fc4e8-1013-46a4-b7e1-c1ca0d60ab96", searchResults[0].MasterGameID.ToString());
        }

        [Test]
        public async Task SkippedWordMetroidTest()
        {
            var searchResults = await SearchGames("Metroid 4");

            Assert.GreaterOrEqual(searchResults.Count, 1);
            Assert.AreEqual("914fc4e8-1013-46a4-b7e1-c1ca0d60ab96", searchResults[0].MasterGameID.ToString());
        }

        [Test]
        public async Task SkippedWordMispelledMetroidTest()
        {
            var searchResults = await SearchGames("Matroid 4");

            Assert.GreaterOrEqual(searchResults.Count, 1);
            Assert.AreEqual("914fc4e8-1013-46a4-b7e1-c1ca0d60ab96", searchResults[0].MasterGameID.ToString());
        }

        [Test]
        public async Task ArabicNumeralsCivilizationTest()
        {
            var searchResults = await SearchGames("Civ 6");

            Assert.GreaterOrEqual(searchResults.Count, 1);
            Assert.AreEqual("3729b47d-35b6-48c3-8acd-a57364617b8e", searchResults[0].MasterGameID.ToString());
        }

        [Test]
        public async Task WordNumbersCivilizationTest()
        {
            var searchResults = await SearchGames("Civ Six");

            Assert.GreaterOrEqual(searchResults.Count, 1);
            Assert.AreEqual("3729b47d-35b6-48c3-8acd-a57364617b8e", searchResults[0].MasterGameID.ToString());
        }
    }
}
