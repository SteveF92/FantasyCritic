using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;
using static FantasyCritic.IntegrationTests.Helpers.SlotTestHelpers;

namespace FantasyCritic.IntegrationTests.Tests.League.Slots;

/// <summary>
/// Which slot a manager claim puts a game in. A claim goes to the least permissive open slot the game is eligible
/// for, so a game with the special slot's tag fills the special slot before a regular one, and a game without it
/// never lands there. Counter picks fill counter-pick slots in order, and a removed game's slot is reused.
/// </summary>
[TestFixture]
public class SlotPlacementTests : IntegrationTestBase
{
    // Slots 0-2 regular, slot 3 requires New Game; two counter-pick slots.
    private static readonly LeagueScenario OneSpecialSlot = Scenario("SlotPlacement", 4, 2, [[NewGame]]);

    [Test]
    public async Task ManagerClaim_FillsTheSpecialSlotFirstWithAGameThatFitsIt()
    {
        await using var league = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(Factory, OneSpecialSlot, NewUser);
        var games = await GetTaggedGamesAsync(league, Remake, NewGame, NewGame, Remake);
        var publisher = league.Publishers[0];

        var firstRemake = await ClaimAsync(league, publisher, games[0]);
        var firstNewGame = await ClaimAsync(league, publisher, games[1]);
        var secondNewGame = await ClaimAsync(league, publisher, games[2]);
        var secondRemake = await ClaimAsync(league, publisher, games[3]);

        var leagueYear = await league.GetLeagueYearAsync();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(GetSlotOf(leagueYear, firstRemake.PublisherGameID).SlotNumber, Is.Zero);
            Assert.That(GetSlotOf(leagueYear, firstNewGame.PublisherGameID).SlotNumber, Is.EqualTo(3),
                "A New Game should take the New Game slot while it is open.");
            Assert.That(GetSlotOf(leagueYear, firstNewGame.PublisherGameID).SpecialSlot, Is.Not.Null);
            Assert.That(GetSlotOf(leagueYear, secondNewGame.PublisherGameID).SlotNumber, Is.EqualTo(1),
                "With the special slot full, a New Game takes the first open regular slot.");
            Assert.That(GetSlotOf(leagueYear, secondRemake.PublisherGameID).SlotNumber, Is.EqualTo(2));
        }

