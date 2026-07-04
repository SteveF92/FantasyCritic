# Create League as Multi Draft — Design Spec

**Date:** 2026-06-28  
**Status:** Approved for implementation  
**Branch:** `multi-draft-leagues`  
**Prerequisite:** Phase 2 Slice 1 (Draft CRUD endpoints) complete

---

## Overview

This spec covers two related improvements delivered together:

1. **Leagues can be created as Multi Draft from the outset.** The creation request accepts a `drafts[]` array (min 1 for Standard/One Shot, min 2 for Multi Draft), and all drafts are created atomically with the league year.

2. **The "Game Settings" introduction UI is redesigned.** The single combined dropdown (One Shot / Beginner / Standard / Advanced) is split into two orthogonal questions — **Game Mode** and **Experience Level** — backed by a dedicated preset system extracted into new, testable frontend files.

These two improvements share the same frontend refactor: `GamesToDraft` and `CounterPicksToDraft` move out of `LeagueYearSettingsViewModel` into a sibling `drafts[]` structure, cleaning up a coupling that was making the Multi Draft UI awkward to build.

---

## Key Design Decisions

### `GamesToDraft` / `CounterPicksToDraft` leave `LeagueYearSettingsViewModel`

These fields do not belong in the league-year settings object — they are per-draft settings. Moving them out:

- Makes `LeagueYearSettingsViewModel` a clean league-year-only shape
- Eliminates the "Draft 1 is special-cased inside settings, Draft 2 is alongside settings" asymmetry from the original spec
- Simplifies `leagueYearSettings.vue` significantly (removes the `!oneShotMode && !isMultiDraft` draft-field visibility guards and all auto-update draft logic)

### `EditLeagueYearRequest` becomes a new wrapper type

`EditLeagueYearSettings` currently accepts `LeagueYearSettingsViewModel` directly. It becomes `EditLeagueYearRequest` — a wrapper with a `FirstDraft?` field for single-draft leagues. Multi-draft leagues omit `FirstDraft`; the server ignores it if present and the league has 2+ drafts.

### Conference multi-draft is supported from the start

Conference creation uses the same `drafts[]` structure. The "Multi Draft" option is **not** hidden for conferences. The only deferred conference work is the `AssignLeaguePlayers` clone fix (Slice 5), which applies to adding new leagues to an *existing* conference year — not to initial creation.

---

## Backend

### New: `DraftSettingsRequest` (Web/Models/Requests/LeagueManager)

Used as elements of the `Drafts` array on `CreateLeagueRequest` and as `FirstDraft` on `EditLeagueYearRequest`. Distinct from the existing `CreateLeagueDraftRequest`, which handles adding drafts to an existing league year (and carries slot-expansion fields).

```csharp
public record DraftSettingsRequest(
    string? Name,          // nullable; client pre-fills "Initial Draft" / "Draft 2" / "Draft 3" etc.; server defaults if null
    LocalDate? ScheduledDate,
    int GamesToDraft,
    int CounterPicksToDraft);
```

### Modified: `CreateLeagueRequest`

- **Remove:** nothing from direct fields
- **Add:** `IReadOnlyList<DraftSettingsRequest> Drafts`
- `LeagueYearSettings` no longer contains `GamesToDraft` or `CounterPicksToDraft` (see ViewModel section)
- `IsValid()` validates: `Drafts.Count >= 1`; each draft `GamesToDraft >= 1`, `CounterPicksToDraft >= 0`

```csharp
public class CreateLeagueRequest
{
    // ...existing fields...
    public LeagueYearSettingsViewModel LeagueYearSettings { get; }
    public IReadOnlyList<DraftSettingsRequest> Drafts { get; }
}
```

### New: `EditLeagueYearRequest` (Web/Models/Requests/LeagueManager)

Replaces `LeagueYearSettingsViewModel` as the body type for `EditLeagueYearSettings`.

```csharp
public class EditLeagueYearRequest
{
    public Guid LeagueID { get; }
    public int Year { get; }
    public string? LeagueYearName { get; }
    public LeagueYearSettingsViewModel LeagueYearSettings { get; }
    public DraftSettingsRequest? FirstDraft { get; }   // null/absent for multi-draft leagues
}
```

Server behaviour: if `FirstDraft` is provided and the league has exactly 1 draft, that draft's `GamesToDraft`/`CounterPicksToDraft` are updated atomically with the year settings. If the league has 2+ drafts, `FirstDraft` is silently ignored.

