using System.Linq;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests.PostDraft;

/// <summary>
/// Drop requests and the watchlist: request a drop and cancel it; add games to the watchlist, reorder them by dragging,
/// write a note and remove one.
/// </summary>
public sealed class DropAndWatchlistTests : PostDraftTestBase
{
    [Test]
    public async Task RequestThenCancelADrop()
    {
        var (league, _) = await NewDraftedLeagueAsync();
        var player = league.Publishers[1];
        var gameName = (await PublisherAsync(league, player)).Games.First(game => !game.CounterPick).GameName;
        var page = await NewSignedInPageAsync(await UserIDOfAsync(player), LeaguePath(league));

        await Ui.SidebarAction(page, "Drop a Game").ClickAsync();
        var dialog = Ui.Dialog(page, "Drop a Game");
        await Expect(dialog.GetByText("Drops Remaining:")).ToBeVisibleAsync();
        await dialog.GetByRole(AriaRole.Combobox).SelectOptionAsync(new SelectOptionValue { Label = gameName });
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Make Drop Game Request" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();
        await Expect(page.GetByText($"Drop Request for {gameName} was made.")).ToBeVisibleAsync();

        await Ui.SidebarAction(page, "My Pending Drops").ClickAsync();
        var drops = Ui.Dialog(page, "My Pending Drop Requests");
        var row = drops.Locator("tbody tr").Filter(new() { HasText = gameName });
        await Expect(row).ToBeVisibleAsync();
        await row.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();
        await ConfirmAsync(page, $"Are you sure you want to cancel your drop for {gameName}?");
        await Expect(page.GetByText($"Drop Request for {gameName} was cancelled.")).ToBeVisibleAsync();
        await Expect(drops.Locator("tbody tr")).ToHaveCountAsync(0);
    }

    [Test]
    public async Task WatchlistAddReorderNoteAndRemove()
    {
        var (league, _) = await NewDraftedLeagueAsync();
        var player = league.Publishers[1];
        var games = await AvailableGamesAsync(league, player, 2);
        var page = await NewSignedInPageAsync(await UserIDOfAsync(player), LeaguePath(league));

        await Ui.SidebarAction(page, "Watchlist").ClickAsync();
        var dialog = Ui.Dialog(page, "My Watchlist");
        var watchlist = dialog.GetByRole(AriaRole.Table).Filter(new() { Has = page.GetByRole(AriaRole.Columnheader, new() { Name = "Notes" }) });
        var rows = watchlist.Locator("tbody tr");

        foreach (var game in games)
        {
            await Expect(dialog.GetByRole(AriaRole.Heading, new() { Name = "Top Available Games" })).ToBeVisibleAsync();
            await dialog.GetByRole(AriaRole.Row).Filter(new() { Has = page.GetByRole(AriaRole.Cell, new() { Name = game.GameName, Exact = true }) })
                .First.GetByRole(AriaRole.Button, new() { Name = "Select" }).ClickAsync();
            await Expect(watchlist).ToContainTextAsync(game.GameName);
        }

        await Expect(rows).ToHaveCountAsync(2);
        await Expect(rows.First).ToContainTextAsync(games[0].GameName);

        // Drag the second game to the top and save the rankings.
        await Ui.DragAsync(page, rows.Nth(1).Locator("svg").First, rows.First);
        await Expect(rows.First).ToContainTextAsync(games[1].GameName);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Set Rankings" }).ClickAsync();
        await Expect(page.GetByText("Watchlist reordered.")).ToBeVisibleAsync();

        await page.ReloadAsync();
        await Ui.SidebarAction(page, "Watchlist").ClickAsync();
        await Expect(rows).ToHaveCountAsync(2);
        await Expect(rows.First).ToContainTextAsync(games[1].GameName);

        // Notes open in a modal of their own.
        await rows.First.GetByRole(AriaRole.Button, new() { Name = "Edit Notes" }).ClickAsync();
        var notes = page.GetByRole(AriaRole.Dialog).Filter(new() { Has = page.GetByRole(AriaRole.Button, new() { Name = "Save Notes" }) }).Last;
        await notes.GetByRole(AriaRole.Textbox).FillAsync("Buy on day one.");
        await notes.GetByRole(AriaRole.Button, new() { Name = "Save Notes" }).ClickAsync();
        await Expect(page.GetByText("Watchlist notes saved.")).ToBeVisibleAsync();
        await Expect(rows.First).ToContainTextAsync("Buy on day one.");

        await rows.Filter(new() { HasText = games[0].GameName }).GetByRole(AriaRole.Button, new() { Name = "Remove" }).ClickAsync();
        await Expect(page.GetByText("Game removed from watchlist.")).ToBeVisibleAsync();
        await Expect(rows).ToHaveCountAsync(1);
        await Expect(rows.First).ToContainTextAsync(games[1].GameName);
    }
}
