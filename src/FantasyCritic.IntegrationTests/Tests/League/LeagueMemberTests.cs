using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League;

/// <summary>
/// Tests member-management workflows: inviting, accepting, creating publishers,
/// setting draft order, invite links, and invite rescind.
///
/// <see cref="SetUpMembers"/> runs once and builds a fully-populated 4-player league
/// (manager + 3 players, all with publishers, draft order set). The individual
/// [Test] methods assert against that state. Tests are ordered so that destructive
/// operations (rescind) run after the read-only checks.
/// </summary>
[TestFixture]
public class LeagueMemberTests : IntegrationTestBase
{
    private ApiSession _managerSession = null!;
    private List<ApiSession> _playerSessions = null!;
    private Guid _leagueID;
    private int _year;
    private List<Guid> _publisherIDs = null!;

    [OneTimeSetUp]
    public async Task SetUpMembers()
    {
        var (mgrEmail, mgrPassword, mgrDisplayName) = NewUser();
        _managerSession = new ApiSession(Factory);
        await _managerSession.RegisterAsync(mgrEmail, mgrPassword, mgrDisplayName);

        _playerSessions = new List<ApiSession>();
        for (int i = 0; i < LeagueScenarios.Standard.PlayerCount - 1; i++)
        {
            var (email, password, displayName) = NewUser();
            var session = new ApiSession(Factory);
            await session.RegisterAsync(email, password, displayName);
            _playerSessions.Add(session);
        }

        _year = await LeagueTestHelpers.GetOpenYearAsync(_managerSession);
        _leagueID = await LeagueTestHelpers.CreateLeagueAsync(_managerSession, LeagueScenarios.Standard, _year);

        foreach (var playerSession in _playerSessions)
        {
            await LeagueTestHelpers.InviteAndAcceptAsync(_managerSession, playerSession, _leagueID);
        }

        _publisherIDs = new List<Guid>();
        var managerPubID = await LeagueTestHelpers.CreatePublisherAsync(
            _managerSession, _leagueID, _year, "Manager Publisher");
        _publisherIDs.Add(managerPubID);

        for (int i = 0; i < _playerSessions.Count; i++)
        {
            var pubID = await LeagueTestHelpers.CreatePublisherAsync(
                _playerSessions[i], _leagueID, _year, $"Player{i + 1} Publisher");
            _publisherIDs.Add(pubID);
        }

        await LeagueTestHelpers.SetDraftOrderAsync(_managerSession, _leagueID, _year, _publisherIDs);
    }

    [OneTimeTearDown]
    public void TearDownSessions()
    {
        _managerSession?.Dispose();
        if (_playerSessions != null)
            foreach (var s in _playerSessions)
                s.Dispose();
    }

    // ---------------------------------------------------------------------------
    // Read-only checks (run first)
    // ---------------------------------------------------------------------------

    [Test, Order(1)]
    public async Task League_AfterSetup_IsManager_ForCreator()
    {
        var league = await _managerSession.League.GetLeagueAsync(_leagueID, null);
        Assert.That(league.IsManager, Is.True);
    }

    [Test, Order(2)]
    public async Task LeagueYear_AfterSetup_HasExpectedPublisherCount()
    {
        var leagueYear = await _managerSession.League.GetLeagueYearAsync(_leagueID, _year, null);
        Assert.That(leagueYear.Publishers.Count,
            Is.EqualTo(LeagueScenarios.Standard.PlayerCount),
            "Every player (including manager) must have a publisher.");
    }

    [Test, Order(3)]
    public async Task LeagueYear_AfterSetup_DraftOrderIsSet()
    {
        var leagueYear = await _managerSession.League.GetLeagueYearAsync(_leagueID, _year, null);

        // All publishers should have a position ≥ 1 (0 means unset)
        var positions = leagueYear.Publishers.Select(p => p.DraftPosition).ToList();
        Assert.That(positions, Has.All.GreaterThan(0), "All publishers must have a draft position ≥ 1.");

        // Positions should be exactly 1..N with no duplicates
        var expected = Enumerable.Range(1, LeagueScenarios.Standard.PlayerCount).ToList();
        Assert.That(positions.OrderBy(x => x).ToList(),
            Is.EqualTo(expected),
            "Draft positions must be 1..N.");
    }

