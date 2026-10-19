# Publisher slots: review notes

Findings from reviewing [Step 2 of the publisher slot plan](publisher-slot-plan.md), the domain layer, as a checklist
to cross off. Written 2026-10-13 against `7b582c7d2` on `new-slot-system`; line numbers are as of that commit and will
drift, so search for the named code if a line has moved.

When an item is done, tick it and add the commit. When one is dropped, tick it and say why.

## Bugs

- [x] **B1. Tag-only slot edits are silently dropped.** `LeagueSlot` equality is by ID only, so
  `LeagueOptions.GetDifferences` (`LeagueSlots.SequenceEqual`, `LeagueOptions.cs:242`) sees no change when a slot's
  tags or name change. If that is the only edit, `HasChanges` is false and `EditLeague` never calls
  `EditLeagueYear`. Keep ID equality (dictionary keys rely on it) and compare a value projection in the diff, e.g.
  counter pick, number, name and tags.
  - Fixed in `f8a2656b5`: `LeagueSlotDifferenceFunctions.GetSlotDifferences` matches slots by ID and compares tags (as a set), name
    and number, one log line per change ("Slot 3 required tags changed from 'New Game' to 'New Game' or 'Remake'.",
    "Slot 2 removed.", "Slot 3 moved to slot 2."). That replaces the before/after dump of every slot. Renumbering
    caused by an add or remove isn't reported as a move; a kept slot whose number changed says "(was slot N)".
    Numbers are 1-based. Tests in `LeagueSlotDifferenceFunctionsTests`, run in a scratch project against Lib since
    the Test project doesn't build until Step 4.
- [x] **B2. New counter-pick slots are created as standard slots.** `LeagueOptions.WithNewDraftOptions`
  (`LeagueOptions.cs:284`) passes `false` for the counter-pick flag when numbering new counter-pick slots.
- [x] **B3. Former games carry a made-up slot ID.** `tbl_league_formerpublishergame` has no slot column, but
  `FormerPublisherGame` wraps a `PublisherGame`, whose `LeagueSlotID` is non-null, so `FormerPublisherGameEntity.ToDomain`
  (`FantasyCritic.MySQL/Entities/FormerPublisherGameEntity.cs:72`) has to invent one; it passed `0` before. The
  interface is covered (`OptionalLeagueSlotID => null`), and the `LeagueYear` slot check looks only at current games,
  but anything reading `former.PublisherGame.LeagueSlotID` gets the made-up value. Options: give
  `FormerPublisherGame` its own fields, or accept `Guid.Empty` and document it.
  - Fixed in Lib: `FormerPublisherGame` no longer wraps a `PublisherGame`. It has its own fields (everything but the
    slot) and `PublisherGame.GetFormerPublisherGame` copies them across. Left for Step 4, outside Lib:
    - `FormerPublisherGameEntity`: the constructor and `ToDomain` read and build through `.PublisherGame`. The
      constructor also never sets `AcquiredInTradeID`, so the insert always writes null for it; worth fixing then.
    - `DomainConversionUtilities.GetActivePickupBids` and `GetConditionalDropPublisherGame` (both copies, the other in
      `MySQLFantasyCriticRepo`) key their lookups by `.PublisherGame`. When a bid's conditional-drop game is a former
      game, they hand back `former.PublisherGame` as the bid's `PublisherGame`, so that case now has to build a
      `PublisherGame` itself and needs a slot ID, like the fake game just below it. `CanConditionallyDropGame` fails
      such a bid before the slot is read, so `Guid.Empty` would do, but it is the same made-up value in a new place.
    - `PublisherGameViewModel(FormerPublisherGame, ...)` reads everything through `.PublisherGame`, including
      `WillRelease`/`CouldRelease`, which live on `PublisherGame` and have no former-game equivalent yet.

## Design questions

- [ ] **D1. How a settings edit matches slot IDs.** `LeagueYearParameters` takes `LeagueSlot`s with IDs, so whatever
  builds them from the Web request ("counts + special slots") decides which rows survive. The plan says match by
  (counter pick, slot number). Combined with "special slots go last", that moves tags between rows. Example: 10 slots
  with specials at 8 and 9, shrunk to 9 keeping both specials. Row 7 gains special 0's tags, row 8 gets special 1's,
  row 9 is deleted and its game displaced. The old algorithm kept games in their special position.
  - Proposed: match regular slots in order to existing regular rows, and special position k to the k-th existing
    special row, renumbering rows as needed. Games keep their tags; only the extra rows are deleted. Renumbering hits
    the same transient unique-key problem as reorder, so the repo needs that trick in one more place.
  - Proposed: this matching lives in Lib, since it is the "keep IDs" rule; Web only translates request shapes.
  - Shapes D3 and the Step 4 settings-edit repo work, so settle it first.
