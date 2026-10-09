# Publisher slots as data: league-level slot rows

## Context

Today a publisher's slots don't exist in the database. `tbl_league_publishergame.SlotNumber` is an integer, and
`Publisher.GetPublisherSlots` builds the slot list at runtime by counting from `tbl_league_year.StandardGames` and
`CounterPicks`, then attaches a special slot to the last N standard slots by position arithmetic in `LeagueOptions`.
Special slots are stored in `tbl_league_specialgameslot` as one row per (position, tag), so a flex slot is several rows.

Because nothing in the database knows how many slots exist, a game can end up with a `SlotNumber` past the roster:
it stops showing, stops scoring, and the next settings edit fails with "Cannot figure out slots". The fix in
`SlotAssignmentFunctions` (commit f7088cae0) patches one path; this plan removes the class of bug.

### New model

Slots are defined once per league year, and every publisher has the same slots.

- **`tbl_league_slot`**: one row per slot per league year. `LeagueSlotID`, `LeagueID`, `Year`, `CounterPick`,
  `SlotNumber`, `SlotName` (nullable). Unique on (`LeagueID`, `Year`, `CounterPick`, `SlotNumber`).
- **`tbl_league_slotrequirestag`**: the tags a slot requires. A slot with no tag rows and no name is a regular slot.
  A slot with tags is what we call a special slot today. A slot with a name and no tags is a custom slot (phase 2).
- **`tbl_league_publishergame`** loses `SlotNumber` and gains a non-null `LeagueSlotID`. Unique on
  (`PublisherID`, `LeagueSlotID`), so a publisher has at most one game per slot. A composite foreign key on
  (`LeagueSlotID`, `CounterPick`) to the slot table guarantees a counter pick sits in a counter-pick slot.
- **`tbl_league_year`** loses `StandardGames` and `CounterPicks`. The slot rows are the source of truth; the counts are
  derived. `tbl_league_draft.GamesToDraft` and `CounterPicksToDraft` stay, they describe a draft, not the roster.
- **`tbl_league_specialgameslot`** is dropped.

A game must now reference a slot that exists, so a game can't be invisible. Changing a slot's tags or name is an
update on one row: no game moves because a slot's rules changed.

### Decisions already made

- **`SlotNumber` is display order only**, numbered separately for standard and counter-pick slots, as today. Special
  slots stay last in the order. The migration places them with the current formula
  (`StandardGames - specialSlotCount + position`).
- **A game's slot must belong to the game's league year.** The schema can't express that without denormalizing
  `LeagueID`/`Year` onto the game row, which we won't do. The league-year loader throws if a game's slot isn't in that
  year's slot list.
- **The API contract doesn't change in this phase.** `LeagueYearParameters` and the league creation request keep
  "standard game count + counter pick count + special slots"; the server derives slot rows from that. View models keep
  `SlotNumber`, `OverallSlotNumber`, `SpecialSlotPosition` and `RequiredTags`. The gate is that regenerating the NSwag
  clients produces no diff.
- **Settings edits reconcile slot rows, keeping IDs.** Match the new definition to existing rows by counter-pick flag
  and slot number; update tags and names in place; insert new numbers; delete removed numbers. Games stay attached.
- **Removing a special slot without shrinking the roster is an update, not a deletion.** The row loses its tags.
  Nothing moves. The reassignment logic in `SlotAssignmentFunctions`, `OrganizeSlots` and
  `MakePublisherGameSlotsConsistent` is deleted.
- **Shrinking the roster is a contraction** (Step 5). Until Step 5 lands, an edit that would delete a slot any
  publisher has a game in is refused with a message.
- **Reordering needs an intermediate state.** With the unique key on (`PublisherID`, `LeagueSlotID`) and a non-null
  slot ID, swapping two games fails on the transient duplicate. Nothing references `PublisherGameID` by foreign key,
  so the repo can delete and reinsert the publisher's games inside the transaction, or park them on temporary slot
  rows. Either way it's contained in `ReorderPublisherGames`.
- **The migration fails loudly rather than guessing.** Any game whose slot number is past the roster blocks the
  `NOT NULL` step before anything is dropped. Such data is fixed by hand first.
- **Phase 2 (custom named slots)** is out of scope here, but the schema is ready for it: `SlotName` exists, and a
  slot may have tags, a name, or both. When it arrives, `HasSpecialSlots` (which flips projected scoring for
  ineligible games) must come to mean "has tag-restricted slots", not "has any non-regular slot", and the slot
  editor replaces the count-plus-special-slots request shape.

### Why break-first

Warnings are errors across the solution, so the compiler is a complete inventory of every C# use of the old fields.
It is not an inventory of SQL strings or stored procedures. Those are listed in Step 4 so none is found in production.

## Workflow

