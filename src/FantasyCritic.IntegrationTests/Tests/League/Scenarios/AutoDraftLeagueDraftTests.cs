using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Scenarios;

/// <summary>
/// Exercises a full draft where the last-position publisher has AutoDraftMode = "All".
/// The server auto-picks on their behalf every turn; the simulator drives only the other
/// three publishers. Verifies the draft completes correctly and the auto-draft publisher
/// ends up with the expected game counts (inherited tests) and that the mode is persisted.
/// </summary>
[TestFixture]
public class AutoDraftLeagueDraftTests : LeagueDraftTestBase
{
    protected override LeagueScenario Scenario => LeagueScenarios.Standard;

    protected override async Task SimulateDraftAsync(
        IReadOnlyDictionary<Guid, ApiSession> publisherSessionMap,
        Guid leagueID,
        int year)
    {
        // The last publisher in draft order is designated as the auto-draft player.
        var autoDraftPubID = PublisherIDs.Last();
        var autoDraftSession = publisherSessionMap[autoDraftPubID];

        // Enabling "All" causes the server to auto-pick for this publisher every time it
        // is their turn. After any other player's pick, DraftGame → AutoDraftForLeague
        // runs on the server side, so this publisher will never appear as NextToDraft
        // when the simulator loop polls the league year. If they do appear (i.e. auto-draft
        // failed), DraftSimulator throws a clear error — which is the intended failure signal.
        await autoDraftSession.League.SetAutoDraftAsync(new SetAutoDraftRequest
        {
            PublisherID = autoDraftPubID,
            Mode = "All",
            OnlyDraftFromWatchlist = false,
        });

        var nonAutoDraftPlayers = publisherSessionMap
            .Where(kvp => kvp.Key != autoDraftPubID)
            .Select(kvp => new MockedLivePlayer(kvp.Value, kvp.Key, leagueID));
        var simulator = new DraftSimulator(ManagerSession, nonAutoDraftPlayers);
        await simulator.RunAsync(leagueID, year);
    }

    [Test]
    public void AutoDraftPlayer_HasAutoDraftModeSetToAll()
    {
        var autoDraftPublisher = LeagueYearSnapshot.Publishers
            .Single(p => p.PublisherID == PublisherIDs.Last());
        Assert.That(autoDraftPublisher.AutoDraftMode, Is.EqualTo("All"),
            "The auto-draft publisher's AutoDraftMode should be persisted as 'All' in the post-draft snapshot.");
    }
}
