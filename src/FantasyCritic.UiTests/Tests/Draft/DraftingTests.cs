using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.IntegrationTests.Helpers;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests.Draft;

/// <summary>
/// Running a draft in the browser: starting it, picks showing up live on another player's page (SignalR), counter
/// picks, and the draft complete dialog.
/// </summary>
public sealed class DraftingTests : DraftTestBase
{
    private static string LeaguePath(LeagueFixture league) => $"/league/{league.LeagueID}/{league.Year}";

    [Test]
    public async Task StartDraftThenPicksShowLive()
    {
        var (league, managerUserID) = await NewLeagueAsync(LeagueScenarios.TwoPlayerSmall);
        var managerPage = await NewSignedInPageAsync(managerUserID, LeaguePath(league));

        await managerPage.GetByRole(AriaRole.Button, new() { Name = "Start Drafting!" }).ClickAsync();
        var warning = WarningDialog(managerPage);
        await Expect(warning).ToContainTextAsync("Once you start drafting, you can no longer add or remove players");
        await warning.GetByRole(AriaRole.Button, new() { Name = "OK" }).ClickAsync();
        await Expect(managerPage.GetByText("The draft is currently in progress!").First).ToBeVisibleAsync();

        var firstUp = await NextUpAsync(league);
        var secondUp = Other(league, firstUp);
        var firstPage = await NewSignedInPageAsync(await UserIDOfAsync(firstUp), LeaguePath(league));
        var secondPage = await NewSignedInPageAsync(await UserIDOfAsync(secondUp), LeaguePath(league));
        await Expect(firstPage.GetByText("It is your turn to draft!")).ToBeVisibleAsync();
        await Expect(secondPage.GetByText("Next to draft:")).ToContainTextAsync(firstUp.PublisherName);

        // The first pick searches for its game.
        var firstGame = await DraftableGameNameAsync(league, firstUp);
        await Ui.ButtonElement(firstPage.Locator("body"), "Draft Game").ClickAsync();
        var dialog = Ui.Dialog(firstPage, "Select Draft Game");
        await Expect(dialog.GetByRole(AriaRole.Heading, new() { Name = "Top Available Games" })).ToBeVisibleAsync();
        await dialog.Locator("#searchGameName").FillAsync(firstGame);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Search Game" }).ClickAsync();
        await Expect(dialog.GetByRole(AriaRole.Heading, new() { Name = "Search Results" })).ToBeVisibleAsync();
        await SelectGameRowAsync(dialog, firstGame);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Draft Game", Exact = true }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();
        await Expect(firstPage.GetByText($"You have drafted: {firstGame}")).ToBeVisibleAsync();

        // The other player's page hears about it without a reload.
        await Expect(secondPage.GetByText("It is your turn to draft!")).ToBeVisibleAsync();
        await Expect(secondPage.GetByText(firstGame).First).ToBeVisibleAsync();

        // The second pick comes straight from the top available list.
        var secondGame = await DraftableGameNameAsync(league, secondUp);
        await Ui.SidebarAction(secondPage, "Draft Game").ClickAsync();
        var secondDialog = Ui.Dialog(secondPage, "Select Draft Game");
        await Expect(secondDialog.GetByRole(AriaRole.Heading, new() { Name = "Top Available Games" })).ToBeVisibleAsync();
        await SelectGameRowAsync(secondDialog, secondGame);
        await secondDialog.GetByRole(AriaRole.Button, new() { Name = "Draft Game", Exact = true }).ClickAsync();
        await Expect(secondPage.GetByText($"You have drafted: {secondGame}")).ToBeVisibleAsync();
        await Expect(firstPage.GetByText(secondGame).First).ToBeVisibleAsync();

        var publishers = (await league.GetLeagueYearAsync()).Publishers;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(publishers.Single(p => p.PublisherID == firstUp.PublisherID).Games.Select(game => game.GameName), Does.Contain(firstGame));
            Assert.That(publishers.Single(p => p.PublisherID == secondUp.PublisherID).Games.Select(game => game.GameName), Does.Contain(secondGame));
        }
    }

    [Test]
    public async Task CounterPickFromTheSidebar()
    {
        var (league, _) = await NewLeagueAsync(LeagueScenarios.TwoPlayerSmall, startDraft: true);
        await league.DraftUntilCounterPickPhaseAsync();
        var next = await NextUpAsync(league);
        var page = await NewSignedInPageAsync(await UserIDOfAsync(next), LeaguePath(league));
        await Expect(page.GetByText("It's time to draft counter picks!")).ToBeVisibleAsync();

        await Ui.SidebarAction(page, "Draft Counter Pick").ClickAsync();
        var dialog = Ui.Dialog(page, "Select Counter Pick");
        await Expect(dialog.GetByRole(AriaRole.Heading, new() { Name = "Available Counter Picks" })).ToBeVisibleAsync();
        var firstOption = dialog.GetByRole(AriaRole.Row).Filter(new() { Has = page.GetByRole(AriaRole.Button, new() { Name = "Select" }) }).First;
        var gameName = (await firstOption.GetByRole(AriaRole.Cell).First.TextContentAsync())!.Trim();
        await firstOption.GetByRole(AriaRole.Button, new() { Name = "Select" }).ClickAsync();
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Select Game as Counter Pick" }).ClickAsync();

        await Expect(dialog).ToBeHiddenAsync();
        await Expect(page.GetByText($"You have selected {gameName} as a counter pick.")).ToBeVisibleAsync();
        var publisher = (await league.GetLeagueYearAsync()).Publishers.Single(p => p.PublisherID == next.PublisherID);
        Assert.That(publisher.Games.Where(game => game.CounterPick).Select(game => game.GameName), Does.Contain(gameName));
    }

    [Test]
    public async Task DraftCompleteDialogOpensWhenTheLastPickLands()
    {
        var (league, managerUserID) = await NewLeagueAsync(LeagueScenarios.TwoPlayerSmall, startDraft: true);
        await league.DraftUntilCounterPickPhaseAsync();
        var page = await NewSignedInPageAsync(managerUserID, LeaguePath(league));
        await Expect(page.GetByText("counter picks!").First).ToBeVisibleAsync();

        await league.DraftToCompletionAsync();

        var dialog = Ui.Dialog(page, "Draft Complete!");
        await Expect(dialog).ToBeVisibleAsync(new() { Timeout = 15000 });
        await dialog.GetByRole(AriaRole.Button, new() { Name = "OK" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();
    }
}
