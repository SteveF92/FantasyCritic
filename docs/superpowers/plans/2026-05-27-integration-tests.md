# Integration Tests Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Stand up a `FantasyCritic.IntegrationTests` project that boots the ASP.NET Core pipeline via `WebApplicationFactory`, connects to a real MySQL instance, and proves the auth stack works end-to-end.

**Architecture:** `WebApplicationFactory<Program>` in-process; tests POST to Razor Pages for auth cookies, then call JSON API endpoints. The single production-code change is replacing the Postmark `IEmailSender` with a no-op in `ConfigureTestServices`. All other test differences (no Discord, no schedulers) are already gated by the existing `appsettings.json` defaults and `IsDevelopment()` checks.

**Tech Stack:** .NET 10, NUnit 4, `Microsoft.AspNetCore.Mvc.Testing`, `Newtonsoft.Json`, MySQL 8.4 (Docker)

---

## Pre-flight: start MySQL

Before running any integration tests, MySQL must be running with the schema migrated:

```powershell
docker compose -f infrastructure/docker-compose-mysql.yaml up -d
```

Wait for the `fantasycritic-database-updater` container to exit (it exits 0 when migrations are done). Only needs to be done once per dev session (the volume persists).

---

## File map

| Path | Action | Purpose |
|------|--------|---------|
| `src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj` | Create | Project definition |
| `src/FantasyCritic.slnx` | Modify | Add project to solution |
| `src/FantasyCritic.IntegrationTests/appsettings.Testing.json` | Create | Placeholder for local overrides |
| `src/FantasyCritic.IntegrationTests/NullEmailSender.cs` | Create | No-op `IEmailSender` |
| `src/FantasyCritic.IntegrationTests/FantasyCriticWebApplicationFactory.cs` | Create | Custom WAF |
| `src/FantasyCritic.IntegrationTests/Helpers/AntiForgeryHelper.cs` | Create | Extracts CSRF token from HTML |
| `src/FantasyCritic.IntegrationTests/Helpers/ApiSession.cs` | Create | Wraps `HttpClient` with register/login/request helpers |
| `src/FantasyCritic.IntegrationTests/Tests/Auth/AuthTests.cs` | Create | Four auth smoke tests |

---

## Key facts about the codebase (read before coding)

- `IEmailSender` (in `FantasyCritic.Lib.Interfaces`) has one method: `Task SendEmailAsync(string email, string subject, string htmlMessage)`. The Postmark implementation is registered as `services.AddScoped<IEmailSender>(_ => new PostmarkEmailSender(...))` in `HostingExtensions.cs:129`.
- Discord registration is already skipped when `BotToken == "secret"` (`HostingExtensions.cs:162`). The default `appsettings.json` has `"BotToken": "secret"`, so no override needed.
- Scheduled background tasks are already gated on `!environment.IsDevelopment()` (`HostingExtensions.cs:144`). Setting the environment to `"Development"` skips them automatically.
- `RequireConfirmedAccount = false` is already the production default (`HostingExtensions.cs:223`). No override needed.
- Password rules: minimum 10 characters, minimum 5 unique characters, no other requirements (`HostingExtensions.cs:229-233`). Use `"IntegrationTestPass"` (19 chars, many unique) as the test password throughout.
- Display name: max 30 characters (`Register.cshtml.cs:44`).
- The auth cookie is named `"FantasyCriticCookie"` and `SecurePolicy = SameAsRequest` in Development, so HTTP works fine in tests.

---

