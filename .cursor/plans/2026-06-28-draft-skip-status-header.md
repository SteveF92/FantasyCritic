# Draft Skip Status Header Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Surface a list of skipped draft picks (since the last real pick) in `draftStatusHeader.vue`, including whether each skip was automatic or manual.

**Architecture:** Add `IsManualSkip` to the DB skip table and bubble it through `PublisherDraftPickSkip` → `PastDraftPick` → a new `SkippedPicksSinceLastRealPick` list on `DraftStatus` → `LeagueDraftViewModel` → Vue computed property that renders a human-readable message.

**Tech Stack:** MySQL migration (DbUp), C# / .NET (Lib, MySQL, Web projects), NUnit tests, Vue 2 (Options API).

---

## File Map

| File | Change |
|------|--------|
| `src/FantasyCritic.DatabaseUpdater/Scripts/Sequential/2026-06-28_000_draftPickSkipIsManual.sql` | **Create** — adds `IsManualSkip bit(1) NOT NULL` to `tbl_league_draftpickskip` |
| `src/FantasyCritic.Lib/Domain/PublisherDraftInfo.cs` | **Modify** — add `bool IsManualSkip` to `PublisherDraftPickSkip` |
| `src/FantasyCritic.Lib/Domain/Draft/DraftStatus.cs` | **Modify** — add `bool? IsManualSkip` to `PastDraftPick`; add `SkippedPicksSinceLastRealPick` to `DraftStatus` |
| `src/FantasyCritic.Lib/Domain/Draft/DraftFunctions.cs` | **Modify** — pass `IsManualSkip` through `GetPastDraftPicks`; compute trailing skips in `GetDraftStatus` |
| `src/FantasyCritic.MySQL/Entities/DraftPickSkipEntity.cs` | **Modify** — add `IsManualSkip` property; update `ToDomain()` |
| `src/FantasyCritic.MySQL/MySQLFantasyCriticRepo.cs` | **Modify** — add `bool isManualSkip` to `AddDraftPickSkip`; update INSERT SQL; fix explicit SELECT in single-publisher query |
| `src/FantasyCritic.Lib/Interfaces/IFantasyCriticRepo.cs` | **Modify** — `AddDraftPickSkip` signature |
| `src/FantasyCritic.Lib/Services/DraftService.cs` | **Modify** — two call sites for `AddDraftPickSkip` |
| `src/FantasyCritic.Web/Models/Responses/SkippedPickViewModel.cs` | **Create** — new class |
| `src/FantasyCritic.Web/Models/Requests/League/LeagueDraftViewModel.cs` | **Modify** — add `SkippedPicksSinceLastRealPick` |
| `src/FantasyCritic.Test/Draft/GetDraftStatusTestBuilder.cs` | **Modify** — add `SkipPick()` method and `SkipSpec` inner type |
| `src/FantasyCritic.Test/Draft/GetDraftStatusTests.cs` | **Modify** — add tests for `SkippedPicksSinceLastRealPick` |
| `src/FantasyCritic.Web/ClientApp/src/components/draftStatusHeader.vue` | **Modify** — add `skippedPicksMessage` computed + template |

---

## Task 1: DB Migration

**Files:**
- Create: `src/FantasyCritic.DatabaseUpdater/Scripts/Sequential/2026-06-28_000_draftPickSkipIsManual.sql`

- [ ] **Step 1: Create the migration file**

```sql
ALTER TABLE tbl_league_draftpickskip
    ADD COLUMN IsManualSkip bit(1) NOT NULL DEFAULT 0;

ALTER TABLE tbl_league_draftpickskip
    ALTER COLUMN IsManualSkip DROP DEFAULT;
```

- [ ] **Step 2: Commit**

```
git add src/FantasyCritic.DatabaseUpdater/Scripts/Sequential/2026-06-28_000_draftPickSkipIsManual.sql
git commit -m "Add IsManualSkip column to tbl_league_draftpickskip."
```

---

## Task 2: Domain Layer — Lib + Test Builder

Update domain types, the draft functions, and fix the test builder so the solution compiles.

**Files:**
- Modify: `src/FantasyCritic.Lib/Domain/PublisherDraftInfo.cs`
- Modify: `src/FantasyCritic.Lib/Domain/Draft/DraftStatus.cs`
- Modify: `src/FantasyCritic.Lib/Domain/Draft/DraftFunctions.cs`
- Modify: `src/FantasyCritic.Test/Draft/GetDraftStatusTestBuilder.cs`

- [ ] **Step 1: Add `IsManualSkip` to `PublisherDraftPickSkip`**

Open `src/FantasyCritic.Lib/Domain/PublisherDraftInfo.cs`. The file currently ends with:

```csharp
public record PublisherDraftPickSkip(bool CounterPick, int PickNumber);
```

Replace it with:

```csharp
public record PublisherDraftPickSkip(bool CounterPick, int PickNumber, bool IsManualSkip);
```

