using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;
using static FantasyCritic.IntegrationTests.Helpers.SlotTestHelpers;

namespace FantasyCritic.IntegrationTests.Tests.League.Slots;

/// <summary>
/// League year settings edits on a roster with games in it, through the "standard games + counter picks + special
/// slots" request shape. Each test edits one thing and checks that games stay with the slots they were in: regular
/// games keep their numbers, the special slot's game stays in that special slot, counter picks stay put.
/// </summary>
[TestFixture]
public class SlotSettingsEditTests : IntegrationTestBase
{
    // Slots 0-2 regular, slot 3 requires New Game; two counter-pick slots.
    private static readonly LeagueScenario OneSpecialSlot = Scenario("SlotSettingsEdit", 4, 2, [[NewGame]]);

    /// <summary>
    /// Publisher 1: Remake in slot 0, Remake in slot 1, New Game in the special slot (3); slot 2 open.
    /// Publisher 2: Remake in slot 0, and a counter pick of publisher 1's first game in counter-pick slot 0.
    /// </summary>
    private sealed record Roster(
        LeagueFixture League,
        PublisherGameViewModel FirstRemake,
        PublisherGameViewModel SecondRemake,
        PublisherGameViewModel SpecialSlotGame,
        PublisherGameViewModel OtherPublisherGame,
        PublisherGameViewModel CounterPick);

    private async Task<Roster> BuildRosterAsync()
    {
        var league = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(Factory, OneSpecialSlot, NewUser);
        var games = await GetTaggedGamesAsync(league, Remake, NewGame, Remake, Remake);
        var publisher = league.Publishers[0];
        var otherPublisher = league.Publishers[1];

        var firstRemake = await ClaimAsync(league, publisher, games[0]);
        var specialSlotGame = await ClaimAsync(league, publisher, games[1]);
        var secondRemake = await ClaimAsync(league, publisher, games[2]);
        var otherPublisherGame = await ClaimAsync(league, otherPublisher, games[3]);
        var counterPick = await ClaimAsync(league, otherPublisher, games[0], counterPick: true);

        var placements = GetPlacements(await league.GetLeagueYearAsync());
        using (Assert.EnterMultipleScope())
        {
            Assert.That(placements[specialSlotGame.PublisherGameID], Is.EqualTo("3 (special 0: NewGame)"),
                "Setup: the New Game should start in the special slot.");
            Assert.That(placements[secondRemake.PublisherGameID], Is.EqualTo("1"), "Setup: slot 2 should be the open one.");
        }

        return new Roster(league, firstRemake, secondRemake, specialSlotGame, otherPublisherGame, counterPick);
    }

    [Test]
    public async Task SavingUnchangedSettings_MovesNothing()
    {
        var roster = await BuildRosterAsync();
        await using var league = roster.League;
        var before = GetPlacements(await league.GetLeagueYearAsync());

        await EditSettingsAsync(league, _ => { });

        var after = await league.GetLeagueYearAsync();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(GetPlacements(after), Is.EquivalentTo(before));
            Assert.That(after.Settings.SpecialGameSlots.Single().RequiredTags, Is.EquivalentTo(new[] { NewGame }));
        }