        AssertSlotsAreConsistent(leagueYear);
    }

    [Test]
    public async Task ManagerClaim_GameThatDoesNotFitTheOnlyOpenSlot_IsRefused()
    {
        await using var league = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(Factory, OneSpecialSlot, NewUser);
        var games = await GetTaggedGamesAsync(league, Remake, Remake, Remake, Remake, NewGame);
        var publisher = league.Publishers[0];
        for (var i = 0; i < 3; i++)
        {
            await ClaimAsync(league, publisher, games[i]);
        }

        var refused = await TryClaimAsync(league, publisher, games[3]);
        var newGame = await ClaimAsync(league, publisher, games[4]);

        var leagueYear = await league.GetLeagueYearAsync();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(refused.Success, Is.False, "A Remake should not be placed in the New Game slot.");
            Assert.That(GetPublisher(leagueYear, publisher.PublisherID).Games, Has.Count.EqualTo(4));
            Assert.That(GetSlotOf(leagueYear, newGame.PublisherGameID).SlotNumber, Is.EqualTo(3));
        }

        AssertSlotsAreConsistent(leagueYear);
    }

    [Test]
    public async Task RemovingAGame_FreesItsSlotForTheNextClaim()
    {
        await using var league = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(Factory, OneSpecialSlot, NewUser);
        var games = await GetTaggedGamesAsync(league, Remake, Remake, Remake, Remake);
        var publisher = league.Publishers[0];
        await ClaimAsync(league, publisher, games[0]);
        var middleGame = await ClaimAsync(league, publisher, games[1]);
        await ClaimAsync(league, publisher, games[2]);

        await league.Manager.LeagueManager.RemovePublisherGameAsync(new GameRemoveRequest
        {
            PublisherID = publisher.PublisherID,
            PublisherGameID = middleGame.PublisherGameID,
        });
        var replacement = await ClaimAsync(league, publisher, games[3]);

        var leagueYear = await league.GetLeagueYearAsync();
        Assert.That(GetSlotOf(leagueYear, replacement.PublisherGameID).SlotNumber, Is.EqualTo(1),
            "The claim should take the slot the removed game left.");
        AssertSlotsAreConsistent(leagueYear);
    }

    [Test]
    public async Task CounterPickClaims_FillCounterPickSlotsInOrder_AndReuseAFreedSlot()
    {
        await using var league = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(Factory, OneSpecialSlot, NewUser);
        var games = await GetTaggedGamesAsync(league, Remake, Remake, Remake);
        var owner = league.Publishers[0];
        var counterPicker = league.Publishers[1];
        foreach (var game in games)
        {
            await ClaimAsync(league, owner, game);
        }

        var firstCounterPick = await ClaimAsync(league, counterPicker, games[0], counterPick: true);
        var secondCounterPick = await ClaimAsync(league, counterPicker, games[1], counterPick: true);

        var afterTwo = await league.GetLeagueYearAsync();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(GetSlotOf(afterTwo, firstCounterPick.PublisherGameID).SlotNumber, Is.Zero);
            Assert.That(GetSlotOf(afterTwo, secondCounterPick.PublisherGameID).SlotNumber, Is.EqualTo(1));
            Assert.That(GetSlotOf(afterTwo, secondCounterPick.PublisherGameID).OverallSlotNumber,
                Is.EqualTo(OneSpecialSlot.StandardGames + 1));
        }

        AssertSlotsAreConsistent(afterTwo);

        await league.Manager.LeagueManager.RemovePublisherGameAsync(new GameRemoveRequest
        {
            PublisherID = counterPicker.PublisherID,
            PublisherGameID = firstCounterPick.PublisherGameID,
        });
        var thirdCounterPick = await ClaimAsync(league, counterPicker, games[2], counterPick: true);

        // Today the repo renumbers counter picks after a removal, so the survivor moves up to slot 0; with slot rows it
        // stays where it is. Either way the two counter picks share the two slots.
        var afterReplacement = await league.GetLeagueYearAsync();
        var counterPickSlotNumbers = new[] { secondCounterPick, thirdCounterPick }
            .Select(g => GetSlotOf(afterReplacement, g.PublisherGameID).SlotNumber);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(counterPickSlotNumbers, Is.EquivalentTo(new[] { 0, 1 }),
                "The new counter pick should take the counter-pick slot the removed one left.");
            Assert.That(GetPublisher(afterReplacement, counterPicker.PublisherID).Games.Where(g => !g.CounterPick), Is.Empty,
                "Counter picks must not land in standard slots.");
        }

        AssertSlotsAreConsistent(afterReplacement);
    }

    [Test]
    public async Task NewLeague_ShowsTheSpecialSlotsAsCreated()
    {
        var scenario = Scenario("SlotPlacementTwoSpecial", 5, 1, [[NewGame], [Remake, Remaster]]);
        await using var league = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(Factory, scenario, NewUser);

        var leagueYear = await league.GetLeagueYearAsync();
        var specialSlots = leagueYear.Settings.SpecialGameSlots.OrderBy(s => s.SpecialSlotPosition).ToList();
        var specialSlotsInRoster = GetPublisher(leagueYear, league.Publishers[0].PublisherID).GameSlots
            .Where(s => s.SpecialSlot is not null)
            .ToList();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(leagueYear.Settings.StandardGames, Is.EqualTo(5));
            Assert.That(leagueYear.Settings.CounterPicks, Is.EqualTo(1));
            Assert.That(specialSlots, Has.Count.EqualTo(2));
            Assert.That(specialSlots[0].RequiredTags, Is.EquivalentTo(new[] { NewGame }));
            Assert.That(specialSlots[1].RequiredTags, Is.EquivalentTo(new[] { Remake, Remaster }));
            Assert.That(specialSlotsInRoster.Select(s => s.SlotNumber), Is.EqualTo(new[] { 3, 4 }),
                "Special slots are the last standard slots.");
        }

        AssertSlotsAreConsistent(leagueYear);
    }
}