- [ ] **Step 2: Add `IsManualSkip` to `PastDraftPick` and add `SkippedPicksSinceLastRealPick` to `DraftStatus`**

Open `src/FantasyCritic.Lib/Domain/Draft/DraftStatus.cs`. Replace the entire file with:

```csharp
namespace FantasyCritic.Lib.Domain.Draft;

public class DraftStatus
{
    public DraftStatus(LeagueDraft draft, FutureDraftPick nextPick, PastDraftPick? previousPick, PastDraftPick? previousNonSkippedPick, IReadOnlyList<FutureDraftPick> picksToSkip, IReadOnlyList<PastDraftPick> skippedPicksSinceLastRealPick)
    {
        Draft = draft;
        NextPick = nextPick;
        PreviousPick = previousPick;
        PreviousNonSkippedPick = previousNonSkippedPick;
        PicksToSkip = picksToSkip;
        SkippedPicksSinceLastRealPick = skippedPicksSinceLastRealPick;
    }

    public LeagueDraft Draft { get; }
    public FutureDraftPick NextPick { get; }
    public PastDraftPick? PreviousPick { get; }
    public PastDraftPick? PreviousNonSkippedPick { get; }
    public IReadOnlyList<FutureDraftPick> PicksToSkip { get; }
    public IReadOnlyList<PastDraftPick> SkippedPicksSinceLastRealPick { get; }

    public DraftPhase DraftPhase
    {
        get
        {
            if (NextPick.CounterPick)
            {
                return DraftPhase.CounterPicks;
            }

            return DraftPhase.StandardGames;
        }
    }

    public Publisher NextDraftPublisher => NextPick.Publisher;
    public Publisher? PreviousPublisherThatWasNotSkipped => PreviousNonSkippedPick?.Publisher;
    public int RoundNumber => NextPick.RoundNumber;
    public int OverallPickNumber => NextPick.OverallPickNumber ?? throw new InvalidOperationException($"NextPick for publisher {NextPick.Publisher.PublisherID} has no pick number. NextPick should never be a skipped pick.");
}

public record PastDraftPick(Publisher Publisher, bool CounterPick, int RoundNumber, PublisherGame? GameSelected, bool? IsManualSkip)
{
    public bool Skipped => GameSelected is null;
    public int? OverallPickNumber => GameSelected?.OverallDraftPosition;
}

public record FutureDraftPick(Publisher Publisher, bool CounterPick, int RoundNumber, int? OverallPickNumber)
{
    public bool WillBeSkipped => OverallPickNumber is null;
}

public record PickProcessingResult(FutureDraftPick? NextPick, IReadOnlyList<FutureDraftPick> PicksToSkip);
```

- [ ] **Step 3: Update `GetPastDraftPicks` to pass `IsManualSkip` through, and update `GetDraftStatus` to compute `SkippedPicksSinceLastRealPick`**

Open `src/FantasyCritic.Lib/Domain/Draft/DraftFunctions.cs`.

In `GetDraftStatus` (starting at line 64), replace the body of the method with:

```csharp
public static DraftStatus? GetDraftStatus(LeagueYear leagueYear)
{
    if (leagueYear.ActiveDraft is null)
    {
        return null;
    }

    var previousDraftPicks = GetPastDraftPicks(leagueYear, leagueYear.ActiveDraft);
    var processedPicks = GetFutureDraftPicks(leagueYear, leagueYear.ActiveDraft, previousDraftPicks);
    if (processedPicks.NextPick is null)
    {
        return null;
    }

    var previousDraftPick = previousDraftPicks.LastOrDefault();
    var previousNonSkippedPick = previousDraftPicks.LastOrDefault(x => !x.Skipped);

    var skippedPicksSinceLastRealPick = previousDraftPicks
        .Reverse()
        .TakeWhile(x => x.Skipped)
        .Reverse()
        .ToList();

    var draftStatus = new DraftStatus(leagueYear.ActiveDraft, processedPicks.NextPick, previousDraftPick, previousNonSkippedPick, processedPicks.PicksToSkip, skippedPicksSinceLastRealPick);
    return draftStatus;
}
```

In `GetPastDraftPicks`, update the two places where a skipped `PastDraftPick` is constructed. Find the standard-games loop:

```csharp
if (skipLookup.Contains((publisher.PublisherID, false, roundNumber)))
{
    draftPicks.Add(new PastDraftPick(publisher, false, roundNumber, null));
```

Replace with:

```csharp
if (skipLookup.TryGetValue((publisher.PublisherID, false, roundNumber), out var stdSkip))
{
    draftPicks.Add(new PastDraftPick(publisher, false, roundNumber, null, stdSkip.IsManualSkip));
```

And for the counter-picks loop:

```csharp
if (skipLookup.Contains((publisher.PublisherID, true, roundNumber)))
{
    draftPicks.Add(new PastDraftPick(publisher, true, roundNumber, null));
```

