using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.IntegrationTests.Helpers;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests.PostDraft;

/// <summary>
/// Trades between two players of a three-player league: proposed, then accepted, voted on by the manager and
/// executed; or rejected.
/// </summary>
public sealed class TradeTests : PostDraftTestBase
{
    private sealed record Trade(TestPublisher Proposer, TestPublisher CounterParty, string Offered, string Requested);

    private static async Task<Trade> PlanTradeAsync(LeagueFixture league)
    {
        var proposer = league.Publishers[1];
        var counterParty = league.Publishers[2];
        var offered = (await PublisherAsync(league, proposer)).Games.First(game => !game.CounterPick).GameName;
        var requested = (await PublisherAsync(league, counterParty)).Games.First(game => !game.CounterPick).GameName;
        return new Trade(proposer, counterParty, offered, requested);
    }

    private async Task<IPage> ProposeAsync(LeagueFixture league, Trade trade)
    {
        var page = await NewSignedInPageAsync(await UserIDOfAsync(trade.Proposer), LeaguePath(league));
        await Ui.SidebarAction(page, "Propose a Trade").ClickAsync();
        var dialog = Ui.Dialog(page, "Propose Trade");
        var selects = dialog.GetByRole(AriaRole.Combobox);
        await selects.First.SelectOptionAsync(new SelectOptionValue { Label = trade.CounterParty.PublisherName });

        await dialog.GetByRole(AriaRole.Button, new() { Name = "Add Game" }).Nth(0).ClickAsync();
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Add Game" }).Nth(1).ClickAsync();
        await Expect(selects).ToHaveCountAsync(3);
        await Ui.SelectOptionContainingAsync(selects.Nth(1), trade.Offered);
        await Ui.SelectOptionContainingAsync(selects.Nth(2), trade.Requested);
        await dialog.Locator("textarea").FillAsync("A fair swap.");

        await dialog.GetByRole(AriaRole.Button, new() { Name = "Propose Trade" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();
        await Expect(page.GetByText("You proposed a trade.")).ToBeVisibleAsync();
        return page;
    }

    private static async Task<ILocator> OpenActiveTradesAsync(IPage page, string alertText)
    {
        await page.GetByRole(AriaRole.Alert).Filter(new() { HasText = alertText }).GetByRole(AriaRole.Button, new() { Name = "View Trades" }).ClickAsync();
        var dialog = Ui.Dialog(page, "Active Trades");
        await Expect(dialog).ToBeVisibleAsync();
        return dialog;
    }

    [Test]
    public async Task AcceptVoteAndExecute()
    {
        var (league, managerUserID) = await NewDraftedLeagueAsync(players: 3);
        var trade = await PlanTradeAsync(league);
        await ProposeAsync(league, trade);

        var counterPartyPage = await NewSignedInPageAsync(await UserIDOfAsync(trade.CounterParty), LeaguePath(league));
        var counterPartyTrades = await OpenActiveTradesAsync(counterPartyPage, "Someone has proposed a trade with you.");
        await Expect(counterPartyTrades).ToContainTextAsync("This trade is waiting for your approval.");
        await Expect(counterPartyTrades).ToContainTextAsync(trade.Offered);
        await counterPartyTrades.GetByRole(AriaRole.Button, new() { Name = "Accept" }).ClickAsync();
        await Expect(counterPartyPage.GetByText("You accepted a trade.")).ToBeVisibleAsync();
        await Expect(counterPartyTrades).ToContainTextAsync("You accepted this trade, but you are free to change your mind.");

        var managerPage = await NewSignedInPageAsync(managerUserID, LeaguePath(league));
        var managerTrades = await OpenActiveTradesAsync(managerPage, "There are active trades under consideration.");
        await Expect(managerTrades).ToContainTextAsync("There are no votes yet.");
        await managerTrades.GetByPlaceholder("Enter a comment (Optional)").FillAsync("Looks fine to me.");
        await managerTrades.GetByRole(AriaRole.Button, new() { Name = "Vote Approve" }).ClickAsync();
        await Expect(managerPage.GetByText("Voted on trade.")).ToBeVisibleAsync();
        await Expect(managerTrades.GetByRole(AriaRole.Heading, new() { Name = "Votes" })).ToBeVisibleAsync();
        await Expect(managerTrades).ToContainTextAsync("Looks fine to me.");

        await managerTrades.GetByRole(AriaRole.Button, new() { Name = "Execute Trade" }).ClickAsync();
        await Expect(managerPage.GetByText("Trade has been executed.")).ToBeVisibleAsync();

        var proposer = await PublisherAsync(league, trade.Proposer);
        var counterParty = await PublisherAsync(league, trade.CounterParty);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(proposer.Games.Select(game => game.GameName), Does.Contain(trade.Requested).And.Not.Contain(trade.Offered));
            Assert.That(counterParty.Games.Select(game => game.GameName), Does.Contain(trade.Offered).And.Not.Contain(trade.Requested));
        }
    }

    [Test]
    public async Task CounterPartyRejects()
    {
        var (league, _) = await NewDraftedLeagueAsync(players: 3);
        var trade = await PlanTradeAsync(league);
        await ProposeAsync(league, trade);

        var page = await NewSignedInPageAsync(await UserIDOfAsync(trade.CounterParty), LeaguePath(league));
        var trades = await OpenActiveTradesAsync(page, "Someone has proposed a trade with you.");
        await trades.GetByRole(AriaRole.Button, new() { Name = "Reject", Exact = true }).ClickAsync();
        await Expect(page.GetByText("You rejected a trade.")).ToBeVisibleAsync();
        await Expect(trades).ToContainTextAsync("There are no active trades for this league.");
    }
}
