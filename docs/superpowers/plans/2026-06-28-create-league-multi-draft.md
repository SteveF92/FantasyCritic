# Create League as Multi Draft — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Allow leagues and conferences to be created with multiple drafts from the outset, and redesign the creation UI with separate Game Mode and Experience Level selectors.

**Architecture:** Extract `GamesToDraft`/`CounterPicksToDraft` out of `LeagueYearParameters` into a sibling `Drafts` list on creation parameters; add `EditLeagueYearRequest` that carries an optional `FirstDraft?` on the edit path. Frontend: new `leagueCreationPresets.ts` utility, `leagueCreationPresets.vue` selector component, and `DraftCreationSettings.vue` drafts-array component; slim down `leagueYearSettings.vue` to be a pure settings form.

**Tech Stack:** ASP.NET Core 10, C#, NodaTime, CSharpFunctionalExtensions, Vue 3 Options API, TypeScript, BootstrapVue

---

## File Map

| Action | File |
|--------|------|
| **Create** | `src/FantasyCritic.Lib/Domain/Requests/DraftParameters.cs` |
| **Create** | `src/FantasyCritic.Web/Models/Requests/LeagueManager/DraftSettingsRequest.cs` |
| **Create** | `src/FantasyCritic.Web/Models/Requests/LeagueManager/EditLeagueYearRequest.cs` |
| **Create** | `src/FantasyCritic.Web/ClientApp/src/utilities/leagueCreationPresets.ts` |
| **Create** | `src/FantasyCritic.Web/ClientApp/src/components/leagueCreationPresets.vue` |
| **Create** | `src/FantasyCritic.Web/ClientApp/src/components/DraftCreationSettings.vue` |
| **Modify** | `src/FantasyCritic.Lib/Domain/Requests/LeagueYearParameters.cs` — remove `GamesToDraft`, `CounterPicksToDraft` |
| **Modify** | `src/FantasyCritic.Lib/Domain/Requests/LeagueCreationParameters.cs` — add `Drafts` |
| **Modify** | `src/FantasyCritic.Lib/Domain/Conferences/ConferenceCreationParameters.cs` — add `Drafts` |
| **Modify** | `src/FantasyCritic.Lib/Domain/LeagueDraft.cs` — add `UpdateDraft` overload |
| **Modify** | `src/FantasyCritic.Lib/Interfaces/IFantasyCriticRepo.cs` — `CreateLeague` accepts `IReadOnlyList<LeagueDraft>` |
| **Modify** | `src/FantasyCritic.Lib/Interfaces/IConferenceRepo.cs` — same |
| **Modify** | `src/FantasyCritic.Lib/Services/FantasyCriticService.cs` — `CreateLeague`, `EditLeague` |
| **Modify** | `src/FantasyCritic.Lib/Services/ConferenceService.cs` — `CreateConference` |
| **Modify** | `src/FantasyCritic.MySQL/Repositories/MySQLFantasyCriticRepo.cs` — `CreateLeague` |
| **Modify** | `src/FantasyCritic.MySQL/Repositories/MySQLConferenceRepo.cs` — `CreateConference` |
| **Modify** | `src/FantasyCritic.FakeRepo/Repositories/FakeFantasyCriticRepo.cs` — `CreateLeague` |
| **Modify** | `src/FantasyCritic.FakeRepo/Repositories/FakeConferenceRepo.cs` — `CreateConference` |
| **Modify** | `src/FantasyCritic.Web/Models/RoundTrip/LeagueYearSettingsViewModel.cs` — remove draft fields |
| **Modify** | `src/FantasyCritic.Web/Models/Requests/LeagueManager/CreateLeagueRequest.cs` — add `Drafts` |
| **Modify** | `src/FantasyCritic.Web/Models/Requests/Conferences/CreateConferenceRequest.cs` — add `Drafts` |
| **Modify** | `src/FantasyCritic.Web/Controllers/API/LeagueManagerController.cs` — `EditLeagueYearSettings` |
| **Modify** | `src/FantasyCritic.Web/Controllers/API/ConferenceController.cs` — `CreateConference` |
| **Modify** | `src/FantasyCritic.IntegrationTests/Helpers/LeagueScenario.cs` |
| **Modify** | `src/FantasyCritic.IntegrationTests/Helpers/LeagueTestHelpers.cs` |
| **Modify** | `src/FantasyCritic.IntegrationTests/Tests/League/Setup/LeagueSetupTests.cs` |
| **Create** | `src/FantasyCritic.IntegrationTests/Tests/League/Setup/MultiDraftCreationTests.cs` |
| **Modify** | `src/FantasyCritic.Web/ClientApp/src/components/leagueYearSettings.vue` |
| **Modify** | `src/FantasyCritic.Web/ClientApp/src/views/createLeague.vue` |
| **Modify** | `src/FantasyCritic.Web/ClientApp/src/views/editLeague.vue` |
| **Modify** | `src/FantasyCritic.Web/ClientApp/src/views/createConference.vue` |

---

## Task 1: New domain and request types (pure additions, no breaking changes)

**Files:**
- Create: `src/FantasyCritic.Lib/Domain/Requests/DraftParameters.cs`
- Create: `src/FantasyCritic.Web/Models/Requests/LeagueManager/DraftSettingsRequest.cs`

- [ ] **Step 1: Create `DraftParameters` in Lib**

```csharp
// src/FantasyCritic.Lib/Domain/Requests/DraftParameters.cs
using NodaTime;

namespace FantasyCritic.Lib.Domain.Requests;

public record DraftParameters(
    string? Name,
    LocalDate? ScheduledDate,
    int GamesToDraft,
    int CounterPicksToDraft);
```

- [ ] **Step 2: Create `DraftSettingsRequest` in Web**

`Name` is nullable — callers pre-fill defaults but the server accepts null and resolves the name.

```csharp
// src/FantasyCritic.Web/Models/Requests/LeagueManager/DraftSettingsRequest.cs
using NodaTime;

namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record DraftSettingsRequest(
    string? Name,
    LocalDate? ScheduledDate,
    int GamesToDraft,
    int CounterPicksToDraft)
{
    public DraftParameters ToDomain(int draftIndex)
    {
        string resolvedName = Name ?? (draftIndex == 0 ? "Initial Draft" : $"Draft {draftIndex + 1}");
        return new DraftParameters(resolvedName, ScheduledDate, GamesToDraft, CounterPicksToDraft);
    }
}
```

- [ ] **Step 3: Verify project builds**

```powershell
cd I:\CodeProjects\FantasyCritic\src
dotnet build FantasyCritic.Lib FantasyCritic.Web
```

Expected: 0 errors.

- [ ] **Step 4: Commit**

```powershell
cd I:\CodeProjects\FantasyCritic
git add src/FantasyCritic.Lib/Domain/Requests/DraftParameters.cs src/FantasyCritic.Web/Models/Requests/LeagueManager/DraftSettingsRequest.cs
git commit -m "Add DraftParameters (Lib) and DraftSettingsRequest (Web) types."
```

---

## Task 2: Remove `GamesToDraft`/`CounterPicksToDraft` from `LeagueYearParameters`

**Files:**
- Modify: `src/FantasyCritic.Lib/Domain/Requests/LeagueYearParameters.cs`
- Modify: `src/FantasyCritic.Web/Models/RoundTrip/LeagueYearSettingsViewModel.cs` (the only call site)

> **Note:** `LeagueYearParameters` is constructed in exactly one production location: `LeagueYearSettingsViewModel.ToDomain()`. Run `dotnet build` after each step — the compiler will flag every remaining reference.

- [ ] **Step 1: Remove the two fields from `LeagueYearParameters`**

Open `src/FantasyCritic.Lib/Domain/Requests/LeagueYearParameters.cs`. Remove:
- `int gamesToDraft` and `int counterPicksToDraft` constructor parameters (4th and 6th positions in the list)
- `public int GamesToDraft { get; }` property
- `public int CounterPicksToDraft { get; }` property

The constructor should now read:

```csharp
public LeagueYearParameters(
    Guid leagueID,
    int year,
    string? leagueYearName,
    int standardGames,
    int counterPicks,
    int unrestrictedReleaseStatusDroppableGames,
    int willNotReleaseDroppableGames,
    int willReleaseDroppableGames,
    bool dropOnlyDraftGames,
    bool grantSuperDrops,
    bool counterPicksBlockDrops,
    bool allowMoveIntoIneligible,
    int minimumBidAmount,
    bool enableBids,
    IEnumerable<LeagueTagStatus> leagueTags,
    IEnumerable<SpecialGameSlot> specialGameSlots,
    DraftSystem draftSystem,
    PickupSystem pickupSystem,
    ScoringSystem scoringSystem,
    TradingSystem tradingSystem,
    TiebreakSystem tiebreakSystem,
    ReleaseSystem releaseSystem,
    IneligibleGameSystem ineligibleGameSystem,
    AnnualDate counterPickDeadline,
    AnnualDate? mightReleaseDroppableDate)
```

- [ ] **Step 2: Update `LeagueYearSettingsViewModel.ToDomain()`**

In `src/FantasyCritic.Web/Models/RoundTrip/LeagueYearSettingsViewModel.cs`, find the `ToDomain` method (~line 176). Remove `GamesToDraft` and `CounterPicksToDraft` from the `LeagueYearParameters` constructor call. The call starts at ~line 212 and should become:

```csharp
LeagueYearParameters parameters = new LeagueYearParameters(LeagueID, Year, LeagueYearName, StandardGames,
    CounterPicks,
    unrestrictedReleaseStatusDroppableGames, willNotReleaseDroppableGames, willReleaseDroppableGames,
    DropOnlyDraftGames, GrantSuperDrops, CounterPicksBlockDrops, AllowMoveIntoIneligible,
    MinimumBidAmount, EnableBids,
    leagueTags, specialGameSlots, draftSystem, pickupSystem, scoringSystem, tradingSystem,
    tiebreakSystem, releaseSystem, ineligibleGameSystem, counterPickDeadline, mightReleaseDroppableDate);
```