Replace with:

```csharp
if (skipLookup.TryGetValue((publisher.PublisherID, true, roundNumber), out var cpSkip))
{
    draftPicks.Add(new PastDraftPick(publisher, true, roundNumber, null, cpSkip.IsManualSkip));
```

**Also update the `skipLookup` definition** in `GetPastDraftPicks`. Currently it is a `HashSet<(Guid, bool, int)>`. It needs to become a `Dictionary` so we can look up the `IsManualSkip` value:

Current code (around line 209):
```csharp
var skipLookup = draft.PublisherDraftInfo
    .SelectMany(info => info.PickSkips, (info, skip) => (info.PublisherID, skip.CounterPick, skip.PickNumber))
    .ToHashSet();
```

Replace with:
```csharp
var skipLookup = draft.PublisherDraftInfo
    .SelectMany(info => info.PickSkips, (info, skip) => (Key: (info.PublisherID, skip.CounterPick, skip.PickNumber), Skip: skip))
    .ToDictionary(x => x.Key, x => x.Skip);
```

Also update the real-pick `PastDraftPick` construction sites to include `null` for `IsManualSkip`. Find:

```csharp
draftPicks.Add(new PastDraftPick(publisher, false, roundNumber, pickedGame));
```

Replace with:
```csharp
draftPicks.Add(new PastDraftPick(publisher, false, roundNumber, pickedGame, null));
```

And:
```csharp
draftPicks.Add(new PastDraftPick(publisher, true, roundNumber, pickedGame));
```

Replace with:
```csharp
draftPicks.Add(new PastDraftPick(publisher, true, roundNumber, pickedGame, null));
```

- [ ] **Step 4: Update `GetDraftStatusTestBuilder` to support skips and fix `SkipSpec`**

Open `src/FantasyCritic.Test/Draft/GetDraftStatusTestBuilder.cs`.

Add a `SkipSpec` record inside `GetDraftStatusTestBuilder` (after `GameSpec`):

```csharp
internal sealed record SkipSpec(int DraftNumber, bool CounterPick, int PickNumber, bool IsManualSkip);
```

Add `List<SkipSpec> Skips { get; } = [];` to `PublisherSpec`:

```csharp
internal sealed class PublisherSpec(Guid publisherID, string name, int draftPosition)
{
    public Guid PublisherID { get; } = publisherID;
    public string Name { get; } = name;
    public int DraftPosition { get; } = draftPosition;
    public List<GameSpec> Games { get; } = [];
    public List<SkipSpec> Skips { get; } = [];
}
```

Add `AddSkip` method to `GetDraftStatusTestBuilder`:

```csharp
internal void AddSkip(DraftSpec draft, Publisher publisher, bool counterPick, int pickNumber, bool isManualSkip)
{
    var publisherSpec = _publisherSpecs.Single(x => x.PublisherID == publisher.PublisherID);
    publisherSpec.Skips.Add(new SkipSpec(draft.DraftNumber, counterPick, pickNumber, isManualSkip));
}
```

Update the `PublisherDraftInfo` construction in `Build()` to include skips. Find:

```csharp
var draftInfos = orderedDrafts
    .Select(draftSpec => new PublisherDraftInfo(
        DraftIDFor(draftSpec.DraftNumber),
        draftSpec.DraftNumber,
        publisherSpec.PublisherID,
        publisherSpec.DraftPosition,
        new List<PublisherDraftPickSkip>()))
    .ToList();
```

Replace with:

```csharp
var draftInfos = orderedDrafts
    .Select(draftSpec => new PublisherDraftInfo(
        DraftIDFor(draftSpec.DraftNumber),
        draftSpec.DraftNumber,
        publisherSpec.PublisherID,
        publisherSpec.DraftPosition,
        publisherSpec.Skips
            .Where(s => s.DraftNumber == draftSpec.DraftNumber)
            .Select(s => new PublisherDraftPickSkip(s.CounterPick, s.PickNumber, s.IsManualSkip))
            .ToList()))
    .ToList();
```

Update `CreateLeagueDraft` the same way. Find:

```csharp
var publisherDraftInfo = publisherSpecs
    .Select(publisher => new PublisherDraftInfo(draftID, draftSpec.DraftNumber, publisher.PublisherID, publisher.DraftPosition, new List<PublisherDraftPickSkip>()))
    .ToList();
```

Replace with:

```csharp
var publisherDraftInfo = publisherSpecs
    .Select(publisher => new PublisherDraftInfo(draftID, draftSpec.DraftNumber, publisher.PublisherID, publisher.DraftPosition,
        publisher.Skips
            .Where(s => s.DraftNumber == draftSpec.DraftNumber)
            .Select(s => new PublisherDraftPickSkip(s.CounterPick, s.PickNumber, s.IsManualSkip))
            .ToList()))
    .ToList();
```

- [ ] **Step 5: Add `SkipPick` to `DraftScenarioBuilder`**

