using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FantasyCritic.IntegrationTests.Helpers;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests.Conference;

/// <summary>
/// Conferences: creating one, invite links, adding a league, assigning players by dragging and locking leagues, the
/// conference manager's message, settings, and handing over league and conference management.
/// </summary>
public sealed class ConferenceTests : UiTestBase
{
    private sealed record ConferenceSetup(TestUser Manager, Guid ConferenceID, int Year, string ConferenceName, string PrimaryLeagueName, IReadOnlyList<TestUser> Players)
    {
        public string Path => $"/conference/{ConferenceID}/{Year}";
    }

    private static string UniqueName(string prefix) => $"{prefix} {Guid.NewGuid():N}"[..(prefix.Length + 9)];

    /// <summary>
    /// A conference made through the API, with the given number of players joined besides its manager.
    /// </summary>
    private async Task<ConferenceSetup> NewConferenceAsync(int players, bool plusPlayers = false)
    {
        var manager = await NewPlusUserAsync();
        var year = await LeagueTestHelpers.GetOpenYearAsync(manager.Session);
        var conferenceName = UniqueName("UI Conference");
        var primaryLeagueName = UniqueName("UI Primary");
        var conferenceID = await ConferenceFixtureBuilder.CreateAsync(manager.Session, LeagueScenarios.TwoPlayerSmall, year, conferenceName, primaryLeagueName);

        var joined = new List<TestUser>();
        for (var i = 0; i < players; i++)
        {
            var player = plusPlayers ? await NewPlusUserAsync() : await NewUserAsync();
            await ConferenceFixtureBuilder.InviteAndJoinAsync(manager.Session, player.Session, conferenceID);
            joined.Add(player);
        }

        return new ConferenceSetup(manager, conferenceID, year, conferenceName, primaryLeagueName, joined);
    }

    private static async Task<ILocator> ManagerActionAsync(IPage page, string action, string dialogTitle)
    {
        await page.GetByRole(AriaRole.Button, new() { Name = "Manager Actions" }).ClickAsync();
        await page.GetByRole(AriaRole.Menuitem, new() { Name = action }).ClickAsync();
        var dialog = Ui.Dialog(page, dialogTitle);
        await Expect(dialog).ToBeVisibleAsync();
        return dialog;
    }

    /// <summary>
    /// Submits a manager modal, waits for its request to succeed, and for the toast that comes once the page has
    /// refreshed the conference.
    /// </summary>
    private static async Task SubmitAsync(IPage page, ILocator submit, string endpoint, string toast)
    {
        var response = await page.RunAndWaitForResponseAsync(() => submit.ClickAsync(), new Regex($"/api/conference/{endpoint}$", RegexOptions.IgnoreCase));
        Assert.That(response.Ok, Is.True, $"{endpoint} returned {response.Status}.");
        await Expect(page.GetByText(toast, new() { Exact = true })).ToBeVisibleAsync();
    }

    private static ILocator LeagueTable(IPage page) => page.GetByRole(AriaRole.Table).Filter(new() { Has = page.GetByRole(AriaRole.Columnheader, new() { Name = "League Manager" }) });

    [Test]
    public async Task CreateAConference()
    {
        var manager = await NewPlusUserAsync();
        var page = await NewSignedInPageAsync(manager, "/createConference");
        var conferenceName = UniqueName("UI Conference");
        var primaryLeagueName = UniqueName("UI Primary");

        await page.Locator("#conferenceName").FillAsync(conferenceName);
        await page.Locator("#primaryLeagueName").FillAsync(primaryLeagueName);
        var yearSelect = page.Locator("#initialYear");
        var year = await yearSelect.Locator("option").First.GetAttributeAsync("value");
        await yearSelect.SelectOptionAsync(year!);
        await page.Locator("#intendedNumberOfPlayers").FillAsync("4");
        await Expect(Ui.Heading(page, "Game Settings")).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Button, new() { Name = "Create Conference" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex($"/conference/[0-9a-f-]{{36}}/{year}$"));
        await Expect(Ui.Heading(page, conferenceName, 1)).ToBeVisibleAsync();
        await Expect(LeagueTable(page)).ToContainTextAsync(primaryLeagueName);
        await Expect(page.GetByText("Now that your conference is created, here are the next steps:")).ToBeVisibleAsync();
    }

