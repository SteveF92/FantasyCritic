using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.MultiDraft;

/// <summary>
/// Verifies that removing a publisher renumbers draft positions in every draft, not just draft 1.
/// </summary>
[TestFixture]
public class MultiDraftRemovePublisherTests : IntegrationTestBase
{
    private static readonly LeagueScenario ThreePlayerMultiDraftScenario = new()
    {
        Name = "ThreePlayerMultiDraft",
        PlayerCount = 3,
        StandardGames = 6,
        GamesToDraft = 3,
        CounterPicks = 0,
        CounterPicksToDraft = 0,
        DraftSystem = "Flexible",
        PickupSystem = "SemiPublicBiddingSecretCounterPicks",
        ScoringSystem = "LinearPositive",
        TradingSystem = "Standard",
        TiebreakSystem = "LowestProjectedPoints",
        ReleaseSystem = "MustBeReleased",
        IneligibleGameSystem = "DroppableAsWillNotRelease",
        UnrestrictedReleaseStatusDroppableGames = 0,
        WillNotReleaseDroppableGames = 0,
        WillReleaseDroppableGames = 0,
        DropOnlyDraftGames = true,
        GrantSuperDrops = false,
        CounterPicksBlockDrops = true,
        AllowMoveIntoIneligible = false,
        MinimumBidAmount = 0,
        EnableBids = false,
    };

    [Test]
    public async Task RemovePlayer_MultiDraftLeague_RenumbersAllDrafts()
    {
        var (mgrEmail, mgrPassword, mgrDisplayName) = NewUser();
        using var manager = new ApiSession(Factory);
        await manager.RegisterAsync(mgrEmail, mgrPassword, mgrDisplayName);

        var playerSessions = new List<ApiSession>();
        for (var i = 0; i < ThreePlayerMultiDraftScenario.PlayerCount - 1; i++)
        {
            var (email, password, displayName) = NewUser();
            var session = new ApiSession(Factory);
            await session.RegisterAsync(email, password, displayName);
            playerSessions.Add(session);
        }

        var year = await LeagueTestHelpers.GetOpenYearAsync(manager);
        var leagueID = await manager.LeagueManager.CreateLeagueAsync(new CreateLeagueRequest
        {
            LeagueName = $"TestLeague-{Guid.NewGuid():N}"[..30],
            PublicLeague = false,
            TestLeague = true,
            CustomRulesLeague = false,
            LeagueYearSettings = ThreePlayerMultiDraftScenario.BuildSettings(year),
            Drafts = new List<DraftSettingsRequest>
            {
                new() { Name = null, ScheduledDate = null, GamesToDraft = 3, CounterPicksToDraft = 0 },
                new() { Name = "Draft 2", ScheduledDate = null, GamesToDraft = 3, CounterPicksToDraft = 0 },
            },
        });

        foreach (var playerSession in playerSessions)
            await LeagueTestHelpers.InviteAndAcceptAsync(manager, playerSession, leagueID);

        var publisherIDsInOrder = new List<Guid>();
        publisherIDsInOrder.Add(await LeagueTestHelpers.CreatePublisherAsync(
            manager, leagueID, year, "Manager Publisher"));

        for (var i = 0; i < playerSessions.Count; i++)
        {
            publisherIDsInOrder.Add(await LeagueTestHelpers.CreatePublisherAsync(
                playerSessions[i], leagueID, year, $"Player{i + 1} Publisher"));
        }

        await LeagueTestHelpers.SetDraftOrderAsync(manager, leagueID, year, publisherIDsInOrder);

        var removedPublisherID = publisherIDsInOrder[1];
        var removedUser = await playerSessions[0].Account.CurrentUserAsync();

        await manager.LeagueManager.RemovePlayerAsync(new PlayerRemoveRequest
        {
            LeagueID = leagueID,
            UserID = removedUser.UserID,
        });

        var afterRemove = await manager.League.GetLeagueYearAsync(leagueID, year, null);
        Assert.That(afterRemove.Publishers, Has.Count.EqualTo(2));
        Assert.That(afterRemove.Publishers.Any(p => p.PublisherID == removedPublisherID), Is.False);

        var draft1 = afterRemove.Drafts.Single(d => d.DraftNumber == 1);
        var expectedOrder = new[] { publisherIDsInOrder[0], publisherIDsInOrder[2] };
        AssertDraftOrder(draft1, expectedOrder, "draft 1");

        var draft2 = afterRemove.Drafts.Single(d => d.DraftNumber == 2);
        AssertDraftOrder(draft2, expectedOrder, "draft 2");
    }

    private static void AssertDraftOrder(LeagueDraftViewModel draft, IReadOnlyList<Guid> expectedPublisherOrder, string draftLabel)
    {
        var actualOrder = draft.PublisherDraftInfo
            .OrderBy(i => i.DraftPosition)
            .Select(i => i.PublisherID)
            .ToList();

        Assert.That(actualOrder, Is.EqualTo(expectedPublisherOrder),
            $"{draftLabel} should renumber remaining publishers without gaps.");

        var positions = draft.PublisherDraftInfo.Select(i => i.DraftPosition).OrderBy(x => x).ToList();
        Assert.That(positions, Is.EqualTo(Enumerable.Range(1, expectedPublisherOrder.Count)),
            $"{draftLabel} positions should be 1..N.");
    }
}
