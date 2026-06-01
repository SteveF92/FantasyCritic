# OpenAPI Phase 1 — Controller Return Type Strengthening

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Strengthen all API controller action return types so that `Microsoft.AspNetCore.OpenApi` can generate accurate response schemas from compile-time types alone — without any manual schema overrides.

**Architecture:** Convert `IActionResult` to `ActionResult<T>` wherever a success body type can be named. Command actions (no body) keep `IActionResult` and gain explicit `[ProducesResponseType]` attributes. Anonymous `Ok(new { … })` responses are replaced with named ViewModel classes. Every action gets an explicit `[HttpGet]`/`[HttpPost]` attribute (already done in a prior commit).

**Tech Stack:** ASP.NET Core MVC, `FantasyCritic.Web` project only. No new NuGet packages are added in Phase 1.

> **Scope note:** This plan covers Phase 1 only. Do not proceed to Phase 2 (adding `Microsoft.AspNetCore.OpenApi` or any other package). Phase 2 will be planned separately once Phase 1 is complete and the build is green.

---

## Progress so far

These steps were completed manually before this plan was written:

- [x] HTTP verb attributes (`[HttpGet]`, `[HttpPost]`) added to all actions that were missing them
- [x] `AccountController` — response types tightened
- [x] `ActionRunnerController` — response types tightened
- [x] `AdminController` — `[ProducesResponseType]` attributes added to command actions
- [x] `FactCheckerController` — response types tightened

---

## Task 1: `GameController` + `GeneralController`

These two controllers have the smallest remaining surface and establish the two core patterns — command actions (no body) and typed read actions — that all subsequent tasks will follow.

**Files:**
- Modify: `src/FantasyCritic.Web/Controllers/API/GameController.cs`
- Modify: `src/FantasyCritic.Web/Controllers/API/GeneralController.cs`

### Pattern A — Command action (no body): add `[ProducesResponseType]` attributes, keep `IActionResult`

The six `[HttpPost]` mutations in `GameController` all return `Ok()` on success plus some mix of `BadRequest`, `StatusCode(403)`, and `NotFound`. They have no typed response body, so `IActionResult` is correct. What's missing are the explicit status-code annotations.

- [ ] **Step 1: Add `[ProducesResponseType]` to `CreateMasterGameRequest`**

This action only ever returns `Ok()`:

```csharp
[HttpPost]
[ProducesResponseType(StatusCodes.Status200OK)]
public async Task<IActionResult> CreateMasterGameRequest([FromBody] MasterGameRequestRequest request)
```

- [ ] **Step 2: Add `[ProducesResponseType]` to `CreateMasterGameChangeRequest`**

This action returns `NotFound()` or `Ok()`:

```csharp
[HttpPost]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> CreateMasterGameChangeRequest([FromBody] MasterGameChangeRequestRequest request)
```

- [ ] **Step 3: Add `[ProducesResponseType]` to `DeleteMasterGameRequest`, `DeleteMasterGameChangeRequest`, `DismissMasterGameRequest`, `DismissMasterGameChangeRequest`**

All four follow the same pattern — `BadRequest(string)`, `StatusCode(403)`, or `Ok()`:

```csharp
[HttpPost]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public async Task<IActionResult> DeleteMasterGameRequest([FromBody] MasterGameRequestDeletionRequest request)
```

Apply the same three attributes to `DeleteMasterGameChangeRequest`, `DismissMasterGameRequest`, and `DismissMasterGameChangeRequest`.

### Pattern B — Read action with a typed body: convert to `ActionResult<T>`

- [ ] **Step 4: Convert `LeagueYearsWithMasterGame` to `ActionResult<T>`**

Current signature:

```csharp
[HttpGet("{masterGameID}")]
public async Task<IActionResult> LeagueYearsWithMasterGame(Guid masterGameID)
```

Change to:

