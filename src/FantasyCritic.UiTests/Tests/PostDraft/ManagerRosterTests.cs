using System.Linq;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests.PostDraft;

/// <summary>
/// A manager fixing rosters after the draft: adding and removing a game, adding an unlisted game then linking it to
/// a master game and scoring it by hand, marking a game as won't release, and overriding eligibility and tags.
/// </summary>
public sealed class ManagerRosterTests : PostDraftTestBase
{
    [Test]
    public async Task AddThenRemoveAPublisherGame()
    {
        var (league, managerUserID) = await NewDraftedLeagueAsync();
        var player = league.Publishers[1];
        var game = (await AvailableGamesAsync(league, player, 1)).Single();
        var page = await NewSignedInPageAsync(managerUserID, LeaguePath(league));

        await Ui.SidebarAction(page, "Add Publisher Game").ClickAsync();
        var add = Ui.Dialog(page, "Add Publisher Game");
        await SearchAndPickAsync(add, game.GameName);
        await Expect(add.GetByRole(AriaRole.Heading, new() { Name = "Selected Game:" })).ToBeVisibleAsync();
        await add.GetByRole(AriaRole.Combobox).SelectOptionAsync(new SelectOptionValue { Label = player.PublisherName });
        await add.GetByRole(AriaRole.Button, new() { Name = "Add game to publisher" }).ClickAsync();
        await Expect(add).ToBeHiddenAsync();
        await Expect(page.GetByText($"{game.GameName} added to {player.PublisherName}")).ToBeVisibleAsync();
        Assert.That((await PublisherAsync(league, player)).Games.Select(g => g.GameName), Does.Contain(game.GameName));

        await Ui.SidebarAction(page, "Remove Publisher Game").ClickAsync();
        var remove = Ui.Dialog(page, "Remove Publisher Game");
        await remove.GetByRole(AriaRole.Combobox).First.SelectOptionAsync(new SelectOptionValue { Label = player.PublisherName });
        await Ui.SelectOptionContainingAsync(remove.GetByRole(AriaRole.Combobox).Nth(1), game.GameName);
        await remove.GetByRole(AriaRole.Button, new() { Name = "Remove Game" }).ClickAsync();
        await Expect(remove).ToBeHiddenAsync();
        await Expect(page.GetByText($"{game.GameName} removed from {player.PublisherName}")).ToBeVisibleAsync();
        Assert.That((await PublisherAsync(league, player)).Games.Select(g => g.GameName), Does.Not.Contain(game.GameName));
    }

