using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.IntegrationTests.Helpers;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests.PostDraft;

/// <summary>
/// Bidding after the draft: placing a bid, then reordering, editing and cancelling bids, and a counter pick bid.
/// </summary>
public sealed class BidTests : PostDraftTestBase
{
    [Test]
    public async Task PlaceABid()
    {
        var (league, _) = await NewDraftedLeagueAsync();
        var player = league.Publishers[1];
        var game = (await AvailableGamesAsync(league, player, 1)).Single();
        var page = await NewSignedInPageAsync(await UserIDOfAsync(player), LeaguePath(league));

        await Ui.SidebarAction(page, "Make a Bid").ClickAsync();
        var dialog = Ui.Dialog(page, "Make a Bid");
        await Expect(dialog.GetByRole(AriaRole.Heading, new() { Name = "Top Available Games" })).ToBeVisibleAsync();
        await SelectGameRowAsync(dialog, game.GameName);
        await dialog.Locator("#bidAmount").FillAsync("5");
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Place Bid" }).ClickAsync();

        // The form stays open after a bid, ready for another.
        await Expect(page.GetByText($"Bid for {game.GameName} for $5 was made.")).ToBeVisibleAsync();
        await Expect(dialog.GetByRole(AriaRole.Heading, new() { Name = "Selected Game:" })).ToBeHiddenAsync();
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Close" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();

        await Ui.SidebarAction(page, "My Current Bids").ClickAsync();
        var bids = Ui.Dialog(page, "My Current Bids");
        await Expect(bids.GetByRole(AriaRole.Row).Filter(new() { HasText = game.GameName })).ToContainTextAsync("$5");
    }

    [Test]
    public async Task ReorderEditAndCancelBids()
    {
        var (league, _) = await NewDraftedLeagueAsync();
        var player = league.Publishers[1];
        var games = await AvailableGamesAsync(league, player, 2);
        await LeaguePickupActions.PlaceBidAsync(player, games[0].MasterGameID, 3, counterPick: false);
        await LeaguePickupActions.PlaceBidAsync(player, games[1].MasterGameID, 4, counterPick: false);
        var page = await NewSignedInPageAsync(await UserIDOfAsync(player), LeaguePath(league));

        await Ui.SidebarAction(page, "My Current Bids").ClickAsync();
        var dialog = Ui.Dialog(page, "My Current Bids");
        var rows = dialog.Locator("tbody tr");
        await Expect(rows).ToHaveCountAsync(2);
        await Expect(rows.First).ToContainTextAsync(games[0].GameName);

        // Change priorities by dragging the second bid above the first.
        await dialog.GetByText("Change Priorities", new() { Exact = true }).ClickAsync();
        await Expect(dialog.GetByText("Drag and drop to change order.")).ToBeVisibleAsync();
        await Ui.DragAsync(page, rows.Nth(1).Locator("svg").First, rows.First);
        await Expect(rows.First).ToContainTextAsync(games[1].GameName);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Set Priority Order" }).ClickAsync();
        await Expect(page.GetByText("Bid priority has been changed.")).ToBeVisibleAsync();

        await page.ReloadAsync();
        await Ui.SidebarAction(page, "My Current Bids").ClickAsync();
        await Expect(rows.First).ToContainTextAsync(games[1].GameName);

        // Edit the first bid's amount.
        await rows.First.GetByRole(AriaRole.Button, new() { Name = "Edit" }).ClickAsync();
        await Expect(dialog.GetByRole(AriaRole.Heading, new() { Name = "Edit Bid:" })).ToBeVisibleAsync();
        await dialog.Locator("#bidAmount").FillAsync("7");
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Edit Bid" }).ClickAsync();
        await Expect(page.GetByText($"Bid for {games[1].GameName} for $7 was made.")).ToBeVisibleAsync();

        // Cancel the other through the confirm dialog. The modal stays open after an edit.
        await Expect(rows.Filter(new() { HasText = games[1].GameName })).ToContainTextAsync("$7");
        await rows.Filter(new() { HasText = games[0].GameName }).GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();
        await ConfirmAsync(page, $"Are you sure you want to cancel your bid for {games[0].GameName}?");
        await Expect(page.GetByText($"Bid for {games[0].GameName} for $3 was canceled.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task PlaceACounterPickBid()
    {
        var (league, _) = await NewDraftedLeagueAsync();
        var player = league.Publishers[1];
        var page = await NewSignedInPageAsync(await UserIDOfAsync(player), LeaguePath(league));

        await Ui.SidebarAction(page, "Make a Counter Pick Bid").ClickAsync();
        var dialog = Ui.Dialog(page, "Make a Counter Pick Bid");
        var target = dialog.GetByRole(AriaRole.Combobox);
        await Expect(target.Locator("option").First).ToBeAttachedAsync();
        var gameName = (await target.Locator("option").First.TextContentAsync())!.Trim();
        await target.SelectOptionAsync(new SelectOptionValue { Index = 0 });
        await dialog.Locator("#bidAmount").FillAsync("2");
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Place Counter Pick Bid" }).ClickAsync();

        await Expect(dialog).ToBeHiddenAsync();
        await Expect(page.GetByText($"Bid for {gameName} for $2 was made.")).ToBeVisibleAsync();
    }
}