```csharp
[HttpGet("{masterGameID}")]
public async Task<ActionResult<List<LeagueYearWithMasterGameViewModel>>> LeagueYearsWithMasterGame(Guid masterGameID)
```

The method body does not change — `Ok(viewModels)` and `Ok(new List<LeagueYearWithMasterGameViewModel>())` both compile as-is under `ActionResult<List<LeagueYearWithMasterGameViewModel>>`. No `[ProducesResponseType]` is needed for the 200; the generator infers it from `T`. There are no non-200 paths in this action.

### Pattern C — Non-JSON content: add `[Produces]`, keep `IActionResult`

- [ ] **Step 5: Add `[Produces]` to `SiteAnnouncementsRss` in `GeneralController`**

This action returns XML via `Content(xml, "application/rss+xml", Encoding.UTF8)`. The return type stays `IActionResult` (no typed body), but the media type should be declared:

```csharp
[HttpGet]
[HttpGet("/rss/announcements")]
[Produces("application/rss+xml")]
[ProducesResponseType(StatusCodes.Status200OK)]
public async Task<IActionResult> SiteAnnouncementsRss()
```

### Verify and commit

- [ ] **Step 6: Build the solution**

```powershell
cd I:\CodeProjects\FantasyCritic
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj
```

Expected: build succeeds with 0 errors.

- [ ] **Step 7: Commit**

```powershell
git add src/FantasyCritic.Web/Controllers/API/GameController.cs `
        src/FantasyCritic.Web/Controllers/API/GeneralController.cs
git commit -m "Strengthen return types in GameController and GeneralController."
```

---

## Task 2: `CombinedDataController` — replace anonymous types with named records

Both actions in this controller return `Ok(new { … })` with anonymous types. The OpenAPI generator cannot produce a schema for anonymous types; they must be replaced with named types. The user has specified: use `record` types, placed in `src/FantasyCritic.Web/Models/Responses/Combined/`.

**Files:**
- Create: `src/FantasyCritic.Web/Models/Responses/Combined/BasicDataViewModel.cs`
- Create: `src/FantasyCritic.Web/Models/Responses/Combined/HomePageDataViewModel.cs`
- Modify: `src/FantasyCritic.Web/Controllers/API/CombinedDataController.cs`

### Step 1: Create `BasicDataViewModel`

Create `src/FantasyCritic.Web/Models/Responses/Combined/BasicDataViewModel.cs`:

```csharp
using FantasyCritic.Lib.SharedSerialization.API;

namespace FantasyCritic.Web.Models.Responses.Combined;

public record BasicDataViewModel(
    BidTimesViewModel BidTimes,
    List<MasterGameTagViewModel> MasterGameTags,
    LeagueOptionsViewModel LeagueOptions,
    List<SupportedYearViewModel> SupportedYears);
```

### Step 2: Create `HomePageDataViewModel`

Create `src/FantasyCritic.Web/Models/Responses/Combined/HomePageDataViewModel.cs`:

```csharp
using FantasyCritic.Web.Models.Responses.Conferences;
using FantasyCritic.Web.Models.Responses.Royale;

namespace FantasyCritic.Web.Models.Responses.Combined;

public record HomePageDataViewModel(
    List<LeagueWithStatusViewModel> MyLeagues,
    List<CompleteLeagueInviteViewModel> MyInvites,
    List<MinimalConferenceViewModel> MyConferences,
    TopBidsAndDropsSetViewModel? TopBidsAndDrops,
    GameNewsViewModel MyGameNews,
    List<PublicLeagueYearViewModel> PublicLeagues,
    RoyaleYearQuarterViewModel ActiveRoyaleQuarter,
    Guid? UserRoyalePublisherID);
