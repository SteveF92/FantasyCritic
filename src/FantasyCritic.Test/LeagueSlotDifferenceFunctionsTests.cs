using System;
using System.Collections.Generic;
using System.Linq;
using FantasyCritic.Lib;
using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class LeagueSlotDifferenceFunctionsTests
{
    private static readonly MasterGameTag _newGame = BuildTag("NewGame", "New Game");
    private static readonly MasterGameTag _remake = BuildTag("Remake", "Remake");
    private static readonly MasterGameTag _remaster = BuildTag("Remaster", "Remaster");

    [Test]
    public void GetSlotDifferences_WhenNothingChanged_ReturnsEmpty()
    {
        var slots = BuildRegularSlots(4);
        var copies = slots.Select(x => new LeagueSlot(x.LeagueSlotID, x.CounterPick, x.SlotNumber, x.SlotName, x.RequiredTags)).ToList();

        var differences = LeagueSlotDifferenceFunctions.GetSlotDifferences(slots, copies);

        Assert.That(differences, Is.Empty);
    }

    [Test]
    public void GetSlotDifferences_WhenTagsReordered_ReturnsEmpty()
    {
        var slot = new LeagueSlot(Guid.NewGuid(), false, 0, null, [_remake, _remaster]);
        var reordered = new LeagueSlot(slot.LeagueSlotID, false, 0, null, [_remaster, _remake]);

        var differences = LeagueSlotDifferenceFunctions.GetSlotDifferences([slot], [reordered]);

        Assert.That(differences, Is.Empty);
    }

    [Test]
    public void GetSlotDifferences_WhenOnlyTagsChange_ReportsTheSlot()
    {
        var slots = BuildRegularSlots(3);
        slots[2] = new LeagueSlot(slots[2].LeagueSlotID, false, 2, null, [_newGame]);
        var edited = slots.ToList();
        edited[2] = new LeagueSlot(slots[2].LeagueSlotID, false, 2, null, [_remake, _newGame]);

        var differences = LeagueSlotDifferenceFunctions.GetSlotDifferences(slots, edited);

        Assert.That(differences, Is.EqualTo(new[] { "Slot 3 required tags changed from 'New Game' to 'New Game' or 'Remake'." }));
    }

    [Test]
    public void GetSlotDifferences_WhenOnlyNameChanges_ReportsTheSlot()
    {
        var slot = new LeagueSlot(Guid.NewGuid(), false, 0, null, []);
        var renamed = new LeagueSlot(slot.LeagueSlotID, false, 0, "Indie Darling", []);

        var differences = LeagueSlotDifferenceFunctions.GetSlotDifferences([slot], [renamed]);

        Assert.That(differences, Is.EqualTo(new[] { "Slot 1 name changed from none to 'Indie Darling'." }));
    }

    [Test]
    public void GetSlotDifferences_WhenSlotsAddedAndRemoved_NamesThemWithTheirRequirements()
    {
        var slots = BuildRegularSlots(2);
        slots.Add(new LeagueSlot(Guid.NewGuid(), false, 2, null, [_remake]));
        var counterPick = new LeagueSlot(Guid.NewGuid(), true, 0, null, []);
        var added = new LeagueSlot(Guid.NewGuid(), false, 2, null, [_remake, _remaster]);

        var differences = LeagueSlotDifferenceFunctions.GetSlotDifferences(slots, [slots[0], slots[1], added, counterPick]);

        Assert.That(differences, Is.EqualTo(new[]
        {
            "Slot 3 removed (requiring 'Remake').",
            "Slot 3 added (requiring 'Remake' or 'Remaster').",
            "Counter pick slot 1 added.",
        }));
    }

    [Test]
    public void GetSlotDifferences_WhenEarlierSlotRemoved_DoesNotReportTheRenumberingAsMoves()
    {
        // Removing slot 2 of 4 shifts slots 3 and 4 down one. Only the removal and the real tag edit are reported,
        // and the edited slot says what number it had.
        var slots = BuildRegularSlots(3);
        slots.Add(new LeagueSlot(Guid.NewGuid(), false, 3, null, [_newGame]));
        var edited = new List<LeagueSlot>
        {
            slots[0],
            new(slots[2].LeagueSlotID, false, 1, null, []),
            new(slots[3].LeagueSlotID, false, 2, null, [_remake]),
        };

        var differences = LeagueSlotDifferenceFunctions.GetSlotDifferences(slots, edited);

        Assert.That(differences, Is.EqualTo(new[]
        {
            "Slot 2 removed.",
            "Slot 3 (was slot 4) required tags changed from 'New Game' to 'Remake'.",
        }));
    }

    [Test]
    public void GetSlotDifferences_WhenSlotsSwapped_ReportsBothMoves()
    {
        var slots = BuildRegularSlots(3);
        var edited = new List<LeagueSlot>
        {
            slots[0],
            new(slots[1].LeagueSlotID, false, 2, null, []),
            new(slots[2].LeagueSlotID, false, 1, null, []),
        };

        var differences = LeagueSlotDifferenceFunctions.GetSlotDifferences(slots, edited);

        Assert.That(differences, Is.EqualTo(new[] { "Slot 3 moved to slot 2.", "Slot 2 moved to slot 3." }));
    }

    [Test]
    public void GetSlotDifferences_WhenSlotChangesKind_ReportsRemovedAndAdded()
    {
        var slot = new LeagueSlot(Guid.NewGuid(), false, 0, null, []);
        var counterPick = new LeagueSlot(slot.LeagueSlotID, true, 0, null, []);

        var differences = LeagueSlotDifferenceFunctions.GetSlotDifferences([slot], [counterPick]);

        Assert.That(differences, Is.EqualTo(new[] { "Slot 1 removed.", "Counter pick slot 1 added." }));
    }

    [Test]
    public void GetDifferences_WhenOnlySlotTagsChange_HasChanges()
    {
        var slots = BuildRegularSlots(3);
        var existingOptions = BuildLeagueOptions(slots);
        var edited = slots.ToList();
        edited[2] = new LeagueSlot(slots[2].LeagueSlotID, false, 2, null, [_newGame]);
        var newOptions = BuildLeagueOptions(edited);

        var differences = newOptions.GetDifferences(existingOptions);

        Assert.That(differences.Differences, Is.EqualTo(new[] { "Slot 3 required tags changed from none to 'New Game'." }));
    }

    private static List<LeagueSlot> BuildRegularSlots(int count)
    {
        return Enumerable.Range(0, count).Select(x => new LeagueSlot(Guid.NewGuid(), false, x, null, [])).ToList();
    }

    private static MasterGameTag BuildTag(string name, string readableName)
    {
        return new MasterGameTag(name, readableName, name, new MasterGameTagType("RemakeLevel"), false, false, "", [], "");
    }

    private static LeagueOptions BuildLeagueOptions(IReadOnlyList<LeagueSlot> slots)
    {
        return new LeagueOptions(
            5, 1, 0,
            false, false, false, false,
            0, false,
            [], slots,
            PickupSystem.SemiPublicBiddingSecretCounterPicks,
            ScoringSystem.GetDefaultScoringSystem(2026),
            TradingSystem.Standard,
            TiebreakSystem.LowestProjectedPoints,
            ReleaseSystem.MustBeReleased,
            IneligibleGameSystem.CaseByCase,
            new AnnualDate(12, 1),
            null,
            false);
    }
}
