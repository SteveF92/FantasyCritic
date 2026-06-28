# League Creation Presets Fix — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix two bugs in the league creation preset system (incorrect special slot tags for One Shot and wrong game counts) and deduplicate slot-building logic by sharing a single exported function.

**Architecture:** All changes are in the Vue/TypeScript frontend (`ClientApp`). The utility function `computeSpecialSlots` is fixed, exported, and imported into `leagueYearSettings.vue`. The `experienceLevel` value from the preset picker is threaded through the event chain so the Vue component can call the shared function correctly when the user manually adjusts game counts.

**Tech Stack:** TypeScript (`leagueCreationPresets.ts`), Vue 2 Options API (`.vue` files)

---

### Task 1: Fix `computeSpecialSlots` and One Shot game counts in `leagueCreationPresets.ts`

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/utilities/leagueCreationPresets.ts`

- [ ] **Step 1: Export `computeSpecialSlots` and fix the slot tags**

  Replace the `function computeSpecialSlots` declaration (currently unexported, lines 73–94) with the corrected, exported version below. Key changes: `export` keyword added; `NewGamingFranchise` removed from the expansion and remake slots; `DirectorsCut` added to the remake slot.

  ```typescript
  export function computeSpecialSlots(standardGames: number, experienceLevel: ExperienceLevel): object[] {
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
      slots.push({ specialSlotPosition: slots.length, requiredTags: ['ExpansionPack'] });
    }
    if (includeRemake) {
      slots.push({ specialSlotPosition: slots.length, requiredTags: ['PartialRemake', 'Remake', 'Reimagining', 'DirectorsCut'] });
    }
    return slots;
  }
  ```

- [ ] **Step 2: Fix One Shot game counts in `computePreset`**

  Replace the opening `recommendedNumberOfGames` / `draftGameRatio` assignment block inside `computePreset` (currently lines 101–116) with the following. The change: compute the experience-level base first, then apply the One Shot 75% scale-down as a separate override.

  ```typescript
  let recommendedNumberOfGames: number;
  let draftGameRatio: number;

  if (experienceLevel === 'Beginner') {
    recommendedNumberOfGames = 42;
    draftGameRatio = 4 / 7;
  } else if (experienceLevel === 'Advanced') {
    recommendedNumberOfGames = 108;
    draftGameRatio = 4 / 9;
  } else {
    recommendedNumberOfGames = 72;
    draftGameRatio = 1 / 2;
  }

  if (gameMode === 'One Shot') {
    recommendedNumberOfGames = Math.floor(recommendedNumberOfGames * 0.75);
    draftGameRatio = 1;
  }
  ```

- [ ] **Step 3: Verify the file compiles with no TypeScript errors**

  Run from `src/FantasyCritic.Web/ClientApp`:
  ```powershell
  npx tsc --noEmit
  ```
  Expected: no errors referencing `leagueCreationPresets.ts`.

- [ ] **Step 4: Commit**

  ```powershell
  git add src/FantasyCritic.Web/ClientApp/src/utilities/leagueCreationPresets.ts
  git commit -m "Fix computeSpecialSlots tags and One Shot game counts in presets utility."
  ```

---

### Task 2: Thread `experienceLevel` through the preset event chain

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/components/leagueCreationPresets.vue`
- Modify: `src/FantasyCritic.Web/ClientApp/src/views/createLeague.vue`
- Modify: `src/FantasyCritic.Web/ClientApp/src/views/createConference.vue`

- [ ] **Step 1: Emit `experienceLevel` from `leagueCreationPresets.vue`**

  In `leagueCreationPresets.vue`, find the `emitIfReady` method (line 65–69). Change the `$emit` call to include `experienceLevel`:

  ```js
  emitIfReady() {
    if (!this.playerCount || this.playerCount < 2 || this.playerCount > 20) return;
    const result = computePreset(this.gameMode, this.experienceLevel, this.playerCount, this.year);
    this.$emit('preset-applied', { gameMode: this.gameMode, experienceLevel: this.experienceLevel, ...result });
  }
  ```

