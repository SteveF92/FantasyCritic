using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;
using static FantasyCritic.IntegrationTests.Helpers.SlotTestHelpers;

namespace FantasyCritic.IntegrationTests.Tests.League.Slots;

/// <summary>
/// Creating an additional draft that adds slots, special slots among them. New special slots are numbered after the
/// existing ones, special slots stay last, and games already in a special slot stay in it.
/// </summary>
[TestFixture]
public class NewDraftSlotTests : IntegrationTestBase
{
    // Slots 0-1 regular, slot 2 requires New Game; one counter-pick slot.
    private static readonly LeagueScenario OneSpecialSlot = Scenario("NewDraftSlots", 3, 1, [[NewGame]]);

    [Test]
    public async Task NewDraftWithASpecialSlot_AppendsItAfterTheExistingSpecialSlot_AndKeepsGamesInPlace()
    {
        await using var league = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(Factory, OneSpecialSlot, NewUser);
        var games = await GetTaggedGamesAsync(league, Remake, NewGame, Remake);
        var publisher = league.Publishers[0];
        var remake = await ClaimAsync(league, publisher, games[0]);
        var specialSlotGame = await ClaimAsync(league, publisher, games[1]);
        var counterPick = await ClaimAsync(league, league.Publishers[1], games[0], counterPick: true);
        await ClaimAsync(league, league.Publishers[1], games[2]);

        await league.Manager.LeagueManager.CreateLeagueDraftAsync(new CreateLeagueDraftRequest
        {
            LeagueID = league.LeagueID,
            Year = league.Year,
            Name = "Second Draft",
            ScheduledDate = null,
            GamesToDraft = 2,
            CounterPicksToDraft = 1,
            AdditionalStandardGames = 2,
            AdditionalCounterPicks = 1,
            NewSpecialGameSlots = [new SpecialGameSlotViewModel { SpecialSlotPosition = 0, RequiredTags = [Remake] }],
        });

        var after = await league.GetLeagueYearAsync();
        var specialSlots = after.Settings.SpecialGameSlots.OrderBy(s => s.SpecialSlotPosition).ToList();
        var placements = GetPlacements(after);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(after.Settings.StandardGames, Is.EqualTo(5));
            Assert.That(after.Settings.CounterPicks, Is.EqualTo(2));
            Assert.That(specialSlots.Select(s => string.Join(",", s.RequiredTags)), Is.EqualTo(new[] { NewGame, Remake }),
                "The new special slot is numbered after the existing one.");
            Assert.That(placements[remake.PublisherGameID], Is.EqualTo("0"));
            Assert.That(placements[specialSlotGame.PublisherGameID], Is.EqualTo("3 (special 0: NewGame)"),
                "The New Game slot is still the first special slot, now slot 3, and its game goes with it.");
            Assert.That(placements[counterPick.PublisherGameID], Is.EqualTo("CP 0"));
        }

        AssertSlotsAreConsistent(after);
    }

    [Test]
    public async Task NewDraftWithMoreSpecialSlotsThanNewStandardSlots_IsRefused()
    {
        await using var league = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(Factory, OneSpecialSlot, NewUser);

        var exception = Assert.ThrowsAsync<ApiException>(() => league.Manager.LeagueManager.CreateLeagueDraftAsync(new CreateLeagueDraftRequest
        {
            LeagueID = league.LeagueID,
            Year = league.Year,
            Name = "Second Draft",
            ScheduledDate = null,
            GamesToDraft = 0,
            CounterPicksToDraft = 0,
            AdditionalStandardGames = 0,
            AdditionalCounterPicks = 0,
            NewSpecialGameSlots = [new SpecialGameSlotViewModel { SpecialSlotPosition = 0, RequiredTags = [Remake] }],
        }));

        var after = await league.GetLeagueYearAsync();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(400));
            Assert.That(after.Drafts, Has.Count.EqualTo(1));
            Assert.That(after.Settings.SpecialGameSlots, Has.Count.EqualTo(1));
        }
    }
}
