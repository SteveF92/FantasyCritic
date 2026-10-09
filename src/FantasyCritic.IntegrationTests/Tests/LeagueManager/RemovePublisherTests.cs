using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.LeagueManager;

/// <summary>
/// The league manager removing a publisher that has no player: once before the draft, and once in a multi-draft
/// league after the publisher was skipped through a later draft, so it has pick skips that have to go with it.
/// </summary>
[TestFixture]
public class RemovePublisherTests : IntegrationTestBase
{
    private static readonly LeagueScenario MultiDraftScenario = new()
    {
        Name = "RemovePublisherMultiDraft",
        PlayerCount = 4,
        StandardGames = 3,
        GamesToDraft = 2,
        CounterPicks = 0,
        CounterPicksToDraft = 0,
        PickupSystem = "SemiPublicBiddingSecretCounterPicks",
        ScoringSystem = "LinearPositive",
        TradingSystem = "NoTrades",
        TiebreakSystem = "LowestProjectedPoints",
        ReleaseSystem = "MustBeReleased",
        IneligibleGameSystem = "DroppableAsWillNotRelease",
        UnrestrictedReleaseStatusDroppableGames = 0,
        WillNotReleaseDroppableGames = 0,
        WillReleaseDroppableGames = 0,
        DropOnlyDraftGames = true,
        GrantSuperDrops = false,
        CounterPicksBlockDrops = true,
        AllowMoveIntoIneligible = false,
        MinimumBidAmount = 0,
        EnableBids = true,
    };

    private ApiSession _adminSession = null!;
    private LeagueFixture _preDraftLeague = null!;
    private LeagueFixture _multiDraftLeague = null!;

    private Guid _preDraftRemovedPublisherID;
    private LeagueYearViewModel _preDraftAfterRemove = null!;

    private Guid _multiDraftRemovedPublisherID;
    private int? _removeConnectedPublisherStatusCode;
    private int? _removeByNonManagerStatusCode;
    private LeagueYearViewModel _multiDraftBeforeRemove = null!;
    private IReadOnlyList<LeagueActionViewModel> _multiDraftActionsBeforeRemove = null!;
    private string _multiDraftRemovedPublisherName = null!;
    private LeagueYearViewModel _multiDraftAfterRemove = null!;
    private IReadOnlyList<LeagueActionViewModel> _multiDraftActionsAfterRemove = null!;
    private PickupBidResultViewModel _bidOnRemovedPublishersGame = null!;

