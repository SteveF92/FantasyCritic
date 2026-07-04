using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Setup;

/// <summary>
/// Integration tests verifying multi-draft conference clone fixes:
///   1. AddLeagueToConference clones ALL draft rows from the primary league (not just Draft 1).
///   2. AssignLeaguePlayers rebuilds draft positions for ALL drafts (not just Draft 1).
/// </summary>
[TestFixture]
public class ConferenceCloneTests : IntegrationTestBase
{
    // -------------------------------------------------------------------------
    // Minimal POCOs for deserializing conference GET responses.
    // The generated API client does not have typed overloads for GetConference/
    // GetConferenceYear, so we deserialize the JSON manually.
    // -------------------------------------------------------------------------

    private sealed class SimpleConferenceVm
    {
        public Guid ConferenceID { get; set; }
        public List<SimpleConferenceLeagueVm> LeaguesInConference { get; set; } = new();
        public SimpleConferenceLeagueVm PrimaryLeague { get; set; } = new();
    }

    private sealed class SimpleConferenceLeagueVm
    {
        public Guid LeagueID { get; set; }
        public string LeagueName { get; set; } = string.Empty;
        public SimplePlayerVm LeagueManager { get; set; } = new();
    }

    private sealed class SimplePlayerVm
    {
        public Guid UserID { get; set; }
    }

    // -------------------------------------------------------------------------
    // Helper: register a user then grant them PlusUser role (required for
    // conference creation) and re-login to refresh their auth cookie.
    // -------------------------------------------------------------------------
    private async Task RegisterAsPlusUserAsync(
        ApiSession session,
        string email,
        string password,
        string displayName)
    {
        await session.RegisterAsync(email, password, displayName);
        var me = await session.Account.CurrentUserAsync();

        using var adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(adminSession);
        await adminSession.Admin.GrantRoleAsync(new UserRoleRequest
        {
            UserID = me.UserID,
            RoleName = "PlusUser",
        });

        // Re-login so the auth cookie picks up the new PlusUser claim.
        await session.LoginAsync(email, password);
    }

    // -------------------------------------------------------------------------
    // Helper: create a conference with two drafts and return the conference ID.
    // -------------------------------------------------------------------------
    private static async Task<Guid> CreateTwoDraftConferenceAsync(ApiSession managerSession, int year)
    {
        return await managerSession.Conference.CreateConferenceAsync(new CreateConferenceRequest
        {
            ConferenceName = $"TestConf-{Guid.NewGuid():N}"[..30],
            PrimaryLeagueName = $"Primary-{Guid.NewGuid():N}"[..20],
            CustomRulesConference = false,
            LeagueYearSettings = LeagueScenarios.Standard.BuildSettings(year),
            Drafts = new List<DraftSettingsRequest>
            {
                new() { Name = null, ScheduledDate = null, GamesToDraft = 3, CounterPicksToDraft = 1 },
                new() { Name = "Draft 2", ScheduledDate = null, GamesToDraft = 3, CounterPicksToDraft = 0 },
            },
        });
    }

    // -------------------------------------------------------------------------
    // Helper: invite + join user B to the conference via an invite link.
    // -------------------------------------------------------------------------
    private static async Task InviteAndJoinConferenceAsync(
        ApiSession managerSession,
        ApiSession joiningSession,
        Guid conferenceID)
    {
        await managerSession.Conference.CreateInviteLinkAsync(
            new CreateConferenceInviteLinkRequest { ConferenceID = conferenceID });

        var links = await managerSession.Conference.InviteLinksAsync(conferenceID);

        await joiningSession.Conference.JoinWithInviteLinkAsync(
            new JoinConferenceWithInviteLinkRequest
            {
                ConferenceID = conferenceID,
                InviteCode = links.First().InviteCode,
            });
    }

    // -------------------------------------------------------------------------
    // Helper: get the full conference data (minimal deserialization).
    // -------------------------------------------------------------------------
    private static async Task<SimpleConferenceVm> GetConferenceAsync(
        ApiSession session,
        Guid conferenceID)
    {
        return await session.GetAndDeserializeAsync<SimpleConferenceVm>(
            $"api/Conference/GetConference/{conferenceID}");
    }

    // -------------------------------------------------------------------------
    // Test 1
    // -------------------------------------------------------------------------

    [Test]
    public async Task AddLeagueToConference_WithTwoDraftPrimary_ClonesAllDrafts()
    {
        using var sessionTemp = new ApiSession(Factory);
        var year = await LeagueTestHelpers.GetOpenYearAsync(sessionTemp);

        var (emailA, passwordA, displayNameA) = NewUser();
        var (emailB, passwordB, displayNameB) = NewUser();
        using var sessionA = new ApiSession(Factory);
        using var sessionB = new ApiSession(Factory);

        // User A needs PlusUser role to create a conference.
        await RegisterAsPlusUserAsync(sessionA, emailA, passwordA, displayNameA);
        await sessionB.RegisterAsync(emailB, passwordB, displayNameB);

        var conferenceID = await CreateTwoDraftConferenceAsync(sessionA, year);

        await InviteAndJoinConferenceAsync(sessionA, sessionB, conferenceID);

        var userB = await sessionB.Account.CurrentUserAsync();

        await sessionA.Conference.AddLeagueToConferenceAsync(new AddLeagueToConferenceRequest
        {
            ConferenceID = conferenceID,
            Year = year,
            LeagueName = $"SecondLeague-{Guid.NewGuid():N}"[..20],
            LeagueManager = userB.UserID,
        });

        // Retrieve conference data to find the secondary league's ID.
        var conference = await GetConferenceAsync(sessionA, conferenceID);

        // The secondary league is the one that is NOT the primary league.
        var primaryLeagueID = conference.PrimaryLeague.LeagueID;
        var secondLeague = conference.LeaguesInConference
            .Single(l => l.LeagueID != primaryLeagueID);

        // Verify the secondary league received 2 cloned draft rows.
        var newLeagueYear = await sessionB.League.GetLeagueYearAsync(secondLeague.LeagueID, year, null);

        Assert.That(newLeagueYear, Is.Not.Null);
        Assert.That(newLeagueYear.Drafts, Has.Count.EqualTo(2),
            "AddLeagueToConference must clone ALL draft rows from the primary league.");

        var draft1 = newLeagueYear.Drafts.Single(d => d.DraftNumber == 1);
        Assert.That(draft1.GamesToDraft, Is.EqualTo(3));
        Assert.That(draft1.CounterPicksToDraft, Is.EqualTo(1));

        var draft2 = newLeagueYear.Drafts.Single(d => d.DraftNumber == 2);
        Assert.That(draft2.Name, Is.EqualTo("Draft 2"));
        Assert.That(draft2.GamesToDraft, Is.EqualTo(3));
        Assert.That(draft2.CounterPicksToDraft, Is.EqualTo(0));
    }