- [ ] **Step 2: Receive and store `experienceLevel` in `createLeague.vue`**

  In `createLeague.vue`, add `experienceLevel: 'Standard'` to the `data()` return object (alongside `gameMode: 'Standard'`):

  ```js
  data() {
    return {
      errorInfo: '',
      leagueName: '',
      initialYear: '',
      leagueYearSettings: null,
      drafts: [],
      gameMode: 'Standard',
      experienceLevel: 'Standard',
      publicLeague: true,
      testLeague: false,
      customRulesLeague: false,
      leagueYearEverValid: false
    };
  },
  ```

  Update `onPresetApplied` to capture `experienceLevel` from the event:

  ```js
  onPresetApplied({ gameMode, experienceLevel, settings, drafts }) {
    this.gameMode = gameMode;
    this.experienceLevel = experienceLevel;
    if (!this.leagueYearSettings) {
      this.leagueYearSettings = {
        year: this.initialYear,
        pickupSystem: 'SemiPublicBiddingSecretCounterPicks',
        tiebreakSystem: 'LowestProjectedPoints',
        tradingSystem: 'Standard',
        draftSystem: 'Flexible',
        scoringSystem: 'LinearPositive',
        releaseSystem: 'MustBeReleased',
        ineligibleGameSystem: 'CaseByCase',
        enableBids: true,
        tags: { banned: [], allowed: [], required: [] },
        specialGameSlots: []
      };
    }
    Object.assign(this.leagueYearSettings, settings);
    this.leagueYearSettings.year = this.initialYear;
    this.drafts = drafts.map((d) => ({ ...d }));
  },
  ```

  Add `:experience-level="experienceLevel"` to the `<leagueYearSettings>` usage in the template (line 44):

  ```html
  <leagueYearSettings v-model="leagueYearSettings" :year="initialYear" :game-mode="gameMode" :experience-level="experienceLevel" fresh-settings>
  ```

- [ ] **Step 3: Apply the same changes to `createConference.vue`**

  In `createConference.vue`, make the identical three changes:

  Add `experienceLevel: 'Standard'` to `data()`:
  ```js
  data() {
    return {
      errorInfo: '',
      conferenceName: '',
      primaryLeagueName: '',
      initialYear: '',
      leagueYearSettings: null,
      drafts: [],
      gameMode: 'Standard',
      experienceLevel: 'Standard',
      customRulesConference: false,
      leagueYearEverValid: false
    };
  },
  ```

  Update `onPresetApplied` (same body as `createLeague.vue` step above, adjusted for conference payload):
  ```js
  onPresetApplied({ gameMode, experienceLevel, settings, drafts }) {
    this.gameMode = gameMode;
    this.experienceLevel = experienceLevel;
    if (!this.leagueYearSettings) {
      this.leagueYearSettings = {
        year: this.initialYear,
        pickupSystem: 'SemiPublicBiddingSecretCounterPicks',
        tiebreakSystem: 'LowestProjectedPoints',
        tradingSystem: 'Standard',
        draftSystem: 'Flexible',
        scoringSystem: 'LinearPositive',
        releaseSystem: 'MustBeReleased',
        ineligibleGameSystem: 'CaseByCase',
        enableBids: true,
        tags: { banned: [], allowed: [], required: [] },
        specialGameSlots: []
      };
    }
    Object.assign(this.leagueYearSettings, settings);
    this.leagueYearSettings.year = this.initialYear;
    this.drafts = drafts.map((d) => ({ ...d }));
  },
  ```

  Add `:experience-level="experienceLevel"` to the `<leagueYearSettings>` usage in the template (line 58):
  ```html
  <leagueYearSettings v-model="leagueYearSettings" :year="initialYear" :game-mode="gameMode" :experience-level="experienceLevel" fresh-settings conference-mode>
  ```

