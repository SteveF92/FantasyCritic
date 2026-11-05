using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;

namespace FantasyCritic.IntegrationTests.Helpers;

/// <summary>
/// Sets up conferences through the API. Creating one needs a manager session with the PlusUser role.
/// </summary>
public static class ConferenceFixtureBuilder
{
    /// <summary>
    /// Creates a conference whose primary league uses the scenario's settings, and returns its ID.
    /// </summary>
    public static Task<Guid> CreateAsync(ApiSession manager, LeagueScenario scenario, int year, string conferenceName, string primaryLeagueName) =>
        manager.Conference.CreateConferenceAsync(new CreateConferenceRequest
        {
            ConferenceName = conferenceName,
            PrimaryLeagueName = primaryLeagueName,
            CustomRulesConference = false,
            LeagueYearSettings = scenario.BuildSettings(year),
            Drafts = scenario.BuildDraftSettings().ToList(),
        });

    /// <summary>
    /// The manager creates an invite link and the player joins the conference with it.
    /// </summary>
    public static async Task InviteAndJoinAsync(ApiSession manager, ApiSession player, Guid conferenceID)
    {
        await manager.Conference.CreateInviteLinkAsync(new CreateConferenceInviteLinkRequest { ConferenceID = conferenceID });
        var links = await manager.Conference.InviteLinksAsync(conferenceID);
        await player.Conference.JoinWithInviteLinkAsync(new JoinConferenceWithInviteLinkRequest
        {
            ConferenceID = conferenceID,
            InviteCode = links.First().InviteCode,
        });
    }

    public static Task AddLeagueAsync(ApiSession manager, Guid conferenceID, int year, string leagueName, Guid leagueManagerUserID) =>
        manager.Conference.AddLeagueToConferenceAsync(new AddLeagueToConferenceRequest
        {
            ConferenceID = conferenceID,
            Year = year,
            LeagueName = leagueName,
            LeagueManager = leagueManagerUserID,
        });
}
