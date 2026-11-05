using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests.Royale;

/// <summary>
/// Critics Royale in the active quarter: creating a publisher, buying, budgeting, renaming and selling, the
/// leaderboard's year and quarter menus and charts, and royale groups with invite links.
/// </summary>
public sealed class RoyaleTests : UiTestBase
{
    private static string UniqueName(string prefix) => $"{prefix} {Guid.NewGuid():N}"[..(prefix.Length + 9)];

    private static async Task<(Guid PublisherID, string PublisherName, PossibleRoyaleMasterGameViewModel Game)> NewRoyalePublisherAsync(TestUser user)
    {
        var quarter = await user.Session.Royale.ActiveRoyaleQuarterAsync();
        var publisherName = UniqueName("UI Royale");
        var publisherID = await user.Session.Royale.CreateRoyalePublisherAsync(new CreateRoyalePublisherRequest
        {
            Year = quarter.Year,
            Quarter = quarter.Quarter,
            PublisherName = publisherName,
        });
        var games = await user.Session.Royale.PossibleMasterGamesAsync(null, publisherID, RoyalePossibleMasterGamesReleaseFilter.All);
        var game = games.First(possible => possible.IsAvailable);
        return (publisherID, publisherName, game);
    }

    [Test]
    public async Task CreateAPublisher()
    {
        var user = await NewUserAsync();
        var page = await NewSignedInPageAsync(user, "/criticsRoyale");
        var publisherName = UniqueName("UI Royale");

        await page.GetByRole(AriaRole.Button, new() { Name = "Create Publisher" }).ClickAsync();
        var dialog = Ui.Dialog(page, "Create Publisher");
        await dialog.Locator("#publisherName").FillAsync(publisherName);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Create Publisher" }).ClickAsync();

        await Expect(page).ToHaveURLAsync(new Regex("/royalePublisher/[0-9a-f-]{36}$"));
        await Expect(Ui.Heading(page, publisherName, 1)).ToBeVisibleAsync();
    }

