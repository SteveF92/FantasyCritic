using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.IntegrationTests.Helpers;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests.Draft;

/// <summary>
/// Auto draft, set by a player and by the manager, and the manager's draft controls: drafting for the next player,
/// pausing, undoing, skipping, resuming and resetting.
/// </summary>
public sealed class DraftControlTests : DraftTestBase
{
    private const string Off = "Off";
    private const string StandardGamesOnly = "Standard Games Only (Don't Auto Draft Counter Picks)";
    private const string All = "On (Including Counter Picks)";

    private static string LeaguePath(LeagueFixture league) => $"/league/{league.LeagueID}/{league.Year}";

    [Test]
    public async Task PlayerSetsAutoDraft()
    {
        var (league, _) = await NewLeagueAsync(LeagueScenarios.TwoPlayerSmall);
        var player = league.Publishers.Single(publisher => publisher.DraftPosition == 2);
        var page = await NewSignedInPageAsync(await UserIDOfAsync(player), LeaguePath(league));

        await Ui.SidebarAction(page, "Set Auto Draft").ClickAsync();
        var dialog = Ui.Dialog(page, "Edit Auto Draft");
        var mode = dialog.GetByRole(AriaRole.Combobox);
        var watchlistOnly = dialog.GetByRole(AriaRole.Checkbox, new() { Name = "Only Auto Draft from Watchlist" });
        await Expect(watchlistOnly).ToBeDisabledAsync();

        await mode.SelectOptionAsync(new SelectOptionValue { Label = All });
        await Expect(watchlistOnly).ToBeEnabledAsync();
        await dialog.GetByText("Only Auto Draft from Watchlist", new() { Exact = true }).ClickAsync();
        await Expect(watchlistOnly).ToBeCheckedAsync();

        // Turning auto draft off clears the watchlist option.
        await mode.SelectOptionAsync(new SelectOptionValue { Label = Off });
        await Expect(watchlistOnly).Not.ToBeCheckedAsync();
        await Expect(watchlistOnly).ToBeDisabledAsync();

        await mode.SelectOptionAsync(new SelectOptionValue { Label = StandardGamesOnly });
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Set Auto Draft" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();
        await Expect(page.GetByText("Auto draft set to StandardGamesOnly")).ToBeVisibleAsync();

        await Ui.SidebarAction(page, "Set Auto Draft").ClickAsync();
        await Expect(mode).ToHaveValueAsync("StandardGamesOnly");
        var publisher = (await league.GetLeagueYearAsync()).Publishers.Single(p => p.PublisherID == player.PublisherID);
        Assert.That(publisher.AutoDraftMode, Is.EqualTo("StandardGamesOnly"));
    }

    [Test]
    public async Task ManagerSetsAPlayersAutoDraft()
    {
        var (league, managerUserID) = await NewLeagueAsync(LeagueScenarios.TwoPlayerSmall);
        var player = league.Publishers.Single(publisher => publisher.DraftPosition == 2);
        var page = await NewSignedInPageAsync(managerUserID, LeaguePath(league));

        await Ui.SidebarAction(page, "Edit Player Auto Draft").ClickAsync();
        var dialog = Ui.Dialog(page, "Edit Auto Draft");
        var playerMode = dialog.Locator($"label:text-is(\"{player.PublisherName}\") ~ select");
        await Expect(playerMode).ToHaveValueAsync("Off");
        await playerMode.SelectOptionAsync(new SelectOptionValue { Label = All });
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Set Auto Draft" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();

        await Ui.WaitUntilAsync(
            async () => (await league.GetLeagueYearAsync()).Publishers.Single(p => p.PublisherID == player.PublisherID).AutoDraftMode == "All",
            "the player's auto draft to be saved");
    }

    [Test]
    public async Task ManagerDraftControls()
    {
        var (league, managerUserID) = await NewLeagueAsync(LeagueScenarios.TwoPlayerSmall, startDraft: true);
        var page = await NewSignedInPageAsync(managerUserID, LeaguePath(league));
        var warning = WarningDialog(page);

        // Draft for whoever is next.
        var next = await NextUpAsync(league);
        var game = await DraftableGameNameAsync(league, next);
        await Ui.SidebarAction(page, "Draft Game for Next Player").ClickAsync();
        var draftDialog = Ui.Dialog(page, "Select Draft Game");
        await Expect(draftDialog.GetByRole(AriaRole.Heading, new() { Name = "Top Available Games" })).ToBeVisibleAsync();
        await SelectGameRowAsync(draftDialog, game);
        await draftDialog.GetByRole(AriaRole.Button, new() { Name = "Add Game to Publisher" }).ClickAsync();
        await Expect(page.GetByText($"{game} drafted by {next.PublisherName}")).ToBeVisibleAsync();
        await Expect(page.GetByText(game).First).ToBeVisibleAsync();

        // Pause, then undo that pick.
        // This sidebar item holds a "Pause Draft" span and a hidden "Resume Draft" one.
        await page.GetByText("Pause Draft", new() { Exact = true }).ClickAsync();
        await Expect(warning).ToContainTextAsync("Do you wish to pause the draft?");
        await warning.GetByRole(AriaRole.Button, new() { Name = "OK" }).ClickAsync();
        await Expect(page.GetByText("Draft has been paused.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(page.GetByText("The draft has been paused. You can undo games that have been drafted.")).ToBeVisibleAsync();

        await Ui.SidebarAction(page, "Undo Last Drafted Game").ClickAsync();
        await warning.GetByRole(AriaRole.Button, new() { Name = "OK" }).ClickAsync();
        await Expect(page.GetByText("Last action was undone.")).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Table).GetByText(game)).ToHaveCountAsync(0);

        // Skipping needs the confirmation typed.
        await Ui.SidebarAction(page, "Skip Current Pick").ClickAsync();
        var skipOK = warning.GetByRole(AriaRole.Button, new() { Name = "OK" });
        await Expect(skipOK).ToBeDisabledAsync();
        await warning.GetByRole(AriaRole.Textbox).FillAsync("SKIP TURN");
        await Expect(skipOK).ToBeEnabledAsync();
        await skipOK.ClickAsync();
        await Expect(page.GetByText("Turn was skipped.")).ToBeVisibleAsync();

        await Ui.ButtonElement(page.Locator("body"), "Resume Draft").ClickAsync();
        await Expect(warning).ToContainTextAsync("Do you wish to unpause the draft?");
        await warning.GetByRole(AriaRole.Button, new() { Name = "OK" }).ClickAsync();
        await Expect(page.GetByText("Draft has been un-paused.")).ToBeVisibleAsync();

        // Resetting needs its confirmation typed too, and puts the league back before the draft.
        await Ui.SidebarAction(page, "Reset Draft").ClickAsync();
        var resetOK = warning.GetByRole(AriaRole.Button, new() { Name = "OK" });
        await Expect(resetOK).ToBeDisabledAsync();
        await warning.GetByRole(AriaRole.Textbox).FillAsync("RESET DRAFT");
        await resetOK.ClickAsync();
        await Expect(page.GetByText("Draft has been reset.")).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Start Drafting!" })).ToBeVisibleAsync();
    }
}
