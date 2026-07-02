using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.MultiDraft;

/// <summary>
/// Verifies that adding a second draft with slot expansion re-slots existing publisher games,
/// especially games that were sitting in special slots before the expansion.
/// </summary>
[TestFixture]
public class SlotReassignmentTests : IntegrationTestBase
{
    private static readonly LeagueScenario SpecialSlotExpansion = new()
    {
        Name = "SpecialSlotExpansion",
        PlayerCount = 2,
        StandardGames = 8,
        GamesToDraft = 8,
        CounterPicks = 0,
        CounterPicksToDraft = 0,
        UnrestrictedReleaseStatusDroppableGames = 0,
        WillNotReleaseDroppableGames = 0,
        WillReleaseDroppableGames = 0,
        DropOnlyDraftGames = true,
        GrantSuperDrops = false,
        CounterPicksBlockDrops = true,
        AllowMoveIntoIneligible = false,
        MinimumBidAmount = 0,
        EnableBids = false,
        DraftSystem = "Flexible",
        PickupSystem = "SemiPublicBiddingSecretCounterPicks",
        ScoringSystem = "LinearPositive",
        TradingSystem = "NoTrades",
        TiebreakSystem = "LowestProjectedPoints",
        ReleaseSystem = "OnlyNeedsScore",
        IneligibleGameSystem = "DroppableAsWillNotRelease",
        HasSpecialSlots = true,
        SpecialGameSlots =
        [
            new SpecialGameSlotViewModel { SpecialSlotPosition = 0, RequiredTags = new List<string> { "NewGame" } },
            new SpecialGameSlotViewModel { SpecialSlotPosition = 1, RequiredTags = new List<string> { "NewGame" } },
        ],
    };

    [Test]
    public async Task CreateLeagueDraft_WithSlotExpansion_ReassignsSpecialSlotGames()
    {
        await using var league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(
            Factory, SpecialSlotExpansion, NewUser);

        await league.DraftToCompletionAsync();

        var before = await league.GetLeagueYearAsync();
        var publisher = before.Publishers.First();
        var specialSlotGamesBefore = GetFilledSpecialSlotGames(publisher);

        Assert.That(specialSlotGamesBefore, Has.Count.EqualTo(2),
            "Each publisher should have both special slots filled after a full draft.");
        Assert.That(specialSlotGamesBefore.Select(slot => slot.SlotNumber), Is.EquivalentTo(new[] { 6, 7 }),
            "Special slots should occupy the last two standard-game indices before expansion.");

        var trackedGames = specialSlotGamesBefore
            .Select(slot => new
            {
                slot.PublisherGame!.PublisherGameID,
                OriginalSlotNumber = slot.SlotNumber,
            })
            .ToList();

        await league.Manager.LeagueManager.CreateLeagueDraftAsync(new CreateLeagueDraftRequest
        {
            LeagueID = league.LeagueID,
            Year = league.Year,
            Name = "Expansion Draft",
            ScheduledDate = null,
            GamesToDraft = 2,
            CounterPicksToDraft = 0,
            AdditionalStandardGames = 5,
            AdditionalCounterPicks = 0,
            NewSpecialGameSlots = new List<SpecialGameSlotViewModel>(),
        });

        var after = await league.GetLeagueYearAsync();
        var publisherAfter = after.Publishers.Single(p => p.PublisherID == publisher.PublisherID);

        Assert.That(after.Settings.StandardGames, Is.EqualTo(SpecialSlotExpansion.StandardGames + 5));

        foreach (var trackedGame in trackedGames)
        {
            var slotAfter = publisherAfter.GameSlots.Single(s =>
                s.PublisherGame?.PublisherGameID == trackedGame.PublisherGameID);

            Assert.Multiple(() =>
            {
                Assert.That(slotAfter.SpecialSlot, Is.Not.Null,
                    "Game should still be treated as occupying a special slot after expansion.");
                Assert.That(slotAfter.SlotNumber, Is.EqualTo(trackedGame.OriginalSlotNumber + 5),
                    "Special-slot games should shift forward by the number of added standard slots.");
            });
        }

        var normalSlotNumbersAfter = publisherAfter.GameSlots
            .Where(slot => slot.SpecialSlot is null && slot.PublisherGame is not null)
            .Select(slot => slot.SlotNumber)
            .ToList();

        Assert.That(normalSlotNumbersAfter, Is.EquivalentTo(new[] { 0, 1, 2, 3, 4, 5 }),
            "Normal-slot games should remain compacted at the front of the roster.");
    }

    private static IReadOnlyList<PublisherSlotViewModel> GetFilledSpecialSlotGames(PublisherViewModel publisher) =>
        publisher.GameSlots
            .Where(slot => slot.SpecialSlot is not null && slot.PublisherGame is not null)
            .OrderBy(slot => slot.SlotNumber)
            .ToList();
}