Add the following method to `DraftScenarioBuilder`:

```csharp
public DraftScenarioBuilder SkipPick(bool isManual = false)
{
    var leagueYear = _root.BuildLeagueYear(forceActiveDraftNumber: _draft.DraftNumber);
    var status = DraftFunctions.GetDraftStatus(leagueYear)
        ?? throw new InvalidOperationException($"Cannot skip in draft {_draft.DraftNumber}: draft has no active turn.");

    _root.AddSkip(_draft, status.NextDraftPublisher, status.NextPick.CounterPick, status.RoundNumber, isManual);
    return this;
}
```

- [ ] **Step 6: Build to verify no compile errors**

```
cd I:\CodeProjects\FantasyCritic\src
dotnet build FantasyCritic.slnx
```

Expected: 0 errors. Fix any compile issues before proceeding.

- [ ] **Step 7: Commit**

```
git add src/FantasyCritic.Lib/Domain/PublisherDraftInfo.cs
git add src/FantasyCritic.Lib/Domain/Draft/DraftStatus.cs
git add src/FantasyCritic.Lib/Domain/Draft/DraftFunctions.cs
git add src/FantasyCritic.Test/Draft/GetDraftStatusTestBuilder.cs
git commit -m "Add IsManualSkip to domain types and compute SkippedPicksSinceLastRealPick in DraftStatus."
```

---

## Task 3: MySQL + Repo + Service Layer

Wire up `IsManualSkip` in the persistence and service layers.

**Files:**
- Modify: `src/FantasyCritic.MySQL/Entities/DraftPickSkipEntity.cs`
- Modify: `src/FantasyCritic.MySQL/MySQLFantasyCriticRepo.cs`
- Modify: `src/FantasyCritic.Lib/Interfaces/IFantasyCriticRepo.cs`
- Modify: `src/FantasyCritic.Lib/Services/DraftService.cs`

- [ ] **Step 1: Update `DraftPickSkipEntity`**

Replace the entire contents of `src/FantasyCritic.MySQL/Entities/DraftPickSkipEntity.cs` with:

```csharp
namespace FantasyCritic.MySQL.Entities;

internal class DraftPickSkipEntity
{
    public Guid DraftID { get; set; }
    public Guid PublisherID { get; set; }
    public bool CounterPick { get; set; }
    public int PickNumber { get; set; }
    public bool IsManualSkip { get; set; }

    public PublisherDraftPickSkip ToDomain() => new PublisherDraftPickSkip(CounterPick, PickNumber, IsManualSkip);
}
```

- [ ] **Step 2: Update `IFantasyCriticRepo.AddDraftPickSkip`**

Open `src/FantasyCritic.Lib/Interfaces/IFantasyCriticRepo.cs`. Find:

```csharp
Task AddDraftPickSkip(LeagueDraft draft, Publisher publisher, bool counterPick, int pickNumber, LeagueAction action);
```

Replace with:

```csharp
Task AddDraftPickSkip(LeagueDraft draft, Publisher publisher, bool counterPick, int pickNumber, bool isManualSkip, LeagueAction action);
```

- [ ] **Step 3: Update `MySQLFantasyCriticRepo.AddDraftPickSkip`**

Open `src/FantasyCritic.MySQL/MySQLFantasyCriticRepo.cs`. Find the `AddDraftPickSkip` method (around line 1058):

```csharp
public async Task AddDraftPickSkip(LeagueDraft draft, Publisher publisher, bool counterPick, int pickNumber, LeagueAction action)
{
    const string sql = "INSERT INTO tbl_league_draftpickskip (DraftID, PublisherID, CounterPick, PickNumber) " +
                       "VALUES (@draftID, @publisherID, @counterPick, @pickNumber);";
    await using var connection = new MySqlConnection(_connectionString);
    await connection.OpenAsync();
    await using var transaction = await connection.BeginTransactionAsync();
    await connection.ExecuteAsync(sql,
        new { draftID = draft.DraftID, publisherID = publisher.PublisherID, counterPick, pickNumber },
        transaction);
    await AddLeagueAction(action, connection, transaction);
    await transaction.CommitAsync();
}
```

Replace with:

```csharp
public async Task AddDraftPickSkip(LeagueDraft draft, Publisher publisher, bool counterPick, int pickNumber, bool isManualSkip, LeagueAction action)
{
    const string sql = "INSERT INTO tbl_league_draftpickskip (DraftID, PublisherID, CounterPick, PickNumber, IsManualSkip) " +
                       "VALUES (@draftID, @publisherID, @counterPick, @pickNumber, @isManualSkip);";
    await using var connection = new MySqlConnection(_connectionString);
    await connection.OpenAsync();
    await using var transaction = await connection.BeginTransactionAsync();
    await connection.ExecuteAsync(sql,
        new { draftID = draft.DraftID, publisherID = publisher.PublisherID, counterPick, pickNumber, isManualSkip },
        transaction);
    await AddLeagueAction(action, connection, transaction);
    await transaction.CommitAsync();
}
```