```

### Step 3: Update `CombinedDataController`

Replace the two `Task<IActionResult>` actions. Add the `Combined` using and change both methods.

Add to usings at the top of the file:
```csharp
using FantasyCritic.Web.Models.Responses.Combined;
using Microsoft.AspNetCore.Http;
```

**`BasicData` — before:**
```csharp
[HttpGet]
[AllowAnonymous]
public async Task<IActionResult> BasicData()
{
    // ... (body builds bidTimes, masterGameTags, leagueOptions, supportedYears)
    var vm = new
    {
        BidTimes = bidTimes,
        MasterGameTags = masterGameTags,
        LeagueOptions = leagueOptions,
        SupportedYears = supportedYears
    };
    return Ok(vm);
}
```

**`BasicData` — after:**
```csharp
[HttpGet]
[AllowAnonymous]
[ProducesResponseType(StatusCodes.Status200OK)]
public async Task<ActionResult<BasicDataViewModel>> BasicData()
{
    // ... body unchanged up to the var vm = ... line
    var vm = new BasicDataViewModel(bidTimes, masterGameTags, leagueOptions, supportedYears);
    return Ok(vm);
}
```

**`HomePageData` — before:**
```csharp
[HttpGet]
public async Task<IActionResult> HomePageData()
{
    // ...
    var myInviteViewModels = homePageData.InvitedLeagues.Select(x => new CompleteLeagueInviteViewModel(x));
    // ...
    var vm = new
    {
        MyLeagues = myLeagueViewModels,
        MyInvites = myInviteViewModels,
        MyConferences = myConferenceViewModels,
        TopBidsAndDrops = completeTopBidsAndDropsViewModel,
        MyGameNews = myGameNewsViewModel,
        PublicLeagues = publicLeagueViewModels,
        ActiveRoyaleQuarter = activeRoyaleQuarterViewModel,
        UserRoyalePublisherID = homePageData.ActiveYearQuarterRoyalePublisherID
    };
    return Ok(vm);
}
```

**`HomePageData` — after:**
```csharp
[HttpGet]
[ProducesResponseType(StatusCodes.Status200OK)]
public async Task<ActionResult<HomePageDataViewModel>> HomePageData()
{
    // ...
    // Add .ToList() since the record expects List<T>, not IEnumerable<T>:
    var myInviteViewModels = homePageData.InvitedLeagues.Select(x => new CompleteLeagueInviteViewModel(x)).ToList();
    // ... rest of body unchanged until vm construction ...
    var vm = new HomePageDataViewModel(
        myLeagueViewModels,
        myInviteViewModels,
        myConferenceViewModels,
        completeTopBidsAndDropsViewModel,
        myGameNewsViewModel,
        publicLeagueViewModels,
        activeRoyaleQuarterViewModel,
        homePageData.ActiveYearQuarterRoyalePublisherID);
    return Ok(vm);
}
```

### Verify and commit

- [ ] **Step 4: Build**

```powershell
cd I:\CodeProjects\FantasyCritic
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj
```

Expected: 0 errors.

- [ ] **Step 5: Commit**

```powershell
cd I:\CodeProjects\FantasyCritic
git add src/FantasyCritic.Web/Models/Responses/Combined/BasicDataViewModel.cs `
        src/FantasyCritic.Web/Models/Responses/Combined/HomePageDataViewModel.cs `
        src/FantasyCritic.Web/Controllers/API/CombinedDataController.cs