    [Test, Order(4)]
    public async Task LeagueYear_AfterSetup_PlayStatusIsReadyToDraft()
    {
        var leagueYear = await _managerSession.League.GetLeagueYearAsync(_leagueID, _year, null);
        Assert.That(leagueYear.PlayStatus.ReadyToDraft, Is.True,
            "All publishers have been created and draft order set — league should be ready to draft.");
        Assert.That(leagueYear.PlayStatus.StartDraftErrors, Is.Empty,
            $"No start-draft errors expected. Got: {string.Join("; ", leagueYear.PlayStatus.StartDraftErrors)}");
    }

    [Test, Order(5)]
    public async Task Players_AfterAcceptingInvites_AreVisibleInLeague()
    {
        var league = await _managerSession.League.GetLeagueAsync(_leagueID, null);
        Assert.That(league.Players, Is.Not.Null);
        Assert.That(league.Players!.Count, Is.EqualTo(LeagueScenarios.Standard.PlayerCount));
    }

    // ---------------------------------------------------------------------------
    // Invite link tests
    // ---------------------------------------------------------------------------

    [Test, Order(10)]
    public async Task InviteLink_Created_IsReturnedByInviteLinks()
    {
        await _managerSession.LeagueManager.CreateInviteLinkAsync(
            new CreateInviteLinkRequest { LeagueID = _leagueID });

        var links = await _managerSession.LeagueManager.InviteLinksAsync(_leagueID);

        Assert.That(links, Is.Not.Empty, "At least one invite link must be returned.");
        Assert.That(links.First().LeagueID, Is.EqualTo(_leagueID));
        Assert.That(links.First().InviteCode, Is.Not.EqualTo(Guid.Empty));
    }

    [Test, Order(11)]
    public async Task JoinWithInviteLink_NewUser_JoinsLeague()
    {
        await _managerSession.LeagueManager.CreateInviteLinkAsync(
            new CreateInviteLinkRequest { LeagueID = _leagueID });
        var links = await _managerSession.LeagueManager.InviteLinksAsync(_leagueID);
        var inviteCode = links.First().InviteCode;

        var (email, password, displayName) = NewUser();
        using var newPlayerSession = new ApiSession(Factory);
        await newPlayerSession.RegisterAsync(email, password, displayName);

        await newPlayerSession.League.JoinWithInviteLinkAsync(new JoinWithInviteLinkRequest
        {
            LeagueID = _leagueID,
            InviteCode = inviteCode,
        });

        // JoinWithInviteLink directly accepts the invite (no pending state),
        // so the user should now appear as a member of the league.
        var league = await newPlayerSession.League.GetLeagueAsync(_leagueID, null);
        Assert.That(league.UserIsInLeague, Is.True,
            "New player should be a league member after joining with invite link.");
    }

    // ---------------------------------------------------------------------------
    // Invite rescind test
    // ---------------------------------------------------------------------------

    [Test, Order(20)]
    public async Task RescindInvite_PendingInvite_RemovesItFromPlayerView()
    {
        var (email, password, displayName) = NewUser();
        using var pendingSession = new ApiSession(Factory);
        await pendingSession.RegisterAsync(email, password, displayName);

        var pendingUser = await pendingSession.Account.CurrentUserAsync();
        await _managerSession.LeagueManager.InvitePlayerAsync(new CreateInviteRequest
        {
            LeagueID = _leagueID,
            InviteEmail = pendingUser.EmailAddress,
        });

        var invitesBefore = await pendingSession.League.MyInvitesAsync();
        var invite = invitesBefore.SingleOrDefault(i => i.LeagueID == _leagueID);
        Assert.That(invite, Is.Not.Null, "Pending invite should be visible to the invitee.");

        await _managerSession.LeagueManager.RescindInviteAsync(new DeleteInviteRequest
        {
            LeagueID = _leagueID,
            InviteID = invite!.InviteID,
        });

        var invitesAfter = await pendingSession.League.MyInvitesAsync();
        Assert.That(invitesAfter.Any(i => i.LeagueID == _leagueID), Is.False,
            "Invite should be gone after rescind.");
    }
}
