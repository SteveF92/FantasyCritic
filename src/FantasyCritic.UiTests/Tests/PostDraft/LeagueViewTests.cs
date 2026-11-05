using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.IntegrationTests.Helpers;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests.PostDraft;

/// <summary>
/// Looking at a drafted league: the projections toggle, the read-only option and override modals, and moving a game
/// between slots on the publisher page.
/// </summary>
public sealed class LeagueViewTests : PostDraftTestBase
{
    [Test]
    public async Task ProjectionsToggleSwitchesTotals()
    {
        var (league, managerUserID) = await NewDraftedLeagueAsync();
        var page = await NewSignedInPageAsync(managerUserID, LeaguePath(league));
        var toggle = page.GetByText("Show Projections", new() { Exact = true }).Locator("xpath=following-sibling::label[1]");

        await Expect(page.GetByText("Total Fantasy Points").First).ToBeVisibleAsync();
        await Expect(toggle).ToContainTextAsync("Off");

        await toggle.ClickAsync();
        await Expect(toggle).ToContainTextAsync("On");
        await Expect(page.GetByText("Projected Fantasy Points")).ToHaveCountAsync(league.Publishers.Count);

        await toggle.ClickAsync();
        await Expect(toggle).ToContainTextAsync("Off");
        await Expect(page.GetByText("Projected Fantasy Points")).ToHaveCountAsync(0);
    }

    [Test]
    public async Task ReadOnlyModalsOpen()
    {
        var (league, _) = await NewDraftedLeagueAsync();
        var player = league.Publishers[1];
        var page = await NewSignedInPageAsync(await UserIDOfAsync(player), LeaguePath(league));

        await Ui.SidebarAction(page, "See League Options").ClickAsync();
        var options = Ui.Dialog(page, "League Options");
        await Expect(options.GetByRole(AriaRole.Row).Filter(new() { HasText = "Total Standard Games" })).ToContainTextAsync("4");
        await options.GetByRole(AriaRole.Button, new() { Name = "Close" }).ClickAsync();
        await Expect(options).ToBeHiddenAsync();

        await Ui.SidebarAction(page, "See Eligibility Overrides").ClickAsync();
        var eligibility = Ui.Dialog(page, "Eligibility Overrides");
        await Expect(eligibility.GetByRole(AriaRole.Columnheader, new() { Name = "Eligible?" })).ToBeVisibleAsync();
        await eligibility.GetByRole(AriaRole.Button, new() { Name = "Close" }).ClickAsync();
        await Expect(eligibility).ToBeHiddenAsync();

        await Ui.SidebarAction(page, "See Tag Overrides").ClickAsync();
        var tags = Ui.Dialog(page, "Tag Overrides");
        await Expect(tags.GetByRole(AriaRole.Columnheader, new() { Name = "Tags" })).ToBeVisibleAsync();
        await tags.GetByRole(AriaRole.Button, new() { Name = "Close" }).ClickAsync();
        await Expect(tags).ToBeHiddenAsync();
    }

    [Test]
    public async Task MoveAGameToAnotherSlot()
    {
        var scenario = SlotTestHelpers.Scenario("UiMoveMode", standardGames: 4, counterPicks: 1, specialSlotTags: [["NewGamingFranchise"]], gamesToDraft: 2);
        var (league, _) = await NewLeagueAsync(scenario, startDraft: true);
        await league.DraftToCompletionAsync();
        var player = league.Publishers[1];

        var slots = (await PublisherAsync(league, player)).GameSlots.Where(slot => !slot.CounterPick).OrderBy(slot => slot.SlotNumber).ToList();
        var moving = slots.First(slot => slot.PublisherGame is not null);
        var target = slots.First(slot => slot.PublisherGame is null && slot.SpecialSlot is null);
        var game = moving.PublisherGame!;

        var page = await NewSignedInPageAsync(await UserIDOfAsync(player), $"/publisher/{player.PublisherID}");
        await page.GetByRole(AriaRole.Button, new() { Name = "Move Games" }).ClickAsync();
        await page.GetByRole(AriaRole.Row).Filter(new() { HasText = game.GameName }).GetByRole(AriaRole.Button, new() { Name = "Move" }).ClickAsync();

        // Every standard slot row now offers "Here", in slot order.
        var here = page.GetByRole(AriaRole.Button, new() { Name = "Here", Exact = true });
        await Expect(here).ToHaveCountAsync(slots.Count);
        await here.Nth(target.SlotNumber).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Confirm Positions" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Move Games" })).ToBeVisibleAsync();

        var moved = SlotTestHelpers.GetSlotOf(await league.GetLeagueYearAsync(), game.PublisherGameID);
        Assert.That(moved.SlotNumber, Is.EqualTo(target.SlotNumber));
    }
}
