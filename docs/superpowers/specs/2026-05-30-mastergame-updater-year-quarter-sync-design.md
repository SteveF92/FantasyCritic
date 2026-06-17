# Design: MasterGameUpdater — Supported Year & Royale Quarter Sync

**Date:** 2026-05-30

## Problem

`tbl_meta_supportedyear` and `tbl_royale_supportedquarter` are seeded with static data in
`2026-03-24_001_initialData.sql`. As new years and quarters are added on production, the local
Docker DB falls behind and integration tests break or miss valid states.

The fix must **not** live in the DatabaseUpdater (those scripts are deployed to production). It
belongs in the local-only tooling that already calls the production API.

## Solution Overview

Extend `FantasyCritic.MasterGameUpdater` to sync supported years and royale quarters from the
production API into the local Docker DB, using the same fetch-then-upsert pattern already used
for master games.

---

## Layer 1 — `MySQLLocalSetupSyncer` (new class)

**File:** `src/FantasyCritic.MySQL/SyncingRepos/MySQLLocalSetupSyncer.cs`

Owns SQL upsert logic for the two tables. Constructor takes `string connectionString`.

### `UpsertSupportedYears(IEnumerable<SupportedYearEntity> years)`

```sql
INSERT INTO tbl_meta_supportedyear
    (Year, OpenForCreation, OpenForPlay, OpenForBetaUsers, StartDate, Finished)
VALUES
    (@Year, @OpenForCreation, @OpenForPlay, @OpenForBetaUsers, @StartDate, @Finished)
ON DUPLICATE KEY UPDATE
    OpenForCreation = VALUES(OpenForCreation),
    OpenForPlay     = VALUES(OpenForPlay),
    OpenForBetaUsers = VALUES(OpenForBetaUsers),
    StartDate       = VALUES(StartDate),
    Finished        = VALUES(Finished);
```

`OpenForBetaUsers` is always `false` (not exposed by the production API; irrelevant locally).

### `UpsertRoyaleYearQuarters(IEnumerable<RoyaleYearQuarterSyncEntity> quarters)`

```sql
INSERT INTO tbl_royale_supportedquarter
    (Year, Quarter, OpenForPlay, Finished, WinningUser)
VALUES
    (@Year, @Quarter, @OpenForPlay, @Finished, NULL)
ON DUPLICATE KEY UPDATE
    OpenForPlay = VALUES(OpenForPlay),
    Finished    = VALUES(Finished);
    -- WinningUser intentionally NOT updated; always NULL in local
```

`WinningUser` is `NULL` on insert and never touched on update. A finished quarter with no winner
is a valid, supported state (the schema already allows it).

---

## Layer 2 — `MasterGameUpdater` program changes

**File:** `src/FantasyCritic.MasterGameUpdater/Program.cs`

Two new private static async methods, called from `Main` alongside the existing `UpdateMasterGames`:

```
await UpdateMasterGames();
await UpdateSupportedYears();
await UpdateRoyaleQuarters();
```

### `UpdateSupportedYears()`

1. `GET {baseAddress}/api/Game/SupportedYears`
2. Deserialize into a list of a local `SupportedYearResponse` record
   (fields: `int Year`, `bool OpenForCreation`, `bool OpenForPlay`, `DateTime StartDate`, `bool Finished`)
3. Map to `SupportedYearEntity` (setting `OpenForBetaUsers = false`)
4. Call `MySQLLocalSetupSyncer.UpsertSupportedYears(...)`

### `UpdateRoyaleQuarters()`

1. `GET {baseAddress}/api/Royale/RoyaleQuarters`
2. Deserialize into a list of a local `RoyaleQuarterResponse` record
   (fields: `int Year`, `int Quarter`, `bool OpenForPlay`, `bool Finished`)
3. Map to `RoyaleYearQuarterSyncEntity`
4. Call `MySQLLocalSetupSyncer.UpsertRoyaleYearQuarters(...)`

---

## Layer 3 — Local response records

Two thin private record types (or file-scoped classes) inside `MasterGameUpdater` to deserialize
the production JSON — no sharing needed since these mirror existing `ViewModel` shapes.

```csharp
private record SupportedYearResponse(int Year, bool OpenForCreation, bool OpenForPlay,
    DateTime StartDate, bool Finished);

private record RoyaleQuarterResponse(int Year, int Quarter, bool OpenForPlay, bool Finished);
```

---

## Entity types

- **`SupportedYearEntity`** — already exists in `FantasyCritic.MySQL/Entities/`. Reused directly.
- **`RoyaleYearQuarterSyncEntity`** — new minimal entity (Year, Quarter, OpenForPlay, Finished)
  used only in `MySQLLocalSetupSyncer`; distinct from `RoyaleYearQuarterEntity` which carries
  `WinningUser` and `WinningUserDisplayName` for the read path.

  Alternatively: reuse `RoyaleYearQuarterEntity` with `WinningUser = null`; avoids adding a type.
  Prefer this if it keeps things simpler.

---

## What is NOT changed

- `DatabaseUpdater` scripts — untouched.
- `tbl_royale_supportedquarter.WinningUser` — never written by the syncer.
- Existing `MySQLMasterGameUpdater` — untouched; year/quarter syncing is a separate concern.
- `FantasyCriticWebApplicationFactory` — no changes; it connects to the same local Docker DB.

---

## Testing

Run `MasterGameUpdater` locally against the Docker DB, then verify:
- `SELECT * FROM tbl_meta_supportedyear` includes the current year with correct flags.
- `SELECT * FROM tbl_royale_supportedquarter` includes the current quarter with `OpenForPlay = 1, Finished = 0`.
- Re-running is idempotent.
