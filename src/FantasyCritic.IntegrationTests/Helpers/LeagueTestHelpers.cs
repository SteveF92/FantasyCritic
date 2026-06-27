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
    /// Fetches the current pending draft to obtain its DraftID automatically.
    /// </summary>
    public static async Task SetDraftOrderAsync(
        ApiSession managerSession,
        Guid leagueID,
        int year,
        IReadOnlyList<Guid> publisherIDsInOrder)
    {
        var snapshot = await managerSession.League.GetLeagueYearAsync(leagueID, year, null);
        var pendingDraft = snapshot.PendingDraft()
            ?? throw new InvalidOperationException("No pending draft found when setting draft order.");
        await managerSession.LeagueManager.SetDraftOrderAsync(new DraftOrderRequest
        {
            LeagueID = leagueID,
            Year = year,
            DraftID = pendingDraft.DraftID,
            DraftOrderType = "Manual",
            ManualPublisherDraftPositions = publisherIDsInOrder.ToList(),
        });
    }

}
