# Bids Only Before Next Scheduled Draft — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a `BidsOnlyBeforeNextScheduledDraft` league option that blocks bids on games whose `MaximumReleaseDate` is not strictly before the next scheduled draft date, enforced at both placement time and processing time.

**Architecture:** A single `bool` field threads through all four settings layers (`LeagueYearParameters` → `LeagueOptions` → `LeagueYearEntity` → `LeagueYearSettingsViewModel`). The eligibility check lives inside `GameEligibilityFunctions.GetGenericSlotMasterGameErrors` as a non-overridable `ClaimError`, which is called by both `GameAcquisitionService.MakePickupBid` (placement) and `ActionProcessor.ProcessPickups` (processing) — so a single code change covers both enforcement points.

**Tech Stack:** C# / .NET (`FantasyCritic.Lib`, `FantasyCritic.Web`), NUnit, NodaTime `LocalDate`, Vue 2 Options API, MySQL (DbUp migration)

---

### Task 1: Database migration

**Files:**
- Create: `src/FantasyCritic.DatabaseUpdater/Scripts/Sequential/2026-06-28_001_bidsOnlyBeforeNextScheduledDraft.sql`

- [ ] **Step 1: Create the migration script**

  ```sql
  ALTER TABLE tbl_leagueyear
    ADD COLUMN BidsOnlyBeforeNextScheduledDraft TINYINT(1) NOT NULL DEFAULT 0;

  ALTER TABLE tbl_leagueyear
    ALTER COLUMN BidsOnlyBeforeNextScheduledDraft DROP DEFAULT;
  ```

  `DEFAULT 0` is applied immediately to all existing rows (feature off for every existing league). The default is then dropped so future inserts must be explicit.

- [ ] **Step 2: Verify the script runs**

  Run the DatabaseUpdater project against your local dev database. Confirm the column appears with `SHOW COLUMNS FROM tbl_leagueyear LIKE 'BidsOnlyBeforeNextScheduledDraft';` — expected: one row, Type `tinyint(1)`, Null `NO`, Default `NULL`.

- [ ] **Step 3: Commit**

  ```
  git add src/FantasyCritic.DatabaseUpdater/Scripts/Sequential/2026-06-28_001_bidsOnlyBeforeNextScheduledDraft.sql
  git commit -m "Add BidsOnlyBeforeNextScheduledDraft column migration."
  ```

---

### Task 2: Domain plumbing — thread `BidsOnlyBeforeNextScheduledDraft` through all layers

This task updates five files. All must be done before the solution compiles again; commit them together.

**Files:**
- Modify: `src/FantasyCritic.Lib/Domain/Requests/LeagueYearParameters.cs`
- Modify: `src/FantasyCritic.Lib/Domain/LeagueOptions.cs`
- Modify: `src/FantasyCritic.Lib/SharedSerialization/Database/LeagueYearEntity.cs`
- Modify: `src/FantasyCritic.Web/Models/RoundTrip/LeagueYearSettingsViewModel.cs`
- Modify: `src/FantasyCritic.Web/Models/Requests/LeagueManager/EditLeagueYearRequest.cs`
- Modify: `src/FantasyCritic.Test/Discord/BaseGameNewsTests.cs`
- Modify: `src/FantasyCritic.Test/Draft/GetDraftStatusTestBuilder.cs`

---

#### Step 1: `LeagueYearParameters.cs` — add the new parameter

Add `bool bidsOnlyBeforeNextScheduledDraft` as the last constructor parameter and expose it as a property. The full updated constructor signature (append the new param):

```csharp
public LeagueYearParameters(Guid leagueID, int year, string? leagueYearName, int standardGames, int counterPicks,
    int unrestrictedReleaseStatusDroppableGames, int willNotReleaseDroppableGames, int willReleaseDroppableGames,
    bool dropOnlyDraftGames, bool grantSuperDrops, bool counterPicksBlockDrops, bool allowMoveIntoIneligible,
    int minimumBidAmount, bool enableBids, IEnumerable<LeagueTagStatus> leagueTags,
    IEnumerable<SpecialGameSlot> specialGameSlots, DraftSystem draftSystem, PickupSystem pickupSystem,
    ScoringSystem scoringSystem, TradingSystem tradingSystem, TiebreakSystem tiebreakSystem,
    ReleaseSystem releaseSystem, IneligibleGameSystem ineligibleGameSystem,
    AnnualDate counterPickDeadline, AnnualDate? mightReleaseDroppableDate,
    bool bidsOnlyBeforeNextScheduledDraft)
```

Add the assignment in the body:
```csharp
BidsOnlyBeforeNextScheduledDraft = bidsOnlyBeforeNextScheduledDraft;
```

Add the property:
```csharp
public bool BidsOnlyBeforeNextScheduledDraft { get; }
```

---

#### Step 2: `LeagueOptions.cs` — primary constructor, `LeagueYearParameters` ctor, property, and three internal callers

**A. Primary constructor** — add `bool bidsOnlyBeforeNextScheduledDraft` as the last parameter:

```csharp
public LeagueOptions(int standardGames, int counterPicks, int unrestrictedReleaseStatusDroppableGames,
    int willNotReleaseDroppableGames, int willReleaseDroppableGames, bool dropOnlyDraftGames, bool grantSuperDrops,
    bool counterPicksBlockDrops, bool allowMoveIntoIneligible, int minimumBidAmount, bool enableBids,
    IEnumerable<LeagueTagStatus> leagueTags, IEnumerable<SpecialGameSlot> specialGameSlots,
    DraftSystem draftSystem, PickupSystem pickupSystem, ScoringSystem scoringSystem,
    TradingSystem tradingSystem, TiebreakSystem tiebreakSystem, ReleaseSystem releaseSystem,
    IneligibleGameSystem ineligibleGameSystem, AnnualDate counterPickDeadline,
    AnnualDate? mightReleaseDroppableDate,
    bool bidsOnlyBeforeNextScheduledDraft)
```

Add to ctor body:
```csharp
BidsOnlyBeforeNextScheduledDraft = bidsOnlyBeforeNextScheduledDraft;
```

**B. `LeagueYearParameters` ctor overload** — read from the parameters object:
```csharp
BidsOnlyBeforeNextScheduledDraft = parameters.BidsOnlyBeforeNextScheduledDraft;
```

