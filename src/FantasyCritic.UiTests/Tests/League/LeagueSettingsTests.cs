using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FantasyCritic.IntegrationTests.Helpers;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests.League;

/// <summary>
/// A manager editing an existing league year: /editLeague's settings round trip, and /manageDrafts adding, editing
/// and deleting a draft.
/// </summary>
public sealed class LeagueSettingsTests : UiTestBase
{
    private static ILocator DropRowCheckbox(IPage page, string row) =>
        page.GetByRole(AriaRole.Row).Filter(new() { Has = page.GetByRole(AriaRole.Rowheader, new() { Name = row, Exact = true }) }).GetByRole(AriaRole.Checkbox);

    [Test]
    public async Task EditLeagueLoadsAndSavesSettings()
    {
        var (league, managerUserID) = await NewLeagueAsync(LeagueScenarios.TwoPlayerSmall);
        var editPath = $"/editLeague/{league.LeagueID}/{league.Year}";
        var page = await NewSignedInPageAsync(managerUserID, editPath);

        await Expect(Ui.Heading(page, "Edit League Year Settings")).ToBeVisibleAsync();
        await Expect(page.Locator("#standardGames")).ToHaveValueAsync(LeagueScenarios.TwoPlayerSmall.StandardGames.ToString());
        await Expect(page.Locator("#counterPicks")).ToHaveValueAsync(LeagueScenarios.TwoPlayerSmall.CounterPicks.ToString());
        await Expect(page.Locator("#gamesToDraft-0")).ToHaveValueAsync(LeagueScenarios.TwoPlayerSmall.GamesToDraft.ToString());
        await Expect(page.Locator("#willReleaseDroppableGames")).ToBeVisibleAsync();

        await page.Locator("#standardGames").FillAsync("4");
        await DropRowCheckbox(page, "Will Release").CheckAsync(new() { Force = true });
        await Expect(page.Locator("#willReleaseDroppableGames")).ToHaveCountAsync(0);
        await page.Locator("#willNotReleaseDroppableGames").FillAsync("2");

        await page.GetByRole(AriaRole.Button, new() { Name = "Confirm Settings" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex($"/league/{league.LeagueID}/{league.Year}$"));

        // The year name is saved in an edit of its own: on main, an edit that also changes a setting drops the name.
        await page.GotoAsync(editPath);
        await page.Locator("#leagueYearName").FillAsync("UI Year Name");
        await page.GetByRole(AriaRole.Button, new() { Name = "Confirm Settings" }).ClickAsync();
        await Expect(Ui.Heading(page, "UI Year Name", 1)).ToBeVisibleAsync();

        var settings = (await league.GetLeagueYearAsync()).Settings;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(settings.LeagueYearName, Is.EqualTo("UI Year Name"));
            Assert.That(settings.StandardGames, Is.EqualTo(4));
            Assert.That(settings.UnlimitedWillReleaseDroppableGames, Is.True);
            Assert.That(settings.WillNotReleaseDroppableGames, Is.EqualTo(2));
        }

        await page.GotoAsync(editPath);
        await Expect(page.Locator("#standardGames")).ToHaveValueAsync("4");
        await Expect(page.Locator("#leagueYearName")).ToHaveValueAsync("UI Year Name");
        await Expect(DropRowCheckbox(page, "Will Release")).ToBeCheckedAsync();
        await Expect(page.Locator("#willReleaseDroppableGames")).ToHaveCountAsync(0);
        await Expect(page.Locator("#willNotReleaseDroppableGames")).ToHaveValueAsync("2");
    }

    [Test]
    public async Task ManageDraftsAddsEditsAndDeletesADraft()
    {
        var (league, managerUserID) = await NewLeagueAsync(LeagueScenarios.TwoPlayerSmall);
        var page = await NewSignedInPageAsync(managerUserID, $"/manageDrafts/{league.LeagueID}/{league.Year}");
        await Expect(page.GetByText("Draft 1:")).ToBeVisibleAsync();

        await page.GetByPlaceholder("e.g. Summer Draft").FillAsync("UI Second Draft");
        await Ui.InputAfterLabel(page.Locator("body"), "Games to Draft").FillAsync("1");
        await Ui.InputAfterLabel(page.Locator("body"), "Additional Standard Games").FillAsync("1");
        await page.GetByRole(AriaRole.Button, new() { Name = "Add Draft" }).ClickAsync();
        await Expect(page.GetByText("Draft 2: UI Second Draft")).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Button, new() { Name = "Edit", Exact = true }).Nth(1).ClickAsync();
        var nameInput = Ui.InputAfterLabel(page.Locator("body"), "Draft Name").First;
        await Expect(nameInput).ToHaveValueAsync("UI Second Draft");
        await nameInput.FillAsync("UI Renamed Draft");
        await page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();
        await Expect(page.GetByText("Draft 2: UI Renamed Draft")).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Button, new() { Name = "Delete", Exact = true }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Confirm Delete" }).ClickAsync();
        await Expect(page.GetByText("Draft 2:")).ToHaveCountAsync(0);

        var drafts = (await league.GetLeagueYearAsync()).Drafts;
        Assert.That(drafts.Select(draft => draft.Name), Has.None.EqualTo("UI Renamed Draft"));
    }
}
