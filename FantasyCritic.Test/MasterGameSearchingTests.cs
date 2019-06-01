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
        public async Task ExactTest()
        {
            var searchResults = await SearchGames("Sekiro: Shadows Die Twice");

            Assert.GreaterOrEqual(1, searchResults.Count);
            Assert.AreEqual("96f5e8e3-672b-4626-b47e-4bff3a6c4430", searchResults[0].MasterGameID.ToString());
        }

        [Test]
        public async Task PartOfStringTest()
        {
            var searchResults = await SearchGames("Sekiro");

            Assert.GreaterOrEqual(1, searchResults.Count);
            Assert.AreEqual("96f5e8e3-672b-4626-b47e-4bff3a6c4430", searchResults[0].MasterGameID.ToString());
        }

        [Test]
        public async Task MispelledStringTest()
        {
            var searchResults = await SearchGames("Sakiro: Shadows Die Twice");

            Assert.GreaterOrEqual(1, searchResults.Count);
            Assert.AreEqual("96f5e8e3-672b-4626-b47e-4bff3a6c4430", searchResults[0].MasterGameID.ToString());
        }

        [Test]
        public async Task MispelledPartStringTest()
        {
            var searchResults = await SearchGames("Sakiro");

            Assert.GreaterOrEqual(1, searchResults.Count);
            Assert.AreEqual("96f5e8e3-672b-4626-b47e-4bff3a6c4430", searchResults[0].MasterGameID.ToString());
        }
    }
}