**C. Add property:**
```csharp
public bool BidsOnlyBeforeNextScheduledDraft { get; }
```
Place it after `public AnnualDate? MightReleaseDroppableDate { get; }`.

**D. `GetDifferences`** — add inside the differences list checks, after the `MightReleaseDroppableDate` block:
```csharp
if (BidsOnlyBeforeNextScheduledDraft != existingOptions.BidsOnlyBeforeNextScheduledDraft)
{
    differences.Add($"'Bids Only Before Next Scheduled Draft' changed from '{existingOptions.BidsOnlyBeforeNextScheduledDraft.ToYesNoString()}' to '{BidsOnlyBeforeNextScheduledDraft.ToYesNoString()}'.");
}
```

**E. `UpdateOptionsForYear`** — add `BidsOnlyBeforeNextScheduledDraft` as the last argument to the `new LeagueOptions(...)` call:
```csharp
LeagueOptions options = new LeagueOptions(StandardGames, CounterPicks, UnrestrictedReleaseStatusDroppableGames,
    WillNotReleaseDroppableGames, WillReleaseDroppableGames, DropOnlyDraftGames, GrantSuperDrops,
    CounterPicksBlockDrops, AllowMoveIntoIneligible, MinimumBidAmount, EnableBids, LeagueTags,
    SpecialGameSlots, DraftSystem, PickupSystem, newScoringSystem, TradingSystem, TiebreakSystem,
    ReleaseSystem, IneligibleGameSystem, CounterPickDeadline, MightReleaseDroppableDate,
    BidsOnlyBeforeNextScheduledDraft);
```

**F. `WithNewDraftOptions`** — same addition to the `newOptions` constructor call:
```csharp
LeagueOptions newOptions = new LeagueOptions(totalStandardGames, totalCounterPicks,
    UnrestrictedReleaseStatusDroppableGames, WillNotReleaseDroppableGames, WillReleaseDroppableGames,
    DropOnlyDraftGames, GrantSuperDrops, CounterPicksBlockDrops, AllowMoveIntoIneligible,
    MinimumBidAmount, EnableBids, LeagueTags, totalSpecialGameSlots, DraftSystem, PickupSystem,
    ScoringSystem, TradingSystem, TiebreakSystem, ReleaseSystem, IneligibleGameSystem,
    CounterPickDeadline, MightReleaseDroppableDate, BidsOnlyBeforeNextScheduledDraft);
```

---

#### Step 3: `LeagueYearEntity.cs` — add column property, write in constructor, read in `ToDomain`

**A. Add property:**
```csharp
public bool BidsOnlyBeforeNextScheduledDraft { get; set; }
```
Place it after `public int? MightReleaseDroppableDay { get; set; }`.

**B. In the writing constructor** (`public LeagueYearEntity(League league, int year, LeagueOptions options, ...)`), add after the `MightReleaseDroppableDay` assignment:
```csharp
BidsOnlyBeforeNextScheduledDraft = options.BidsOnlyBeforeNextScheduledDraft;
```

**C. In `ToDomain`**, add `BidsOnlyBeforeNextScheduledDraft` as the last argument to `new LeagueOptions(...)`:
```csharp
LeagueOptions options = new LeagueOptions(StandardGames, CounterPicks, UnrestrictedReleaseStatusDroppableGames,
    WillNotReleaseDroppableGames, WillReleaseDroppableGames, DropOnlyDraftGames, GrantSuperDrops,
    CounterPicksBlockDrops, AllowMoveIntoIneligible, MinimumBidAmount, EnableBids, leagueTags,
    specialGameSlots, draftSystem, pickupSystem, scoringSystem, tradingSystem, tiebreakSystem,
    releaseSystem, ineligibleGameSystem, counterPickDeadline, mightReleaseDroppableDate,
    BidsOnlyBeforeNextScheduledDraft);
```

---

#### Step 4: `LeagueYearSettingsViewModel.cs` — add to `[JsonConstructor]`, `LeagueYear` ctor, and `ToDomain`

**A. `[JsonConstructor]`** — add `bool bidsOnlyBeforeNextScheduledDraft` as the last parameter (after `List<SpecialGameSlotViewModel> specialGameSlots`), and assign it:
```csharp
BidsOnlyBeforeNextScheduledDraft = bidsOnlyBeforeNextScheduledDraft;
```

**B. `LeagueYear`-based constructor** — add:
```csharp
BidsOnlyBeforeNextScheduledDraft = leagueYear.Options.BidsOnlyBeforeNextScheduledDraft;
```

**C. Add property** (after `public bool EnableBids { get; }`):
```csharp
public bool BidsOnlyBeforeNextScheduledDraft { get; }
```

**D. `ToDomain`** — pass it as the last argument to `new LeagueYearParameters(...)`:
```csharp
LeagueYearParameters parameters = new LeagueYearParameters(LeagueID, Year, LeagueYearName,
    StandardGames, CounterPicks, unrestrictedReleaseStatusDroppableGames,
    willNotReleaseDroppableGames, willReleaseDroppableGames, DropOnlyDraftGames, GrantSuperDrops,
    CounterPicksBlockDrops, AllowMoveIntoIneligible, MinimumBidAmount, EnableBids, leagueTags,
    specialGameSlots, draftSystem, pickupSystem, scoringSystem, tradingSystem, tiebreakSystem,
    releaseSystem, ineligibleGameSystem, counterPickDeadline, mightReleaseDroppableDate,
    BidsOnlyBeforeNextScheduledDraft);
```

---

#### Step 5: `EditLeagueYearRequest.cs` — pass through in `ToDomain`

The `settings` object (which is a `LeagueYearParameters` returned from `LeagueYearSettings.ToDomain(...)`) already has the new field, but the manual `new LeagueYearParameters(...)` construction in this file needs updating. Add `parsed.BidsOnlyBeforeNextScheduledDraft` as the last argument:

```csharp
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
    parsed.MightReleaseDroppableDate,
    parsed.BidsOnlyBeforeNextScheduledDraft);
```

---

#### Step 6: Fix test files that call the positional `new LeagueOptions(...)` constructor

**`BaseGameNewsTests.cs`** — the existing call ends with `new AnnualDate(10, 1)`. Append `false`:

```csharp
var leagueOptions = new LeagueOptions(10, 5, 2, 1, 0, false, false, false, false, 0, true, leagueTags,
    new List<SpecialGameSlot>(),
    DraftSystem.Flexible, PickupSystem.SemiPublicBiddingSecretCounterPicks,
    ScoringSystem.GetDefaultScoringSystem(2025),
    TradingSystem.Standard, TiebreakSystem.LowestProjectedPoints, ReleaseSystem.MustBeReleased,
    IneligibleGameSystem.CaseByCase,
    new AnnualDate(10, 1), new AnnualDate(10, 1), false);
```

**`GetDraftStatusTestBuilder.cs`** — the existing call ends with `new AnnualDate(10, 1)`. Append `false`:

```csharp
var leagueOptions = new LeagueOptions(
    10,
    5,
    orderedDrafts.Max(x => x.GamesToDraft),
    orderedDrafts.Max(x => x.CounterPicksToDraft),
    0,
    false,
    false,
    false,
    false,
    0,
    true,
    [new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["PRT"], TagStatus.Banned)],
    [],
    DraftSystem.Flexible,
    PickupSystem.SemiPublicBiddingSecretCounterPicks,
    ScoringSystem.GetDefaultScoringSystem(_year),
    TradingSystem.Standard,
    TiebreakSystem.LowestProjectedPoints,
    ReleaseSystem.MustBeReleased,
    IneligibleGameSystem.CaseByCase,
    new AnnualDate(10, 1),
    new AnnualDate(10, 1),
    false);
```

---

#### Step 7: Build and commit

- [ ] **Run the build and confirm zero errors:**

  ```powershell
  dotnet build src/FantasyCritic.sln
  ```
  Expected: Build succeeded, 0 error(s).

- [ ] **Commit:**

  ```
  git add -A
  git commit -m "Thread BidsOnlyBeforeNextScheduledDraft through all settings layers."
  ```

---

### Task 3: Unit tests (write failing) + eligibility check (TDD)

**Files:**
- Modify: `src/FantasyCritic.Test/EligibilityTests.cs`
- Modify: `src/FantasyCritic.Lib/BusinessLogicFunctions/GameEligibilityFunctions.cs`

#### Step 1: Add a minimal `LeagueYear` builder helper to `EligibilityTests.cs`

The existing eligibility tests only call `LeagueTagExtensions.GameHasValidTags` (which takes no `LeagueYear`). The new tests must call `GameEligibilityFunctions.GetGenericSlotMasterGameErrors`, which does take a `LeagueYear`. Add this private static helper to the `EligibilityTests` class (add the required usings if not already present):

```csharp
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
// (add to the existing using block at the top of the file)
```

```csharp
private static LeagueYear CreateLeagueYearForBidTest(
    bool bidsOnlyBeforeNextScheduledDraft,
    LocalDate? pendingDraftScheduledDate)
{
    var leagueID = Guid.NewGuid();
    var year = 2026;
    var leagueYearKey = new LeagueYearKey(leagueID, year);

    var manager = new MinimalFantasyCriticUser(Guid.NewGuid(), "Manager", "manager@test.com");
    var league = new League(leagueID, "Test League", manager, null, null,
        [new MinimalLeagueYearInfo(year, false, true)],
        true, false, false, false, 0);

    var supportedYear = new SupportedYear(year, true, true, true, new LocalDate(year - 1, 12, 8), false);

    var options = new LeagueOptions(
        10, 2,
        0, 0, 0,
        false, false, false, false,
        0, true,
        [], [],
        DraftSystem.Flexible,
        PickupSystem.SemiPublicBiddingSecretCounterPicks,
        ScoringSystem.GetDefaultScoringSystem(year),
        TradingSystem.Standard,
        TiebreakSystem.LowestProjectedPoints,
        ReleaseSystem.MustBeReleased,
        IneligibleGameSystem.CaseByCase,
        new AnnualDate(12, 1),
        null,
        bidsOnlyBeforeNextScheduledDraft);

    // Draft 1 is DraftFinal so it is no longer pending.
    var draftID1 = Guid.NewGuid();
    var draft1 = new LeagueDraft(draftID1, leagueYearKey, 1, "Initial Draft",
        null, 5, 1, true, PlayStatus.DraftFinal, [], null);

    // Draft 2 is NotStartedDraft (pending), with the caller-supplied scheduled date.
    var draftID2 = Guid.NewGuid();
    var draft2 = new LeagueDraft(draftID2, leagueYearKey, 2, "Draft 2",
        pendingDraftScheduledDate, 5, 1, false, PlayStatus.NotStartedDraft, [], null);

    return new LeagueYear(league, supportedYear, options, [draft1, draft2],
        [], [], null, [], null, false, null);
}

private static LeagueYear CreateLeagueYearForBidTestNoPendingDraft(
    bool bidsOnlyBeforeNextScheduledDraft)
{
    var leagueID = Guid.NewGuid();
    var year = 2026;
    var leagueYearKey = new LeagueYearKey(leagueID, year);

    var manager = new MinimalFantasyCriticUser(Guid.NewGuid(), "Manager", "manager@test.com");
    var league = new League(leagueID, "Test League", manager, null, null,
        [new MinimalLeagueYearInfo(year, false, true)],
        true, false, false, false, 0);

    var supportedYear = new SupportedYear(year, true, true, true, new LocalDate(year - 1, 12, 8), false);

    var options = new LeagueOptions(
        10, 2,
        0, 0, 0,
        false, false, false, false,
        0, true,
        [], [],
        DraftSystem.Flexible,
        PickupSystem.SemiPublicBiddingSecretCounterPicks,
        ScoringSystem.GetDefaultScoringSystem(year),
        TradingSystem.Standard,
        TiebreakSystem.LowestProjectedPoints,
        ReleaseSystem.MustBeReleased,
        IneligibleGameSystem.CaseByCase,
        new AnnualDate(12, 1),
        null,
        bidsOnlyBeforeNextScheduledDraft);

    // Only one draft, already final — no pending draft.
    var draftID = Guid.NewGuid();
    var draft = new LeagueDraft(draftID, leagueYearKey, 1, "Initial Draft",
        null, 10, 2, true, PlayStatus.DraftFinal, [], null);

    return new LeagueYear(league, supportedYear, options, [draft],
        [], [], null, [], null, false, null);
}
```

#### Step 2: Add the failing unit tests

