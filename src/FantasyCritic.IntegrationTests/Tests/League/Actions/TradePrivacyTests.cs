using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Actions;

[TestFixture]
public class TradePrivacyTests : IntegrationTestBase
{
    private LeagueFixture _privateLeague = null!;
    private LeagueFixture _standardLeague = null!;

    private Guid _privateAcceptedTradeID;
    private Guid _privateRescindedTradeID;
    private Guid _standardProposedTradeID;

    private IReadOnlyList<Guid> _privateProposerProposalActiveTrades = null!;
    private IReadOnlyList<Guid> _privateCounterPartyProposalActiveTrades = null!;
    private IReadOnlyList<Guid> _privateNonInvolvedProposalActiveTrades = null!;
    private IReadOnlyList<Guid> _privateManagerProposalActiveTrades = null!;
    private IReadOnlyList<Guid> _privateNonInvolvedAcceptedActiveTrades = null!;
    private IReadOnlyList<Guid> _privateManagerAcceptedActiveTrades = null!;
    private IReadOnlyList<Guid> _privateHistoryTradeIDs = null!;
    private IReadOnlyList<Guid> _privateExportTradeIDs = null!;
    private IReadOnlyList<Guid> _standardNonInvolvedProposalActiveTrades = null!;
    private IReadOnlyList<Guid> _standardManagerProposalActiveTrades = null!;

    private int? _privateProposalVoteStatusCode;
    private int? _privateProposalManagerRejectStatusCode;

    [OneTimeSetUp]
    public async Task SetUpTradePrivacyLeagues()
    {
        _privateLeague = await BuildDraftedLeagueAsync("PrivateUntilAccepted");
        await SetUpPrivateUntilAcceptedTradesAsync();

        _standardLeague = await BuildDraftedLeagueAsync("Standard");
        await SetUpStandardTradeAsync();
    }

    [OneTimeTearDown]
    public async Task TearDownTradePrivacyLeagues()
    {
        if (_privateLeague != null)
        {
            await _privateLeague.DisposeAsync();
        }

        if (_standardLeague != null)
        {
            await _standardLeague.DisposeAsync();
        }
    }

    [Test]
    public void PrivateProposal_IsVisibleOnlyToProposerAndCounterparty()
    {
        Assert.That(_privateProposerProposalActiveTrades, Does.Contain(_privateAcceptedTradeID));
        Assert.That(_privateCounterPartyProposalActiveTrades, Does.Contain(_privateAcceptedTradeID));
        Assert.That(_privateNonInvolvedProposalActiveTrades, Does.Not.Contain(_privateAcceptedTradeID));
        Assert.That(_privateManagerProposalActiveTrades, Does.Not.Contain(_privateAcceptedTradeID));
    }

    [Test]
    public void PrivateProposal_BlocksNonInvolvedDirectVoteAndManagerReject()
    {
        Assert.That(_privateProposalVoteStatusCode, Is.EqualTo(403));
        Assert.That(_privateProposalManagerRejectStatusCode, Is.EqualTo(403));
    }

    [Test]
    public void PrivateAcceptedTrade_BecomesVisibleToLeagueAndManager()
    {
        Assert.That(_privateNonInvolvedAcceptedActiveTrades, Does.Contain(_privateAcceptedTradeID));
        Assert.That(_privateManagerAcceptedActiveTrades, Does.Contain(_privateAcceptedTradeID));
    }

    [Test]
    public void PrivateNeverAcceptedTrades_AreExcludedFromHistoryAndExport()
    {
        Assert.That(_privateHistoryTradeIDs, Does.Contain(_privateAcceptedTradeID));
        Assert.That(_privateHistoryTradeIDs, Does.Not.Contain(_privateRescindedTradeID));

        Assert.That(_privateExportTradeIDs, Does.Contain(_privateAcceptedTradeID));
        Assert.That(_privateExportTradeIDs, Does.Not.Contain(_privateRescindedTradeID));
    }

    [Test]
    public void StandardProposal_RemainsVisibleToNonInvolvedMembersAndManager()
    {
        Assert.That(_standardNonInvolvedProposalActiveTrades, Does.Contain(_standardProposedTradeID));
        Assert.That(_standardManagerProposalActiveTrades, Does.Contain(_standardProposedTradeID));
    }

    private async Task<LeagueFixture> BuildDraftedLeagueAsync(string tradingSystem)
    {
        var scenario = new LeagueScenario
        {
            Name = $"TradePrivacy-{tradingSystem}",
            PlayerCount = 4,
            StandardGames = 2,
            GamesToDraft = 2,
            CounterPicks = 0,
            CounterPicksToDraft = 0,
            DraftSystem = "Flexible",
            PickupSystem = "SemiPublicBiddingSecretCounterPicks",
            ScoringSystem = "LinearPositive",
            TradingSystem = tradingSystem,
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
        };

        var league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(Factory, scenario, NewUser);
        await league.DraftToCompletionAsync();
        return league;
    }

