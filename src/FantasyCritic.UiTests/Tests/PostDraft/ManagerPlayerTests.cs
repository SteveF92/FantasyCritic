using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.IntegrationTests.Helpers;
using Microsoft.Playwright;
using NodaTime;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests.PostDraft;

/// <summary>
/// A manager looking after players and publishers after the draft: editing a publisher, disconnecting a player and
/// reassigning their publisher, handing over the league, and running a special auction.
/// </summary>
public sealed class ManagerPlayerTests : PostDraftTestBase
{
    [Test]
    public async Task EditAPublisher()
    {
        var (league, managerUserID) = await NewDraftedLeagueAsync();
        var player = league.Publishers[1];
        var page = await NewSignedInPageAsync(managerUserID, LeaguePath(league));

        await Ui.SidebarAction(page, "Edit Publishers").ClickAsync();
        var dialog = Ui.Dialog(page, "Edit Publishers");
        await dialog.GetByRole(AriaRole.Combobox).SelectOptionAsync(new SelectOptionValue { Label = player.PublisherName });
        await Expect(dialog.Locator("#newPublisherName")).ToHaveValueAsync(player.PublisherName);
        await dialog.Locator("#newPublisherName").FillAsync("UI Edited Publisher");
        await dialog.Locator("#newBudget").FillAsync("90");
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Edit Publisher" }).ClickAsync();

        await Expect(dialog).ToBeHiddenAsync();
        await Expect(page.GetByText("Publisher has been edited.")).ToBeVisibleAsync();
        var publisher = await PublisherAsync(league, player);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(publisher.PublisherName, Is.EqualTo("UI Edited Publisher"));
            Assert.That(publisher.Budget, Is.EqualTo(90));
        }
    }

    [Test]
    public async Task DisconnectThenReassignAPublisher()
    {
        var (league, managerUserID) = await NewDraftedLeagueAsync();
        var player = league.Publishers[1];
        var playerName = (await player.Session.Account.CurrentUserAsync()).DisplayName;
        var page = await NewSignedInPageAsync(managerUserID, LeaguePath(league));

        await Ui.SidebarAction(page, "Disconnect a Player").ClickAsync();
        var disconnect = Ui.Dialog(page, "Disconnect a Player");
        await Ui.SelectOptionContainingAsync(disconnect.GetByRole(AriaRole.Combobox), player.PublisherName);
        await disconnect.GetByRole(AriaRole.Button, new() { Name = "Disconnect Player" }).ClickAsync();
        await Expect(disconnect).ToBeHiddenAsync();
        await Expect(page.GetByText($"{playerName} has been disconnected from {player.PublisherName}.")).ToBeVisibleAsync();

        // A player who joins after the draft is inactive, so they can take the publisher over.
        var replacement = await NewUserAsync();
        await LeagueTestHelpers.InviteAndAcceptAsync(league.Manager, replacement.Session, league.LeagueID);
        await page.ReloadAsync();

        await Ui.SidebarAction(page, "Reassign a Publisher").ClickAsync();
        var reassign = Ui.Dialog(page, "Reassign a Publisher");
        await Ui.SelectOptionContainingAsync(reassign.GetByRole(AriaRole.Combobox).First, player.PublisherName);
        await reassign.GetByRole(AriaRole.Combobox).Nth(1).SelectOptionAsync(new SelectOptionValue { Label = replacement.DisplayName });
        var confirm = reassign.GetByRole(AriaRole.Button, new() { Name = "Reassign Publisher", Exact = true });
        await Expect(confirm).ToBeDisabledAsync();
        await reassign.GetByRole(AriaRole.Textbox).FillAsync("Reassign Publisher");
        await Expect(confirm).ToBeEnabledAsync();
        await confirm.ClickAsync();
        await Expect(page.GetByText("Publisher has been reassigned.")).ToBeVisibleAsync();

        Assert.That((await PublisherAsync(league, player)).PlayerName, Is.EqualTo(replacement.DisplayName));
    }

    [Test]
    public async Task TransferTheLeague()
    {
        var (league, managerUserID) = await NewDraftedLeagueAsync();
        var newManager = await league.Publishers[1].Session.Account.CurrentUserAsync();
        var page = await NewSignedInPageAsync(managerUserID, LeaguePath(league));
        await Expect(Ui.Heading(page, "Manage League")).ToBeVisibleAsync();

        await Ui.SidebarAction(page, "Promote new League Manager").ClickAsync();
        var dialog = Ui.Dialog(page, "Transfer Manager");
        await dialog.GetByRole(AriaRole.Combobox).SelectOptionAsync(new SelectOptionValue { Label = newManager.DisplayName });
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Transfer League Manager" }).ClickAsync();

        await Expect(dialog).ToBeHiddenAsync();
        await Expect(page.GetByText("You have transferred league manager status.")).ToBeVisibleAsync();
        await Expect(Ui.Heading(page, "Manage League")).ToHaveCountAsync(0);
        Assert.That((await league.GetLeagueYearAsync()).League.LeagueManager.UserID, Is.EqualTo(newManager.UserID));
    }

    [Test]
    public async Task CreateThenCancelASpecialAuction()
    {
        var (league, managerUserID) = await NewDraftedLeagueAsync();
        var game = (await AvailableGamesAsync(league, league.Publishers[0], 1)).Single();
        var page = await NewSignedInPageAsync(managerUserID, LeaguePath(league));

        await Ui.SidebarAction(page, "Special Auctions").ClickAsync();
        var dialog = Ui.Dialog(page, "Manage Special Auctions");
        await SearchAndPickAsync(dialog, game.GameName);
        await Expect(dialog.GetByRole(AriaRole.Heading, new() { Name = "Selected Game:" })).ToBeVisibleAsync();
        var endTime = PinnedStart.Plus(Duration.FromDays(2)).ToDateTimeOffset().ToLocalTime();
        await dialog.Locator("input[type=datetime-local]").FillAsync(endTime.ToString("yyyy-MM-dd'T'HH:mm"));
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Create Special Auction" }).ClickAsync();
        await Expect(page.GetByText($"Special auction created for: {game.GameName}")).ToBeVisibleAsync();

        var auction = dialog.GetByRole(AriaRole.Row).Filter(new() { HasText = game.GameName });
        await Expect(auction.GetByRole(AriaRole.Button, new() { Name = "Cancel" })).ToBeVisibleAsync();
        Assert.That((await league.GetLeagueYearAsync()).ActiveSpecialAuctions.Select(a => a.MasterGameYear.GameName), Does.Contain(game.GameName));

        await auction.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();
        await Expect(page.GetByText($"Cancelled special auction for: {game.GameName}")).ToBeVisibleAsync();
        await Expect(auction).ToHaveCountAsync(0);
    }
}
