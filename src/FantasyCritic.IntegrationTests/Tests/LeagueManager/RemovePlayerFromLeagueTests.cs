using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.LeagueManager;

/// <summary>
/// The league manager removing a member from the league, which is only allowed for a member who has no publisher in any
/// year: a no-show who never made one, or a player whose only publisher was disconnected from them.
/// </summary>
[TestFixture]
public class RemovePlayerFromLeagueTests : IntegrationTestBase
{
    private LeagueFixture _league = null!;
    private ApiSession _noShowSession = null!;
    private ApiSession _strangerSession = null!;

    private Guid _managerUserID;
    private Guid _playerWithPublisherUserID;
    private Guid _noShowUserID;
    private Guid _disconnectedPlayerUserID;
    private Guid _disconnectedPublisherID;
    private Guid _fullyRemovedPlayerUserID;
    private Guid _fullyRemovedPublisherID;

    private int? _removeNoShowStatusCode;
    private int? _removePlayerWithPublisherStatusCode;
    private int? _removeManagerStatusCode;
    private int? _removeStrangerStatusCode;
    private int? _removeByNonManagerStatusCode;
    private int? _removeDisconnectedPlayerStatusCode;
    private int? _removeFullyRemovedPlayerStatusCode;
    private int? _removedPlayerViewsLeagueStatusCode;
    private LeagueYearViewModel _afterRemovals = null!;

    [OneTimeSetUp]
    public async Task SetUpLeague()
    {
        _league = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(Factory, LeagueScenarios.Standard, NewUser);
        var manager = _league.Manager;
        var playerWithPublisher = _league.Publishers[1];
        var disconnectedPlayer = _league.Publishers[2];
        var fullyRemovedPlayer = _league.Publishers[3];

        _managerUserID = (await manager.Account.CurrentUserAsync()).UserID;
        _playerWithPublisherUserID = (await playerWithPublisher.Session.Account.CurrentUserAsync()).UserID;
        _disconnectedPlayerUserID = (await disconnectedPlayer.Session.Account.CurrentUserAsync()).UserID;
        _fullyRemovedPlayerUserID = (await fullyRemovedPlayer.Session.Account.CurrentUserAsync()).UserID;
        _disconnectedPublisherID = disconnectedPlayer.PublisherID;
        _fullyRemovedPublisherID = fullyRemovedPlayer.PublisherID;

        _noShowSession = await RegisterAsync();
        await LeagueTestHelpers.InviteAndAcceptAsync(manager, _noShowSession, _league.LeagueID);
        _noShowUserID = (await _noShowSession.Account.CurrentUserAsync()).UserID;

        _strangerSession = await RegisterAsync();
        var strangerUserID = (await _strangerSession.Account.CurrentUserAsync()).UserID;

        _removeNoShowStatusCode = await RemovePlayerAsync(manager, _noShowUserID);
        _removePlayerWithPublisherStatusCode = await RemovePlayerAsync(manager, _playerWithPublisherUserID);
        _removeManagerStatusCode = await RemovePlayerAsync(manager, _managerUserID);
        _removeStrangerStatusCode = await RemovePlayerAsync(manager, strangerUserID);
        _removeByNonManagerStatusCode = await RemovePlayerAsync(playerWithPublisher.Session, _disconnectedPlayerUserID);

        await manager.LeagueManager.DisconnectPlayerAsync(new DisconnectPlayerRequest { PublisherID = _disconnectedPublisherID });
        _removeDisconnectedPlayerStatusCode = await RemovePlayerAsync(manager, _disconnectedPlayerUserID);

        await manager.LeagueManager.DisconnectPlayerAsync(new DisconnectPlayerRequest { PublisherID = _fullyRemovedPublisherID });
        await manager.LeagueManager.RemovePublisherAsync(new RemovePublisherRequest { PublisherID = _fullyRemovedPublisherID });
        _removeFullyRemovedPlayerStatusCode = await RemovePlayerAsync(manager, _fullyRemovedPlayerUserID);

        _removedPlayerViewsLeagueStatusCode = await CaptureApiStatusCodeAsync(() =>
            fullyRemovedPlayer.Session.League.GetLeagueYearAsync(_league.LeagueID, _league.Year, null));
        _afterRemovals = await _league.GetLeagueYearAsync();
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        _noShowSession?.Dispose();
        _strangerSession?.Dispose();

        if (_league != null)
        {
            await _league.DisposeAsync();
        }
    }

    [Test]
    public void NoShow_WithNoPublisher_IsRemoved()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_removeNoShowStatusCode, Is.Null);
            Assert.That(LeagueMemberIDs(), Does.Not.Contain(_noShowUserID));
        }
    }

    [Test]
    public void Remove_IsRejected_ForAPlayerWithAPublisher_TheManager_ANonMember_OrANonManager()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_removePlayerWithPublisherStatusCode, Is.EqualTo(400));
            Assert.That(_removeManagerStatusCode, Is.EqualTo(400));
            Assert.That(_removeStrangerStatusCode, Is.EqualTo(400));
            Assert.That(_removeByNonManagerStatusCode, Is.EqualTo(403));
            Assert.That(LeagueMemberIDs(), Does.Contain(_playerWithPublisherUserID));
            Assert.That(LeagueMemberIDs(), Does.Contain(_managerUserID));
        }
    }

    [Test]
    public void DisconnectedPlayer_IsRemoved_AndTheirPublisherStays()
    {
        var publisher = _afterRemovals.Publishers.SingleOrDefault(x => x.PublisherID == _disconnectedPublisherID);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_removeDisconnectedPlayerStatusCode, Is.Null);
            Assert.That(LeagueMemberIDs(), Does.Not.Contain(_disconnectedPlayerUserID));
            Assert.That(publisher, Is.Not.Null);
            Assert.That(publisher?.UserID, Is.Null);
        }
    }

    [Test]
    public void FullNoShowFlow_DisconnectRemovePublisherRemovePlayer_RemovesBoth()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_removeFullyRemovedPlayerStatusCode, Is.Null);
            Assert.That(LeagueMemberIDs(), Does.Not.Contain(_fullyRemovedPlayerUserID));
            Assert.That(_afterRemovals.Publishers.Select(x => x.PublisherID), Does.Not.Contain(_fullyRemovedPublisherID));
            Assert.That(_removedPlayerViewsLeagueStatusCode, Is.EqualTo(403));
        }
    }

    private IReadOnlyList<Guid> LeagueMemberIDs() => _afterRemovals.League.Players!.Select(x => x.UserID).ToList();

    private async Task<ApiSession> RegisterAsync()
    {
        var (email, password, displayName) = NewUser();
        var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);
        return session;
    }

    private Task<int?> RemovePlayerAsync(ApiSession session, Guid userID) =>
        CaptureApiStatusCodeAsync(() => session.LeagueManager.RemovePlayerFromLeagueAsync(new RemovePlayerFromLeagueRequest
        {
            LeagueID = _league.LeagueID,
            UserID = userID,
        }));

    private static async Task<int?> CaptureApiStatusCodeAsync(Func<Task> action)
    {
        try
        {
            await action();
            return null;
        }
        catch (ApiException ex)
        {
            return ex.StatusCode;
        }
    }
}
