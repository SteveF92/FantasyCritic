using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests.League;

/// <summary>
/// /createLeague for each game mode: the preset fills the settings, the draft list can grow and shrink, invalid
/// settings block creation, and what is created matches what the form showed.
/// </summary>
public sealed class CreateLeagueTests : UiTestBase
{
    private static string NewLeagueName() => $"UI League {Guid.NewGuid():N}"[..20];

    private async Task<(IPage Page, TestUser User, string LeagueName, int Year)> StartAsync(string gameMode)
    {
        var user = await NewUserAsync();
        var page = await NewSignedInPageAsync(user, "/createLeague");
        var leagueName = NewLeagueName();

        await page.Locator("#leagueName").FillAsync(leagueName);
        var yearSelect = page.Locator("#initialYear");
        var year = int.Parse(await yearSelect.Locator("option").First.GetAttributeAsync("value") ?? throw new InvalidOperationException("No open year."));
        await yearSelect.SelectOptionAsync(year.ToString());

        await page.Locator("#intendedNumberOfPlayers").FillAsync("4");
        await page.Locator("#gameMode").SelectOptionAsync(new SelectOptionValue { Label = gameMode });
        await Expect(Ui.Heading(page, "Game Settings")).ToBeVisibleAsync();
        return (page, user, leagueName, year);
    }

    private static async Task CreateAsTestLeagueAsync(IPage page, string leagueName, int year)
    {
        await page.GetByText("Test League", new() { Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Checkbox, new() { Name = "Test League" })).ToBeCheckedAsync();

        await page.GetByRole(AriaRole.Button, new() { Name = "Create League" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex($"/league/[0-9a-f-]{{36}}/{year}$"));
        await Expect(Ui.Heading(page, leagueName, 1)).ToBeVisibleAsync();
    }

    private static Guid LeagueIDFrom(IPage page) => Guid.Parse(page.Url.Split('/')[^2]);

    [Test]
    public async Task StandardLeagueFromPreset()
    {
        var (page, user, leagueName, year) = await StartAsync("Standard");

        var standardGames = await page.Locator("#standardGames").InputValueAsync();
        var counterPicks = await page.Locator("#counterPicks").InputValueAsync();
        var gamesToDraft = await page.Locator("#gamesToDraft-0").InputValueAsync();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(int.Parse(standardGames), Is.GreaterThan(0));
            Assert.That(int.Parse(gamesToDraft), Is.GreaterThan(0));
        }
        await Expect(page.Locator("#draftName-0")).ToHaveCountAsync(0);

        await CreateAsTestLeagueAsync(page, leagueName, year);

        var leagueYear = await user.Session.League.GetLeagueYearAsync(LeagueIDFrom(page), year, null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(leagueYear.League.LeagueName, Is.EqualTo(leagueName));
            Assert.That(leagueYear.League.TestLeague, Is.True);
            Assert.That(leagueYear.Settings.StandardGames, Is.EqualTo(int.Parse(standardGames)));
            Assert.That(leagueYear.Settings.CounterPicks, Is.EqualTo(int.Parse(counterPicks)));
            Assert.That(leagueYear.Drafts.Single().GamesToDraft, Is.EqualTo(int.Parse(gamesToDraft)));
        }
    }

    [Test]
    public async Task MultiDraftLeagueAddsAndRemovesDrafts()
    {
        var (page, user, leagueName, year) = await StartAsync("Multi Draft");
        await Expect(page.Locator("#draftName-1")).ToBeVisibleAsync();
        await Expect(page.Locator("#draftName-2")).ToHaveCountAsync(0);

        await page.GetByRole(AriaRole.Button, new() { Name = "+ Add Another Draft" }).ClickAsync();
        await Expect(page.Locator("#draftName-2")).ToBeVisibleAsync();

        var secondDraftDate = $"{year}-06-15";
        await page.Locator("#draftName-1").FillAsync("Summer Draft");
        await page.Locator("#scheduledDate-1").FillAsync(secondDraftDate);
        await page.Locator("#draftName-2").FillAsync("Doomed Draft");
        await page.Locator("#scheduledDate-2").FillAsync($"{year}-09-15");

        await page.GetByRole(AriaRole.Button, new() { Name = "Remove Draft 3" }).ClickAsync();
        await Expect(page.Locator("#draftName-2")).ToHaveCountAsync(0);
        await Expect(page.Locator("#draftName-1")).ToHaveValueAsync("Summer Draft");
        await Expect(page.Locator("#scheduledDate-1")).ToHaveValueAsync(secondDraftDate);

        await CreateAsTestLeagueAsync(page, leagueName, year);

        var drafts = (await user.Session.League.GetLeagueYearAsync(LeagueIDFrom(page), year, null)).Drafts.OrderBy(draft => draft.DraftNumber).ToList();
        Assert.That(drafts, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(drafts[1].Name, Is.EqualTo("Summer Draft"));
            Assert.That(drafts[1].ScheduledDate?.ToString("yyyy-MM-dd"), Is.EqualTo(secondDraftDate));
        }
    }

    [Test]
    public async Task OneShotLeagueHasNoDraftSettings()
    {
        var (page, user, leagueName, year) = await StartAsync("One Shot");
        await Expect(Ui.Heading(page, "Draft Settings")).ToHaveCountAsync(0);

        await CreateAsTestLeagueAsync(page, leagueName, year);

        var leagueYear = await user.Session.League.GetLeagueYearAsync(LeagueIDFrom(page), year, null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(leagueYear.Settings.OneShotMode, Is.True);
            Assert.That(leagueYear.Drafts, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public async Task InvalidSettingsBlockCreation()
    {
        var (page, _, _, _) = await StartAsync("Standard");
        var create = page.GetByRole(AriaRole.Button, new() { Name = "Create League" });
        await Expect(create).ToBeEnabledAsync();

        await page.Locator("#standardGames").FillAsync("60");
        await Expect(create).ToBeDisabledAsync();
        await Expect(page.GetByText("Can't create league. Some of your settings are invalid.")).ToBeVisibleAsync();

        await page.Locator("#standardGames").FillAsync("12");
        await Expect(create).ToBeEnabledAsync();
    }

    [Test]
    public async Task AdvancedSettingsCollapseOpensAndCloses()
    {
        var (page, _, _, _) = await StartAsync("Standard");
        var advanced = page.GetByText("We recommend you keep these settings to the default options.");
        await Expect(advanced).ToBeHiddenAsync();

        // The toggle is an h3, but bootstrap-vue's v-b-toggle gives it the button role.
        var toggle = page.GetByText("Advanced Settings", new() { Exact = true });
        await toggle.ClickAsync();
        await Expect(advanced).ToBeVisibleAsync();
        await Expect(page.Locator("#counterPickDeadline")).ToBeVisibleAsync();

        await toggle.ClickAsync();
        await Expect(advanced).ToBeHiddenAsync();
    }
}
