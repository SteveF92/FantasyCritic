using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.LeagueManager;

/// <summary>
/// The league manager disconnecting a player from their publisher: once mid-season, after the player has pending
/// bids, drops, a watchlist and an open trade, and once before the draft, after which the draft skips the publisher.
/// </summary>
[TestFixture]
public class DisconnectPlayerTests : IntegrationTestBase
{
    private ApiSession _adminSession = null!;
    private LeagueFixture _midSeasonLeague = null!;
    private LeagueFixture _preDraftLeague = null!;

    private TestPublisher _disconnectedPublisher = null!;
    private Guid _disconnectedUserID;
    private Guid _disconnectedPlayerTradeID;
    private Guid _otherPlayersTradeID;

    private LeagueYearViewModel _afterDisconnect = null!;
    private LeagueYearViewModel _disconnectedPlayersViewAfterDisconnect = null!;
    private IReadOnlyList<TradeViewModel> _tradeHistoryAfterDisconnect = null!;
    private IReadOnlyList<LeagueActionViewModel> _leagueActionsAfterDisconnect = null!;
    private int? _bidAfterDisconnectStatusCode;
    private int? _secondDisconnectStatusCode;
    private int? _disconnectManagerStatusCode;
    private int? _disconnectByNonManagerStatusCode;
    private LeagueYearViewModel _disconnectedPlayersViewAfterReassign = null!;

    private ApiSession _replacementSession = null!;
    private TestPublisher _handedOverPublisher = null!;
    private Guid _replacementUserID;
    private Guid _replacedUserID;
    private LeagueYearViewModel _replacementsViewAfterReassign = null!;
    private LeagueYearViewModel _replacedPlayersViewAfterReassign = null!;
    private QueueResultViewModel _replacementsQueueResult = null!;

    private TestPublisher _preDraftDisconnectedPublisher = null!;
    private LeagueYearViewModel _preDraftAfterDraft = null!;

