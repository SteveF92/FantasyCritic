# Design: Multi-Draft League UI

**Date:** 2026-06-25  
**Status:** Approved  
**Branch:** `multi-draft-leagues`  
**Context:** Slice 3 of [multi-draft-leagues-design](2026-06-17-multi-draft-leagues-design.md). Per-draft status (the previous mini-spec) is already implemented. The backend APIs (`CreateLeagueDraft`, `EditLeagueDraft`, `DeleteLeagueDraft`, `SetDraftOrder` with `draftID`, `ResetDraft` with `draftID`) are already implemented. The home-page league table already shows the multi-draft icon. This spec covers the remaining frontend work needed to add, edit, and run multiple drafts.

---

## Scope

What's in:
- Manage Drafts page (add / edit / delete drafts; works for single-draft leagues too)
- `draftReadinessBanner` component (replaces scattered inline blocks in `league.vue`)
- Draft Schedule modal (multi-draft only, all-player read-only view)
- `leagueActions` sidebar additions
- `leagueYearSettings` / `editLeague` changes for multi-draft
- Running the second draft (SignalR fix, DraftFinished modal, draftID wiring)

What's out:
- Create League / Add New League Year multi-draft preset flow (future slice)
- Auto-skip edge cases

---

## Section 1: Manage Drafts Page

### Route and file

| Item | Value |
|---|---|
| Path | `/manageDrafts/:leagueid/:year` |
| Route name | `manageDrafts` |
| View file | `src/.../views/manageDrafts.vue` |
| Access | Commissioner-only (enforce in template and link) |

Link to this page is added in `leagueActions.vue` under "Manage League > League Management" as **"Manage Drafts"**. It is always shown to the commissioner (no `playStarted` gate), as long as the year is not finished. The page works for single-draft leagues too — the commissioner can edit the first draft's name and scheduled date there.

### Page layout

The page lists all existing drafts as Bootstrap cards, ordered by `DraftNumber`. Each card displays:

| Field | Notes |
|---|---|
| Draft # | Badge, e.g. `Draft 1` |
| Status | Badge: `Not Started` / `In Progress` / `Paused` / `Completed` |
| Name | Plain text when not editing |
| Scheduled Date | Formatted date or "—" if unset |
| Games to Draft | Number |
| Counter Picks to Draft | Number |

**Card actions:**

- **Edit** button → expands an inline form within the card. `Name` and `ScheduledDate` are always editable. `GamesToDraft` and `CounterPicksToDraft` are disabled (grayed out, not hidden) once the draft has started (`playStatus !== 'NotStartedDraft'`), so the commissioner can see the values but cannot change them.
- **Delete** button → only rendered when `draftNumber > 1 && playStatus === 'NotStartedDraft'`. Clicking shows an inline confirmation ("Are you sure? This cannot be undone.") with a confirm/cancel pair before submitting.

**"Add Another Draft" section:**

Rendered below all draft cards when no draft is currently in progress (`activeDraft == null`). Uses a single form with these fields:

| Field | Required | Notes |
|---|---|---|
| Name | Yes | Text input |
| Scheduled Date | No | Date picker |
| Games to Draft | Yes | Number, ≥ 1 |
| Counter Picks to Draft | No | Number, ≥ 0 |
| Additional Standard Games | No | Number, ≥ 0; default 0; expands total roster slots |
| Additional Counter Picks | No | Number, ≥ 0; default 0 |
| New Special Slots | No | Reuse the existing special-slot builder component from `createLeague.vue`; defaults to empty |

The section is hidden (not just disabled) while a draft is in progress, with an explanatory note: *"You cannot add a draft while one is in progress."*

### API calls

| Action | Endpoint | Payload |
|---|---|---|
| Edit draft | `POST /api/leagueManager/EditLeagueDraft` | `{ draftID, leagueID, year, name, scheduledDate, gamesToDraft, counterPicksToDraft }` |
| Delete draft | `POST /api/leagueManager/DeleteLeagueDraft` | `{ draftID, leagueID, year }` |
| Add draft | `POST /api/leagueManager/CreateLeagueDraft` | `{ leagueID, year, name, scheduledDate, gamesToDraft, counterPicksToDraft, additionalStandardGames, additionalCounterPicks, newSpecialGameSlots }` |

All actions call `notifyAction` on success to refresh the page data.

---

## Section 2: League Home Page — Draft Readiness + Sidebar