Also remove `GamesToDraft` and `CounterPicksToDraft` properties, their `[Range]` attributes, and their assignments from both constructors:
- Remove from the `[JsonConstructor]` constructor parameters (`int gamesToDraft`, `int counterPicksToDraft`)
- Remove from constructor body assignments (`GamesToDraft = gamesToDraft;`, etc.)
- Remove from the `LeagueYear`-based constructor body (`GamesToDraft = leagueYear.FirstDraft.GamesToDraft;`, etc.)
- Remove property declarations and `[Range]` annotations

- [ ] **Step 3: Build and fix all remaining references**

```powershell
cd I:\CodeProjects\FantasyCritic\src
dotnet build
```

The build will fail everywhere `parameters.GamesToDraft` or `parameters.CounterPicksToDraft` is read from a `LeagueYearParameters`. The known sites:

- `FantasyCriticService.CreateLeague` (creates `initialDraft` from `parameters.LeagueYearParameters.GamesToDraft`) — this will be replaced in Task 6; for now comment out the draft creation line and replace it with a `throw new NotImplementedException("Draft creation will be updated in Task 6")` so the build passes.
- `FantasyCriticService.EditLeague` (reads `parameters.GamesToDraft`) — similarly stub: `int gamesToDraft = 0; int counterPicksToDraft = 0; // TODO: wired in Task 7`

Fix all remaining compile errors. Run `dotnet build` until it passes with 0 errors.

- [ ] **Step 4: Commit**

```powershell
cd I:\CodeProjects\FantasyCritic
git add -u
git commit -m "Remove GamesToDraft/CounterPicksToDraft from LeagueYearParameters and LeagueYearSettingsViewModel. Stub service call sites for follow-up tasks."
```

---

## Task 3: Add `Drafts` to creation parameters types

**Files:**
- Modify: `src/FantasyCritic.Lib/Domain/Requests/LeagueCreationParameters.cs`
- Modify: `src/FantasyCritic.Lib/Domain/Conferences/ConferenceCreationParameters.cs`

- [ ] **Step 1: Update `LeagueCreationParameters`**

```csharp
// src/FantasyCritic.Lib/Domain/Requests/LeagueCreationParameters.cs
public class LeagueCreationParameters
{
    public LeagueCreationParameters(
        FantasyCriticUser manager,
        string leagueName,
        bool publicLeague,
        bool testLeague,
        bool customRulesLeague,
        LeagueYearParameters leagueYearParameters,
        IEnumerable<DraftParameters> drafts)
    {
        Manager = manager;
        LeagueName = leagueName;
        PublicLeague = publicLeague;
        TestLeague = testLeague;
        CustomRulesLeague = customRulesLeague;
        LeagueYearParameters = leagueYearParameters;
        Drafts = drafts.ToList();
    }

    public FantasyCriticUser Manager { get; }
    public string LeagueName { get; }
    public bool PublicLeague { get; }
    public bool TestLeague { get; }
    public bool CustomRulesLeague { get; }
    public LeagueYearParameters LeagueYearParameters { get; }
    public IReadOnlyList<DraftParameters> Drafts { get; }
}
```

- [ ] **Step 2: Update `ConferenceCreationParameters`**

```csharp
// src/FantasyCritic.Lib/Domain/Conferences/ConferenceCreationParameters.cs
public class ConferenceCreationParameters
{
    public ConferenceCreationParameters(
        MinimalFantasyCriticUser manager,
        string conferenceName,
        string primaryLeagueName,
        bool customRulesConference,
        LeagueYearParameters leagueYearParameters,
        IEnumerable<DraftParameters> drafts)
    {
        Manager = manager;
        ConferenceName = conferenceName;
        PrimaryLeagueName = primaryLeagueName;
        CustomRulesConference = customRulesConference;
        LeagueYearParameters = leagueYearParameters;
        Drafts = drafts.ToList();
    }

    public MinimalFantasyCriticUser Manager { get; }
    public string ConferenceName { get; }
    public string PrimaryLeagueName { get; }
    public bool CustomRulesConference { get; }
    public LeagueYearParameters LeagueYearParameters { get; }
    public IReadOnlyList<DraftParameters> Drafts { get; }
}
```

- [ ] **Step 3: Build and fix call sites**

```powershell
cd I:\CodeProjects\FantasyCritic\src
dotnet build
```

`LeagueCreationParameters` is constructed in `CreateLeagueRequest.ToDomain()` and `ConferenceCreationParameters` in `CreateConferenceRequest.ToDomain()`. Both constructors now require `IEnumerable<DraftParameters> drafts` as the last argument. Pass an empty list for now:

In `CreateLeagueRequest.ToDomain()`:
```csharp
LeagueCreationParameters parameters = new LeagueCreationParameters(
    manager, LeagueName, PublicLeague, TestLeague, CustomRulesLeague,
    leagueYearParameters,
    Array.Empty<DraftParameters>()); // TODO: replaced in Task 4
return parameters;
```

In `CreateConferenceRequest.ToDomain()`:
```csharp
return new ConferenceCreationParameters(
    manager, ConferenceName, PrimaryLeagueName, CustomRulesConference,
    leagueYearParameters,
    Array.Empty<DraftParameters>()); // TODO: replaced in Task 4
```

Run `dotnet build` until 0 errors.

- [ ] **Step 4: Commit**

```powershell
cd I:\CodeProjects\FantasyCritic
git add -u
git commit -m "Add Drafts list to LeagueCreationParameters and ConferenceCreationParameters."
```

---

## Task 4: Update Web request types — `CreateLeagueRequest` and `CreateConferenceRequest`

**Files:**
- Modify: `src/FantasyCritic.Web/Models/Requests/LeagueManager/CreateLeagueRequest.cs`
- Modify: `src/FantasyCritic.Web/Models/Requests/Conferences/CreateConferenceRequest.cs`

- [ ] **Step 1: Update `CreateLeagueRequest`**

Replace the file with:

```csharp
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Web.Models.RoundTrip;

namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public class CreateLeagueRequest
{
    public CreateLeagueRequest(string leagueName, bool publicLeague, bool testLeague, bool customRulesLeague,
        LeagueYearSettingsViewModel leagueYearSettings, IReadOnlyList<DraftSettingsRequest> drafts)
    {
        LeagueName = leagueName;
        PublicLeague = publicLeague;
        TestLeague = testLeague;
        CustomRulesLeague = customRulesLeague;
        LeagueYearSettings = leagueYearSettings;
        Drafts = drafts;
    }

    public string LeagueName { get; }
    public bool PublicLeague { get; }
    public bool TestLeague { get; }
    public bool CustomRulesLeague { get; }
    public LeagueYearSettingsViewModel LeagueYearSettings { get; }
    public IReadOnlyList<DraftSettingsRequest> Drafts { get; }

    public Result IsValid()
    {
        if (string.IsNullOrWhiteSpace(LeagueName))
            return Result.Failure("You cannot have a blank league name.");

        var settingsValid = LeagueYearSettings.IsValid();
        if (settingsValid.IsFailure)
            return Result.Failure(settingsValid.Error);

        if (Drafts.Count < 1)
            return Result.Failure("At least one draft is required.");

        for (int i = 0; i < Drafts.Count; i++)
        {
            if (Drafts[i].GamesToDraft < 1)
                return Result.Failure($"Draft {i + 1}: games to draft must be at least 1.");
            if (Drafts[i].CounterPicksToDraft < 0)
                return Result.Failure($"Draft {i + 1}: counter picks to draft cannot be negative.");
        }

        int totalGamesToDraft = Drafts.Sum(d => d.GamesToDraft);
        if (totalGamesToDraft > LeagueYearSettings.StandardGames)
            return Result.Failure($"Total games to draft across all drafts ({totalGamesToDraft}) cannot exceed standard games ({LeagueYearSettings.StandardGames}).");

        return Result.Success();
    }

    public LeagueCreationParameters ToDomain(FantasyCriticUser manager, IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        LeagueYearParameters leagueYearParameters = LeagueYearSettings.ToDomain(tagDictionary);
        var draftParams = Drafts.Select((d, i) => d.ToDomain(i)).ToList();
        return new LeagueCreationParameters(manager, LeagueName, PublicLeague, TestLeague, CustomRulesLeague,
            leagueYearParameters, draftParams);
    }
}
```

- [ ] **Step 2: Update `CreateConferenceRequest`**

Apply the same changes to `CreateConferenceRequest`. Add `IReadOnlyList<DraftSettingsRequest> Drafts` property, update the constructor, update `IsValid()` (same draft validations), and update `ToDomain()`:

```csharp
public ConferenceCreationParameters ToDomain(
    MinimalFantasyCriticUser manager,
    IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
{
    LeagueYearParameters leagueYearParameters = LeagueYearSettings.ToDomain(tagDictionary);
    var draftParams = Drafts.Select((d, i) => d.ToDomain(i)).ToList();
    return new ConferenceCreationParameters(manager, ConferenceName, PrimaryLeagueName,
        CustomRulesConference, leagueYearParameters, draftParams);
}
```

Remove the `// TODO: replaced in Task 4` stubs from Task 3.

- [ ] **Step 3: Build**

```powershell
cd I:\CodeProjects\FantasyCritic\src
dotnet build
```

Expected: 0 errors.

- [ ] **Step 4: Commit**

```powershell
cd I:\CodeProjects\FantasyCritic
git add -u
git commit -m "Add Drafts array to CreateLeagueRequest and CreateConferenceRequest with validation."
```

---

## Task 5: Update repo interfaces and implementations to accept multiple drafts

