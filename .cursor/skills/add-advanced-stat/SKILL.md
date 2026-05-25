---
name: add-advanced-stat
description: Guide a contributor through designing and implementing a new Advanced Stats panel for the FantasyCritic Advanced Game Stats page. Use when a user wants to add a new stat, mentions the Advanced Stats page, or asks about contributing a new game metric.
---

# Adding a New Advanced Stat Panel

## Step 1 — Idea check (do this first)

Before writing any code, confirm the stat fits the model:

**✅ Good fit:**
- Returns a ranked list of **MasterGame** objects with one or more aggregated integer metrics
- Examples: "games that appeared on the most winning rosters", "games most often traded away"

**⚠️ Redirect — not a flat no:**
- Aggregates by *league*, *user*, or *publisher* instead of *game* → ask "could we flip this to be per-game?"
- Requires a new DB column or table → explain that new-stat PRs should work with existing schema; suggest an alternative using existing data
- Seems very niche → note it, but go ahead if they make the case for it

---

## Step 2 — Design the SQL

All data lives in MySQL. No schema changes. Key tables:

| Table | Contents |
|---|---|
| `tbl_mastergame` | Master game records |
| `tbl_caching_mastergameyear` | Per-year stats per game (hype, %, points) |
| `tbl_league_publishergame` | Active picks in league rosters |
| `tbl_league_formerpublishergame` | Dropped/traded picks |
| `tbl_league_publisher` | Publishers (has `UserID`, `Year`) |
| `tbl_league` | Leagues (filter: `TestLeague = 0`, `IsDeleted = 0`) |
| `tbl_royale_publishergame` / `tbl_royale_publisher` | Critics Royale picks |

Result shape: `SELECT MasterGameID, [your metric] AS MetricName FROM ... GROUP BY MasterGameID ORDER BY MetricName DESC LIMIT 100`

See `GetMostDreamsDashedGames` in [`MySQLMasterGameRepo.cs`](src/FantasyCritic.MySQL/MySQLMasterGameRepo.cs) as a canonical example.

---

## Step 3 — Backend (5 files, follow existing patterns exactly)

**A. Domain result type** — if your stat returns only a `MasterGameYear` with no extra fields, you don't need a new domain type. If you need additional integer stats alongside the game, add a new record to `src/FantasyCritic.Lib/Domain/` following the `LongestTenuredGame` pattern.

**B. [`IMasterGameRepo.cs`](src/FantasyCritic.Lib/Interfaces/IMasterGameRepo.cs)**
```csharp
Task<IReadOnlyList<LongestTenuredGame>> GetMyNewStat(LocalDate currentDate);
```

**C. [`MySQLMasterGameRepo.cs`](src/FantasyCritic.MySQL/MySQLMasterGameRepo.cs)**
- Write the SQL, execute with Dapper, hydrate via `GetMasterGameYear(id, year)` or `GetMasterGameYears(year)`
- For "use the highest available year per game" use the `MAX(Year)` from `tbl_caching_mastergameyear` pattern (see `GetMostDreamsDashedGames`)

**D. [`FakeMasterGameRepo.cs`](src/FantasyCritic.FakeRepo/FakeMasterGameRepo.cs)**
```csharp
public Task<IReadOnlyList<LongestTenuredGame>> GetMyNewStat(LocalDate currentDate)
    => Task.FromResult<IReadOnlyList<LongestTenuredGame>>(Array.Empty<LongestTenuredGame>());
```

**E. [`InterLeagueService.cs`](src/FantasyCritic.Lib/Services/InterLeagueService.cs)** — pass-through

**F. [`GameController.cs`](src/FantasyCritic.Web/Controllers/API/GameController.cs)**
```csharp
[HttpGet]
public async Task<ActionResult<List<LongestTenuredGameViewModel>>> GetMyNewStat()
{
    var currentDate = _clock.GetToday();
    var results = await _interLeagueService.GetMyNewStat(currentDate);
    return results.Select(x => new LongestTenuredGameViewModel(x, currentDate)).ToList();
}
```
If your stat only needs game data, return `List<MasterGameYearViewModel>` directly. If you need extra fields alongside the game, create a new ViewModel in `src/FantasyCritic.Web/Models/Responses/` following the `LongestTenuredGameViewModel` pattern (wraps `MasterGameYearViewModel` + your specific properties).

---

## Step 4 — Frontend (2 files)

**A. Create `src/FantasyCritic.Web/ClientApp/src/components/gameStats/myNewStatPanel.vue`**

Use `mostDreamsDashedPanel.vue` as your template. Key structure:
- `created()` fetches from your new endpoint
- `isBusy` / `showTable` / spinner pattern
- `b-table` with `masterGamePopover` for the game name cell, `masterGameTagBadge` for tags
- If you need `supportedYears`, add `mixins: [BasicMixin]`

**B. Register in [`advancedGameStats.vue`](src/FantasyCritic.Web/ClientApp/src/views/advancedGameStats.vue)**

Add to `sections` array and `v-else-if` block:
```js
{ hash: 'myNewStat', label: 'My New Stat' }
```
```html
<myNewStatPanel v-else-if="activeSection === 'myNewStat'"></myNewStatPanel>
```
Also add the hash to `VALID_SECTIONS`.

---

## Step 5 — Verify

- `dotnet build` compiles clean (0 warnings — `TreatWarningsAsErrors` is on)
- Page loads and spinner resolves to a table
- Each row shows a game name (with popover), not a user or league
