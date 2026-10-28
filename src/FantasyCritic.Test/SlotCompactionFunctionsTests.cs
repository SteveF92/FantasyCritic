using System;
using System.Collections.Generic;
using System.Linq;
using FantasyCritic.FakeRepo.TestUtilities;
using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Identity;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class SlotCompactionFunctionsTests
{
    private static readonly int _year = 2026;

    // Leagues without special slots

    [Test]
    public void GetCompactedSlotAssignments_WhenStandardGamesAlreadyCompact_ReturnsEmpty()
    {
        var pair = BuildPair(standardGames: 5, specialSlotCount: 0, standard: [(0, 1), (1, 2), (2, 3)]);

        var assignments = SlotCompactionFunctions.GetCompactedSlotAssignments([pair]);

        Assert.That(assignments, Is.Empty);
    }

    [Test]
    public void GetCompactedSlotAssignments_WhenStandardGameLeavesAGap_ClosesIt()
    {
        // Games in slots 0, 2, 3 (the game in slot 1 was dropped).
        var pair = BuildPair(standardGames: 5, specialSlotCount: 0, standard: [(0, 1), (2, 2), (3, 3)]);
        var gamesBySlot = GamesBySlot(pair);

        var assignments = SlotCompactionFunctions.GetCompactedSlotAssignments([pair]);

        Assert.Multiple(() =>
        {
            Assert.That(assignments, Has.Count.EqualTo(2));
            Assert.That(assignments.ContainsKey(gamesBySlot[0]), Is.False, "A game already in the lowest slot should not move.");
            Assert.That(assignments[gamesBySlot[2]].SlotNumber, Is.EqualTo(1));
            Assert.That(assignments[gamesBySlot[3]].SlotNumber, Is.EqualTo(2));
        });
    }

    [Test]
    public void GetCompactedSlotAssignments_OrdersStandardGamesByAcquisitionTime()
    {
        // The game in slot 0 was acquired after the game in slot 1, so they swap.
        var pair = BuildPair(standardGames: 5, specialSlotCount: 0, standard: [(0, 2), (1, 1)]);
        var gamesBySlot = GamesBySlot(pair);

        var assignments = SlotCompactionFunctions.GetCompactedSlotAssignments([pair]);

        Assert.Multiple(() =>
        {
            Assert.That(assignments[gamesBySlot[1]].SlotNumber, Is.EqualTo(0));
            Assert.That(assignments[gamesBySlot[0]].SlotNumber, Is.EqualTo(1));
        });
    }

    [Test]
    public void GetCompactedSlotAssignments_KeepsEveryGameInASlotOfItsOwnKind()
    {
        var pair = BuildPair(standardGames: 5, specialSlotCount: 0, standard: [(2, 1), (4, 2)], counterPicks: 2, counterPick: [(1, 3)]);

        var assignments = SlotCompactionFunctions.GetCompactedSlotAssignments([pair]);

        Assert.Multiple(() =>
        {
            Assert.That(assignments, Is.Not.Empty);
            foreach (var (game, slot) in assignments)
            {
                Assert.That(pair.LeagueYear.Options.LeagueSlotDictionary[slot.LeagueSlotID], Is.SameAs(slot));
                Assert.That(slot.CounterPick, Is.EqualTo(game.CounterPick), "A game must stay in a slot of its own kind.");
            }
        });
    }

    // Leagues with special slots

    [Test]
    public void GetCompactedSlotAssignments_WhenLeagueHasSpecialSlots_LeavesStandardGapsAlone()
    {
        // 5 standard slots, the last 2 special. A gap is a choice here, so nothing moves.
        var pair = BuildPair(standardGames: 5, specialSlotCount: 2, standard: [(0, 1), (2, 2), (4, 3)]);

        var assignments = SlotCompactionFunctions.GetCompactedSlotAssignments([pair]);

        Assert.That(assignments, Is.Empty);
    }

    // Counter picks

    [Test]
    public void GetCompactedSlotAssignments_ClosesCounterPickGaps()
    {
        var pair = BuildPair(standardGames: 5, specialSlotCount: 0, standard: [], counterPicks: 3, counterPick: [(1, 1), (2, 2)]);
        var counterPicksBySlot = GamesBySlot(pair, counterPick: true);

        var assignments = SlotCompactionFunctions.GetCompactedSlotAssignments([pair]);

        Assert.Multiple(() =>
        {
            Assert.That(assignments[counterPicksBySlot[1]].SlotNumber, Is.EqualTo(0));
            Assert.That(assignments[counterPicksBySlot[2]].SlotNumber, Is.EqualTo(1));
            Assert.That(assignments.Values, Has.All.Property(nameof(LeagueSlot.CounterPick)).True);
        });
    }

    [Test]
    public void GetCompactedSlotAssignments_ClosesCounterPickGapsEvenWithSpecialSlots()
    {
        var pair = BuildPair(standardGames: 5, specialSlotCount: 2, standard: [(1, 1)], counterPicks: 3, counterPick: [(2, 1)]);
        var counterPicksBySlot = GamesBySlot(pair, counterPick: true);

        var assignments = SlotCompactionFunctions.GetCompactedSlotAssignments([pair]);

        Assert.Multiple(() =>
        {
            Assert.That(assignments, Has.Count.EqualTo(1));
            Assert.That(assignments[counterPicksBySlot[2]].SlotNumber, Is.EqualTo(0));
        });
    }

    // Several publishers

    [Test]
    public void GetCompactedSlotAssignments_HandlesEachPublisherSeparately()
    {
        var gapped = BuildPair(standardGames: 5, specialSlotCount: 0, standard: [(3, 1)]);
        var compact = BuildPair(standardGames: 5, specialSlotCount: 0, standard: [(0, 1), (1, 2)]);

        var assignments = SlotCompactionFunctions.GetCompactedSlotAssignments([gapped, compact]);

        Assert.Multiple(() =>
        {
            Assert.That(assignments, Has.Count.EqualTo(1));
            Assert.That(assignments.Keys.Single().PublisherID, Is.EqualTo(gapped.Publisher.PublisherID));
            Assert.That(assignments.Values.Single().SlotNumber, Is.EqualTo(0));
        });
    }

    // Helpers

    private static Dictionary<int, PublisherGame> GamesBySlot(LeagueYearPublisherPair pair, bool counterPick = false)
    {
        var slots = pair.LeagueYear.Options.LeagueSlotDictionary;
        return pair.Publisher.PublisherGames
            .Where(x => x.CounterPick == counterPick)
            .ToDictionary(x => slots[x.LeagueSlotID].SlotNumber);
    }

    /// <param name="standard">(slot number, acquisition hour) for each standard game.</param>
    /// <param name="counterPick">(slot number, acquisition hour) for each counter pick.</param>
    private static LeagueYearPublisherPair BuildPair(int standardGames, int specialSlotCount, IReadOnlyList<(int Slot, int Hour)> standard,
        int counterPicks = 0, IReadOnlyList<(int Slot, int Hour)>? counterPick = null)
    {
        var leagueID = Guid.NewGuid();
        var leagueYearKey = new LeagueYearKey(leagueID, _year);
        var draftID = Guid.NewGuid();
        var publisherID = Guid.NewGuid();

        var league = new League(leagueID, "Test League",
            new MinimalFantasyCriticUser(Guid.NewGuid(), "Manager", "manager@test.com"), null, null,
            [new MinimalLeagueYearInfo(_year, false, true)],
            true, false, false, false, 0);
        var supportedYear = new SupportedYear(_year, true, true, true, new LocalDate(_year - 1, 12, 8), false);
        var options = BuildLeagueOptions(standardGames, specialSlotCount, counterPicks);

        var publisherDraftInfo = new List<PublisherDraftInfo> { new(draftID, 1, publisherID, 1, []) };
        var draft = new LeagueDraft(draftID, leagueYearKey, 1, "Draft 1",
            null, standardGames, counterPicks, false, true, PlayStatus.DraftFinal, publisherDraftInfo, null);

        var publisherGames = standard
            .Select(x => CreatePublisherGame(publisherID, options.LeagueSlots.Single(s => !s.CounterPick && s.SlotNumber == x.Slot), false, x.Hour))
            .Concat((counterPick ?? [])
                .Select(x => CreatePublisherGame(publisherID, options.LeagueSlots.Single(s => s.CounterPick && s.SlotNumber == x.Slot), true, x.Hour)))
            .ToList();

        var publisher = new Publisher(
            publisherID, leagueYearKey,
            FantasyCriticUser.GetFakeUser(),
            "Publisher 1", null, null,
            publisherDraftInfo,
            publisherGames, [],
            100, 0, 0, 0, 0,
            new AutoDraftSettings(AutoDraftMode.Off, false));

        var leagueYear = new LeagueYear(league, supportedYear, options, [draft], [], [], null, [publisher], null, false, null);
        return new LeagueYearPublisherPair(leagueYear, publisher);
    }

    private static LeagueOptions BuildLeagueOptions(int standardGames, int specialSlotCount, int counterPicks)
    {
        var regularSlotCount = standardGames - specialSlotCount;
        var regularSlots = Enumerable.Range(0, regularSlotCount)
            .Select(position => new LeagueSlot(Guid.NewGuid(), false, position, null, []));
        var specialSlots = Enumerable.Range(0, specialSlotCount)
            .Select(position => new LeagueSlot(Guid.NewGuid(), false, regularSlotCount + position, null,
                [MasterGameTagDictionary.TagDictionary["NG"]]));
        var counterPickSlots = Enumerable.Range(0, counterPicks)
            .Select(position => new LeagueSlot(Guid.NewGuid(), true, position, null, []));
        var leagueSlots = regularSlots.Concat(specialSlots).Concat(counterPickSlots).ToList();

        return new LeagueOptions(
            5, 1, 0,
            false, false, false, false,
            0, false,
            [], leagueSlots,
            PickupSystem.SemiPublicBiddingSecretCounterPicks,
            ScoringSystem.GetDefaultScoringSystem(_year),
            TradingSystem.Standard,
            TiebreakSystem.LowestProjectedPoints,
            ReleaseSystem.MustBeReleased,
            IneligibleGameSystem.CaseByCase,
            new AnnualDate(12, 1),
            null,
            false);
    }

    private static PublisherGame CreatePublisherGame(Guid publisherID, LeagueSlot leagueSlot, bool counterPick, int acquiredHour)
    {
        var gameName = $"Game-{(counterPick ? "CP" : "S")}{leagueSlot.SlotNumber}";
        var masterGame = new MasterGame(Guid.NewGuid(), gameName, "2026-06-01",
            new LocalDate(2026, 6, 1), new LocalDate(2026, 6, 1),
            null, null, null, new LocalDate(2026, 6, 1), null, null, null, null,
            false, null, "", null, null, null,
            false, false, false, false,
            Instant.MinValue,
            new FantasyCriticUser { Id = Guid.Empty }.ToVeryMinimal(),
            [], [MasterGameTagDictionary.TagDictionary["NG"]]);

        return new PublisherGame(
            publisherID, Guid.NewGuid(), gameName,
            Instant.FromUtc(_year, 1, 1, acquiredHour, 0), counterPick, null, false, null,
            new MasterGameYear(masterGame, 2026),
            leagueSlot.LeagueSlotID, null, null, null, null, null);
    }
}
