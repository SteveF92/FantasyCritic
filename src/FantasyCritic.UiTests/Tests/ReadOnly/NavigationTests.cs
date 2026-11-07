using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests.ReadOnly;

/// <summary>
/// The router's redirects and history, and the top nav bar logged out, logged in and on a phone.
/// </summary>
public sealed class NavigationTests : UiTestBase
{
    // Signed in, because /404 isn't public: logged out, it goes to the login page.
    [Test]
    public async Task UnknownPathShowsNotFound()
    {
        var user = await NewUserAsync();
        var page = await NewSignedInPageAsync(user, "/no-such-page");

        await Expect(page).ToHaveURLAsync(new Regex("/404$"));
        await Expect(Ui.Heading(page, "Woops!")).ToBeVisibleAsync();
        await Ui.LinkButton(page, "Go back home").ClickAsync();
        await Expect(Ui.Heading(page, $"Welcome {user.DisplayName}!")).ToBeVisibleAsync();
    }

    [Test]
    public async Task LoggedOutPrivatePageGoesToLogin()
    {
        var page = await NewPageAsync();
        await page.GotoAsync("/home");

        await Expect(page).ToHaveURLAsync(new Regex("/Account/Login", RegexOptions.IgnoreCase));
        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Log in", Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    public async Task BackAndForwardMoveThroughSpaHistory()
    {
        var page = await NewPageAsync();
        await page.GotoAsync("/about");
        await Expect(Ui.Heading(page, "About")).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Navigation).GetByRole(AriaRole.Link, new() { Name = "FAQ" }).ClickAsync();
        await Expect(Ui.Heading(page, "Frequently Asked Questions")).ToBeVisibleAsync();

        await page.GoBackAsync();
        await Expect(page).ToHaveURLAsync(new Regex("/about$"));
        await Expect(Ui.Heading(page, "About")).ToBeVisibleAsync();

        await page.GoForwardAsync();
        await Expect(page).ToHaveURLAsync(new Regex("/faq$"));
        await Expect(Ui.Heading(page, "Frequently Asked Questions")).ToBeVisibleAsync();
    }

    [Test]
    public async Task LoggedOutNavOffersLogInAndSignUp()
    {
        var page = await NewPageAsync();
        await page.GotoAsync("/about");
        var nav = page.GetByRole(AriaRole.Navigation);

        await Expect(Ui.LinkButton(nav, "Log In")).ToHaveAttributeAsync("href", "/Account/Login");
        await Expect(Ui.LinkButton(nav, "Sign Up")).ToHaveAttributeAsync("href", "/Account/Register");

        await nav.GetByRole(AriaRole.Link, new() { Name = "Games" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex("/games$"));
        await Expect(Ui.Heading(page, "Master Games List")).ToBeVisibleAsync();
    }

    [Test]
    public async Task SignedInNavHasUserMenu()
    {
        var user = await NewUserAsync();
        var page = await NewSignedInPageAsync(user, "/about");
        var nav = page.GetByRole(AriaRole.Navigation);

        await Expect(Ui.LinkButton(nav, "Log In")).ToHaveCountAsync(0);
        var manageAccount = page.GetByRole(AriaRole.Menuitem, new() { Name = "Manage Account" });
        await Expect(manageAccount).ToBeHiddenAsync();

        await nav.GetByRole(AriaRole.Button, new() { Name = user.DisplayName }).ClickAsync();
        await Expect(manageAccount).ToBeVisibleAsync();
        await Expect(manageAccount).ToHaveAttributeAsync("href", "/Account/Manage");
        await Expect(page.GetByRole(AriaRole.Menuitem, new() { Name = "Log Off" })).ToHaveAttributeAsync("href", "/Account/Logout");
    }

    [Test]
    public async Task PhoneNavOpensFromTheToggler()
    {
        var page = await NewPageAsync(new ViewportSize { Width = 375, Height = 812 });
        await page.GotoAsync("/about");
        var howToPlay = page.GetByRole(AriaRole.Navigation).GetByRole(AriaRole.Link, new() { Name = "How to Play" });
        var toggler = page.GetByRole(AriaRole.Button, new() { Name = "Toggle navigation" });

        await Expect(howToPlay).ToBeHiddenAsync();
        await toggler.ClickAsync();
        await Expect(howToPlay).ToBeVisibleAsync();

        await toggler.ClickAsync();
        await Expect(howToPlay).ToBeHiddenAsync();

        await toggler.ClickAsync();
        await howToPlay.ClickAsync();
        await Expect(Ui.Heading(page, "How to Play")).ToBeVisibleAsync();
    }
}
