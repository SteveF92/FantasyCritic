# Per-Draft Status Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove `CompleteFirstDraftPlayStatus` / `PlayStatusViewModel` and instead expose all draft-readiness data directly on `LeagueDraftViewModel`, with three frontend mixin helpers (`firstDraft`, `pendingDraft`, `activeDraft`) mirroring the backend domain concepts.

**Architecture:** `LeagueDraftViewModel` gains computed readiness fields (only pending drafts compute `StartDraftErrors`/`ReadyToDraft`/`ReadyToSetDraftOrder`; all drafts get PlayStatus convenience bools). `LeagueYearViewModel` and `ConsolidatedLeagueYearViewModel` drop the `PlayStatus` property and get enriched `Drafts`. Frontend mixin replaces `playStatus.*` reads with `firstDraft`, `pendingDraft`, `activeDraft` computed properties.

**Tech Stack:** C# / ASP.NET Core (backend), Vue 2 / Vuex / JavaScript (frontend)

---

### Task 1: Enrich `LeagueDraftViewModel`

**Files:**
- Modify: `src/FantasyCritic.Web/Models/Requests/League/LeagueDraftViewModel.cs`

- [ ] **Step 1: Replace the file contents**

Replace the entire file with the following:

```csharp
using FantasyCritic.Lib.Domain.Draft;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Web.Models.RoundTrip;

namespace FantasyCritic.Web.Models.Responses;

public class LeagueDraftViewModel
{
    public LeagueDraftViewModel(LeagueDraft domain, LeagueYear leagueYear, IEnumerable<FantasyCriticUser> activeUsers, bool isManager, bool conferenceDraftsNotEnabled)
    {
        DraftID = domain.DraftID;
        DraftNumber = domain.DraftNumber;
        Name = domain.Name;
        ScheduledDate = domain.ScheduledDate;
        GamesToDraft = domain.GamesToDraft;
        CounterPicksToDraft = domain.CounterPicksToDraft;
        PlayStatus = domain.PlayStatus.Value;
        DraftStartedTimestamp = domain.DraftStartedTimestamp;
        DraftOrderSet = domain.DraftOrderSet;
        PublisherDraftInfo = domain.PublisherDraftInfo.Select(x => new PublisherDraftInfoViewModel(x)).ToList();

        PlayStarted = domain.PlayStatus.PlayStarted;
        DraftIsActive = domain.PlayStatus.DraftIsActive;
        DraftIsPaused = domain.PlayStatus.DraftIsPaused;
        DraftFinished = domain.PlayStatus.DraftFinished;

        if (domain.PlayStatus.DraftIsActiveOrPaused)
        {
            var draftStatus = DraftFunctions.GetDraftStatus(leagueYear);
            DraftingCounterPicks = DraftPhase.CounterPicks.Equals(draftStatus?.DraftPhase);
        }

        if (!domain.PlayStatus.PlayStarted)
        {
            ReadyToSetDraftOrder = DraftFunctions.LeagueIsReadyToSetDraftOrder(leagueYear.Publishers, activeUsers);
            StartDraftErrors = DraftFunctions.GetStartDraftResult(leagueYear, domain, activeUsers, isManager, conferenceDraftsNotEnabled);
            ReadyToDraft = !StartDraftErrors.Any();
        }
        else
        {
            StartDraftErrors = [];
        }
    }

    public Guid DraftID { get; }
    public int DraftNumber { get; }
    public string Name { get; }
    public LocalDate? ScheduledDate { get; }
    public int GamesToDraft { get; }
    public int CounterPicksToDraft { get; }
    public string PlayStatus { get; }
    public Instant? DraftStartedTimestamp { get; }
    public bool DraftOrderSet { get; }
    public List<PublisherDraftInfoViewModel> PublisherDraftInfo { get; }
    public bool PlayStarted { get; }
    public bool DraftIsActive { get; }
    public bool DraftIsPaused { get; }
    public bool DraftFinished { get; }
    public bool DraftingCounterPicks { get; }
    public bool ReadyToSetDraftOrder { get; }
    public IReadOnlyList<string> StartDraftErrors { get; }
    public bool ReadyToDraft { get; }
}
```

- [ ] **Step 2: Verify build**

```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj 2>&1 | Select-String "error CS"
```

Expected: errors on `LeagueYearViewModel` (still passing old constructor), not on `LeagueDraftViewModel` itself.

