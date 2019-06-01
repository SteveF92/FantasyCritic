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
        [Test]
        public async Task BasicSearchTest()
        {
            var fakeRepo = new FakeMasterGameRepo(new FakeFantasyCriticUserStore(FakeClock.FromUtc(2019, 1, 1)));

            var masterGames = await fakeRepo.GetMasterGames();
            string searchString = "Sekiro: Shadows Die Twice";
            var searchResults = MasterGameSearching.SearchMasterGames(searchString, masterGames);

            Assert.AreEqual(1, searchResults.Count);
            Assert.AreEqual("96f5e8e3-672b-4626-b47e-4bff3a6c4430", searchResults[0].MasterGameID.ToString());
        }
    }
}