Add these nine test methods to `EligibilityTests`. They all call `GameEligibilityFunctions.GetGenericSlotMasterGameErrors` and inspect the result for the new check. At this point they will fail (expecting errors but getting none, or vice versa).

```csharp
// Helper: create a master game with a specific MaximumReleaseDate.
// Reuses the existing CreateComplexMasterGame method that's already in the file.
// Reminder: CreateComplexMasterGame(name, minimumReleaseDate, maximumReleaseDate?, ...)

[Test]
public void BidsOnlyBeforeNextDraft_GameReleasesBeforeDraft_Allowed()
{
    var draftDate = new LocalDate(2026, 6, 1);
    var maxRelease = new LocalDate(2026, 5, 31); // strictly before draft
    var game = CreateComplexMasterGame("Game A", new LocalDate(2026, 1, 1), maxRelease, null, null, null,
        [MasterGameTagDictionary.TagDictionary["NG"]]);
    var leagueYear = CreateLeagueYearForBidTest(bidsOnlyBeforeNextScheduledDraft: true, pendingDraftScheduledDate: draftDate);
    var currentDate = new LocalDate(2026, 1, 15);

    var errors = GameEligibilityFunctions.GetGenericSlotMasterGameErrors(
        leagueYear, game, dropping: false, currentDate, currentDate,
        counterPick: false, counterPickedGameIsManualWillNotRelease: false,
        drafting: false, partOfSpecialAuction: false);

    Assert.That(errors.Any(e => e.Error.Contains("only allows bids")), Is.False,
        "No 'bids only before draft' error expected when MaximumReleaseDate < draft date.");
}

[Test]
public void BidsOnlyBeforeNextDraft_GameReleasesOnDraftDate_Blocked()
{
    var draftDate = new LocalDate(2026, 6, 1);
    var maxRelease = draftDate; // same day — not strictly before
    var game = CreateComplexMasterGame("Game B", new LocalDate(2026, 1, 1), maxRelease, null, null, null,
        [MasterGameTagDictionary.TagDictionary["NG"]]);
    var leagueYear = CreateLeagueYearForBidTest(bidsOnlyBeforeNextScheduledDraft: true, pendingDraftScheduledDate: draftDate);
    var currentDate = new LocalDate(2026, 1, 15);

    var errors = GameEligibilityFunctions.GetGenericSlotMasterGameErrors(
        leagueYear, game, dropping: false, currentDate, currentDate,
        counterPick: false, counterPickedGameIsManualWillNotRelease: false,
        drafting: false, partOfSpecialAuction: false);

    var bidError = errors.FirstOrDefault(e => e.Error.Contains("only allows bids"));
    Assert.That(bidError, Is.Not.Null, "Expected a 'bids only before draft' error when MaximumReleaseDate == draft date.");
    Assert.That(bidError!.Overridable, Is.False, "The error must be non-overridable.");
}

[Test]
public void BidsOnlyBeforeNextDraft_GameReleasesAfterDraft_Blocked()
{
    var draftDate = new LocalDate(2026, 6, 1);
    var maxRelease = new LocalDate(2026, 7, 1); // after draft
    var game = CreateComplexMasterGame("Game C", new LocalDate(2026, 1, 1), maxRelease, null, null, null,
        [MasterGameTagDictionary.TagDictionary["NG"]]);
    var leagueYear = CreateLeagueYearForBidTest(bidsOnlyBeforeNextScheduledDraft: true, pendingDraftScheduledDate: draftDate);
    var currentDate = new LocalDate(2026, 1, 15);

    var errors = GameEligibilityFunctions.GetGenericSlotMasterGameErrors(
        leagueYear, game, dropping: false, currentDate, currentDate,
        counterPick: false, counterPickedGameIsManualWillNotRelease: false,
        drafting: false, partOfSpecialAuction: false);

    var bidError = errors.FirstOrDefault(e => e.Error.Contains("only allows bids"));
    Assert.That(bidError, Is.Not.Null, "Expected a 'bids only before draft' error when MaximumReleaseDate > draft date.");
    Assert.That(bidError!.Overridable, Is.False);
}

[Test]
public void BidsOnlyBeforeNextDraft_NullMaximumReleaseDate_Blocked()
{
    var draftDate = new LocalDate(2026, 6, 1);
    // null MaximumReleaseDate — unknown upper bound
    var game = CreateComplexMasterGame("Game D", new LocalDate(2026, 1, 1), null, null, null, null,
        [MasterGameTagDictionary.TagDictionary["NG"]]);
    var leagueYear = CreateLeagueYearForBidTest(bidsOnlyBeforeNextScheduledDraft: true, pendingDraftScheduledDate: draftDate);
    var currentDate = new LocalDate(2026, 1, 15);

    var errors = GameEligibilityFunctions.GetGenericSlotMasterGameErrors(
        leagueYear, game, dropping: false, currentDate, currentDate,
        counterPick: false, counterPickedGameIsManualWillNotRelease: false,
        drafting: false, partOfSpecialAuction: false);

    var bidError = errors.FirstOrDefault(e => e.Error.Contains("only allows bids"));
    Assert.That(bidError, Is.Not.Null, "Expected a 'bids only before draft' error when MaximumReleaseDate is null.");
    Assert.That(bidError!.Overridable, Is.False);
}

[Test]
public void BidsOnlyBeforeNextDraft_NoDraftScheduledDate_AllBidsBlocked()
{
    // Option on, pending draft exists but has no scheduled date.
    var maxRelease = new LocalDate(2026, 5, 31);
    var game = CreateComplexMasterGame("Game E", new LocalDate(2026, 1, 1), maxRelease, null, null, null,
        [MasterGameTagDictionary.TagDictionary["NG"]]);
    var leagueYear = CreateLeagueYearForBidTest(bidsOnlyBeforeNextScheduledDraft: true, pendingDraftScheduledDate: null);
    var currentDate = new LocalDate(2026, 1, 15);

    var errors = GameEligibilityFunctions.GetGenericSlotMasterGameErrors(
        leagueYear, game, dropping: false, currentDate, currentDate,
        counterPick: false, counterPickedGameIsManualWillNotRelease: false,
        drafting: false, partOfSpecialAuction: false);

    var bidError = errors.FirstOrDefault(e => e.Error.Contains("only allows bids"));
    Assert.That(bidError, Is.Not.Null, "Expected a 'bids only before draft' error when pending draft has no scheduled date.");
    Assert.That(bidError!.Overridable, Is.False);
}

[Test]
public void BidsOnlyBeforeNextDraft_NoPendingDraft_NoRestriction()
{
    // Option on, but all drafts are complete — no pending draft.
    var maxRelease = new LocalDate(2099, 1, 1); // far-future release
    var game = CreateComplexMasterGame("Game F", new LocalDate(2026, 1, 1), maxRelease, null, null, null,
        [MasterGameTagDictionary.TagDictionary["NG"]]);
    var leagueYear = CreateLeagueYearForBidTestNoPendingDraft(bidsOnlyBeforeNextScheduledDraft: true);
    var currentDate = new LocalDate(2026, 1, 15);

    var errors = GameEligibilityFunctions.GetGenericSlotMasterGameErrors(
        leagueYear, game, dropping: false, currentDate, currentDate,
        counterPick: false, counterPickedGameIsManualWillNotRelease: false,
        drafting: false, partOfSpecialAuction: false);

    Assert.That(errors.Any(e => e.Error.Contains("only allows bids")), Is.False,
        "No bid restriction expected when there is no pending draft.");
}

[Test]
public void BidsOnlyBeforeNextDraft_OptionOff_NoRestriction()
{
    // Option off — the restriction must not apply regardless of dates.
    var draftDate = new LocalDate(2026, 6, 1);
    var maxRelease = new LocalDate(2026, 7, 1); // would fail if option were on
    var game = CreateComplexMasterGame("Game G", new LocalDate(2026, 1, 1), maxRelease, null, null, null,
        [MasterGameTagDictionary.TagDictionary["NG"]]);
    var leagueYear = CreateLeagueYearForBidTest(bidsOnlyBeforeNextScheduledDraft: false, pendingDraftScheduledDate: draftDate);
    var currentDate = new LocalDate(2026, 1, 15);

    var errors = GameEligibilityFunctions.GetGenericSlotMasterGameErrors(
        leagueYear, game, dropping: false, currentDate, currentDate,
        counterPick: false, counterPickedGameIsManualWillNotRelease: false,
        drafting: false, partOfSpecialAuction: false);

    Assert.That(errors.Any(e => e.Error.Contains("only allows bids")), Is.False,
        "No bid restriction when option is off.");
}

[Test]
public void BidsOnlyBeforeNextDraft_DroppingIsExempt()
{
    // Even when the option is on and dates would normally block, dropping = true skips the check.
    var draftDate = new LocalDate(2026, 6, 1);
    var maxRelease = new LocalDate(2026, 7, 1); // would fail for a bid
    var game = CreateComplexMasterGame("Game H", new LocalDate(2026, 1, 1), maxRelease, null, null, null,
        [MasterGameTagDictionary.TagDictionary["NG"]]);
    var leagueYear = CreateLeagueYearForBidTest(bidsOnlyBeforeNextScheduledDraft: true, pendingDraftScheduledDate: draftDate);
    var currentDate = new LocalDate(2026, 1, 15);

    var errors = GameEligibilityFunctions.GetGenericSlotMasterGameErrors(
        leagueYear, game, dropping: true, currentDate, currentDate,
        counterPick: false, counterPickedGameIsManualWillNotRelease: false,
        drafting: false, partOfSpecialAuction: false);

    Assert.That(errors.Any(e => e.Error.Contains("only allows bids")), Is.False,
        "Drop actions must be exempt from this restriction.");
}

[Test]
public void BidsOnlyBeforeNextDraft_DraftingIsExempt()
{
    // Drafting = true skips the check entirely.
    var draftDate = new LocalDate(2026, 6, 1);
    var maxRelease = new LocalDate(2026, 7, 1);
    var game = CreateComplexMasterGame("Game I", new LocalDate(2026, 1, 1), maxRelease, null, null, null,
        [MasterGameTagDictionary.TagDictionary["NG"]]);
    var leagueYear = CreateLeagueYearForBidTest(bidsOnlyBeforeNextScheduledDraft: true, pendingDraftScheduledDate: draftDate);
    var currentDate = new LocalDate(2026, 1, 15);

    var errors = GameEligibilityFunctions.GetGenericSlotMasterGameErrors(
        leagueYear, game, dropping: false, currentDate, currentDate,
        counterPick: false, counterPickedGameIsManualWillNotRelease: false,
        drafting: true, partOfSpecialAuction: false);

    Assert.That(errors.Any(e => e.Error.Contains("only allows bids")), Is.False,
        "Drafting actions must be exempt from this restriction.");
}
```

