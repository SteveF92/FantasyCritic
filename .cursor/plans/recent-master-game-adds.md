# Recent Master Game Adds Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Include newly added master games in the Recent Master Game Changes feed by unioning `tbl_mastergame` add timestamps with changelog rows, capped at 100 newest.

**Architecture:** One SQL change in `MySQLMasterGameRepo.GetRecentMasterGameChanges` — `UNION ALL` of changelog entries and synthetic `"Game added"` rows from `tbl_mastergame.AddedTimestamp` / `AddedByUserID`. API, view models, and Vue stay unchanged. Also clear the master-game cache on create so new games hydrate correctly when the feed is read in-process.

**Tech Stack:** C# / MySQL / Dapper, ASP.NET Core API, NUnit integration tests, NSwag-generated `GameClient`.

**Spec:** `docs/superpowers/specs/2026-08-09-recent-master-game-adds-design.md`

## Global Constraints

- No DB schema or storage changes
- Keep `CompleteMasterGameChangeViewModel` response shape
- Feed is newest-first, max 100 rows overall
- Synthetic description text is exactly `Game added`
- Synthetic `MasterGameChangeID` for add rows = that game’s `MasterGameID`
- PowerShell for interactive commands on Windows

## File map

| File | Role |
| --- | --- |
| `src/FantasyCritic.MySQL/MySQLMasterGameRepo.cs` | UNION SQL in `GetRecentMasterGameChanges`; `ClearMasterGameCache` after `CreateMasterGame` |
| `src/FantasyCritic.IntegrationTests/Tests/Game/GameTests.cs` | Integration test: create game → feed contains `"Game added"` for that game |

No changes: `IMasterGameRepo`, `InterLeagueService`, `GameController`, view models, Vue, FakeRepo (still unused stub).

---

### Task 1: Failing integration test for “Game added” in recent changes

**Files:**
- Modify: `src/FantasyCritic.IntegrationTests/Tests/Game/GameTests.cs`
- Test: same file

**Interfaces:**
- Consumes: `ApiSession.FactChecker.CreateMasterGameAsync`, `ApiSession.Game.GetRecentMasterGameChangesAsync`, `IntegrationTestBase.NewUser` / `LoginAsLocalAdminAsync`, `Admin.GrantRoleAsync`
- Produces: Failing test that proves adds are missing from the feed today

- [ ] **Step 1: Confirm generated client method exists**

```powershell
Select-String -Path "src/FantasyCritic.ApiClient/Generated/*.cs" -Pattern "GetRecentMasterGameChanges"
```

Expected: a method like `GetRecentMasterGameChangesAsync` returning a collection of `CompleteMasterGameChangeViewModel` (or similarly named generated type with `MasterGame` + `Change` properties). If the method is untyped/`Task` only, stop and add `[ProducesResponseType]` — should not be needed because the action already returns `ActionResult<List<CompleteMasterGameChangeViewModel>>`.

- [ ] **Step 2: Add the failing test to `GameTests.cs`**

Add a private helper (same pattern as `FactCheckerTests`) and the test at the end of the fixture:

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

[Test]
public async Task GetRecentMasterGameChanges_IncludesNewlyAddedGame()
{
    var (email, password, displayName) = NewUser();
    using var regSession = new ApiSession(Factory);
    await regSession.RegisterAsync(email, password, displayName);
    var me = await regSession.Account.CurrentUserAsync();
    await GrantFactCheckerRoleAsync(me.UserID);

    using var fcSession = new ApiSession(Factory);
    await fcSession.LoginAsync(email, password);

    var gameName = $"Recent Add {Guid.NewGuid():N}"[..36];
    var created = await fcSession.FactChecker.CreateMasterGameAsync(new CreateMasterGameRequest
    {
        GameName = gameName,
        EstimatedReleaseDate = "2099",
        Tags = ["NewGame"],
    });

    var recentChanges = await fcSession.Game.GetRecentMasterGameChangesAsync();

    Assert.That(recentChanges, Is.Not.Null);
    var addEntry = recentChanges.SingleOrDefault(x =>
        x.MasterGame.MasterGameID == created.MasterGameID
        && x.Change.Description == "Game added");

    Assert.That(addEntry, Is.Not.Null,
        "Newly created master game should appear in recent changes with description 'Game added'.");
    Assert.That(addEntry!.Change.ChangedByUser.UserID, Is.EqualTo(me.UserID));
}
```

Adjust property names if the generated client uses different casing/names (match what NSwag emitted).

- [ ] **Step 3: Run the test and confirm it fails for the right reason**

Prerequisite: MySQL is up (`docker compose -f infrastructure/docker-compose-mysql.yaml up -d`).

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~GetRecentMasterGameChanges_IncludesNewlyAddedGame"
```