    [Test]
    public async Task InviteLinksCanBeMadeDeletedAndUsed()
    {
        var conference = await NewConferenceAsync(players: 0);
        var page = await NewSignedInPageAsync(conference.Manager, conference.Path);

        var dialog = await ManagerActionAsync(page, "Invite/Remove Players", "Invite/Remove Players");
        var links = dialog.Locator("input[readonly]");
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Create Invite Link" }).ClickAsync();
        await Expect(links).ToHaveCountAsync(1);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Create Invite Link" }).ClickAsync();
        await Expect(links).ToHaveCountAsync(2);

        var deleted = await links.First.InputValueAsync();
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Delete" }).First.ClickAsync();
        await Expect(links).ToHaveCountAsync(1);
        var kept = await links.First.InputValueAsync();
        Assert.That(kept, Is.Not.EqualTo(deleted));

        var joiner = await NewUserAsync();
        var joinerPage = await NewSignedInPageAsync(joiner, new Uri(kept).PathAndQuery);
        await Expect(joinerPage.GetByText("You have been invited to join this conference.")).ToBeVisibleAsync();
        await joinerPage.GetByRole(AriaRole.Button, new() { Name = "Join Conference" }).ClickAsync();
        await Expect(joinerPage.GetByText("You have joined the conference, but your conference manager has not yet assigned you")).ToBeVisibleAsync();
    }

    [Test]
    public async Task AddALeagueThenAssignAndLockPlayers()
    {
        var conference = await NewConferenceAsync(players: 2);
        var (leagueManager, assignee) = (conference.Players[0], conference.Players[1]);
        var page = await NewSignedInPageAsync(conference.Manager, conference.Path);
        var leagueName = UniqueName("UI Second");

        var add = await ManagerActionAsync(page, "Add League to Conference", "Add League to Conference");
        await add.Locator("#newLeagueName").FillAsync(leagueName);
        await add.GetByRole(AriaRole.Combobox).SelectOptionAsync(new SelectOptionValue { Label = leagueManager.DisplayName });
        await SubmitAsync(page, add.GetByRole(AriaRole.Button, new() { Name = "Add New League" }), "AddLeagueToConference", "A new league has been added.");
        await Expect(LeagueTable(page)).ToContainTextAsync(leagueName);

        // Drag the unassigned player into the new league.
        var assign = await ManagerActionAsync(page, "Assign Players to Leagues / Lock In Leagues", "Assign Players");
        var unassigned = assign.GetByRole(AriaRole.Heading, new() { Name = "Unassigned Players" }).Locator("xpath=following-sibling::div[1]");
        var leagueHeader = assign.GetByRole(AriaRole.Heading, new() { Name = leagueName });
        var leagueList = leagueHeader.Locator("xpath=following-sibling::div[1]");
        await Expect(unassigned).ToContainTextAsync(assignee.DisplayName);
        await Ui.DragAsync(page, unassigned.GetByText(assignee.DisplayName, new() { Exact = true }), leagueList);
        await Expect(leagueList).ToContainTextAsync(assignee.DisplayName);
        await Expect(unassigned).Not.ToContainTextAsync(assignee.DisplayName);
        await SubmitAsync(page, assign.GetByRole(AriaRole.Button, new() { Name = "Assign Players to Leagues" }), "AssignLeaguePlayers", "Player Assignments have been updated.");

        var leagueID = Guid.Parse((await LeagueTable(page).GetByRole(AriaRole.Link, new() { Name = leagueName }).GetAttributeAsync("href"))!.Split('/')[2]);
        var leagueYear = await assignee.Session.League.GetLeagueYearAsync(leagueID, conference.Year, null);
        Assert.That(leagueYear.Players.Select(player => player.User?.UserID), Does.Contain(assignee.UserID));

        // Locking turns the league's list into a plain one; unlocking turns it back.
        var lockRequest = new Regex("/api/conference/SetConferenceLeagueLockStatus$", RegexOptions.IgnoreCase);
        var assignAgain = await ManagerActionAsync(page, "Assign Players to Leagues / Lock In Leagues", "Assign Players");
        var header = assignAgain.GetByRole(AriaRole.Heading, new() { Name = leagueName });
        await page.RunAndWaitForRequestAsync(() => header.Locator("svg").First.ClickAsync(), lockRequest);
        await Expect(header.Locator("xpath=following-sibling::ol[1]")).ToContainTextAsync(assignee.DisplayName);
        await page.RunAndWaitForRequestAsync(() => header.Locator("svg").First.ClickAsync(), lockRequest);
        await Expect(header.Locator("xpath=following-sibling::ol[1]")).ToHaveCountAsync(0);
    }