        AssertSlotsAreConsistent(after);
    }

    [Test]
    public async Task AddingStandardAndCounterPickSlots_KeepsEveryGameWithItsSlot()
    {
        var roster = await BuildRosterAsync();
        await using var league = roster.League;

        await EditSettingsAsync(league, s =>
        {
            s.StandardGames += 2;
            s.CounterPicks += 1;
        });

        var after = await league.GetLeagueYearAsync();
        var placements = GetPlacements(after);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(after.Settings.StandardGames, Is.EqualTo(6));
            Assert.That(after.Settings.CounterPicks, Is.EqualTo(3));
            Assert.That(placements[roster.FirstRemake.PublisherGameID], Is.EqualTo("0"));
            Assert.That(placements[roster.SecondRemake.PublisherGameID], Is.EqualTo("1"));
            Assert.That(placements[roster.SpecialSlotGame.PublisherGameID], Is.EqualTo("5 (special 0: NewGame)"),
                "The special slot is still the last slot, and its game goes with it.");
            Assert.That(placements[roster.OtherPublisherGame.PublisherGameID], Is.EqualTo("0"));
            Assert.That(placements[roster.CounterPick.PublisherGameID], Is.EqualTo("CP 0"));
        }

        AssertSlotsAreConsistent(after);
    }

    [Test]
    public async Task RemovingAnOpenStandardSlot_KeepsEveryGameWithItsSlot()
    {
        var roster = await BuildRosterAsync();
        await using var league = roster.League;

        await EditSettingsAsync(league, s => s.StandardGames -= 1);

        var after = await league.GetLeagueYearAsync();
        var placements = GetPlacements(after);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(after.Settings.StandardGames, Is.EqualTo(3));
            Assert.That(placements[roster.FirstRemake.PublisherGameID], Is.EqualTo("0"));
            Assert.That(placements[roster.SecondRemake.PublisherGameID], Is.EqualTo("1"));
            Assert.That(placements[roster.SpecialSlotGame.PublisherGameID], Is.EqualTo("2 (special 0: NewGame)"));
            Assert.That(placements[roster.OtherPublisherGame.PublisherGameID], Is.EqualTo("0"));
            Assert.That(placements[roster.CounterPick.PublisherGameID], Is.EqualTo("CP 0"));
        }

        AssertSlotsAreConsistent(after);
    }

    [Test]
    public async Task ShrinkingTheRosterBelowAPublishersGames_IsRefused()
    {
        var roster = await BuildRosterAsync();
        await using var league = roster.League;
        var before = GetPlacements(await league.GetLeagueYearAsync());

        var exception = Assert.ThrowsAsync<ApiException>(() => EditSettingsAsync(league, s => s.StandardGames = 2));

        var after = await league.GetLeagueYearAsync();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(400));
            Assert.That(after.Settings.StandardGames, Is.EqualTo(4));
            Assert.That(GetPlacements(after), Is.EquivalentTo(before));
        }
    }

    [Test]
    public async Task ChangingOnlyASpecialSlotsTags_IsSavedAndKeepsItsGame()
    {
        var roster = await BuildRosterAsync();
        await using var league = roster.League;

        await EditSettingsAsync(league, s => s.SpecialGameSlots.Single().RequiredTags = [NewGame, Remake]);

        var after = await league.GetLeagueYearAsync();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(after.Settings.SpecialGameSlots.Single().RequiredTags, Is.EquivalentTo(new[] { NewGame, Remake }),
                "A change to the tags alone must still be saved.");
            Assert.That(GetPlacements(after)[roster.SpecialSlotGame.PublisherGameID], Is.EqualTo("3 (special 0: NewGame,Remake)"));
        }

        AssertSlotsAreConsistent(after);
    }

    [Test]
    public async Task ChangingASpecialSlotsTagsSoItsGameNoLongerFits_LeavesTheGameAndFlagsTheSlot()
    {
        var roster = await BuildRosterAsync();
        await using var league = roster.League;
        var before = GetPlacements(await league.GetLeagueYearAsync());

        await EditSettingsAsync(league, s => s.SpecialGameSlots.Single().RequiredTags = [Remaster]);

        var after = await league.GetLeagueYearAsync();
        var slot = GetSlotOf(after, roster.SpecialSlotGame.PublisherGameID);
        var expectedPlacements = new Dictionary<System.Guid, string>(before)
        {
            [roster.SpecialSlotGame.PublisherGameID] = "3 (special 0: Remaster)",
        };
        using (Assert.EnterMultipleScope())
        {
            Assert.That(GetPlacements(after), Is.EquivalentTo(expectedPlacements), "A tag change should not move any game.");
            Assert.That(slot.GameMeetsSlotCriteria, Is.False, "The New Game no longer meets a Remaster slot's requirement.");
            Assert.That(slot.EligibilityErrors, Is.Not.Empty);
        }

        AssertSlotsAreConsistent(after);
    }

    [Test]
    public async Task AddingASpecialSlot_KeepsTheExistingSpecialSlotGameInItsPosition()
    {
        var roster = await BuildRosterAsync();
        await using var league = roster.League;

        await EditSettingsAsync(league, s => s.SpecialGameSlots.Add(new SpecialGameSlotViewModel
        {
            SpecialSlotPosition = 1,
            RequiredTags = [Remake],
        }));

        var after = await league.GetLeagueYearAsync();
        var placements = GetPlacements(after);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(after.Settings.StandardGames, Is.EqualTo(4));
            Assert.That(after.Settings.SpecialGameSlots, Has.Count.EqualTo(2));
            Assert.That(placements[roster.FirstRemake.PublisherGameID], Is.EqualTo("0"));
            Assert.That(placements[roster.SecondRemake.PublisherGameID], Is.EqualTo("1"));
            Assert.That(placements[roster.SpecialSlotGame.PublisherGameID], Is.EqualTo("2 (special 0: NewGame)"),
                "Special slots stay last, so the New Game slot moves up to make room, and its game goes with it.");
            Assert.That(placements[roster.OtherPublisherGame.PublisherGameID], Is.EqualTo("0"));
        }

        AssertSlotsAreConsistent(after);
    }

    [Test]
    public async Task RemovingAStandardSlotWithAGameInIt_MovesTheGameIntoAnOpenRegularSlot()
    {
        var roster = await BuildRosterAsync();
        await using var league = roster.League;
        var otherPublisher = league.Publishers[1];
        var games = await GetTaggedGamesAsync(league, Remake, Remake);
        var secondGame = await ClaimAsync(league, otherPublisher, games[0]);
        var thirdGame = await ClaimAsync(league, otherPublisher, games[1]);
        await league.Manager.LeagueManager.RemovePublisherGameAsync(new GameRemoveRequest
        {
            PublisherID = otherPublisher.PublisherID,
            PublisherGameID = roster.OtherPublisherGame.PublisherGameID,
        });
        var before = GetPlacements(await league.GetLeagueYearAsync());
        Assert.That(new[] { before[secondGame.PublisherGameID], before[thirdGame.PublisherGameID] }, Is.EqualTo(new[] { "1", "2" }),
            "Setup: slot 0 should be open and slot 2, which the edit removes, filled.");

        await EditSettingsAsync(league, s => s.StandardGames -= 1);

        // Today every regular game is renumbered from 0; with slot rows only the displaced game moves, into slot 0.
        // Either way the two games fill the two regular slots that are left.
        var after = await league.GetLeagueYearAsync();
        var placements = GetPlacements(after);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(new[] { placements[secondGame.PublisherGameID], placements[thirdGame.PublisherGameID] },
                Is.EquivalentTo(new[] { "0", "1" }));
            Assert.That(placements[roster.SpecialSlotGame.PublisherGameID], Is.EqualTo("2 (special 0: NewGame)"));
        }

        AssertSlotsAreConsistent(after);
    }

    [Test]
    public async Task RemovingACounterPickSlotWithACounterPickInIt_MovesItIntoTheOpenCounterPickSlot()
    {
        var roster = await BuildRosterAsync();
        await using var league = roster.League;
        var secondCounterPick = await ClaimAsync(league, league.Publishers[1], roster.SecondRemake.MasterGame!, counterPick: true);
        await league.Manager.LeagueManager.RemovePublisherGameAsync(new GameRemoveRequest
        {
            PublisherID = league.Publishers[1].PublisherID,
            PublisherGameID = roster.CounterPick.PublisherGameID,
        });

        await EditSettingsAsync(league, s => s.CounterPicks -= 1);

        var after = await league.GetLeagueYearAsync();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(after.Settings.CounterPicks, Is.EqualTo(1));
            Assert.That(GetPlacements(after)[secondCounterPick.PublisherGameID], Is.EqualTo("CP 0"));
        }

        AssertSlotsAreConsistent(after);
    }

    [Test]
    public async Task RemovingAnOpenCounterPickSlot_KeepsTheCounterPick()
    {
        var roster = await BuildRosterAsync();
        await using var league = roster.League;

        await EditSettingsAsync(league, s => s.CounterPicks -= 1);

        var after = await league.GetLeagueYearAsync();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(after.Settings.CounterPicks, Is.EqualTo(1));
            Assert.That(GetPlacements(after)[roster.CounterPick.PublisherGameID], Is.EqualTo("CP 0"));
        }

        AssertSlotsAreConsistent(after);
    }
}
