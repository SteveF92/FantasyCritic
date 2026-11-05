using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests;

/// <summary>
/// The suite's own plumbing: the build is served, sign-in works, and browser problems are caught.
/// </summary>
public sealed class HarnessTests : UiTestBase
{
    [Test]
    public async Task WelcomePageRendersLoggedOut()
    {
        var page = await NewPageAsync();
        await page.GotoAsync("/");

        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Registration is free!" })).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Link, new() { Name = "Sign up", Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    public async Task SignedInUserLandsOnHome()
    {
        var user = await NewUserAsync();
        var page = await NewSignedInPageAsync(user, "/");

        await Expect(page).ToHaveURLAsync(new Regex("/home$"));
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = $"Welcome {user.DisplayName}!" })).ToBeVisibleAsync();
    }

    [Test]
    public async Task BrowserProblemsAreCaught()
    {
        var page = await NewPageAsync();
        await page.GotoAsync("/");

        await page.EvaluateAsync("() => { console.error('deliberate console error'); setTimeout(() => { throw new Error('deliberate page exception'); }); }");
        await Ui.WaitUntilAsync(() => Task.FromResult(Problems.Count >= 2), "both problems to be reported");

        Assert.That(Problems, Has.Some.Contains("deliberate console error"));
        Assert.That(Problems, Has.Some.Contains("deliberate page exception"));
        ForgetProblems();
    }
}