    [Test]
    public async Task UnlistedGameIsLinkedThenScored()
    {
        var (league, managerUserID) = await NewDraftedLeagueAsync();
        var player = league.Publishers[1];
        var masterGame = (await AvailableGamesAsync(league, player, 1)).Single();
        var page = await NewSignedInPageAsync(managerUserID, LeaguePath(league));
        const string unlisted = "UI Unlisted Game";

        await Ui.SidebarAction(page, "Add Publisher Game").ClickAsync();
        var add = Ui.Dialog(page, "Add Publisher Game");
        await add.Locator("#searchGameName").FillAsync("zzz no such game zzz");
        await add.GetByRole(AriaRole.Button, new() { Name = "Search Game" }).ClickAsync();
        // The search is fuzzy, so it finds something anyway; the unlisted option is offered either way.
        await add.GetByRole(AriaRole.Button, new() { Name = "Select unlisted game." }).ClickAsync();
        await add.Locator("#claimUnlistedGame").FillAsync(unlisted);
        await add.GetByRole(AriaRole.Combobox).SelectOptionAsync(new SelectOptionValue { Label = player.PublisherName });
        await add.GetByRole(AriaRole.Button, new() { Name = "Add game to publisher" }).ClickAsync();
        await Expect(page.GetByText($"{unlisted} added to {player.PublisherName}")).ToBeVisibleAsync();

        await Ui.SidebarAction(page, "Associate Unlinked Game").ClickAsync();
        var associate = Ui.Dialog(page, "Associate Publisher Game");
        await associate.GetByRole(AriaRole.Combobox).First.SelectOptionAsync(new SelectOptionValue { Label = player.PublisherName });
        await associate.GetByRole(AriaRole.Combobox).Nth(1).SelectOptionAsync(new SelectOptionValue { Label = unlisted });
        await SearchAndPickAsync(associate, masterGame.GameName);
        await Expect(associate.GetByText($"Selected Game: {masterGame.GameName}")).ToBeVisibleAsync();
        await associate.GetByRole(AriaRole.Button, new() { Name = "Associate game" }).ClickAsync();
        await Expect(associate).ToBeHiddenAsync();
        await Expect(page.GetByText($"{masterGame.GameName} successfully associated.")).ToBeVisibleAsync();

        await Ui.SidebarAction(page, "Score a Game Manually").ClickAsync();
        var score = Ui.Dialog(page, "Manually Score Publisher Game");
        await score.GetByRole(AriaRole.Combobox).First.SelectOptionAsync(new SelectOptionValue { Label = player.PublisherName });
        // Once linked, the publisher game goes by the master game's name.
        await score.GetByRole(AriaRole.Combobox).Nth(1).SelectOptionAsync(new SelectOptionValue { Label = masterGame.GameName });
        await score.Locator("input[name=manualScore]").FillAsync("85");
        await score.GetByRole(AriaRole.Button, new() { Name = "Set Manual Score" }).ClickAsync();
        await Expect(score).ToBeHiddenAsync();
        await Expect(page.GetByText($"{masterGame.GameName} was given a score of 85.")).ToBeVisibleAsync();

        var game = (await PublisherAsync(league, player)).Games.Single(g => g.MasterGame?.MasterGameID == masterGame.MasterGameID);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(game.ManualCriticScore, Is.True);
            Assert.That(game.CriticScore, Is.EqualTo(85m));
        }
    }

    [Test]
    public async Task WillNotReleaseAndOverrides()
    {
        var (league, managerUserID) = await NewDraftedLeagueAsync();
        var player = league.Publishers[1];
        var drafted = (await PublisherAsync(league, player)).Games.First(game => !game.CounterPick).GameName;
        var other = (await AvailableGamesAsync(league, player, 1)).Single().GameName;
        var page = await NewSignedInPageAsync(managerUserID, LeaguePath(league));

        await Ui.SidebarAction(page, "Override \"Will not Release\"").ClickAsync();
        var willNotRelease = Ui.Dialog(page, "Manually Set Will Not Release");
        await willNotRelease.GetByRole(AriaRole.Combobox).First.SelectOptionAsync(new SelectOptionValue { Label = player.PublisherName });
        await willNotRelease.GetByRole(AriaRole.Combobox).Nth(1).SelectOptionAsync(new SelectOptionValue { Label = drafted });
        await willNotRelease.GetByRole(AriaRole.Button, new() { Name = "Set game as 'Will not Release'" }).ClickAsync();
        await Expect(willNotRelease).ToBeHiddenAsync();
        await Expect(page.GetByText("Will not release status updated.")).ToBeVisibleAsync();

        await Ui.SidebarAction(page, "Override Game Eligibility").ClickAsync();
        var eligibility = Ui.Dialog(page, "Manage Eligibility Overrides");
        await SearchAndPickAsync(eligibility, other);
        await Expect(eligibility.GetByText($"Selected Game: {other}")).ToBeVisibleAsync();
        await eligibility.GetByRole(AriaRole.Button, new() { Name = "Ban Game" }).ClickAsync();
        await Expect(page.GetByText($"{other} was marked as ineligible.")).ToBeVisibleAsync();
        var banned = eligibility.Locator("tbody tr").Filter(new() { HasText = other }).First;
        await Expect(banned).ToContainTextAsync("No");
        await banned.GetByRole(AriaRole.Button, new() { Name = "Reset" }).ClickAsync();
        await Expect(page.GetByText($"{other}'s eligibility was reset to normal.")).ToBeVisibleAsync();
        await eligibility.GetByRole(AriaRole.Button, new() { Name = "Close" }).ClickAsync();

        // The tag picker is vue-multiselect.
        await Ui.SidebarAction(page, "Override Game Tags").ClickAsync();
        var tags = Ui.Dialog(page, "Manage Tag Overrides");
        await SearchAndPickAsync(tags, other);
        await tags.GetByText("Search or add a tag").ClickAsync();
        await page.Keyboard.TypeAsync("Remaster");
        await page.Keyboard.PressAsync("Enter");
        await tags.GetByRole(AriaRole.Button, new() { Name = "Set Tags" }).ClickAsync();
        await Expect(page.GetByText($"{other} had it's tags overriden.")).ToBeVisibleAsync();
        var overridden = tags.Locator("tbody tr").Filter(new() { HasText = other }).First;
        await Expect(overridden).ToContainTextAsync("Remaster");
        await overridden.GetByRole(AriaRole.Button, new() { Name = "Reset" }).ClickAsync();
        await Expect(page.GetByText($"{other} had it's tags reset.")).ToBeVisibleAsync();
    }
}
