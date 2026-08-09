# Recent Master Game Changes Mode Filter Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a `mode` query parameter and Vue radio group so the recent master-game feed can show Both (default), NewGames only, or Changes only — each capped at 100 newest rows for that mode.

**Architecture:** Introduce `RecentMasterGameChangeMode` in Lib, thread it from `GameController` through `InterLeagueService` / `IMasterGameRepo` into SQL that selects changelog-only, adds-only, or the existing `UNION ALL`. Vue radios refetch with `?mode=…`. Regenerate NSwag after the controller signature change.

**Tech Stack:** C# / MySQL / Dapper, ASP.NET Core, Vue 2.7 + Bootstrap-Vue radios, NUnit integration tests, NSwag `GameClient`.

**Spec:** `docs/superpowers/specs/2026-08-09-recent-master-game-changes-mode-filter-design.md`

## Global Constraints

- No DB schema or storage changes
- Keep `CompleteMasterGameChangeViewModel` response shape
- Default mode is **Both**
- Cap remains 100 newest rows **for the selected mode**
- Synthetic add description remains exactly `Game added`
- Enum values exactly: `Both`, `NewGames`, `Changes`
- Query parameter name exactly: `mode`
- PowerShell for interactive commands on Windows
- This work continues on the branch that already has the UNION feed (verify `GetRecentMasterGameChanges` already has the UNION SQL before starting)

## File map

| File | Role |
| --- | --- |
| `src/FantasyCritic.Lib/Domain/RecentMasterGameChangeMode.cs` | New enum |
| `src/FantasyCritic.Lib/Interfaces/IMasterGameRepo.cs` | Add `mode` parameter |
| `src/FantasyCritic.Lib/Services/InterLeagueService.cs` | Pass `mode` through |
| `src/FantasyCritic.MySQL/MySQLMasterGameRepo.cs` | Mode-specific SQL |
| `src/FantasyCritic.FakeRepo/FakeMasterGameRepo.cs` | Signature match (still unused stub) |
| `src/FantasyCritic.Web/Controllers/API/GameController.cs` | `[FromQuery] mode` |
| `src/FantasyCritic.IntegrationTests/Tests/Game/GameTests.cs` | Mode integration tests |
| `src/FantasyCritic.Web/ClientApp/src/views/recentMasterGameChanges.vue` | Radio group + refetch |
| NSwag regen | Typed client gets optional `mode` |

---

### Task 1: Enum, API wiring, mode-specific SQL, and integration tests

**Files:**
- Create: `src/FantasyCritic.Lib/Domain/RecentMasterGameChangeMode.cs`
- Modify: `src/FantasyCritic.Lib/Interfaces/IMasterGameRepo.cs`
- Modify: `src/FantasyCritic.Lib/Services/InterLeagueService.cs`
- Modify: `src/FantasyCritic.MySQL/MySQLMasterGameRepo.cs`
- Modify: `src/FantasyCritic.FakeRepo/FakeMasterGameRepo.cs`
- Modify: `src/FantasyCritic.Web/Controllers/API/GameController.cs`
- Modify: `src/FantasyCritic.IntegrationTests/Tests/Game/GameTests.cs`
- Regenerate: ApiClient via `scripts/Regenerate-ApiClient.ps1`

**Interfaces:**
- Consumes: Existing UNION SQL in `GetRecentMasterGameChanges`; existing `GrantFactCheckerRoleAsync` / create-game test helpers in `GameTests`
- Produces:
  - `public enum RecentMasterGameChangeMode { Both, NewGames, Changes }`
  - `Task<IReadOnlyList<MasterGameChangeLogEntry>> GetRecentMasterGameChanges(RecentMasterGameChangeMode mode)`
  - Controller: `[FromQuery] RecentMasterGameChangeMode mode = RecentMasterGameChangeMode.Both`
  - Generated: `GetRecentMasterGameChangesAsync(RecentMasterGameChangeMode? mode = null)` (or equivalent NSwag shape)

- [ ] **Step 1: Add the enum**

