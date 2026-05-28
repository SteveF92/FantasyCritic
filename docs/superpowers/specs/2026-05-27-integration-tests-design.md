# End-to-End Integration Tests — Design Spec

**Date:** 2026-05-27  
**Status:** Approved, pending implementation

---

## Goal

Add a suite of end-to-end integration tests that exercise the FantasyCritic HTTP API from the outside (HTTP request → controller → service → MySQL → response). Tests must:

- Run locally from a cold start (no pre-existing data beyond a freshly migrated schema)
- Be deterministic and isolated — no shared mutable state between tests
- Cover auth and the core user journey (register, login, league creation, etc.)
- Not require any external secrets (OAuth providers, Postmark, Discord, AWS)

## Approach: `WebApplicationFactory` (in-process)

Uses `Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>` to boot the full ASP.NET Core pipeline inside the test runner. Tests speak to it over an in-memory HTTP transport. The real MySQL instance (via Docker) is used for persistence — there is no in-memory database substitute, because the codebase relies on MySQL stored procedures.

OpenAPI/Swashbuckle is explicitly **not** part of this effort. Tests use raw `HttpClient` + JSON, not a generated typed client.

---

## Prerequisites (developer machine)

MySQL must be running and fully migrated before executing the integration test project:

```
docker compose -f infrastructure/docker-compose-mysql.yaml up -d
```

This spins up MySQL 8.4 on port 3307 and runs `DatabaseUpdater`, which applies all migrations and seeds all reference data (game tags, supported years, scoring systems, roles, etc.). No additional seeding is needed.

---

## Project structure

```
src/FantasyCritic.IntegrationTests/
  FantasyCriticWebApplicationFactory.cs
  Helpers/
    AntiForgeryHelper.cs
    ApiSession.cs
  Tests/
    Auth/
      AuthTests.cs
  appsettings.Testing.json
  FantasyCritic.IntegrationTests.csproj
```

---

## Section 1 — Project setup

**`FantasyCritic.IntegrationTests.csproj`**

- Target framework: `net10.0` (matches all other projects)
- Test framework: NUnit 4 + NUnit3TestAdapter (consistent with `FantasyCritic.Test`)
- Key NuGet packages:
  - `Microsoft.AspNetCore.Mvc.Testing`
  - `Microsoft.NET.Test.Sdk`
  - `NUnit`, `NUnit.Analyzers`, `NUnit3TestAdapter`
  - `Newtonsoft.Json` (for deserializing API responses; matches the web project)
- Project references:
  - `FantasyCritic.Web` (provides `Program` as the `WebApplicationFactory` entry point)

---

## Section 2 — `FantasyCriticWebApplicationFactory`

A custom `WebApplicationFactory<Program>` that makes the web app test-safe without changing any production code.

### 2a — Environment and configuration loading

Sets `ASPNETCORE_ENVIRONMENT` to `"Development"` so `Program.cs` skips the AWS Secrets Manager configuration branch. All real secrets remain unused.

ASP.NET Core auto-loads `appsettings.{Environment}.json`, so with environment = `"Development"` it would load `appsettings.Development.json` from the web project — not the test project's settings. To avoid any confusion, the test settings file is loaded **explicitly** via `ConfigureAppConfiguration`, after the host's default config sources, so it wins over anything loaded by the app itself.

### 2b — `appsettings.Testing.json` (explicitly loaded)

Committed to the repo under `src/FantasyCritic.IntegrationTests/`, no real secrets. If a developer needs different local values (e.g., different MySQL port), they create `appsettings.Testing.Local.json` in the same directory — this file is gitignored and overrides the committed file.