- [ ] **Step 4: Fix the single-publisher explicit SELECT in `MySQLFantasyCriticRepo`**

In the same file, find the single-publisher query (around line 2121) where columns are listed explicitly:

```csharp
const string draftPickSkipSql =
    """
    SELECT DraftID, PublisherID, CounterPick, PickNumber
    FROM tbl_league_draftpickskip
    WHERE PublisherID = @publisherID
    """;
```

Replace with:

```csharp
const string draftPickSkipSql =
    """
    SELECT DraftID, PublisherID, CounterPick, PickNumber, IsManualSkip
    FROM tbl_league_draftpickskip
    WHERE PublisherID = @publisherID
    """;
```

- [ ] **Step 5: Update `DraftService` — two `AddDraftPickSkip` call sites**

Open `src/FantasyCritic.Lib/Services/DraftService.cs`.

**Manual skip call site** (around line 142 — `SkipCurrentDraftPick` method):

Find:
```csharp
await _fantasyCriticRepo.AddDraftPickSkip(leagueYear.ActiveDraft, nextPick.Publisher, nextPick.CounterPick, nextPick.RoundNumber, action);
```

Replace with:
```csharp
await _fantasyCriticRepo.AddDraftPickSkip(leagueYear.ActiveDraft, nextPick.Publisher, nextPick.CounterPick, nextPick.RoundNumber, isManualSkip: true, action);
```

**Auto-skip call site** (around line 239 — inside the draft-and-advance loop):

Find:
```csharp
await _fantasyCriticRepo.AddDraftPickSkip(activeDraft, pickToSkip.Publisher, pickToSkip.CounterPick, pickToSkip.RoundNumber, action);
```

Replace with:
```csharp
await _fantasyCriticRepo.AddDraftPickSkip(activeDraft, pickToSkip.Publisher, pickToSkip.CounterPick, pickToSkip.RoundNumber, isManualSkip: false, action);
```

- [ ] **Step 6: Build to verify no compile errors**

```
cd I:\CodeProjects\FantasyCritic\src
dotnet build FantasyCritic.slnx
```

Expected: 0 errors.

- [ ] **Step 7: Commit**

```
git add src/FantasyCritic.MySQL/Entities/DraftPickSkipEntity.cs
git add src/FantasyCritic.MySQL/MySQLFantasyCriticRepo.cs
git add src/FantasyCritic.Lib/Interfaces/IFantasyCriticRepo.cs
git add src/FantasyCritic.Lib/Services/DraftService.cs
git commit -m "Wire IsManualSkip through MySQL entity, repo, and service layer."
```

---

## Task 4: Web Layer

Create `SkippedPickViewModel` and expose the list in `LeagueDraftViewModel`.

**Files:**
- Create: `src/FantasyCritic.Web/Models/Responses/SkippedPickViewModel.cs`
- Modify: `src/FantasyCritic.Web/Models/Requests/League/LeagueDraftViewModel.cs`

- [ ] **Step 1: Create `SkippedPickViewModel`**

Create `src/FantasyCritic.Web/Models/Responses/SkippedPickViewModel.cs`:

```csharp
using FantasyCritic.Lib.Domain.Draft;

namespace FantasyCritic.Web.Models.Responses;

public class SkippedPickViewModel
{
    public SkippedPickViewModel(PastDraftPick domain)
    {
        PublisherName = domain.Publisher.GetPublisherAndUserDisplayName();
        RoundNumber = domain.RoundNumber;
        CounterPick = domain.CounterPick;
        IsManualSkip = domain.IsManualSkip!.Value; // non-null guaranteed: only skipped picks are mapped here
    }

    public string PublisherName { get; }
    public int RoundNumber { get; }
    public bool CounterPick { get; }
    public bool IsManualSkip { get; }
}
```

- [ ] **Step 2: Add `SkippedPicksSinceLastRealPick` to `LeagueDraftViewModel`**

Open `src/FantasyCritic.Web/Models/Requests/League/LeagueDraftViewModel.cs`.

Add the `using` at the top if not already present:
```csharp
using FantasyCritic.Web.Models.Responses;
```

In the constructor, inside the `if (domain.PlayStatus.DraftIsActiveOrPaused)` block, after the existing `if (draftStatus is not null)` block, add:

```csharp
if (draftStatus is not null)
{
    NextPickPublisherName = draftStatus.NextDraftPublisher.GetPublisherAndUserDisplayName();
    NextPickRoundNumber = draftStatus.RoundNumber;
    NextPickIsCounterPick = draftStatus.NextPick.CounterPick;
    SkippedPicksSinceLastRealPick = draftStatus.SkippedPicksSinceLastRealPick
        .Select(x => new SkippedPickViewModel(x))
        .ToList();
}
```