Create `src/FantasyCritic.Lib/Domain/RecentMasterGameChangeMode.cs`:

```csharp
namespace FantasyCritic.Lib.Domain;

public enum RecentMasterGameChangeMode
{
    Both,
    NewGames,
    Changes
}
```

- [ ] **Step 2: Thread `mode` through interface, service, FakeRepo, and controller (keep current SQL for now)**

`IMasterGameRepo`:

```csharp
Task<IReadOnlyList<MasterGameChangeLogEntry>> GetRecentMasterGameChanges(RecentMasterGameChangeMode mode);
```

`InterLeagueService`:

```csharp
public Task<IReadOnlyList<MasterGameChangeLogEntry>> GetRecentMasterGameChanges(RecentMasterGameChangeMode mode)
{
    return _masterGameRepo.GetRecentMasterGameChanges(mode);
}
```

`FakeMasterGameRepo` — update signature to match; body can remain `throw new NotImplementedException();`.

`GameController.GetRecentMasterGameChanges`:

```csharp
[HttpGet]
public async Task<ActionResult<List<CompleteMasterGameChangeViewModel>>> GetRecentMasterGameChanges(
    [FromQuery] RecentMasterGameChangeMode mode = RecentMasterGameChangeMode.Both)
{
    IReadOnlyList<MasterGameChangeLogEntry> recentChanges = await _interLeagueService.GetRecentMasterGameChanges(mode);
    var currentDate = _clock.GetToday();
    var vms = recentChanges.Select(x => new CompleteMasterGameChangeViewModel(x, currentDate)).ToList();
    return vms;
}
```

`MySQLMasterGameRepo.GetRecentMasterGameChanges` — change signature to take `RecentMasterGameChangeMode mode` but **leave the existing UNION SQL body unchanged** for this step (so Both still works; mode is unused until Step 5). Suppress unused-parameter warning with a discard comment or `_ = mode;` temporarily if TreatWarningsAsErrors complains — or jump straight to Step 5 in the same edit session if preferred. Prefer implementing SQL in Step 5 immediately after the failing test in Step 4 (classic TDD).

- [ ] **Step 3: Rebuild Web project and regenerate ApiClient**

```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj
scripts/Regenerate-ApiClient.ps1
dotnet build src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release
```

Expected: build succeeds; generated `GetRecentMasterGameChangesAsync` accepts `mode`.

- [ ] **Step 4: Update/add integration tests (RED for Changes / NewGames until SQL branches)**

In `GameTests.cs`:

1. Update existing `GetRecentMasterGameChanges_IncludesNewlyAddedGame` to pass `RecentMasterGameChangeMode.Both` explicitly (or omit for default — either is fine; prefer explicit `Both`).

2. Add:

```csharp
[Test]
public async Task GetRecentMasterGameChanges_NewGamesMode_IncludesNewlyAddedGame()
{
    var (email, password, displayName) = NewUser();
    using var regSession = new ApiSession(Factory);
    await regSession.RegisterAsync(email, password, displayName);
    var me = await regSession.Account.CurrentUserAsync();
    await GrantFactCheckerRoleAsync(me.UserID);

    using var fcSession = new ApiSession(Factory);
    await fcSession.LoginAsync(email, password);

    var gameName = $"NewGames Mode {Guid.NewGuid():N}"[..36];
    var created = await fcSession.FactChecker.CreateMasterGameAsync(new CreateMasterGameRequest
    {
        GameName = gameName,
        EstimatedReleaseDate = "2099",
        Tags = ["NewGame"],
    });

    var recentChanges = await fcSession.Game.GetRecentMasterGameChangesAsync(RecentMasterGameChangeMode.NewGames);

    Assert.That(recentChanges, Is.Not.Null);
    var addEntry = recentChanges.SingleOrDefault(x =>
        x.MasterGame.MasterGameID == created.MasterGameID
        && x.Change.Description == "Game added");

    Assert.That(addEntry, Is.Not.Null,
        "NewGames mode should include newly created master game with description 'Game added'.");
    Assert.That(recentChanges.All(x => x.Change.Description == "Game added"), Is.True,
        "NewGames mode should only return Game added rows.");
}

[Test]
public async Task GetRecentMasterGameChanges_ChangesMode_ExcludesNewlyAddedGame()
{
    var (email, password, displayName) = NewUser();
    using var regSession = new ApiSession(Factory);
    await regSession.RegisterAsync(email, password, displayName);
    var me = await regSession.Account.CurrentUserAsync();
    await GrantFactCheckerRoleAsync(me.UserID);

    using var fcSession = new ApiSession(Factory);
    await fcSession.LoginAsync(email, password);

    var gameName = $"Changes Mode {Guid.NewGuid():N}"[..36];
    var created = await fcSession.FactChecker.CreateMasterGameAsync(new CreateMasterGameRequest
    {
        GameName = gameName,
        EstimatedReleaseDate = "2099",
        Tags = ["NewGame"],
    });

    var recentChanges = await fcSession.Game.GetRecentMasterGameChangesAsync(RecentMasterGameChangeMode.Changes);

    Assert.That(recentChanges, Is.Not.Null);
    var addEntry = recentChanges.SingleOrDefault(x =>
        x.MasterGame.MasterGameID == created.MasterGameID
        && x.Change.Description == "Game added");

    Assert.That(addEntry, Is.Null,
        "Changes mode must not include synthetic Game added rows for newly created games.");
}
```

Use the generated enum namespace (`FantasyCritic.ApiClient` if NSwag emits it there, or Lib if shared — match whatever the regenerated client uses). Add `using` as needed.

