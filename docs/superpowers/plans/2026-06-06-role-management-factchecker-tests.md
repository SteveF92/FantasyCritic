# Role Management API + FactChecker Integration Tests Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add `GetUserInfo`, `GrantRole`, and `RemoveRole` endpoints to `AdminController`, regenerate the typed API client, then write `AdminTests` and `FactCheckerTests` integration test fixtures that exercise the full role-management and FactChecker-protected flows.

**Architecture:** The three new admin endpoints live on the existing `AdminController` (already `[Authorize("Admin")]`). Role mutation reuses the existing `FantasyCriticUserManager.AddToRoleAsync` / `RemoveFromRoleAsync` infrastructure. Integration tests use the pre-seeded `localadmin@example.com` user (UserID = `Guid.Empty`, password = `localadminpassword`, all roles) to grant/remove roles on freshly registered test users.

**Tech Stack:** ASP.NET Core, ASP.NET Identity (`FantasyCriticUserManager`), NSwag (API client generation), NUnit, `FantasyCritic.ApiClient` (generated typed clients)

---

## File Map

| File | Action |
|---|---|
| `src/FantasyCritic.Web/Models/Requests/Admin/UserRoleRequest.cs` | **Create** |
| `src/FantasyCritic.Web/Controllers/API/AdminController.cs` | **Modify** – add 3 endpoints |
| `src/FantasyCritic.Web/Controllers/API/FactCheckerController.cs` | **Modify** – add `[ProducesResponseType]` to `CreateMasterGame` |
| `src/FantasyCritic.ApiClient/Generated/FantasyCriticClients.cs` | **Regenerate** (do not edit by hand) |
| `src/FantasyCritic.IntegrationTests/IntegrationTestBase.cs` | **Modify** – add admin constants + helper |
| `src/FantasyCritic.IntegrationTests/Tests/Admin/AdminTests.cs` | **Create** |
| `src/FantasyCritic.IntegrationTests/Tests/FactChecker/FactCheckerTests.cs` | **Create** |

---

## Task 1: `UserRoleRequest` type

**Files:**
- Create: `src/FantasyCritic.Web/Models/Requests/Admin/UserRoleRequest.cs`

- [ ] **Step 1: Create the file**

```csharp
namespace FantasyCritic.Web.Models.Requests.Admin;

public class UserRoleRequest
{
    public Guid UserID { get; init; }
    public string RoleName { get; init; } = "";
}
```

- [ ] **Step 2: Build to confirm no errors**

