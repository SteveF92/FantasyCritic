using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.MultiDraft;

/// <summary>
/// Verifies that <see cref="LeagueYearViewModel.Publishers"/> and related fields use
/// <c>DraftForPublisherDisplayOrder</c> — the highest draft with its order set, or the
/// most recent completed draft when no pending draft has order yet.
/// </summary>
[TestFixture]
public class MultiDraftPublisherDisplayOrderTests : IntegrationTestBase
{
    private static void AssertPublisherDisplayOrder(LeagueYearViewModel snapshot)
    {
        var expectedPublisherIDs = snapshot.PublisherIDsInDisplayOrder();
        var expectedPositions = expectedPublisherIDs
            .Select((id, index) => (id, position: index + 1))
            .ToDictionary(x => x.id, x => x.position);

        Assert.That(snapshot.Publishers.Select(p => p.PublisherID).ToList(), Is.EqualTo(expectedPublisherIDs),
            "Publishers should be ordered by the display-order draft.");

        foreach (var publisher in snapshot.Publishers)
        {
            Assert.That(publisher.DraftPosition, Is.EqualTo(expectedPositions[publisher.PublisherID]),
                $"Publisher {publisher.PublisherName} DraftPosition should match the display-order draft.");
        }

        var playersWithPublishers = snapshot.Players.Where(p => p.Publisher is not null).ToList();
        if (playersWithPublishers.Count == expectedPublisherIDs.Count)
        {
            Assert.That(playersWithPublishers.Select(p => p.Publisher!.PublisherID).ToList(), Is.EqualTo(expectedPublisherIDs),
                "Players list should be ordered by the display-order draft when every player has a publisher.");
        }
    }

    [Test]
    public async Task GetLeagueYear_BeforeAnyDraftStarts_UsesFirstDraftOrder()
    {
        await using var league = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(
            Factory, MultiDraftTestScenario.TwoPlayer, NewUser);

        var snapshot = await league.GetLeagueYearAsync();

        Assert.That(snapshot.DisplayOrderDraft().DraftNumber, Is.EqualTo(1));
        AssertPublisherDisplayOrder(snapshot);
    }

    [Test]
    public async Task GetLeagueYear_AfterDraft1_WhenDraft2HasNoOrder_UsesDraft1Order()
    {
        await using var league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(
            Factory, MultiDraftTestScenario.TwoPlayer, NewUser);
        await league.DraftToCompletionAsync();

        await MultiDraftTestScenario.CreateSecondDraftAsync(
            league, gamesToDraft: 2, counterPicksToDraft: 0, additionalStandardGames: 2, additionalCounterPicks: 0);

        var snapshot = await league.GetLeagueYearAsync();
        var draft2 = snapshot.Drafts.Single(d => d.DraftNumber == 2);

        Assert.That(draft2.DraftOrderSet, Is.False, "Draft 2 should not have order set yet.");
        Assert.That(snapshot.DisplayOrderDraft().DraftNumber, Is.EqualTo(1),
            "With no pending draft order, display should fall back to the last completed draft.");

        var manager = league.Publishers.Single(p => p.DraftPosition == 1);
        var player = league.Publishers.Single(p => p.DraftPosition == 2);
        Assert.That(snapshot.Publishers.Select(p => p.PublisherID).ToList(),
            Is.EqualTo(new[] { manager.PublisherID, player.PublisherID }));

        AssertPublisherDisplayOrder(snapshot);
    }

    [Test]
    public async Task GetLeagueYear_WhenPendingDraftHasOrderSet_UsesThatDraftOrder()
    {
        await using var league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(
            Factory, MultiDraftTestScenario.TwoPlayer, NewUser);
        await league.DraftToCompletionAsync();

        await MultiDraftTestScenario.CreateSecondDraftAsync(
            league, gamesToDraft: 2, counterPicksToDraft: 0, additionalStandardGames: 2, additionalCounterPicks: 0);

        var manager = league.Publishers.Single(p => p.DraftPosition == 1);
        var player = league.Publishers.Single(p => p.DraftPosition == 2);

        // Draft 1 order was manager-first; reverse it for draft 2.
        await LeagueTestHelpers.SetDraftOrderAsync(
            league.Manager, league.LeagueID, league.Year, new[] { player.PublisherID, manager.PublisherID });

        var snapshot = await league.GetLeagueYearAsync();

        Assert.That(snapshot.DisplayOrderDraft().DraftNumber, Is.EqualTo(2));
        Assert.That(snapshot.Publishers.Select(p => p.PublisherID).ToList(),
            Is.EqualTo(new[] { player.PublisherID, manager.PublisherID }),
            "Publishers should follow draft 2 order, not draft 1.");
        Assert.That(snapshot.Publishers.Select(p => p.DraftPosition).ToList(), Is.EqualTo(new[] { 1, 2 }));

        AssertPublisherDisplayOrder(snapshot);
    }
}