### New component: `draftReadinessBanner.vue`

**File:** `src/.../components/draftReadinessBanner.vue`

Replaces the two inline draft-readiness blocks currently in `league.vue` (lines 120–151). Uses `LeagueMixin`. Renders nothing when `pendingDraft` is null.

The component renders a single banner whose content depends on the combination of states below. States 1 and 2 can overlap (a draft can be both not-ready AND imminent/scheduled).

**State 1 — Not ready to start** (`!pendingDraft.readyToDraft`)

Alert variant: `warning`.

- Header text varies by draft number:
  - `draftNumber === 1`: *"This year is not active yet!"*
  - `draftNumber > 1`: *"Your next draft — **[Name]** — isn't ready to start yet."*
- Error list: renders each item in `pendingDraft.startDraftErrors`.
- If `mustSetDraftOrder` (the blocking error is "You must set the draft order"): shows a **Set Draft Order** button (`v-b-modal="'editDraftOrderForm'"`).
- For `draftNumber > 1`, commissioner also sees a router-link to the Manage Drafts page.
- Non-managers see the errors as informational text only, no action buttons.
- Existing note preserved: *"Please note that once you start the draft, you can no longer add/remove players."* (shown on first draft only)

**State 2 — Imminent / scheduled** (overlapping with State 1 or 3)

An additional line rendered inside the same banner when any of these are true:
- `pendingDraft.scheduledDate` is within 7 days of today (computed client-side)
- `pendingDraft.draftOrderSet === true`

Line text adapts:
- Order set, no near date → *"Draft order is set."*
- Near date, order not set → *"Scheduled for **[DATE]**."*
- Both → *"Scheduled for **[DATE]** — draft order is set."*

**State 3 — Ready to start** (`pendingDraft.readyToDraft`)

Alert variant: `success`.

- Commissioner:
  - `draftNumber === 1`: *"Things are all set to get started!"* + note about player finality + **Start Drafting!** button (triggers existing `startDraftModal` via `v-b-modal="'startDraft'"`)
  - `draftNumber > 1`: *"**[Name]** is ready to go!"* + **Start Drafting!** button
- Non-commissioner:
  - `draftNumber === 1`: *"Things are all set! Your league manager can choose when to begin the draft."*
  - `draftNumber > 1`: *"**[Name]** is ready to go! Your league manager can choose when to begin."*

**State 4 — No scheduled date set** (soft nudge)