---

### Task 2: Update `LeagueYearViewModel`

**Files:**
- Modify: `src/FantasyCritic.Web/Models/Responses/LeagueYearViewModel.cs`

- [ ] **Step 1: Update the constructor signature**

Change the constructor signature from:
```csharp
public LeagueYearViewModel(LeagueViewModel leagueViewModel, LeagueYear leagueYear, Instant currentInstant, IReadOnlyList<MinimalFantasyCriticUser> activeUsers,
    CompleteFirstDraftPlayStatus completePlayStatus, IEnumerable<LeagueInvite> invitedPlayers, bool userIsInLeague, bool userIsInvitedToLeague, bool userIsManager,
```

To:
```csharp
public LeagueYearViewModel(LeagueViewModel leagueViewModel, LeagueYear leagueYear, Instant currentInstant, IReadOnlyList<MinimalFantasyCriticUser> activeUsers,
    IEnumerable<FantasyCriticUser> fullActiveUsers, bool conferenceDraftsNotEnabled, IEnumerable<LeagueInvite> invitedPlayers, bool userIsInLeague, bool userIsInvitedToLeague, bool userIsManager,
```

- [ ] **Step 2: Fix the `Publishers` construction (uses `NextDraftPublisher`)**

Change line ~31 from:
```csharp
        Publishers = leagueYear.Publishers
            .OrderBy(x => x.FirstDraftInfo.DraftPosition)
            .Select(x => new PublisherViewModel(leagueYear, x, currentDate, completePlayStatus.DraftStatus?.NextDraftPublisher, userIsInLeague, userIsInvitedToLeague, supplementalData.SystemWideValues, counterPickedByDictionary))
            .ToList();
```

To:
```csharp
        var activeDraftNextPublisher = DraftFunctions.GetDraftStatus(leagueYear)?.NextDraftPublisher;
        Publishers = leagueYear.Publishers
            .OrderBy(x => x.FirstDraftInfo.DraftPosition)
            .Select(x => new PublisherViewModel(leagueYear, x, currentDate, activeDraftNextPublisher, userIsInLeague, userIsInvitedToLeague, supplementalData.SystemWideValues, counterPickedByDictionary))
            .ToList();
```

- [ ] **Step 3: Update `Drafts` construction and remove `PlayStatus`**

Change line ~104-108:
```csharp
        PlayStatus = new PlayStatusViewModel(completePlayStatus);
        EligibilityOverrides = leagueYear.EligibilityOverrides.Select(x => new EligibilityOverrideViewModel(x, currentDate)).ToList();
        TagOverrides = leagueYear.TagOverrides.Select(x => new TagOverrideViewModel(x, currentDate)).ToList();
        SlotInfo = new PublisherSlotRequirementsViewModel(leagueYear.Options);
        Drafts = leagueYear.Drafts.Select(d => new LeagueDraftViewModel(d)).ToList();
```

To:
```csharp
        EligibilityOverrides = leagueYear.EligibilityOverrides.Select(x => new EligibilityOverrideViewModel(x, currentDate)).ToList();
        TagOverrides = leagueYear.TagOverrides.Select(x => new TagOverrideViewModel(x, currentDate)).ToList();
        SlotInfo = new PublisherSlotRequirementsViewModel(leagueYear.Options);
        Drafts = leagueYear.Drafts.Select(d => new LeagueDraftViewModel(d, leagueYear, fullActiveUsers, userIsManager, conferenceDraftsNotEnabled)).ToList();
```

- [ ] **Step 4: Remove the `PlayStatus` property from the properties section**

Remove this line from the properties block (~line 156):
```csharp
    public PlayStatusViewModel PlayStatus { get; }
```

- [ ] **Step 5: Verify build**

```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj 2>&1 | Select-String "error CS"
```

Expected: errors on `LeagueController` call sites (wrong number of constructor args), not on `LeagueYearViewModel` itself.

---

### Task 3: Update `ConsolidatedLeagueYearViewModel`

**Files:**
- Modify: `src/FantasyCritic.Web/Models/Responses/ConsolidatedLeagueDataViewModel.cs`

- [ ] **Step 1: Replace the `CompleteFirstDraftPlayStatus` block with direct calls**

