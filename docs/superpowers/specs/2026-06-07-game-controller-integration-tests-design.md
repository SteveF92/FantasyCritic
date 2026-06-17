# Game Controller Integration Tests — Design

**Date:** 2026-06-07

## Goal

Add integration test coverage for `GameController` and the master game request/change-request lifecycle. Consolidate the existing request approval/denial tests from `FactCheckerTests` into dedicated fixtures so the full lifecycle (user submits → FactChecker responds) lives in one place.

---

## Controller changes (prerequisite)

Six POST actions in `GameController` use `GetCurrentUserOrThrow()` but lack `[Authorize]`:

- `CreateMasterGameRequest`
- `CreateMasterGameChangeRequest`
- `DeleteMasterGameRequest`
- `DeleteMasterGameChangeRequest`
- `DismissMasterGameRequest`
- `DismissMasterGameChangeRequest`

Add `[Authorize]` to each. This is a correctness fix (unauthenticated callers currently get an unhandled exception rather than a clean 401) and it enables the 401 tests below.

After adding the attribute, regenerate the API client:

```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj
scripts/Regenerate-ApiClient.ps1
dotnet build
```

---

## New and modified test files

### `Tests/FactChecker/FactCheckerTests.cs` — shrink

Remove the two request-lifecycle tests (they move to `MasterGameRequestTests`). Keep:

- `ParseEstimatedDate_ValidQuarterInput_ReturnsDateRange`
- `CreateMasterGame_Succeeds_AndCanBeRetrievedViaGameController`

### `Tests/Game/GameTests.cs` — new (8 tests)

Smoke tests for public read endpoints, not-found cases, and auth edge cases. No authentication needed for most tests.

| Test | Description |
|---|---|
| `SupportedYears_ReturnsNonEmptyList` | `SupportedYearsAsync()` returns a non-null, non-empty list |
| `GetMasterGameTags_ReturnsNonEmptyList` | `GetMasterGameTagsAsync()` returns a non-null, non-empty list |
| `MasterGameAll_ReturnsNonEmptyList` | `MasterGameAllAsync()` returns a non-null, non-empty list |
| `MasterGame_UnknownID_Returns404` | `MasterGameAsync(Guid.NewGuid())` throws `ApiException` with status 404 |
| `MasterGameChangeLog_UnknownID_Returns404` | `MasterGameChangeLogAsync(Guid.NewGuid())` throws `ApiException` with status 404 |
| `MyMasterGameRequests_Unauthenticated_Returns401` | Unauthenticated GET → 401 |
| `MyMasterGameChangeRequests_Unauthenticated_Returns401` | Unauthenticated GET → 401 |
| `LeagueYearsWithMasterGame_Unauthenticated_ReturnsEmptyList` | Unauthenticated call returns empty list (not an error — soft auth) |

### `Tests/Game/MasterGameRequestTests.cs` — new (8 tests)

Full request lifecycle. Tests the user side and the FactChecker side in one fixture. Two tests are moved verbatim from `FactCheckerTests`; the other six are new.

**User side (6 new tests):**

| Test | Description |
|---|---|
| `CreateMasterGameRequest_AppearsInMyRequests` | After `CreateMasterGameRequestAsync`, the request appears in `MyMasterGameRequestsAsync` with the matching `GameName` |
| `DeleteMasterGameRequest_DisappearsFromMyRequests` | After `DeleteMasterGameRequestAsync`, the request is absent from `MyMasterGameRequestsAsync` |
| `DismissMasterGameRequest_SetsHiddenFlag` | After `DismissMasterGameRequestAsync`, the request is still in `MyMasterGameRequestsAsync` but `Hidden == true` |
| `DeleteMasterGameRequest_NotOwner_Returns403` | User B attempts to delete User A's request → `ApiException` 403 |
| `DeleteMasterGameRequest_UnknownID_Returns400` | `DeleteMasterGameRequestAsync` with `Guid.NewGuid()` → `ApiException` 400 |
| `CreateMasterGameRequest_Unauthenticated_Returns401` | Unauthenticated POST → `ApiException` 401 (requires `[Authorize]` fix) |

**FactChecker side (2 tests moved from `FactCheckerTests`):**

| Test | Description |
|---|---|
| `ApproveMasterGameRequest_WithLinkedGame_Succeeds` | User creates request; FactChecker calls `CompleteMasterGameRequestAsync` with a `MasterGameID`; request is answered with the linked game |
| `DenyMasterGameRequest_NoLinkedGame_Succeeds` | User creates request; FactChecker calls `CompleteMasterGameRequestAsync` with `MasterGameID = null`; request is answered without a linked game |

**Helper:** A private `GrantFactCheckerRoleAsync(Guid userID)` method (same pattern as in the current `FactCheckerTests`).

### `Tests/Game/MasterGameChangeRequestTests.cs` — new (7 tests)

Full change request lifecycle. Getting a real `MasterGameID` for test setup: call `MasterGameAllAsync()` and use the first game's ID (relies on the seed DB containing at least one master game).

**User side (6 new tests):**

| Test | Description |
|---|---|
| `CreateMasterGameChangeRequest_AppearsInMyChangeRequests` | After `CreateMasterGameChangeRequestAsync`, the request appears in `MyMasterGameChangeRequestsAsync` with the matching `MasterGame.MasterGameID` |
| `DeleteMasterGameChangeRequest_DisappearsFromMyChangeRequests` | After `DeleteMasterGameChangeRequestAsync`, the request is absent from `MyMasterGameChangeRequestsAsync` |
| `DismissMasterGameChangeRequest_SetsHiddenFlag` | After `DismissMasterGameChangeRequestAsync`, the request is still in `MyMasterGameChangeRequestsAsync` but `Hidden == true` |
| `DeleteMasterGameChangeRequest_NotOwner_Returns403` | User B attempts to delete User A's change request → `ApiException` 403 |
| `DeleteMasterGameChangeRequest_UnknownID_Returns400` | `DeleteMasterGameChangeRequestAsync` with `Guid.NewGuid()` → `ApiException` 400 |
| `CreateMasterGameChangeRequest_Unauthenticated_Returns401` | Unauthenticated POST → `ApiException` 401 (requires `[Authorize]` fix) |

**FactChecker side (1 new test):**

| Test | Description |
|---|---|
| `CompleteMasterGameChangeRequest_Succeeds` | User creates change request; FactChecker calls `CompleteMasterGameChangeRequestAsync`; request is answered (`Answered == true`) |

**Helper:** Same `GrantFactCheckerRoleAsync(Guid userID)` private helper as in `MasterGameRequestTests`.

---

## Test data strategy

- All user accounts created via `NewUser()` + `RegisterAsync` — never hardcoded credentials.
- Master game IDs for change request tests come from `MasterGameAllAsync().First()` — relies on seed data, not FactChecker setup.
- No direct DB access; all state built through the API.

---

## What is NOT in scope

- `MasterGameYear*`, `MasterGameYearWithStatistics`, `MasterGameYears`, `MasterGameYearInLeagueContext` — read-only endpoints that require meaningful year/league data to assert against; deferred to a future league-focused test pass.
- `GetTopBidsAndDrops`, `GetMostDesiredReviews`, `GetLongestTenuredGames`, `GetMostDreamsDashedGames`, `GetRecentMasterGameChanges` — require processed auction/pick data that won't be present in the bare seed DB; deferred.
- `MasterGameYearInLeagueContext` — requires a PlusUser and a live league; deferred.