- [ ] **Run the unit tests to confirm they fail:**

  ```powershell
  dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj --filter "BidsOnlyBeforeNextDraft" --no-build
  ```
  Expected: 8 failures (all the tests that expect an error find none; the "Allowed" test may pass already).

---

#### Step 3: Implement the eligibility check in `GameEligibilityFunctions.cs`

Inside `GetGenericSlotMasterGameErrors`, add the following block **before the `return claimErrors;`** statement at the end of the method. Place it after the counter-pick deadline block (after the closing `}` of the `if (counterPick && !drafting)` block):

```csharp
if (!dropping && !drafting && leagueYear.Options.BidsOnlyBeforeNextScheduledDraft)
{
    var pendingDraft = leagueYear.PendingDraft;
    if (pendingDraft is not null)
    {
        if (pendingDraft.ScheduledDate is null)
        {
            claimErrors.Add(new ClaimError(
                "Your league only allows bids for games that release before the next scheduled draft, but your next draft does not have a scheduled date.",
                false));
        }
        else
        {
            var scheduledDate = pendingDraft.ScheduledDate.Value;
            var maxRelease = masterGame.MaximumReleaseDate;
            if (maxRelease is null)
            {
                claimErrors.Add(new ClaimError(
                    $"Your league only allows bids for games that release before the next scheduled draft ({scheduledDate.ToLongDate()}), but this game has no known maximum release date.",
                    false));
            }
            else if (maxRelease.Value >= scheduledDate)
            {
                claimErrors.Add(new ClaimError(
                    $"Your league only allows bids for games that release before the next scheduled draft ({scheduledDate.ToLongDate()}).",
                    false));
            }
        }
    }
    // else: no pending draft — all drafts are done, no restriction
}
```

Note: `ToLongDate()` is already imported via `FantasyCritic.Lib.Extensions` (already referenced in this file through existing date formatting calls).