In `ConsolidatedLeagueYearViewModel`'s constructor, change lines ~51-60 from:
```csharp
        List<FantasyCriticUser> activePublisherUsers = leagueYear.Publishers.Select(x => x.User).ToList();
        List<MinimalFantasyCriticUser> activeUsersMinimal = activePublisherUsers.Select(x => x.ToMinimal()).ToList();
        bool conferenceDraftsNotEnabled = leagueYear.ConferenceLocked.HasValue && !leagueYear.ConferenceLocked.Value;
        CompleteFirstDraftPlayStatus completePlayStatus = new CompleteFirstDraftPlayStatus(leagueYear, activePublisherUsers, false, conferenceDraftsNotEnabled);

        Publishers = leagueYear.Publishers
            .OrderBy(x => x.GetDraftPosition(leagueYear.DraftForPublisherDisplayOrder.DraftID))
            .Select(x => new PublisherViewModel(leagueYear, x, currentDate, completePlayStatus.DraftStatus?.NextDraftPublisher,
                userIsInLeague: false, outstandingInvite: false, systemWideValues, counterPickedByDictionary))
            .ToList();
```

To:
```csharp
        List<FantasyCriticUser> activePublisherUsers = leagueYear.Publishers.Select(x => x.User).ToList();
        List<MinimalFantasyCriticUser> activeUsersMinimal = activePublisherUsers.Select(x => x.ToMinimal()).ToList();
        bool conferenceDraftsNotEnabled = leagueYear.ConferenceLocked.HasValue && !leagueYear.ConferenceLocked.Value;
        var activeDraftNextPublisher = DraftFunctions.GetDraftStatus(leagueYear)?.NextDraftPublisher;

        Publishers = leagueYear.Publishers
            .OrderBy(x => x.GetDraftPosition(leagueYear.DraftForPublisherDisplayOrder.DraftID))
            .Select(x => new PublisherViewModel(leagueYear, x, currentDate, activeDraftNextPublisher,
                userIsInLeague: false, outstandingInvite: false, systemWideValues, counterPickedByDictionary))
            .ToList();
```

- [ ] **Step 2: Replace `Drafts` construction and remove `PlayStatus`**

Change line ~98-99:
```csharp
        PlayStatus = new PlayStatusViewModel(completePlayStatus);
        EligibilityOverrides = leagueYear.EligibilityOverrides.Select(x => new EligibilityOverrideViewModel(x, currentDate)).ToList();
```

To:
```csharp
        Drafts = leagueYear.Drafts.Select(d => new LeagueDraftViewModel(d, leagueYear, activePublisherUsers, isManager: false, conferenceDraftsNotEnabled)).ToList();
        EligibilityOverrides = leagueYear.EligibilityOverrides.Select(x => new EligibilityOverrideViewModel(x, currentDate)).ToList();
```

- [ ] **Step 3: Update the properties section**

Remove from the properties block:
```csharp
    public PlayStatusViewModel PlayStatus { get; }
```

Add in the properties block (alongside existing `Publishers`, etc.):
```csharp
    public IReadOnlyList<LeagueDraftViewModel> Drafts { get; }
```

- [ ] **Step 4: Add required using**

Add at the top of the file if not already present:
```csharp
using FantasyCritic.Lib.Domain.Draft;
```

- [ ] **Step 5: Verify build**

```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj 2>&1 | Select-String "error CS"
```

Expected: errors only on `LeagueController` call sites.

---

### Task 4: Fix `LeagueController` call sites

**Files:**
- Modify: `src/FantasyCritic.Web/Controllers/API/LeagueController.cs`

There are two identical call sites (around lines 201-208 and 295-302). Apply the same change to both.

- [ ] **Step 1: Fix first call site (~line 201)**

Change from:
```csharp
        var completePlayStatus = new CompleteFirstDraftPlayStatus(leagueYear, validResult.ActiveUsers, relationship.LeagueManager, conferenceDraftsNotEnabled);
        var activeUsers = validResult.ActiveUsers.Select(x => x.ToMinimal()).ToList();

        var leagueViewModel = new LeagueViewModel(league, relationship.LeagueManager, validResult.PlayersInLeague,
```

To:
```csharp
        var activeUsers = validResult.ActiveUsers.Select(x => x.ToMinimal()).ToList();

        var leagueViewModel = new LeagueViewModel(league, relationship.LeagueManager, validResult.PlayersInLeague,
```

Then find the `LeagueYearViewModel` constructor call (a few lines below) and change from:
```csharp
        var leagueYearViewModel = new LeagueYearViewModel(leagueViewModel, leagueYear, currentInstant, activeUsers,
            completePlayStatus, invitedPlayers,
```