Expected: FAIL — no matching `"Game added"` row (or assertion message above). Not a compile error.

- [ ] **Step 4: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/Tests/Game/GameTests.cs
git commit -m "Add failing test for newly added games in recent master game changes."
```

---

### Task 2: UNION SQL + clear cache on create

**Files:**
- Modify: `src/FantasyCritic.MySQL/MySQLMasterGameRepo.cs` (`CreateMasterGame`, `GetRecentMasterGameChanges`)

**Interfaces:**
- Consumes: Existing `MasterGameChangeLogEntity` mapping / `GetMasterGames` / user dictionary pattern
- Produces: `GetRecentMasterGameChanges` returns up to 100 rows including synthetic adds; create invalidates master-game cache

- [ ] **Step 1: Clear master-game caches after `CreateMasterGame`**

At the end of `CreateMasterGame` (after `CommitAsync`), add the same clears `EditMasterGame` already uses:

```csharp
await transaction.CommitAsync();
ClearMasterGameCache();
ClearMasterGameYearCache();
```

Why: `GetRecentMasterGameChanges` hydrates via `GetMasterGames()` (cached). Without a clear, a newly inserted game can appear in the UNION result but be missing from the in-process dictionary → `KeyNotFoundException`.

- [ ] **Step 2: Replace the changelog-only SQL in `GetRecentMasterGameChanges`**

Replace only the `sql` constant (leave user/master-game hydration as-is):

```csharp
const string sql =
    """
    SELECT MasterGameChangeID, MasterGameID, ChangedByUserID, Timestamp, Description
    FROM (
        SELECT MasterGameChangeID, MasterGameID, ChangedByUserID, Timestamp, Description
        FROM tbl_mastergame_changelog

        UNION ALL

        SELECT MasterGameID AS MasterGameChangeID,
               MasterGameID,
               AddedByUserID AS ChangedByUserID,
               AddedTimestamp AS Timestamp,
               'Game added' AS Description
        FROM tbl_mastergame
    ) AS recent
    ORDER BY Timestamp DESC
    LIMIT 100
    """;
```

- [ ] **Step 3: Re-run the integration test**

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~GetRecentMasterGameChanges_IncludesNewlyAddedGame"
```

Expected: PASS.

- [ ] **Step 4: Smoke the existing Game tests that touch master games (optional but quick)**

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~FantasyCritic.IntegrationTests.Tests.Game.GameTests"
```

Expected: all PASS.

- [ ] **Step 5: Commit**

```powershell
git add src/FantasyCritic.MySQL/MySQLMasterGameRepo.cs
git commit -m "Include newly added master games in the recent changes feed."
```

---

## Spec coverage check

| Spec requirement | Task |
| --- | --- |
| UNION of changelog + adds via `AddedTimestamp` | Task 2 |
| Description `"Game added"` | Task 2 (+ asserted in Task 1) |
| `AddedByUserID` as changed-by | Task 2 (+ asserted in Task 1) |
| Top 100 by timestamp | Task 2 `ORDER BY … LIMIT 100` |
| No schema / API / Vue changes | File map (none of those files) |
| Manual verification alternative | Covered by Task 1 automated test (stronger than optional smoke) |