### Modified: `LeagueYearSettingsViewModel` (Web/Models/RoundTrip)

- **Remove:** `GamesToDraft`, `CounterPicksToDraft` (and their `[Range]` annotations and all constructor/mapping references)
- All other fields unchanged

This is a breaking change to the read side (`GetLeagueYearOptions` returns this type). The edit form now reads draft counts from `leagueYear.drafts[0]` (already available via `GetLeagueYear`), not from the options endpoint.

`LeagueYearName` is **kept** in `LeagueYearSettingsViewModel` for the read path — `GetLeagueYearOptions` still returns it and `editLeague.vue` reads it from there to pre-populate the name field. On the write side, `EditLeagueYearRequest` has `LeagueYearName` at the top level; the controller uses that value and ignores any `LeagueYearName` that might appear inside the nested `LeagueYearSettings` object.

### Domain: New `DraftParameters` record (Lib/Domain/Requests)

```csharp
public record DraftParameters(
    string? Name,
    LocalDate? ScheduledDate,
    int GamesToDraft,
    int CounterPicksToDraft);
```

### Modified: `LeagueYearParameters` (Lib/Domain/Requests)

- **Remove:** `GamesToDraft`, `CounterPicksToDraft`

### Modified: `LeagueCreationParameters` (Lib/Domain/Requests)

- **Add:** `IReadOnlyList<DraftParameters> Drafts`

### Service: `FantasyCriticService.CreateLeague`

Iterates `Drafts` in order, creating each `tbl_league_draft` row within the same DB transaction as the league and league year. Reuses the existing internal draft-creation logic already called by the `CreateLeagueDraft` endpoint.

### Service: `FantasyCriticService.EditLeagueYear`

If `FirstDraft` is provided and the league has exactly 1 draft, updates that draft's `GamesToDraft`/`CounterPicksToDraft` atomically with the year settings edit. No change to the multi-draft path.

### Also affected: `CreateConference` backend

`CreateConference` receives the same `drafts[]` structure alongside `leagueYearSettings`. The service creates all drafts atomically, same as `CreateLeague`. Note: the `AssignLeaguePlayers` clone fix (needed when adding new leagues to an existing conference year) remains deferred to Phase 2 Slice 5 — it is unrelated to initial conference creation.

---

## Frontend

### New: `leagueCreationPresets.ts` (ClientApp/src/utilities/leagueCreationPresets.ts)

TypeScript module. Contains all auto-update logic currently living in `leagueYearSettings.vue`'s `autoUpdateOptions()` method.

**Exports:**

```typescript
export type GameMode = 'Standard' | 'Multi Draft' | 'One Shot';
export type ExperienceLevel = 'Beginner' | 'Standard' | 'Advanced';

export interface DraftSettings {
  name: string;
  scheduledDate: string | null;
  gamesToDraft: number;
  counterPicksToDraft: number;
}

export interface PresetResult {
  settings: Partial<LeagueYearSettings>;  // all auto-update fields
  drafts: DraftSettings[];
}

export function computePreset(
  gameMode: GameMode,
  experienceLevel: ExperienceLevel,
  playerCount: number
): PresetResult;

export function getDefaultDraft(
  draftIndex: number,       // 0-based; index 0 → "Initial Draft", index 1 → "Draft 2", index N → "Draft N+1"
  standardGames: number,
  allocatedSoFar: number    // sum of gamesToDraft in preceding drafts
): DraftSettings;
```

**`computePreset` behaviour:**

| Game Mode | Experience Level | `recommendedGames` | `draftGameRatio` | `enableBids` |
|---|---|---|---|---|
| Standard | Beginner | 42 | 4/7 | true |
| Standard | Standard | 72 | 1/2 | true |
| Standard | Advanced | 108 | 4/9 | true |
| One Shot | any | 50 | 1 (all) | false |
| Multi Draft | Beginner | 42 | 4/7 | false |
| Multi Draft | Standard | 72 | 1/2 | false |
| Multi Draft | Advanced | 108 | 4/9 | false |

Multi Draft always sets `enableBids = false`. The standard-games, counter-picks, drop, and other settings formulas are identical to the Standard game mode for the same experience level — only `enableBids` and the draft split differ.