Note: `SkippedPicksSinceLastRealPick` must also be initialized to an empty list when `draftStatus` is null. Add the property declaration and set the default:

```csharp
public IReadOnlyList<SkippedPickViewModel> SkippedPicksSinceLastRealPick { get; } = [];
```

Replace the inline initialization with a field set in the constructor. The full constructor block for `DraftIsActiveOrPaused` should look like:

```csharp
if (domain.PlayStatus.DraftIsActiveOrPaused)
{
    var draftStatus = DraftFunctions.GetDraftStatus(leagueYear);
    DraftingCounterPicks = DraftPhase.CounterPicks.Equals(draftStatus?.DraftPhase);
    if (draftStatus is not null)
    {
        NextPickPublisherName = draftStatus.NextDraftPublisher.GetPublisherAndUserDisplayName();
        NextPickRoundNumber = draftStatus.RoundNumber;
        NextPickIsCounterPick = draftStatus.NextPick.CounterPick;
        SkippedPicksSinceLastRealPick = draftStatus.SkippedPicksSinceLastRealPick
            .Select(x => new SkippedPickViewModel(x))
            .ToList();
    }
}
```

Add the property:

```csharp
public IReadOnlyList<SkippedPickViewModel> SkippedPicksSinceLastRealPick { get; } = [];
```

Because C# `init`-only properties can't be set in an `if` block like this, you'll need to use a backing field or make it a regular `{ get; private set; }`. Change the existing immutable properties in the class to use a backing field pattern for `SkippedPicksSinceLastRealPick` specifically, or declare it as `{ get; }` and set it via a local variable. The cleanest approach: declare a local variable before the if block:

```csharp
IReadOnlyList<SkippedPickViewModel> skippedPicks = [];

if (domain.PlayStatus.DraftIsActiveOrPaused)
{
    var draftStatus = DraftFunctions.GetDraftStatus(leagueYear);
    DraftingCounterPicks = DraftPhase.CounterPicks.Equals(draftStatus?.DraftPhase);
    if (draftStatus is not null)
    {
        NextPickPublisherName = draftStatus.NextDraftPublisher.GetPublisherAndUserDisplayName();
        NextPickRoundNumber = draftStatus.RoundNumber;
        NextPickIsCounterPick = draftStatus.NextPick.CounterPick;
        skippedPicks = draftStatus.SkippedPicksSinceLastRealPick
            .Select(x => new SkippedPickViewModel(x))
            .ToList();
    }
}

SkippedPicksSinceLastRealPick = skippedPicks;
```

And declare the property as `public IReadOnlyList<SkippedPickViewModel> SkippedPicksSinceLastRealPick { get; }`.

- [ ] **Step 3: Build to verify no compile errors**

```
cd I:\CodeProjects\FantasyCritic\src
dotnet build FantasyCritic.slnx
```

Expected: 0 errors.

- [ ] **Step 4: Commit**

```
git add src/FantasyCritic.Web/Models/Responses/SkippedPickViewModel.cs
git add src/FantasyCritic.Web/Models/Requests/League/LeagueDraftViewModel.cs
git commit -m "Add SkippedPickViewModel and expose SkippedPicksSinceLastRealPick in LeagueDraftViewModel."
```

---

## Task 5: Tests for `SkippedPicksSinceLastRealPick`

**Files:**
- Modify: `src/FantasyCritic.Test/Draft/GetDraftStatusTests.cs`

- [ ] **Step 1: Add tests**

Open `src/FantasyCritic.Test/Draft/GetDraftStatusTests.cs`. Add the following tests inside the `GetDraftStatusTests` class. The test builder's `SkipPick(isManual: bool)` method skips the publisher who is currently `NextDraftPublisher`.

