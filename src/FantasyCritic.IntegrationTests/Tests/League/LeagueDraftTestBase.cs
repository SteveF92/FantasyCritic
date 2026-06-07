using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League;

/// <summary>
/// Abstract base for draft scenario tests. Subclasses provide a <see cref="LeagueScenario"/>
/// and optionally override <see cref="SimulateDraftAsync"/> to change how the draft is run
/// (e.g. player-side vs manager-side).
///
/// <see cref="SetUpLeagueAndDraft"/> builds the complete state; the shared <c>[Test]</c> methods
/// assert against the post-draft <see cref="LeagueYearSnapshot"/>.
/// </summary>
public abstract class LeagueDraftTestBase : IntegrationTestBase
{
    protected abstract LeagueScenario Scenario { get; }

    // State filled in by OneTimeSetUp
    protected int Year;
    protected Guid LeagueID;
    protected IReadOnlyList<Guid> PublisherIDs = [];
    protected LeagueYearViewModel LeagueYearSnapshot = null!;
    protected ApiSession ManagerSession = null!;
    protected IReadOnlyList<ApiSession> PlayerSessions = [];

    [OneTimeSetUp]
    public async Task SetUpLeagueAndDraft()
    {
        // 1. Create manager session
        var (mgrEmail, mgrPassword, mgrDisplayName) = NewUser();
        ManagerSession = new ApiSession(Factory);
        await ManagerSession.RegisterAsync(mgrEmail, mgrPassword, mgrDisplayName);

        // 2. Create player sessions (Scenario.PlayerCount - 1 non-manager players)
        var playerSessions = new List<ApiSession>();
        for (int i = 0; i < Scenario.PlayerCount - 1; i++)
        {
            var (email, password, displayName) = NewUser();
            var session = new ApiSession(Factory);
            await session.RegisterAsync(email, password, displayName);
            playerSessions.Add(session);
        }
        PlayerSessions = playerSessions.AsReadOnly();

        // 3. Get open year and create league
        Year = await LeagueTestHelpers.GetOpenYearAsync(ManagerSession);
        LeagueID = await LeagueTestHelpers.CreateLeagueAsync(ManagerSession, Scenario, Year);

        // 4. Invite all players and have them accept
        foreach (var playerSession in PlayerSessions)
        {
            await LeagueTestHelpers.InviteAndAcceptAsync(ManagerSession, playerSession, LeagueID);
        }

        // 5. Create publishers; build publisher-to-session map
        var publisherSessionMap = new Dictionary<Guid, ApiSession>();
        var publisherIDs = new List<Guid>();

        var managerPubID = await LeagueTestHelpers.CreatePublisherAsync(
            ManagerSession, LeagueID, Year, "Manager Publisher");
        publisherSessionMap[managerPubID] = ManagerSession;
        publisherIDs.Add(managerPubID);

        for (int i = 0; i < PlayerSessions.Count; i++)
        {
            var pubID = await LeagueTestHelpers.CreatePublisherAsync(
                PlayerSessions[i], LeagueID, Year, $"Player{i + 1} Publisher");
            publisherSessionMap[pubID] = PlayerSessions[i];
            publisherIDs.Add(pubID);
        }

        PublisherIDs = publisherIDs.AsReadOnly();

        // 6. Set draft order
        await LeagueTestHelpers.SetDraftOrderAsync(ManagerSession, LeagueID, Year, PublisherIDs);

        // 7. Start the draft
        await ManagerSession.LeagueManager.StartDraftAsync(new StartDraftRequest
        {
            LeagueID = LeagueID,
            Year = Year,
        });

        // 8. Run the draft simulation (overridable by subclasses)
        await SimulateDraftAsync(publisherSessionMap, LeagueID, Year);

        // 9. Capture the post-draft snapshot
        LeagueYearSnapshot = await ManagerSession.League.GetLeagueYearAsync(LeagueID, Year, null);
    }

    [OneTimeTearDown]
    public void TearDownSessions()
    {
        ManagerSession?.Dispose();
        foreach (var s in PlayerSessions)
            s.Dispose();
    }

    /// <summary>
    /// Runs the draft to completion using <see cref="MockedLivePlayer"/> / <see cref="DraftSimulator"/>.
    /// Override to use the manager-side <c>ManagerDraftGame</c> endpoint instead.
    /// </summary>
    protected virtual async Task SimulateDraftAsync(
        IReadOnlyDictionary<Guid, ApiSession> publisherSessionMap,
        Guid leagueID,
        int year)
    {
        var players = publisherSessionMap.Select(kvp =>
            new MockedLivePlayer(kvp.Value, kvp.Key, leagueID));
        var simulator = new DraftSimulator(ManagerSession, players);
        await simulator.RunAsync(leagueID, year);
    }

    // ---------------------------------------------------------------------------
    // Shared tests — run for every concrete subclass
    // ---------------------------------------------------------------------------

    [Test]
    public void Draft_Completed_PlayStatusShowsDraftFinished()
    {
        Assert.That(LeagueYearSnapshot.PlayStatus.DraftFinished, Is.True,
            "Draft must be finished after simulation.");
    }

    [Test]
    public void Draft_Completed_DraftIsNoLongerActive()
    {
        Assert.That(LeagueYearSnapshot.PlayStatus.DraftIsActive, Is.False);
    }

    [Test]
    public void Draft_Completed_NoPublisherIsNextToDraft()
    {
        Assert.That(
            LeagueYearSnapshot.Publishers.Any(p => p.NextToDraft),
            Is.False,
            "No publisher should be marked NextToDraft once the draft is finished.");
    }

    [Test]
    public void Draft_Completed_AllPublishersHaveCorrectStandardGameCount()
    {
        foreach (var publisher in LeagueYearSnapshot.Publishers)
        {
            var standardGames = publisher.Games.Count(g => !g.CounterPick);
            Assert.That(standardGames, Is.EqualTo(Scenario.GamesToDraft),
                $"Publisher '{publisher.PublisherName}' should have {Scenario.GamesToDraft} standard games.");
        }
    }

    [Test]
    public void Draft_Completed_AllPublishersHaveCorrectCounterPickCount()
    {
        foreach (var publisher in LeagueYearSnapshot.Publishers)
        {
            var counterPicks = publisher.Games.Count(g => g.CounterPick);
            Assert.That(counterPicks, Is.EqualTo(Scenario.CounterPicksToDraft),
                $"Publisher '{publisher.PublisherName}' should have {Scenario.CounterPicksToDraft} counter-pick(s).");
        }
    }

    [Test]
    public void Draft_Completed_TotalGamesAcrossAllPublishers_EqualsExpectedCount()
    {
        int expectedTotal =
            Scenario.PlayerCount * (Scenario.GamesToDraft + Scenario.CounterPicksToDraft);
        int actualTotal = LeagueYearSnapshot.Publishers.Sum(p => p.Games.Count);

        Assert.That(actualTotal, Is.EqualTo(expectedTotal),
            $"Total games across all publishers should be {expectedTotal}.");
    }
}