**Files:**
- Modify: `src/FantasyCritic.Lib/Interfaces/IFantasyCriticRepo.cs`
- Modify: `src/FantasyCritic.Lib/Interfaces/IConferenceRepo.cs`
- Modify: `src/FantasyCritic.MySQL/Repositories/MySQLFantasyCriticRepo.cs`
- Modify: `src/FantasyCritic.MySQL/Repositories/MySQLConferenceRepo.cs`
- Modify: `src/FantasyCritic.FakeRepo/Repositories/FakeFantasyCriticRepo.cs`
- Modify: `src/FantasyCritic.FakeRepo/Repositories/FakeConferenceRepo.cs`

- [ ] **Step 1: Update `IFantasyCriticRepo.CreateLeague` signature**

In `src/FantasyCritic.Lib/Interfaces/IFantasyCriticRepo.cs`, change:
```csharp
// Before:
Task CreateLeague(League league, int initialYear, LeagueOptions options, LeagueDraft initialDraft);

// After:
Task CreateLeague(League league, int initialYear, LeagueOptions options, IReadOnlyList<LeagueDraft> drafts);
```

- [ ] **Step 2: Update `IConferenceRepo.CreateConference` signature**

Find the `CreateConference` method signature in `IConferenceRepo.cs` and change the single `LeagueDraft initialDraft` parameter to `IReadOnlyList<LeagueDraft> drafts`.

- [ ] **Step 3: Update `MySQLFantasyCriticRepo.CreateLeague`**

Open `src/FantasyCritic.MySQL/Repositories/MySQLFantasyCriticRepo.cs`. Find the `CreateLeague` method implementation. The method currently inserts `initialDraft`. Change the signature to match the interface:

```csharp
public async Task CreateLeague(League league, int initialYear, LeagueOptions options, IReadOnlyList<LeagueDraft> drafts)
```

Wherever the method previously inserted the single `initialDraft` into `tbl_league_draft`, replace with a loop over `drafts`:

```csharp
// Replace the single initialDraft insert with:
foreach (var draft in drafts)
{
    // Use the same INSERT pattern that was used for initialDraft
    // The entity mapping follows the existing LeagueDraftEntity pattern
}
```

Study the existing insert code to understand the exact entity/SQL used and apply the same for each draft in the loop.

- [ ] **Step 4: Update `MySQLConferenceRepo.CreateConference`**

Open `src/FantasyCritic.MySQL/Repositories/MySQLConferenceRepo.cs`. Find the `CreateConference` method. Change the signature:

```csharp
public async Task CreateConference(Conference conference, League primaryLeague, int initialYear, LeagueOptions options, IReadOnlyList<LeagueDraft> drafts)
```

Wherever `initialDraft` was passed as a single object to an INSERT, replace with the same loop pattern used in `MySQLFantasyCriticRepo`:

```csharp
foreach (var draft in drafts)
{
    // Insert draft row using the same entity/SQL pattern as MySQLFantasyCriticRepo Step 3
}
```

- [ ] **Step 5: Update `FakeFantasyCriticRepo.CreateLeague`**

Open `src/FantasyCritic.FakeRepo/Repositories/FakeFantasyCriticRepo.cs`. Find `CreateLeague`. Change the signature:

```csharp
public Task CreateLeague(League league, int initialYear, LeagueOptions options, IReadOnlyList<LeagueDraft> drafts)
```

The fake repo stores league data in in-memory collections. Where `initialDraft` was stored (e.g. added to a `List<LeagueDraft>` or similar), replace with:

```csharp
foreach (var draft in drafts)
{
    // Add draft to whichever in-memory collection the repo uses for drafts
    // (check the existing CreateLeagueDraft method for the correct collection name)
}
```

- [ ] **Step 6: Update `FakeConferenceRepo.CreateConference`**

Open `src/FantasyCritic.FakeRepo/Repositories/FakeConferenceRepo.cs`. Find `CreateConference`. Change the signature:

```csharp
public Task CreateConference(Conference conference, League primaryLeague, int initialYear, LeagueOptions options, IReadOnlyList<LeagueDraft> drafts)
```

Apply the same in-memory loop pattern as the FakeFantasyCriticRepo step above.

- [ ] **Step 7: Build**

```powershell
cd I:\CodeProjects\FantasyCritic\src
dotnet build
```

Expected: 0 errors.

- [ ] **Step 8: Commit**

```powershell
cd I:\CodeProjects\FantasyCritic
git add -u
git commit -m "Update IFantasyCriticRepo and IConferenceRepo CreateLeague/CreateConference to accept IReadOnlyList<LeagueDraft>."
```

---

## Task 6: Update `FantasyCriticService.CreateLeague` and `ConferenceService.CreateConference`

**Files:**
- Modify: `src/FantasyCritic.Lib/Services/FantasyCriticService.cs`
- Modify: `src/FantasyCritic.Lib/Services/ConferenceService.cs`

- [ ] **Step 1: Update `FantasyCriticService.CreateLeague`**

Replace the stub from Task 2 with proper draft construction. The full new `CreateLeague` method body:

```csharp
public async Task<Result<League>> CreateLeague(LeagueCreationParameters parameters)
{
    LeagueOptions options = new LeagueOptions(parameters.LeagueYearParameters);

    var validateOptions = options.Validate();
    if (validateOptions.IsFailure)
        return Result.Failure<League>(validateOptions.Error);

    if (!parameters.LeagueYearParameters.ScoringSystem.SupportedInYear(parameters.LeagueYearParameters.Year))
        return Result.Failure<League>("That scoring mode is no longer supported.");

    IEnumerable<MinimalLeagueYearInfo> years = new List<MinimalLeagueYearInfo>
        { new MinimalLeagueYearInfo(parameters.LeagueYearParameters.Year, false, false) };
    League newLeague = new League(Guid.NewGuid(), parameters.LeagueName, parameters.Manager.ToMinimal(),
        null, null, years, parameters.PublicLeague, parameters.TestLeague,
        parameters.CustomRulesLeague, false, 0);

    var leagueYearKey = new LeagueYearKey(newLeague.LeagueID, parameters.LeagueYearParameters.Year);
    var drafts = parameters.Drafts.Select((d, i) => new LeagueDraft(
        Guid.NewGuid(), leagueYearKey, i + 1,
        d.Name ?? (i == 0 ? "Initial Draft" : $"Draft {i + 1}"),
        d.ScheduledDate, d.GamesToDraft, d.CounterPicksToDraft,
        false, PlayStatus.NotStartedDraft, new List<PublisherDraftInfo>(), null))
        .ToList();

    await _fantasyCriticRepo.CreateLeague(newLeague, parameters.LeagueYearParameters.Year, options, drafts);
    return Result.Success(newLeague);
}
```

- [ ] **Step 2: Update `ConferenceService.CreateConference`**

Apply the same pattern — replace the single `initialDraft` build with an iteration over `parameters.Drafts`:

```csharp
var leagueYearKey = new LeagueYearKey(primaryLeague.LeagueID, parameters.LeagueYearParameters.Year);
var drafts = parameters.Drafts.Select((d, i) => new LeagueDraft(
    Guid.NewGuid(), leagueYearKey, i + 1,
    d.Name ?? (i == 0 ? "Initial Draft" : $"Draft {i + 1}"),
    d.ScheduledDate, d.GamesToDraft, d.CounterPicksToDraft,
    false, PlayStatus.NotStartedDraft, new List<PublisherDraftInfo>(), null))
    .ToList();

await _conferenceRepo.CreateConference(newConference, primaryLeague,
    parameters.LeagueYearParameters.Year, options, drafts);
```

- [ ] **Step 3: Build**

```powershell
cd I:\CodeProjects\FantasyCritic\src
dotnet build
```

Expected: 0 errors. The `NotImplementedException` stubs placed in Task 2 should now be fully replaced.

- [ ] **Step 4: Commit**

```powershell
cd I:\CodeProjects\FantasyCritic
git add -u
git commit -m "Update CreateLeague and CreateConference services to build drafts from DraftParameters list."
```

---

## Task 7: `EditLeagueYearRequest` + update `FantasyCriticService.EditLeague` + controller

**Files:**
- Create: `src/FantasyCritic.Web/Models/Requests/LeagueManager/EditLeagueYearRequest.cs`
- Modify: `src/FantasyCritic.Lib/Domain/LeagueDraft.cs`
- Modify: `src/FantasyCritic.Lib/Services/FantasyCriticService.cs`
- Modify: `src/FantasyCritic.Web/Controllers/API/LeagueManagerController.cs`

- [ ] **Step 1: Add `UpdateDraft` overload to `LeagueDraft`**

The existing `UpdateDraft(int gamesToDraft, int counterPicksToDraft)` only updates counts. Add an overload that also updates name and scheduled date:

```csharp
// src/FantasyCritic.Lib/Domain/LeagueDraft.cs
public LeagueDraft UpdateDraft(string name, LocalDate? scheduledDate, int gamesToDraft, int counterPicksToDraft)
{
    return new LeagueDraft(DraftID, LeagueYearKey, DraftNumber, name, scheduledDate, gamesToDraft, counterPicksToDraft,
        DraftOrderSet, PlayStatus, PublisherDraftInfo, DraftStartedTimestamp);
}
```

- [ ] **Step 2: Create `EditLeagueYearRequest`**