- Branch: `new-slot-system` (already exists, holds the draft migration).
- Commit this plan to `docs/publisher-slot-plan.md` first. Commit it again whenever it's revised.
- One step at a time: build, test, commit, then stop for review. Steps 1 and 3 are committed in a non-building state
  on purpose.

---

## Step 1: The full database migration (Claude)

Rewrite `src/FantasyCritic.DatabaseUpdater/Scripts/Sequential/2026-10-09_000_publisherSlotRefactor.sql` to take the
schema all the way to the final state. Pure SQL; no code migration, and no stored procedure edits yet.

Order inside the script, so a data problem stops it before anything is destroyed:

1. `CREATE TABLE IF NOT EXISTS tbl_league_slot` and `tbl_league_slotrequirestag` as drafted, plus:
   - `SlotName VARCHAR(50) NULL` on `tbl_league_slot`;
   - `UNIQUE (LeagueSlotID, CounterPick)` on `tbl_league_slot`, which the composite FK from publisher games needs.
2. Generate slot rows for every league year with a recursive CTE: `0..StandardGames-1` with `CounterPick = 0` and
   `0..CounterPicks-1` with `CounterPick = 1`, `UUID()` for IDs. Skip league years that already have rows, so a
   rerun after a data fix is safe.
3. Copy tags: for each `tbl_league_specialgameslot` row, insert into `tbl_league_slotrequirestag` against the standard
   slot numbered `StandardGames - specialSlotCount + SpecialSlotPosition` for that league year.
4. Inside a throwaway stored procedure (MySQL has no `ADD COLUMN IF NOT EXISTS`, and MySqlConnector rejects `@user`
   variables in scripts, so a procedure is the only way to get `IF` and `SIGNAL`):
   - guard: every special slot tag found a slot, or `SIGNAL` with the count and stop;
   - add `LeagueSlotID CHAR(36) NULL` to `tbl_league_publishergame` if it is missing;
   - backfill it by joining publisher game to publisher to slot on league, year, `CounterPick`, `SlotNumber`;
   - guard: no game is left without a slot, or `SIGNAL` with the count and stop. **This is the point where a game
     past the roster stops the migration, with nothing dropped.**
5. `MODIFY LeagueSlotID CHAR(36) NOT NULL`, add `UNIQUE (PublisherID, LeagueSlotID)` and the composite FK
   `(LeagueSlotID, CounterPick)`. The unique index also takes over from `UNQ_Slot` as the index behind the
   `PublisherID` foreign key, so it must exist before `UNQ_Slot` is dropped.
6. Drop `UNQ_Slot` and `SlotNumber` from `tbl_league_publishergame`.
7. Drop `StandardGames` and `CounterPicks` from `tbl_league_year`.
8. Drop `tbl_league_specialgameslot`.

Before asking for review: run the script against a production snapshot (RdsSnapshotManager / LocalDatabaseTool) and
report the result, including the count of games the guard would reject. Also note that the idempotent stored
procedures still name the dropped columns; MySQL accepts them at creation and they fail when called, which is expected
until Step 4.

**Tested (2026-10-09)** on a clone of the local Docker database (99 league years, 231 publishers, 794 games, 14
league years with special slots, 2 flex slots) and on an empty database running the whole chain. The clone had 20
games past the roster left behind by the integration tests that reproduced the bug; the guard stopped the script
with those 20 named and nothing dropped, a rerun after compacting them succeeded, and every surviving game kept its
slot number and every special slot tag landed on the slot the old formula put it in. The production snapshot run
is still outstanding (no connectivity at the time), so the production guard count is unknown.

The step ends with the migration committed and the solution not building.

## Step 2: Review, then the domain layer (Steve)

Review the migration. Then, with the tables in front of you, define the domain in `FantasyCritic.Lib` the way you want
it, and get **Lib building**. MySQL, FakeRepo, Web and the tests may all break; that's fine here.

Likely territory, not a prescription:

- `SpecialGameSlot` becomes a league slot type with an ID, counter-pick flag, slot number, optional name and tags.
- `LeagueOptions` holds the slot list; `StandardGames` and `CounterPicks` become computed. The overall-slot-number
  dictionary and `GetSpecialGameSlotByOverallSlotNumber` go away.
- `PublisherGame.SlotNumber` becomes a slot reference.
- `PublisherSlot` is built from a league slot plus that publisher's game, rather than from an index.
- `LeagueYearParameters` and `LeagueCreationParameters` keep their shape; conversion to slot rows lives in the domain
  so the Web contract doesn't move.
- `SlotAssignmentFunctions` is deleted.

Commit when Lib builds.

## Step 3: Entities 1:1 with the tables (Claude)

Make the MySQL row types match the schema exactly, nothing more:

- New `LeagueSlotEntity` and `LeagueSlotRequiresTagEntity`. Put them beside `LeagueYearEntity`,
  `PublisherGameEntity` and `SpecialGameSlotEntity` in `Lib/SharedSerialization/Database`, because the FakeRepo test
  data loader uses those row types and will need the new one for its CSV.
