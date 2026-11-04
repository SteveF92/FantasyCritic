using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace FantasyCritic.UiTests;

/// <summary>
/// Locators and actions shared by the UI tests. Anything that has to know how bootstrap-vue draws something lives
/// here, so a change of UI library is fixed in one place.
/// </summary>
public static class Ui
{
    public static ILocator Heading(IPage page, string name, int? level = null) =>
        page.GetByRole(AriaRole.Heading, new() { Name = name, Exact = true, Level = level });

    /// <summary>
    /// The open popover. Both bootstrap-vue and bootstrap-vue-next give popovers the tooltip role.
    /// </summary>
    public static ILocator Popover(IPage page) => page.GetByRole(AriaRole.Tooltip);

    public static ILocator BodyRows(ILocator table) => table.Locator("tbody tr");

    /// <summary>
    /// The trimmed text of one column's cells, in row order, found by the column's header text.
    /// </summary>
    public static Task<string[]> ColumnTextsAsync(ILocator table, string header) =>
        table.EvaluateAsync<string[]>(
            """
            (table, header) => {
              const headers = [...table.querySelectorAll('thead th')];
              const index = headers.findIndex(th => th.textContent.trim().startsWith(header));
              if (index < 0) {
                throw new Error(`No column headed ${header}`);
              }
              return [...table.querySelectorAll('tbody tr')].map(row => row.children[index].textContent.trim());
            }
            """,
            header);

    /// <summary>
    /// Answers requests matching the glob with JSON the test builds, so a page can be shown data the local database
    /// doesn't have. The builder can fetch the real response first with <see cref="FetchJsonAsync"/>.
    /// </summary>
    public static Task PlantJsonAsync(IPage page, string urlGlob, Func<IRoute, Task<JsonNode>> respond) =>
        page.RouteAsync(urlGlob, async route =>
        {
            var json = await respond(route);
            await route.FulfillAsync(new RouteFulfillOptions
            {
                Status = 200,
                ContentType = "application/json",
                Body = json.ToJsonString(),
            });
        });

    public static async Task<JsonNode> FetchJsonAsync(IRoute route)
    {
        var response = await route.FetchAsync();
        return JsonNode.Parse(await response.TextAsync()) ?? throw new InvalidOperationException($"{route.Request.Url} returned no JSON.");
    }

    public static async Task<JsonNode> GetJsonAsync(IPage page, string path)
    {
        var response = await page.APIRequest.GetAsync(path);
        if (!response.Ok)
        {
            throw new InvalidOperationException($"GET {path} returned {response.Status}.");
        }

        return JsonNode.Parse(await response.TextAsync()) ?? throw new InvalidOperationException($"{path} returned no JSON.");
    }

    /// <summary>
    /// How many pixels of the canvas are not transparent, so zero means the chart drew nothing.
    /// </summary>
    public static Task<int> DrawnPixelsAsync(ILocator canvas) =>
        canvas.EvaluateAsync<int>(
            """
            canvas => {
              const pixels = canvas.getContext('2d').getImageData(0, 0, canvas.width, canvas.height).data;
              let drawn = 0;
              for (let alpha = 3; alpha < pixels.length; alpha += 4) {
                if (pixels[alpha] > 0) {
                  drawn++;
                }
              }
              return drawn;
            }
            """);

    /// <summary>
    /// For conditions Playwright's assertions can't express, such as reading a canvas or the test's own state.
    /// </summary>
    public static async Task WaitUntilAsync(Func<Task<bool>> condition, string description, int timeoutMilliseconds = 5000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
        while (!await condition())
        {
            if (DateTime.UtcNow > deadline)
            {
                throw new TimeoutException($"Waited {timeoutMilliseconds} ms for {description}.");
            }

            await Task.Delay(50);
        }
    }

    public static IReadOnlyList<decimal> Numbers(IEnumerable<string> texts) => texts.Select(text => decimal.Parse(text, CultureInfo.InvariantCulture)).ToList();
}