```csharp
// src/FantasyCritic.Web/Models/Requests/LeagueManager/EditLeagueYearRequest.cs
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Web.Models.RoundTrip;

namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public class EditLeagueYearRequest
{
    public EditLeagueYearRequest(Guid leagueID, int year, string? leagueYearName,
        LeagueYearSettingsViewModel leagueYearSettings, DraftSettingsRequest? firstDraft)
    {
        LeagueID = leagueID;
        Year = year;
        LeagueYearName = leagueYearName;
        LeagueYearSettings = leagueYearSettings;
        FirstDraft = firstDraft;
    }

    public Guid LeagueID { get; }
    public int Year { get; }
    public string? LeagueYearName { get; }
    public LeagueYearSettingsViewModel LeagueYearSettings { get; }
    public DraftSettingsRequest? FirstDraft { get; }

    public Result IsValid() => LeagueYearSettings.IsValid();

    public (LeagueYearParameters settings, DraftParameters? firstDraft) ToDomain(
        IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        // Call the ViewModel's ToDomain to parse all enum strings and tag/slot data
        var parsed = LeagueYearSettings.ToDomain(tagDictionary);
        // Reconstruct using this wrapper's LeagueID, Year, and LeagueYearName as the
        // authoritative values (the nested ViewModel may carry a stale LeagueYearName
        // from when the form was loaded)
        var settings = new LeagueYearParameters(
            LeagueID, Year, LeagueYearName,
            parsed.StandardGames, parsed.CounterPicks,
            parsed.UnrestrictedReleaseStatusDroppableGames,
            parsed.WillNotReleaseDroppableGames,
            parsed.WillReleaseDroppableGames,
            parsed.DropOnlyDraftGames, parsed.GrantSuperDrops,
            parsed.CounterPicksBlockDrops, parsed.AllowMoveIntoIneligible,
            parsed.MinimumBidAmount, parsed.EnableBids,
            parsed.LeagueTags, parsed.SpecialGameSlots,
            parsed.DraftSystem, parsed.PickupSystem, parsed.ScoringSystem,
            parsed.TradingSystem, parsed.TiebreakSystem, parsed.ReleaseSystem,
            parsed.IneligibleGameSystem, parsed.CounterPickDeadline,
            parsed.MightReleaseDroppableDate);
        var firstDraftParams = FirstDraft?.ToDomain(0);
        return (settings, firstDraftParams);
    }
}

- [ ] **Step 3: Update `FantasyCriticService.EditLeague` signature and implementation**

The method currently takes `(LeagueYear leagueYear, LeagueYearParameters parameters)`. Add an optional third parameter:

```csharp
public async Task<Result> EditLeague(LeagueYear leagueYear, LeagueYearParameters parameters, DraftParameters? firstDraft = null)
```

Replace the single-draft validation block (around lines 147–175 in the current file) with:

```csharp
if (leagueYear.Drafts.Count == 1 && firstDraft is not null)
{
    if (leagueYear.FirstDraft.PlayStatus.DraftIsActive)
    {
        if (leagueYear.FirstDraft.GamesToDraft > firstDraft.GamesToDraft)
            return Result.Failure("Cannot decrease the number of drafted games during the draft. Reset the draft if you need to do this.");
        if (leagueYear.FirstDraft.CounterPicksToDraft > firstDraft.CounterPicksToDraft)
            return Result.Failure("Cannot decrease the number of drafted counter picks during the draft. Reset the draft if you need to do this.");
    }
    if (leagueYear.FirstDraft.PlayStatus.DraftFinished)
    {
        if (leagueYear.FirstDraft.GamesToDraft != firstDraft.GamesToDraft)
            return Result.Failure("Cannot change the number of drafted games after the draft.");
        if (leagueYear.FirstDraft.CounterPicksToDraft != firstDraft.CounterPicksToDraft)
            return Result.Failure("Cannot change the number of drafted counter picks after the draft.");
    }
}
```

Replace the draft update block (~lines 199–203) with:

```csharp
List<LeagueDraft> leagueDrafts = leagueYear.Drafts.ToList();
if (leagueYear.Drafts.Count == 1 && firstDraft is not null)
{
    string resolvedName = firstDraft.Name ?? leagueYear.FirstDraft.Name;
    leagueDrafts = [leagueYear.FirstDraft.UpdateDraft(
        resolvedName, firstDraft.ScheduledDate, firstDraft.GamesToDraft, firstDraft.CounterPicksToDraft)];
}
```

Replace the diff calculation to use the (possibly) updated first draft:

```csharp
var draftDiff = leagueYear.Drafts.Count == 1 && firstDraft is not null
    ? leagueDrafts[0].GetDifferences(leagueYear.FirstDraft)
    : new LeagueOptionsDifferences([]);
```

Remove the old `// TODO this needs to be adjusted once multi draft is actually possible` comment.

- [ ] **Step 4: Update `LeagueManagerController.EditLeagueYearSettings`**

The action currently takes `[FromBody] LeagueYearSettingsViewModel request`. Replace with `EditLeagueYearRequest`:

```csharp
[HttpPost]
public async Task<IActionResult> EditLeagueYearSettings([FromBody] EditLeagueYearRequest request)
{
    var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year,
        ActionProcessingModeBehavior.Ban, RequiredRelationship.LeagueManager, RequiredYearStatus.AnyYearNotFinished);
    if (leagueYearRecord.FailedResult is not null)
        return leagueYearRecord.FailedResult;
    var validResult = leagueYearRecord.ValidResult!;
    var leagueYear = validResult.LeagueYear;

    var requestValid = request.IsValid();
    if (requestValid.IsFailure)
        return BadRequest(requestValid.Error);

    var tagDictionary = await _interLeagueService.GetMasterGameTagDictionary();
    var (domainSettings, firstDraft) = request.ToDomain(tagDictionary);
    Result result = await _fantasyCriticService.EditLeague(leagueYear, domainSettings, firstDraft);
    if (result.IsFailure)
        return BadRequest(result.Error);

    await _fantasyCriticService.UpdatePublisherGameCalculatedStats(leagueYear);
    return Ok();
}
```

- [ ] **Step 5: Build**

```powershell
cd I:\CodeProjects\FantasyCritic\src
dotnet build
```

Expected: 0 errors.

- [ ] **Step 6: Commit**

```powershell
cd I:\CodeProjects\FantasyCritic
git add -u
git commit -m "Add EditLeagueYearRequest; update EditLeague service and controller to accept optional FirstDraft."
```

---

## Task 8: Regenerate API client and update integration test helpers

**Files:**
- Regenerate: `src/FantasyCritic.ApiClient/Generated/FantasyCriticClients.cs`
- Modify: `src/FantasyCritic.IntegrationTests/Helpers/LeagueScenario.cs`
- Modify: `src/FantasyCritic.IntegrationTests/Helpers/LeagueTestHelpers.cs`
- Modify: `src/FantasyCritic.IntegrationTests/Tests/League/Setup/LeagueSetupTests.cs`

- [ ] **Step 1: Regenerate the NSwag API client**

Build the web project (triggers OpenAPI spec regeneration), then regenerate the NSwag client:

```powershell
cd I:\CodeProjects\FantasyCritic\src
dotnet build FantasyCritic.Web
# Follow the project's existing NSwag generation workflow to regenerate FantasyCriticClients.cs
# (typically: nswag run or dotnet nswag from the ApiClient project directory)
```

The generated `CreateLeagueRequest` class in `FantasyCriticClients.cs` will gain a `Drafts` property. The generated `EditLeagueYearSettingsAsync` client method will accept `EditLeagueYearRequest`.

- [ ] **Step 2: Add `BuildDraftSettings` to `LeagueScenario`**

In `src/FantasyCritic.IntegrationTests/Helpers/LeagueScenario.cs`, add a method that returns the draft list for use in request objects. The `LeagueScenario` class already has `GamesToDraft` and `CounterPicksToDraft` properties — use them:

```csharp
public IReadOnlyList<DraftSettingsRequest> BuildDraftSettings() =>
    new List<DraftSettingsRequest>
    {
        new DraftSettingsRequest(Name: null, ScheduledDate: null,
            GamesToDraft: GamesToDraft, CounterPicksToDraft: CounterPicksToDraft)
    };
```

`Name: null` lets the server resolve the default name ("Initial Draft").

- [ ] **Step 3: Update `LeagueTestHelpers.CreateLeagueAsync`**

```csharp
public static async Task<Guid> CreateLeagueAsync(
    ApiSession managerSession,
    LeagueScenario scenario,
    int year)
{
    var leagueID = await managerSession.LeagueManager.CreateLeagueAsync(
        new CreateLeagueRequest
        {
            LeagueName = $"TestLeague-{Guid.NewGuid():N}"[..30],
            PublicLeague = false,
            TestLeague = true,
            CustomRulesLeague = false,
            LeagueYearSettings = scenario.BuildSettings(year),
            Drafts = scenario.BuildDraftSettings(),
        });
    return leagueID;
}
```

- [ ] **Step 4: Update `LeagueSetupTests` — fix broken assertion**

The test `GetLeagueYearOptions_AfterCreate_RoundTripsStandardGameCount` currently asserts `settings.GamesToDraft` and `settings.CounterPicksToDraft` from `GetLeagueYearOptionsAsync`. Those fields no longer exist on `LeagueYearSettingsViewModel`. Replace those assertions with a check against the league year's draft:

```csharp
// Remove:
Assert.That(settings.GamesToDraft, Is.EqualTo(LeagueScenarios.Standard.GamesToDraft));
Assert.That(settings.CounterPicksToDraft, Is.EqualTo(LeagueScenarios.Standard.CounterPicksToDraft));

// Replace with:
var leagueYear = await session.League.GetLeagueYearAsync(leagueID, year);
Assert.That(leagueYear.Drafts[0].GamesToDraft, Is.EqualTo(LeagueScenarios.Standard.GamesToDraft));
Assert.That(leagueYear.Drafts[0].CounterPicksToDraft, Is.EqualTo(LeagueScenarios.Standard.CounterPicksToDraft));
```

Search for any other test files that reference `settings.GamesToDraft` or `settings.CounterPicksToDraft` from options responses and apply the same fix.

- [ ] **Step 5: Build integration tests**

```powershell
cd I:\CodeProjects\FantasyCritic\src
dotnet build FantasyCritic.IntegrationTests
```

Expected: 0 errors.

- [ ] **Step 6: Run existing integration tests**

