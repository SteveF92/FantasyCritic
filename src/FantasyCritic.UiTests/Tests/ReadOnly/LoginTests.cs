using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests.ReadOnly;

/// <summary>
/// The server-rendered login form hands over to the SPA, which then knows who is signed in.
/// </summary>
public sealed class LoginTests : UiTestBase
{
    [Test]
    public async Task LoginFormSignsInToTheSpa()
    {
        var user = await NewUserAsync();
        var page = await NewPageAsync();
        await page.GotoAsync("/Account/Login");

        await page.GetByLabel("Email").FillAsync(user.Email);
        await page.GetByLabel("Password").FillAsync(user.Password);
        await page.GetByRole(AriaRole.Button, new() { Name = "Log in", Exact = true }).ClickAsync();

        await Expect(page).ToHaveURLAsync(new Regex("/home$"));
        await Expect(Ui.Heading(page, $"Welcome {user.DisplayName}!")).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Navigation).GetByRole(AriaRole.Button, new() { Name = user.DisplayName })).ToBeVisibleAsync();
    }
}