```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 3: Commit**

```powershell
git add src/FantasyCritic.Web/Models/Requests/Admin/UserRoleRequest.cs
git commit -m "Add UserRoleRequest type for Admin role management endpoints."
```

---

## Task 2: Admin endpoints — `GetUserInfo`, `GrantRole`, `RemoveRole`

**Files:**
- Modify: `src/FantasyCritic.Web/Controllers/API/AdminController.cs`
- Modify: `src/FantasyCritic.Web/Controllers/API/FactCheckerController.cs`

These three endpoints live at the bottom of `AdminController`, before the closing `}` of the class. The controller is already `[Authorize("Admin")]` so no additional auth attributes are needed.

**Required using not yet present in the file:**

```csharp
using FantasyCritic.Lib.SharedSerialization.API;
using FantasyCritic.Web.Models.Responses;
using Microsoft.AspNetCore.Http;
```

Check the file's existing usings — `FantasyCritic.Web.Models.Responses` and `Microsoft.AspNetCore.Http` are likely already there; add only what's missing.

- [ ] **Step 1: Add `GetUserInfo` endpoint**

Add to `AdminController` (inside the class, after the last existing method):

```csharp
[HttpGet]
[ProducesResponseType<FantasyCriticUserViewModel>(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<FantasyCriticUserViewModel>> GetUserInfo([FromQuery] Guid userID)
{
    var user = await _userManager.FindByIdAsync(userID.ToString());
    if (user is null)
        return NotFound();

    var roles = await _userManager.GetRolesAsync(user);
    return new FantasyCriticUserViewModel(user, roles);
}
```

- [ ] **Step 2: Add `GrantRole` endpoint**

```csharp
[HttpPost]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> GrantRole([FromBody] UserRoleRequest request)
{
    var user = await _userManager.FindByIdAsync(request.UserID.ToString());
    if (user is null)
        return NotFound();

    try
    {
        await _userManager.AddToRoleAsync(user, request.RoleName);
    }
    catch (InvalidOperationException)
    {
        return BadRequest($"Role '{request.RoleName}' does not exist.");
    }

    return Ok();
}
```

- [ ] **Step 3: Add `RemoveRole` endpoint**

```csharp
[HttpPost]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> RemoveRole([FromBody] UserRoleRequest request)
{
    var user = await _userManager.FindByIdAsync(request.UserID.ToString());
    if (user is null)
        return NotFound();

    await _userManager.RemoveFromRoleAsync(user, request.RoleName);
    return Ok();
}
```

- [ ] **Step 4: Build**

```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 5: Add `[ProducesResponseType]` to `FactCheckerController.CreateMasterGame`**

`CreateMasterGame` returns `CreatedAtAction` (HTTP 201). Without an explicit annotation NSwag generates `Task` (void) instead of `Task<MasterGameViewModel>`, which would break Test 2 in Task 6.

In `src/FantasyCritic.Web/Controllers/API/FactCheckerController.cs`, find the `CreateMasterGame` action and add the attribute:

```csharp
[HttpPost]
[ProducesResponseType<MasterGameViewModel>(StatusCodes.Status201Created)]
public async Task<ActionResult<MasterGameViewModel>> CreateMasterGame([FromBody] CreateMasterGameRequest viewModel)
```

`StatusCodes` and `MasterGameViewModel` are already in scope (`Microsoft.AspNetCore.Http` and `FantasyCritic.Lib.SharedSerialization.API` are imported at the top of the file).

- [ ] **Step 6: Build**

```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 7: Commit**

```powershell
git add src/FantasyCritic.Web/Controllers/API/AdminController.cs
git add src/FantasyCritic.Web/Controllers/API/FactCheckerController.cs
git commit -m "Add GetUserInfo, GrantRole, RemoveRole to AdminController; add ProducesResponseType to FactCheckerController.CreateMasterGame."
```

---

## Task 3: Regenerate the API client

**Files:**
- Regenerate: `src/FantasyCritic.ApiClient/Generated/FantasyCriticClients.cs`

- [ ] **Step 1: Build the Web project (NSwag reads the DLL)**

```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj
```

- [ ] **Step 2: Run the regen script**

```powershell
scripts/Regenerate-ApiClient.ps1
```

Expected output ends with: `Done. Build the solution to verify, then run your tests:`

- [ ] **Step 3: Build the full solution to verify the generated code compiles**

```powershell
dotnet build
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Verify the new methods appear in the generated file**

Search `src/FantasyCritic.ApiClient/Generated/FantasyCriticClients.cs` for `GetUserInfo`, `GrantRole`, `RemoveRole`. Each should appear as an `async Task` method on `AdminClient`.

- [ ] **Step 5: Commit**

```powershell
git add src/FantasyCritic.ApiClient/Generated/FantasyCriticClients.cs
git commit -m "Regenerate ApiClient with GetUserInfo, GrantRole, RemoveRole."
```

---

## Task 4: `IntegrationTestBase` — admin login helper

**Files:**
- Modify: `src/FantasyCritic.IntegrationTests/IntegrationTestBase.cs`

- [ ] **Step 1: Add the constants and helper method**

Add to `IntegrationTestBase` (inside the class, after `NewUser()`):

```csharp
/// <summary>
/// Credentials for the pre-seeded local admin user created by FantasyCritic.LocalDatabaseTool.
/// UserID = Guid.Empty; has every role.
/// </summary>
protected const string LocalAdminEmail = "localadmin@example.com";
protected const string LocalAdminPassword = "localadminpassword";

/// <summary>
/// Logs an <see cref="ApiSession"/> in as the pre-seeded local admin user.
/// Throws if the login fails (most likely cause: DB not seeded — run docker compose first).
/// </summary>
protected static async Task LoginAsLocalAdminAsync(ApiSession session)
{
    var success = await session.LoginAsync(LocalAdminEmail, LocalAdminPassword);
    if (!success)
        throw new InvalidOperationException(
            "Local admin login failed. Is the Docker DB running and seeded? " +
            "Run: docker compose -f infrastructure/docker-compose-mysql.yaml up -d");
}
```

- [ ] **Step 2: Build**

```powershell
dotnet build src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 3: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/IntegrationTestBase.cs
git commit -m "Add LocalAdmin login helper to IntegrationTestBase."
```

---

## Task 5: `AdminTests` fixture

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/Tests/Admin/AdminTests.cs`

All tests use:
- `session.Account.CurrentUserAsync()` to get the registered user's `UserID`
- `session.Admin.GetUserInfoAsync(userID)` to inspect roles — returns `FantasyCriticUserViewModel` (from `FantasyCritic.ApiClient` namespace), which has a `Roles` property (`IEnumerable<string>`)
- `session.Admin.GrantRoleAsync(request)` / `RemoveRoleAsync(request)` — throw `ApiException` on non-2xx

- [ ] **Step 1: Create the file**

```csharp
using System;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.Admin;

[TestFixture]
public class AdminTests : IntegrationTestBase
{
    [Test]
    public async Task GrantRole_FactChecker_RoleAppearsInGetUserInfo()
    {
        // Register a fresh user and get their ID.
        var (email, password, displayName) = NewUser();
        using var userSession = new ApiSession(Factory);
        await userSession.RegisterAsync(email, password, displayName);
        var me = await userSession.Account.CurrentUserAsync();

        using var adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(adminSession);

        // Verify FactChecker is NOT in roles yet.
        var before = await adminSession.Admin.GetUserInfoAsync(me.UserID);
        Assert.That(before.Roles, Does.Not.Contain("FactChecker"),
            "New user should not have FactChecker role before it is granted.");

        // Grant the role.
        await adminSession.Admin.GrantRoleAsync(new UserRoleRequest
        {
            UserID = me.UserID,
            RoleName = "FactChecker",
        });

        // Verify it appears.
        var after = await adminSession.Admin.GetUserInfoAsync(me.UserID);
        Assert.That(after.Roles, Contains.Item("FactChecker"),
            "FactChecker role should be present after GrantRole.");
    }

    [Test]
    public async Task RemoveRole_FactChecker_RoleDisappearsFromGetUserInfo()
    {
        var (email, password, displayName) = NewUser();
        using var userSession = new ApiSession(Factory);
        await userSession.RegisterAsync(email, password, displayName);
        var me = await userSession.Account.CurrentUserAsync();

        using var adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(adminSession);

        // Grant the role first.
        await adminSession.Admin.GrantRoleAsync(new UserRoleRequest
        {
            UserID = me.UserID,
            RoleName = "FactChecker",
        });
        var withRole = await adminSession.Admin.GetUserInfoAsync(me.UserID);
        Assert.That(withRole.Roles, Contains.Item("FactChecker"),
            "Role should be present after GrantRole.");

        // Remove the role.
        await adminSession.Admin.RemoveRoleAsync(new UserRoleRequest
        {
            UserID = me.UserID,
            RoleName = "FactChecker",
        });
        var withoutRole = await adminSession.Admin.GetUserInfoAsync(me.UserID);
        Assert.That(withoutRole.Roles, Does.Not.Contain("FactChecker"),
            "FactChecker role should be gone after RemoveRole.");
    }

    [Test]
    public async Task GrantRole_UnknownUser_Returns404()
    {
        using var adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(adminSession);

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
            await adminSession.Admin.GrantRoleAsync(new UserRoleRequest
            {
                UserID = Guid.NewGuid(),   // random — does not exist
                RoleName = "FactChecker",
            }));

        Assert.That(ex!.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task GrantRole_AsNonAdmin_Returns403()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
            await session.Admin.GrantRoleAsync(new UserRoleRequest
            {
                UserID = Guid.NewGuid(),
                RoleName = "FactChecker",
            }));

        Assert.That(ex!.StatusCode, Is.EqualTo(403));
    }
}
```

- [ ] **Step 2: Build**

```powershell
dotnet build src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 3: Run only AdminTests to confirm they pass**

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~AdminTests"
```

Expected: 4 tests pass, 0 fail.

- [ ] **Step 4: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/Tests/Admin/AdminTests.cs
git commit -m "Add AdminTests for GetUserInfo, GrantRole, RemoveRole endpoints."
```

---

## Task 6: `FactCheckerTests` fixture

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/Tests/FactChecker/FactCheckerTests.cs`

### Setup pattern used in every test

Each test:
1. Registers a user; gets their `UserID` via `Account.CurrentUserAsync()`.
2. Admin grants that user the `FactChecker` role via `GrantRoleAsync`.
3. Creates a **new** `ApiSession` and calls `LoginAsync` to get a fresh auth cookie that includes the FactChecker role claim.

The fresh session is needed because ASP.NET Identity bakes role claims into the auth cookie at sign-in time. The session created by `RegisterAsync` predates the role grant, so it lacks the FactChecker claim.

### Private helper used by Tests 3 and 4

```csharp
private async Task GrantFactCheckerRoleAsync(Guid userID)
{
    using var adminSession = new ApiSession(Factory);
    await LoginAsLocalAdminAsync(adminSession);
    await adminSession.Admin.GrantRoleAsync(new UserRoleRequest
    {
        UserID = userID,
        RoleName = "FactChecker",
    });
}
```

- [ ] **Step 1: Create the file with all four tests**

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.FactChecker;

[TestFixture]
public class FactCheckerTests : IntegrationTestBase
{
    // ──────────────────────────────────────────────────────────────────────────
    // Shared helpers
    // ──────────────────────────────────────────────────────────────────────────

    private async Task GrantFactCheckerRoleAsync(Guid userID)
    {
        using var adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(adminSession);
        await adminSession.Admin.GrantRoleAsync(new UserRoleRequest
        {
            UserID = userID,
            RoleName = "FactChecker",
        });
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Test 1 — ParseEstimatedDate
    // ──────────────────────────────────────────────────────────────────────────

    [Test]
    public async Task ParseEstimatedDate_ValidQuarterInput_ReturnsDateRange()
    {
        // Set up a FactChecker session.
        var (email, password, displayName) = NewUser();
        using var regSession = new ApiSession(Factory);
        await regSession.RegisterAsync(email, password, displayName);
        var me = await regSession.Account.CurrentUserAsync();
        await GrantFactCheckerRoleAsync(me.UserID);

        // Re-login on a fresh session to pick up the new role claim.
        using var fcSession = new ApiSession(Factory);
        await fcSession.LoginAsync(email, password);

        // Call the endpoint.
        var result = await fcSession.FactChecker.ParseEstimatedDateAsync("Q2 2027");

        Assert.That(result, Is.Not.Null);
        Assert.That(result.MinimumReleaseDate, Is.Not.Null,
            "Q2 2027 should resolve to a non-null minimum date.");
        Assert.That(result.MaximumReleaseDate, Is.Not.Null,
            "Q2 2027 should resolve to a non-null maximum date.");
        Assert.That(result.MinimumReleaseDate!.Value.Year, Is.EqualTo(2027));
        Assert.That(result.MaximumReleaseDate!.Value.Year, Is.EqualTo(2027));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Test 2 — CreateMasterGame
    // ──────────────────────────────────────────────────────────────────────────

    [Test]
    public async Task CreateMasterGame_Succeeds_AndCanBeRetrievedViaGameController()
    {
        var (email, password, displayName) = NewUser();
        using var regSession = new ApiSession(Factory);
        await regSession.RegisterAsync(email, password, displayName);
        var me = await regSession.Account.CurrentUserAsync();
        await GrantFactCheckerRoleAsync(me.UserID);

        using var fcSession = new ApiSession(Factory);
        await fcSession.LoginAsync(email, password);

        var gameName = $"Test Game {Guid.NewGuid():N}"[..36];

        var created = await fcSession.FactChecker.CreateMasterGameAsync(new CreateMasterGameRequest
        {
            GameName = gameName,
            EstimatedReleaseDate = "2099",   // Far future — will never incidentally score
            Tags = ["NewGame"],
        });

        Assert.That(created, Is.Not.Null);
        Assert.That(created.MasterGameID, Is.Not.EqualTo(Guid.Empty));
        Assert.That(created.GameName, Is.EqualTo(gameName));

        // Round-trip: verify the game is visible via the public Game endpoint.
        var fetched = await fcSession.Game.MasterGameAsync(created.MasterGameID);
        Assert.That(fetched.GameName, Is.EqualTo(gameName));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Test 3 — Approve a well-formed game request
    // ──────────────────────────────────────────────────────────────────────────

    [Test]
    public async Task ApproveMasterGameRequest_WithLinkedGame_Succeeds()
    {
        // ── Requester: a regular user submits a good request ──────────────────
        var (requesterEmail, requesterPassword, requesterName) = NewUser();
        using var requesterSession = new ApiSession(Factory);
        await requesterSession.RegisterAsync(requesterEmail, requesterPassword, requesterName);

        // Game name is unique per run so we can find it back from MyMasterGameRequests.
        var requestedGameName = $"Hollow Knight Silksong Alt {Guid.NewGuid():N}"[..48];

        await requesterSession.Game.CreateMasterGameRequestAsync(new MasterGameRequestRequest
        {
            GameName = requestedGameName,
            EstimatedReleaseDate = "2027",
            RequestNote = "Highly anticipated Metroidvania sequel from Team Cherry. " +
                          "Confirmed to be in development; expected to release in the near future. " +
                          "Steam page exists, significant community demand.",
        });

        // Fetch the request ID from the requester's own request list.
        var myRequests = await requesterSession.Game.MyMasterGameRequestsAsync();
        var theRequest = myRequests.Single(r => r.GameName == requestedGameName);

        // ── FactChecker: set up and handle the request ────────────────────────
        var (fcEmail, fcPassword, fcName) = NewUser();
        using var fcRegSession = new ApiSession(Factory);
        await fcRegSession.RegisterAsync(fcEmail, fcPassword, fcName);
        var fcMe = await fcRegSession.Account.CurrentUserAsync();
        await GrantFactCheckerRoleAsync(fcMe.UserID);

        using var fcSession = new ApiSession(Factory);
        await fcSession.LoginAsync(fcEmail, fcPassword);

        // Create the master game entry for the approved title.
        var createdGame = await fcSession.FactChecker.CreateMasterGameAsync(new CreateMasterGameRequest
        {
            GameName = requestedGameName,
            EstimatedReleaseDate = "2027",
            Tags = ["NewGame"],
        });

        // Link the request to the newly created game.
        await fcSession.FactChecker.CompleteMasterGameRequestAsync(new CompleteMasterGameRequestRequest
        {
            RequestID = theRequest.RequestID,
            ResponseNote = "Added to the database. Meets all eligibility requirements.",
            MasterGameID = createdGame.MasterGameID,
        });

        // Verify: fetch the request again and check it is answered with the linked game.
        var completedRequests = await requesterSession.Game.MyMasterGameRequestsAsync();
        var completed = completedRequests.Single(r => r.RequestID == theRequest.RequestID);
        Assert.That(completed.Answered, Is.True, "Request should be marked as answered.");
        Assert.That(completed.MasterGame, Is.Not.Null, "Completed request should have a linked game.");
        Assert.That(completed.MasterGame!.MasterGameID, Is.EqualTo(createdGame.MasterGameID));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Test 4 — Deny a vague/unhelpful game request (no linked game)
    // ──────────────────────────────────────────────────────────────────────────

    [Test]
    public async Task DenyMasterGameRequest_NoLinkedGame_Succeeds()
    {
        // ── Requester: a user submits a bad request ────────────────────────────
        var (requesterEmail, requesterPassword, requesterName) = NewUser();
        using var requesterSession = new ApiSession(Factory);
        await requesterSession.RegisterAsync(requesterEmail, requesterPassword, requesterName);

        var requestedGameName = $"Unknown Mobile Game {Guid.NewGuid():N}"[..40];

        await requesterSession.Game.CreateMasterGameRequestAsync(new MasterGameRequestRequest
        {
            GameName = requestedGameName,
            EstimatedReleaseDate = "TBD",
            RequestNote = "idk some game i saw on tiktok",
        });

        var myRequests = await requesterSession.Game.MyMasterGameRequestsAsync();
        var theRequest = myRequests.Single(r => r.GameName == requestedGameName);

        // ── FactChecker: deny the request with no linked game ─────────────────
        var (fcEmail, fcPassword, fcName) = NewUser();
        using var fcRegSession = new ApiSession(Factory);
        await fcRegSession.RegisterAsync(fcEmail, fcPassword, fcName);
        var fcMe = await fcRegSession.Account.CurrentUserAsync();
        await GrantFactCheckerRoleAsync(fcMe.UserID);

        using var fcSession = new ApiSession(Factory);
        await fcSession.LoginAsync(fcEmail, fcPassword);

        await fcSession.FactChecker.CompleteMasterGameRequestAsync(new CompleteMasterGameRequestRequest
        {
            RequestID = theRequest.RequestID,
            ResponseNote = "Request too vague to act on. Please provide a game title, " +
                           "Steam link, or other identifying information.",
            MasterGameID = null,   // No game linked — this is a denial
        });

        // Verify: request is answered but has no linked game.
        var updatedRequests = await requesterSession.Game.MyMasterGameRequestsAsync();
        var denied = updatedRequests.Single(r => r.RequestID == theRequest.RequestID);
        Assert.That(denied.Answered, Is.True, "Denied request should be marked as answered.");
        Assert.That(denied.MasterGame, Is.Null, "Denied request should have no linked game.");
    }
}
```

- [ ] **Step 2: Build**

```powershell
dotnet build src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 3: Run only FactCheckerTests**

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~FactCheckerTests"
```

Expected: 4 tests pass, 0 fail.

- [ ] **Step 4: Run all integration tests to confirm no regressions**

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release
```

Expected: All tests pass, 0 fail.

- [ ] **Step 5: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/Tests/FactChecker/FactCheckerTests.cs
git commit -m "Add FactCheckerTests: ParseEstimatedDate, CreateMasterGame, Approve and Deny game requests."
```

---

## Self-Review

**Spec coverage:**
- ✅ `GetUserInfo` endpoint (Task 2, Step 1)
- ✅ `GrantRole` endpoint (Task 2, Step 2)
- ✅ `RemoveRole` endpoint (Task 2, Step 3)
- ✅ `ProgrammaticallyAssigned = 0` — handled by existing `AddToRoleAsync` (not the `AddToRoleProgrammaticAsync` variant)
- ✅ `IntegrationTestBase` admin helper (Task 4)
- ✅ `AdminTests`: GrantRole→GetUserInfo, RemoveRole→GetUserInfo, 404 unknown user, 403 non-admin (Task 5)
- ✅ `FactCheckerTests`: ParseEstimatedDate, CreateMasterGame round-trip, Approve request with linked game, Deny request no linked game (Task 6)
- ✅ API client regenerated before tests are written (Task 3)

**No placeholders, no TBDs.**

**Type consistency:**
- `UserRoleRequest` defined in Task 1, used in Tasks 5 and 6.
- `FantasyCriticUserViewModel.Roles` (`IEnumerable<string>`) — existing type, used in Task 5 assertions.
- `ApiException` (from `FantasyCritic.ApiClient`) — thrown by generated client on non-2xx; used in Task 5 Tests 3 and 4.
- All other request/response types come from `FantasyCritic.ApiClient` namespace (generated).