```powershell
dotnet test FantasyCritic.IntegrationTests --filter "Category!=Slow" -v minimal
```

Expected: all previously passing tests still pass.

- [ ] **Step 7: Commit**

```powershell
cd I:\CodeProjects\FantasyCritic
git add -u
git commit -m "Regenerate NSwag client; update integration test helpers and fix broken draft assertions."
```

---

## Task 9: Integration tests for multi-draft creation and edit

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/Tests/League/Setup/MultiDraftCreationTests.cs`

> **Pattern:** All state is built through the HTTP API. Assertions read back state via API calls. Follow the pattern in `LeagueSetupTests.cs` and `MultiDraftCrudTests.cs`.

- [ ] **Step 1: Create `MultiDraftCreationTests.cs`**

```csharp
// src/FantasyCritic.IntegrationTests/Tests/League/Setup/MultiDraftCreationTests.cs
using FantasyCritic.ApiClient.Generated;
using FantasyCritic.IntegrationTests.Helpers;
using NodaTime;

namespace FantasyCritic.IntegrationTests.Tests.League.Setup;

[TestFixture]
public class MultiDraftCreationTests : IntegrationTestBase
{
    [Test]
    public async Task CreateLeague_WithTwoDrafts_CreatesBothDraftRows()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var year = await LeagueTestHelpers.GetOpenYearAsync(session);
        var scenario = LeagueScenarios.Standard;

        var leagueID = await session.LeagueManager.CreateLeagueAsync(new CreateLeagueRequest
        {
            LeagueName = $"TestLeague-{Guid.NewGuid():N}"[..30],
            PublicLeague = false,
            TestLeague = true,
            CustomRulesLeague = false,
            LeagueYearSettings = scenario.BuildSettings(year),
            Drafts = new List<DraftSettingsRequest>
            {
                new DraftSettingsRequest(null, null, 3, 1),
                new DraftSettingsRequest("Draft 2", null, 3, 0),
            }
        });

        var leagueYear = await session.League.GetLeagueYearAsync(leagueID, year);

        Assert.That(leagueYear, Is.Not.Null);
        Assert.That(leagueYear.Drafts, Has.Count.EqualTo(2));

        var first = leagueYear.Drafts.Single(d => d.DraftNumber == 1);
        Assert.That(first.Name, Is.EqualTo("Initial Draft"));
        Assert.That(first.GamesToDraft, Is.EqualTo(3));
        Assert.That(first.CounterPicksToDraft, Is.EqualTo(1));

        var second = leagueYear.Drafts.Single(d => d.DraftNumber == 2);
        Assert.That(second.Name, Is.EqualTo("Draft 2"));
        Assert.That(second.GamesToDraft, Is.EqualTo(3));
        Assert.That(second.CounterPicksToDraft, Is.EqualTo(0));
    }

    [Test]
    public async Task CreateLeague_WithThreeDrafts_CreatesThreeDraftRows()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var year = await LeagueTestHelpers.GetOpenYearAsync(session);
        var scenario = LeagueScenarios.Standard;

        // Standard scenario has StandardGames = 6; use 2+2+2 split
        var leagueID = await session.LeagueManager.CreateLeagueAsync(new CreateLeagueRequest
        {
            LeagueName = $"TestLeague-{Guid.NewGuid():N}"[..30],
            PublicLeague = false,
            TestLeague = true,
            CustomRulesLeague = false,
            LeagueYearSettings = scenario.BuildSettings(year),
            Drafts = new List<DraftSettingsRequest>
            {
                new DraftSettingsRequest(null, null, 2, 1),
                new DraftSettingsRequest(null, null, 2, 0),
                new DraftSettingsRequest(null, null, 2, 0),
            }
        });

        var leagueYear = await session.League.GetLeagueYearAsync(leagueID, year);

        Assert.That(leagueYear.Drafts, Has.Count.EqualTo(3));
        Assert.That(leagueYear.Drafts.Select(d => d.DraftNumber).Order(), Is.EqualTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public async Task EditLeagueYearSettings_WithFirstDraft_UpdatesDraftCounts()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var year = await LeagueTestHelpers.GetOpenYearAsync(session);
        var leagueID = await LeagueTestHelpers.CreateLeagueAsync(session, LeagueScenarios.Standard, year);

        var settings = await session.League.GetLeagueYearOptionsAsync(leagueID, year);

        await session.LeagueManager.EditLeagueYearSettingsAsync(new EditLeagueYearRequest
        {
            LeagueID = leagueID,
            Year = year,
            LeagueYearName = null,
            LeagueYearSettings = settings,
            FirstDraft = new DraftSettingsRequest(null, null, GamesToDraft: 4, CounterPicksToDraft: 1),
        });

        var leagueYear = await session.League.GetLeagueYearAsync(leagueID, year);
        Assert.That(leagueYear.Drafts[0].GamesToDraft, Is.EqualTo(4));
        Assert.That(leagueYear.Drafts[0].CounterPicksToDraft, Is.EqualTo(1));
    }

    [Test]
    public async Task EditLeagueYearSettings_MultiDraftLeague_FirstDraftIgnored()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var year = await LeagueTestHelpers.GetOpenYearAsync(session);

        // Create with 2 drafts
        var leagueID = await session.LeagueManager.CreateLeagueAsync(new CreateLeagueRequest
        {
            LeagueName = $"TestLeague-{Guid.NewGuid():N}"[..30],
            PublicLeague = false,
            TestLeague = true,
            CustomRulesLeague = false,
            LeagueYearSettings = LeagueScenarios.Standard.BuildSettings(year),
            Drafts = new List<DraftSettingsRequest>
            {
                new DraftSettingsRequest(null, null, 3, 1),
                new DraftSettingsRequest(null, null, 3, 0),
            }
        });

        var originalLeagueYear = await session.League.GetLeagueYearAsync(leagueID, year);
        var originalGamesToDraft = originalLeagueYear.Drafts[0].GamesToDraft;

        var settings = await session.League.GetLeagueYearOptionsAsync(leagueID, year);

        // Send FirstDraft with different counts — should be silently ignored for multi-draft leagues
        await session.LeagueManager.EditLeagueYearSettingsAsync(new EditLeagueYearRequest
        {
            LeagueID = leagueID,
            Year = year,
            LeagueYearName = null,
            LeagueYearSettings = settings,
            FirstDraft = new DraftSettingsRequest(null, null, GamesToDraft: 99, CounterPicksToDraft: 99),
        });

        var updatedLeagueYear = await session.League.GetLeagueYearAsync(leagueID, year);
        Assert.That(updatedLeagueYear.Drafts[0].GamesToDraft, Is.EqualTo(originalGamesToDraft));
    }
}
```

- [ ] **Step 2: Run the new tests**

```powershell
cd I:\CodeProjects\FantasyCritic\src
dotnet test FantasyCritic.IntegrationTests --filter "FullyQualifiedName~MultiDraftCreationTests" -v normal
```

Expected: all 4 tests pass.

- [ ] **Step 3: Commit**

```powershell
cd I:\CodeProjects\FantasyCritic
git add src/FantasyCritic.IntegrationTests/Tests/League/Setup/MultiDraftCreationTests.cs
git commit -m "Add MultiDraftCreationTests integration tests."
```

---

## Task 10: `leagueCreationPresets.ts`

**Files:**
- Create: `src/FantasyCritic.Web/ClientApp/src/utilities/leagueCreationPresets.ts`

> Create the `src/FantasyCritic.Web/ClientApp/src/utilities/` folder if it does not exist.

- [ ] **Step 1: Create the file**

```typescript
// src/FantasyCritic.Web/ClientApp/src/utilities/leagueCreationPresets.ts

export type GameMode = 'Standard' | 'Multi Draft' | 'One Shot';
export type ExperienceLevel = 'Beginner' | 'Standard' | 'Advanced';

export interface DraftSettings {
  name: string | null;
  scheduledDate: string | null;
  gamesToDraft: number;
  counterPicksToDraft: number;
}

export interface LeagueYearSettingsPartial {
  standardGames: number;
  counterPicks: number;
  gamesToDraft?: number;         // not used post-refactor; included for transitional compat
  counterPicksToDraft?: number;  // same
  minimumBidAmount: number;
  enableBids: boolean;
  tradingSystem: string;
  grantSuperDrops: boolean;
  pickupSystem: string;
  counterPicksBlockDrops: boolean;
  unrestrictedReleaseStatusDroppableGames: number;
  willNotReleaseDroppableGames: number;
  willReleaseDroppableGames: number;
  unlimitedUnrestrictedReleaseStatusDroppableGames: boolean;
  unlimitedWillNotReleaseDroppableGames: boolean;
  unlimitedWillReleaseDroppableGames: boolean;
  specialGameSlots: object[];
  tags: { banned: string[]; required: string[] };
  counterPickDeadline: string;
  mightReleaseDroppableDate: string;
}

export interface PresetResult {
  settings: Partial<LeagueYearSettingsPartial>;
  drafts: DraftSettings[];
}

// ---------------------------------------------------------------------------
// Internal helpers
// ---------------------------------------------------------------------------

function computeGameCounts(
  recommendedNumberOfGames: number,
  draftGameRatio: number,
  playerCount: number
): { standardGames: number; counterPicks: number; gamesToDraft: number; counterPicksToDraft: number } {
  const avgStdGames = Math.floor(recommendedNumberOfGames / 6);
  const avgCPs = Math.floor(avgStdGames / 6);
  const avgGTD = Math.floor(avgStdGames * draftGameRatio);
  const avgCPTD = Math.floor(avgCPs * draftGameRatio);

  const thisStdGames = Math.floor(recommendedNumberOfGames / playerCount);
  const thisCPs = Math.floor(thisStdGames / 6);
  const thisGTD = Math.floor(thisStdGames * draftGameRatio);
  const thisCPTD = Math.floor(thisCPs * draftGameRatio);

  let standardGames = Math.floor((avgStdGames + thisStdGames) / 2);
  let counterPicks = Math.floor((avgCPs + thisCPs) / 2);
  let gamesToDraft = Math.floor((avgGTD + thisGTD) / 2);
  let counterPicksToDraft = Math.floor((avgCPTD + thisCPTD) / 2);

  if (counterPicks === 0 || counterPicksToDraft === 0) {
    counterPicks = 1;
    counterPicksToDraft = 1;
  }

  return { standardGames, counterPicks, gamesToDraft, counterPicksToDraft };
}