    [OneTimeSetUp]
    public async Task SetUpRemovedPublishers()
    {
        _adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(_adminSession);

        await _adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 6, 12, 0, 0, TimeSpan.Zero)
        });

        await SetUpPreDraftLeagueAsync();
        await SetUpMultiDraftLeagueAsync();
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        if (_adminSession != null)
        {
            await _adminSession.Admin.ResetTimeAsync();
        }

        _adminSession?.Dispose();

        if (_preDraftLeague != null)
        {
            await _preDraftLeague.DisposeAsync();
        }

        if (_multiDraftLeague != null)
        {
            await _multiDraftLeague.DisposeAsync();
        }
    }

    [Test]
    public void PreDraftRemove_RemovesThePublisherAndRenumbersTheDraft()
    {
        var expectedOrder = _preDraftLeague.Publishers.Select(x => x.PublisherID).Where(x => x != _preDraftRemovedPublisherID).ToList();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_preDraftAfterRemove.Publishers.Select(x => x.PublisherID), Does.Not.Contain(_preDraftRemovedPublisherID));
            AssertDraftOrder(_preDraftAfterRemove, draftNumber: 1, expectedOrder);
        }
    }

    [Test]
    public void Remove_IsRejected_ForAPublisherWithAPlayer_OrANonManager()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_removeConnectedPublisherStatusCode, Is.EqualTo(400));
            Assert.That(_removeByNonManagerStatusCode, Is.EqualTo(403));
        }
    }

    [Test]
    public void MultiDraftRemove_RemovedPublisherWasSkippedInTheSecondDraft()
    {
        Assert.That(_multiDraftActionsBeforeRemove.Any(x => x.ActionType == "Draft Pick Skipped" && x.PublisherName == _multiDraftRemovedPublisherName), Is.True,
            "The publisher with no player should have been auto-skipped in draft 2, leaving pick skips for the removal to delete.");
    }

    [Test]
    public void MultiDraftRemove_RenumbersEveryDraft()
    {
        var expectedOrder = _multiDraftLeague.Publishers.Select(x => x.PublisherID).Where(x => x != _multiDraftRemovedPublisherID).ToList();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_multiDraftAfterRemove.Publishers.Select(x => x.PublisherID), Does.Not.Contain(_multiDraftRemovedPublisherID));
            AssertDraftOrder(_multiDraftAfterRemove, draftNumber: 1, expectedOrder);
            AssertDraftOrder(_multiDraftAfterRemove, draftNumber: 2, expectedOrder);
        }
    }

    [Test]
    public void MultiDraftRemove_LeavesTheOtherPublishersRostersAlone()
    {
        foreach (var publisher in _multiDraftAfterRemove.Publishers)
        {
            var before = _multiDraftBeforeRemove.Publishers.Single(x => x.PublisherID == publisher.PublisherID);
            Assert.That(publisher.Games.Select(x => x.PublisherGameID), Is.EquivalentTo(before.Games.Select(x => x.PublisherGameID)));
        }
    }

    [Test]
    public void MultiDraftRemove_RecordsAManagerAction()
    {
        var action = _multiDraftActionsAfterRemove.SingleOrDefault(x => x.ActionType == "Publisher Removed");
        Assert.That(action, Is.Not.Null);
        Assert.That(action!.ManagerAction, Is.True);
    }

    [Test]
    public void MultiDraftRemove_MakesTheRemovedPublishersGamesAvailable()
    {
        Assert.That(_bidOnRemovedPublishersGame.Success, Is.True,
            $"Could not bid on a game the removed publisher had: {string.Join("; ", _bidOnRemovedPublishersGame.Errors ?? [])}");
    }

    private async Task SetUpPreDraftLeagueAsync()
    {
        _preDraftLeague = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(Factory, LeagueScenarios.Standard, NewUser);
        _preDraftRemovedPublisherID = _preDraftLeague.Publishers[1].PublisherID;

        await _preDraftLeague.Manager.LeagueManager.DisconnectPlayerAsync(new DisconnectPlayerRequest { PublisherID = _preDraftRemovedPublisherID });
        await _preDraftLeague.Manager.LeagueManager.RemovePublisherAsync(new RemovePublisherRequest { PublisherID = _preDraftRemovedPublisherID });
        _preDraftAfterRemove = await _preDraftLeague.GetLeagueYearAsync();
    }

    private async Task SetUpMultiDraftLeagueAsync()
    {
        _multiDraftLeague = await LeagueFixtureBuilder.CreateAndStartDraftAsync(Factory, MultiDraftScenario, NewUser);
        await _multiDraftLeague.DraftToCompletionAsync();

        var manager = _multiDraftLeague.Manager;
        var removedPublisher = _multiDraftLeague.Publishers[1];
        var bidder = _multiDraftLeague.Publishers[2];
        _multiDraftRemovedPublisherID = removedPublisher.PublisherID;

        await manager.LeagueManager.DisconnectPlayerAsync(new DisconnectPlayerRequest { PublisherID = removedPublisher.PublisherID });

        await manager.LeagueManager.CreateLeagueDraftAsync(new CreateLeagueDraftRequest
        {
            LeagueID = _multiDraftLeague.LeagueID,
            Year = _multiDraftLeague.Year,
            Name = "Draft 2",
            ScheduledDate = null,
            GamesToDraft = 1,
            CounterPicksToDraft = 0,
            AdditionalStandardGames = 1,
            AdditionalCounterPicks = 0,
            NewSpecialGameSlots = [],
        });
        await LeagueTestHelpers.SetDraftOrderAsync(manager, _multiDraftLeague.LeagueID, _multiDraftLeague.Year,
            _multiDraftLeague.Publishers.Select(x => x.PublisherID).ToList());
        await manager.LeagueManager.StartDraftAsync(new StartDraftRequest
        {
            LeagueID = _multiDraftLeague.LeagueID,
            Year = _multiDraftLeague.Year,
        });

        var connectedPublishers = _multiDraftLeague.Publishers
            .Where(x => x.PublisherID != removedPublisher.PublisherID)
            .ToDictionary(x => x.PublisherID, x => x.Session);
        await _multiDraftLeague.DraftToCompletionAsync(connectedPublishers);

        _multiDraftBeforeRemove = await _multiDraftLeague.GetLeagueYearAsync();
        _multiDraftActionsBeforeRemove = (await manager.League.GetLeagueActionsAsync(_multiDraftLeague.LeagueID, _multiDraftLeague.Year)).ToList();
        _multiDraftRemovedPublisherName = removedPublisher.PublisherName;
        var removedPublishersGame = _multiDraftBeforeRemove.Publishers
            .Single(x => x.PublisherID == removedPublisher.PublisherID)
            .Games.First(x => !x.CounterPick && x.MasterGame is not null)
            .MasterGame!.MasterGameID;

        _removeConnectedPublisherStatusCode = await CaptureApiStatusCodeAsync(() =>
            manager.LeagueManager.RemovePublisherAsync(new RemovePublisherRequest { PublisherID = bidder.PublisherID }));
        _removeByNonManagerStatusCode = await CaptureApiStatusCodeAsync(() =>
            bidder.Session.LeagueManager.RemovePublisherAsync(new RemovePublisherRequest { PublisherID = removedPublisher.PublisherID }));

        await manager.LeagueManager.RemovePublisherAsync(new RemovePublisherRequest { PublisherID = removedPublisher.PublisherID });

        _multiDraftAfterRemove = await _multiDraftLeague.GetLeagueYearAsync();
        _multiDraftActionsAfterRemove = (await manager.League.GetLeagueActionsAsync(_multiDraftLeague.LeagueID, _multiDraftLeague.Year)).ToList();
        _bidOnRemovedPublishersGame = await LeaguePickupActions.TryPlaceBidAsync(bidder, removedPublishersGame, 5, false);
    }

    private static void AssertDraftOrder(LeagueYearViewModel leagueYear, int draftNumber, IReadOnlyList<Guid> expectedPublisherOrder)
    {
        var draft = leagueYear.Drafts.Single(x => x.DraftNumber == draftNumber);
        var actualOrder = draft.PublisherDraftInfo
            .OrderBy(x => x.DraftPosition)
            .Select(x => x.PublisherID)
            .ToList();
        var positions = draft.PublisherDraftInfo.Select(x => x.DraftPosition).OrderBy(x => x).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(actualOrder, Is.EqualTo(expectedPublisherOrder), $"Draft {draftNumber} should keep the remaining publishers in order.");
            Assert.That(positions, Is.EqualTo(Enumerable.Range(1, expectedPublisherOrder.Count)), $"Draft {draftNumber} positions should be 1 to N.");
        }
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
