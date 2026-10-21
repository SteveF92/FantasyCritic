using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;
using static FantasyCritic.IntegrationTests.Helpers.SlotTestHelpers;

namespace FantasyCritic.IntegrationTests.Tests.League.Slots;

/// <summary>
/// Where traded games land once the manager executes a trade. Each side's game takes an open slot it fits on the
/// other side, the slots the outgoing games leave included, and a trade that can't be placed is refused.
/// </summary>
[TestFixture]
public class TradeSlotTests : IntegrationTestBase
{
    // Slots 0-1 regular, slot 2 requires New Game; one counter-pick slot.
    private static readonly LeagueScenario OneSpecialSlot = Scenario("TradeSlots", 3, 1, [[NewGame]]);

    [Test]
    public async Task TradingRegularGames_EachTakesTheSlotTheOtherLeft()
    {
        await using var league = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(Factory, OneSpecialSlot, NewUser);
        var games = await GetTaggedGamesAsync(league, Remake, Remake, Remake, NewGame);
        var proposer = league.Publishers[0];
        var counterParty = league.Publishers[1];
        // Proposer: Remake A in slot 0, Remake B in slot 1. Counter party: Remake C in slot 0, New Game in slot 2.
        var proposerSends = await ClaimAsync(league, proposer, games[0]);
        var proposerKeeps = await ClaimAsync(league, proposer, games[1]);
        var counterPartySends = await ClaimAsync(league, counterParty, games[2]);
        var counterPartyKeeps = await ClaimAsync(league, counterParty, games[3]);

        await ExecuteTradeAsync(league, proposer, counterParty, proposerSends, counterPartySends);

        var after = await league.GetLeagueYearAsync();
        var received = GetReceivedGames(after, proposer, counterParty, proposerSends, counterPartySends);
        var placements = GetPlacements(after);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(DescribeSlot(received.ByProposer), Is.EqualTo("0"), "The proposer's new game should take the slot its old one left.");
            Assert.That(DescribeSlot(received.ByCounterParty), Is.EqualTo("0"));
            Assert.That(placements[proposerKeeps.PublisherGameID], Is.EqualTo("1"));
            Assert.That(placements[counterPartyKeeps.PublisherGameID], Is.EqualTo("2 (special 0: NewGame)"));
        }

        AssertSlotsAreConsistent(after);
    }

    [Test]
    public async Task TradingAwayTheSpecialSlotGame_PutsTheIncomingGameInASlotItFits()
    {
        await using var league = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(Factory, OneSpecialSlot, NewUser);
        var games = await GetTaggedGamesAsync(league, NewGame, Remake);
        var proposer = league.Publishers[0];
        var counterParty = league.Publishers[1];
        // Proposer: New Game in the special slot (2), slots 0-1 open. Counter party: Remake in slot 0.
        var proposerSends = await ClaimAsync(league, proposer, games[0]);
        var counterPartySends = await ClaimAsync(league, counterParty, games[1]);

        await ExecuteTradeAsync(league, proposer, counterParty, proposerSends, counterPartySends);

        var after = await league.GetLeagueYearAsync();
        var received = GetReceivedGames(after, proposer, counterParty, proposerSends, counterPartySends);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(DescribeSlot(received.ByProposer), Is.EqualTo("0"),
                "A Remake doesn't fit the special slot the New Game left, so it takes the first regular slot.");
            Assert.That(DescribeSlot(received.ByCounterParty), Is.EqualTo("2 (special 0: NewGame)"),
                "A New Game takes the open New Game slot before a regular one.");
        }

        AssertSlotsAreConsistent(after);
    }

    [Test]
    public async Task TradingACounterPick_PutsItInACounterPickSlot()
    {
        await using var league = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(Factory, OneSpecialSlot, NewUser);
        var games = await GetTaggedGamesAsync(league, Remake, Remake, Remake);
        var proposer = league.Publishers[0];
        var counterParty = league.Publishers[1];
        // A third publisher owns the counter-picked game, so the counter pick is valid on either side of the trade.
        await ClaimAsync(league, league.Publishers[2], games[0]);
        var proposerSends = await ClaimAsync(league, proposer, games[0], counterPick: true);
        await ClaimAsync(league, proposer, games[1]);
        var counterPartySends = await ClaimAsync(league, counterParty, games[2]);

        await ExecuteTradeAsync(league, proposer, counterParty, proposerSends, counterPartySends);

        var after = await league.GetLeagueYearAsync();
        var received = GetReceivedGames(after, proposer, counterParty, proposerSends, counterPartySends);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(DescribeSlot(received.ByCounterParty), Is.EqualTo("CP 0"));
            Assert.That(DescribeSlot(received.ByProposer), Is.EqualTo("1"),
                "Sending a counter pick frees no standard slot, so the incoming game takes the proposer's open regular slot.");
        }

        AssertSlotsAreConsistent(after);
    }

    private sealed record ReceivedGames(PublisherSlotViewModel ByProposer, PublisherSlotViewModel ByCounterParty);

    private static ReceivedGames GetReceivedGames(LeagueYearViewModel leagueYear, TestPublisher proposer, TestPublisher counterParty,
        PublisherGameViewModel proposerSent, PublisherGameViewModel counterPartySent)
    {
        PublisherSlotViewModel SlotHolding(TestPublisher publisher, PublisherGameViewModel game) =>
            GetPublisher(leagueYear, publisher.PublisherID).GameSlots.Single(s =>
                s.PublisherGame?.MasterGame?.MasterGameID == game.MasterGame!.MasterGameID && s.CounterPick == game.CounterPick);

        return new ReceivedGames(SlotHolding(proposer, counterPartySent), SlotHolding(counterParty, proposerSent));
    }

    private static async Task ExecuteTradeAsync(LeagueFixture league, TestPublisher proposer, TestPublisher counterParty,
        PublisherGameViewModel proposerSends, PublisherGameViewModel counterPartySends)
    {
        const string message = "slot trade";
        await proposer.Session.League.ProposeTradeAsync(new ProposeTradeRequest
        {
            ProposerPublisherID = proposer.PublisherID,
            CounterPartyPublisherID = counterParty.PublisherID,
            ProposerPublisherGameIDs = [proposerSends.PublisherGameID],
            CounterPartyPublisherGameIDs = [counterPartySends.PublisherGameID],
            ProposerBudgetSendAmount = 0,
            CounterPartyBudgetSendAmount = 0,
            Message = message,
        });

        var leagueYear = await league.GetLeagueYearAsync();
        var tradeRequest = new BasicTradeRequest
        {
            LeagueID = league.LeagueID,
            Year = league.Year,
            TradeID = leagueYear.ActiveTrades.Single(t => t.Message == message).TradeID,
        };
        await counterParty.Session.League.AcceptTradeAsync(tradeRequest);
        await league.Manager.LeagueManager.ExecuteTradeAsync(tradeRequest);
    }
}