For Multi Draft: `totalGamesToDraft` is split roughly evenly between Draft 1 and Draft 2 (`ceil(total/2)` for Draft 1, remainder for Draft 2). Additional drafts added by the user via "Add another draft" use `getDefaultDraft()` which allocates whatever games remain (`standardGames - allocatedSoFar`, min 1).

For One Shot: `gamesToDraft = standardGames`, `counterPicksToDraft = counterPicks`, `tradingSystem = 'NoTrades'`, `grantSuperDrops = false`, `enableBids = false`, drops all zero.

For Beginner: `counterPicks = 0`, `counterPicksToDraft = 0`, `tradingSystem = 'NoTrades'`, `grantSuperDrops = true`.

For Advanced: `pickupSystem = 'SecretBidding'`, `counterPicksBlockDrops = true`.

### New: `leagueCreationPresets.vue` (ClientApp/src/components)

Three-question form rendered above `leagueYearSettings` on the create pages. Implemented as dropdowns for this release; the component is structured to swap the inputs for icon-card controls in a future pass.

**Props:** `year: number`, `playerCountLabel?: string` (overrides the "How many players" label for conference mode)

**Emits:** `preset-applied: { gameMode: GameMode, settings: Partial<LeagueYearSettings>, drafts: DraftSettings[] }`

**Questions in order:**
1. **How many players?** (same numeric input as today; 2–20 range)
2. **Game Mode** — Standard / Multi Draft / One Shot  
   Description text explains each option; replaces the combined paragraph currently above the single dropdown.
3. **Experience Level** — Beginner / Standard / Advanced  
   Description explains the difficulty/complexity dimension (number of games, special slots).

All three must be filled before the component emits. When any value changes, `computePreset()` is called and `preset-applied` fires.

### New: `DraftCreationSettings.vue` (ClientApp/src/components)

Manages the `drafts` array for create/edit contexts.

**Props:**
- `modelValue: DraftSettings[]`
- `standardGames: number`
- `gameMode: GameMode`
- `editMode?: boolean` — if true, hides the "Add another draft" button (for the `editLeague` single-draft case)

**Behaviour:**
- Always renders at least 1 draft section (Draft 1)
- "Add another draft" button: visible only when `gameMode === 'Multi Draft'` and `!editMode`
- Each draft section: Name (text input), Scheduled Date (optional date picker), Games to Draft, Counter Picks to Draft
- Live validation banner: warns if sum of `gamesToDraft` across all drafts exceeds `standardGames`
- Draft sections 2+ have a remove button (only enabled when `drafts.length > 2` in Multi Draft mode, preventing below the minimum of 2)

### Modified: `leagueYearSettings.vue`

**Removed:**
- Entire `freshSettings` player-count + game-mode block (lines ~9–30 in the current component)
- `gamesToDraft` and `counterPicksToDraft` form fields and their `ValidationProvider` wrappers
- `oneShotMode` computed property
- `autoUpdateOptions()` method and all formulas within it
- `fullAutoUpdate()` method (or reduced to only calling `autoUpdateSpecialSlotOptions()`)
- `gameMode` and `gameModeOptions` data properties
- Watches on `internalValue.standardGames` and `internalValue.counterPicks` that kept one-shot counts in sync

**Kept:**
- `isMultiDraft` prop — still used in edit mode to show the "Visit Manage Drafts" info banner instead of draft fields (since there are no draft fields left, this now only controls the banner)
- `autoUpdateSpecialSlotOptions()` — still called when `standardGames` changes
- All other settings fields unchanged

The `isMultiDraft` info banner now reads: *"This league has multiple drafts. To edit draft settings (games to draft, counter picks, etc.), [visit the Manage Drafts page]."*

### Modified: `createLeague.vue`

**Structure after change:**
```
1. Basic Settings (league name, year) — unchanged
2. leagueCreationPresets.vue  [NEW — player count, Game Mode, Experience Level]
3. leagueYearSettings.vue     [settings only, no game mode / draft fields]
4. DraftCreationSettings.vue  [NEW — drafts array]
5. Other Options (public/test/custom rules) — unchanged
```

**Data:**
- `leagueYearSettings` — unchanged shape but without draft fields
- `drafts: DraftSettings[]` — new, starts as `[{ name: 'Initial Draft', ... }]`, updated by `preset-applied`
- `gameMode: GameMode` — stored to drive `DraftCreationSettings` behaviour

