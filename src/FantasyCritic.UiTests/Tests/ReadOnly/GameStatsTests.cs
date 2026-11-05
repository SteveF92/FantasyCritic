using System;
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
/// Advanced Game Stats' sections, Game Changes' filter buttons and Most Desired Reviews' release window.
/// </summary>
public sealed class GameStatsTests : UiTestBase
{
    [Test]
    public async Task SectionLinksSwitchPanels()
    {
        var page = await NewPageAsync();
        await page.GotoAsync("/advancedGameStats");
        await Expect(page.GetByText("This is a list of unreleased games")).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Link, new() { Name = "Longest Tenured (Released)" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex("#longestTenuredReleased$"));
        await Expect(page.GetByRole(AriaRole.Columnheader, new() { Name = "Days Before Release" })).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Link, new() { Name = "Most Dreams Dashed" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex("#mostDreamsDashed$"));
        await Expect(page.GetByText("Games that have dashed the most dreams")).ToBeVisibleAsync();

        await page.RunAndWaitForRequestAsync(
            () => page.GetByRole(AriaRole.Combobox, new() { Name = "Year" }).SelectOptionAsync("2025"),
            new Regex("/api/game/GetMostDreamsDashedGames\\?year=2025$"));
    }

    [Test]
    public async Task UnannouncedGamesShowOnlyWhenAsked()
    {
        var page = await NewPageAsync();
        await Ui.PlantJsonAsync(page, "**/api/game/GetLongestTenuredGames*", async route =>
        {
            var games = new JsonArray((await Ui.FetchJsonAsync(route)).AsArray().Take(3).Select(game => game!.DeepClone()).ToArray());
            games[0]!["masterGame"]!["gameName"] = "Unannounced Game";
            games[0]!["masterGame"]!["tags"] = new JsonArray("UnannouncedGame");
            return games;
        });

        await page.GotoAsync("/advancedGameStats");
        var rows = Ui.BodyRows(page.GetByRole(AriaRole.Table));
        await Expect(rows).ToHaveCountAsync(2);
        await Expect(page.GetByRole(AriaRole.Table)).Not.ToContainTextAsync("Unannounced Game");

        await page.GetByText("Include unannounced games").ClickAsync();
        await Expect(page.GetByRole(AriaRole.Checkbox, new() { Name = "Include unannounced games" })).ToBeCheckedAsync();
        await Expect(rows).ToHaveCountAsync(3);
        await Expect(page.GetByRole(AriaRole.Table)).ToContainTextAsync("Unannounced Game");
    }

    [Test]
    public async Task GameChangesButtonsFilterByMode()
    {
        var page = await NewPageAsync();
        await page.GotoAsync("/gameChanges");
        await Expect(Ui.BodyRows(page.GetByRole(AriaRole.Table)).First).ToBeVisibleAsync();

        await page.RunAndWaitForRequestAsync(
            () => page.GetByRole(AriaRole.Button, new() { Name = "New games" }).ClickAsync(),
            new Regex("/api/game/GetRecentMasterGameChanges\\?mode=NewGames$"));
        await Expect(page.GetByRole(AriaRole.Table)).ToBeVisibleAsync();

        await page.RunAndWaitForRequestAsync(
            () => page.GetByRole(AriaRole.Button, new() { Name = "Changes", Exact = true }).ClickAsync(),
            new Regex("/api/game/GetRecentMasterGameChanges\\?mode=Changes$"));
        await Expect(page.GetByRole(AriaRole.Table)).ToBeVisibleAsync();
    }

    [Test]
    public async Task MostDesiredReviewsFilterByReleaseWindow()
    {
        var user = await NewUserAsync();
        var year = await LeagueTestHelpers.GetOpenYearAsync(user.Session);
        var page = await NewPageAsync();
        var templates = (await Ui.GetJsonAsync(page, $"/api/game/MasterGameYear/{year}")).AsArray();
        Assert.That(templates, Has.Count.GreaterThanOrEqualTo(4), $"The local database has too few master games for {year}.");

        var today = DateTime.Today;
        var planted = new (string Name, int DaysAgo)[] { ("Week Game", 3), ("Month Game", 20), ("Quarter Game", 60), ("Year Game", 150) };
        var reviews = new JsonArray(planted.Select((game, index) =>
        {
            var masterGame = templates[index]!.DeepClone();
            masterGame["gameName"] = game.Name;
            masterGame["releaseDate"] = today.AddDays(-game.DaysAgo).ToString("yyyy-MM-dd");
            masterGame["tags"] = new JsonArray();
            return (JsonNode)new JsonObject { ["masterGame"] = masterGame, ["desireFactor"] = planted.Length - index };
        }).ToArray());
        await Ui.PlantJsonAsync(page, "**/api/game/GetMostDesiredReviews", _ => Task.FromResult<JsonNode>(reviews));

        await page.GotoAsync("/mostDesiredReviews");
        var table = page.GetByRole(AriaRole.Table);
        var rows = Ui.BodyRows(table);
        await Expect(rows).ToHaveCountAsync(4);

        foreach (var (window, expected) in new[] { ("Past week", 1), ("Past month", 2), ("Past 3 months", 3), ("All Year", 4) })
        {
            await page.GetByText(window, new() { Exact = true }).ClickAsync();
            await Expect(page.GetByRole(AriaRole.Radio, new() { Name = window })).ToBeCheckedAsync();
            await Expect(rows).ToHaveCountAsync(expected);
            Assert.That(await Ui.ColumnTextsAsync(table, "Name"), Is.EqualTo(planted.Take(expected).Select(game => game.Name)), window);
        }
    }
}