```csharp
[Test]
public void GetDraftStatus_NoSkips_SkippedPicksSinceLastRealPickIsEmpty()
{
    var leagueYear = new GetDraftStatusTestBuilder()
        .WithPublishers(3)
        .WithDraft(gamesToDraft: 2, counterPicksToDraft: 0, PlayStatus.Drafting)
        .PickStandard()
        .PickStandard()
        .Build();

    var status = DraftFunctions.GetDraftStatus(leagueYear)!;
    Assert.That(status.SkippedPicksSinceLastRealPick, Is.Empty);
}

[Test]
public void GetDraftStatus_OneAutoSkip_SkippedPicksSinceLastRealPickContainsThatSkip()
{
    // 2 publishers, 2 rounds. After round 1 pick 1 (P1), skip P2 in round 1.
    // P2 gets a skip record; P1 is next in round 2.
    var leagueYear = new GetDraftStatusTestBuilder()
        .WithPublishers(2)
        .WithDraft(gamesToDraft: 2, counterPicksToDraft: 0, PlayStatus.Drafting)
        .PickStandard()     // P1 picks round 1
        .SkipPick(isManual: false)  // P2 is skipped in round 1
        .Build();

    var activeDraft = leagueYear.ActiveDraft!;
    var status = DraftFunctions.GetDraftStatus(leagueYear)!;

    Assert.That(status.SkippedPicksSinceLastRealPick, Has.Count.EqualTo(1));
    Assert.That(status.SkippedPicksSinceLastRealPick[0].Publisher.GetDraftPosition(activeDraft.DraftID), Is.EqualTo(2));
    Assert.That(status.SkippedPicksSinceLastRealPick[0].IsManualSkip, Is.False);
    Assert.That(status.SkippedPicksSinceLastRealPick[0].Skipped, Is.True);
}

[Test]
public void GetDraftStatus_OneManualSkip_SkippedPicksSinceLastRealPickIsManual()
{
    var leagueYear = new GetDraftStatusTestBuilder()
        .WithPublishers(2)
        .WithDraft(gamesToDraft: 2, counterPicksToDraft: 0, PlayStatus.Drafting)
        .PickStandard()
        .SkipPick(isManual: true)
        .Build();

    var status = DraftFunctions.GetDraftStatus(leagueYear)!;

    Assert.That(status.SkippedPicksSinceLastRealPick, Has.Count.EqualTo(1));
    Assert.That(status.SkippedPicksSinceLastRealPick[0].IsManualSkip, Is.True);
}

[Test]
public void GetDraftStatus_TwoConsecutiveSkips_BothInSkippedPicksSinceLastRealPick()
{
    // 3 publishers, 2 rounds. P1 picks round 1. Then P2 and P3 are both skipped in round 1.
    var leagueYear = new GetDraftStatusTestBuilder()
        .WithPublishers(3)
        .WithDraft(gamesToDraft: 2, counterPicksToDraft: 0, PlayStatus.Drafting)
        .PickStandard()             // P1 picks round 1
        .SkipPick(isManual: false)  // P2 is skipped round 1
        .SkipPick(isManual: false)  // P3 is skipped round 1
        .Build();

    var status = DraftFunctions.GetDraftStatus(leagueYear)!;

    Assert.That(status.SkippedPicksSinceLastRealPick, Has.Count.EqualTo(2));
}

[Test]
public void GetDraftStatus_SkipThenRealPick_OnlySkipsAfterLastRealPickReturned()
{
    // Snake order for 2 publishers: Round 1 = [P1, P2], Round 2 = [P2, P1], Round 3 = [P1, P2].
    // Sequence:
    //   PickStandard  → P1 picks round 1 (overall pick 1)
    //   SkipPick      → P2 is skipped round 1
    //   PickStandard  → P2 picks round 2 (first in snake reversal, overall pick 2)
    //   SkipPick      → P1 is skipped round 2
    // After: previousDraftPicks = [P1r1(real), P2r1(skip), P2r2(real), P1r2(skip)]
    // Trailing skips = [P1r2] → SkippedPicksSinceLastRealPick has 1 entry (P1 round 2).
    var leagueYear = new GetDraftStatusTestBuilder()
        .WithPublishers(2)
        .WithDraft(gamesToDraft: 3, counterPicksToDraft: 0, PlayStatus.Drafting)
        .PickStandard()             // P1 picks round 1
        .SkipPick(isManual: false)  // P2 is skipped round 1
        .PickStandard()             // P2 picks round 2 (snake reversed: P2 goes first)
        .SkipPick(isManual: false)  // P1 is skipped round 2
        .Build();

    var status = DraftFunctions.GetDraftStatus(leagueYear)!;

    // Only P1's round-2 skip appears — P2's round-1 skip is before the last real pick (P2 round 2).
    Assert.That(status.SkippedPicksSinceLastRealPick, Has.Count.EqualTo(1));
    Assert.That(status.SkippedPicksSinceLastRealPick[0].RoundNumber, Is.EqualTo(2));
}

[Test]
public void GetDraftStatus_SkipAtStart_NoRealPicksYet_SkippedPicksSinceLastRealPickContainsAllSkips()
{
    // If the draft starts and the very first publisher is skipped, there's no "last real pick".
    // All trailing skips should be returned.
    var leagueYear = new GetDraftStatusTestBuilder()
        .WithPublishers(2)
        .WithDraft(gamesToDraft: 2, counterPicksToDraft: 0, PlayStatus.Drafting)
        .SkipPick(isManual: true)  // P1 is skipped before any real pick has happened
        .Build();

    var status = DraftFunctions.GetDraftStatus(leagueYear)!;

    Assert.That(status.SkippedPicksSinceLastRealPick, Has.Count.EqualTo(1));
    Assert.That(status.SkippedPicksSinceLastRealPick[0].IsManualSkip, Is.True);
}
```

- [ ] **Step 2: Run the new tests**

```
cd I:\CodeProjects\FantasyCritic\src
dotnet test FantasyCritic.Test/FantasyCritic.Test.csproj --filter "FullyQualifiedName~GetDraftStatus" -v normal
```

Expected: all tests pass (including the new ones and all pre-existing draft status tests).

