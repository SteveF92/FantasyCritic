using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests.ReadOnly;

/// <summary>
/// The Public Leagues table on planted leagues: sorted by followers, paged ten at a time, searched by name, and
/// reloaded for another year.
/// </summary>
public sealed class PublicLeaguesTests : UiTestBase
{
    private static readonly Guid LeagueID = Guid.NewGuid();

    private static JsonArray Leagues(string prefix, int count) =>
        new(Enumerable.Range(1, count).Select(number => (JsonNode)new JsonObject
        {
            ["leagueID"] = LeagueID.ToString(),
            ["leagueName"] = $"{prefix} {number:00}",
            ["numberOfFollowers"] = number,
            ["anyDraftStarted"] = true,
        }).ToArray());

    private async Task<(IPage Page, ILocator Year, string CurrentYear)> OpenAsync()
    {
        var page = await NewPageAsync();
        await Ui.PlantJsonAsync(page, "**/api/league/PublicLeagues/*", route =>
        {
            var year = new Uri(route.Request.Url).Segments.Last();
            return Task.FromResult<JsonNode>(year == "2025" ? Leagues("Older League", 3) : Leagues("League", 12));
        });

        await page.GotoAsync("/publicLeagues");
        var year = page.GetByRole(AriaRole.Combobox).Filter(new() { Has = page.GetByRole(AriaRole.Option, new() { Name = "2018", Exact = true }) });
        await Expect(Ui.BodyRows(page.GetByRole(AriaRole.Table))).ToHaveCountAsync(10);
        return (page, year, await year.InputValueAsync());
    }

    [Test]
    public async Task SortsByFollowersAndPages()
    {
        var (page, _, currentYear) = await OpenAsync();
        var table = page.GetByRole(AriaRole.Table);

        Assert.That(await Ui.ColumnTextsAsync(table, "Name"), Is.EqualTo(Enumerable.Range(3, 10).Reverse().Select(number => $"League {number:00}")));
        await Expect(table.GetByRole(AriaRole.Link, new() { Name = "League 12" })).ToHaveAttributeAsync("href", $"/league/{LeagueID}/{currentYear}");

        await Ui.PageButton(page, 2).ClickAsync();
        await Expect(Ui.BodyRows(table)).ToHaveCountAsync(2);
        Assert.That(await Ui.ColumnTextsAsync(table, "Name"), Is.EqualTo(new[] { "League 02", "League 01" }));
    }

    [Test]
    public async Task SearchFiltersByName()
    {
        var (page, _, _) = await OpenAsync();
        var table = page.GetByRole(AriaRole.Table);

        await page.GetByRole(AriaRole.Searchbox).FillAsync("League 1");
        await Expect(Ui.BodyRows(table)).ToHaveCountAsync(3);
        Assert.That(await Ui.ColumnTextsAsync(table, "Name"), Is.EqualTo(new[] { "League 12", "League 11", "League 10" }));
    }

    [Test]
    public async Task YearSelectLoadsThatYearsLeagues()
    {
        var (page, year, _) = await OpenAsync();
        var table = page.GetByRole(AriaRole.Table);

        await page.RunAndWaitForRequestAsync(() => year.SelectOptionAsync("2025"), new Regex("/api/league/PublicLeagues/2025$"));
        await Expect(Ui.BodyRows(table)).ToHaveCountAsync(3);
        Assert.That(await Ui.ColumnTextsAsync(table, "Name"), Is.EqualTo(new[] { "Older League 03", "Older League 02", "Older League 01" }));
        await Expect(table.GetByRole(AriaRole.Link, new() { Name = "Older League 03" })).ToHaveAttributeAsync("href", $"/league/{LeagueID}/2025");
    }
}
