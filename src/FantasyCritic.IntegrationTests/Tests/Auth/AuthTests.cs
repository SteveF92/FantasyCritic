using System;
using System.Net;
using System.Threading.Tasks;
using FantasyCritic.IntegrationTests.Helpers;
using FantasyCritic.Lib.SharedSerialization.API;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.Auth;

[TestFixture]
public class AuthTests : IntegrationTestBase
{
    [Test]
    public async Task Factory_Boots_AndHealthEndpointResponds()
    {
        var client = Factory.CreateClient();
        var response = await client.GetAsync("/");
        Assert.That((int)response.StatusCode, Is.LessThan(500));
    }

    [Test]
    public async Task Register_AutoSignsIn_CurrentUser_ReturnsOk()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);

        // Registration with RequireConfirmedAccount=false signs the user in immediately.
        await session.RegisterAsync(email, password, displayName);

        var user = await session.GetAndDeserializeAsync<FantasyCriticUserViewModel>("/api/Account/CurrentUser");
        Assert.That(user.EmailAddress, Is.EqualTo(email).IgnoreCase);
    }

    [Test]
    public async Task Login_WithValidCredentials_CurrentUser_ReturnsOk()
    {
        var (email, password, displayName) = NewUser();

        // Register from session A (creates the user in the DB).
        using var sessionA = new ApiSession(Factory);
        await sessionA.RegisterAsync(email, password, displayName);

        // Log in from session B (fresh — no carry-over auth cookie from registration).
        using var sessionB = new ApiSession(Factory);
        var loginSucceeded = await sessionB.LoginAsync(email, password);
        Assert.That(loginSucceeded, Is.True, "Login with correct credentials should return a redirect.");

        var user = await sessionB.GetAndDeserializeAsync<FantasyCriticUserViewModel>("/api/Account/CurrentUser");
        Assert.That(user.EmailAddress, Is.EqualTo(email).IgnoreCase);
    }

    [Test]
    public async Task Login_WithWrongPassword_Fails()
    {
        var (email, password, displayName) = NewUser();

        using var registerSession = new ApiSession(Factory);
        await registerSession.RegisterAsync(email, password, displayName);

        using var session = new ApiSession(Factory);
        var loginSucceeded = await session.LoginAsync(email, "WrongPassword!!");
        Assert.That(loginSucceeded, Is.False, "Login with wrong password should not redirect.");
    }

    [Test]
    public async Task UnauthenticatedRequest_ToProtectedEndpoint_Returns401()
    {
        // Fresh session — never registered or logged in.
        using var session = new ApiSession(Factory);
        var response = await session.GetAsync("/api/Account/CurrentUser");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
