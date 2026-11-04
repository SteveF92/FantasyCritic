using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FantasyCritic.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.Auth;

[TestFixture]
public class DevSignInTests : IntegrationTestBase
{
    [Test]
    public async Task SignInAs_ExistingUser_SignsInAsThatUser()
    {
        var userId = await RegisterUser();

        using var session = new ApiSession(Factory);
        await session.SignInAsAsync(userId);

        var currentUser = await session.Account.CurrentUserAsync();
        Assert.That(currentUser.UserID, Is.EqualTo(userId));
    }

    [Test]
    public async Task SignInAs_UnknownUser_Returns404()
    {
        using var session = new ApiSession(Factory);
        var response = await session.GetAsync($"/dev/sign-in-as?userId={Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task SignInAs_DeletedUser_Returns404()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);
        var userId = (await session.Account.CurrentUserAsync()).UserID;

        const string deletePage = "/Account/Manage/DeletePersonalData";
        var token = await session.GetPageAntiForgeryTokenAsync(deletePage);
        var deleteResponse = await session.PostFormAsync(deletePage, new Dictionary<string, string>
        {
            ["Input.Password"] = password,
            ["__RequestVerificationToken"] = token,
        });
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.Redirect), "Deleting the account should redirect home.");

        using var freshSession = new ApiSession(Factory);
        var response = await freshSession.GetAsync($"/dev/sign-in-as?userId={userId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task SignInAs_FromAnotherMachine_Returns404()
    {
        var userId = await RegisterUser();

        using var client = Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add(TestRemoteAddressStartupFilter.HeaderName, "192.168.1.50");
        var response = await client.GetAsync($"/dev/sign-in-as?userId={userId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [TestCase("/admin", "/admin")]
    [TestCase("/league/123?year=2026", "/league/123?year=2026")]
    [TestCase("//evil.example.com", "/")]
    [TestCase("/\\evil.example.com", "/")]
    [TestCase("https://evil.example.com/", "/")]
    [TestCase("", "/")]
    public async Task SignInAs_RedirectsOnlyToLocalPaths(string returnUrl, string expectedLocation)
    {
        var userId = await RegisterUser();

        using var session = new ApiSession(Factory);
        var response = await session.GetAsync($"/dev/sign-in-as?userId={userId}&returnUrl={Uri.EscapeDataString(returnUrl)}");

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
            Assert.That(response.Headers.Location?.OriginalString, Is.EqualTo(expectedLocation));
        });
    }

    private async Task<Guid> RegisterUser()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);
        var user = await session.Account.CurrentUserAsync();
        return user.UserID;
    }
}
