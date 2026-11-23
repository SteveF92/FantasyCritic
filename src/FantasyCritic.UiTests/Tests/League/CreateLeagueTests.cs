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

    private async Task<(IPage Page, TestUser User, string LeagueName, int Year)> StartAsync(string gameMode, ViewportSize? viewport = null)
    {
        var user = await NewUserAsync();
        var page = await NewSignedInPageAsync(user, "/createLeague", viewport);
        var leagueName = NewLeagueName();

        await page.Locator("#leagueName").FillAsync(leagueName);
        var yearSelect = page.Locator("#initialYear");
        var year = int.Parse(await yearSelect.Locator("option").First.GetAttributeAsync("value") ?? throw new InvalidOperationException("No open year."));
        await yearSelect.SelectOptionAsync(year.ToString());

        await page.Locator("#intendedNumberOfPlayers").FillAsync("4");
        await page.Locator("#gameMode").SelectOptionAsync(new SelectOptionValue { Label = gameMode });
        await page.GetByRole(AriaRole.Button, new() { Name = "Select Preset" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Select a different preset" })).ToBeVisibleAsync();
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
    public async Task TagButtonsMoveTagsOnAPhone()
    {
        var (page, user, leagueName, year) = await StartAsync("Standard", new ViewportSize { Width = 375, Height = 812 });
        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Allow Remaster" })).ToBeVisibleAsync();
        var scrollsSideways = await page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth");
        Assert.That(scrollsSideways, Is.False);

        await page.GetByRole(AriaRole.Button, new() { Name = "Ban Free to Play" }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Allow Remaster" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Allow Free to Play" })).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Ban Remaster" })).ToBeVisibleAsync();

        await CreateAsTestLeagueAsync(page, leagueName, year);

        var banned = (await user.Session.League.GetLeagueYearAsync(LeagueIDFrom(page), year, null)).Settings.Tags.Banned;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(banned, Does.Contain("FreeToPlay"));
            Assert.That(banned, Does.Not.Contain("Remaster"));
        }
    }

    [Test]
    public async Task TagsDragBetweenLists()
    {
        var (page, _, _, _) = await StartAsync("Standard");
        // A tag's row is its button's parent.
        var remaster = page.GetByRole(AriaRole.Button, new() { Name = "Allow Remaster" }).Locator("..");
        var virtualReality = page.GetByRole(AriaRole.Button, new() { Name = "Ban Virtual Reality" }).Locator("..");

        await DragRowOntoRowAsync(page, remaster, virtualReality);

        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Ban Remaster" })).ToBeVisibleAsync();
    }

    /// <summary>
    /// Ui.DragAsync drops just inside the target's top edge, which is enough to reorder one list but not for Sortable to
    /// move a row into another: the pointer has to rest over the middle of a row there for a moment. The drag starts at
    /// the row's left edge, clear of its badge and button.
    /// </summary>
    private static async Task DragRowOntoRowAsync(IPage page, ILocator source, ILocator target)
    {
        await source.ScrollIntoViewIfNeededAsync();
        var from = await source.BoundingBoxAsync() ?? throw new InvalidOperationException("The drag source isn't visible.");
        var to = await target.BoundingBoxAsync() ?? throw new InvalidOperationException("The drop target isn't visible.");

        await page.Mouse.MoveAsync(from.X + 5, from.Y + (from.Height / 2));
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync(from.X + 10, from.Y + (from.Height / 2), new() { Steps = 5 });
        await page.Mouse.MoveAsync(to.X + (to.Width / 2), to.Y + (to.Height / 2), new() { Steps = 30 });
        await page.WaitForTimeoutAsync(300);
        await page.Mouse.MoveAsync(to.X + (to.Width / 2), to.Y + (to.Height / 2) - 5, new() { Steps = 5 });
        await page.WaitForTimeoutAsync(300);
        await page.Mouse.UpAsync();
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
