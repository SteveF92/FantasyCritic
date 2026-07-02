using System;
using System.Collections.Generic;
using System.Linq;
using FantasyCritic.FakeRepo.TestUtilities;
using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class SlotAssignmentFunctionsTests
{
    [Test]
    public void GetNewSlotAssignments_WhenStandardGamesUnchanged_ReturnsEmpty()
    {
        var leagueYear = BuildLeagueYear(standardGames: 8, specialSlotCount: 2, filledStandardSlotNumbers: [0, 1, 6, 7]);

        var assignments = SlotAssignmentFunctions.GetNewSlotAssignments(8, leagueYear, leagueYear.Publishers);

        Assert.That(assignments, Is.Empty);
    }

    [Test]
    public void GetNewSlotAssignments_WhenStandardGamesExpand_KeepsNormalSlotsCompacted()
    {
        var leagueYear = BuildLeagueYear(standardGames: 8, specialSlotCount: 2, filledStandardSlotNumbers: [0, 1, 2, 3, 4, 5, 6, 7]);
        var publisher = leagueYear.Publishers.Single();
        var normalGames = publisher.PublisherGames.Where(g => g.SlotNumber is 0 or 1 or 2 or 3 or 4 or 5).ToList();

        var assignments = SlotAssignmentFunctions.GetNewSlotAssignments(13, leagueYear, leagueYear.Publishers);

        Assert.Multiple(() =>
        {
            foreach (var (index, game) in normalGames.Select((game, index) => (index, game)))
            {
                Assert.That(assignments[game.PublisherGameID], Is.EqualTo(index),
                    $"Normal game in slot {game.SlotNumber} should compact to {index}.");
            }
        });
    }

    [Test]
    public void GetNewSlotAssignments_WhenStandardGamesExpand_ShiftsSpecialSlotGamesForward()
    {
        var leagueYear = BuildLeagueYear(standardGames: 8, specialSlotCount: 2, filledStandardSlotNumbers: [0, 1, 2, 3, 4, 5, 6, 7]);
        var publisher = leagueYear.Publishers.Single();
        var specialGames = publisher.PublisherGames.Where(g => g.SlotNumber is 6 or 7).ToList();

        var assignments = SlotAssignmentFunctions.GetNewSlotAssignments(13, leagueYear, leagueYear.Publishers);

        Assert.Multiple(() =>
        {
            Assert.That(assignments[specialGames.Single(g => g.SlotNumber == 6).PublisherGameID], Is.EqualTo(11));
            Assert.That(assignments[specialGames.Single(g => g.SlotNumber == 7).PublisherGameID], Is.EqualTo(12));
        });
    }

    [Test]
    public void GetNewSlotAssignments_WhenShiftWouldCollide_CompactsAllFilledSlots()
    {
        var leagueYear = BuildLeagueYear(standardGames: 10, specialSlotCount: 2, filledStandardSlotNumbers: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);
        var publisher = leagueYear.Publishers.Single();
        var gamesInSlotOrder = publisher.GetPublisherSlots(leagueYear)
            .Where(slot => !slot.CounterPick && slot.PublisherGame is not null)
            .Select(slot => slot.PublisherGame!)
            .ToList();

        var assignments = SlotAssignmentFunctions.GetNewSlotAssignments(8, leagueYear, leagueYear.Publishers);

        Assert.Multiple(() =>
        {
            foreach (var (expectedSlot, game) in gamesInSlotOrder.Select((game, index) => (index, game)))
            {
                Assert.That(assignments[game.PublisherGameID], Is.EqualTo(expectedSlot),
                    $"Fallback should compact game '{game.GameName}' to slot {expectedSlot}.");
            }
        });
    }

    private static LeagueYear BuildLeagueYear(int standardGames, int specialSlotCount, IReadOnlyList<int> filledStandardSlotNumbers)
    {
        var leagueID = Guid.NewGuid();
        var year = 2026;
        var leagueYearKey = new LeagueYearKey(leagueID, year);
        var draftID = Guid.NewGuid();
        var publisherID = Guid.NewGuid();

        var league = new League(leagueID, "Test League", new MinimalFantasyCriticUser(Guid.NewGuid(), "Manager", "manager@test.com"), null, null,
            [new MinimalLeagueYearInfo(year, false, true)],
            true, false, false, false, 0);

        var supportedYear = new SupportedYear(year, true, true, true, new LocalDate(year - 1, 12, 8), false);

        var specialSlots = Enumerable.Range(0, specialSlotCount)
            .Select(position => new SpecialGameSlot(position, [MasterGameTagDictionary.TagDictionary["NG"]]))
            .ToList();

        var options = new LeagueOptions(
            standardGames, 0,
            5, 1, 0,
            false, false, false, false,
            0, false,
            [], specialSlots,
            DraftSystem.Flexible,
            PickupSystem.SemiPublicBiddingSecretCounterPicks,
            ScoringSystem.GetDefaultScoringSystem(year),
            TradingSystem.Standard,
            TiebreakSystem.LowestProjectedPoints,
            ReleaseSystem.MustBeReleased,
            IneligibleGameSystem.CaseByCase,
            new AnnualDate(12, 1),
            null,
            false);

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
            publisherID,
            leagueYearKey,
            FantasyCriticUser.GetFakeUser(),
            "Publisher 1",
            null,
            null,
            publisherDraftInfo,
            publisherGames,
            [],
            100,
            0,
            0,
            0,
            0,
            new AutoDraftSettings(AutoDraftMode.Off, false));

        return new LeagueYear(league, supportedYear, options, [draft], [], [], null, [publisher], null, false, null);
    }

    private static PublisherGame CreatePublisherGame(Guid publisherID, int slotNumber, string gameName)
    {
        var masterGame = new MasterGame(Guid.NewGuid(), gameName, "2026-06-01", new LocalDate(2026, 6, 1), new LocalDate(2026, 6, 1),
            null, null, null, new LocalDate(2026, 6, 1), null, null, null, null, false, null, "", null, null, null, false, false, false, false,
            Instant.MinValue, new FantasyCriticUser { Id = Guid.Empty }.ToVeryMinimal(), [], [MasterGameTagDictionary.TagDictionary["NG"]]);

        return new PublisherGame(
            publisherID,
            Guid.NewGuid(),
            gameName,
            Instant.MinValue,
            false,
            null,
            false,
            null,
            new MasterGameYear(masterGame, 2026),
            slotNumber,
            null,
            null,
            null,
            null);
    }
}