function computeSpecialSlots(standardGames: number, experienceLevel: ExperienceLevel): object[] {
  if (experienceLevel === 'Beginner') return [];
  const numberOfSpecialSlots = Math.floor(standardGames / 2);
  if (numberOfSpecialSlots < 1) return [];

  const slots: object[] = [];
  const includeExpansion = numberOfSpecialSlots >= 2;
  const includeRemake = numberOfSpecialSlots >= 2;
  const nonNgfCount = (includeExpansion ? 1 : 0) + (includeRemake ? 1 : 0);
  const ngfCount = Math.max(0, numberOfSpecialSlots - nonNgfCount);

  for (let i = 0; i < ngfCount; i++) {
    slots.push({ specialSlotPosition: slots.length, requiredTags: ['NewGamingFranchise'] });
  }
  if (includeExpansion) {
    slots.push({ specialSlotPosition: slots.length, requiredTags: ['NewGamingFranchise', 'ExpansionPack'] });
  }
  if (includeRemake) {
    slots.push({ specialSlotPosition: slots.length, requiredTags: ['NewGamingFranchise', 'PartialRemake', 'Remake', 'Reimagining'] });
  }
  return slots;
}

// ---------------------------------------------------------------------------
// Public API
// ---------------------------------------------------------------------------

export function computePreset(
  gameMode: GameMode,
  experienceLevel: ExperienceLevel,
  playerCount: number,
  year: number
): PresetResult {
  let recommendedNumberOfGames: number;
  let draftGameRatio: number;

  if (gameMode === 'One Shot') {
    recommendedNumberOfGames = 50;
    draftGameRatio = 1;
  } else if (experienceLevel === 'Beginner') {
    recommendedNumberOfGames = 42;
    draftGameRatio = 4 / 7;
  } else if (experienceLevel === 'Advanced') {
    recommendedNumberOfGames = 108;
    draftGameRatio = 4 / 9;
  } else {
    recommendedNumberOfGames = 72;
    draftGameRatio = 1 / 2;
  }

  const { standardGames, counterPicks, gamesToDraft, counterPicksToDraft } =
    computeGameCounts(recommendedNumberOfGames, draftGameRatio, playerCount);

  const alwaysBanned = ['Port', 'PlannedForEarlyAccess', 'CurrentlyInEarlyAccess', 'ReleasedInternationally'];
  const standardBanned = ['YearlyInstallment', 'DirectorsCut', 'PartialRemake', 'Remaster', 'ExpansionPack'];
  const bannedTags = experienceLevel === 'Beginner' ? alwaysBanned : [...alwaysBanned, ...standardBanned];

  const settings: Partial<LeagueYearSettingsPartial> = {
    standardGames,
    counterPicks,
    minimumBidAmount: 0,
    enableBids: gameMode !== 'One Shot' && gameMode !== 'Multi Draft',
    tradingSystem: gameMode === 'One Shot' || experienceLevel === 'Beginner' ? 'NoTrades' : 'Standard',
    grantSuperDrops: experienceLevel === 'Beginner',
    pickupSystem: experienceLevel === 'Advanced' ? 'SecretBidding' : 'SemiPublicBiddingSecretCounterPicks',
    counterPicksBlockDrops: experienceLevel === 'Advanced',
    unrestrictedReleaseStatusDroppableGames: 0,
    willNotReleaseDroppableGames: 0,
    unlimitedUnrestrictedReleaseStatusDroppableGames: false,
    unlimitedWillReleaseDroppableGames: false,
    willReleaseDroppableGames: gameMode === 'One Shot' ? 0 : 1,
    unlimitedWillNotReleaseDroppableGames: gameMode !== 'One Shot',
    specialGameSlots: computeSpecialSlots(standardGames, experienceLevel),
    tags: { banned: bannedTags, required: [] },
    counterPickDeadline: `${year}-11-01`,
    mightReleaseDroppableDate: `${year}-11-01`,
  };

  let drafts: DraftSettings[];
  if (gameMode === 'Multi Draft') {
    const draft1Games = Math.ceil(gamesToDraft / 2);
    const draft2Games = gamesToDraft - draft1Games;
    drafts = [
      { name: null, scheduledDate: null, gamesToDraft: draft1Games, counterPicksToDraft },
      { name: null, scheduledDate: null, gamesToDraft: Math.max(1, draft2Games), counterPicksToDraft: 0 },
    ];
  } else if (gameMode === 'One Shot') {
    drafts = [{ name: null, scheduledDate: null, gamesToDraft: standardGames, counterPicksToDraft: counterPicks }];
  } else {
    drafts = [{ name: null, scheduledDate: null, gamesToDraft, counterPicksToDraft }];
  }

  return { settings, drafts };
}

export function getDefaultDraft(
  draftIndex: number,
  standardGames: number,
  allocatedSoFar: number
): DraftSettings {
  const remaining = Math.max(1, standardGames - allocatedSoFar);
  const name = draftIndex === 0 ? null : null; // null = server resolves; caller can override
  return { name, scheduledDate: null, gamesToDraft: remaining, counterPicksToDraft: 0 };
}
```

- [ ] **Step 2: Verify TypeScript compiles**

```powershell
cd I:\CodeProjects\FantasyCritic\src\FantasyCritic.Web\ClientApp
npx tsc --noEmit
```

Expected: 0 errors.

- [ ] **Step 3: Commit**

```powershell
cd I:\CodeProjects\FantasyCritic
git add src/FantasyCritic.Web/ClientApp/src/utilities/leagueCreationPresets.ts
git commit -m "Add leagueCreationPresets.ts with computePreset and getDefaultDraft."
```

---

## Task 11: `leagueCreationPresets.vue`

**Files:**
- Create: `src/FantasyCritic.Web/ClientApp/src/components/leagueCreationPresets.vue`

- [ ] **Step 1: Create the component**

```vue
<!-- src/FantasyCritic.Web/ClientApp/src/components/leagueCreationPresets.vue -->
<template>
  <div>
    <div class="form-group">
      <label for="intendedNumberOfPlayers" class="control-label">{{ playerCountLabel }}</label>
      <ValidationProvider v-slot="{ errors }" rules="required|min_value:2|max_value:20|integer">
        <input
          id="intendedNumberOfPlayers"
          v-model.number="playerCount"
          name="Intended Number of Players"
          type="text"
          class="form-control input"
          @input="emitIfReady" />
        <span class="text-danger">{{ errors[0] }}</span>
      </ValidationProvider>
      <p>You aren't locked into this number of people. This is just to recommend how many games to have per person.</p>
    </div>

    <div class="form-group">
      <label for="gameMode" class="control-label">Game Mode</label>
      <p>
        <strong>Standard</strong> — the full Fantasy Critic experience with drafts, bids, and pickups throughout the year.<br />
        <strong>Multi Draft</strong> — run two or more drafts across the year; bids between drafts are off by default.<br />
        <strong>One Shot</strong> — the draft is final; no drops, bids, or trades. Great for a low-commitment league.
      </p>
      <b-form-select id="gameMode" v-model="gameMode" :options="gameModeOptions" @change="emitIfReady"></b-form-select>
    </div>

    <div class="form-group">
      <label for="experienceLevel" class="control-label">Experience Level</label>
      <p>Controls the recommended number of games per player and whether special game slots are included.</p>
      <b-form-select id="experienceLevel" v-model="experienceLevel" :options="experienceLevelOptions" @change="emitIfReady"></b-form-select>
    </div>
  </div>
</template>

<script>
import { computePreset } from '@/utilities/leagueCreationPresets';

