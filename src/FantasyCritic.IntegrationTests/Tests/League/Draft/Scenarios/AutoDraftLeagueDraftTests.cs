using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Draft.Scenarios;

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

    protected override async Task SimulateDraftAsync()
    {
        var autoDraftPublisher = League.Publishers.Last();

        await autoDraftPublisher.Session.League.SetAutoDraftAsync(new SetAutoDraftRequest
        {
            PublisherID = autoDraftPublisher.PublisherID,
            Mode = "All",
            OnlyDraftFromWatchlist = false,
        });

        var nonAutoDraftPublishers = League.Publishers
            .Where(p => p.PublisherID != autoDraftPublisher.PublisherID)
            .ToDictionary(p => p.PublisherID, p => p.Session);

        await League.DraftToCompletionAsync(nonAutoDraftPublishers);
    }

    [Test]
    public void AutoDraftPlayer_HasAutoDraftModeSetToAll()
    {
        var autoDraftPublisher = LeagueYearSnapshot.Publishers
            .Single(p => p.PublisherID == League.Publishers.Last().PublisherID);
        Assert.That(autoDraftPublisher.AutoDraftMode, Is.EqualTo("All"),
            "The auto-draft publisher's AutoDraftMode should be persisted as 'All' in the post-draft snapshot.");
    }
}