## Task 1: Project scaffolding

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj`
- Create: `src/FantasyCritic.IntegrationTests/appsettings.Testing.json`
- Modify: `src/FantasyCritic.slnx`

- [ ] **Step 1: Create the project file**

Create `src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <PropertyGroup>
    <WarningLevel>5</WarningLevel>
    <TreatWarningsAsErrors>True</TreatWarningsAsErrors>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.5" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.3.0" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
    <PackageReference Include="NUnit" Version="4.5.1" />
    <PackageReference Include="NUnit.Analyzers" Version="4.12.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="NUnit3TestAdapter" Version="6.2.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\FantasyCritic.Web\FantasyCritic.Web.csproj" />
  </ItemGroup>

  <ItemGroup>
    <!--
      Copy the web project's appsettings.json to the test output directory so the
      WebApplicationFactory can find it when it sets the content root to the output dir.
    -->
    <Content Include="..\FantasyCritic.Web\appsettings.json"
             CopyToOutputDirectory="PreserveNewest"
             Link="appsettings.json" />
    <Content Include="appsettings.Testing.json" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Create `appsettings.Testing.json`**

Create `src/FantasyCritic.IntegrationTests/appsettings.Testing.json`:

```json
{}
```

This file is intentionally empty — the web project's `appsettings.json` (copied to the test output directory via the csproj `<Content>` item) already has the correct MySQL connection string and `BotToken: "secret"` for local development. Add overrides here (or in the gitignored `appsettings.Testing.Local.json`) only when your local setup differs.

- [ ] **Step 3: Add project to solution**

Open `src/FantasyCritic.slnx` and add the new project inside the `<Folder Name="/Tests/">` block:

```xml
<Folder Name="/Tests/">
  <Project Path="FantasyCritic.FakeRepo/FantasyCritic.FakeRepo.csproj" />
  <Project Path="FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj" />
  <Project Path="FantasyCritic.Test/FantasyCritic.Test.csproj" />
</Folder>
```

- [ ] **Step 4: Verify the project builds**

```powershell
cd src
dotnet build FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj
```

Expected: `Build succeeded.` with 0 errors. The project has no source files yet so there's nothing to compile beyond the project reference.

- [ ] **Step 5: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/ src/FantasyCritic.slnx
git commit -m "Add FantasyCritic.IntegrationTests project scaffold."
```

---

## Task 2: NullEmailSender and WebApplicationFactory

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/NullEmailSender.cs`
- Create: `src/FantasyCritic.IntegrationTests/FantasyCriticWebApplicationFactory.cs`

- [ ] **Step 1: Create `NullEmailSender.cs`**

```csharp
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.IntegrationTests;

internal sealed class NullEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
        => Task.CompletedTask;
}
```

- [ ] **Step 2: Create `FantasyCriticWebApplicationFactory.cs`**

```csharp
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.MySQL.DapperTypeMaps;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FantasyCritic.IntegrationTests;

public sealed class FantasyCriticWebApplicationFactory : WebApplicationFactory<Program>
{
    // Called once before any instance is created — safe to call repeatedly (idempotent).
    static FantasyCriticWebApplicationFactory()
    {
        DapperNodaTimeSetup.SetupDapperNodaTimeMappings();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Use Development so Program.cs skips AWS Secrets Manager,
        // and HostingExtensions.cs skips scheduled background tasks.
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            // Load test-specific overrides on top of appsettings.json.
            // appsettings.Testing.json is copied to the test output dir by the csproj.
            var outputDir = Path.GetDirectoryName(
                typeof(FantasyCriticWebApplicationFactory).Assembly.Location)!;

            configBuilder.AddJsonFile(
                Path.Combine(outputDir, "appsettings.Testing.json"),
                optional: true,
                reloadOnChange: false);

            // Per-machine local overrides — gitignored, never committed.
            configBuilder.AddJsonFile(
                Path.Combine(outputDir, "appsettings.Testing.Local.json"),
                optional: true,
                reloadOnChange: false);
        });

        builder.ConfigureTestServices(services =>
        {
            // Replace the Postmark sender with a no-op so registration doesn't make
            // outbound HTTP calls. Everything else (Discord, schedulers, RequireConfirmedAccount)
            // is already handled by the Development environment + appsettings.json defaults.
            services.RemoveAll<IEmailSender>();
            services.AddScoped<IEmailSender, NullEmailSender>();
        });
    }
}
```

