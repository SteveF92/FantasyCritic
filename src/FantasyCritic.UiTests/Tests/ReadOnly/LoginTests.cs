using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests.ReadOnly;

/// <summary>
/// The server-rendered login form: its client-side validation, and handing over to the SPA, which then knows who is
/// signed in.
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

    [Test]
    public async Task ClientValidationStopsAnEmptyForm()
    {
        var page = await NewPageAsync();
        await page.GotoAsync("/Account/Login");
        var posts = new List<string>();
        page.Request += (_, request) =>
        {
            if (request.Method == "POST")
            {
                posts.Add(request.Url);
            }
        };

        await page.GetByRole(AriaRole.Button, new() { Name = "Log in", Exact = true }).ClickAsync();
        await Expect(page.GetByText("The Email field is required.").First).ToBeVisibleAsync();
        await Expect(page.GetByText("The Password field is required.").First).ToBeVisibleAsync();
        Assert.That(posts, Is.Empty, "The empty form was sent to the server instead of being stopped by client validation.");

        // jquery-validate checks a field again on keyup or focusout; Fill sends neither.
        await page.GetByLabel("Email").FillAsync("not an email");
        await page.GetByLabel("Email").PressAsync("Tab");
        await Expect(page.GetByText("The Email field is not a valid e-mail address.").First).ToBeVisibleAsync();
    }
}