- [ ] **Step 3: Commit**

```
git add src/FantasyCritic.Test/Draft/GetDraftStatusTests.cs
git commit -m "Add tests for SkippedPicksSinceLastRealPick in GetDraftStatus."
```

---

## Task 6: Frontend — `draftStatusHeader.vue`

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/components/draftStatusHeader.vue`

- [ ] **Step 1: Add `skippedPicksMessage` computed property**

Open `src/FantasyCritic.Web/ClientApp/src/components/draftStatusHeader.vue`.

Replace the `<script>` section with:

```javascript
<script>
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
  computed: {
    skippedPicksMessage() {
      const skips = this.activeDraft?.skippedPicksSinceLastRealPick;
      if (!skips || skips.length === 0) return null;

      // Count occurrences per publisher name
      const counts = {};
      for (const skip of skips) {
        counts[skip.publisherName] = (counts[skip.publisherName] || 0) + 1;
      }

      const distinctNames = Object.keys(counts);

      if (distinctNames.length === 1) {
        const name = distinctNames[0];
        const count = counts[name];
        const hasManual = skips.some(s => s.publisherName === name && s.isManualSkip);

        if (hasManual) {
          return `${name}'s pick was skipped by the league manager.`;
        }

        if (count === 1) {
          return `${name}'s pick was auto-skipped because they have no open slots.`;
        }

        const timesWord = count === 2 ? 'twice' : `${count} times`;
        return `${name}'s pick was auto-skipped ${timesWord} because they have no open slots.`;
      }

      // Multiple distinct publishers — keep it simple
      const nameList = distinctNames.join(', ');
      return `The following players had their draft picks skipped: ${nameList}. See the History page for more information.`;
    }
  }
};
</script>
```

- [ ] **Step 2: Add the message to both alert blocks in the template**

In the template, inside the `v-if="!userIsNextInDraft"` alert (info alert), add after the existing "Next to draft:" `<div>`:

```html
<div v-if="skippedPicksMessage" class="text-muted small mt-1">{{ skippedPicksMessage }}</div>
```

In the `v-else` alert (success alert — user's turn), add the same line after the "It is your turn to draft!" `<div>`:

```html
<div v-if="skippedPicksMessage" class="text-muted small mt-1">{{ skippedPicksMessage }}</div>
```

The full updated template section:

```html
<template>
  <div>
    <div v-if="activeDraft?.draftIsPaused">
      <div class="alert alert-danger">
        <div v-if="!league.isManager">The draft has been paused. Speak to your league manager for details.</div>
        <div v-else>
          The draft has been paused. You can undo games that have been drafted.
          <b-button v-b-modal="'setPauseModal'" variant="success">Resume Draft</b-button>
        </div>
      </div>
    </div>
    <div v-if="activeDraft?.draftIsActive && nextPublisherUp">
      <div v-if="!userIsNextInDraft">
        <div class="alert alert-info">
          <div v-show="!activeDraft?.draftingCounterPicks">The draft is currently in progress!</div>
          <div v-show="activeDraft?.draftingCounterPicks">It's time to draft Counter Picks!</div>
          <div>
            Next to draft:
            <strong>{{ nextPublisherUp.publisherName }}</strong>
          </div>
          <div v-if="skippedPicksMessage" class="text-muted small mt-1">{{ skippedPicksMessage }}</div>
          <div v-if="league.isManager">To select the next player's game for them, Select 'Select Next Game' under 'Draft Management' in the sidebar!</div>
        </div>
      </div>
      <div v-else>
        <div class="alert alert-success draft-header">
          <div>
            <div v-show="!activeDraft?.draftingCounterPicks">The draft is currently in progress!</div>
            <div v-show="activeDraft?.draftingCounterPicks">It's time to draft counter picks!</div>
            <div><strong>It is your turn to draft!</strong></div>
            <div v-if="skippedPicksMessage" class="text-muted small mt-1">{{ skippedPicksMessage }}</div>
          </div>
          <div v-if="!activeDraft?.draftingCounterPicks">
            <b-button v-b-modal="'playerDraftGameForm'" variant="primary">Draft Game</b-button>
          </div>
          <div v-else>
            <b-button v-b-modal="'playerDraftCounterPickForm'" variant="primary">Draft Counter Pick</b-button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
```

- [ ] **Step 3: Commit**

```
git add src/FantasyCritic.Web/ClientApp/src/components/draftStatusHeader.vue
git commit -m "Show skipped pick message in draftStatusHeader."
```

---

## Final Verification

- [ ] **Run all tests**

```
cd I:\CodeProjects\FantasyCritic\src
dotnet test FantasyCritic.slnx -v normal
```

Expected: all tests pass.

- [ ] **Build frontend**

```
cd I:\CodeProjects\FantasyCritic\src\FantasyCritic.Web\ClientApp
npm run build
```

Expected: no errors or warnings related to the changed component.
