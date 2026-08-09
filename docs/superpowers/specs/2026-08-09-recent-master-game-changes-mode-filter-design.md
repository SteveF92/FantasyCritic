# Recent Master Game Changes Mode Filter

## Goal

Let users view the Recent Master Game Changes feed as **new games only**, **edits only**, or **all** (default), via an API query parameter and mode buttons on the page.

## Background

The feed already unions changelog rows with synthetic `"Game added"` rows from `tbl_mastergame.AddedTimestamp` (see `2026-08-09-recent-master-game-adds-design.md`). A mixed top-100 is less useful when you only care about one kind of activity, so filtering must happen **server-side** before `LIMIT 100`.

## Constraints

- No DB schema or storage changes
- Keep `CompleteMasterGameChangeViewModel` response shape
- Default mode is **All** (combined feed)
- Cap remains 100 newest rows **for the selected mode**
- Synthetic add description remains exactly `Game added`

## API

`GET /api/game/GetRecentMasterGameChanges?mode=All`

- Enum in Lib: `RecentMasterGameChangeMode` with values `All`, `NewGames`, `Changes` (same pattern as `RoyalePossibleMasterGamesReleaseFilter`)
- Controller: `[FromQuery] RecentMasterGameChangeMode mode = RecentMasterGameChangeMode.All`
- Pass `mode` through `InterLeagueService` → `IMasterGameRepo.GetRecentMasterGameChanges(RecentMasterGameChangeMode mode)`
- Repo SQL by mode:
  - **Changes** — `tbl_mastergame_changelog` only, `ORDER BY Timestamp DESC LIMIT 100`
  - **NewGames** — synthetic add rows only (`AddedTimestamp` / `AddedByUserID` / `'Game added'`), same order/limit
  - **All** — existing `UNION ALL`, same order/limit
- Regenerate NSwag API client after the signature change

## UI

In `recentMasterGameChanges.vue`, above the table:

- Horizontal row of `b-button`s (selected = `primary`, others = `secondary`)
- Labels: **All** / **New games** / **Changes**
- Default: `All`
- On change and on create: refetch with `?mode=…` and replace table data
- In-page state only (no URL sync in this change)
- Table headers: **NewGames** drops Description and labels the date column **Date Added**; other modes keep **Date of Change** + Description

## Testing

- Integration tests for modes:
  - **All** — newly created game appears with `"Game added"` (existing coverage, keep/adapt)
  - **NewGames** — newly created game appears with `"Game added"`
  - **Changes** — newly created game does **not** appear as `"Game added"`
- Manual: switch the three mode buttons and confirm the table refreshes

## Out of scope

- Persisting preference / URL query sync
- Showing `"Game added"` on the per-game changelog page
- Changing the 100-row cap