    // -------------------------------------------------------------------------
    // Test 2
    // -------------------------------------------------------------------------

    [Test]
    public async Task AssignLeaguePlayers_WithTwoDraftConference_UpdatesPositionsForAllDrafts()
    {
        using var sessionTemp = new ApiSession(Factory);
        var year = await LeagueTestHelpers.GetOpenYearAsync(sessionTemp);

        var (emailA, passwordA, displayNameA) = NewUser();
        var (emailB, passwordB, displayNameB) = NewUser();
        using var sessionA = new ApiSession(Factory);
        using var sessionB = new ApiSession(Factory);

        // User A needs PlusUser role to create a conference.
        await RegisterAsPlusUserAsync(sessionA, emailA, passwordA, displayNameA);
        await sessionB.RegisterAsync(emailB, passwordB, displayNameB);

        var userA = await sessionA.Account.CurrentUserAsync();
        var userB = await sessionB.Account.CurrentUserAsync();
        var userAId = userA.UserID;
        var userBId = userB.UserID;

        var conferenceID = await CreateTwoDraftConferenceAsync(sessionA, year);

        await InviteAndJoinConferenceAsync(sessionA, sessionB, conferenceID);

        // Add a second league managed by user B.
        await sessionA.Conference.AddLeagueToConferenceAsync(new AddLeagueToConferenceRequest
        {
            ConferenceID = conferenceID,
            Year = year,
            LeagueName = $"SecondLeague-{Guid.NewGuid():N}"[..20],
            LeagueManager = userBId,
        });

        // Discover league IDs.
        var conference = await GetConferenceAsync(sessionA, conferenceID);
        var primaryLeagueID = conference.PrimaryLeague.LeagueID;
        var secondLeagueID = conference.LeaguesInConference
            .Single(l => l.LeagueID != primaryLeagueID).LeagueID;

        // Initial assignment: A → primary, B → secondary.
        await sessionA.Conference.AssignLeaguePlayersAsync(new AssignLeaguePlayersRequest
        {
            ConferenceID = conferenceID,
            Year = year,
            LeagueAssignments = new Dictionary<string, ICollection<Guid>>
            {
                [primaryLeagueID.ToString()] = new List<Guid> { userAId },
                [secondLeagueID.ToString()] = new List<Guid> { userBId },
            },
        });

        // Both users create publishers in their assigned leagues.
        await LeagueTestHelpers.CreatePublisherAsync(sessionA, primaryLeagueID, year, "PublisherA");
        await LeagueTestHelpers.CreatePublisherAsync(sessionB, secondLeagueID, year, "PublisherB");

        // Swap assignment: B → primary, A → secondary.
        // Note: we do not pre-set draft order; AssignLeaguePlayers is responsible for
        // creating draft-position rows for ALL drafts (including draft 2) even when none existed.
        await sessionA.Conference.AssignLeaguePlayersAsync(new AssignLeaguePlayersRequest
        {
            ConferenceID = conferenceID,
            Year = year,
            LeagueAssignments = new Dictionary<string, ICollection<Guid>>
            {
                [primaryLeagueID.ToString()] = new List<Guid> { userBId },
                [secondLeagueID.ToString()] = new List<Guid> { userAId },
            },
        });

        // Verify: both drafts in the primary league have 1 publisher (B moved in).
        // Use sessionA (conference manager) to access both leagues.
        var updatedPrimary = await sessionA.League.GetLeagueYearAsync(primaryLeagueID, year, null);
        Assert.That(updatedPrimary.Drafts, Has.Count.EqualTo(2),
            "Primary league must still have 2 drafts after reassignment.");
        foreach (var draft in updatedPrimary.Drafts)
        {
            Assert.That(draft.PublisherDraftInfo, Has.Count.EqualTo(1),
                $"Primary Draft {draft.DraftNumber}: AssignLeaguePlayers must rebuild positions for ALL drafts.");
        }

        // Verify: both drafts in the secondary league have 1 publisher (A moved in).
        var updatedSecondary = await sessionA.League.GetLeagueYearAsync(secondLeagueID, year, null);
        Assert.That(updatedSecondary.Drafts, Has.Count.EqualTo(2),
            "Secondary league must still have 2 drafts after reassignment.");
        foreach (var draft in updatedSecondary.Drafts)
        {
            Assert.That(draft.PublisherDraftInfo, Has.Count.EqualTo(1),
                $"Secondary Draft {draft.DraftNumber}: AssignLeaguePlayers must rebuild positions for ALL drafts.");
        }
    }
}