When `pendingDraft.scheduledDate` is null AND State 2 is not active (i.e. the draft isn't already imminent via order-set), append a soft line inside the banner:
*"No scheduled date set — [set one on the Manage Drafts page] to help your players plan ahead."*
The bracketed text is a router-link (shown to all users). This nudge is suppressed when State 2 is active because scheduling context is already covered there.

### `leagueActions.vue` additions

**Under "Public Actions"** (visible to all users):  
- *"See Draft Schedule"* → `v-b-modal="'draftScheduleModal'"`. Only rendered when `leagueYear.drafts.length >= 2`.

**Under "Manage League > League Management"** (commissioner only):  
- *"Manage Drafts"* → router-link to `manageDrafts`. Shown when `!leagueYear.supportedYear.finished`.

### New modal: `draftScheduleModal.vue`

A read-only modal listing all drafts. Triggered by "See Draft Schedule" in the sidebar.

Displays a table with columns: `#`, `Name`, `Scheduled Date`, `Games / CPs`, `Status`. No edit actions — the Manage Drafts page is the editing surface. Shown only for multi-draft leagues (the triggering link is already gated on `drafts.length >= 2`).

---

## Section 3: EditLeague Page + LeagueYearSettings

### `leagueYearSettings.vue`

Gains a new Boolean prop: **`isMultiDraft`** (default `false`).

When `isMultiDraft` is true:
- `gamesToDraft` and `counterPicksToDraft` fields are hidden (not just disabled).
- A note replaces them: *"This league has multiple drafts. Visit the [Manage Drafts page] to configure draft settings."* The bracketed text is a router-link, but since `leagueYearSettings` is used in multiple contexts (create, edit, conference), the link target must be passed in or the note rendered without a link when the context doesn't have a league year yet. Simplest approach: pass an optional `manageDraftsRoute` object prop; render the note as a plain string when it's absent.

### `editLeague.vue`

- Fetches `leagueYear` (already does this).
- Computes `isMultiDraft = leagueYear.drafts.length >= 2`.
- Passes `isMultiDraft` and `manageDraftsRoute` to `<leagueYearSettings>`.
- No additional draft fields are added to `editLeague.vue` itself — all draft-specific settings (name, date, games/CPs) live on the Manage Drafts page.
- `postRequest` still calls only `EditLeagueYear` (no second API call).

### Create League / addNewLeagueYear

**Not touched in this slice.** Multi-draft preset flow is future work.

---

## Section 4: Running the Second Draft

### `startHubConnection` in `league.vue`

**Current condition (connects SignalR):**
```js
if (!this.leagueYear || !this.firstDraft.playStarted || this.firstDraft.draftFinished) {
  return;
}
```

**Revised condition:**
```js
if (!this.leagueYear || !this.activeDraft) {
  return;
}
```

This connects SignalR whenever any draft is actively running or paused, regardless of draft number, and disconnects between drafts when `activeDraft` is null.

### New component: `draftCompleteModal.vue`

**File:** `src/.../components/modals/draftCompleteModal.vue`

Replaces the inline `b-modal#draftFinishedModal` in `league.vue`. Uses `LeagueMixin`. Triggered the same way — `this.$refs.draftCompleteModalRef.show()` in the SignalR `DraftFinished` handler after the page refreshes.

Title: **"Draft Complete!"** (always).

The body text is determined by the combination of league type and draft position at the moment the modal is shown. `pendingDraft` reflects the post-refresh state, so it is null when the just-finished draft was the final one.

**Decision table** (`oneShotMode` and `enableBids` from `leagueYear.settings`; `isFinalDraft = pendingDraft === null`):

| League shape | `enableBids` | `isFinalDraft` | Body text |
|---|---|---|---|
| Single draft, not one-shot | `true` | — | *[Lorem ipsum — bids now open.]* |
| Single draft, not one-shot | `false` | — | *[Lorem ipsum — draft done, no bids.]* |
| One-shot (`oneShotMode`) | `false` | — | *[Lorem ipsum — year complete, locked.]* |
| Multi-draft | `true` | `false` | *[Lorem ipsum — draft N done, bids may open, next draft coming.]* |
| Multi-draft | `false` | `false` | *[Lorem ipsum — draft N done, no bids between drafts, next draft coming.]* |
| Multi-draft | `true` | `true` | *[Lorem ipsum — all drafts done, bids now open for rest of year.]* |
| Multi-draft | `false` | `true` | *[Lorem ipsum — all drafts done, year locked.]* |

One-shot + `enableBids = true` is not a valid combination and is not handled.

The `userIsInLeague` guard from the original modal is preserved: when the viewing user is not in the league, show a short fallback line (*"The draft is complete!"*) regardless of league shape.

### `editDraftOrderForm.vue`

The `SetDraftOrder` API call must include `draftID`. The form sources this from `pendingDraft.draftID` (already available via `LeagueMixin`). The server validates that the supplied `draftID` matches `PendingDraft`.

### `resetDraftModal.vue`

The `ResetDraft` API call must include `draftID`. The modal sources this from `activeDraft.draftID`. The server validates that the supplied `draftID` matches `ActiveDraft`.

---

## Files Created / Modified

| File | Change |
|---|---|
| `views/manageDrafts.vue` | **New** — Manage Drafts page |
| `components/draftReadinessBanner.vue` | **New** — replaces inline draft-readiness blocks |
| `components/modals/draftScheduleModal.vue` | **New** — read-only draft list modal |
| `router/routes.js` | Add `manageDrafts` route |
| `components/modals/draftCompleteModal.vue` | **New** — replaces inline `draftFinishedModal` with full league-shape-aware text |
| `views/league.vue` | Replace inline draft-readiness blocks with `<draftReadinessBanner>`; replace inline modal with `<draftCompleteModal>`; fix `startHubConnection`; register new components |
| `components/leagueActions.vue` | Add "See Draft Schedule" (public) + "Manage Drafts" link (commissioner) |
| `components/leagueYearSettings.vue` | Add `isMultiDraft` + `manageDraftsRoute` props; hide games/CPs fields when multi-draft |
| `views/editLeague.vue` | Compute `isMultiDraft`; pass props to `leagueYearSettings` |
| `components/modals/editDraftOrderForm.vue` | Include `pendingDraft.draftID` in request |
| `components/modals/resetDraftModal.vue` | Include `activeDraft.draftID` in request |