- [ ] **Run the unit tests again — all 9 should now pass:**

  ```powershell
  dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj --filter "BidsOnlyBeforeNextDraft"
  ```
  Expected: 9 passed, 0 failed.

- [ ] **Run the full unit test suite to confirm no regressions:**

  ```powershell
  dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj
  ```
  Expected: All tests pass.

- [ ] **Commit:**

  ```
  git add src/FantasyCritic.Test/EligibilityTests.cs
  git add src/FantasyCritic.Lib/BusinessLogicFunctions/GameEligibilityFunctions.cs
  git commit -m "Add BidsOnlyBeforeNextScheduledDraft eligibility check with unit tests."
  ```

---

### Task 4: Frontend settings UI

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/components/leagueYearSettings.vue`
- Modify: `src/FantasyCritic.IntegrationTests/Helpers/LeagueScenario.cs`

#### Step 1: Add the checkbox to `leagueYearSettings.vue`

The Bidding Settings section currently ends with the `minimumBidAmount` `<div class="form-group">`. Insert the new checkbox **immediately after** that closing `</div>` and **before** the outer `</div>` that closes the `v-show="gameMode !== 'One Shot'"` Bidding Settings block:

```html
<div v-if="isMultiDraft && internalValue.enableBids" class="form-group">
  <b-form-checkbox v-model="internalValue.bidsOnlyBeforeNextScheduledDraft">
    <span class="checkbox-label">Only allow bids for games that release before the next scheduled draft</span>
    <p>
      When enabled, bids can only be placed on games whose maximum known release date falls before your
      next draft's scheduled date. This ensures the bidding system is used only for games that no one
      will have a chance to draft.
    </p>
  </b-form-checkbox>
</div>
```

`isMultiDraft` is already a prop on this component (used for the existing multi-draft info banner). The checkbox is only rendered when the league is multi-draft AND bids are enabled.

#### Step 2: Add `BidsOnlyBeforeNextScheduledDraft` to `LeagueScenario`

In `LeagueScenario.cs`, add a new property with a default of `false`:

```csharp
public bool BidsOnlyBeforeNextScheduledDraft { get; init; } = false;
```

Place it after the `EnableBids` property.

In `BuildSettings`, add the new field to the `LeagueYearSettingsViewModel` initializer:

```csharp
BidsOnlyBeforeNextScheduledDraft = BidsOnlyBeforeNextScheduledDraft,
```

(This is already a `required` init-style object initializer; add it after the `EnableBids` line.)

- [ ] **Verify the frontend compiles with no TypeScript/Vue errors:**

  ```powershell
  npx tsc --noEmit
  ```
  Run from `src/FantasyCritic.Web/ClientApp`. Expected: no errors.

- [ ] **Commit:**

  ```
  git add src/FantasyCritic.Web/ClientApp/src/components/leagueYearSettings.vue
  git add src/FantasyCritic.IntegrationTests/Helpers/LeagueScenario.cs
  git commit -m "Add BidsOnlyBeforeNextScheduledDraft settings checkbox and LeagueScenario support."
  ```

---

### Task 5: Frontend settings validation

When `bidsOnlyBeforeNextScheduledDraft` is enabled, the UI must require that all non-first drafts have a scheduled date set — leaving it unscheduled blocks all bidding.

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/components/leagueYearSettings.vue` (or wherever the draft-settings validation lives — search for the `bidsOnlyBeforeNextScheduledDraft` watcher + `DraftCreationSettings.vue` if draft settings are a sub-component)

> **Note:** The exact location of this validation depends on how draft scheduling is exposed in the create/edit league flow. In `createLeague.vue` and `editLeague.vue`, find where `DraftCreationSettings.vue` is used and where per-draft `scheduledDate` fields are entered. The validation described below should live near that form component.

- [ ] **Step 1: Add the validation computed property or watcher**

  In the parent view that holds both the settings and draft configuration (most likely `createLeague.vue` and `editLeague.vue`), add a computed property:

  ```javascript
  bidsOnlyBeforeNextDraftScheduleError() {
    if (!this.settings.bidsOnlyBeforeNextScheduledDraft) return null;
    const nonFirstDrafts = (this.drafts ?? []).slice(1); // everything after draft 1
    const anyMissingDate = nonFirstDrafts.some(d => !d.scheduledDate);
    if (anyMissingDate) {
      return "'Only allow bids before next scheduled draft' is enabled — all drafts after the first must have a scheduled date.";
    }
    return null;
  }
  ```

- [ ] **Step 2: Display the validation message near the draft configuration**

  In the template, next to (or below) the draft settings section, add:

  ```html
  <div v-if="bidsOnlyBeforeNextDraftScheduleError" class="alert alert-warning">
    {{ bidsOnlyBeforeNextDraftScheduleError }}
  </div>
  ```

- [ ] **Step 3: Block save when the validation fires**

  Find the submit handler (e.g., `submitLeague` or `editLeagueYear`) and gate it:

  ```javascript
  if (this.bidsOnlyBeforeNextDraftScheduleError) {
    return; // form-level guard; the alert already explains why
  }
  ```

- [ ] **Commit:**

  ```
  git add src/FantasyCritic.Web/ClientApp/src/views/createLeague.vue
  git add src/FantasyCritic.Web/ClientApp/src/views/editLeague.vue
  git commit -m "Add client-side validation: require draft scheduled dates when BidsOnlyBeforeNextScheduledDraft is on."
  ```

---

