# League Creation Presets Fix — Design Spec

**Date:** 2026-06-28

## Problem Summary

Two bugs in the league creation preset system introduced during a prior refactor:

1. **Special slots bug** — `computeSpecialSlots` in `leagueCreationPresets.ts` incorrectly adds `NewGamingFranchise` to the Expansion Pack slot and the Remake slot. The correct behavior (present in the Vue component's `autoUpdateSpecialSlotOptions`) is that those slots should only require their own respective tags. Additionally, `DirectorsCut` is missing from the remake slot in the preset function but present in the Vue component.

2. **One Shot game counts** — `computePreset` ignores experience level for One Shot mode, always using `recommendedNumberOfGames = 50`. One Shot leagues should scale from the same experience-level bases as Standard leagues, at 75% of the standard count.

3. **Duplicated / diverged slot logic** — `computeSpecialSlots` (preset utility) and `autoUpdateSpecialSlotOptions` (Vue component) implement the same logic independently and have drifted. `autoUpdateSpecialSlotOptions` also has a broken guard (`gameMode === 'Beginner'` should be `experienceLevel === 'Beginner'`). The fix deduplicates these by exporting `computeSpecialSlots` from the utility and having the Vue component call it directly.

---

## Design

### 1. Fix `computeSpecialSlots` in `leagueCreationPresets.ts`

- Expansion slot: `['NewGamingFranchise', 'ExpansionPack']` → `['ExpansionPack']`
- Remake slot: `['NewGamingFranchise', 'PartialRemake', 'Remake', 'Reimagining']` → `['PartialRemake', 'Remake', 'Reimagining', 'DirectorsCut']`
- Export the function (currently unexported) so Vue components can import it.

### 2. Fix One Shot game counts in `computePreset`

Restructure the `recommendedNumberOfGames` / `draftGameRatio` assignment so experience level is determined first, then One Shot applies a 75% multiplier and forces `draftGameRatio = 1`:

```
if experienceLevel === 'Beginner':   base = 42,  ratio = 4/7
if experienceLevel === 'Advanced':   base = 108, ratio = 4/9
else (Standard):                     base = 72,  ratio = 1/2

if gameMode === 'One Shot':
  base = floor(base * 0.75)
  ratio = 1
```

Resulting One Shot targets by experience level:

| Experience | Standard base | One Shot target (75%) |
|---|---|---|
| Beginner | 42 | 31 |
| Standard | 72 | 54 |
| Advanced | 108 | 81 |

### 3. Thread `experienceLevel` through to `leagueYearSettings.vue`

**`leagueCreationPresets.vue`** — include `experienceLevel` in the `preset-applied` event payload alongside `gameMode`.

**`createLeague.vue`** — add `experienceLevel: 'Standard'` to `data()`, capture it in `onPresetApplied`, pass `:experience-level="experienceLevel"` to `<leagueYearSettings>`.

**`createConference.vue`** — same changes as `createLeague.vue`.

**`editLeague.vue`** — no change. The new prop defaults to `'Standard'`; edit mode has `freshSettings: false` so `autoUpdateSpecialSlotOptions` never fires.

**`leagueYearSettings.vue`**:
- Add `experienceLevel` prop (String, default `'Standard'`).
- Import `computeSpecialSlots` from `@/utilities/leagueCreationPresets`.
- Replace the entire inline slot-building body of `autoUpdateSpecialSlotOptions` with:
  ```js
  this.internalValue.specialGameSlots = computeSpecialSlots(this.internalValue.standardGames, this.experienceLevel);
  ```
- Remove the dead `gameMode === 'Beginner'` guard (the imported function handles the Beginner case correctly).

---

## Files Changed

| File | Change |
|---|---|
| `src/utilities/leagueCreationPresets.ts` | Fix slot tags; fix One Shot game counts; export `computeSpecialSlots` |
| `src/components/leagueCreationPresets.vue` | Emit `experienceLevel` in `preset-applied` |
| `src/views/createLeague.vue` | Store + pass `experienceLevel` |
| `src/views/createConference.vue` | Store + pass `experienceLevel` |
| `src/components/leagueYearSettings.vue` | Add prop; import + call shared function |

`editLeague.vue` — no change required.
