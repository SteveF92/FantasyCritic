using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Setup;

/// <summary>
/// Tests member-management workflows: inviting, accepting, creating publishers,
/// setting draft order, invite links, and invite rescind.
/// </summary>
[TestFixture]
public class LeagueMemberTests : IntegrationTestBase
{
    private LeagueFixture _league = null!;

    [OneTimeSetUp]
    public async Task SetUpMembers()
    {
        _league = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(
            Factory, LeagueScenarios.Standard, NewUser);
    }

    [OneTimeTearDown]
    public async Task TearDownSessions() => await _league.DisposeAsync();

    [Test, Order(1)]
    public async Task League_AfterSetup_IsManager_ForCreator()
    {
        var league = await _league.Manager.League.GetLeagueAsync(_league.LeagueID, null);
        Assert.That(league.IsManager, Is.True);
    }

    [Test, Order(2)]
    public async Task LeagueYear_AfterSetup_HasExpectedPublisherCount()
    {
        var leagueYear = await _league.GetLeagueYearAsync();
        Assert.That(leagueYear.Publishers.Count,
            Is.EqualTo(LeagueScenarios.Standard.PlayerCount),
            "Every player (including manager) must have a publisher.");
    }

    [Test, Order(3)]
    public async Task LeagueYear_AfterSetup_DraftOrderIsSet()
    {
        var leagueYear = await _league.GetLeagueYearAsync();

        var positions = leagueYear.Publishers.Select(p => p.DraftPosition).ToList();
        Assert.That(positions, Has.All.GreaterThan(0), "All publishers must have a draft position ≥ 1.");

        var expected = Enumerable.Range(1, LeagueScenarios.Standard.PlayerCount).ToList();
        Assert.That(positions.OrderBy(x => x).ToList(),
            Is.EqualTo(expected),
            "Draft positions must be 1..N.");
    }

    [Test, Order(4)]
    public async Task LeagueYear_AfterSetup_PlayStatusIsReadyToDraft()
    {
        var leagueYear = await _league.GetLeagueYearAsync();
        Assert.That(leagueYear.PlayStatus.ReadyToDraft, Is.True,
            "All publishers have been created and draft order set — league should be ready to draft.");
        Assert.That(leagueYear.PlayStatus.StartDraftErrors, Is.Empty,
            $"No start-draft errors expected. Got: {string.Join("; ", leagueYear.PlayStatus.StartDraftErrors)}");
    }

    [Test, Order(5)]
    public async Task Players_AfterAcceptingInvites_AreVisibleInLeague()
    {
        var league = await _league.Manager.League.GetLeagueAsync(_league.LeagueID, null);
        Assert.That(league.Players, Is.Not.Null);
        Assert.That(league.Players!.Count, Is.EqualTo(LeagueScenarios.Standard.PlayerCount));
    }

    [Test, Order(10)]
    public async Task InviteLink_Created_IsReturnedByInviteLinks()
    {
        await _league.Manager.LeagueManager.CreateInviteLinkAsync(
            new CreateInviteLinkRequest { LeagueID = _league.LeagueID });

        var links = await _league.Manager.LeagueManager.InviteLinksAsync(_league.LeagueID);

        Assert.That(links, Is.Not.Empty, "At least one invite link must be returned.");
        Assert.That(links.First().LeagueID, Is.EqualTo(_league.LeagueID));
        Assert.That(links.First().InviteCode, Is.Not.EqualTo(Guid.Empty));
    }

    [Test, Order(11)]
    public async Task JoinWithInviteLink_NewUser_JoinsLeague()
    {
        await _league.Manager.LeagueManager.CreateInviteLinkAsync(
            new CreateInviteLinkRequest { LeagueID = _league.LeagueID });
        var links = await _league.Manager.LeagueManager.InviteLinksAsync(_league.LeagueID);
        var inviteCode = links.First().InviteCode;

        var (email, password, displayName) = NewUser();
        using var newPlayerSession = new ApiSession(Factory);
        await newPlayerSession.RegisterAsync(email, password, displayName);

        await newPlayerSession.League.JoinWithInviteLinkAsync(new JoinWithInviteLinkRequest
        {
            LeagueID = _league.LeagueID,
            InviteCode = inviteCode,
        });

        var league = await newPlayerSession.League.GetLeagueAsync(_league.LeagueID, null);
        Assert.That(league.UserIsInLeague, Is.True,
            "New player should be a league member after joining with invite link.");
    }

    [Test, Order(20)]
    public async Task RescindInvite_PendingInvite_RemovesItFromPlayerView()
    {
        var (email, password, displayName) = NewUser();
        using var pendingSession = new ApiSession(Factory);
        await pendingSession.RegisterAsync(email, password, displayName);

        var pendingUser = await pendingSession.Account.CurrentUserAsync();
        await _league.Manager.LeagueManager.InvitePlayerAsync(new CreateInviteRequest
        {
            LeagueID = _league.LeagueID,
            InviteEmail = pendingUser.EmailAddress,
        });

        var invitesBefore = await pendingSession.League.MyInvitesAsync();
        var invite = invitesBefore.SingleOrDefault(i => i.LeagueID == _league.LeagueID);
        Assert.That(invite, Is.Not.Null, "Pending invite should be visible to the invitee.");

        await _league.Manager.LeagueManager.RescindInviteAsync(new DeleteInviteRequest
        {
            LeagueID = _league.LeagueID,
            InviteID = invite!.InviteID,
        });

        var invitesAfter = await pendingSession.League.MyInvitesAsync();
        Assert.That(invitesAfter.Any(i => i.LeagueID == _league.LeagueID), Is.False,
            "Invite should be gone after rescind.");
    }
}