    private async Task SetUpPrivateUntilAcceptedTradesAsync()
    {
        var proposer = _privateLeague.Publishers[1];
        var counterParty = _privateLeague.Publishers[2];
        var nonInvolvedPlayer = _privateLeague.Publishers[3];
        var manager = _privateLeague.Manager;

        _privateAcceptedTradeID = await ProposeOneForOneTradeAsync(_privateLeague, proposer, counterParty, "private accepted trade");

        _privateProposerProposalActiveTrades = await GetActiveTradeIDsAsync(proposer.Session, _privateLeague);
        _privateCounterPartyProposalActiveTrades = await GetActiveTradeIDsAsync(counterParty.Session, _privateLeague);
        _privateNonInvolvedProposalActiveTrades = await GetActiveTradeIDsAsync(nonInvolvedPlayer.Session, _privateLeague);
        _privateManagerProposalActiveTrades = await GetActiveTradeIDsAsync(manager, _privateLeague);

        var privateTradeRequest = BuildBasicTradeRequest(_privateLeague, _privateAcceptedTradeID);
        _privateProposalVoteStatusCode = await CaptureApiStatusCodeAsync(() => nonInvolvedPlayer.Session.League.VoteOnTradeAsync(
            new TradeVoteRequest
            {
                LeagueID = _privateLeague.LeagueID,
                Year = _privateLeague.Year,
                TradeID = _privateAcceptedTradeID,
                Approved = true,
                Comment = "not allowed yet",
            }));
        _privateProposalManagerRejectStatusCode = await CaptureApiStatusCodeAsync(() => manager.LeagueManager.RejectTradeAsync(privateTradeRequest));

        await counterParty.Session.League.AcceptTradeAsync(privateTradeRequest);

        _privateNonInvolvedAcceptedActiveTrades = await GetActiveTradeIDsAsync(nonInvolvedPlayer.Session, _privateLeague);
        _privateManagerAcceptedActiveTrades = await GetActiveTradeIDsAsync(manager, _privateLeague);

        await nonInvolvedPlayer.Session.League.VoteOnTradeAsync(new TradeVoteRequest
        {
            LeagueID = _privateLeague.LeagueID,
            Year = _privateLeague.Year,
            TradeID = _privateAcceptedTradeID,
            Approved = true,
            Comment = "visible after acceptance",
        });
        await manager.LeagueManager.RejectTradeAsync(privateTradeRequest);

        _privateRescindedTradeID = await ProposeOneForOneTradeAsync(_privateLeague, nonInvolvedPlayer, proposer, "private rescinded trade");
        await nonInvolvedPlayer.Session.League.RescindTradeAsync(BuildBasicTradeRequest(_privateLeague, _privateRescindedTradeID));

        _privateHistoryTradeIDs = await GetTradeHistoryIDsAsync(manager, _privateLeague);
        _privateExportTradeIDs = await GetConsolidatedExportTradeIDsAsync(manager, _privateLeague);
    }

    private async Task SetUpStandardTradeAsync()
    {
        var proposer = _standardLeague.Publishers[1];
        var counterParty = _standardLeague.Publishers[2];
        var nonInvolvedPlayer = _standardLeague.Publishers[3];

        _standardProposedTradeID = await ProposeOneForOneTradeAsync(_standardLeague, proposer, counterParty, "standard proposed trade");
        _standardNonInvolvedProposalActiveTrades = await GetActiveTradeIDsAsync(nonInvolvedPlayer.Session, _standardLeague);
        _standardManagerProposalActiveTrades = await GetActiveTradeIDsAsync(_standardLeague.Manager, _standardLeague);
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
            ProposerPublisherGameIDs = new List<Guid> { proposerGame.PublisherGameID },
            CounterPartyPublisherGameIDs = new List<Guid> { counterPartyGame.PublisherGameID },
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

    private static async Task<IReadOnlyList<Guid>> GetActiveTradeIDsAsync(ApiSession session, LeagueFixture league)
    {
        var leagueYear = await session.League.GetLeagueYearAsync(league.LeagueID, league.Year, null);
        return leagueYear.ActiveTrades.Select(x => x.TradeID).ToList();
    }

    private static async Task<IReadOnlyList<Guid>> GetTradeHistoryIDsAsync(ApiSession session, LeagueFixture league)
    {
        var history = await session.League.TradeHistoryAsync(league.LeagueID, league.Year);
        return history.Select(x => x.TradeID).ToList();
    }

    private static async Task<IReadOnlyList<Guid>> GetConsolidatedExportTradeIDsAsync(ApiSession session, LeagueFixture league)
    {
        var export = await session.League.DownloadConsolidatedLeagueYearDataAsync(league.LeagueID, league.Year);
        return export.Trades.Select(x => x.TradeID).ToList();
    }

    private static BasicTradeRequest BuildBasicTradeRequest(LeagueFixture league, Guid tradeID) =>
        new()
        {
            LeagueID = league.LeagueID,
            Year = league.Year,
            TradeID = tradeID,
        };

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