- [ ] **Step 5: Run Changes-mode test — expect RED**

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~GetRecentMasterGameChanges_ChangesMode_ExcludesNewlyAddedGame"
```

Expected: FAIL — `addEntry` is not null (UNION still includes adds). If SQL was already branched in Step 2, skip RED and go to Step 6 verification.

- [ ] **Step 6: Implement mode-specific SQL in `MySQLMasterGameRepo.GetRecentMasterGameChanges`**

Replace the method body SQL selection with:

```csharp
public async Task<IReadOnlyList<MasterGameChangeLogEntry>> GetRecentMasterGameChanges(RecentMasterGameChangeMode mode)
{
    var users = await _userStore.GetAllUsers();
    var userDictionary = users.ToDictionary(x => x.Id);

    var masterGames = await GetMasterGames();
    var masterGameDictionary = masterGames.ToDictionary(x => x.MasterGameID);

    string sql = mode switch
    {
        RecentMasterGameChangeMode.Changes =>
            """
            SELECT MasterGameChangeID, MasterGameID, ChangedByUserID, Timestamp, Description
            FROM tbl_mastergame_changelog
            ORDER BY Timestamp DESC
            LIMIT 100
            """,
        RecentMasterGameChangeMode.NewGames =>
            """
            SELECT MasterGameID AS MasterGameChangeID,
                   MasterGameID,
                   AddedByUserID AS ChangedByUserID,
                   AddedTimestamp AS Timestamp,
                   'Game added' AS Description
            FROM tbl_mastergame
            ORDER BY Timestamp DESC
            LIMIT 100
            """,
        RecentMasterGameChangeMode.Both =>
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
            """,
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
    };

    await using var connection = new MySqlConnection(_connectionString);
    IEnumerable<MasterGameChangeLogEntity> entities = await connection.QueryAsync<MasterGameChangeLogEntity>(sql);

    var domains = entities
        .Select(entity => entity.ToDomain(masterGameDictionary[entity.MasterGameID], userDictionary[entity.ChangedByUserID]))
        .ToList();

    return domains;
}
```

Note: for **NewGames**, the `ORDER BY Timestamp` refers to the selected alias `Timestamp` (from `AddedTimestamp AS Timestamp`). That is intentional and matches MySQL alias ordering.

- [ ] **Step 7: Run all three recent-changes tests GREEN**

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~GetRecentMasterGameChanges_"
```

Expected: 3 passed (Both, NewGames, Changes).

- [ ] **Step 8: Commit**

```powershell
git add src/FantasyCritic.Lib/Domain/RecentMasterGameChangeMode.cs `
  src/FantasyCritic.Lib/Interfaces/IMasterGameRepo.cs `
  src/FantasyCritic.Lib/Services/InterLeagueService.cs `
  src/FantasyCritic.MySQL/MySQLMasterGameRepo.cs `
  src/FantasyCritic.FakeRepo/FakeMasterGameRepo.cs `
  src/FantasyCritic.Web/Controllers/API/GameController.cs `
  src/FantasyCritic.IntegrationTests/Tests/Game/GameTests.cs `
  docs/superpowers/specs/2026-08-09-recent-master-game-changes-mode-filter-design.md
git commit -m "Add mode filter for recent master game changes feed."
```

Do **not** commit generated ApiClient output if it is gitignored.

---

### Task 2: Vue radio group

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/views/recentMasterGameChanges.vue`

**Interfaces:**
- Consumes: `GET /api/game/GetRecentMasterGameChanges?mode=Both|NewGames|Changes`
- Produces: In-page radios defaulting to Both; refetch on change

- [ ] **Step 1: Update the Vue view**

Replace `recentMasterGameChanges.vue` with:

```vue
<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <h1>Recent Master Game Changes</h1>

    <b-form-group>
      <b-form-radio v-model="mode" value="Both">Both</b-form-radio>
      <b-form-radio v-model="mode" value="NewGames">New games</b-form-radio>
      <b-form-radio v-model="mode" value="Changes">Changes</b-form-radio>
    </b-form-group>

    <b-table small bordered striped responsive :items="recentChanges" :fields="gameFields">
      <template #cell(masterGame.gameName)="data">
        <masterGamePopover :master-game="data.item.masterGame"></masterGamePopover>
      </template>
      <template #cell(timestamp)="data">
        {{ data.item.change.timestamp | longDate }}
      </template>
      <template #cell(description)="data">
        {{ data.item.change.description }}
      </template>
      <template #cell(changedByUser)="data">
        {{ data.item.change.changedByUser.displayName }}
      </template>
    </b-table>
  </div>
</template>

<script>
import axios from 'axios';
import MasterGamePopover from '@/components/masterGamePopover.vue';

export default {
  components: {
    MasterGamePopover
  },
  data() {
    return {
      mode: 'Both',
      recentChanges: null,
      gameFields: [
        { key: 'masterGame.gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
        { key: 'timestamp', label: 'Date of Change', sortable: true, thClass: 'bg-primary' },
        { key: 'description', label: 'Description', thClass: 'bg-primary' }
        //{ key: 'changedByUser', label: 'Changed by', thClass: 'bg-primary' }
      ]
    };
  },
  watch: {
    mode() {
      this.fetchRecentChanges();
    }
  },
  async created() {
    await this.fetchRecentChanges();
  },
  methods: {
    async fetchRecentChanges() {
      const response = await axios.get('/api/game/GetRecentMasterGameChanges', {
        params: { mode: this.mode }
      });
      this.recentChanges = response.data;
    }
  }
};
</script>
```

- [ ] **Step 2: Manual smoke (or leave for human)**

With the web app running, open Recent Master Game Changes, confirm default Both loads, then switch New games / Changes and confirm the table refreshes and descriptions match the mode.

- [ ] **Step 3: Commit**

```powershell
git add src/FantasyCritic.Web/ClientApp/src/views/recentMasterGameChanges.vue
git commit -m "Add mode radios to recent master game changes page."
```

---

## Spec coverage check

| Spec requirement | Task |
| --- | --- |
| `mode` query param + enum Both/NewGames/Changes | Task 1 |
| Default Both | Task 1 controller default + Task 2 `mode: 'Both'` |
| SQL per mode, LIMIT 100 | Task 1 |
| Unchanged response shape | Task 1 (no VM changes) |
| Vue radios + refetch | Task 2 |
| Integration tests for three modes | Task 1 |
| NSwag regen | Task 1 |
| No schema changes | File map |