Contents:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3307;Database=fantasycritic;Uid=fantasycritic;Pwd=afantasticpassword;SslMode=required;charset=utf8;"
  },
  "AWS": { "rdsInstanceName": "test", "region": "us-east-1" },
  "Postmark": { "apiKey": "test" },
  "OpenCritic": { "apiKey": "test" },
  "BotToken": "test",
  "BaseAddress": "https://localhost",
  "Authentication": {
    "Google:ClientId": "test", "Google:ClientSecret": "test",
    "Microsoft:ClientId": "test", "Microsoft:ClientSecret": "test",
    "Twitch:ClientId": "test", "Twitch:ClientSecret": "test",
    "Patreon:ClientId": "test", "Patreon:ClientSecret": "test",
    "Discord:ClientId": "test", "Discord:ClientSecret": "test"
  },
  "PatreonService": { "AccessToken": "test", "RefreshToken": "test", "CampaignID": "test" }
}
```

The connection string matches the Docker Compose MySQL defaults already in `appsettings.json`. If developers use a different local setup, they can override via `appsettings.Testing.Local.json` (gitignored) or environment variables.

### 2c — Service overrides (`ConfigureTestServices`)

Three categories of services are overridden:

**Email — no-op sender + disable email confirmation**

Replace the concrete `IEmailSender` implementation with a `NullEmailSender` that does nothing. This prevents any attempt to call Postmark during registration. Additionally, reconfigure ASP.NET Identity's `SignInOptions` to set `RequireConfirmedAccount = false`, so tests can log in immediately after registering without needing to confirm an email address.

**Background services — remove all**

Remove all `IHostedService` registrations. This prevents the Discord bot, OpenCritic refresh scheduler, and any other hosted services from starting. They would fail immediately with dummy credentials and add noise to test output.

**Discord — no-op**

The `FantasyCriticDiscordConfiguration` is instantiated with the dummy `BotToken` from config. Since hosted services are removed, it never actually connects. No additional override needed.

---

## Section 3 — Auth helpers

### `AntiForgeryHelper`

Static utility. Takes an `HttpResponseMessage` whose content is the HTML of a Razor page, and extracts the value of the `__RequestVerificationToken` hidden input field using a regex. Returns the token string.

```csharp
// Approximate interface
static string ExtractToken(string html);
```

### `ApiSession`

Wraps an `HttpClient` (obtained from `WebApplicationFactory.CreateClient()` with `AllowAutoRedirect = false` and a shared `CookieContainer`) and exposes async helpers:

```csharp
Task RegisterAsync(string email, string password, string displayName);
Task LoginAsync(string email, string password);
Task<HttpResponseMessage> GetAsync(string path);
Task<HttpResponseMessage> PostJsonAsync(string path, object body);
```

`RegisterAsync` flow:
1. GET `/Account/Register` → extract antiforgery token
2. POST `/Account/Register` with form fields: `Input.DisplayName`, `Input.Email`, `Input.ConfirmEmail`, `Input.Password`, `Input.ConfirmPassword`, `__RequestVerificationToken`
3. Assert redirect (302) — registration succeeded

`LoginAsync` flow:
1. GET `/Account/Login` → extract antiforgery token
2. POST `/Account/Login` with form fields: `Input.Email`, `Input.Password`, `__RequestVerificationToken`
3. Assert redirect (302) — login succeeded; auth cookie is captured automatically by the `CookieContainer`

After `LoginAsync`, all subsequent calls on the same `ApiSession` carry the auth cookie.

**Test isolation:** Each test (or test class) creates its own `ApiSession` with a GUID-based email address (e.g., `$"user-{Guid.NewGuid():N}@integrationtest.local"`). Tests never share users or sessions.

---

## Section 4 — Test data strategy

**No teardown, no cleanup.** Tests create isolated data via the API using GUID-suffixed identifiers. The Docker MySQL volume accumulates test data across runs; this is acceptable because the volume is ephemeral and can be wiped by running `docker compose down -v` if needed.

**Reference data is free.** The DatabaseUpdater seeds all lookup tables (tags, years, scoring systems, roles, etc.) on every fresh schema. Tests can rely on this data being present without any additional setup.

**Future consideration:** If test run time or database size becomes a concern, the `Respawn` library (which resets a database to a clean state without recreating the schema) can be layered in later without changing any test logic.

---

## Section 5 — First tests: auth smoke tests (`AuthTests.cs`)

Three tests prove the full stack in one fixture:

1. **`RegisterAndLogin_ReturnsCurrentUser`**
   - Create a new `ApiSession` with a unique email
   - Call `RegisterAsync` → assert no exception
   - Call `LoginAsync` → assert no exception
   - GET `/api/Account/CurrentUser` → assert HTTP 200
   - Deserialize response, assert `email` field matches the registered email

2. **`LoginWithWrongPassword_DoesNotSucceed`**
   - Register a user
   - Attempt login with the wrong password
   - Assert the response is NOT a redirect (i.e., login failed)

3. **`UnauthenticatedRequest_ToProtectedEndpoint_Returns401`**
   - Create a fresh `ApiSession` (never logged in)
   - GET `/api/Account/CurrentUser`
   - Assert HTTP 401

These three tests validate: WAF boots cleanly, MySQL connection works, Identity stack works (create user, sign in, require auth), and the Razor → API cookie flow works end-to-end.

---

## Out of scope for this spec

- OpenAPI / Swashbuckle
- Generated typed API client
- League creation, drafting, bidding tests (follow-on work; the harness established here supports them)
- CI pipeline integration (follow-on; CI would need a MySQL service container)
- Respawn / database cleanup between runs
