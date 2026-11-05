using System;
using System.Threading.Tasks;
using FantasyCritic.IntegrationTests.Helpers;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests.Requests;

/// <summary>
/// The master game request and correction forms: their validation (vee-validate), the date input, submitting, and
/// cancelling the request from the user's list.
/// </summary>
public sealed class MasterGameRequestTests : UiTestBase
{
    [Test]
    public async Task RequestAGameThenCancelIt()
    {
        var user = await NewUserAsync();
        var page = await NewSignedInPageAsync(user, "/masterGameRequest");
        var gameName = $"UI Requested Game {Guid.NewGuid():N}"[..26];
        var submit = page.GetByRole(AriaRole.Button, new() { Name = "Submit" });
        await Expect(Ui.Heading(page, "Master Game Request")).ToBeVisibleAsync();
        await Expect(submit).ToBeDisabledAsync();

        await page.Locator("#gameName").FillAsync(gameName);
        await page.GetByText("Does this game have a confirmed release date?").ClickAsync();
        await page.Locator("#releaseDate").FillAsync("2027-03-15");
        await Expect(page.Locator("#releaseDate")).ToHaveValueAsync("2027-03-15");

        // The note must be at least 30 characters.
        await page.Locator("#requestNote").FillAsync("Too short.");
        await Expect(submit).ToBeDisabledAsync();

        // A rumored game needs the warning acknowledged.
        await page.Locator("#requestNote").FillAsync("This is a rumor from a reliable industry source.");
        await Expect(page.GetByText("It looks like you are requesting an unannounced or rumored game.")).ToBeVisibleAsync();
        await Expect(submit).ToBeDisabledAsync();
        await page.GetByText("I have read and understand this message.").ClickAsync();
        await Expect(submit).ToBeEnabledAsync();

        await page.Locator("#requestNote").FillAsync("Announced at a showcase this week, with a trailer and a date.");
        await Expect(submit).ToBeEnabledAsync();
        await submit.ClickAsync();

        await Expect(page.GetByText("Master Game request made.")).ToBeVisibleAsync();
        await Expect(page.Locator("#gameName")).ToHaveValueAsync("");
        await Expect(page.Locator("#requestNote")).ToHaveValueAsync("");
        await Expect(page.Locator("#gameName + span")).ToBeEmptyAsync();
        await Expect(page.Locator("#requestNote + span")).ToBeEmptyAsync();

        var row = page.GetByRole(AriaRole.Row).Filter(new() { HasText = gameName });
        await Expect(row).ToBeVisibleAsync();
        await row.GetByRole(AriaRole.Button, new() { Name = "Cancel Request" }).ClickAsync();
        await Expect(page.GetByText("Master Game request was deleted.")).ToBeVisibleAsync();
        await Expect(row).ToHaveCountAsync(0);
    }

    [Test]
    public async Task SuggestACorrectionThenCancelIt()
    {
        var user = await NewUserAsync();
        var year = await LeagueTestHelpers.GetOpenYearAsync(user.Session);
        var page = await NewPageAsync();
        var game = (await Ui.GetJsonAsync(page, $"/api/game/MasterGameYear/{year}")).AsArray()[0]!;
        var gameName = game["gameName"]!.GetValue<string>();
        await page.GotoAsync($"/dev/sign-in-as?userId={user.UserID}&returnUrl={Uri.EscapeDataString($"/masterGameChangeRequest?mastergameid={game["masterGameID"]}")}");

        await Expect(Ui.Heading(page, gameName, 2)).ToBeVisibleAsync();
        var submit = page.GetByRole(AriaRole.Button, new() { Name = "Submit" });
        await Expect(submit).ToBeDisabledAsync();
        await page.Locator("#requestNote").FillAsync("The release date moved to next spring.");
        await Expect(submit).ToBeEnabledAsync();
        await submit.ClickAsync();

        await Expect(page.GetByText("Master Game request made.")).ToBeVisibleAsync();
        var row = page.GetByRole(AriaRole.Row).Filter(new() { HasText = "The release date moved to next spring." });
        await Expect(row).ToBeVisibleAsync();
        await row.GetByRole(AriaRole.Button, new() { Name = "Cancel Request" }).ClickAsync();
        await Expect(page.GetByText("Master Game request was deleted.")).ToBeVisibleAsync();
        await Expect(row).ToHaveCountAsync(0);
    }
}
