using System;
using System.Net;
using System.Threading.Tasks;
using FantasyCritic.IntegrationTests.Helpers;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.Auth;

[TestFixture]
public class AuthTests
{
    private FantasyCriticWebApplicationFactory _factory = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new FantasyCriticWebApplicationFactory();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _factory.Dispose();
    }

    /// <summary>
    /// Produces unique credentials so tests never collide across runs.
    /// Display name is capped at 20 chars (well within the 30-char limit).
    /// Password satisfies: min 10 chars, min 5 unique chars.
    /// </summary>
    private static (string email, string password, string displayName) NewUser()
    {
        var id = Guid.NewGuid().ToString("N")[..12];
        return (
            $"u-{id}@integrationtest.local",
            "IntegrationTestPass",
            $"T-{id}"                         // "T-" + 12 hex chars = 14 chars
        );
    }

    [Test]
    public async Task Factory_Boots_AndHealthEndpointResponds()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");
        Assert.That((int)response.StatusCode, Is.LessThan(500));
    }

    [Test]
    public async Task Register_AutoSignsIn_CurrentUser_ReturnsOk()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(_factory);

        // Registration with RequireConfirmedAccount=false signs the user in immediately.
        await session.RegisterAsync(email, password, displayName);

        var response = await session.GetAsync("/api/Account/CurrentUser");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(body);
        Assert.That(
            json["emailAddress"]?.ToString(),
            Is.EqualTo(email).IgnoreCase,
            "CurrentUser response should contain the registered email.");
    }

    [Test]
    public async Task Login_WithValidCredentials_CurrentUser_ReturnsOk()
    {
        var (email, password, displayName) = NewUser();

        // Register from session A (creates the user in the DB).
        using var sessionA = new ApiSession(_factory);
        await sessionA.RegisterAsync(email, password, displayName);

        // Log in from session B (fresh — no carry-over auth cookie from registration).
        using var sessionB = new ApiSession(_factory);
        var loginSucceeded = await sessionB.LoginAsync(email, password);
        Assert.That(loginSucceeded, Is.True, "Login with correct credentials should return a redirect.");

        var response = await sessionB.GetAsync("/api/Account/CurrentUser");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(body);
        Assert.That(
            json["emailAddress"]?.ToString(),
            Is.EqualTo(email).IgnoreCase);
    }

    [Test]
    public async Task Login_WithWrongPassword_Fails()
    {
        var (email, password, displayName) = NewUser();

        using var registerSession = new ApiSession(_factory);
        await registerSession.RegisterAsync(email, password, displayName);

        using var session = new ApiSession(_factory);
        var loginSucceeded = await session.LoginAsync(email, "WrongPassword!!");
        Assert.That(loginSucceeded, Is.False, "Login with wrong password should not redirect.");
    }

    [Test]
    public async Task UnauthenticatedRequest_ToProtectedEndpoint_Returns401()
    {
        // Fresh session — never registered or logged in.
        using var session = new ApiSession(_factory);
        var response = await session.GetAsync("/api/Account/CurrentUser");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
