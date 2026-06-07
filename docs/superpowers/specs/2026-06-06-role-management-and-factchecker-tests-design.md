# Role Management API + FactChecker Integration Tests

**Date:** 2026-06-06  
**Status:** Approved

---

## Goal

1. Add `GrantRole` and `RemoveRole` endpoints to `AdminController` so tests (and admin UI in future) can programmatically manage user roles without direct DB access.
2. Add integration tests for those new endpoints (`AdminTests`).
3. Add integration tests for `FactCheckerController` using the new role-management endpoints to set up a FactChecker-role session (`FactCheckerTests`).

---

## Background

### Local admin seed user

`FantasyCritic.LocalDatabaseTool` seeds a fixed admin user on every fresh DB:

| Field | Value |
|---|---|
| `UserID` | `Guid.Empty` (all zeros) |
| `EmailAddress` | `localadmin@example.com` |
| `Password` (cleartext) | `localadminpassword` |
| Roles | All roles (Admin, FactChecker, ActionRunner, PlusUser, …) |

This user is the only one with the `Admin` role in integration tests and is the sole caller of the new endpoints.

### Role storage

Roles live in `tbl_user_hasrole (UserID, RoleID, ProgrammaticallyAssigned)`.  
`ProgrammaticallyAssigned = 1` is reserved for Patreon-controlled grants.  
The new endpoints always write `ProgrammaticallyAssigned = 0`.

`FantasyCriticUserManager` already exposes:
- `AddToRoleAsync(user, roleName)` — inserts with `ProgrammaticallyAssigned = 0`
- `RemoveFromRoleAsync(user, roleName)` — deletes the row

---

## Part 1 — New Admin endpoints

### Request type

**New file:** `src/FantasyCritic.Web/Models/Requests/Admin/UserRoleRequest.cs`

```csharp
public class UserRoleRequest
{
    public Guid UserID { get; init; }
    public string RoleName { get; init; } = "";
}
```

One type shared by both endpoints (identical shape).

### Endpoints added to `AdminController`

```
POST /api/Admin/GrantRole
POST /api/Admin/RemoveRole
```

Both require the `Admin` role (inherited from the controller-level `[Authorize("Admin")]`).

**`GrantRole` logic:**
1. Look up user by `UserID` — 404 if not found.
2. Call `_userManager.AddToRoleAsync(user, request.RoleName)` (wrapped in try/catch for invalid role names → 400).
3. Return 200.

**`RemoveRole` logic:**
1. Look up user by `UserID` — 404 if not found.
2. Call `_userManager.RemoveFromRoleAsync(user, request.RoleName)`.
3. Return 200.

---

## Part 2 — `IntegrationTestBase` additions

Add to `IntegrationTestBase.cs`:

```csharp
protected const string LocalAdminEmail = "localadmin@example.com";
protected const string LocalAdminPassword = "localadminpassword";

protected static async Task LoginAsLocalAdminAsync(ApiSession session)
{
    var success = await session.LoginAsync(LocalAdminEmail, LocalAdminPassword);
    if (!success)
        throw new InvalidOperationException("Local admin login failed — is the DB seeded?");
}
```

These are known seed constants, not arbitrary user data, so defining them here is appropriate.

---

## Part 3 — `AdminTests` fixture

**New file:** `src/FantasyCritic.IntegrationTests/Tests/Admin/AdminTests.cs`

### Test 1 — `GrantRole_FactChecker_Returns200`

1. Register a new user.
2. Admin session logs in via `LoginAsLocalAdminAsync`.
3. Admin calls `session.Admin.GrantRoleAsync({ UserID: newUser.UserID, RoleName: "FactChecker" })`.
4. Assert 200.
5. To verify the role is effective: new user's session calls a FactChecker endpoint (e.g., `ParseEstimatedDate`) — assert 200 (not 403).

### Test 2 — `RemoveRole_FactChecker_Returns200`

1. Register a new user.
2. Admin grants `FactChecker` to that user.
3. Verify FactChecker endpoint returns 200 for that user.
4. Admin calls `RemoveRole`.
5. Create a fresh session for that user and re-login.
6. Verify FactChecker endpoint returns 403.

### Test 3 — `GrantRole_UnknownUser_Returns404`

1. Admin calls `GrantRole` with a random `UserID` that doesn't exist.
2. Assert 404.