- [ ] **Step 4: Commit**

  ```powershell
  git add src/FantasyCritic.Web/ClientApp/src/components/leagueCreationPresets.vue
  git add src/FantasyCritic.Web/ClientApp/src/views/createLeague.vue
  git add src/FantasyCritic.Web/ClientApp/src/views/createConference.vue
  git commit -m "Thread experienceLevel through preset-applied event to leagueYearSettings."
  ```

---

### Task 3: Update `leagueYearSettings.vue` to use the shared `computeSpecialSlots`

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/components/leagueYearSettings.vue`

- [ ] **Step 1: Add the import**

  At the top of the `<script>` block in `leagueYearSettings.vue` (currently line 297), add the import after the existing component imports:

  ```js
  import LeagueTagSelector from '@/components/leagueTagSelector.vue';
  import SpecialGameSlotSelector from '@/components/specialGameSlotSelector.vue';
  import { computeSpecialSlots } from '@/utilities/leagueCreationPresets';
  ```

- [ ] **Step 2: Add `experienceLevel` prop**

  In the `props` object (currently lines 305–314), add `experienceLevel` after `gameMode`:

  ```js
  props: {
    value: { type: Object, required: true },
    year: { type: Number, required: true },
    editMode: { type: Boolean },
    currentNumberOfPlayers: { type: Number, default: null },
    freshSettings: { type: Boolean, required: true },
    conferenceMode: { type: Boolean },
    isMultiDraft: { type: Boolean, default: false },
    leagueId: { type: String, default: null },
    gameMode: { type: String, default: 'Standard' },
    experienceLevel: { type: String, default: 'Standard' }
  },
  ```

- [ ] **Step 3: Replace `autoUpdateSpecialSlotOptions`**

  Find `autoUpdateSpecialSlotOptions` in the `methods` object (currently lines 349–406) and replace its entire body with a call to the shared function:

  ```js
  autoUpdateSpecialSlotOptions() {
    if (!this.freshSettings) {
      return;
    }
    if (!this.internalValue.standardGames || this.internalValue.standardGames > 50) {
      return;
    }
    this.internalValue.specialGameSlots = computeSpecialSlots(this.internalValue.standardGames, this.experienceLevel);
  }
  ```

- [ ] **Step 4: Verify no linter errors**

  Check the file for linter/TypeScript errors (IDE will surface these, or run `npx tsc --noEmit` from `ClientApp`). There should be none.

- [ ] **Step 5: Commit**

  ```powershell
  git add src/FantasyCritic.Web/ClientApp/src/components/leagueYearSettings.vue
  git commit -m "Use shared computeSpecialSlots in leagueYearSettings; add experienceLevel prop."
  ```

---

### Task 4: Manual verification

- [ ] **Step 1: Start the dev server** (if not already running)

  From `src/FantasyCritic.Web`:
  ```powershell
  dotnet run
  ```

- [ ] **Step 2: Verify Standard league special slots**

  Navigate to Create League. Set player count to 6, Game Mode = Standard, Experience Level = Standard. Confirm the preset applies special slots without `NewGamingFranchise` on the Expansion Pack or Remake slots. Specifically:
  - NGF-only slots should appear first
  - One slot should show only `ExpansionPack`
  - One slot should show `PartialRemake`, `Remake`, `Reimagining`, `DirectorsCut`

- [ ] **Step 3: Verify Beginner special slots**

  Set Experience Level = Beginner. Confirm no special slots are generated.

- [ ] **Step 4: Verify One Shot game counts respect experience level**

  Set Game Mode = One Shot. Confirm:
  - Beginner: `standardGames` ~5–6 per player (based on 31 total / player count)
  - Standard: ~8–9 per player (54 total)
  - Advanced: ~12–14 per player (81 total)

- [ ] **Step 5: Verify live update when manually changing game count**

  Apply a Standard preset, then manually change the `standardGames` field. Confirm the special slots update automatically and do not include `NewGamingFranchise` on the Expansion/Remake slots.