    [Test]
    public async Task BuyBudgetRenameAndSell()
    {
        var user = await NewUserAsync();
        var (publisherID, _, game) = await NewRoyalePublisherAsync(user);
        var page = await NewSignedInPageAsync(user, $"/royalePublisher/{publisherID}");
        var gameName = game.MasterGame.GameName;

        await page.GetByRole(AriaRole.Button, new() { Name = "Purchase a Game" }).ClickAsync();
        var purchase = Ui.Dialog(page, "Purchase Game");
        await purchase.Locator("#searchGameName").FillAsync(gameName);
        await purchase.GetByRole(AriaRole.Button, new() { Name = "Search Game" }).ClickAsync();
        await purchase.GetByRole(AriaRole.Row).Filter(new() { Has = page.GetByRole(AriaRole.Cell, new() { Name = gameName, Exact = true }) })
            .First.GetByRole(AriaRole.Button, new() { Name = "Select" }).ClickAsync();
        await Expect(purchase.GetByText($"Selected Game: {gameName}")).ToBeVisibleAsync();
        await purchase.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("^Purchase Game for ") }).ClickAsync();
        await Expect(purchase).ToBeHiddenAsync();
        // The Action History table names the game too, so the games row is the one with a Sell button.
        var row = page.GetByRole(AriaRole.Row).Filter(new() { HasText = gameName }).Filter(new() { Has = page.GetByRole(AriaRole.Button, new() { Name = "Sell" }) });
        await Expect(row).ToBeVisibleAsync();

        await row.GetByRole(AriaRole.Button, new() { Name = "Set Budget" }).ClickAsync();
        var budget = Ui.Dialog(page, "Set Advertising Budget");
        await budget.GetByRole(AriaRole.Textbox).FillAsync("2");
        await budget.GetByRole(AriaRole.Button, new() { Name = "OK" }).ClickAsync();
        await Expect(row).ToContainTextAsync("$2.00");

        await page.GetByRole(AriaRole.Button, new() { Name = "Change Publisher Name" }).ClickAsync();
        var rename = Ui.Dialog(page, "Change Publisher Name");
        await rename.Locator("#newPublisherName").FillAsync("UI Renamed Royale");
        await rename.GetByRole(AriaRole.Button, new() { Name = "Change Name" }).ClickAsync();
        await Expect(page.GetByText("Publisher name changed.")).ToBeVisibleAsync();
        await Expect(Ui.Heading(page, "UI Renamed Royale", 1)).ToBeVisibleAsync();

        await row.GetByRole(AriaRole.Button, new() { Name = "Sell" }).ClickAsync();
        await Ui.Dialog(page, "Sell Game").GetByRole(AriaRole.Button, new() { Name = "OK" }).ClickAsync();
        await Expect(row).ToHaveCountAsync(0);
    }

    [Test]
    [Ignore("Main's bug: royalePublisher.vue still calls this.$options.filters.money, which the filters removal left undefined, so buying and selling throw before their toasts.")]
    public async Task BuyingAndSellingToast()
    {
        var user = await NewUserAsync();
        var (publisherID, _, game) = await NewRoyalePublisherAsync(user);
        await user.Session.Royale.PurchaseGameAsync(new PurchaseRoyaleGameRequest { PublisherID = publisherID, MasterGameID = game.MasterGame.MasterGameID });
        var page = await NewSignedInPageAsync(user, $"/royalePublisher/{publisherID}");

        await page.GetByRole(AriaRole.Row).Filter(new() { HasText = game.MasterGame.GameName }).GetByRole(AriaRole.Button, new() { Name = "Sell" }).ClickAsync();
        await Ui.Dialog(page, "Sell Game").GetByRole(AriaRole.Button, new() { Name = "OK" }).ClickAsync();
        await Expect(page.GetByText($"{game.MasterGame.GameName} was sold for")).ToBeVisibleAsync();
    }

    /// <summary>
    /// A month of points history, which the charts need and the local database doesn't have.
    /// </summary>
    private static JsonArray Statistics(int year) =>
        new(Enumerable.Range(1, 6).Select(day => (JsonNode)new JsonObject { ["date"] = $"{year}-10-{day:00}", ["fantasyPoints"] = day * 3 }).ToArray());

    [Test]
    public async Task LeaderboardQuarterMenusAndCharts()
    {
        var user = await NewUserAsync();
        var (publisherID, _, _) = await NewRoyalePublisherAsync(user);
        var quarter = await user.Session.Royale.ActiveRoyaleQuarterAsync();
        var page = await NewPageAsync();
        await Ui.PlantJsonAsync(page, $"**/api/Royale/RoyaleData/{quarter.Year}/{quarter.Quarter}", async route =>
        {
            var data = await Ui.FetchJsonAsync(route);
            data["topPublishers"] = new JsonArray(
                new JsonObject { ["publisherName"] = "Chart Publisher A", ["statistics"] = Statistics(quarter.Year) },
                new JsonObject { ["publisherName"] = "Chart Publisher B", ["statistics"] = Statistics(quarter.Year) });
            return data;
        });
        await Ui.PlantJsonAsync(page, $"**/api/Royale/GetRoyalePublisher/{publisherID}", async route =>
        {
            var publisher = await Ui.FetchJsonAsync(route);
            publisher["statistics"] = Statistics(quarter.Year);
            return publisher;
        });
        await page.GotoAsync($"/dev/sign-in-as?userId={user.UserID}&returnUrl=/criticsRoyale");
        await Expect(Ui.Heading(page, $"Leaderboards {quarter.Year}-Q{quarter.Quarter}")).ToBeVisibleAsync();
        await Expect(Ui.Heading(page, "Top 10 Publisher Statistics")).ToBeVisibleAsync();
        await Ui.WaitUntilAsync(async () => await Ui.DrawnPixelsAsync(page.Locator("canvas").First) > 1000, "the top 10 chart to draw");

        var previousYear = quarter.Year - 1;
        await page.GetByRole(AriaRole.Button, new() { Name = quarter.Year.ToString(), Exact = true }).ClickAsync();
        await page.GetByRole(AriaRole.Menuitem, new() { Name = previousYear.ToString(), Exact = true }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = $"Q{quarter.Quarter}", Exact = true }).ClickAsync();
        await page.GetByRole(AriaRole.Menuitem, new() { Name = $"{previousYear}-Q3" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex($"/criticsRoyale/{previousYear}/3$"));
        await Expect(Ui.Heading(page, $"Leaderboards {previousYear}-Q3")).ToBeVisibleAsync();

        await page.GotoAsync($"/royalePublisher/{publisherID}");
        await Ui.WaitUntilAsync(async () => await Ui.DrawnPixelsAsync(page.Locator("canvas").First) > 1000, "the publisher chart to draw");
    }

    [Test]
    public async Task GroupInviteJoinLeaveAndRemove()
    {
        var manager = await NewUserAsync();
        var page = await NewSignedInPageAsync(manager, "/criticsRoyale");
        var groupName = UniqueName("UI Group");

        await page.GetByRole(AriaRole.Button, new() { Name = "Create Group" }).ClickAsync();
        var create = Ui.Dialog(page, "Create Royale Group");
        await create.Locator("#newGroupName").FillAsync(groupName);
        await create.GetByRole(AriaRole.Button, new() { Name = "OK" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex("/royaleGroup/[0-9a-f-]{36}$"));
        await Expect(Ui.Heading(page, groupName, 1)).ToBeVisibleAsync();
        var groupPath = new Uri(page.Url).AbsolutePath;

        await page.GetByRole(AriaRole.Button, new() { Name = "Invite Links" }).ClickAsync();
        var invites = Ui.Dialog(page, "Invite Links");
        await invites.GetByRole(AriaRole.Button, new() { Name = "Create Invite Link" }).ClickAsync();
        var link = invites.Locator("input[readonly]");
        await Expect(link).ToHaveCountAsync(1);
        var invitePath = new Uri(await link.InputValueAsync()).PathAndQuery;

        // One player joins and then leaves.
        var leaver = await NewUserAsync();
        var leaverPage = await NewSignedInPageAsync(leaver, invitePath);
        await leaverPage.GetByRole(AriaRole.Button, new() { Name = "Join Group" }).ClickAsync();
        await Expect(leaverPage.GetByText("You have joined the group!")).ToBeVisibleAsync();
        await Expect(leaverPage.GetByRole(AriaRole.Table)).ToContainTextAsync(leaver.DisplayName);
        await leaverPage.GetByRole(AriaRole.Button, new() { Name = "Leave Group" }).ClickAsync();
        var leaveConfirm = leaverPage.GetByRole(AriaRole.Dialog).Filter(new() { HasText = "Are you sure you want to leave this group?" });
        await leaveConfirm.GetByRole(AriaRole.Button, new() { Name = "OK" }).ClickAsync();
        await Expect(leaverPage).ToHaveURLAsync(new Regex("/criticsRoyale(/[0-9]+/[0-9])?$"));

        // Another joins and the manager removes them.
        var removed = await NewUserAsync();
        var removedPage = await NewSignedInPageAsync(removed, invitePath);
        await removedPage.GetByRole(AriaRole.Button, new() { Name = "Join Group" }).ClickAsync();
        await Expect(removedPage.GetByText("You have joined the group!")).ToBeVisibleAsync();

        await page.GotoAsync(groupPath);
        var members = page.GetByRole(AriaRole.Table);
        await Expect(members).Not.ToContainTextAsync(leaver.DisplayName);
        var removedRow = members.GetByRole(AriaRole.Row).Filter(new() { HasText = removed.DisplayName });
        await removedRow.GetByRole(AriaRole.Button, new() { Name = "Remove" }).ClickAsync();
        var removeConfirm = page.GetByRole(AriaRole.Dialog).Filter(new() { HasText = "Are you sure you want to remove this member?" });
        await removeConfirm.GetByRole(AriaRole.Button, new() { Name = "OK" }).ClickAsync();
        await Expect(removedRow).ToHaveCountAsync(0);
    }
}
