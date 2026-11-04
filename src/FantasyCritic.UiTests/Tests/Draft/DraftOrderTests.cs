using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.IntegrationTests.Helpers;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests.Draft;

/// <summary>
/// Setting the draft order by dragging the publishers, and randomly.
/// </summary>
public sealed class DraftOrderTests : DraftTestBase
{
    [Test]
    public async Task ManualOrderByDragAndDrop()
    {
        var (league, managerUserID) = await NewLeagueAsync(LeagueScenarios.TwoPlayerSmall);
        var first = league.Publishers.Single(publisher => publisher.DraftPosition == 1);
        var second = league.Publishers.Single(publisher => publisher.DraftPosition == 2);
        var page = await NewSignedInPageAsync(managerUserID, $"/league/{league.LeagueID}/{league.Year}");

        await Ui.SidebarAction(page, "Edit Draft Order").ClickAsync();
        var dialog = Ui.Dialog(page, "Set Draft Order");
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Manually" }).ClickAsync();
        var items = dialog.GetByRole(AriaRole.Listitem);
        await Expect(items.First).ToContainTextAsync(first.PublisherName);

        var secondItem = items.Filter(new() { HasText = second.PublisherName });
        await Ui.DragAsync(page, secondItem.Locator("svg").First, items.Filter(new() { HasText = first.PublisherName }));
        await Expect(items.First).ToContainTextAsync(second.PublisherName);
        await Expect(items.First).ToContainTextAsync("1");

        await dialog.GetByRole(AriaRole.Button, new() { Name = "Confirm Draft Order" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();
        await Expect(page.GetByText("Draft order has been changed.")).ToBeVisibleAsync();

        var publishers = (await league.GetLeagueYearAsync()).Publishers;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(publishers.Single(publisher => publisher.PublisherID == second.PublisherID).DraftPosition, Is.EqualTo(1));
            Assert.That(publishers.Single(publisher => publisher.PublisherID == first.PublisherID).DraftPosition, Is.EqualTo(2));
        }
    }

    [Test]
    public async Task RandomOrderIsSet()
    {
        var (league, managerUserID) = await NewLeagueAsync(LeagueScenarios.TwoPlayerSmall);
        var page = await NewSignedInPageAsync(managerUserID, $"/league/{league.LeagueID}/{league.Year}");

        await Ui.SidebarAction(page, "Edit Draft Order").ClickAsync();
        var dialog = Ui.Dialog(page, "Set Draft Order");
        await page.RunAndWaitForRequestAsync(
            () => dialog.GetByRole(AriaRole.Button, new() { Name = "Randomly" }).ClickAsync(),
            new System.Text.RegularExpressions.Regex("/api/leagueManager/SetDraftOrder$", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
        await Expect(dialog).ToBeHiddenAsync();
        await Expect(page.GetByText("Draft order has been changed.")).ToBeVisibleAsync();
        await Expect(page.GetByText("Draft order is set.")).ToBeVisibleAsync();
    }
}
