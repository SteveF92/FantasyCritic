using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;

namespace FantasyCritic.IntegrationTests.Helpers;

/// <summary>
/// Immutable league state for integration test scenarios. Owns all <see cref="ApiSession"/>
/// instances it created; call <see cref="DisposeAsync"/> when the fixture is done.
/// </summary>
public sealed class LeagueFixture : IAsyncDisposable
{
    private readonly List<ApiSession> _ownedSessions;

    internal LeagueFixture(
        LeagueScenario scenario,
        Guid leagueID,
        int year,
        ApiSession manager,
        IReadOnlyList<TestPublisher> publishers,
        List<ApiSession> ownedSessions)
    {
        Scenario = scenario;
        LeagueID = leagueID;
        Year = year;
        Manager = manager;
        Publishers = publishers;
        PublisherSessionMap = publishers.ToDictionary(p => p.PublisherID, p => p.Session);
        _ownedSessions = ownedSessions;
    }

    public LeagueScenario Scenario { get; }
    public Guid LeagueID { get; }
    public int Year { get; }
    public ApiSession Manager { get; }
    public IReadOnlyList<TestPublisher> Publishers { get; }
    public IReadOnlyDictionary<Guid, ApiSession> PublisherSessionMap { get; }

    public Task<LeagueYearViewModel> GetLeagueYearAsync() =>
        Manager.League.GetLeagueYearAsync(LeagueID, Year, null);

    public Task DraftToCompletionAsync(IReadOnlyDictionary<Guid, ApiSession>? activePublishers = null)
    {
        var simulator = new DraftSimulator(Manager, ResolvePlayers(activePublishers));
        return simulator.RunAsync(LeagueID, Year);
    }

    public Task DraftUntilCounterPickPhaseAsync(IReadOnlyDictionary<Guid, ApiSession>? activePublishers = null)
    {
        var simulator = new DraftSimulator(Manager, ResolvePlayers(activePublishers));
        return simulator.RunUntilCounterPickPhaseAsync(LeagueID, Year);
    }

    public Task DraftStandardPicksAsync(int count, IReadOnlyDictionary<Guid, ApiSession>? activePublishers = null)
    {
        var simulator = new DraftSimulator(Manager, ResolvePlayers(activePublishers));
        return simulator.RunStandardPickCountAsync(LeagueID, Year, count);
    }

    public ValueTask DisposeAsync()
    {
        foreach (var session in _ownedSessions)
            session.Dispose();
        return ValueTask.CompletedTask;
    }

    private IEnumerable<MockedLivePlayer> ResolvePlayers(IReadOnlyDictionary<Guid, ApiSession>? activePublishers) =>
        activePublishers != null
            ? activePublishers.Select(kvp => new MockedLivePlayer(kvp.Value, kvp.Key, LeagueID))
            : Publishers.Select(p => new MockedLivePlayer(p.Session, p.PublisherID, LeagueID));
}

/// <summary>
/// Builds <see cref="LeagueFixture"/> instances through the HTTP API.
/// </summary>
public static class LeagueFixtureBuilder
{
    /// <summary>
    /// Registers manager + (Scenario.PlayerCount - 1) players, creates the league,
    /// invites, creates publishers, and sets manual draft order. Does not start the draft.
    /// </summary>
    public static Task<LeagueFixture> CreateLeagueWithMembersAsync(
        FantasyCriticWebApplicationFactory factory,
        LeagueScenario scenario,
        Func<(string email, string password, string displayName)> newUser) =>
        BuildAsync(factory, scenario, newUser, startDraft: false);

    /// <summary>
    /// Same as <see cref="CreateLeagueWithMembersAsync"/> plus <c>StartDraft</c>.
    /// </summary>
    public static Task<LeagueFixture> CreateAndStartDraftAsync(
        FantasyCriticWebApplicationFactory factory,
        LeagueScenario scenario,
        Func<(string email, string password, string displayName)> newUser) =>
        BuildAsync(factory, scenario, newUser, startDraft: true);

    private static async Task<LeagueFixture> BuildAsync(
        FantasyCriticWebApplicationFactory factory,
        LeagueScenario scenario,
        Func<(string email, string password, string displayName)> newUser,
        bool startDraft)
    {
        var ownedSessions = new List<ApiSession>();

        var (mgrEmail, mgrPassword, mgrDisplayName) = newUser();
        var manager = new ApiSession(factory);
        ownedSessions.Add(manager);
        await manager.RegisterAsync(mgrEmail, mgrPassword, mgrDisplayName);

        var playerSessions = new List<ApiSession>();
        for (var i = 0; i < scenario.PlayerCount - 1; i++)
        {
            var (email, password, displayName) = newUser();
            var session = new ApiSession(factory);
            ownedSessions.Add(session);
            await session.RegisterAsync(email, password, displayName);
            playerSessions.Add(session);
        }

        var year = await LeagueTestHelpers.GetOpenYearAsync(manager);
        var leagueID = await LeagueTestHelpers.CreateLeagueAsync(manager, scenario, year);

        foreach (var playerSession in playerSessions)
            await LeagueTestHelpers.InviteAndAcceptAsync(manager, playerSession, leagueID);

        var publishers = new List<TestPublisher>();
        var publisherIDsInOrder = new List<Guid>();

        var managerPubID = await LeagueTestHelpers.CreatePublisherAsync(
            manager, leagueID, year, "Manager Publisher");
        publisherIDsInOrder.Add(managerPubID);
        publishers.Add(new TestPublisher(1, manager, managerPubID, "Manager Publisher"));

        for (var i = 0; i < playerSessions.Count; i++)
        {
            var name = $"Player{i + 1} Publisher";
            var pubID = await LeagueTestHelpers.CreatePublisherAsync(
                playerSessions[i], leagueID, year, name);
            publisherIDsInOrder.Add(pubID);
            publishers.Add(new TestPublisher(i + 2, playerSessions[i], pubID, name));
        }

        await LeagueTestHelpers.SetDraftOrderAsync(manager, leagueID, year, publisherIDsInOrder);

        if (startDraft)
        {
            await manager.LeagueManager.StartDraftAsync(new StartDraftRequest
            {
                LeagueID = leagueID,
                Year = year,
            });
        }

        return new LeagueFixture(scenario, leagueID, year, manager, publishers, ownedSessions);
    }
}