- `PublisherGameEntity`: `SlotNumber` becomes `LeagueSlotID`.
- `LeagueYearEntity`: drop `StandardGames` and `CounterPicks`.
- Delete `SpecialGameSlotEntity`, including `ConvertSpecialGameSlotEntities`.
- Delete `PublisherGameSlotNumberUpdateEntity` if nothing in Step 2 kept a use for it.

No repo edits. Commit with the solution not building, and stop.

If an entity doesn't map cleanly onto a Step 2 domain type, that's a signal that one of the two is wrong. Say so
rather than papering over it in the conversion code.

## Step 4: Fix everything else (Steve leads, Claude on call)

Work through the compiler errors. Suggested order, so the list stays readable: MySQL repo load path, MySQL repo write
paths, services, FakeRepo and unit tests, Web view models last. Build Lib alone first, then the solution.

**Things the compiler will not find.** Check each one off:

- Inline SQL in `MySQLFantasyCriticRepo`:
  - league year insert (~1188) and the two `UPDATE tbl_league_year SET StandardGames` statements (~1238, ~1285);
  - the second league year insert for new years (~1417);
  - special slot bulk inserts and the delete (~1177-1269, ~1326-1333, ~1412-1458) become slot and tag inserts;
  - both publisher game inserts (~2302, ~3713) write `LeagueSlotID`;
  - the three `UPDATE ... SET SlotNumber` routines (~2815, ~2850, ~2868): two are deleted with their callers, the
    reorder one is rewritten per the decision above;
  - `select * from tbl_league_specialgameslot where Year = @year` (~3809).
- Stored procedures under `Scripts/Idempotent/Stored Procedures/`:
  - `sp_getleagueyear` and `sp_getleagueyearsforconferenceyear` return `tbl_league_slot` and
    `tbl_league_slotrequirestag` rows instead of the special slot table;
  - `sp_getleaguesforuser` classifies a league as "OneShot" using `StandardGames` and `CounterPicks`; replace with
    counts over `tbl_league_slot`;
  - `sp_gethomepagedata` and `vw_meta_sitecounts` only read columns that survive; confirm and leave them.
- `DeleteLeagueYear` deletes only the year row; it must delete slot tags and slots first.
- Creating a league, creating a new league year, and creating an additional draft (`AdditionalStandardGames`,
  `AdditionalCounterPicks`) insert slot rows.
- Settings edits reconcile slot rows keeping IDs, and refuse to delete a slot any publisher has filled (until Step 5).
- The league-year loader throws when a game's slot isn't in that year's slots.
- Unit test fixtures under `src/FantasyCritic.Test/TestData/April2023/`: `LeagueYears.csv` loses the two count
  columns, `PublisherGames.csv` swaps `SlotNumber` for `LeagueSlotID`, and a new `LeagueSlots.csv` (plus tags)
  is generated from the old counts and special slots the same way the migration does. `TestDataService` loads it.
- `SlotAssignmentFunctionsTests` goes with its function. Keep the two integration tests in `SpecialSlotRemovalTests`;
  they should pass with no reassignment logic left.
- The Discord `PublisherCommand` orders games by slot number; it reads it through the slot now.

**Done when:**

- `dotnet build src/FantasyCritic.slnx` has zero warnings.
- Unit tests pass.
- Docker MySQL migrated from scratch, then `scripts/Regenerate-ApiClient.ps1` produces **no diff** in the generated
  clients. That is the proof the contract held.
- Integration tests pass, in particular draft, bids, trades, reorder, settings edits, special slot removal, and
  creating a second draft.

## Step 5: Automatic placement on contraction

When a settings edit shrinks the roster and a publisher has a game in a slot being deleted, the system moves that game
into the most reasonable open slot instead of refusing the edit. The rules are deliberately not defined yet; we'll
design them when we get here, with the Step 4 code in front of us.

What is fixed now:

- It replaces the Step 4 refusal. The refusal stays until this step lands.
- It runs inside the settings edit transaction, per publisher, over the slots that remain.
- If no open slot exists for a displaced game, the edit is refused, as before.

Inputs to weigh when designing it: the existing best-eligible-slot heuristic in `SlotEligibilityFunctions`,
`AllowMoveIntoIneligible`, and whether a displaced game should prefer a regular slot over a special one it happens to
qualify for.

## Verification (each step)

- Step 1: migration applied to a prod snapshot and to the Docker database from scratch; guard count reported.
- Step 2: `dotnet build src/FantasyCritic.Lib/FantasyCritic.Lib.csproj` with zero warnings.
- Step 3: entities match the tables column for column; nothing else changed.
- Step 4: the "Done when" list above.
- Step 5: integration tests for shrinking the roster with filled slots, written alongside the rules.
