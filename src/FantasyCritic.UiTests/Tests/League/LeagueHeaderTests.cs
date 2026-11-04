using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests.League;

/// <summary>
/// The league page's header and manager basics: the manager's message and its dismissal, league options, the year
/// select, following and unfollowing, and copying the league ID.
/// </summary>
public sealed class LeagueHeaderTests : UiTestBase
{
    private static string LeaguePath(LeagueFixture league) => $"/league/{league.LeagueID}/{league.Year}";

    [Test]
    public async Task ManagerMessageShowsAndStaysDismissed()
    {
        var (league, managerUserID) = await NewLeagueAsync(LeagueScenarios.TwoPlayerSmall);
        var page = await NewSignedInPageAsync(managerUserID, LeaguePath(league));
        const string message = "Draft is on Friday, bring snacks.";

        await Ui.SidebarAction(page, "Post new Message to League").ClickAsync();
        var dialog = Ui.Dialog(page, "Post New Message to League");
        await Expect(dialog).ToBeVisibleAsync();
        await dialog.GetByRole(AriaRole.Textbox).FillAsync(message);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Post Message" }).ClickAsync();

        await Expect(dialog).ToBeHiddenAsync();
        await Expect(page.GetByText("New manager's message posted.")).ToBeVisibleAsync();
        var alert = page.GetByRole(AriaRole.Alert).Filter(new() { HasText = message });
        await Expect(alert).ToBeVisibleAsync();

        await page.RunAndWaitForRequestAsync(
            () => alert.GetByRole(AriaRole.Button, new() { Name = "Close" }).ClickAsync(),
            new Regex("/api/league/DismissManagerMessage$"));
        await Expect(alert).ToBeHiddenAsync();

        await page.ReloadAsync();
        await Expect(Ui.Heading(page, "Standings")).ToBeVisibleAsync();
        await Expect(page.GetByText(message)).ToHaveCountAsync(0);
    }

    [Test]
    public async Task ChangeLeagueOptionsRenamesAndPublishes()
    {
        var (league, managerUserID) = await NewLeagueAsync(LeagueScenarios.TwoPlayerSmall);
        var page = await NewSignedInPageAsync(managerUserID, LeaguePath(league));
        var originalName = (await league.GetLeagueYearAsync()).League.LeagueName;

        await Ui.SidebarAction(page, "Change General League Options").ClickAsync();
        var dialog = Ui.Dialog(page, "Change League Options");
        var name = dialog.Locator("#newleagueName");
        await Expect(name).ToHaveValueAsync(originalName);
        await Expect(dialog.GetByRole(AriaRole.Checkbox, new() { Name = "Public League" })).Not.ToBeCheckedAsync();

        await name.FillAsync("UI Renamed League");
        await dialog.GetByText("Public League", new() { Exact = true }).ClickAsync();
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Change Settings" }).ClickAsync();

        await Expect(dialog).ToBeHiddenAsync();
        await Expect(page.GetByText("League options have been updated.")).ToBeVisibleAsync();
        await Expect(Ui.Heading(page, "UI Renamed League", 1)).ToBeVisibleAsync();
        var updated = (await league.GetLeagueYearAsync()).League;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(updated.LeagueName, Is.EqualTo("UI Renamed League"));
            Assert.That(updated.PublicLeague, Is.True);
        }
    }

    [Test]
    public async Task YearSelectSwitchesToThatYear()
    {
        var (league, managerUserID) = await NewLeagueAsync(LeagueScenarios.TwoPlayerSmall);
        var previousYear = league.Year - 1;
        var page = await NewPageAsync();

        // A league only gets a second year once the next one opens, so the earlier year is planted from this one.
        await Ui.PlantJsonAsync(page, new Regex("/api/League/GetLeagueYear\\?"), async route =>
        {
            var leagueYear = await Ui.FetchJsonAsync(route, route.Request.Url.Replace($"year={previousYear}", $"year={league.Year}"));
            leagueYear["league"]!["years"] = new JsonArray(league.Year, previousYear);
            if (route.Request.Url.Contains($"year={previousYear}"))
            {
                leagueYear["year"] = previousYear;
            }

            return leagueYear;
        });
        await page.GotoAsync($"/dev/sign-in-as?userId={managerUserID}&returnUrl={LeaguePath(league)}");
        var yearSelect = page.GetByRole(AriaRole.Combobox).Filter(new() { Has = page.GetByRole(AriaRole.Option, new() { Name = previousYear.ToString(), Exact = true }) });
        await Expect(yearSelect).ToHaveValueAsync(league.Year.ToString());

        await page.RunAndWaitForRequestAsync(() => yearSelect.SelectOptionAsync(previousYear.ToString()), new Regex($"year={previousYear}"));
        await Expect(page).ToHaveURLAsync(new Regex($"/league/{league.LeagueID}/{previousYear}$"));
        await Expect(yearSelect).ToHaveValueAsync(previousYear.ToString());
    }

    [Test]
    public async Task FollowThenUnfollowFromHome()
    {
        var (league, _) = await NewLeagueAsync(LeagueScenarios.TwoPlayerSmall);
        var leagueName = (await league.GetLeagueYearAsync()).League.LeagueName;
        await league.Manager.LeagueManager.ChangeLeagueOptionsAsync(new ChangeLeagueOptionsRequest
        {
            LeagueID = league.LeagueID,
            LeagueName = leagueName,
            PublicLeague = true,
            TestLeague = true,
            CustomRulesLeague = false,
        });

        var outsider = await NewUserAsync();
        var page = await NewSignedInPageAsync(outsider, LeaguePath(league));
        await page.GetByRole(AriaRole.Button, new() { Name = "Follow", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Unfollow", Exact = true })).ToBeVisibleAsync();

        await page.GotoAsync("/home");
        await page.GetByRole(AriaRole.Tab, new() { Name = "Followed Leagues" }).ClickAsync();
        var followed = page.GetByRole(AriaRole.Tabpanel, new() { Name = "Followed Leagues" });
        await Expect(followed.GetByText(leagueName)).ToBeVisibleAsync();

        await followed.GetByRole(AriaRole.Button, new() { Name = "Unfollow" }).ClickAsync();
        var confirm = Ui.Dialog(page, "Unfollow League");
        await Expect(confirm).ToContainTextAsync($"Are you sure you want to unfollow {leagueName}?");
        await confirm.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();
        await Expect(confirm).ToBeHiddenAsync();
        await Expect(followed.GetByText(leagueName)).ToBeVisibleAsync();

        await followed.GetByRole(AriaRole.Button, new() { Name = "Unfollow" }).ClickAsync();
        await confirm.GetByRole(AriaRole.Button, new() { Name = "Unfollow" }).ClickAsync();
        await Expect(followed.GetByText(leagueName)).ToHaveCountAsync(0);
    }

    [Test]
    public async Task CopyLeagueIDPutsItOnTheClipboard()
    {
        var (league, managerUserID) = await NewLeagueAsync(LeagueScenarios.TwoPlayerSmall);
        var page = await NewSignedInPageAsync(managerUserID, LeaguePath(league));
        await page.Context.GrantPermissionsAsync(["clipboard-read", "clipboard-write"]);
        var leagueName = (await league.GetLeagueYearAsync()).League.LeagueName;

        await Ui.Heading(page, leagueName, 1).Locator("svg").Last.ClickAsync();

        await Expect(page.GetByText("League ID copied to clipboard.")).ToBeVisibleAsync();
        Assert.That(await page.EvaluateAsync<string>("() => navigator.clipboard.readText()"), Is.EqualTo(league.LeagueID.ToString()));
    }
}