export default {
  name: 'LeagueCreationPresets',
  props: {
    year: { type: Number, required: true },
    playerCountLabel: {
      type: String,
      default: 'How many players do you think will be in this league?'
    }
  },
  emits: ['preset-applied'],
  data() {
    return {
      playerCount: null,
      gameMode: 'Standard',
      experienceLevel: 'Standard',
      gameModeOptions: [
        { value: 'Standard', text: 'Standard' },
        { value: 'Multi Draft', text: 'Multi Draft' },
        { value: 'One Shot', text: 'One Shot' },
      ],
      experienceLevelOptions: [
        { value: 'Beginner', text: 'Beginner' },
        { value: 'Standard', text: 'Standard' },
        { value: 'Advanced', text: 'Advanced' },
      ],
    };
  },
  methods: {
    emitIfReady() {
      if (!this.playerCount || this.playerCount < 2 || this.playerCount > 20) return;
      const result = computePreset(this.gameMode, this.experienceLevel, this.playerCount, this.year);
      this.$emit('preset-applied', { gameMode: this.gameMode, ...result });
    }
  }
};
</script>
```

- [ ] **Step 2: Verify no lint/compile errors in the frontend**

```powershell
cd I:\CodeProjects\FantasyCritic\src\FantasyCritic.Web\ClientApp
npx tsc --noEmit
```

- [ ] **Step 3: Commit**

```powershell
cd I:\CodeProjects\FantasyCritic
git add src/FantasyCritic.Web/ClientApp/src/components/leagueCreationPresets.vue
git commit -m "Add leagueCreationPresets.vue — Game Mode and Experience Level selectors."
```

---

## Task 12: `DraftCreationSettings.vue`

**Files:**
- Create: `src/FantasyCritic.Web/ClientApp/src/components/DraftCreationSettings.vue`

- [ ] **Step 1: Create the component**

```vue
<!-- src/FantasyCritic.Web/ClientApp/src/components/DraftCreationSettings.vue -->
<template>
  <div>
    <div v-if="totalGamesToDraft > standardGames" class="alert alert-warning">
      Total games to draft across all drafts ({{ totalGamesToDraft }}) exceeds the total number of games ({{ standardGames }}).
      Please adjust the numbers below.
    </div>

    <div v-for="(draft, index) in internalDrafts" :key="index" class="draft-section mb-3">
      <h4>{{ index === 0 ? 'Initial Draft' : `Draft ${index + 1}` }}</h4>

      <div class="form-group">
        <label :for="`draftName-${index}`" class="control-label">Draft Name</label>
        <input
          :id="`draftName-${index}`"
          v-model="draft.name"
          type="text"
          class="form-control input"
          :placeholder="index === 0 ? 'Initial Draft' : `Draft ${index + 1}`"
          @input="emitUpdate" />
      </div>

      <div class="form-group">
        <label :for="`scheduledDate-${index}`" class="control-label">Scheduled Date (Optional)</label>
        <input
          :id="`scheduledDate-${index}`"
          v-model="draft.scheduledDate"
          type="date"
          class="form-control input"
          @input="emitUpdate" />
      </div>

      <div class="form-group">
        <label :for="`gamesToDraft-${index}`" class="control-label">Games to Draft</label>
        <ValidationProvider v-slot="{ errors }" rules="required|min_value:1|max_value:50|integer">
          <input
            :id="`gamesToDraft-${index}`"
            v-model.number="draft.gamesToDraft"
            type="text"
            class="form-control input"
            @input="emitUpdate" />
          <span class="text-danger">{{ errors[0] }}</span>
        </ValidationProvider>
      </div>

      <div class="form-group">
        <label :for="`counterPicksToDraft-${index}`" class="control-label">Counter Picks to Draft</label>
        <ValidationProvider v-slot="{ errors }" rules="required|min_value:0|max_value:50|integer">
          <input
            :id="`counterPicksToDraft-${index}`"
            v-model.number="draft.counterPicksToDraft"
            type="text"
            class="form-control input"
            @input="emitUpdate" />
          <span class="text-danger">{{ errors[0] }}</span>
        </ValidationProvider>
      </div>

      <div v-if="canRemoveDraft(index)">
        <b-button size="sm" variant="outline-danger" @click="removeDraft(index)">Remove Draft {{ index + 1 }}</b-button>
      </div>
    </div>

    <div v-if="showAddButton">
      <b-button variant="outline-secondary" @click="addDraft">+ Add Another Draft</b-button>
    </div>
  </div>
</template>

<script>
import { getDefaultDraft } from '@/utilities/leagueCreationPresets';

export default {
  name: 'DraftCreationSettings',
  props: {
    value: { type: Array, required: true },
    standardGames: { type: Number, default: 0 },
    gameMode: { type: String, default: 'Standard' },
    editMode: { type: Boolean, default: false },
  },
  data() {
    return {
      internalDrafts: []
    };
  },
  computed: {
    totalGamesToDraft() {
      return this.internalDrafts.reduce((sum, d) => sum + (d.gamesToDraft || 0), 0);
    },
    showAddButton() {
      return this.gameMode === 'Multi Draft' && !this.editMode;
    }
  },
  watch: {
    value: {
      immediate: true,
      handler(newVal) {
        this.internalDrafts = newVal.map(d => ({ ...d }));
      }
    }
  },
  methods: {
    emitUpdate() {
      this.$emit('input', this.internalDrafts.map(d => ({ ...d })));
    },
    canRemoveDraft(index) {
      // Only drafts beyond the first can be removed, and only if at least 2 remain (minimum for multi-draft)
      return !this.editMode && index > 0 && this.internalDrafts.length > 2;
    },
    addDraft() {
      const allocatedSoFar = this.internalDrafts.reduce((s, d) => s + (d.gamesToDraft || 0), 0);
      const newDraft = getDefaultDraft(this.internalDrafts.length, this.standardGames, allocatedSoFar);
      this.internalDrafts.push(newDraft);
      this.emitUpdate();
    },
    removeDraft(index) {
      this.internalDrafts.splice(index, 1);
      this.emitUpdate();
    }
  }
};
</script>

<style scoped>
.draft-section {
  border-left: 3px solid #6c757d;
  padding-left: 1rem;
}
</style>
```

- [ ] **Step 2: Commit**

```powershell
cd I:\CodeProjects\FantasyCritic
git add src/FantasyCritic.Web/ClientApp/src/components/DraftCreationSettings.vue
git commit -m "Add DraftCreationSettings.vue component."
```

---

## Task 13: Refactor `leagueYearSettings.vue`

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/components/leagueYearSettings.vue`

- [ ] **Step 1: Remove the `freshSettings` game mode / player count block**

Delete the entire `<div v-if="freshSettings">` block (lines ~9–31) that contains the "How many players?" input and the game mode dropdown. This block is replaced by `leagueCreationPresets.vue`.

- [ ] **Step 2: Remove `gamesToDraft` and `counterPicksToDraft` form fields**

Delete:
- The `<div v-show="!oneShotMode && !isMultiDraft">` block containing the `gamesToDraft` input (~lines 49–60)
- The `<div v-show="!oneShotMode && !isMultiDraft">` block containing the `counterPicksToDraft` input (~lines 75–87)
- The `isMultiDraft` info banner block can stay (it references the Manage Drafts page for edit mode), but update the `v-show`/`v-if` condition: since the draft fields are removed entirely, the banner can use `v-if="isMultiDraft"` alone

- [ ] **Step 3: Remove `autoUpdateOptions()` and related data/computed**

Delete:
- `gameMode` and `gameModeOptions` from `data()`
- `oneShotMode` computed property
- `autoUpdateOptions()` method body (keep the function name if other code calls it, but make it a no-op: `autoUpdateOptions() {}`)
- The `intendedNumberOfPlayers`, `intendedNumberOfPlayersEverValid` data properties
- The watch on `intendedNumberOfPlayers`
- The watches on `internalValue.standardGames` and `internalValue.counterPicks` that kept one-shot values in sync
- `fullAutoUpdate()` — replace the body with just `this.autoUpdateSpecialSlotOptions()`

- [ ] **Step 4: Add `gameMode` prop; update `autoUpdateSpecialSlotOptions` and `leagueTagSelector`**

Add a `gameMode` prop that `autoUpdateSpecialSlotOptions` and `leagueTagSelector` can use:

```javascript
// In props:
gameMode: { type: String, default: 'Standard' },

// Update the leagueTagSelector binding in the template:
// <leagueTagSelector v-model="internalValue.tags" :game-mode="gameMode"></leagueTagSelector>
// (it's already bound to :game-mode="gameMode" — now gameMode is a prop, not internal data)

// In autoUpdateSpecialSlotOptions(), replace `this.gameMode` references with `this.gameMode`
// (same expression; just now reading from prop instead of data)
```

- [ ] **Step 5: Verify the component is self-consistent**

Check that `freshSettings` prop is still used (it gates `autoUpdateSpecialSlotOptions`). Confirm that `isMultiDraft` prop still works for the "Visit Manage Drafts" banner. Confirm `leagueTagSelector` still receives `:game-mode="gameMode"`.

- [ ] **Step 6: Build frontend (TypeScript check)**

```powershell
cd I:\CodeProjects\FantasyCritic\src\FantasyCritic.Web\ClientApp
npx tsc --noEmit
```

- [ ] **Step 7: Commit**

```powershell
cd I:\CodeProjects\FantasyCritic
git add src/FantasyCritic.Web/ClientApp/src/components/leagueYearSettings.vue
git commit -m "Refactor leagueYearSettings.vue: remove game mode, player count, and draft fields. Add gameMode prop."
```

---

## Task 14: Refactor `createLeague.vue`

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/views/createLeague.vue`

- [ ] **Step 1: Import new components**

Add to the `components` object:
```javascript
import LeagueCreationPresets from '@/components/leagueCreationPresets.vue';
import DraftCreationSettings from '@/components/DraftCreationSettings.vue';
```

Register both in `components: { LeagueYearSettings, LeagueCreationPresets, DraftCreationSettings }`.

- [ ] **Step 2: Update `data()`**

Add new fields and remove the parts that are now handled by presets:

```javascript
data() {
  return {
    errorInfo: '',
    leagueName: '',
    initialYear: '',
    leagueYearSettings: null,
    drafts: [],
    gameMode: 'Standard',
    publicLeague: true,
    testLeague: false,
    customRulesLeague: false,
    leagueYearEverValid: false
  };
},
```

Remove from the `created()` hook the manual defaults for `gamesToDraft`, `counterPicksToDraft`, and the `enableBids` default (these are now driven by `leagueCreationPresets.vue`).

- [ ] **Step 3: Update the template**

Replace the `leagueYearSettings` block and add the new components. The section that currently renders `leagueYearSettings` should become:

```html
<div v-if="readyToSetupLeagueYear">
  <hr />
  <div class="text-well">
    <leagueCreationPresets
      :year="initialYear"
      @preset-applied="onPresetApplied">
    </leagueCreationPresets>
  </div>
</div>

<div v-if="leagueYearSettings">
  <div class="text-well">
    <leagueYearSettings
      v-model="leagueYearSettings"
      :year="initialYear"
      :game-mode="gameMode"
      fresh-settings>
    </leagueYearSettings>
  </div>

  <div class="text-well">
    <h3>Draft Settings</h3>
    <p v-if="gameMode === 'Multi Draft'" class="text-info">
      This league will have multiple drafts. Configure each draft below.
    </p>
    <DraftCreationSettings
      v-model="drafts"
      :standard-games="leagueYearSettings.standardGames"
      :game-mode="gameMode">
    </DraftCreationSettings>
  </div>