    [Test]
    public async Task ManagerMessageAndSettings()
    {
        var conference = await NewConferenceAsync(players: 0);
        var page = await NewSignedInPageAsync(conference.Manager, conference.Path);
        const string message = "Conference draft night is Friday.";

        var post = await ManagerActionAsync(page, "Post New Manager Message", "Post New Message to Conference");
        await post.GetByRole(AriaRole.Textbox).FillAsync(message);
        await SubmitAsync(page, post.GetByRole(AriaRole.Button, new() { Name = "Post Message" }), "PostNewConferenceManagerMessage", "New manager's message posted.");
        var alert = page.GetByRole(AriaRole.Alert).Filter(new() { HasText = message });
        await Expect(alert).ToBeVisibleAsync();
        await page.RunAndWaitForRequestAsync(
            () => alert.GetByRole(AriaRole.Button, new() { Name = "Close" }).ClickAsync(),
            new Regex("/api/conference/DismissManagerMessage$", RegexOptions.IgnoreCase));
        await page.ReloadAsync();
        await Expect(Ui.Heading(page, conference.ConferenceName, 1)).ToBeVisibleAsync();
        await Expect(page.GetByText(message)).ToHaveCountAsync(0);

        var settings = await ManagerActionAsync(page, "Edit Conference Settings", "Change Conference Options");
        await Expect(settings.Locator("#newConferenceName")).ToHaveValueAsync(conference.ConferenceName);
        await settings.Locator("#newConferenceName").FillAsync("UI Renamed Conference");
        await SubmitAsync(page, settings.GetByRole(AriaRole.Button, new() { Name = "Change Settings" }), "EditConference", "Conference settings have been updated.");
        await Expect(Ui.Heading(page, "UI Renamed Conference", 1)).ToBeVisibleAsync();
    }

    [Test]
    public async Task HandOverLeagueThenConferenceManagement()
    {
        // Only a Plus member can become conference manager.
        var conference = await NewConferenceAsync(players: 1, plusPlayers: true);
        var player = conference.Players[0];
        var page = await NewSignedInPageAsync(conference.Manager, conference.Path);

        var reassign = await ManagerActionAsync(page, "Reassign League Managers", "Reassign League Manager");
        await reassign.GetByRole(AriaRole.Combobox).First.SelectOptionAsync(new SelectOptionValue { Label = conference.PrimaryLeagueName });
        await reassign.GetByRole(AriaRole.Combobox).Nth(1).SelectOptionAsync(new SelectOptionValue { Label = player.DisplayName });
        await SubmitAsync(page, reassign.GetByRole(AriaRole.Button, new() { Name = "Transfer League Manager" }), "ReassignLeagueManager", "League manager has been changed.");
        await Expect(LeagueTable(page).GetByRole(AriaRole.Row).Filter(new() { HasText = conference.PrimaryLeagueName })).ToContainTextAsync(player.DisplayName);

        var promote = await ManagerActionAsync(page, "Promote New Conference Manager", "Promote New Conference Manager");
        await promote.GetByRole(AriaRole.Combobox).SelectOptionAsync(new SelectOptionValue { Label = player.DisplayName });
        await SubmitAsync(page, promote.GetByRole(AriaRole.Button, new() { Name = "Transfer Conference Manager" }), "PromoteNewConferenceManager", "You have transferred conference manager status.");
        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Manager Actions" })).ToHaveCountAsync(0);
        await Expect(page.GetByText("Conference Manager:").Locator("..")).ToContainTextAsync(player.DisplayName);
    }
}