    [OneTimeSetUp]
    public async Task SetUpDisconnectedLeagues()
    {
        _adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(_adminSession);

        await _adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 6, 12, 0, 0, TimeSpan.Zero)
        });

        await SetUpMidSeasonLeagueAsync();
        await SetUpPreDraftLeagueAsync();
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        if (_adminSession != null)
        {
            await _adminSession.Admin.ResetTimeAsync();
        }

        _adminSession?.Dispose();
        _replacementSession?.Dispose();

        if (_midSeasonLeague != null)
        {
            await _midSeasonLeague.DisposeAsync();
        }

        if (_preDraftLeague != null)
        {
            await _preDraftLeague.DisposeAsync();
        }
    }

    [Test]
    public void Disconnect_LeavesThePublisherWithNoUser()
    {
        var publisher = _afterDisconnect.Publishers.Single(x => x.PublisherID == _disconnectedPublisher.PublisherID);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(publisher.UserID, Is.Null);
            Assert.That(publisher.PlayerName, Is.EqualTo("User Disconnected"));
            Assert.That(publisher.Games, Is.Not.Empty, "The publisher keeps its roster.");
        }
    }

    [Test]
    public void Disconnect_KeepsThePublishersStandingsRow()
    {
        var row = _afterDisconnect.Players.SingleOrDefault(x => x.Publisher?.PublisherID == _disconnectedPublisher.PublisherID);
        Assert.That(row, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(row!.User, Is.Null);
            Assert.That(row.InviteID, Is.Null);
        }
    }

    [Test]
    public void Disconnect_MarksThePlayerInactive()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_disconnectedPlayersViewAfterDisconnect.UserIsActive, Is.False);
            Assert.That(_afterDisconnect.Players.Any(x => x.User?.UserID == _disconnectedUserID), Is.False);
        }
    }

    [Test]
    public void Disconnect_RejectsOnlyThePlayersOpenTrades()
    {
        var rejectedTrade = _tradeHistoryAfterDisconnect.SingleOrDefault(x => x.TradeID == _disconnectedPlayerTradeID);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_afterDisconnect.ActiveTrades.Select(x => x.TradeID), Does.Not.Contain(_disconnectedPlayerTradeID));
            Assert.That(_afterDisconnect.ActiveTrades.Select(x => x.TradeID), Does.Contain(_otherPlayersTradeID));
            Assert.That(rejectedTrade?.Status, Is.EqualTo("RejectedByManager"));
        }
    }

    [Test]
    public void Disconnect_RecordsAManagerAction()
    {
        var action = _leagueActionsAfterDisconnect.SingleOrDefault(x => x.ActionType == "Player Disconnected");
        Assert.That(action, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action!.PublisherName, Is.EqualTo(_disconnectedPublisher.PublisherName));
            Assert.That(action.ManagerAction, Is.True);
        }
    }

    [Test]
    public void DisconnectedPlayer_CannotActForThePublisher()
    {
        Assert.That(_bidAfterDisconnectStatusCode, Is.EqualTo(403));
    }

    [Test]
    public void Disconnect_IsRejected_ForAPublisherWithNoUser_TheManagersOwnPublisher_OrANonManager()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_secondDisconnectStatusCode, Is.EqualTo(400));
            Assert.That(_disconnectManagerStatusCode, Is.EqualTo(400));
            Assert.That(_disconnectByNonManagerStatusCode, Is.EqualTo(403));
        }
    }

    [Test]
    public void Reassign_ReattachesThePublisher_WithNoPendingBidsDropsOrWatchlist()
    {
        var publisher = _disconnectedPlayersViewAfterReassign.Publishers.Single(x => x.PublisherID == _disconnectedPublisher.PublisherID);
        var privateData = _disconnectedPlayersViewAfterReassign.PrivatePublisherData;
        Assert.That(privateData, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(publisher.UserID, Is.EqualTo(_disconnectedUserID));
            Assert.That(_disconnectedPlayersViewAfterReassign.UserIsActive, Is.True);
            Assert.That(privateData!.MyActiveBids, Is.Empty);
            Assert.That(privateData.MyActiveDrops, Is.Empty);
            Assert.That(privateData.QueuedGames, Is.Empty);
        }
    }

    [Test]
    public void Reassign_HandsThePublisherToANewlyInvitedPlayer()
    {
        var publisher = _replacementsViewAfterReassign.Publishers.Single(x => x.PublisherID == _handedOverPublisher.PublisherID);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(publisher.UserID, Is.EqualTo(_replacementUserID));
            Assert.That(_replacementsViewAfterReassign.UserIsActive, Is.True);
            Assert.That(_replacementsQueueResult.Success, Is.True, $"The new player could not act for the publisher: {string.Join("; ", _replacementsQueueResult.Errors ?? [])}");
            Assert.That(_replacedPlayersViewAfterReassign.UserIsActive, Is.False);
            Assert.That(_replacementsViewAfterReassign.Players.Any(x => x.User?.UserID == _replacedUserID), Is.False);
        }
    }

    [Test]
    public void PreDraftDisconnect_DraftSkipsThePublisher()
    {
        var publisher = _preDraftAfterDraft.Publishers.Single(x => x.PublisherID == _preDraftDisconnectedPublisher.PublisherID);
        var otherPublishers = _preDraftAfterDraft.Publishers.Where(x => x.PublisherID != _preDraftDisconnectedPublisher.PublisherID).ToList();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(publisher.UserID, Is.Null);
            Assert.That(publisher.Games, Is.Empty);
            Assert.That(otherPublishers.All(x => x.Games.Count > 0), Is.True);
        }
    }

    private async Task SetUpMidSeasonLeagueAsync()
    {
        _midSeasonLeague = await LeagueFixtureBuilder.CreateAndStartDraftAsync(Factory, LeagueScenarios.FourPlayerDrops, NewUser);
        await _midSeasonLeague.DraftToCompletionAsync();

        var manager = _midSeasonLeague.Manager;
        var managerPublisher = _midSeasonLeague.Publishers[0];
        _disconnectedPublisher = _midSeasonLeague.Publishers[1];
        var counterParty = _midSeasonLeague.Publishers[2];
        var otherPlayer = _midSeasonLeague.Publishers[3];
        _disconnectedUserID = (await _disconnectedPublisher.Session.Account.CurrentUserAsync()).UserID;

        _disconnectedPlayerTradeID = await ProposeOneForOneTradeAsync(_midSeasonLeague, _disconnectedPublisher, counterParty, "trade with the disconnected player");
        _otherPlayersTradeID = await ProposeOneForOneTradeAsync(_midSeasonLeague, otherPlayer, managerPublisher, "trade between other players");

        var postDraft = await _midSeasonLeague.GetLeagueYearAsync();
        var disconnectedPublisherBeforeDisconnect = postDraft.Publishers.Single(x => x.PublisherID == _disconnectedPublisher.PublisherID);
        var tradedGameID = GetTradeableGame(postDraft, _disconnectedPublisher.PublisherID).PublisherGameID;
        var dropGame = FindDroppableDraftedGame(disconnectedPublisherBeforeDisconnect, tradedGameID);
        var bidTarget = await PickAvailableBidTargetAsync(_midSeasonLeague, _disconnectedPublisher, [dropGame.MasterGame!.MasterGameID]);
        var queueTarget = await PickAvailableBidTargetAsync(_midSeasonLeague, _disconnectedPublisher, [dropGame.MasterGame!.MasterGameID, bidTarget]);

        await LeaguePickupActions.PlaceDropAsync(_disconnectedPublisher, dropGame.PublisherGameID);
        await LeaguePickupActions.PlaceBidAsync(_disconnectedPublisher, bidTarget, 10, false);
        var queueResult = await _disconnectedPublisher.Session.League.AddGameToQueueAsync(new AddGameToQueueRequest
        {
            PublisherID = _disconnectedPublisher.PublisherID,
            MasterGameID = queueTarget,
        });
        Assert.That(queueResult.Success, Is.True, $"Failed to queue game: {string.Join("; ", queueResult.Errors ?? [])}");

        await manager.LeagueManager.DisconnectPlayerAsync(new DisconnectPlayerRequest { PublisherID = _disconnectedPublisher.PublisherID });

        _afterDisconnect = await _midSeasonLeague.GetLeagueYearAsync();
        _disconnectedPlayersViewAfterDisconnect = await _disconnectedPublisher.Session.League.GetLeagueYearAsync(_midSeasonLeague.LeagueID, _midSeasonLeague.Year, null);
        _tradeHistoryAfterDisconnect = (await manager.League.TradeHistoryAsync(_midSeasonLeague.LeagueID, _midSeasonLeague.Year)).ToList();
        _leagueActionsAfterDisconnect = (await manager.League.GetLeagueActionsAsync(_midSeasonLeague.LeagueID, _midSeasonLeague.Year)).ToList();

        var secondBidTarget = await PickAvailableBidTargetAsync(_midSeasonLeague, otherPlayer, [dropGame.MasterGame!.MasterGameID, bidTarget, queueTarget]);
        _bidAfterDisconnectStatusCode = await CaptureApiStatusCodeAsync(() => LeaguePickupActions.TryPlaceBidAsync(_disconnectedPublisher, secondBidTarget, 5, false));
        _secondDisconnectStatusCode = await CaptureApiStatusCodeAsync(() =>
            manager.LeagueManager.DisconnectPlayerAsync(new DisconnectPlayerRequest { PublisherID = _disconnectedPublisher.PublisherID }));
        _disconnectManagerStatusCode = await CaptureApiStatusCodeAsync(() =>
            manager.LeagueManager.DisconnectPlayerAsync(new DisconnectPlayerRequest { PublisherID = managerPublisher.PublisherID }));
        _disconnectByNonManagerStatusCode = await CaptureApiStatusCodeAsync(() =>
            otherPlayer.Session.LeagueManager.DisconnectPlayerAsync(new DisconnectPlayerRequest { PublisherID = counterParty.PublisherID }));

        await manager.LeagueManager.ReassignPublisherAsync(new ReassignPublisherRequest
        {
            LeagueID = _midSeasonLeague.LeagueID,
            Year = _midSeasonLeague.Year,
            PublisherID = _disconnectedPublisher.PublisherID,
            NewUserID = _disconnectedUserID,
        });
        _disconnectedPlayersViewAfterReassign = await _disconnectedPublisher.Session.League.GetLeagueYearAsync(_midSeasonLeague.LeagueID, _midSeasonLeague.Year, null);

        await HandOverAPublisherToANewPlayerAsync(otherPlayer, [dropGame.MasterGame!.MasterGameID, bidTarget, queueTarget]);
    }

    private async Task HandOverAPublisherToANewPlayerAsync(TestPublisher publisherToHandOver, IEnumerable<Guid> gamesAlreadyQueued)
    {
        var manager = _midSeasonLeague.Manager;
        _handedOverPublisher = publisherToHandOver;
        _replacedUserID = (await publisherToHandOver.Session.Account.CurrentUserAsync()).UserID;

        var (email, password, displayName) = NewUser();
        _replacementSession = new ApiSession(Factory);
        await _replacementSession.RegisterAsync(email, password, displayName);
        await LeagueTestHelpers.InviteAndAcceptAsync(manager, _replacementSession, _midSeasonLeague.LeagueID);
        _replacementUserID = (await _replacementSession.Account.CurrentUserAsync()).UserID;

        await manager.LeagueManager.DisconnectPlayerAsync(new DisconnectPlayerRequest { PublisherID = publisherToHandOver.PublisherID });
        await manager.LeagueManager.ReassignPublisherAsync(new ReassignPublisherRequest
        {
            LeagueID = _midSeasonLeague.LeagueID,
            Year = _midSeasonLeague.Year,
            PublisherID = publisherToHandOver.PublisherID,
            NewUserID = _replacementUserID,
        });

        _replacementsViewAfterReassign = await _replacementSession.League.GetLeagueYearAsync(_midSeasonLeague.LeagueID, _midSeasonLeague.Year, null);
        _replacedPlayersViewAfterReassign = await publisherToHandOver.Session.League.GetLeagueYearAsync(_midSeasonLeague.LeagueID, _midSeasonLeague.Year, null);

        var replacementPublisher = new TestPublisher(publisherToHandOver.DraftPosition, _replacementSession, publisherToHandOver.PublisherID, publisherToHandOver.PublisherName);
        var queueTarget = await PickAvailableBidTargetAsync(_midSeasonLeague, replacementPublisher, gamesAlreadyQueued);
        _replacementsQueueResult = await _replacementSession.League.AddGameToQueueAsync(new AddGameToQueueRequest
        {
            PublisherID = publisherToHandOver.PublisherID,
            MasterGameID = queueTarget,
        });
    }

    private async Task SetUpPreDraftLeagueAsync()
    {
        _preDraftLeague = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(Factory, LeagueScenarios.Standard, NewUser);
        _preDraftDisconnectedPublisher = _preDraftLeague.Publishers[1];

        await _preDraftLeague.Manager.LeagueManager.DisconnectPlayerAsync(new DisconnectPlayerRequest { PublisherID = _preDraftDisconnectedPublisher.PublisherID });
        await _preDraftLeague.Manager.LeagueManager.StartDraftAsync(new StartDraftRequest
        {
            LeagueID = _preDraftLeague.LeagueID,
            Year = _preDraftLeague.Year,
        });

        var connectedPublishers = _preDraftLeague.Publishers
            .Where(x => x.PublisherID != _preDraftDisconnectedPublisher.PublisherID)
            .ToDictionary(x => x.PublisherID, x => x.Session);
        await _preDraftLeague.DraftToCompletionAsync(connectedPublishers);
        _preDraftAfterDraft = await _preDraftLeague.GetLeagueYearAsync();
    }

    private static PublisherGameViewModel FindDroppableDraftedGame(PublisherViewModel publisher, Guid excludedPublisherGameID)
    {
        return publisher.Games.First(g =>
            g.PublisherGameID != excludedPublisherGameID
            && !g.CounterPick
            && !g.DropBlocked
            && g.MasterGame != null
            && g.OverallPickNumber.HasValue);
    }

    private static async Task<Guid> PickAvailableBidTargetAsync(LeagueFixture league, TestPublisher publisher, IEnumerable<Guid> excludedMasterGameIDs)
    {
        var excluded = excludedMasterGameIDs.ToHashSet();
        var available = await publisher.Session.League.TopAvailableGamesAsync(league.Year, league.LeagueID, publisher.PublisherID, null);
        var target = available.First(g =>
            g.IsAvailable
            && !g.Taken
            && !g.IsReleased
            && g.MasterGame != null
            && !excluded.Contains(g.MasterGame.MasterGameID));

        return target.MasterGame!.MasterGameID;
    }

    private static async Task<Guid> ProposeOneForOneTradeAsync(LeagueFixture league, TestPublisher proposer, TestPublisher counterParty, string message)
    {
        var leagueYear = await proposer.Session.League.GetLeagueYearAsync(league.LeagueID, league.Year, null);
        var proposerGame = GetTradeableGame(leagueYear, proposer.PublisherID);
        var counterPartyGame = GetTradeableGame(leagueYear, counterParty.PublisherID);

        await proposer.Session.League.ProposeTradeAsync(new ProposeTradeRequest
        {
            ProposerPublisherID = proposer.PublisherID,
            CounterPartyPublisherID = counterParty.PublisherID,
            ProposerPublisherGameIDs = [proposerGame.PublisherGameID],
            CounterPartyPublisherGameIDs = [counterPartyGame.PublisherGameID],
            ProposerBudgetSendAmount = 0,
            CounterPartyBudgetSendAmount = 0,
            Message = message,
        });

        var updatedLeagueYear = await proposer.Session.League.GetLeagueYearAsync(league.LeagueID, league.Year, null);
        return updatedLeagueYear.ActiveTrades.Single(x => x.Message == message).TradeID;
    }

    private static PublisherGameViewModel GetTradeableGame(LeagueYearViewModel leagueYear, Guid publisherID)
    {
        var publisher = leagueYear.Publishers.Single(x => x.PublisherID == publisherID);
        return publisher.Games.First(x => !x.CounterPick && x.MasterGame is not null);
    }

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