To:
```csharp
        var leagueYearViewModel = new LeagueYearViewModel(leagueViewModel, leagueYear, currentInstant, activeUsers,
            validResult.ActiveUsers, conferenceDraftsNotEnabled, invitedPlayers,
```

- [ ] **Step 2: Fix second call site (~line 295) — identical change**

Apply the exact same two edits to the second call site.

- [ ] **Step 3: Verify build**

```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj 2>&1 | Select-String "error CS"
```

Expected: errors only on `LeagueManagerController` (still uses `CompleteFirstDraftPlayStatus`).

---

### Task 5: Fix `LeagueManagerController` guard checks

**Files:**
- Modify: `src/FantasyCritic.Web/Controllers/API/LeagueManagerController.cs`

- [ ] **Step 1: Fix `StartDraft` guard (~line 844)**

Change from:
```csharp
        var activeUsers = await _leagueMemberService.GetActivePlayersForLeagueYear(leagueYear.League, request.Year);
        var completePlayStatus = new CompleteFirstDraftPlayStatus(leagueYear, activeUsers, validResult.Relationship.LeagueManager, false);
        if (!completePlayStatus.ReadyToDraft)
        {
            return BadRequest();
        }
```

To:
```csharp
        var activeUsers = await _leagueMemberService.GetActivePlayersForLeagueYear(leagueYear.League, request.Year);
        var pendingDraft = leagueYear.PendingDraft;
        if (pendingDraft is null)
        {
            return BadRequest();
        }
        var startDraftErrors = DraftFunctions.GetStartDraftResult(leagueYear, pendingDraft, activeUsers, validResult.Relationship.LeagueManager, false);
        if (startDraftErrors.Any())
        {
            return BadRequest();
        }
```

- [ ] **Step 2: Fix `SetDraftOrder` guard (~line 904)**

Change from:
```csharp
        var activeUsers = await _leagueMemberService.GetActivePlayersForLeagueYear(leagueYear.League, request.Year);

        bool conferenceDraftsNotEnabled = leagueYear.ConferenceLocked.HasValue && !leagueYear.ConferenceLocked.Value;
        var completePlayStatus = new CompleteFirstDraftPlayStatus(leagueYear, activeUsers, validResult.Relationship.LeagueManager, conferenceDraftsNotEnabled);
        if (!completePlayStatus.ReadyToSetDraftOrder)
        {
            return BadRequest();
        }
```

To:
```csharp
        var activeUsers = await _leagueMemberService.GetActivePlayersForLeagueYear(leagueYear.League, request.Year);

        if (!DraftFunctions.LeagueIsReadyToSetDraftOrder(leagueYear.Publishers, activeUsers))
        {
            return BadRequest();
        }
```

- [ ] **Step 3: Verify build**

```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj 2>&1 | Select-String "error CS"
```

Expected: clean build (zero errors).

---

### Task 6: Delete `CompleteFirstDraftPlayStatus` and `PlayStatusViewModel`

**Files:**
- Delete: `src/FantasyCritic.Lib/Domain/Draft/CompleteFirstDraftPlayStatus.cs`
- Delete: `src/FantasyCritic.Web/Models/Responses/PlayStatusViewModel.cs`

- [ ] **Step 1: Delete both files**

```powershell
Remove-Item "src/FantasyCritic.Lib/Domain/Draft/CompleteFirstDraftPlayStatus.cs"
Remove-Item "src/FantasyCritic.Web/Models/Responses/PlayStatusViewModel.cs"
```

- [ ] **Step 2: Full solution build**

```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj 2>&1 | Select-String "error CS|Build succeeded"
```

Expected: `Build succeeded` with `0 Error(s)`.

- [ ] **Step 3: Commit backend work**

```powershell
git add -A
git commit -m "Replace CompleteFirstDraftPlayStatus with per-draft status on LeagueDraftViewModel."
```

---

### Task 7: Update `leagueMixin.js`

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/mixins/leagueMixin.js`

This task adds the three draft helper computeds and migrates all existing `playStatus.*` reads in the mixin.

- [ ] **Step 1: Add the three draft helper computed properties**

Add the following three computed properties after `supportedYear()` and before `nextPublisherUp()`:

```js
    firstDraft() {
      return this.leagueYear?.drafts?.[0] ?? null;
    },
    pendingDraft() {
      return this.leagueYear?.drafts?.find((d) => d.playStatus === 'NotStartedDraft') ?? null;
    },
    activeDraft() {
      return this.leagueYear?.drafts?.find((d) => d.draftIsActive || d.draftIsPaused) ?? null;
    },