- [ ] **Step 3: Write a minimal smoke test to verify the factory boots**

Create `src/FantasyCritic.IntegrationTests/Tests/Auth/AuthTests.cs` with just one test:

```csharp
using System.Net;
using FantasyCritic.IntegrationTests;
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

    [Test]
    public async Task Factory_Boots_AndHealthEndpointResponds()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");
        // Any response (even 200 or a redirect) confirms the factory booted
        Assert.That((int)response.StatusCode, Is.LessThan(500));
    }
}
```

- [ ] **Step 4: Run the smoke test — expect it to PASS**

```powershell
cd src
dotnet test FantasyCritic.IntegrationTests/ --filter "Factory_Boots_AndHealthEndpointResponds" -v normal
```

Expected output contains `Passed! - Failed: 0`. If it fails with a connection error, confirm MySQL is running (`docker ps`). If it fails with a startup exception, read the test output — common causes are missing config keys or Dapper setup not running.

- [ ] **Step 5: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/
git commit -m "Add NullEmailSender and FantasyCriticWebApplicationFactory."
```

---

## Task 3: AntiForgeryHelper and ApiSession

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/Helpers/AntiForgeryHelper.cs`
- Create: `src/FantasyCritic.IntegrationTests/Helpers/ApiSession.cs`

- [ ] **Step 1: Create `AntiForgeryHelper.cs`**

```csharp
using System.Text.RegularExpressions;

namespace FantasyCritic.IntegrationTests.Helpers;

internal static class AntiForgeryHelper
{
    // Matches both attribute orderings:
    //   <input name="__RequestVerificationToken" ... value="TOKEN" ...>
    //   <input ... value="TOKEN" ... name="__RequestVerificationToken" ...>
    private static readonly Regex TokenByValue = new(
        @"<input[^>]*name=""__RequestVerificationToken""[^>]*value=""([^""]+)""",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex TokenByName = new(
        @"<input[^>]*value=""([^""]+)""[^>]*name=""__RequestVerificationToken""",
        RegexOptions.Compiled | RegexOptions.Singleline);

    public static string ExtractToken(string html)
    {
        var m = TokenByValue.Match(html);
        if (m.Success) return m.Groups[1].Value;

        m = TokenByName.Match(html);
        if (m.Success) return m.Groups[1].Value;

        throw new InvalidOperationException(
            "Could not find __RequestVerificationToken in the page HTML. " +
            "Confirm the GET response was a Razor Page with antiforgery enabled, " +
            "not a redirect or error page.");
    }
}
```

- [ ] **Step 2: Create `ApiSession.cs`**

