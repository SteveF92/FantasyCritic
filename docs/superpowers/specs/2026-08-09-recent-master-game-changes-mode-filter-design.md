# Recent Master Game Changes Mode Filter

## Goal

Let users view the Recent Master Game Changes feed as **new games only**, **edits only**, or **both** (default), via an API query parameter and a radio group on the page.

## Background

The feed already unions changelog rows with synthetic `"Game added"` rows from `tbl_mastergame.AddedTimestamp` (see `2026-08-09-recent-master-game-adds-design.md`). A mixed top-100 is less useful when you only care about one kind of activity, so filtering must happen **server-side** before `LIMIT 100`.

## Constraints

- No DB schema or storage changes
- Keep `CompleteMasterGameChangeViewModel` response shape
- Default mode is **Both** (current behavior)
- Cap remains 100 newest rows **for the selected mode**
- Synthetic add description remains exactly `Game added`

## API

`GET /api/game/GetRecentMasterGameChanges?mode=Both`

- Enum in Lib: `RecentMasterGameChangeMode` with values `Both`, `NewGames`, `Changes` (same pattern as `RoyalePossibleMasterGamesReleaseFilter`)
- Controller: `[FromQuery] RecentMasterGameChangeMode mode = RecentMasterGameChangeMode.Both`
- Pass `mode` through `InterLeagueService` → `IMasterGameRepo.GetRecentMasterGameChanges(RecentMasterGameChangeMode mode)`
- Repo SQL by mode:
  - **Changes** — `tbl_mastergame_changelog` only, `ORDER BY Timestamp DESC LIMIT 100`
  - **NewGames** — synthetic add rows only (`AddedTimestamp` / `AddedByUserID` / `'Game added'`), same order/limit
  - **Both** — existing `UNION ALL`, same order/limit
- Regenerate NSwag API client after the signature change

## UI

In `recentMasterGameChanges.vue`, above the table:

- `b-form-radio` group (same style as `masterGames.vue` filters)
- Labels: **Both** / **New games** / **Changes**
- Default: `Both`
- On change and on create: refetch with `?mode=…` and replace table data
- In-page state only (no URL sync in this change)

## Testing

- Integration tests for modes:
  - **Both** — newly created game appears with `"Game added"` (existing coverage, keep/adapt)
  - **NewGames** — newly created game appears with `"Game added"`
  - **Changes** — newly created game does **not** appear as `"Game added"`
- Manual: switch the three radios and confirm the table refreshes

## Out of scope

- Persisting preference / URL query sync
- Showing `"Game added"` on the per-game changelog page
- Changing the 100-row cap or page columns