```

- [ ] **Step 2: Migrate all `playStatus.*` reads in the existing computed properties**

Replace the six affected computed properties:

```js
    draftIsPaused() {
      return this.activeDraft?.draftIsPaused ?? false;
    },
    // ...
    playStarted() {
      return this.firstDraft?.playStarted ?? false;
    },
    readyToSetDraftOrder() {
      return this.pendingDraft?.readyToSetDraftOrder ?? false;
    },
    draftFinished() {
      return this.firstDraft?.draftFinished ?? false;
    },
    postDraftPlayable() {
      return (this.firstDraft?.draftFinished ?? false) && !this.leagueYear.supportedYear.finished;
    },
    postDraftEditable() {
      return (this.firstDraft?.draftFinished ?? false) && (!this.leagueYear.supportedYear.finished || this.leagueYear.underReview);
    },
```

---

### Task 8: Update `league.vue`

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/views/league.vue`

All of these are replacements in the `<template>` or `<script>` sections. The `firstDraft`, `pendingDraft`, and `activeDraft` helpers come from `LeagueMixin` which this component already uses.

- [ ] **Step 1: Fix invite code banner (line 71)**

```html
<!-- BEFORE -->
<div v-if="inviteCode && !league.userIsInLeague && !leagueYear.playStatus.playStarted" class="alert alert-secondary">
<!-- AFTER -->
<div v-if="inviteCode && !league.userIsInLeague && !firstDraft?.playStarted" class="alert alert-secondary">
```

- [ ] **Step 2: Fix year-finished banner (line 105)**

```html
<!-- BEFORE -->
<div v-if="leagueYear.playStatus.playStarted && leagueYear.supportedYear.finished">
<!-- AFTER -->
<div v-if="firstDraft?.playStarted && leagueYear.supportedYear.finished">
```

- [ ] **Step 3: Fix "year not active" warning (lines 120-123)**

```html
<!-- BEFORE -->
<div v-if="(leagueYear.userIsActive || league.isManager) && !leagueYear.playStatus.readyToDraft" class="alert alert-warning">
  <h2>This year is not active yet!</h2>
  <ul>
    <li v-for="error in leagueYear.playStatus.startDraftErrors" :key="error">{{ error }}</li>
  </ul>
<!-- AFTER -->
<div v-if="(leagueYear.userIsActive || league.isManager) && !pendingDraft?.readyToDraft" class="alert alert-warning">
  <h2>This year is not active yet!</h2>
  <ul>
    <li v-for="error in (pendingDraft?.startDraftErrors ?? [])" :key="error">{{ error }}</li>
  </ul>
```

- [ ] **Step 4: Fix manager-without-playing banner (line 139)**

```html
<!-- BEFORE -->
<div v-if="league.isManager && !leagueYear.playStatus.playStarted && !leagueYear.userIsActive" class="alert alert-info">
<!-- AFTER -->
<div v-if="league.isManager && !firstDraft?.playStarted && !leagueYear.userIsActive" class="alert alert-info">
```

- [ ] **Step 5: Fix "ready to draft" banner (line 141)**

```html
<!-- BEFORE -->
<div v-if="!leagueYear.playStatus.playStarted && leagueYear.playStatus.readyToDraft && !league.outstandingInvite">
<!-- AFTER -->
<div v-if="!firstDraft?.playStarted && pendingDraft?.readyToDraft && !league.outstandingInvite">
```

- [ ] **Step 6: Fix draft-paused banner (line 153)**

```html
<!-- BEFORE -->
<div v-if="leagueYear.playStatus.draftIsPaused">
<!-- AFTER -->
<div v-if="activeDraft?.draftIsPaused">
```

- [ ] **Step 7: Fix active-draft banner (lines 162-188)**