git commit -m "Replace anonymous types in CombinedDataController with named records."
```

---

## Task 3: `ConferenceController`

**File:** `src/FantasyCritic.Web/Controllers/API/ConferenceController.cs`

**Return style convention:** For typed reads, return the value directly — do NOT wrap in `Ok()`. For void commands, keep `return Ok();`. The controller already imports `Microsoft.AspNetCore.Mvc`; add `using Microsoft.AspNetCore.Http;` for `StatusCodes`.

**FailedResult cast:** `FailedResult` is typed as `IActionResult` (an interface). `ActionResult<T>` has an implicit conversion from the abstract `ActionResult` class but NOT from the interface. In any typed `ActionResult<T>` method that passes through a FailedResult, cast it explicitly: `return (ActionResult)record.FailedResult;`. All concrete types the helpers return (`BadRequestResult`, `UnauthorizedResult`, `ForbidResult`, `StatusCodeResult`) derive from `ActionResult`, so this cast is safe.

### FailedResult non-200 attributes

Most actions in this controller delegate failure paths to `GetExistingConference*`/`GetExistingConferenceYear*`/`GetExistingLeague*` helpers whose `FailedResult` can produce **400**, **401**, or **403** depending on auth state. Any action that calls one of these helpers gets:

```csharp
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
```

Actions with *additional* explicit `BadRequest(...)` calls still just need those three (BadRequest is already covered). Actions that also do explicit `StatusCode(403, "…")` are already covered by the 403 attribute.

### Actions to convert to `ActionResult<T>` (return value directly, no `Ok()`)

| Method | New return type | Notes |
|---|---|---|
| `MyConferences` | `ActionResult<List<MinimalConferenceViewModel>>` | No FailedResult, no non-200 |
| `CreateConference` | `ActionResult<Guid>` | Returns `conference.Value.ConferenceID`; add `[ProducesResponseType(400)]` |
| `AvailableYears` | `ActionResult<IEnumerable<int>>` | Has FailedResult → add 400/401/403 |
| `GetConference` | `ActionResult<ConferenceViewModel>` | Has FailedResult → add 400/401/403 |
| `GetConferenceYear` | `ActionResult<ConferenceYearViewModel>` | Has FailedResult → add 400/401/403 |
| `InviteLinks` | `ActionResult<IEnumerable<ConferenceInviteLinkViewModel>>` | Has FailedResult → add 400/401/403 |

### Actions that stay `IActionResult` (void success — add `[ProducesResponseType(200)]`)

All remaining 16 POST mutations: `AddLeagueToConference`, `AddNewConferenceYear`, `AddNewLeagueYear`, `SetPlayerActiveStatus`, `EditConference`, `CreateInviteLink`, `DeleteInviteLink`, `JoinWithInviteLink`, `RemovePlayerFromConference`, `PromoteNewConferenceManager`, `ReassignLeagueManager`, `AssignLeaguePlayers`, `SetConferenceLeagueLockStatus`, `PostNewConferenceManagerMessage`, `DeleteConferenceManagerMessage`, `DismissManagerMessage`.

Add FailedResult 400/401/403 attributes to all that use `GetExisting*` helpers (i.e., all except `DismissManagerMessage`).

`DismissManagerMessage` only does explicit `BadRequest` — just add `[ProducesResponseType(200)]` and `[ProducesResponseType(400)]`.

### Commit

```powershell
cd I:\CodeProjects\FantasyCritic
git add src/FantasyCritic.Web/Controllers/API/ConferenceController.cs
git commit -m "Strengthen return types in ConferenceController."
```

---

## Task 4: `RoyaleController` + `RoyaleGroupController`

These two controllers have anonymous `Ok(new { … })` returns that require new record types, plus a set of regular typed reads and void commands.

### New record files to create

**`src/FantasyCritic.Web/Models/Responses/Royale/RoyaleQuarterDataViewModel.cs`**

Replaces the anonymous object in `RoyaleController.RoyaleData`:

```csharp
namespace FantasyCritic.Web.Models.Responses.Royale;

public record RoyaleQuarterDataViewModel(
    IEnumerable<RoyaleYearQuarterViewModel> RoyaleYearQuarters,
    RoyaleYearQuarterViewModel RoyaleYearQuarter,
    Guid? UserRoyalePublisherID,
    List<RoyaleStandingsViewModel> RoyaleStandings,
    List<RoyalePublisherViewModel> TopPublishers);
```

**`src/FantasyCritic.Web/Models/Responses/Royale/UserRoyaleHistoryViewModel.cs`**

Replaces the anonymous object in `RoyaleController.UserRoyaleHistory`:

```csharp
namespace FantasyCritic.Web.Models.Responses.Royale;