```csharp
using System.Net;
using System.Text;
using FantasyCritic.IntegrationTests;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;

namespace FantasyCritic.IntegrationTests.Helpers;

/// <summary>
/// Encapsulates an <see cref="HttpClient"/> with cookie support so that
/// a single test user's session (register → login → API calls) stays together.
/// Create one per test (or per test class when setup is shared).
/// </summary>
internal sealed class ApiSession : IDisposable
{
    private readonly HttpClient _client;

    public ApiSession(FantasyCriticWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
        });
    }

    /// <summary>
    /// POSTs to the /Account/Register Razor Page.
    /// Because RequireConfirmedAccount=false, a successful registration
    /// also signs the user in and sets the auth cookie on this session.
    /// </summary>
    public async Task RegisterAsync(string email, string password, string displayName)
    {
        var token = await GetAntiForgeryTokenAsync("/Account/Register");

        var response = await _client.PostAsync("/Account/Register",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Input.DisplayName"] = displayName,
                ["Input.Email"] = email,
                ["Input.ConfirmEmail"] = email,
                ["Input.Password"] = password,
                ["Input.ConfirmPassword"] = password,
                ["__RequestVerificationToken"] = token,
            }));

        // Success = 302 redirect (to home or RegisterConfirmation)
        if (response.StatusCode is not (HttpStatusCode.Redirect or HttpStatusCode.Found))
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"RegisterAsync failed. Status: {response.StatusCode}. " +
                $"Body snippet: {body[..Math.Min(500, body.Length)]}");
        }
    }

    /// <summary>
    /// POSTs to the /Account/Login Razor Page.
    /// Returns true when login succeeded (HTTP 302), false when it failed
    /// (e.g. wrong password — the page re-renders with a 200).
    /// </summary>
    public async Task<bool> LoginAsync(string email, string password)
    {
        var token = await GetAntiForgeryTokenAsync("/Account/Login");

        var response = await _client.PostAsync("/Account/Login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Input.Email"] = email,
                ["Input.Password"] = password,
                ["__RequestVerificationToken"] = token,
            }));

        return response.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.Found;
    }

    public Task<HttpResponseMessage> GetAsync(string path)
        => _client.GetAsync(path);

    public Task<HttpResponseMessage> PostJsonAsync(string path, object body)
    {
        var json = JsonConvert.SerializeObject(body);
        return _client.PostAsync(path,
            new StringContent(json, Encoding.UTF8, "application/json"));
    }

    private async Task<string> GetAntiForgeryTokenAsync(string path)
    {
        var response = await _client.GetAsync(path);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        return AntiForgeryHelper.ExtractToken(html);
    }

    public void Dispose() => _client.Dispose();
}
```

- [ ] **Step 3: Verify everything compiles**

```powershell
cd src
dotnet build FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj
```

Expected: `Build succeeded.` 0 errors.

- [ ] **Step 4: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/
git commit -m "Add AntiForgeryHelper and ApiSession test helpers."
```

---

## Task 4: Auth smoke tests

**Files:**
- Modify: `src/FantasyCritic.IntegrationTests/Tests/Auth/AuthTests.cs`

- [ ] **Step 1: Write all four tests**

Replace the entire content of `AuthTests.cs`:

```csharp
using System.Net;
using FantasyCritic.IntegrationTests;
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
            json["email"]?.ToString(),
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
            json["email"]?.ToString(),
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
```

- [ ] **Step 2: Run the tests and verify they all pass**

```powershell
cd src
dotnet test FantasyCritic.IntegrationTests/ -v normal
```

Expected:
```
Passed  Register_AutoSignsIn_CurrentUser_ReturnsOk
Passed  Login_WithValidCredentials_CurrentUser_ReturnsOk
Passed  Login_WithWrongPassword_Fails
Passed  UnauthenticatedRequest_ToProtectedEndpoint_Returns401

Passed! - Failed: 0, Errors: 0, Skipped: 0
```

**Troubleshooting guide if tests fail:**

| Symptom | Likely cause | Fix |
|---|---|---|
| `MySqlException: Unable to connect` | MySQL not running | `docker compose -f infrastructure/docker-compose-mysql.yaml up -d` and wait for DatabaseUpdater to exit |
| `InvalidOperationException: RegisterAsync failed` with body containing validation errors | Password doesn't meet requirements | Ensure password is ≥10 chars with ≥5 unique chars |
| `Could not find __RequestVerificationToken` | GET returned a redirect or error | Check that the `/Account/Register` route is correct and the app starts cleanly |
| `KeyNotFoundException` on `json["email"]` | JSON property name differs | Print `body` in the test and check the actual property name returned by `CurrentUser` |
| 500 on first request | Startup exception | Run `dotnet run` in `FantasyCritic.Web/` with matching config and read the error |

- [ ] **Step 3: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/
git commit -m "Add auth smoke tests: register, login, current user, 401."
```

---

## Done

At this point:
- `FantasyCritic.IntegrationTests` builds and all four tests pass green
- The harness (WAF + `ApiSession`) is ready to extend — adding a league creation test is just a matter of calling `session.PostJsonAsync("/api/LeagueManager/CreateLeague", ...)` from a logged-in session
- No production code was changed except via `ConfigureTestServices` (the no-op email sender)