```html
<!-- BEFORE -->
<div v-if="leagueYear.playStatus.draftIsActive && nextPublisherUp">
  <div v-if="!userIsNextInDraft">
    <div class="alert alert-info">
      <div v-show="!leagueYear.playStatus.draftingCounterPicks">The draft is currently in progress!</div>
      <div v-show="leagueYear.playStatus.draftingCounterPicks">It's time to draft Counter Picks!</div>
      ...
  <div v-else>
    <div class="alert alert-success draft-header">
      <div>
        <div v-show="!leagueYear.playStatus.draftingCounterPicks">The draft is currently in progress!</div>
        <div v-show="leagueYear.playStatus.draftingCounterPicks">It's time to draft counter picks!</div>
        ...
      <div v-if="!leagueYear.playStatus.draftingCounterPicks">
<!-- AFTER -->
<div v-if="activeDraft?.draftIsActive && nextPublisherUp">
  <div v-if="!userIsNextInDraft">
    <div class="alert alert-info">
      <div v-show="!activeDraft?.draftingCounterPicks">The draft is currently in progress!</div>
      <div v-show="activeDraft?.draftingCounterPicks">It's time to draft Counter Picks!</div>
      ...
  <div v-else>
    <div class="alert alert-success draft-header">
      <div>
        <div v-show="!activeDraft?.draftingCounterPicks">The draft is currently in progress!</div>
        <div v-show="activeDraft?.draftingCounterPicks">It's time to draft counter picks!</div>
        ...
      <div v-if="!activeDraft?.draftingCounterPicks">
```

- [ ] **Step 8: Fix post-draft game news (line 197)**

```html
<!-- BEFORE -->
<div v-if="leagueYear.playStatus.draftFinished && !leagueYear.supportedYear.finished">
<!-- AFTER -->
<div v-if="firstDraft?.draftFinished && !leagueYear.supportedYear.finished">
```

- [ ] **Step 9: Fix `mustSetDraftOrder` computed (line 297)**

```js
// BEFORE
mustSetDraftOrder() {
  return this.leagueYear.playStatus.readyToSetDraftOrder && this.leagueYear.playStatus.startDraftErrors.includes('You must set the draft order.');
},
// AFTER
mustSetDraftOrder() {
  return (this.pendingDraft?.readyToSetDraftOrder ?? false) && (this.pendingDraft?.startDraftErrors ?? []).includes('You must set the draft order.');
},
```

- [ ] **Step 10: Fix `startHubConnection` method (line 425)**

```js
// BEFORE
if (!this.leagueYear || !this.leagueYear.playStatus.playStarted || this.leagueYear.playStatus.draftFinished) {
// AFTER
if (!this.leagueYear || !this.firstDraft?.playStarted || this.firstDraft?.draftFinished) {
```

---