public record UserRoyaleHistoryViewModel(
    Guid UserID,
    string PlayerName,
    List<RoyaleYearQuarterViewModel> QuartersWon,
    List<RoyalePublisherHistoryViewModel> Publishers);
```

**`src/FantasyCritic.Web/Models/Responses/Royale/CreatedRoyaleGroupViewModel.cs`**

Replaces the `new { GroupID = … }` returns in `RoyaleGroupController` create methods:

```csharp
namespace FantasyCritic.Web.Models.Responses.Royale;

public record CreatedRoyaleGroupViewModel(Guid GroupID);
```

**`src/FantasyCritic.Web/Models/Responses/Royale/RoyaleGroupStatusViewModel.cs`**

Replaces the discriminated anonymous objects in `GetRoyaleGroupForLeague` and `GetRoyaleGroupForConference` (currently `{ HasRoyaleGroup = false }` or `{ HasRoyaleGroup = true, RoyaleGroup = vm }`):

```csharp
namespace FantasyCritic.Web.Models.Responses.Royale;

public record RoyaleGroupStatusViewModel(bool HasRoyaleGroup, RoyaleGroupViewModel? RoyaleGroup);
```

Return `new RoyaleGroupStatusViewModel(false, null)` and `new RoyaleGroupStatusViewModel(true, vm)` respectively.

### `RoyaleController` — actions to convert

Add `using Microsoft.AspNetCore.Http;` to the controller.

**FailedResult cast:** `FailedResult` is typed as `IActionResult`. In any `ActionResult<T>` method with a FailedResult passthrough, cast it: `return (ActionResult)record.FailedResult;`.

**`ActionResult<T>` (return value directly):**

| Method | New return type | Non-200 attributes |
|---|---|---|
| `RoyaleQuarters` | `ActionResult<IEnumerable<RoyaleYearQuarterViewModel>>` | none |
| `ActiveRoyaleQuarter` | `ActionResult<RoyaleYearQuarterViewModel>` | none |
| `RoyaleQuarter` | `ActionResult<RoyaleYearQuarterViewModel>` | `[ProducesResponseType(404)]` |
| `CreateRoyalePublisher` | `ActionResult<Guid>` | `[ProducesResponseType(400)]` |
| `GetRoyalePublisher` | `ActionResult<RoyalePublisherViewModel>` | `[ProducesResponseType(404)]` |
| `RoyaleData` | `ActionResult<RoyaleQuarterDataViewModel>` | `[ProducesResponseType(404)]` |
| `PurchaseGame` | `ActionResult<PlayerClaimResultViewModel>` | `[ProducesResponseType(400)]`, `[ProducesResponseType(403)]`, `[ProducesResponseType(404)]` |
| `UserRoyaleHistory` | `ActionResult<UserRoyaleHistoryViewModel>` | `[ProducesResponseType(404)]` |

**`IActionResult` (void commands):** `ChangePublisherName`, `ChangePublisherIcon`, `ChangePublisherSlogan`, `SellGame`, `SetAdvertisingMoney`. Each has NotFound + Forbidden + BadRequest paths: add `[ProducesResponseType(200)]`, `[ProducesResponseType(400)]`, `[ProducesResponseType(403)]`, `[ProducesResponseType(404)]`.

### `RoyaleGroupController` — actions to convert

Add `using Microsoft.AspNetCore.Http;` to the controller.

**FailedResult cast:** This controller does not use FailedResult helpers — no cast needed.

**`ActionResult<T>` (return value directly):**

| Method | New return type | Non-200 |
|---|---|---|
| `GetRoyaleGroup` | `ActionResult<RoyaleGroupViewModel>` | `[404]` |
| `GetRoyaleGroupQuarter` | `ActionResult<RoyaleGroupQuarterViewModel>` | `[404]` |
| `CreateManualRoyaleGroup` | `ActionResult<CreatedRoyaleGroupViewModel>` | `[400]` |
| `CreateLeagueTiedRoyaleGroup` | `ActionResult<CreatedRoyaleGroupViewModel>` | `[400]`, `[403]` |
| `CreateConferenceTiedRoyaleGroup` | `ActionResult<CreatedRoyaleGroupViewModel>` | `[400]`, `[403]` |
| `CreateGroupInviteLink` | `ActionResult<RoyaleGroupInviteLinkViewModel>` | `[400]` |
| `SearchRoyaleGroups` | `ActionResult<List<RoyaleGroupViewModel>>` | none |
| `GetGroupsForUser` | `ActionResult<List<RoyaleGroupViewModel>>` | none |
| `GetGroupInviteLinks` | `ActionResult<List<RoyaleGroupInviteLinkViewModel>>` | `[403]`, `[404]` |
| `GetRulesBasedGroups` | `ActionResult<List<RoyaleGroupViewModel>>` | none |
| `GetRoyaleGroupData` | `ActionResult<RoyaleGroupDataViewModel>` | `[404]` |
| `GetRoyaleGroupForLeague` | `ActionResult<RoyaleGroupStatusViewModel>` | none |
| `GetRoyaleGroupForConference` | `ActionResult<RoyaleGroupStatusViewModel>` | none |

**`IActionResult` (void commands):** `DeactivateGroupInviteLink`, `JoinWithInviteLink`, `LeaveGroup`, `RemoveMember`. Add `[ProducesResponseType(200)]` and `[ProducesResponseType(400)]` on each.

### Commit

```powershell
cd I:\CodeProjects\FantasyCritic
git add src/FantasyCritic.Web/Models/Responses/Royale/RoyaleQuarterDataViewModel.cs `
        src/FantasyCritic.Web/Models/Responses/Royale/UserRoyaleHistoryViewModel.cs `
        src/FantasyCritic.Web/Models/Responses/Royale/CreatedRoyaleGroupViewModel.cs `
        src/FantasyCritic.Web/Models/Responses/Royale/RoyaleGroupStatusViewModel.cs `
        src/FantasyCritic.Web/Controllers/API/RoyaleController.cs `
        src/FantasyCritic.Web/Controllers/API/RoyaleGroupController.cs
git commit -m "Strengthen return types in RoyaleController and RoyaleGroupController."
```

