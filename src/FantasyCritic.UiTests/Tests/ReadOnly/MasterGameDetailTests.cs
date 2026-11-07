using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FantasyCritic.IntegrationTests.Helpers;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests.ReadOnly;

/// <summary>
/// A master game's year stats: the year menu, the Peak Hype Factor popover and the statistics chart. The local
/// database has no year stats, so they are planted.
/// </summary>
public sealed class MasterGameDetailTests : UiTestBase
{
    private static JsonObject YearStats(int year, decimal hypeFactor, decimal peakHypeFactor) => new()
    {
        ["year"] = year,
        ["eligiblePercentStandardGame"] = 0.25,
        ["averageDraftPosition"] = 4.5,
        ["dateAdjustedHypeFactor"] = hypeFactor,
        ["peakHypeFactor"] = peakHypeFactor,
        ["projectedFantasyPoints"] = 12.5,
        ["adjustedPercentCounterPick"] = 0.1,
        ["percentCounterPick"] = 0.05,
    };

    private static JsonObject Statistics(int year) => new()
    {
        ["statistics"] = new JsonArray(Enumerable.Range(1, 6).Select(month => (JsonNode)new JsonObject
        {
            ["date"] = $"{year}-{month:00}-01",
            ["eligiblePercentStandardGame"] = 0.05 * month,
            ["percentCounterPick"] = 0.01 * month,
            ["dateAdjustedHypeFactor"] = 5 * month,
            ["projectedFantasyPoints"] = 2 * month,
            ["royaleCost"] = 10 + month,
        }).ToArray()),
    };

    [Test]
    public async Task YearStatsPopoverAndChart()
    {
        var user = await NewUserAsync();
        var openYear = await LeagueTestHelpers.GetOpenYearAsync(user.Session);
        var previousYear = openYear - 1;
        var page = await NewPageAsync();
        var masterGameID = (await Ui.GetJsonAsync(page, $"/api/game/MasterGameYear/{openYear}")).AsArray()[0]!["masterGameID"]!.GetValue<string>();

        await Ui.PlantJsonAsync(page, $"**/api/game/MasterGameYears/{masterGameID}",
            _ => Task.FromResult<JsonNode>(new JsonArray(YearStats(previousYear, 20m, 20m), YearStats(openYear, 10m, 50m))));
        await Ui.PlantJsonAsync(page, $"**/api/game/MasterGameYearWithStatistics/{masterGameID}/*",
            route => Task.FromResult<JsonNode>(Statistics(int.Parse(route.Request.Url.Split('/').Last()))));

        await page.GotoAsync($"/mastergame/{masterGameID}");
        var stats = page.GetByRole(AriaRole.Listitem);
        await Expect(stats.Filter(new() { HasText = "Hype Factor: 10" }).First).ToBeVisibleAsync();
        var peak = stats.Filter(new() { HasText = "Peak Hype Factor: 50" });
        await Expect(peak).ToBeVisibleAsync();

        await peak.Locator("svg").HoverAsync();
        var popover = Ui.Popover(page);
        await Expect(popover).ToBeVisibleAsync();
        await Expect(popover).ToContainTextAsync("Peak Hype Factor");
        await Expect(popover).ToContainTextAsync("This number is the highest this game's hype factor ever was in the year.");
        await Expect(popover).Not.ToContainTextAsync("=>");

        await Expect(Ui.Heading(page, "Statistics over time")).ToBeVisibleAsync();
        var chart = page.Locator("canvas");
        await Expect(chart).ToHaveCountAsync(1);
        await Ui.WaitUntilAsync(async () => await Ui.DrawnPixelsAsync(chart) > 1000, "the statistics chart to draw");

        // Found by its text, because bootstrap-vue-next names the toggle with the dropdown's aria-label. The closed menu's
        // items are buttons with the same text, so only the visible one is the toggle.
        ILocator YearToggle(int year) => Ui.ButtonElement(page.Locator("body"), year.ToString()).Filter(new() { Visible = true });
        await YearToggle(openYear).ClickAsync();
        await page.RunAndWaitForRequestAsync(
            () => page.GetByRole(AriaRole.Menuitem, new() { Name = previousYear.ToString() }).ClickAsync(),
            new Regex($"/api/game/MasterGameYearWithStatistics/{masterGameID}/{previousYear}$"));
        await Expect(stats.Filter(new() { HasText = "Hype Factor: 20" }).First).ToBeVisibleAsync();
        await Expect(stats.Filter(new() { HasText = "Peak Hype Factor" })).ToHaveCountAsync(0);
        await Expect(page.GetByRole(AriaRole.Menuitem, new() { Name = previousYear.ToString() })).ToBeHiddenAsync();
        await Expect(YearToggle(previousYear)).ToBeVisibleAsync();
    }
}
