using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.IntegrationTests.Helpers;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests.Draft;

/// <summary>
/// Before a draft: inviting a player by email and by link, and creating and renaming a publisher.
/// </summary>
public sealed class InviteAndPublisherTests : UiTestBase
{
    /// <summary>
    /// A league with only its manager, who has no publisher yet.
    /// </summary>
    private async Task<(TestUser Manager, Guid LeagueID, int Year, string LeagueName)> NewEmptyLeagueAsync()
    {
        var manager = await NewUserAsync();
        var year = await LeagueTestHelpers.GetOpenYearAsync(manager.Session);
        var leagueID = await LeagueTestHelpers.CreateLeagueAsync(manager.Session, LeagueScenarios.TwoPlayerSmall, year);
        var leagueName = (await manager.Session.League.GetLeagueYearAsync(leagueID, year, null)).League.LeagueName;
        return (manager, leagueID, year, leagueName);
    }

    [Test]
    public async Task InviteByEmailThenJoin()
    {
        var (manager, leagueID, year, leagueName) = await NewEmptyLeagueAsync();
        var invitee = await NewUserAsync();
        var leaguePath = $"/league/{leagueID}/{year}";

        var managerPage = await NewSignedInPageAsync(manager, leaguePath);
        await Ui.SidebarAction(managerPage, "Invite a Player").ClickAsync();
        var dialog = Ui.Dialog(managerPage, "Invite a Player");
        var send = dialog.GetByRole(AriaRole.Button, new() { Name = "Send Invite" });
        await Expect(send).ToBeDisabledAsync();

        await dialog.Locator("#inviteEmail").FillAsync("not-an-email");
        await Expect(send).ToBeDisabledAsync();
        await dialog.Locator("#inviteEmail").FillAsync(invitee.Email);
        await Expect(send).ToBeEnabledAsync();
        await send.ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();
        await Expect(managerPage.GetByText("Invite was sent to")).ToBeVisibleAsync();

        var inviteePage = await NewSignedInPageAsync(invitee, "/home");
        await inviteePage.GetByRole(AriaRole.Tab, new() { Name = "League Invites" }).ClickAsync();
        await inviteePage.GetByRole(AriaRole.Tabpanel, new() { Name = "League Invites" }).GetByRole(AriaRole.Link, new() { Name = leagueName }).ClickAsync();

        await Expect(inviteePage.GetByText("You have been invited to join this league. Do you wish to join?")).ToBeVisibleAsync();
        await inviteePage.GetByRole(AriaRole.Button, new() { Name = "Join", Exact = true }).ClickAsync();
        await Expect(inviteePage.GetByText("You need to create your publisher for this year.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task InviteLinkJoinsLeague()
    {
        var (manager, leagueID, year, _) = await NewEmptyLeagueAsync();
        var invitee = await NewUserAsync();

        var managerPage = await NewSignedInPageAsync(manager, $"/league/{leagueID}/{year}");
        await Ui.SidebarAction(managerPage, "Invite a Player").ClickAsync();
        var dialog = Ui.Dialog(managerPage, "Invite a Player");
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Create Invite Link" }).ClickAsync();
        var link = dialog.Locator("input[readonly]");
        await Expect(link).ToHaveValueAsync(new System.Text.RegularExpressions.Regex("inviteCode="));
        var invitePath = new Uri(await link.InputValueAsync()).PathAndQuery;

        var inviteePage = await NewSignedInPageAsync(invitee, invitePath);
        await Expect(inviteePage.GetByText("You have been invited to join this league.")).ToBeVisibleAsync();
        await inviteePage.GetByRole(AriaRole.Button, new() { Name = "Join League" }).ClickAsync();
        await Expect(inviteePage.GetByText("You need to create your publisher for this year.")).ToBeVisibleAsync();

        var players = (await manager.Session.League.GetLeagueYearAsync(leagueID, year, null)).Players;
        Assert.That(players.Select(player => player.User?.UserID), Does.Contain(invitee.UserID));
    }

    [Test]
    public async Task CreateThenRenamePublisher()
    {
        var (manager, leagueID, year, _) = await NewEmptyLeagueAsync();
        var page = await NewSignedInPageAsync(manager, $"/league/{leagueID}/{year}");

        await page.GetByRole(AriaRole.Button, new() { Name = "Create Publisher", Exact = true }).ClickAsync();
        var create = Ui.Dialog(page, "Create Publisher");
        await create.Locator("#publisherName").FillAsync("UI First Publisher");
        await create.GetByRole(AriaRole.Button, new() { Name = "Create Publisher" }).ClickAsync();
        await Expect(create).ToBeHiddenAsync();
        await Expect(page.GetByText("UI First Publisher created.")).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Table).GetByText("UI First Publisher")).ToBeVisibleAsync();

        await Ui.SidebarAction(page, "Change Publisher Name").ClickAsync();
        var rename = Ui.Dialog(page, "Change Publisher Name");
        await rename.Locator("#newPublisherName").FillAsync("UI Renamed Publisher");
        await rename.GetByRole(AriaRole.Button, new() { Name = "Change Name" }).ClickAsync();
        await Expect(rename).ToBeHiddenAsync();
        await Expect(page.GetByRole(AriaRole.Table).GetByText("UI Renamed Publisher")).ToBeVisibleAsync();

        var publisher = (await manager.Session.League.GetLeagueYearAsync(leagueID, year, null)).Publishers.Single();
        Assert.That(publisher.PublisherName, Is.EqualTo("UI Renamed Publisher"));
    }
}