---

## Task 5: `LeagueManagerController`

**File:** `src/FantasyCritic.Web/Controllers/API/LeagueManagerController.cs`

Add `using Microsoft.AspNetCore.Http;`. Every action uses the FailedResult pattern from `GetExistingLeague*`/`GetExistingLeagueYear*` helpers (except `CreateLeague`) — add `[ProducesResponseType(400)]`, `[ProducesResponseType(401)]`, `[ProducesResponseType(403)]` to all FailedResult actions. Return values directly (no `Ok()` wrapper) for typed actions.

**FailedResult cast:** `FailedResult` is typed as `IActionResult` (interface). In any `ActionResult<T>` method with a FailedResult passthrough, cast it: `return (ActionResult)record.FailedResult;`.

### Actions to convert to `ActionResult<T>`

| Method | New return type | Extra non-200 |
|---|---|---|
| `CreateLeague` | `ActionResult<Guid>` | `[400]` only (no FailedResult) |
| `AvailableYears` | `ActionResult<IEnumerable<int>>` | FailedResult → 400/401/403 |
| `InviteLinks` | `ActionResult<IEnumerable<LeagueInviteLinkViewModel>>` | FailedResult → 400/401/403 |
| `RemovePlayer` | `ActionResult<string>` | FailedResult → 400/401/403 |
| `ManagerClaimGame` | `ActionResult<ManagerClaimResultViewModel>` | FailedResult → 400/401/403 |
| `ManagerAssociateGame` | `ActionResult<ManagerClaimResultViewModel>` | `[400]` + FailedResult → 400/401/403 |
| `ManagerDraftGame` | `ActionResult<ManagerClaimResultViewModel>` | `[400]` + FailedResult → 400/401/403 |