**`preset-applied` handler:** applies `settings` to `leagueYearSettings` and replaces `drafts` with the preset's `drafts`.

**Validity:** `leagueYearIsValid` adds `drafts.length >= 2` check when `gameMode === 'Multi Draft'`.

**Request payload:**
```json
{
  "leagueName": "...",
  "publicLeague": true,
  "testLeague": false,
  "customRulesLeague": false,
  "leagueYearSettings": { "standardGames": 10, "counterPicks": 2, ... },
  "drafts": [
    { "name": "Initial Draft", "scheduledDate": null, "gamesToDraft": 5, "counterPicksToDraft": 1 },
    { "name": "Draft 2", "scheduledDate": null, "gamesToDraft": 5, "counterPicksToDraft": 1 }
  ]
}
```

### Modified: `editLeague.vue`

**Structure after change:**
- `leagueYearSettings.vue` in edit mode — now always `:is-multi-draft="isMultiDraft"`, no draft fields
- When `!isMultiDraft`: renders `DraftCreationSettings.vue` with `edit-mode` (single draft, no add button)
- When `isMultiDraft`: `leagueYearSettings.vue` shows the "Visit Manage Drafts" banner (no additional component needed)

**Data:** `firstDraft` populated from `leagueYear.drafts[0]` after `fetchLeagueYear()` returns. Updated by `DraftCreationSettings`. The name field will reflect whatever was set at creation (e.g. "Initial Draft").

**Validity update:** `leagueYearIsValid` replaces references to `leagueYearSettings.gamesToDraft` / `leagueYearSettings.counterPicksToDraft` with `firstDraft.gamesToDraft` / `firstDraft.counterPicksToDraft` (guarded by `!isMultiDraft`).

**Request payload:**
```json
{
  "leagueID": "...",
  "year": 2026,
  "leagueYearName": null,
  "leagueYearSettings": { ... },
  "firstDraft": { "name": "Draft 1", "scheduledDate": null, "gamesToDraft": 5, "counterPicksToDraft": 1 }
}
```

### Modified: `createConference.vue`

Same changes as `createLeague.vue`:
- Add `leagueCreationPresets.vue` (no restrictions — Multi Draft is fully available)
- Add `DraftCreationSettings.vue`
- Send `drafts[]` in the request body alongside `leagueYearSettings`
- Update validity check

No `conferenceMode` guard is added. Multi-draft conference creation is fully supported. The `AssignLeaguePlayers` clone path remains deferred to Phase 2 Slice 5.

---

## Validation Summary

| Rule | Where enforced |
|---|---|
| `drafts.length >= 1` | Server (`CreateLeague` controller) |
| `drafts.length >= 2` when Multi Draft selected | Client (`createLeague.vue` validity check) |
| Each draft `gamesToDraft >= 1` | Server (per-draft validation) |
| Sum `gamesToDraft <= standardGames` | Client (warning banner in `DraftCreationSettings`); server enforces hard stop |
| `firstDraft` ignored if league has 2+ drafts | Server (`EditLeagueYear` service method) |

---

## Integration Testing

| Scenario | Assertions |
|---|---|
| Create league with 1 draft (Standard) | 1 `tbl_league_draft` row; `GamesToDraft` matches request |
| Create league with 2 drafts (Multi Draft) | 2 `tbl_league_draft` rows; both names and counts correct |
| Create league with 3 drafts | 3 rows; all correct |
| Edit single-draft league year settings with `firstDraft` | Draft row updated; year settings updated |
| Edit multi-draft league year settings with `firstDraft` present | Draft row NOT updated; year settings updated |
| Create conference with 2 drafts | Same assertions as league |

---

## Out of Scope

- Icon/card UI for Game Mode and Experience Level selectors (dropdowns now; component structure is upgrade-ready)
- `AddNewLeagueYear` multi-draft support (user explicitly deferred; it mirrors last year's settings, users add more drafts via Manage Drafts)
- Conference `AssignLeaguePlayers` clone fix (Phase 2 Slice 5; unrelated to initial creation)
- Changing the Experience Level labels (values stay as `Beginner` / `Standard` / `Advanced`; label in UI is "Experience Level")
- Tuning the exact recommended-games numbers and ratios in `computePreset` (the values in this spec are a reasonable starting point; they can be adjusted independently without any structural changes)