### Test 4 — `GrantRole_AsNonAdmin_Returns403`

1. Register a new plain user.
2. That user tries to call `GrantRole`.
3. Assert 403.

---

## Part 4 — `FactCheckerTests` fixture

**New file:** `src/FantasyCritic.IntegrationTests/Tests/FactChecker/FactCheckerTests.cs`

Each test follows this setup pattern:

```csharp
// Register a regular user
var (email, password, displayName) = NewUser();
using var userSession = new ApiSession(Factory);
await userSession.RegisterAsync(email, password, displayName);
var me = await userSession.Account.CurrentUserAsync();

// Admin grants FactChecker role
using var adminSession = new ApiSession(Factory);
await LoginAsLocalAdminAsync(adminSession);
await adminSession.Admin.GrantRoleAsync(new UserRoleRequest
{
    UserID = me.UserID,
    RoleName = "FactChecker",
});

// Re-login to pick up new role claims
await userSession.LoginAsync(email, password);
```

### Test 1 — `ParseEstimatedDate_ValidInput_ReturnsExpectedRange`

- Input: `"Q2 2027"` (or similar well-known format)
- Assert: `MinimumReleaseDate` and `MaximumReleaseDate` are non-null and span the expected quarter.
- No DB writes. Pure validation smoke test for the role gate.

### Test 2 — `CreateMasterGame_Succeeds_AndCanBeRetrievedViaGameController`

- Calls `FactChecker.CreateMasterGameAsync` with plausible data:
  - `GameName`: `$"Test Game {Guid.NewGuid():N}"[..30]`
  - `EstimatedReleaseDate`: `"2027"` (future year, obviously won't release)
  - `Tags`: `["NewGame"]`
  - All other fields: defaults
- Assert: response returns a `MasterGameViewModel` with a non-empty `MasterGameID`.
- Round-trip: call `session.Game.MasterGameAsync(id)` — assert game name matches.

### Test 3 — `ApproveMasterGameRequest_WithLinkedGame_Succeeds` (the "good request")

Scenario: a user submits a well-formed game request for a real upcoming AAA title; the FactChecker creates the game and links it.

1. **Regular user** calls `userSession.Game.CreateMasterGameRequestAsync` with a request that looks legitimate:
   - Game name: something like `"Hollow Knight: Silksong"` (or a plausible upcoming game placeholder)
   - Note: `"Highly anticipated metroidvania sequel; confirmed for release this year."`
2. **FactChecker** first creates the game via `CreateMasterGameAsync` (same game name).
3. **FactChecker** calls `CompleteMasterGameRequestAsync` with the `RequestID` and the new `MasterGameID`.
4. Assert: 200.

### Test 4 — `DenyMasterGameRequest_NoLinkedGame_Succeeds` (the "bad request")

Scenario: a user submits a vague or unfulfillable request; the FactChecker denies it with no linked game.

1. **Regular user** calls `CreateMasterGameRequestAsync` with a bad-faith request:
   - Game name: `"[UNKNOWN MOBILE GAME]"` (or similarly unhelpful)
   - Note: `"idk some game i saw on tiktok"`
2. **FactChecker** calls `CompleteMasterGameRequestAsync` with `MasterGameID = null` and a dismissal note: `"Request too vague to act on."`
3. Assert: 200.

---

## Regeneration step

After the `GrantRole` / `RemoveRole` endpoints are added, run before writing tests:

```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj
scripts/Regenerate-ApiClient.ps1
dotnet build
```

This ensures `session.Admin.GrantRoleAsync(...)` and `session.Admin.RemoveRoleAsync(...)` are available in the generated client.

---

## Files affected

| File | Change |
|---|---|
| `src/FantasyCritic.Web/Models/Requests/Admin/UserRoleRequest.cs` | New |
| `src/FantasyCritic.Web/Controllers/API/AdminController.cs` | Add `GrantRole` + `RemoveRole` |
| `src/FantasyCritic.ApiClient/Generated/FantasyCriticClients.cs` | Regenerated |
| `src/FantasyCritic.IntegrationTests/IntegrationTestBase.cs` | Add admin login helper + constants |
| `src/FantasyCritic.IntegrationTests/Tests/Admin/AdminTests.cs` | New |
| `src/FantasyCritic.IntegrationTests/Tests/FactChecker/FactCheckerTests.cs` | New |
