using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;

namespace FantasyCritic.IntegrationTests.Helpers;

/// <summary>
/// Reusable async helpers for setting up leagues in integration tests.
/// All methods build state through the HTTP API — no direct DB access.
/// </summary>
internal static class LeagueTestHelpers
{
    /// <summary>
    /// Returns the first year currently open for league creation.
    /// Throws if no open years are available (seed DB may be misconfigured).
    /// </summary>
    public static async Task<int> GetOpenYearAsync(ApiSession session)
    {
        var options = await session.League.LeagueOptionsAsync();
        if (options.OpenYears.Count == 0)
            throw new InvalidOperationException(
                "LeagueOptions returned no open years. Is the seed DB running?");
        return options.OpenYears.First();
    }

    /// <summary>
    /// Creates a league under <paramref name="managerSession"/> using the given
    /// <paramref name="scenario"/> settings and returns the new league's GUID.
    /// </summary>
    public static async Task<Guid> CreateLeagueAsync(
        ApiSession managerSession,
        LeagueScenario scenario,
        int year)
    {
        var leagueID = await managerSession.LeagueManager.CreateLeagueAsync(
            new CreateLeagueRequest
            {
                LeagueName = $"TestLeague-{Guid.NewGuid():N}"[..30],
                PublicLeague = false,
                TestLeague = true,
                CustomRulesLeague = false,
                LeagueYearSettings = scenario.BuildSettings(year),
            });

        return leagueID;
    }

    /// <summary>
    /// Manager invites <paramref name="playerSession"/>'s user by email, then
    /// the player accepts the invite for the given league.
    /// </summary>
    public static async Task InviteAndAcceptAsync(
        ApiSession managerSession,
        ApiSession playerSession,
        Guid leagueID)
    {
        var playerUser = await playerSession.Account.CurrentUserAsync();

        await managerSession.LeagueManager.InvitePlayerAsync(new CreateInviteRequest
        {
            LeagueID = leagueID,
            InviteEmail = playerUser.EmailAddress,
        });

        var pendingInvites = await playerSession.League.MyInvitesAsync();
        var invite = pendingInvites.SingleOrDefault(i => i.LeagueID == leagueID)
            ?? throw new InvalidOperationException(
                $"Player {playerUser.DisplayName} does not see a pending invite for league {leagueID}.");

        await playerSession.League.AcceptInviteAsync(new AcceptInviteRequest
        {
            LeagueID = leagueID,
        });
    }

    /// <summary>
    /// Creates a publisher for <paramref name="playerSession"/> in the given league/year.
    /// Returns the new publisher's GUID.
    /// </summary>
    public static async Task<Guid> CreatePublisherAsync(
        ApiSession playerSession,
        Guid leagueID,
        int year,
        string publisherName)
    {
        var currentUser = await playerSession.Account.CurrentUserAsync();

        await playerSession.League.CreatePublisherAsync(new CreatePublisherRequest
        {
            LeagueID = leagueID,
            Year = year,
            PublisherName = publisherName,
        });

        var leagueYear = await playerSession.League.GetLeagueYearAsync(leagueID, year, null);
        var publisher = leagueYear.Publishers.SingleOrDefault(p => p.UserID == currentUser.UserID)
            ?? throw new InvalidOperationException(
                $"Publisher not found for user {currentUser.UserID} after CreatePublisher.");

        return publisher.PublisherID;
    }

    /// <summary>
    /// Sets the draft order to the supplied publisher order using the "Manual" type.
    /// </summary>
    public static async Task SetDraftOrderAsync(
        ApiSession managerSession,
        Guid leagueID,
        int year,
        IReadOnlyList<Guid> publisherIDsInOrder)
    {
        await managerSession.LeagueManager.SetDraftOrderAsync(new DraftOrderRequest
        {
            LeagueID = leagueID,
            Year = year,
            DraftOrderType = "Manual",
            ManualPublisherDraftPositions = publisherIDsInOrder.ToList(),
        });
    }

    /// <summary>
    /// Creates a league, invites and accepts all non-manager players, creates publishers,
    /// sets draft order (manager = position 1, players in creation order), and starts the draft.
    ///
    /// Returns the state needed by error-case fixtures that stop the draft at a partial point:
    /// <list type="bullet">
    ///   <item><description><c>leagueID</c> and <c>year</c></description></item>
    ///   <item><description><c>publisherIDsInDraftOrder</c> — [0] = manager, [1..n] = players</description></item>
    ///   <item><description><c>publisherSessionMap</c> — maps publisher GUID to the session that owns it</description></item>
    /// </list>
    /// Unlike <see cref="LeagueDraftTestBase"/>, this helper does NOT complete the draft.
    /// </summary>
    public static async Task<(Guid leagueID, int year,
        IReadOnlyList<Guid> publisherIDsInDraftOrder,
        IReadOnlyDictionary<Guid, ApiSession> publisherSessionMap)>
        SetUpLeagueAndStartDraftAsync(
            ApiSession managerSession,
            IReadOnlyList<ApiSession> playerSessions,
            LeagueScenario scenario)
    {
        var year = await GetOpenYearAsync(managerSession);
        var leagueID = await CreateLeagueAsync(managerSession, scenario, year);

        foreach (var playerSession in playerSessions)
        {
            await InviteAndAcceptAsync(managerSession, playerSession, leagueID);
        }

        var publisherSessionMap = new Dictionary<Guid, ApiSession>();
        var publisherIDsInOrder = new List<Guid>();

        var managerPubID = await CreatePublisherAsync(
            managerSession, leagueID, year, $"Mgr-{Guid.NewGuid():N}"[..20]);
        publisherSessionMap[managerPubID] = managerSession;
        publisherIDsInOrder.Add(managerPubID);

        for (var i = 0; i < playerSessions.Count; i++)
        {
            var pubID = await CreatePublisherAsync(
                playerSessions[i], leagueID, year, $"Plr{i + 1}-{Guid.NewGuid():N}"[..20]);
            publisherSessionMap[pubID] = playerSessions[i];
            publisherIDsInOrder.Add(pubID);
        }

        await SetDraftOrderAsync(managerSession, leagueID, year, publisherIDsInOrder);

        await managerSession.LeagueManager.StartDraftAsync(new StartDraftRequest
        {
            LeagueID = leagueID,
            Year = year,
        });

        return (leagueID, year,
            publisherIDsInOrder.AsReadOnly(),
            new Dictionary<Guid, ApiSession>(publisherSessionMap));
    }
}