### Task 6: Integration tests

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/Tests/League/Actions/BidsOnlyBeforeNextDraftTests.cs`

These tests verify the full HTTP stack: that the API accepts bids for eligible games and rejects bids for ineligible games when the option is active.

- [ ] **Step 1: Create the test file**

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Actions;

/// <summary>
/// Verifies the BidsOnlyBeforeNextScheduledDraft league option blocks bids on games
/// whose MaximumReleaseDate is not strictly before the next scheduled draft date,
/// at both placement time and processing time.
/// </summary>
[TestFixture]
public class BidsOnlyBeforeNextDraftTests : IntegrationTestBase
{
    private ApiSession _adminSession = null!;

    [OneTimeSetUp]
    public async Task SetUpAdmin()
    {
        _adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(_adminSession);

        await _adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 6, 12, 0, 0, TimeSpan.Zero)
        });
    }

    /// <summary>
    /// Creates a multi-draft league with BidsOnlyBeforeNextScheduledDraft = true,
    /// draft 2 scheduled for a far-future date (2099-12-31), completes draft 1,
    /// and verifies that a bid on an unreleased game is accepted.
    /// Every unreleased game will have MaximumReleaseDate well before 2099-12-31.
    /// </summary>
    [Test]
    public async Task BidsOnlyBeforeNextDraft_GameClearlyBeforeDraftDate_BidAccepted()
    {
        var scenario = new LeagueScenario
        {
            Name = "BidsOnlyFarFutureDraft",
            PlayerCount = 2,
            StandardGames = 4,
            GamesToDraft = 2,
            CounterPicks = 1,
            CounterPicksToDraft = 1,
            UnrestrictedReleaseStatusDroppableGames = 0,
            WillNotReleaseDroppableGames = 0,
            WillReleaseDroppableGames = 0,
            DropOnlyDraftGames = true,
            GrantSuperDrops = false,
            CounterPicksBlockDrops = true,
            AllowMoveIntoIneligible = false,
            MinimumBidAmount = 0,
            EnableBids = true,
            BidsOnlyBeforeNextScheduledDraft = true,
            DraftSystem = "Flexible",
            PickupSystem = "SemiPublicBiddingSecretCounterPicks",
            ScoringSystem = "LinearPositive",
            TradingSystem = "Standard",
            TiebreakSystem = "LowestProjectedPoints",
            ReleaseSystem = "MustBeReleased",
            IneligibleGameSystem = "DroppableAsWillNotRelease",
        };

        var (email, password, displayName) = NewUser();
        using var managerSession = new ApiSession(Factory);
        await managerSession.RegisterAsync(email, password, displayName);

        var (p2Email, p2Password, p2DisplayName) = NewUser();
        using var p2Session = new ApiSession(Factory);
        await p2Session.RegisterAsync(p2Email, p2Password, p2DisplayName);

        var year = await LeagueTestHelpers.GetOpenYearAsync(managerSession);

        // Create a 2-draft league with draft 2 scheduled far in the future.
        var leagueID = await managerSession.LeagueManager.CreateLeagueAsync(new CreateLeagueRequest
        {
            LeagueName = $"BidsOnlyTest-{Guid.NewGuid():N}"[..30],
            PublicLeague = false,
            TestLeague = true,
            CustomRulesLeague = false,
            LeagueYearSettings = scenario.BuildSettings(year),
            Drafts = new System.Collections.Generic.List<DraftSettingsRequest>
            {
                new() { Name = null, ScheduledDate = null, GamesToDraft = 2, CounterPicksToDraft = 1 },
                new()
                {
                    Name = "Draft 2",
                    // Schedule far in the future so every currently-unreleased game qualifies.
                    ScheduledDate = new DateTimeOffset(2099, 12, 31, 0, 0, 0, TimeSpan.Zero),
                    GamesToDraft = 2,
                    CounterPicksToDraft = 0,
                },
            }
        });

        await LeagueTestHelpers.InviteAndAcceptAsync(managerSession, p2Session, leagueID);

        var mgrPubID = await LeagueTestHelpers.CreatePublisherAsync(managerSession, leagueID, year, "Manager");
        var p2PubID = await LeagueTestHelpers.CreatePublisherAsync(p2Session, leagueID, year, "Player2");

        await LeagueTestHelpers.SetDraftOrderAsync(managerSession, leagueID, year, [mgrPubID, p2PubID]);

        var fixture = await LeagueFixture.CreateLeagueWithMembersAsync(Factory, scenario, NewUser);
        // Use the sessions directly instead.

        // Start and complete draft 1.
        await managerSession.LeagueManager.StartDraftAsync(new StartDraftRequest { LeagueID = leagueID, Year = year });
        var mgrPublisher = new TestPublisher(1, managerSession, mgrPubID, "Manager");
        var p2Publisher = new TestPublisher(2, p2Session, p2PubID, "Player2");
        var tempFixture = new LeagueFixtureManual(leagueID, year, [mgrPublisher, p2Publisher]);
        await tempFixture.DraftToCompletionAsync();

        // Find an unreleased available game for the manager.
        var available = await managerSession.League.TopAvailableGamesAsync(year, leagueID, mgrPubID, null);
        var target = available.First(g => g.IsAvailable && !g.Taken && !g.IsReleased);

        // Place bid — must succeed because MaximumReleaseDate < 2099-12-31.
        var result = await managerSession.League.MakePickupBidAsync(new PickupBidRequest
        {
            PublisherID = mgrPubID,
            MasterGameID = target.MasterGame.MasterGameID,
            CounterPick = false,
            BidAmount = 5,
            AllowIneligibleSlot = false,
        });

        Assert.That(result.Success, Is.True,
            $"Bid should be accepted. Errors: {string.Join("; ", result.Errors ?? [])}");
    }

    /// <summary>
    /// Creates a multi-draft league with BidsOnlyBeforeNextScheduledDraft = true,
    /// draft 2 scheduled for 2025-01-07 (the day after the test clock starts).
    /// Finds a game with no MaximumReleaseDate (null) and verifies the bid is rejected.
    /// </summary>
    [Test]
    public async Task BidsOnlyBeforeNextDraft_GameWithNullMaxRelease_BidRejected()
    {
        var (email, password, displayName) = NewUser();
        using var managerSession = new ApiSession(Factory);
        await managerSession.RegisterAsync(email, password, displayName);

        var (p2Email, p2Password, p2DisplayName) = NewUser();
        using var p2Session = new ApiSession(Factory);
        await p2Session.RegisterAsync(p2Email, p2Password, p2DisplayName);

        var year = await LeagueTestHelpers.GetOpenYearAsync(managerSession);

        var leagueID = await managerSession.LeagueManager.CreateLeagueAsync(new CreateLeagueRequest
        {
            LeagueName = $"BidsOnlyTest2-{Guid.NewGuid():N}"[..29],
            PublicLeague = false,
            TestLeague = true,
            CustomRulesLeague = false,
            LeagueYearSettings = new LeagueYearSettingsViewModel
            {
                LeagueID = Guid.Empty,
                Year = year,
                LeagueYearName = null,
                StandardGames = 4,
                CounterPicks = 1,
                UnrestrictedReleaseStatusDroppableGames = 0,
                WillNotReleaseDroppableGames = 0,
                WillReleaseDroppableGames = 0,
                UnlimitedUnrestrictedReleaseStatusDroppableGames = false,
                UnlimitedWillNotReleaseDroppableGames = false,
                UnlimitedWillReleaseDroppableGames = false,
                DropOnlyDraftGames = true,
                GrantSuperDrops = false,
                CounterPicksBlockDrops = true,
                AllowMoveIntoIneligible = false,
                MinimumBidAmount = 0,
                EnableBids = true,
                BidsOnlyBeforeNextScheduledDraft = true,
                DraftSystem = "Flexible",
                PickupSystem = "SemiPublicBiddingSecretCounterPicks",
                ScoringSystem = "LinearPositive",
                TradingSystem = "Standard",
                TiebreakSystem = "LowestProjectedPoints",
                ReleaseSystem = "MustBeReleased",
                IneligibleGameSystem = "DroppableAsWillNotRelease",
                CounterPickDeadline = new DateTimeOffset(year, 12, 1, 0, 0, 0, TimeSpan.Zero),
                MightReleaseDroppableDate = null,
                Tags = new LeagueTagOptionsViewModel { Banned = [], Required = [] },
                HasSpecialSlots = false,
                SpecialGameSlots = [],
            },
            Drafts = new System.Collections.Generic.List<DraftSettingsRequest>
            {
                new() { Name = null, ScheduledDate = null, GamesToDraft = 2, CounterPicksToDraft = 1 },
                new()
                {
                    Name = "Draft 2",
                    // Scheduled for tomorrow — games with no max release date must be blocked.
                    ScheduledDate = new DateTimeOffset(2025, 1, 7, 0, 0, 0, TimeSpan.Zero),
                    GamesToDraft = 2,
                    CounterPicksToDraft = 0,
                },
            }
        });

        await LeagueTestHelpers.InviteAndAcceptAsync(managerSession, p2Session, leagueID);

        var mgrPubID = await LeagueTestHelpers.CreatePublisherAsync(managerSession, leagueID, year, "Manager");
        var p2PubID = await LeagueTestHelpers.CreatePublisherAsync(p2Session, leagueID, year, "Player2");
        await LeagueTestHelpers.SetDraftOrderAsync(managerSession, leagueID, year, [mgrPubID, p2PubID]);

        await managerSession.LeagueManager.StartDraftAsync(new StartDraftRequest { LeagueID = leagueID, Year = year });

        // Draft to completion via the live-player mock.
        var mgrPublisher = new TestPublisher(1, managerSession, mgrPubID, "Manager");
        var p2Publisher = new TestPublisher(2, p2Session, p2PubID, "Player2");

        // Use the helper to auto-pick until done.
        var draftHelper = new MockedLivePlayer(managerSession, p2Session);
        await draftHelper.DraftToCompletionAsync(leagueID, year);

        // Find a game with null MaximumReleaseDate.
        var available = await managerSession.League.TopAvailableGamesAsync(year, leagueID, mgrPubID, null);
        var nullMaxReleaseGame = available.FirstOrDefault(g =>
            g.IsAvailable && !g.Taken && !g.IsReleased && g.MasterGame.MaximumReleaseDate == null);

        if (nullMaxReleaseGame == null)
        {
            Assert.Inconclusive("No available game with null MaximumReleaseDate found in test data. " +
                                "This test requires a game without a known maximum release date.");
        }

        var result = await managerSession.League.MakePickupBidAsync(new PickupBidRequest
        {
            PublisherID = mgrPubID,
            MasterGameID = nullMaxReleaseGame!.MasterGame.MasterGameID,
            CounterPick = false,
            BidAmount = 5,
            AllowIneligibleSlot = false,
        });

        Assert.That(result.Success, Is.False,
            "Bid on a game with null MaximumReleaseDate must be rejected.");
        Assert.That(result.Errors, Has.Some.Contains("only allows bids"),
            "The error message must mention the restriction.");
    }
}
```

