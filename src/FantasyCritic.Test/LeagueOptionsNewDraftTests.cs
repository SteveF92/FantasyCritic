using System;
using System.Collections.Generic;
using System.Linq;
using FantasyCritic.Lib;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class LeagueOptionsNewDraftTests
{
    private static readonly MasterGameTag _newGame = BuildTag("NewGame", "New Game");
    private static readonly MasterGameTag _remake = BuildTag("Remake", "Remake");
    private static readonly Instant _timestamp = Instant.FromUtc(2026, 10, 25, 12, 0);

    [Test]
    public void WithNewDraftOptions_AddingRegularSlots_MovesSpecialSlotsPastThem()
    {
        var regularSlots = BuildRegularSlots(6);
        var firstSpecial = new LeagueSlot(Guid.NewGuid(), false, 6, null, [_newGame]);
        var secondSpecial = new LeagueSlot(Guid.NewGuid(), false, 7, null, [_newGame]);
        var options = BuildLeagueOptions([.. regularSlots, firstSpecial, secondSpecial]);

        var changes = options.WithNewDraftOptions(BuildParameters(RegularSlots(5)), _timestamp);

        var standardSlots = changes!.NewLeagueOptions.LeagueSlots.Where(x => !x.CounterPick).ToList();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(standardSlots.Select(x => x.SlotNumber), Is.EqualTo(Enumerable.Range(0, 13)));
            Assert.That(standardSlots.Take(6).Select(x => x.LeagueSlotID), Is.EqualTo(regularSlots.Select(x => x.LeagueSlotID)),
                "Existing regular slots keep their numbers.");
            Assert.That(standardSlots.Skip(6).Take(5).Select(x => x.IsSpecial), Is.All.False);
            Assert.That(standardSlots.Skip(11).Select(x => x.LeagueSlotID), Is.EqualTo(new[] { firstSpecial.LeagueSlotID, secondSpecial.LeagueSlotID }),
                "Existing special slots keep their IDs and order, moved up by five.");
            Assert.That(standardSlots.Skip(11).Select(x => x.RequiredTags.Single()), Is.All.EqualTo(_newGame));
        }
    }

    [Test]
    public void WithNewDraftOptions_AddingASpecialSlot_PutsItAfterTheExistingSpecialSlots()
    {
        var regularSlots = BuildRegularSlots(2);
        var existingSpecial = new LeagueSlot(Guid.NewGuid(), false, 2, null, [_newGame]);
        var options = BuildLeagueOptions([.. regularSlots, existingSpecial]);
        var parameters = BuildParameters([
            new RequestedNewDraftSlot(false, 0, null, []),
            new RequestedNewDraftSlot(false, 1, null, [_remake]),
        ]);

        var changes = options.WithNewDraftOptions(parameters, _timestamp);

        var standardSlots = changes!.NewLeagueOptions.LeagueSlots.Where(x => !x.CounterPick).ToList();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(standardSlots.Select(x => x.RequiredTags.SingleOrDefault()?.Name), Is.EqualTo(new[] { null, null, null, "NewGame", "Remake" }));
            Assert.That(standardSlots.Select(x => x.SlotNumber), Is.EqualTo(Enumerable.Range(0, 5)));
            Assert.That(standardSlots[3].LeagueSlotID, Is.EqualTo(existingSpecial.LeagueSlotID));
        }
    }

    [Test]
    public void WithNewDraftOptions_AddingCounterPickSlots_AppendsThemAndLeavesStandardSlotsAlone()
    {
        var standardSlots = BuildRegularSlots(3);
        var counterPick = new LeagueSlot(Guid.NewGuid(), true, 0, null, []);
        var options = BuildLeagueOptions([.. standardSlots, counterPick]);

        var changes = options.WithNewDraftOptions(BuildParameters([new RequestedNewDraftSlot(true, 0, null, [])]), _timestamp);

        var newSlots = changes!.NewLeagueOptions.LeagueSlots;
        var counterPickSlots = newSlots.Where(x => x.CounterPick).ToList();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(newSlots.Where(x => !x.CounterPick).Select(x => (x.LeagueSlotID, x.SlotNumber)),
                Is.EqualTo(standardSlots.Select(x => (x.LeagueSlotID, x.SlotNumber))));
            Assert.That(counterPickSlots.Select(x => x.SlotNumber), Is.EqualTo(new[] { 0, 1 }));
            Assert.That(counterPickSlots[0].LeagueSlotID, Is.EqualTo(counterPick.LeagueSlotID));
        }
    }

    [Test]
    public void WithNewDraftOptions_WithNoNewSlots_ReturnsNoChanges()
    {
        var options = BuildLeagueOptions([.. BuildRegularSlots(2), new LeagueSlot(Guid.NewGuid(), false, 2, null, [_newGame])]);

        var changes = options.WithNewDraftOptions(BuildParameters([]), _timestamp);

        Assert.That(changes, Is.Null);
    }

    private static List<LeagueSlot> BuildRegularSlots(int count)
    {
        return Enumerable.Range(0, count).Select(x => new LeagueSlot(Guid.NewGuid(), false, x, null, [])).ToList();
    }

    private static List<RequestedNewDraftSlot> RegularSlots(int count)
    {
        return Enumerable.Range(0, count).Select(x => new RequestedNewDraftSlot(false, x, null, [])).ToList();
    }

    private static CreateLeagueDraftParameters BuildParameters(IEnumerable<RequestedNewDraftSlot> additionalSlots)
    {
        return new CreateLeagueDraftParameters(new LeagueYearKey(Guid.NewGuid(), 2026), "Second Draft", null, 0, 0, false, additionalSlots);
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