### Task 9: Update `leagueActions.vue`

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/components/leagueActions.vue`

- [ ] **Step 1: Fix player draft-game action (line 48)**

```html
<!-- BEFORE -->
<li v-show="leagueYear.playStatus.draftIsActive && !leagueYear.playStatus.draftingCounterPicks && userIsNextInDraft" v-b-modal="'playerDraftGameForm'" class="fake-link action">
<!-- AFTER -->
<li v-show="activeDraft?.draftIsActive && !activeDraft?.draftingCounterPicks && userIsNextInDraft" v-b-modal="'playerDraftGameForm'" class="fake-link action">
```

- [ ] **Step 2: Fix player draft-counter-pick action (line 51)**

```html
<!-- BEFORE -->
<li v-show="leagueYear.playStatus.draftIsActive && leagueYear.playStatus.draftingCounterPicks && userIsNextInDraft" v-b-modal="'playerDraftCounterPickForm'" class="fake-link action">
<!-- AFTER -->
<li v-show="activeDraft?.draftIsActive && activeDraft?.draftingCounterPicks && userIsNextInDraft" v-b-modal="'playerDraftCounterPickForm'" class="fake-link action">
```

- [ ] **Step 3: Fix draft management section header (line 78)**

```html
<!-- BEFORE -->
<div v-if="leagueYear.playStatus.draftIsActive || leagueYear.playStatus.draftIsPaused">
<!-- AFTER -->
<div v-if="activeDraft?.draftIsActive || activeDraft?.draftIsPaused">
```

- [ ] **Step 4: Fix manager draft-game action (line 81)**

```html
<!-- BEFORE -->
<li v-show="!leagueYear.playStatus.draftingCounterPicks && leagueYear.playStatus.draftIsActive" v-b-modal="'managerDraftGameForm'" class="fake-link action">
<!-- AFTER -->
<li v-show="!activeDraft?.draftingCounterPicks && activeDraft?.draftIsActive" v-b-modal="'managerDraftGameForm'" class="fake-link action">
```

- [ ] **Step 5: Fix manager draft-counter-pick action (line 82)**

```html
<!-- BEFORE -->
<li v-show="leagueYear.playStatus.draftingCounterPicks && leagueYear.playStatus.draftIsActive" v-b-modal="'managerDraftCounterPickForm'" class="fake-link action">
<!-- AFTER -->
<li v-show="activeDraft?.draftingCounterPicks && activeDraft?.draftIsActive" v-b-modal="'managerDraftCounterPickForm'" class="fake-link action">
```

- [ ] **Step 6: Fix pause/resume button labels (lines 86-87)**

```html
<!-- BEFORE -->
<span v-show="leagueYear.playStatus.draftIsActive">Pause Draft</span>
<span v-show="leagueYear.playStatus.draftIsPaused">Resume Draft</span>
<!-- AFTER -->
<span v-show="activeDraft?.draftIsActive">Pause Draft</span>
<span v-show="activeDraft?.draftIsPaused">Resume Draft</span>
```

- [ ] **Step 7: Fix undo action visibility (lines 90-91)**

```html
<!-- BEFORE -->
<li v-show="leagueYear.playStatus.draftIsPaused" v-b-modal="'undoLastDraftActionModal'" class="fake-link action">Undo Last Drafted Game</li>
<li v-show="!leagueYear.playStatus.draftIsPaused">
<!-- AFTER -->
<li v-show="activeDraft?.draftIsPaused" v-b-modal="'undoLastDraftActionModal'" class="fake-link action">Undo Last Drafted Game</li>
<li v-show="!activeDraft?.draftIsPaused">
```

---

### Task 10: Update `leagueYearStandings.vue` and `removePlayerModal.vue`

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/components/leagueYearStandings.vue`
- Modify: `src/FantasyCritic.Web/ClientApp/src/components/modals/removePlayerModal.vue`

Both files use `LeagueMixin` so `firstDraft` is available.

- [ ] **Step 1: Fix `leagueYearStandings.vue` template (lines 14-15)**

```html
<!-- BEFORE -->
<span v-show="!leagueYear.playStatus.draftFinished && data.item.publisher.autoDraftMode === 'All'" ...>Auto Draft</span>
<span v-show="!leagueYear.playStatus.draftFinished && data.item.publisher.autoDraftMode === 'StandardGamesOnly'" ...>
<!-- AFTER -->
<span v-show="!firstDraft?.draftFinished && data.item.publisher.autoDraftMode === 'All'" ...>Auto Draft</span>
<span v-show="!firstDraft?.draftFinished && data.item.publisher.autoDraftMode === 'StandardGamesOnly'" ...>
```

- [ ] **Step 2: Fix `leagueYearStandings.vue` script (lines 83 and 90)**

```js
// BEFORE (line 83):
if (!this.leagueYear.playStatus.draftFinished) {
// AFTER:
if (!this.firstDraft?.draftFinished) {

// BEFORE (line 90):
if (!this.leagueYear.playStatus.draftFinished) {
// AFTER:
if (!this.firstDraft?.draftFinished) {
```

- [ ] **Step 3: Fix `removePlayerModal.vue` template (lines 24, 29, 41)**

```html
<!-- BEFORE (line 24) -->
<div v-show="!leagueYear.playStatus.playStarted && !playerToRemove.removable" class="alert alert-info">
<!-- AFTER -->
<div v-show="!firstDraft?.playStarted && !playerToRemove.removable" class="alert alert-info">

<!-- BEFORE (line 29) -->
<template v-if="leagueYear.playStatus.playStarted">
<!-- AFTER -->
<template v-if="firstDraft?.playStarted">

<!-- BEFORE (line 41) -->
<div v-if="!leagueYear.playStatus.playStarted" class="alert alert-info">
<!-- AFTER -->
<div v-if="!firstDraft?.playStarted" class="alert alert-info">
```

- [ ] **Step 4: Fix `removePlayerModal.vue` script (line 93)**

```js
// BEFORE
if (this.leagueYear.playStatus.playStarted) {
// AFTER
if (this.firstDraft?.playStarted) {
```

- [ ] **Step 5: Final commit**

```powershell
git add -A
git commit -m "Migrate frontend from playStatus blob to firstDraft/pendingDraft/activeDraft helpers."
```
