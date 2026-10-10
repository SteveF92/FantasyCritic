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

Three migrations take the schema all the way to the final state. DbUp runs them in name order, stops at the first
failure, and journals each only when it completes, so the cleanup can never run against unmigrated data, and a rerun
after a data fix starts at the first unjournaled one. No stored procedure edits yet.

1. **`Scripts/Sequential/2026-10-09_000_publisherSlotSchema.sql`**, DDL only, runs exactly once. Creates
   `tbl_league_slot` (with `SlotName VARCHAR(50) NULL` and a `UNIQUE (LeagueSlotID, CounterPick)` index for the
   composite FK to target) and `tbl_league_slotrequirestag`, and adds a bare `LeagueSlotID CHAR(36) NULL` to
   `tbl_league_publishergame`. The column's unique index and foreign key wait for the cleanup script: building
   them in bulk over a filled column is far cheaper than maintaining them row by row through a million-row
   backfill, and they validate the backfill all the same before anything is dropped.
2. **`CodeMigrations/PublisherSlotDataMigration.cs`**, registered as `2026-10-09_001_publisherSlotData.cs`, DML only.
   Guards first, so a failure writes nothing: every special slot position fits inside its roster, and no game has a
   `SlotNumber` past `StandardGames` (or `CounterPicks`). Each offending game is logged with its league, publisher,
   name and slot, then the migration throws. **This is where a game past the roster stops the deploy, with nothing
   changed.** Then it generates `0..StandardGames-1` and `0..CounterPicks-1` slot rows per league year and copies
   special slot tags onto slot `StandardGames - specialSlotCount + SpecialSlotPosition`, committed as one
   transaction, and backfills `LeagueSlotID` with one `UPDATE ... JOIN` per year, each committed on its own and
   logged with its row count and duration. It is safe to rerun at any point: slot rows already present are
   count-checked and kept, and the backfill only touches games whose slot is still null. (A single transaction for
   the whole backfill was tried first and crawled on the production snapshot once it held a million modified rows
   against Docker's 128 MB buffer pool.) It uses raw SQL and private row classes, never a repo, entity or domain
   type, because those change shape in Steps 2 to 4 and this code must keep running on a fresh database forever.
   For the same reason it needs no prerequisites script.
3. **`Scripts/Sequential/2026-10-09_002_publisherSlotCleanup.sql`**, DDL only. One `ALTER` on
   `tbl_league_publishergame` does `MODIFY LeagueSlotID NOT NULL`, adds `UNIQUE (PublisherID, LeagueSlotID)` and the
   composite FK `(LeagueSlotID, CounterPick)` (each fails if the backfill left anything inconsistent), and drops
   `UNQ_Slot` and `SlotNumber` (the new unique index takes over as the index behind the `PublisherID` FK, which
   MySQL accepts within one statement). A single statement because every `ALTER` on that table rebuilds its million
   rows. Then drop `StandardGames` and `CounterPicks` from `tbl_league_year`, and drop `tbl_league_specialgameslot`.

Before asking for review: run the chain against a production snapshot (RdsSnapshotManager / LocalDatabaseTool) and
report the result, including the count of games the guard would reject. Also note that the idempotent stored
procedures still name the dropped columns; MySQL accepts them at creation and they fail when called, which is expected
until Step 4.

**Tested (2026-10-09)** on a clone of the local Docker database (99 league years, 231 publishers, 794 games, 14
league years with special slots, 2 flex slots) and on an empty database running the whole chain. The clone had 20
games past the roster left behind by the integration tests that reproduced the bug; the data migration logged all 20
and threw with the schema script journaled and nothing written, a rerun after compacting them went through all three
steps, and every game outside those 10 publishers kept its slot number while every special slot tag landed on the
slot the old formula put it in.

**Production snapshot (2026-10-10)**, 20,546 league years, 84,872 publishers, 973,431 games, 6,790 league years
with special slots, 50,460 special slot tags, run in Docker against a 128 MB buffer pool:

- **The guard found 13 games past the roster**, in three leagues: two 2020 counter picks numbered 7 and 8 in a
  2-counter-pick league, one game at slot 18 of 18 with slot 16 empty, and ten games at slots 43 and 44 of 43 in a
  league with 14 special slots (the special-slot-removal bug). Every one has an open regular slot (or counter-pick
  slot) to move into, so none has to land in a special slot. **Production needs this 13-row fix before the deploy**;
  the statements, each guarded by the game's current slot number, are the Step 5 placement rule done by hand.
- **Timings** (schema script 5 min; data step 34 min with the first, single-transaction backfill, of which the
  2025 batch alone took 22 min; cleanup script 24 min). The per-year committed backfill and the index build moved to
  cleanup came out of this run. The cleanup's two table rebuilds dominate, and a bigger buffer pool will change every
  number here, so treat ~1 hour as the Docker worst case, not the production estimate.
- End state verified: every game's slot is in its own league year with a matching counter-pick flag, 5,802 flex
  slots carry two or more tags, no counter-pick slot has a tag, and the 13 moved games sit where the fix put them.

**Revised migration (2026-10-10)**, after the snapshot run: per-year committed backfill, resumable data step, indexes
built at cleanup in one `ALTER`. Tested on clones of the local database (12 league years, 182 games, 2 league years
with special slots): the full chain; a data step staged by hand with slot rows present and 102 of 182 games linked,
which the migrator picked up with "resuming at the backfill", linked the remaining 80 and went on to cleanup; and an
empty database. Each ended with the same schema and with every game on its original slot number.

**Revised migration at production scale (2026-10-10, forked session)**, on the snapshot after reversing the first
run in place, with `innodb_buffer_pool_size` raised to 4 GB and `innodb_redo_log_capacity` to 2 GB (production is
db.t4g.large with roughly a 6 GB pool, so the 128 MB Docker default was the pathological case):

- Schema script instant; slot and tag inserts 51 s; the nine per-year backfills about 40 s in total (2025: 5.8 s,
  2026: 6.5 s); cleanup 78 s with the earlier two-statement form. **Whole chain about 3.5 minutes.**
- Kill-and-resume: the migrator was killed after the 2021 batch committed; the rerun logged "resuming at the
  backfill", relinked nothing for the finished years, completed the rest and went on to cleanup. Against reference
  copies of the pre-migration tables: 0 tags missing or extra, 0 slot-count mismatches, 0 games whose slot differs
  from the old slot number, 0 rows lost.
- The single-`ALTER` cleanup was not part of those runs. That MySQL accepts dropping `UNQ_Slot` while adding
  `UNQ_PublisherSlot` in one statement with the `PublisherID` FK present was confirmed separately, on a synthetic
  table and on a small clone.

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
