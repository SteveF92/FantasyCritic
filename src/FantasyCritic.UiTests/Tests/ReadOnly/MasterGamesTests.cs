using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests.ReadOnly;

/// <summary>
/// The Master Games table: sorting, filtering, paging, the year select and the popovers. Sorting runs on planted rows,
/// since every local hype factor is 0.
/// </summary>
public sealed class MasterGamesTests : UiTestBase
{
    // Decimals of different lengths, so that comparing them as strings puts 91.0912 above 91.56.
    private static readonly (string Name, decimal HypeFactor)[] PlantedGames =
    [
        ("Delta Game", 91.0912m),
        ("Alpha Game", 100.27m),
        ("Foxtrot Game", 9.54m),
        ("Charlie Game", 91.56m),
        ("Echo Game", 0.3333m),
        ("Bravo Game", 45m),
    ];

    private async Task<IPage> OpenWithPlantedGamesAsync()
    {
        var page = await NewPageAsync();
        await Ui.PlantJsonAsync(page, "**/api/game/MasterGameYear/*", async route =>
        {
            var realGames = (await Ui.FetchJsonAsync(route)).AsArray();
            var planted = new JsonArray();
            foreach (var ((name, hypeFactor), realGame) in PlantedGames.Zip(realGames))
            {
                var game = realGame!.DeepClone();
                game["gameName"] = name;
                game["dateAdjustedHypeFactor"] = hypeFactor;
                planted.Add(game);
            }

            return planted;
        });

        await page.GotoAsync("/games");
        await Expect(Ui.BodyRows(page.GetByRole(AriaRole.Table))).ToHaveCountAsync(PlantedGames.Length);
        return page;
    }

    [Test]
    public async Task HypeFactorSortsNumerically()
    {
        var page = await OpenWithPlantedGamesAsync();
        var table = page.GetByRole(AriaRole.Table);

        var descending = Ui.Numbers(await Ui.ColumnTextsAsync(table, "Hype Factor"));
        Assert.That(descending, Is.EqualTo(new[] { 100.3m, 91.6m, 91.1m, 45m, 9.5m, 0.3m }));

        await table.GetByRole(AriaRole.Columnheader, new() { Name = "Hype Factor" }).ClickAsync();
        await Expect(table.Locator("tbody tr").First).ToContainTextAsync("Echo Game");
        var ascending = Ui.Numbers(await Ui.ColumnTextsAsync(table, "Hype Factor"));
        Assert.That(ascending, Is.EqualTo(new[] { 0.3m, 9.5m, 45m, 91.1m, 91.6m, 100.3m }));
    }

    [Test]
    public async Task SortSelectsChooseColumnAndDirection()
    {
        var page = await OpenWithPlantedGamesAsync();
        var table = page.GetByRole(AriaRole.Table);
        var direction = page.GetByRole(AriaRole.Combobox).Filter(new() { Has = page.GetByRole(AriaRole.Option, new() { Name = "Desc" }) });

        await page.GetByRole(AriaRole.Combobox, new() { Name = "Sort" }).SelectOptionAsync(new SelectOptionValue { Label = "Name" });
        await direction.SelectOptionAsync(new SelectOptionValue { Label = "Asc" });

        await Expect(table.Locator("tbody tr").First).ToContainTextAsync("Alpha Game");
        Assert.That(await Ui.ColumnTextsAsync(table, "Name"), Is.EqualTo(new[] { "Alpha Game", "Bravo Game", "Charlie Game", "Delta Game", "Echo Game", "Foxtrot Game" }));

        await direction.SelectOptionAsync(new SelectOptionValue { Label = "Desc" });
        await Expect(table.Locator("tbody tr").First).ToContainTextAsync("Foxtrot Game");
    }

    [Test]
    public async Task FilterNarrowsTheRows()
    {
        var page = await OpenWithPlantedGamesAsync();
        var rows = Ui.BodyRows(page.GetByRole(AriaRole.Table));

        await page.GetByRole(AriaRole.Searchbox, new() { Name = "Filter" }).FillAsync("charlie");
        await Expect(rows).ToHaveCountAsync(1);
        await Expect(rows).ToContainTextAsync("Charlie Game");

        await page.GetByRole(AriaRole.Searchbox, new() { Name = "Filter" }).FillAsync("");
        await Expect(rows).ToHaveCountAsync(PlantedGames.Length);
    }

    [Test]
    public async Task PagesThroughTheRealList()
    {
        var page = await NewPageAsync();
        await page.GotoAsync("/games");
        var rows = Ui.BodyRows(page.GetByRole(AriaRole.Table));
        await Expect(rows).ToHaveCountAsync(50);

        await page.GetByRole(AriaRole.Combobox, new() { Name = "Per page" }).SelectOptionAsync("10");
        await Expect(rows).ToHaveCountAsync(10);
        var firstOnPageOne = await rows.First.TextContentAsync();

        await page.GetByRole(AriaRole.Menuitemradio, new() { Name = "Go to page 2" }).First.ClickAsync();
        await Expect(page.GetByRole(AriaRole.Menuitemradio, new() { Name = "Go to page 2" }).First).ToBeCheckedAsync();
        await Expect(rows.First).Not.ToHaveTextAsync(firstOnPageOne!);
        await Expect(rows).ToHaveCountAsync(10);
    }

    [Test]
    public async Task YearSelectLoadsThatYear()
    {
        var page = await NewPageAsync();
        await page.GotoAsync("/games");
        var year = page.GetByRole(AriaRole.Combobox).Filter(new() { Has = page.GetByRole(AriaRole.Option, new() { Name = "2018", Exact = true }) });
        await Expect(Ui.BodyRows(page.GetByRole(AriaRole.Table)).First).ToBeVisibleAsync();

        var currentYear = await year.InputValueAsync();

        // The local database only has master games for the current year, so another year's table is empty.
        await page.RunAndWaitForRequestAsync(() => year.SelectOptionAsync("2025"), new Regex("/api/game/MasterGameYear/2025$"));
        await Expect(year).ToHaveValueAsync("2025");
        await Expect(Ui.BodyRows(page.GetByRole(AriaRole.Table))).ToHaveCountAsync(0);

        await page.RunAndWaitForRequestAsync(() => year.SelectOptionAsync(currentYear), new Regex($"/api/game/MasterGameYear/{currentYear}$"));
        await Expect(Ui.BodyRows(page.GetByRole(AriaRole.Table))).ToHaveCountAsync(50);
    }

    [Test]
    public async Task GamePopoverOpensAndCloses()
    {
        var page = await OpenWithPlantedGamesAsync();

        await page.GetByRole(AriaRole.Link, new() { Name = "Charlie Game" }).ClickAsync();
        var popover = Ui.Popover(page);
        await Expect(popover).ToBeVisibleAsync();
        await Expect(popover.GetByRole(AriaRole.Heading, new() { Name = "Charlie Game" })).ToBeVisibleAsync();

        await Ui.Heading(page, "Master Games List").ClickAsync();
        await Expect(popover).ToBeHiddenAsync();
    }

    [Test]
    public async Task TagPopoverOpensOnHover()
    {
        var page = await NewPageAsync();
        await page.GotoAsync("/games");
        await page.GetByRole(AriaRole.Table).GetByText("NGF", new() { Exact = true }).First.HoverAsync();
        var popover = Ui.Popover(page);
        await Expect(popover).ToBeVisibleAsync();
        await Expect(popover).ToContainTextAsync("New Gaming Franchise");

        await Ui.Heading(page, "Master Games List").HoverAsync();
        await Expect(popover).ToBeHiddenAsync();
    }
}