</div>
```

- [ ] **Step 4: Add `onPresetApplied` handler**

```javascript
onPresetApplied({ gameMode, settings, drafts }) {
  this.gameMode = gameMode;
  if (!this.leagueYearSettings) {
    // Initialize with required non-preset fields if first time
    this.leagueYearSettings = {
      year: this.initialYear,
      pickupSystem: 'SemiPublicBiddingSecretCounterPicks',
      tiebreakSystem: 'LowestProjectedPoints',
      tradingSystem: 'Standard',
      draftSystem: 'Flexible',
      scoringSystem: 'LinearPositive',
      releaseSystem: 'MustBeReleased',
      ineligibleGameSystem: 'CaseByCase',
      tags: { banned: [], allowed: [], required: [] },
      specialGameSlots: [],
    };
  }
  Object.assign(this.leagueYearSettings, settings);
  this.leagueYearSettings.year = this.initialYear;
  this.drafts = drafts.map(d => ({ ...d }));
},
```

- [ ] **Step 5: Update `readyToSetupLeagueYear` computed**

```javascript
readyToSetupLeagueYear() {
  return !!this.leagueName && !!this.initialYear;
},
```

(Remove the check for `leagueYearSettings` being set — presets now initialise it.)

- [ ] **Step 6: Update `leagueYearIsValid` computed**

```javascript
leagueYearIsValid() {
  if (!this.leagueYearSettings || !this.drafts.length) return false;
  const settingsOk =
    this.leagueYearSettings.standardGames >= 1 &&
    this.leagueYearSettings.standardGames <= 50 &&
    this.leagueYearSettings.counterPicks >= 0 &&
    this.leagueYearSettings.counterPicks <= 20;
  const draftsOk = this.gameMode === 'Multi Draft'
    ? this.drafts.length >= 2
    : this.drafts.length >= 1;
  return this.readyToSetupLeagueYear && settingsOk && draftsOk;
},
```

- [ ] **Step 7: Update `postRequest`**

```javascript
async postRequest() {
  this.leagueYearSettings.year = this.initialYear;
  const payload = {
    leagueName: this.leagueName.trim(),
    publicLeague: this.publicLeague,
    testLeague: this.testLeague,
    customRulesLeague: this.customRulesLeague,
    leagueYearSettings: this.leagueYearSettings,
    drafts: this.drafts,
  };
  try {
    const response = await axios.post('/api/leagueManager/createLeague', payload);
    const newLeagueID = response.data;
    this.$router.push({ name: 'league', params: { leagueid: newLeagueID, year: this.initialYear } });
  } catch (error) {
    this.errorInfo = error.response.data;
    window.scroll({ top: 0, left: 0, behavior: 'smooth' });
  }
},
```

- [ ] **Step 8: Commit**

```powershell
cd I:\CodeProjects\FantasyCritic
git add src/FantasyCritic.Web/ClientApp/src/views/createLeague.vue
git commit -m "Refactor createLeague.vue to use leagueCreationPresets and DraftCreationSettings."
```

---

## Task 15: Refactor `editLeague.vue`

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/views/editLeague.vue`

- [ ] **Step 1: Import `DraftCreationSettings`**

```javascript
import DraftCreationSettings from '@/components/DraftCreationSettings.vue';
// Register in components: { LeagueYearSettings, DraftCreationSettings }
```

- [ ] **Step 2: Add `firstDraft` to `data()`**

```javascript
data() {
  return {
    errorInfo: '',
    leagueYearSettings: null,
    leagueYearName: null,
    leagueYear: null,
    firstDraft: null,
    freshSettings: false
  };
},
```

- [ ] **Step 3: Populate `firstDraft` from `fetchLeagueYear()`**

```javascript
fetchLeagueYear() {
  axios
    .get('/api/League/GetLeagueYear?leagueID=' + this.leagueid + '&year=' + this.year)
    .then((response) => {
      this.leagueYear = response.data;
      if (!this.isMultiDraft && response.data.drafts && response.data.drafts.length > 0) {
        const d = response.data.drafts[0];
        this.firstDraft = {
          name: d.name,
          scheduledDate: d.scheduledDate ?? null,
          gamesToDraft: d.gamesToDraft,
          counterPicksToDraft: d.counterPicksToDraft,
        };
      }
    })
    .catch((returnedError) => (this.error = returnedError));
},
```

- [ ] **Step 4: Add `DraftCreationSettings` to the template**

Below `leagueYearSettings`, add:

```html
<div v-if="!isMultiDraft && firstDraft" class="text-well mt-3">
  <h3>Draft Settings</h3>
  <DraftCreationSettings
    v-model="firstDraftAsList"
    :standard-games="leagueYearSettings ? leagueYearSettings.standardGames : 0"
    game-mode="Standard"
    edit-mode>
  </DraftCreationSettings>
</div>
```

Add a computed property to wrap `firstDraft` as a single-element list (since `DraftCreationSettings` uses `v-model` with an array):

```javascript
firstDraftAsList: {
  get() { return this.firstDraft ? [this.firstDraft] : []; },
  set(val) { this.firstDraft = val[0] ?? null; }
},
```

- [ ] **Step 5: Update `leagueYearIsValid` computed**

```javascript
leagueYearIsValid() {
  if (!this.leagueYearSettings) return false;
  const settingsOk =
    this.leagueYearSettings.standardGames >= 1 &&
    this.leagueYearSettings.standardGames <= 50 &&
    this.leagueYearSettings.counterPicks >= 0 &&
    this.leagueYearSettings.counterPicks <= 20;
  const draftOk = this.isMultiDraft || (
    this.firstDraft &&
    this.firstDraft.gamesToDraft >= 1 &&
    this.firstDraft.counterPicksToDraft >= 0
  );
  return settingsOk && draftOk;
},
```

- [ ] **Step 6: Update `postRequest`**

```javascript
postRequest() {
  const payload = {
    leagueID: this.leagueid,
    year: this.year,
    leagueYearName: this.leagueYearName || null,
    leagueYearSettings: this.leagueYearSettings,
    firstDraft: this.isMultiDraft ? null : this.firstDraft,
  };
  axios.post('/api/leagueManager/EditLeagueYearSettings', payload)
    .then(this.responseHandler)
    .catch(this.catchHandler);
},
```

- [ ] **Step 7: Commit**

```powershell
cd I:\CodeProjects\FantasyCritic
git add src/FantasyCritic.Web/ClientApp/src/views/editLeague.vue
git commit -m "Refactor editLeague.vue to use DraftCreationSettings and send EditLeagueYearRequest shape."
```

---

## Task 16: Refactor `createConference.vue`

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/views/createConference.vue`

> Apply the same changes as Task 14 (`createLeague.vue`) with these differences:
> - The `playerCountLabel` prop on `leagueCreationPresets.vue` should be `"How many players do you think will be in each league in this conference?"`
> - The `postRequest` payload field order matches the conference endpoint
> - There is no `publicLeague`/`testLeague` field; `customRulesConference` takes their place

- [ ] **Step 1: Import and register components**

```javascript
import LeagueCreationPresets from '@/components/leagueCreationPresets.vue';
import DraftCreationSettings from '@/components/DraftCreationSettings.vue';
// Add both to components: { LeagueYearSettings, LeagueCreationPresets, DraftCreationSettings }
```

- [ ] **Step 2: Add `drafts` and `gameMode` to `data()`**

```javascript
drafts: [],
gameMode: 'Standard',
```

- [ ] **Step 3: Add `onPresetApplied` handler** — identical to `createLeague.vue` Task 14 Step 4.

- [ ] **Step 4: Update template** — same structure as Task 14 Step 3, but use the `playerCountLabel` prop override:

```html
<leagueCreationPresets
  :year="initialYear"
  player-count-label="How many players do you think will be in each league in this conference?"
  @preset-applied="onPresetApplied">
</leagueCreationPresets>
```

- [ ] **Step 5: Update `leagueYearIsValid`** — same as Task 14 Step 6.

- [ ] **Step 6: Update `postRequest`**

```javascript
async postRequest() {
  this.leagueYearSettings.year = this.initialYear;
  const payload = {
    conferenceName: this.conferenceName.trim(),
    primaryLeagueName: this.primaryLeagueName.trim(),
    customRulesConference: this.customRulesConference,
    leagueYearSettings: this.leagueYearSettings,
    drafts: this.drafts,
  };
  // ... existing try/catch ...
},
```

- [ ] **Step 7: Commit**

```powershell
cd I:\CodeProjects\FantasyCritic
git add src/FantasyCritic.Web/ClientApp/src/views/createConference.vue
git commit -m "Refactor createConference.vue to use leagueCreationPresets and DraftCreationSettings."
```

---

## Final verification

- [ ] **Run all integration tests**

```powershell
cd I:\CodeProjects\FantasyCritic\src
dotnet test FantasyCritic.IntegrationTests -v minimal
```

Expected: all tests pass, including the 4 new `MultiDraftCreationTests`.

- [ ] **Run the full solution build**

```powershell
cd I:\CodeProjects\FantasyCritic\src
dotnet build
```

Expected: 0 errors, 0 warnings (TreatWarningsAsErrors is on for key projects).

- [ ] **Manual smoke test**

Start the dev server and verify:
1. Create League page shows the three-question preset UI (How many players / Game Mode / Experience Level)
2. Selecting Multi Draft shows two draft sections in the form
3. Adding a third draft with "+ Add Another Draft" works; removing it works
4. Submitting a Standard league creates 1 draft row (verified via league year's Drafts list)
5. Submitting a Multi Draft league creates 2+ draft rows
6. Edit League Year page shows a single draft config section for single-draft leagues; shows "Visit Manage Drafts" for multi-draft leagues
7. Saving the edit updates the draft's GamesToDraft/CounterPicksToDraft