- [x] **D2. The placement rule on contraction.** Settled in `13e78ea92`/`4fb37155c`: eligible under the new options,
  then regular before tag-restricted, then lowest number, then any open slot. Recorded in the plan (Steps 2 and 5).
- [ ] **D3. Special slots are no longer always last.** `WithNewDraftOptions` appends new regular slots after the
  existing specials. Fine for the domain, but the Web layer's `SpecialSlotPosition` and `OverallSlotNumber` assume
  specials are last. Under D1 as proposed, the next settings round trip renumbers the rows and games follow their IDs;
  under number matching it would retag them.
- [x] **D4. Two sources of truth in `LeagueYearParameters`.** It still has `StandardGames` and `CounterPicks`, which
  `LeagueOptions(LeagueYearParameters)` ignores. Drop them, or validate that they match the slots.

## Smaller items

- [x] **S1. `Validate` gaps** (`LeagueOptions.cs:94`). It doesn't reject tags on counter-pick slots. The migration
  guarantees none exist, but `HasSpecialSlots` and `GetClaimErrorsForLeagueYear` would count them. It also doesn't
  check that slot numbers run 0..n-1, which `WithNewDraftOptions` assumes when it numbers new slots from `Count`.
  - Counter-pick tags rejected in `87d8467c4`. Slot numbers are checked alongside it: standard and counter-pick
    slots must each run 0..n-1.
- [x] **S2.** `Publisher.GetCounterPickedGameIsValid` (`Publisher.cs:198`) uses `LeagueSlots.Single(...)` where
  `LeagueSlotDictionary` would do.
  - Fixed in `87d8467c4`.
- [x] **S3.** `ReorderPublisherGames` takes a mutable `Dictionary<LeagueSlot, PublisherGame?>` in
  `IFantasyCriticRepo.cs:60` and `PublisherService.cs:208`. The convention is `IReadOnlyDictionary`.
  - Fixed in `87d8467c4`.
- [x] **S4.** The "not acquiring now" fallback in `SlotEligibilityFunctions.GetPublisherSlotAcquisitionResult`
  (`SlotEligibilityFunctions.cs:57` and `:76`) returns `LeagueSlots[0]`, a standard slot even for counter-pick bids.
  It is only a placeholder, but the first slot of the matching kind would be more honest.
  - Fixed in `87d8467c4`.
- [x] **S5.** `SlotEligibilityFunctions.GetTradeSlotResult` (`SlotEligibilityFunctions.cs:117`) no longer uses its
  `publisher` or `leagueYear` parameters.
  - Fixed in `87d8467c4`.
- [ ] **S6.** `LeagueSlot.FakeSlot` (`Guid.Empty`, `LeagueSlot.cs:6`) exists only for `RoyaleService.cs:191`. A
  `ClaimResult` that can succeed without a slot would be cleaner.
- [x] **S7.** `LeagueSlot.cs` sits at the Lib root, namespace `FantasyCritic.Lib`, rather than in `Domain/` with the
  other domain types.
  - Fixed in `87d8467c4`.
- [x] **S8.** `LeagueSlot.ToString` (`LeagueSlot.cs:44`) leaves a trailing `|` when the slot has no name. That string
  appeared in the settings-change log; since B1 the log no longer uses it, so this is cosmetic only.
  - The trailing `|` is gone in `87d8467c4`, but the counter-pick prefix became its own part, so a standard slot
    now prints `|0|REG` and a counter pick `CPK-|0|REG`. Fixed after that: back to `0|REG` and `CPK-0|REG`.
- [x] **S9.** `LeagueOptions.StandardGames`, `CounterPicks` (`:67`-`68`) and `SpecialGameSlots` (`:89`) are
  recalculated on every call, including inside per-publisher loops. `LeagueOptions` is immutable, so compute them
  once in the constructor.
  - `StandardGames` and `CounterPicks` done in `87d8467c4`, `SpecialGameSlots` after it.
- [ ] **S10.** The `MakeSlotsConsistent` job (job type, handler, registry entry) and
  `IFantasyCriticRepo.ManualMakePublisherGameSlotsConsistent` are still in Lib. The plan deletes them with
  `OrganizeSlots`; check whether existing job rows of that type need handling. (Found while updating the plan.)
- [x] **S11. `Publisher.GetPublisherSlots` silently skipped a game whose slot isn't in the options.** Fixed in
  `7b582c7d2`: the `LeagueYear` constructor now throws on such a game.