### All remaining actions — `IActionResult` (void commands)

Add `[ProducesResponseType(200)]` and the FailedResult set `[400]`, `[401]`, `[403]` (plus any extra explicit `BadRequest` — already covered) to every other action. The full list: `AddNewLeagueYear`, `ChangeLeagueOptions`, `EditLeagueYearSettings`, `InvitePlayer`, `CreateInviteLink`, `DeleteInviteLink`, `RescindInvite`, `ReassignPublisher`, `CreatePublisherForUser`, `EditPublisher`, `RemovePublisher`, `SetPlayerActiveStatus`, `SetAutoDraft`, `RemovePublisherGame`, `ManuallyScorePublisherGame`, `RemoveManualPublisherGameScore`, `ManuallySetWillNotRelease`, `StartDraft`, `ResetDraft`, `SetDraftOrder`, `SetDraftPause`, `UndoLastDraftAction`, `SetGameEligibilityOverride`, `SetGameTagOverride`, `PromoteNewLeagueManager`, `PostNewManagerMessage`, `DeleteManagerMessage`, `RejectTrade`, `ExecuteTrade`, `CreateSpecialAuction`, `CancelSpecialAuction`, `SetUnderReviewStatus`.

`RescindInvite` and `SetAutoDraft` have an extra explicit `StatusCode(403)` — already covered by the 403 attribute.

### Commit

```powershell
cd I:\CodeProjects\FantasyCritic
git add src/FantasyCritic.Web/Controllers/API/LeagueManagerController.cs
git commit -m "Strengthen return types in LeagueManagerController."
```

---

## Task 6: `LeagueController`

**File:** `src/FantasyCritic.Web/Controllers/API/LeagueController.cs`

The largest controller. Add `using Microsoft.AspNetCore.Http;`. FailedResult pattern → `[400]`, `[401]`, `[403]`. Many reads also call `UnauthorizedOrForbid` (→ **401** or **403**) — these are already covered. Return values directly for typed reads.

**FailedResult cast:** `FailedResult` is typed as `IActionResult` (interface). In any `ActionResult<T>` method with a FailedResult passthrough, cast it: `return (ActionResult)record.FailedResult;`.

### File download actions — keep `IActionResult`, add `[Produces]`

| Method | `[Produces(...)]` attribute | Non-200 |
|---|---|---|
| `ExportLeagueActionSetsToCsv` | `[Produces("text/csv")]` | `[400]`, `[401]`, `[403]` |
| `DownloadConsolidatedLeagueData` | `[Produces("application/zip")]` | `[400]`, `[401]`, `[403]` |
| `DownloadConsolidatedLeagueYearData` | `[Produces("application/json")]` | `[400]`, `[401]`, `[403]` |

Also add `[ProducesResponseType(200)]` to each of these.

### Actions to convert to `ActionResult<T>` (return value directly)

Read the controller to determine the exact ViewModel type returned by each method. The table below identifies the success model — verify the variable name in the method body:

| Method | New return type | Non-200 |
|---|---|---|
| `MyLeagues` | `ActionResult<List<LeagueWithStatusViewModel>>` | none |
| `MyInvites` | `ActionResult<IEnumerable<CompleteLeagueInviteViewModel>>` | none |
| `PublicLeagues` | `ActionResult<List<PublicLeagueYearViewModel>>` | none |
| `GetLeague` | `ActionResult<LeagueViewModel>` | FailedResult 400/401/403 + `[401]`/`[403]` from UnauthorizedOrForbid (already covered) |
| `GetLeagueYear` | `ActionResult<LeagueYearViewModel>` | FailedResult + UnauthorizedOrForbid → 400/401/403 |
| `GetLeagueAllTimeStats` | read the method — return type is whatever the Ok wraps | FailedResult + UnauthorizedOrForbid → 400/401/403 |
| `GetLeagueYearForPublisher` | `ActionResult<LeagueYearViewModel>` | FailedResult + `[404]` + UnauthorizedOrForbid |
| `GetMyPublishers` | `ActionResult<IEnumerable<LeaguePublisherViewModel>>` (read method for exact type) | FailedResult 400/401/403 |
| `GetQueuedGameYearsForLeague` | `ActionResult<List<PossibleMasterGameYearViewModel>>` | FailedResult 400/401/403 |
| `GetLeagueActions` | read the method — return type is what `Ok(...)` wraps | FailedResult + 401/403 |
| `GetLeagueActionSets` | read the method | FailedResult + 401/403 |
| `GetPublisher` | `ActionResult<PublisherViewModel>` | FailedResult + 401/403 |
| `GetLeagueYearOptions` | `ActionResult<LeagueYearSettingsViewModel>` | FailedResult + 401/403 |
| `MakePickupBid` | `ActionResult<PickupBidResultViewModel>` (read method) | `[400]` + FailedResult 400/401/403 |
| `EditPickupBid` | `ActionResult<PickupBidResultViewModel>` | `[400]`, `[403]` + FailedResult |
| `DraftGame` | `ActionResult<PlayerClaimResultViewModel>` | `[400]` + FailedResult |
| `PossibleMasterGames` | `ActionResult<List<PossibleMasterGameYearViewModel>>` | FailedResult |
| `TopAvailableGames` | `ActionResult<List<PossibleMasterGameYearViewModel>>` | `[400]` + FailedResult |
| `ThisWeeksPublicBiddingGames` | `ActionResult<List<PossibleMasterGameYearViewModel>>` | `[400]` + FailedResult |
| `PossibleCounterPicks` | `ActionResult<List<PublisherGameViewModel>>` | FailedResult |
| `MakeDropRequest` | `ActionResult<DropGameResultViewModel>` | `[400]` + FailedResult |
| `UseSuperDrop` | `ActionResult<DropGameResultViewModel>` | FailedResult |
| `CurrentQueuedGameYears` | `ActionResult<List<PossibleMasterGameYearViewModel>>` | FailedResult |
| `AddGameToQueue` | `ActionResult<QueueResultViewModel>` | `[400]` + FailedResult |
| `TradeHistory` | `ActionResult<IEnumerable<TradeViewModel>>` | FailedResult + 401/403 |

### `IActionResult` void commands — add `[ProducesResponseType(200)]` + error codes

`AcceptInvite`, `JoinWithInviteLink`, `CreatePublisher`, `ChangePublisherName`, `ChangePublisherIcon`, `ChangePublisherSlogan`, `SetAutoDraft`, `DeletePickupBid`, `SetBidPriorities`, `FollowLeague`, `UnfollowLeague`, `DeleteDropRequest`, `DeleteQueuedGame`, `SetQueueRankings`, `ReorderPublisherGames`, `SetArchiveStatus`, `ProposeTrade`, `RescindTrade`, `AcceptTrade`, `RejectTrade`, `VoteOnTrade`, `DeleteTradeVote`, `DeclineInvite`, `DismissManagerMessage`.

For each: add `[ProducesResponseType(200)]`. Methods that use FailedResult also get `[400]`, `[401]`, `[403]`. Methods with explicit `BadRequest` or `Forbid` without FailedResult still get `[400]`/`[403]` as applicable. `DismissManagerMessage` has only an explicit `BadRequest` — just `[200]` and `[400]`.

### Commit

```powershell
cd I:\CodeProjects\FantasyCritic
git add src/FantasyCritic.Web/Controllers/API/LeagueController.cs
git commit -m "Strengthen return types in LeagueController."
```
