using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;
using static FantasyCritic.IntegrationTests.Helpers.SlotTestHelpers;

namespace FantasyCritic.IntegrationTests.Tests.League.Slots;

/// <summary>
/// ReorderPublisherGames, which takes slot number to game for every standard slot, as the publisher page sends it.
/// Swaps go through a transient state where two games want one slot, and a game can't be moved into a special slot
/// it doesn't fit unless the league allows it.
/// </summary>
[TestFixture]
public class ReorderPublisherGamesTests : IntegrationTestBase
{
    // Slots 0-2 regular, slot 3 requires New Game.
    private static readonly LeagueScenario OneSpecialSlot = Scenario("ReorderPublisherGames", 4, 1, [[NewGame]]);

    /// <summary>
    /// Remake in slot 0, a New Game in slot 1, slot 2 open, a New Game in the special slot (3).
    /// </summary>
    private sealed record Roster(
        LeagueFixture League,
        TestPublisher Publisher,
        PublisherGameViewModel Remake,
        PublisherGameViewModel RegularSlotNewGame,
        PublisherGameViewModel SpecialSlotNewGame);

    private async Task<Roster> BuildRosterAsync()
    {
        var league = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(Factory, OneSpecialSlot, NewUser);
        var games = await GetTaggedGamesAsync(league, Remake, NewGame, NewGame);
        var publisher = league.Publishers[0];
        var remake = await ClaimAsync(league, publisher, games[0]);
        var specialSlotNewGame = await ClaimAsync(league, publisher, games[1]);
        var regularSlotNewGame = await ClaimAsync(league, publisher, games[2]);

        var placements = GetPlacements(await league.GetLeagueYearAsync());
        using (Assert.EnterMultipleScope())
        {
            Assert.That(placements[remake.PublisherGameID], Is.EqualTo("0"), "Setup");
            Assert.That(placements[regularSlotNewGame.PublisherGameID], Is.EqualTo("1"), "Setup");
            Assert.That(placements[specialSlotNewGame.PublisherGameID], Is.EqualTo("3 (special 0: NewGame)"), "Setup");
        }

        return new Roster(league, publisher, remake, regularSlotNewGame, specialSlotNewGame);
    }

    [Test]
    public async Task SwappingTwoRegularGames_SwapsTheirSlots()
    {
        var roster = await BuildRosterAsync();
        await using var league = roster.League;

        await ReorderAsync(roster, new Dictionary<int, Guid?>
        {
            [0] = roster.RegularSlotNewGame.PublisherGameID,
            [1] = roster.Remake.PublisherGameID,
            [2] = null,
            [3] = roster.SpecialSlotNewGame.PublisherGameID,
        });

        var after = await league.GetLeagueYearAsync();
        var placements = GetPlacements(after);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(placements[roster.RegularSlotNewGame.PublisherGameID], Is.EqualTo("0"));
            Assert.That(placements[roster.Remake.PublisherGameID], Is.EqualTo("1"));
            Assert.That(placements[roster.SpecialSlotNewGame.PublisherGameID], Is.EqualTo("3 (special 0: NewGame)"));
        }

        AssertSlotsAreConsistent(after);
    }

    [Test]
    public async Task MovingAGameIntoAnOpenSlot_LeavesItsOldSlotOpen()
    {
        var roster = await BuildRosterAsync();
        await using var league = roster.League;

        await ReorderAsync(roster, new Dictionary<int, Guid?>
        {
            [0] = null,
            [1] = roster.RegularSlotNewGame.PublisherGameID,
            [2] = roster.Remake.PublisherGameID,
            [3] = roster.SpecialSlotNewGame.PublisherGameID,
        });

        var after = await league.GetLeagueYearAsync();
        var publisher = GetPublisher(after, roster.Publisher.PublisherID);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(GetPlacements(after)[roster.Remake.PublisherGameID], Is.EqualTo("2"));
            Assert.That(GetStandardSlot(publisher, 0).PublisherGame, Is.Null);
        }

        AssertSlotsAreConsistent(after);
    }

    [Test]
    public async Task SwappingTheSpecialSlotGameWithAGameThatAlsoFits_SwapsTheirSlots()
    {
        var roster = await BuildRosterAsync();
        await using var league = roster.League;

        await ReorderAsync(roster, new Dictionary<int, Guid?>
        {
            [0] = roster.Remake.PublisherGameID,
            [1] = roster.SpecialSlotNewGame.PublisherGameID,
            [2] = null,
            [3] = roster.RegularSlotNewGame.PublisherGameID,
        });

        var after = await league.GetLeagueYearAsync();
        var placements = GetPlacements(after);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(placements[roster.SpecialSlotNewGame.PublisherGameID], Is.EqualTo("1"));
            Assert.That(placements[roster.RegularSlotNewGame.PublisherGameID], Is.EqualTo("3 (special 0: NewGame)"));
        }

        AssertSlotsAreConsistent(after);
    }

    [Test]
    public async Task MovingAGameIntoASpecialSlotItDoesNotFit_IsRefusedAndMovesNothing()
    {
        var roster = await BuildRosterAsync();
        await using var league = roster.League;
        var before = GetPlacements(await league.GetLeagueYearAsync());

        var exception = Assert.ThrowsAsync<ApiException>(() => ReorderAsync(roster, SwapRemakeIntoSpecialSlot(roster)));

        var after = await league.GetLeagueYearAsync();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(400));
            Assert.That(GetPlacements(after), Is.EquivalentTo(before));
        }
    }

    [Test]
    public async Task MovingAGameIntoASpecialSlotItDoesNotFit_IsAllowedWhenTheLeagueAllowsIt()
    {
        var roster = await BuildRosterAsync();
        await using var league = roster.League;
        await EditSettingsAsync(league, s => s.AllowMoveIntoIneligible = true);

        await ReorderAsync(roster, SwapRemakeIntoSpecialSlot(roster));

        var after = await league.GetLeagueYearAsync();
        var remakeSlot = GetSlotOf(after, roster.Remake.PublisherGameID);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(DescribeSlot(remakeSlot), Is.EqualTo("3 (special 0: NewGame)"));
            Assert.That(remakeSlot.GameMeetsSlotCriteria, Is.False);
            Assert.That(GetPlacements(after)[roster.SpecialSlotNewGame.PublisherGameID], Is.EqualTo("0"));
        }

        AssertSlotsAreConsistent(after);
    }

    private static Dictionary<int, Guid?> SwapRemakeIntoSpecialSlot(Roster roster) => new()
    {
        [0] = roster.SpecialSlotNewGame.PublisherGameID,
        [1] = roster.RegularSlotNewGame.PublisherGameID,
        [2] = null,
        [3] = roster.Remake.PublisherGameID,
    };

    private static Task ReorderAsync(Roster roster, Dictionary<int, Guid?> slotStates) =>
        roster.Publisher.Session.League.ReorderPublisherGamesAsync(new ReorderPublisherGamesRequest
        {
            PublisherID = roster.Publisher.PublisherID,
            SlotStates = slotStates.ToDictionary(x => x.Key.ToString(), x => x.Value),
        });
}
