using System.Collections.Generic;
using System.Threading.Tasks;
using FantasyCritic.FakeRepo;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Utilities;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class MasterGameSearchingTests
{
    private static async Task<IReadOnlyList<MasterGame>> SearchGames(string searchString)
    {
        var fakeRepo = new FakeMasterGameRepo();
        var masterGames = await fakeRepo.GetMasterGames();
        var searchResults = MasterGameSearching.SearchMasterGames(searchString, masterGames);
        return searchResults;
    }

    [Test]
    public async Task ExactSekiroTest()
    {
        var searchResults = await SearchGames("Sekiro: Shadows Die Twice");

        Assert.That(searchResults.Count, Is.GreaterThanOrEqualTo(1));
        Assert.That(searchResults[0].MasterGameID.ToString(), Is.EqualTo("96f5e8e3-672b-4626-b47e-4bff3a6c4430"));
    }

    [Test]
    public async Task PartOfStringSekiroTest()
    {
        var searchResults = await SearchGames("Sekiro");

        Assert.That(searchResults.Count, Is.GreaterThanOrEqualTo(1));
        Assert.That(searchResults[0].MasterGameID.ToString(), Is.EqualTo("96f5e8e3-672b-4626-b47e-4bff3a6c4430"));
    }

    [Test]
    public async Task MisspelledStringSekiroTest()
    {
        var searchResults = await SearchGames("Sakiro: Shadows Die Twice");

        Assert.That(searchResults.Count, Is.GreaterThanOrEqualTo(1));
        Assert.That(searchResults[0].MasterGameID.ToString(), Is.EqualTo("96f5e8e3-672b-4626-b47e-4bff3a6c4430"));
    }

    [Test]
    public async Task MisspelledPartStringSekiroTest()
    {
        var searchResults = await SearchGames("Sakiro");

        Assert.That(searchResults.Count, Is.GreaterThanOrEqualTo(1));
        Assert.That(searchResults[0].MasterGameID.ToString(), Is.EqualTo("96f5e8e3-672b-4626-b47e-4bff3a6c4430"));
    }

    [Test]
    public async Task ExactMetroidTest()
    {
        var searchResults = await SearchGames("Metroid Prime 4");

        Assert.That(searchResults.Count, Is.GreaterThanOrEqualTo(1));
        Assert.That(searchResults[0].MasterGameID.ToString(), Is.EqualTo("914fc4e8-1013-46a4-b7e1-c1ca0d60ab96"));
    }

    [Test]
    public async Task PartOfStringMetroidTest()
    {
        var searchResults = await SearchGames("Metroid");

        Assert.That(searchResults.Count, Is.GreaterThanOrEqualTo(1));
        Assert.That(searchResults[0].MasterGameID.ToString(), Is.EqualTo("914fc4e8-1013-46a4-b7e1-c1ca0d60ab96"));
    }

    [Test]
    public async Task MisspelledStringMetroidTest()
    {
        var searchResults = await SearchGames("Matroid Prime 4");

        Assert.That(searchResults.Count, Is.GreaterThanOrEqualTo(1));
        Assert.That(searchResults[0].MasterGameID.ToString(), Is.EqualTo("914fc4e8-1013-46a4-b7e1-c1ca0d60ab96"));
    }

    [Test]
    public async Task MisspelledPartStringMetroidTest()
    {
        var searchResults = await SearchGames("Matroid");

        Assert.That(searchResults.Count, Is.GreaterThanOrEqualTo(1));
        Assert.That(searchResults[0].MasterGameID.ToString(), Is.EqualTo("914fc4e8-1013-46a4-b7e1-c1ca0d60ab96"));
    }

    [Test]
    public async Task SkippedWordMetroidTest()
    {
        var searchResults = await SearchGames("Metroid 4");

        Assert.That(searchResults.Count, Is.GreaterThanOrEqualTo(1));
        Assert.That(searchResults[0].MasterGameID.ToString(), Is.EqualTo("914fc4e8-1013-46a4-b7e1-c1ca0d60ab96"));
    }

    [Test]
    public async Task SkippedWordMisspelledMetroidTest()
    {
        var searchResults = await SearchGames("Matroid 4");

        Assert.That(searchResults.Count, Is.GreaterThanOrEqualTo(1));
        Assert.That(searchResults[0].MasterGameID.ToString(), Is.EqualTo("914fc4e8-1013-46a4-b7e1-c1ca0d60ab96"));
    }

    [Test]
    public async Task ArabicNumeralsCivilizationTest()
    {
        var searchResults = await SearchGames("Civ 6");

        Assert.That(searchResults.Count, Is.GreaterThanOrEqualTo(1));
        Assert.That(searchResults[0].MasterGameID.ToString(), Is.EqualTo("3729b47d-35b6-48c3-8acd-a57364617b8e"));
    }

    [Test]
    public async Task WordNumbersCivilizationTest()
    {
        var searchResults = await SearchGames("Civ Six");

        Assert.That(searchResults.Count, Is.GreaterThanOrEqualTo(1));
        Assert.That(searchResults[0].MasterGameID.ToString(), Is.EqualTo("3729b47d-35b6-48c3-8acd-a57364617b8e"));
    }
}
