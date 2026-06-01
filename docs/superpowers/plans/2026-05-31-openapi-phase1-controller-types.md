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

## Remaining tasks (to be written)

The following controllers still need the same treatment. Tasks will be written and added to this plan in subsequent sessions:

| Controller | Remaining `IActionResult` count | Notes |
|---|---|---|
| `ConferenceController` | 22 | Mix of typed reads and command mutations |
| `LeagueController` | ~52 | Largest controller; mix of reads, mutations, and file downloads |
| `LeagueManagerController` | ~40 | All command/mutation style, some return typed bodies |
| `RoyaleController` | ~13 | Includes anonymous types — needs new ViewModel classes |
| `RoyaleGroupController` | 17 | Includes one anonymous `{ GroupID }` response |
