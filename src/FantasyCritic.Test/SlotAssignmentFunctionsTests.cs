using System;
using System.Collections.Generic;
using System.Linq;
using FantasyCritic.FakeRepo.TestUtilities;
using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Identity;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class SlotAssignmentFunctionsTests
{
    // ── No-op cases ──────────────────────────────────────────────────────────

    [Test]
    public void GetNewSlotAssignments_WhenStandardGamesUnchanged_ReturnsEmpty()
    {
        var leagueYear = BuildLeagueYear(standardGames: 8, specialSlotCount: 2, filledStandardSlotNumbers: [0, 1, 6, 7]);
        var newOptions = BuildLeagueOptions(standardGames: 8, specialSlotCount: 2);

        var assignments = SlotAssignmentFunctions.GetNewSlotAssignments(leagueYear, newOptions, leagueYear.Publishers);

        Assert.That(assignments, Is.Empty);
    }

    [Test]
    public void GetNewSlotAssignments_WhenNewSpecialSlotsEqualNewStandardGames_ReturnsEmpty()
    {
        // Adding 2 std + 2 special: new special slots occupy the same absolute indices as
        // the old ones, so existing games do not need to move (shift = 0).
        // Before: 8 std, 2 special → special at indices 6, 7
        // After:  10 std, 4 special → special at indices 6, 7, 8, 9 (positions 0+1 still land at 6+7)
        var leagueYear = BuildLeagueYear(standardGames: 8, specialSlotCount: 2, filledStandardSlotNumbers: [0, 1, 2, 3, 4, 5, 6, 7]);
        var newOptions = BuildLeagueOptions(standardGames: 10, specialSlotCount: 4);

        var assignments = SlotAssignmentFunctions.GetNewSlotAssignments(leagueYear, newOptions, leagueYear.Publishers);

        Assert.That(assignments, Is.Empty);
    }

    // ── Normal-slot compaction ────────────────────────────────────────────────

    [Test]
    public void GetNewSlotAssignments_WhenStandardGamesExpand_KeepsNormalSlotsCompacted()
    {
        // Before: 8 std, 2 special (special at 6, 7). After: 13 std, 2 special (special at 11, 12).
        // Normal games (slots 0-5) should remain at 0-5.
        var leagueYear = BuildLeagueYear(standardGames: 8, specialSlotCount: 2, filledStandardSlotNumbers: [0, 1, 2, 3, 4, 5, 6, 7]);
        var publisher = leagueYear.Publishers.Single();
        var normalGames = publisher.PublisherGames.Where(g => g.SlotNumber is >= 0 and <= 5).ToList();
        var newOptions = BuildLeagueOptions(standardGames: 13, specialSlotCount: 2);

        var assignments = SlotAssignmentFunctions.GetNewSlotAssignments(leagueYear, newOptions, leagueYear.Publishers);

        Assert.Multiple(() =>
        {
            foreach (var (expectedSlot, game) in normalGames.Select((g, i) => (i, g)))
            {
                Assert.That(assignments[game.PublisherGameID], Is.EqualTo(expectedSlot),
                    $"Normal game originally in slot {game.SlotNumber} should compact to {expectedSlot}.");
            }
        });
    }

    // ── Special-slot shifting ─────────────────────────────────────────────────

    [Test]
    public void GetNewSlotAssignments_WhenStandardGamesExpandWithNoNewSpecialSlots_ShiftsSpecialSlotGamesByFullDelta()
    {
        // Before: 8 std, 2 special (special at 6, 7). After: 13 std, 2 special (special at 11, 12).
        // shift = (13-8) - (2-2) = 5.  Games at 6→11 and 7→12.
        var leagueYear = BuildLeagueYear(standardGames: 8, specialSlotCount: 2, filledStandardSlotNumbers: [0, 1, 2, 3, 4, 5, 6, 7]);
        var publisher = leagueYear.Publishers.Single();
        var specialGames = publisher.PublisherGames.Where(g => g.SlotNumber is 6 or 7).ToList();
        var newOptions = BuildLeagueOptions(standardGames: 13, specialSlotCount: 2);

        var assignments = SlotAssignmentFunctions.GetNewSlotAssignments(leagueYear, newOptions, leagueYear.Publishers);

        Assert.Multiple(() =>
        {
            Assert.That(assignments[specialGames.Single(g => g.SlotNumber == 6).PublisherGameID], Is.EqualTo(11));
            Assert.That(assignments[specialGames.Single(g => g.SlotNumber == 7).PublisherGameID], Is.EqualTo(12));
        });
    }

    [Test]
    public void GetNewSlotAssignments_WhenExpandingWithNewSpecialSlots_ExistingSpecialSlotsShiftByReducedAmount()
    {
        // Before: 8 std, 2 special (special at 6, 7).
        // After:  13 std, 4 special (special at 9, 10, 11, 12).
        // Existing special slots keep positions 0+1 → new absolute indices 9, 10.
        // shift = (13-8) - (4-2) = 3.  Games at 6→9 and 7→10.
        var leagueYear = BuildLeagueYear(standardGames: 8, specialSlotCount: 2, filledStandardSlotNumbers: [0, 1, 2, 3, 4, 5, 6, 7]);
        var publisher = leagueYear.Publishers.Single();
        var specialGames = publisher.PublisherGames.Where(g => g.SlotNumber is 6 or 7).ToList();
        var newOptions = BuildLeagueOptions(standardGames: 13, specialSlotCount: 4);

        var assignments = SlotAssignmentFunctions.GetNewSlotAssignments(leagueYear, newOptions, leagueYear.Publishers);

        Assert.Multiple(() =>
        {
            Assert.That(assignments[specialGames.Single(g => g.SlotNumber == 6).PublisherGameID], Is.EqualTo(9));
            Assert.That(assignments[specialGames.Single(g => g.SlotNumber == 7).PublisherGameID], Is.EqualTo(10));
        });
    }

    [Test]
    public void GetNewSlotAssignments_WhenExpandingWithNewSpecialSlots_NormalSlotsRemainsCompacted()
    {
        // Same expansion as above: normal games (slots 0-5) should still compact to 0-5.
        var leagueYear = BuildLeagueYear(standardGames: 8, specialSlotCount: 2, filledStandardSlotNumbers: [0, 1, 2, 3, 4, 5, 6, 7]);
        var publisher = leagueYear.Publishers.Single();
        var normalGames = publisher.PublisherGames.Where(g => g.SlotNumber is >= 0 and <= 5).ToList();
        var newOptions = BuildLeagueOptions(standardGames: 13, specialSlotCount: 4);

        var assignments = SlotAssignmentFunctions.GetNewSlotAssignments(leagueYear, newOptions, leagueYear.Publishers);

        Assert.Multiple(() =>
        {
            foreach (var (expectedSlot, game) in normalGames.Select((g, i) => (i, g)))
            {
                Assert.That(assignments[game.PublisherGameID], Is.EqualTo(expectedSlot),
                    $"Normal game in slot {game.SlotNumber} should stay at {expectedSlot} after expansion with new special slots.");
            }
        });
    }

    // ── Fallback (collision) ──────────────────────────────────────────────────

    [Test]
    public void GetNewSlotAssignments_WhenShiftWouldCollide_CompactsAllFilledSlots()
    {
        // 10 std, 2 special (special at 8, 9). All 10 slots filled.
        // Shrink to 8 std, 2 special (special at 6, 7).
        // shift = (8-10)-(2-2) = -2. Special games would shift to 6 and 7,
        // colliding with normal games already compacted there. Fallback: compact all.
        var leagueYear = BuildLeagueYear(standardGames: 10, specialSlotCount: 2, filledStandardSlotNumbers: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);
        var publisher = leagueYear.Publishers.Single();
        var gamesInSlotOrder = publisher.GetPublisherSlots(leagueYear)
            .Where(slot => !slot.CounterPick && slot.PublisherGame is not null)
            .Select(slot => slot.PublisherGame!)
            .ToList();
        var newOptions = BuildLeagueOptions(standardGames: 8, specialSlotCount: 2);

        var assignments = SlotAssignmentFunctions.GetNewSlotAssignments(leagueYear, newOptions, leagueYear.Publishers);

        Assert.Multiple(() =>
        {
            foreach (var (expectedSlot, game) in gamesInSlotOrder.Select((g, i) => (i, g)))
            {
                Assert.That(assignments[game.PublisherGameID], Is.EqualTo(expectedSlot),
                    $"Fallback should compact '{game.GameName}' to slot {expectedSlot}.");
            }
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static readonly int _year = 2026;

    private static LeagueOptions BuildLeagueOptions(int standardGames, int specialSlotCount)
    {
        var specialSlots = Enumerable.Range(0, specialSlotCount)
            .Select(position => new SpecialGameSlot(position, [MasterGameTagDictionary.TagDictionary["NG"]]))
            .ToList();

        return new LeagueOptions(
            standardGames, 0,
            5, 1, 0,
            false, false, false, false,
            0, false,
            [], specialSlots,
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

    private static LeagueYear BuildLeagueYear(int standardGames, int specialSlotCount, IReadOnlyList<int> filledStandardSlotNumbers)
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

        var options = BuildLeagueOptions(standardGames, specialSlotCount);

        var publisherDraftInfo = new List<PublisherDraftInfo>
        {
            new(draftID, 1, publisherID, 1, [])
        };

        var draft = new LeagueDraft(draftID, leagueYearKey, 1, "Draft 1",
            null, standardGames, 0, false, true, PlayStatus.DraftFinal, publisherDraftInfo, null);

        var publisherGames = filledStandardSlotNumbers
            .Select(slotNumber => CreatePublisherGame(publisherID, slotNumber, $"Game-{slotNumber}"))
            .ToList();

        var publisher = new Publisher(
            publisherID, leagueYearKey,
            FantasyCriticUser.GetFakeUser(),
            "Publisher 1", null, null,
            publisherDraftInfo,
            publisherGames, [],
            100, 0, 0, 0, 0,
            new AutoDraftSettings(AutoDraftMode.Off, false));

        return new LeagueYear(league, supportedYear, options, [draft], [], [], null, [publisher], null, false, null);
    }

    private static PublisherGame CreatePublisherGame(Guid publisherID, int slotNumber, string gameName)
    {
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
            Instant.MinValue, false, null, false, null,
            new MasterGameYear(masterGame, 2026),
            slotNumber, null, null, null, null, null);
    }
}