> **Note:** The integration tests above use some helper patterns (`LeagueFixtureManual`, `MockedLivePlayer.DraftToCompletionAsync`) that may require small adjustments based on what draft helpers are available in the test infrastructure. If `MockedLivePlayer` does not expose `DraftToCompletionAsync`, use `LeagueFixtureBuilder.CreateAndStartDraftAsync` / `DraftToCompletionAsync` on a `LeagueFixture` with the same scenario, then query the running league by ID for the bid step. Look at `BidProcessingTests.cs` for the established pattern.

- [ ] **Run the integration tests:**

  ```powershell
  dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj --filter "BidsOnlyBeforeNextDraft"
  ```
  Expected: both tests pass (or `Inconclusive` for the null-MaximumReleaseDate case if the fixture lacks such a game).

- [ ] **Commit:**

  ```
  git add src/FantasyCritic.IntegrationTests/Tests/League/Actions/BidsOnlyBeforeNextDraftTests.cs
  git commit -m "Add BidsOnlyBeforeNextScheduledDraft integration tests."
  ```

---

## Self-Review Checklist

### Spec coverage

| Spec requirement | Covered by |
|---|---|
| New `BidsOnlyBeforeNextScheduledDraft` bool option | Tasks 1–2 |
| Default `false`; existing leagues unaffected | Task 1 (DB migration DEFAULT 0) |
| Hard fail at bid placement | Task 3 (eligibility check via `MakePickupBid` → `CanClaimGame`) |
| Re-checked at bid processing time | Task 3 (same check in `ActionProcessor` → `CanClaimGame`) |
| Null `MaximumReleaseDate` → block | Task 3 (unit test U4, integration test I2) |
| Pending draft with no scheduled date → block | Task 3 (unit test U5) |
| No pending draft → no restriction | Task 3 (unit test U6) |
| `dropping = true` → exempt | Task 3 (unit test U8) |
| `drafting = true` → exempt | Task 3 (unit test U9) |
| UI checkbox, multi-draft + bids-enabled only | Task 4 |
| UI validation: require draft dates when option on | Task 5 |
| `BidsOnlyBeforeNextScheduledDraft` in `LeagueScenario.BuildSettings` | Task 4 |
| DB migration: add column + drop default | Task 1 |
| Unit tests (U1–U9) | Task 3 |
| Integration tests (I1, I2) | Task 6 |
