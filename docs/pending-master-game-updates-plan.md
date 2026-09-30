# Pending master game updates go through the database

## Context

`DiscordPushService` is a singleton that batches three kinds of game news in memory and sends them at the end of `RefreshCaches`. That worked while everything ran in the web process. Since data-refresh-jobs moved `RefreshCaches` to the worker, the queues and the sender live in different processes:

| Queue | Filled in | Sent from | Now |
|---|---|---|---|
| New master games | Web, `InterLeagueService.CreateMasterGame` | Worker, `CacheRefresher` | **Never sent** |
| Edits | Web, `InterLeagueService.EditMasterGame` | Worker | **Never sent** |
| Critic score updates | Worker, `CriticScoreRefresher` | Worker | Sent, but lost if the worker restarts first |

Side effects today:
- Web's two queues only grow, until the process restarts.
- "Clear Edit Game Discord Queue" clears Web's queue, which nothing sends.
- On beta, the three spoof endpoints call `SendBatchedMasterGameUpdates` in Web, so each spoof also sends every real new game and edit that has piled up since Web started.

Some flaws were there before the move and get fixed here too:
- A deploy of Web dropped the queued edits.
- `SendBatchedMasterGameUpdates` sends from the bags and then calls `.Clear()`, so anything queued during a send is wiped.

We considered a Web callback from the worker and reading the existing tables, and chose a table of pending updates. Web and the worker write rows, and the worker sends and deletes them.

## What a row must hold to keep today's behavior exactly

The send path never reads `MasterGameYear` stats. For each kind it reads:

| Kind | What the send reads | Stored |
|---|---|---|
| New game | The game as it was created: relevance, tags, release date, note, link | Snapshot of the game |
| Score update | The game **before** the score was saved, plus the old and new score | Snapshot before the update, old score, new score |
| Edit | The game **before** the edit (relevance, `DiscordPushService.cs:161`), the game **after** (grouping key and link name), the year both were read in (for `GetWillReleaseStatus`), and the change strings | Snapshot before, snapshot after, year, changes |

The pre-change game matters. `LeagueGameNewsRelevanceHandler` treats a game as eligible only while `!CriticScore.HasValue`. A first score read fresh at send time would make an eligible game look ineligible, and "eligible only" channels would stop getting first scores.

The snapshot must carry what `MasterGame` computes from, not what it computes:
- `RawCriticScore` rather than `CriticScore`, because `CriticScore` averages the sub-games when the raw score is null.
- The sub-games.
- Tags by name.
- The added-by user.

`MasterGameEntity` can't be reused for this. Its constructor copies the computed `CriticScore` and skips `OpenCriticSlug`.

`MasterGameEditMessage` also keeps the gate in `EditMasterGame`: no message for a minor edit, an edit with no changes, or a game with no `MasterGameYear` row for the current year.

## Design

### Table
`tbl_discord_pendingmastergameupdate`, plus a lookup table for the kind, following the `tbl_job_status` pattern:

| Column | Type | Notes |
|---|---|---|
| `PendingUpdateID` | char(36) PK | |
| `UpdateType` | varchar(50), FK to `tbl_discord_pendingmastergameupdatetype` | `NewGame`, `ScoreUpdate`, `Edit` |
| `MasterGameID` | char(36) | No FK; see below |
| `MasterGameSnapshot` | JSON NOT NULL | The game as created / before the score / before the edit |
| `EditedMasterGameSnapshot` | JSON NULL | Edits only |
| `Year` | int NULL | Edits only: the `MasterGameYear.Year` the old code compared statuses in |
| `OldCriticScore`, `NewCriticScore` | decimal(7,4) NULL | Score updates only, same type as `tbl_mastergame.CriticScore` |
| `Changes` | JSON NULL | Edits only: the change strings |
| `QueuedTimestamp` | timestamp NOT NULL | Sends go in this order |

**No FK on `MasterGameID`.** `MergeMasterGame` and `MySQLMasterGameUpdater` delete master games. A foreign key would make them fail, or would need both of them changed. The row is self-contained anyway. A pending update for a game that gets merged away still sends, as it would have from memory today.

### Snapshot shape
`MasterGameSnapshotEntity` in `FantasyCritic.MySQL/Entities`:
- The `tbl_mastergame` fields, with `RawCriticScore` and `OpenCriticSlug`.
- `AddedByUserID` and `AddedByUserDisplayName`.
- Tag names.
- Sub-games, with the `MasterSubGameEntity` fields.

It is serialized with `FantasyCriticJsonOptions.Default` (camelCase, NodaTime). `ToDomain` looks tags up in the tag dictionary. A tag that no longer exists throws, so the send fails loudly instead of posting with a tag missing. The entity is the only place that knows the snapshot's shape, as `MasterGameTagEntity` is for the tag `Examples` JSON. The domain stays free of JSON.

### Repository
Everything goes on `IMasterGameRepo`: it already has the tag dictionary, the entity mapping and the create/edit transactions.
- `CreateMasterGame(MasterGame)` inserts a `NewGame` row in the same transaction as the game.
- `EditMasterGame(MasterGame, changeLogEntries, MasterGameEditMessage?)` inserts the `Edit` row in the same transaction when the message is non-null.
- `AddPendingScoreUpdate(GameCriticScoreUpdateMessage)`.
- `GetPendingMasterGameUpdates()` returns a `PendingMasterGameUpdates` record: the three message lists plus the IDs read.
- `DeleteSentMasterGameUpdates(IReadOnlyList<Guid>)`.
- `ClearPendingMasterGameEdits()`, for the Clear button.
- `DeletePendingMasterGameUpdate(Guid)` returns false when the row is already gone, for the pending updates page in Step 3b.

Alternative: a separate insert after the edit commits. It's simpler, but a failed insert would leave the edit saved with its message lost, and the fact checker would see an error for an edit that went through.

### Message models (`Lib/Discord/Models`)
- `NewMasterGameMessage(MasterGame)`, unchanged.
- `GameCriticScoreUpdateMessage(MasterGame, decimal?, decimal?)`, unchanged. The game is the one from before the update, as today.
- `MasterGameEditMessage(MasterGame ExistingGame, MasterGame EditedGame, int Year, IReadOnlyList<string> Changes)`, instead of two `MasterGameYear`s, plus a `PreviousReleaseStatus` property that is `DiscordPushService.cs:162` verbatim: the existing game's status for `Year` if it differs from the edited game's, else null. `MasterGameYear.GetWillReleaseStatus()` is `MasterGame.GetWillReleaseStatus(Year)`, so this is the same value.

### Sending
`DiscordPushService` loses its three bags and its `Queue*` and `ClearMasterGameEditQueue` methods.
- `SendPendingMasterGameUpdates()` returns early if the bot is disabled, as today, and the rows wait. Otherwise it reads the pending updates, sends them, then deletes **the IDs it read**, so rows added during the send go out next time. A crash mid-send posts duplicates next time rather than losing messages.
- `SendMasterGameUpdates(newGames, scoreUpdates, edits)` is the current body from `GetAllCombinedChannels` down, verbatim except that `x.ExistingGame.MasterGame` becomes `x.ExistingGame` and so on.
- The spoof endpoints call `SendMasterGameUpdates` with one message, so they no longer touch pending rows.
- Step 3a replaces the delete after sending with recording `SentCount`, so sent rows are kept as history.

## Steps

Each step: build, test, commit alone, then stop for review.

### Step 0: Commit this plan

### Step 1: Table, snapshot and repository, with nothing calling them
- Migration `Sequential/<date>_000_pendingMasterGameUpdates.sql`: the lookup table with its three rows, and the pending table.
- `MasterGameSnapshotEntity`, and a unit test that a `MasterGame` with a raw score, an averaged-only score, sub-games and tags survives the JSON round trip field for field. The test project reaches `FantasyCritic.MySQL` through Web.
- `MasterGameEditMessage`'s new shape and the `PendingMasterGameUpdates` record.
- The new `IMasterGameRepo` methods in `MySQLMasterGameRepo`. `FakeMasterGameRepo` throws `NotImplementedException` for them, like its other unused members.
- The `Create`/`Edit` signature change waits for Step 2, so this step changes no behavior.

As built (d8e260493):
- The migration is `2026-09-29_000_pendingMasterGameUpdates.sql`. The FK is named `FK_tbl_discord_pendingmastergameupdate_type`, because the pattern name is over MySQL's 64-character limit. There's an index on `QueuedTimestamp` for the ordered read.
- The row is `PendingMasterGameUpdateEntity` (internal), with a constructor per message kind and the `PendingMasterGameUpdateType` TypeSafeEnum beside it in `MySQL/Entities`. The type is only a persistence discriminator, so it stays out of `Lib/Enums`.
- `InsertPendingMasterGameUpdate` is a private static helper that takes an optional transaction, so Step 2 only has to call it from `CreateMasterGame` and `EditMasterGame`.
- The new edit message shape meant a three-line change in `DiscordPushService`: `QueueMasterGameEditMessage` still takes two `MasterGameYear`s and converts them, and the send reads `ExistingGame`, `EditedGame` and `PreviousReleaseStatus`. The old code took each game's own year, but both callers pass the same year, so this is the same value.
- Tests:
  - The round-trip test compares the domain `MasterGame` serialized before and after, so a field the snapshot drops fails it. Checked by removing `OpenCriticSlug` for one run.
  - A missing tag throws.
  - `MasterGameEditMessageTests` cover `PreviousReleaseStatus`.
- The repository SQL has no caller yet, so it gets its first run in Step 2's local checks.

### Step 2: Write to the table and send from it
Writers and sender switch together; switching either alone would write rows nobody sends, or send rows nobody writes.
- Each message record gets a `PendingUpdateID` first (decided for Step 3b's page).
  - A second constructor without it assigns `Guid.NewGuid()`, so callers don't deal with IDs.
  - The entity uses the ID as the row's key, and rebuilds messages with it.
  - `PendingMasterGameUpdates` drops its separate ID list and gathers the IDs from its messages.
- `InterLeagueService.CreateMasterGame`: drop the queue call; the repo inserts the row.
- `InterLeagueService.EditMasterGame`: build the `MasterGameEditMessage` before the edit, under the same conditions as today (changes, a `MasterGameYear` row, not minor), and pass it, or null, to `EditMasterGame`. The `GetMasterGameYear` lookup moves above the edit. It only gates and supplies `Year`, which the edit doesn't change.
- `CriticScoreRefresher`: `AddPendingScoreUpdate` right after `UpdateCriticStats`, with no cancellation check between them.
- `DiscordPushService`: as in Sending above.
- `CacheRefresher`: call `SendPendingMasterGameUpdates`. The comment "Anything still queued here goes out with the next RefreshCaches in this process." becomes "...with the next RefreshCaches."
  - The cancellation check before the send stays. There is none between the send and the delete: a stop there would send the same updates again.
  - Reporting, following remaining-jobs: `SendPendingMasterGameUpdates` returns what it sent, and `CacheRefresher` adds it to its closing clause, e.g. `...; game stats for 2026; Discord game updates: 2 new, 5 scores, 1 edit.`, or `none pending`, or `bot disabled, nothing sent`. The disabled case gives no count, because counting would mean reading rows before the early return, and LocalDatabaseTool's `DiscordPushService` has no service provider to read them with.
  - The new log lines use structured properties, while the rest of `DiscordPushService`'s logging stays as it is.
- `AdminService.ClearMasterGameEditDiscordQueue` calls `_masterGameRepo.ClearPendingMasterGameEdits()` and becomes async, and so does its `FactCheckerController` action.
  - `AdminService` already injects `IMasterGameRepo`, and it keeps `DiscordPushService` for `SendActionProcessingSummary`, so its constructor doesn't change.
  - The API surface is unchanged (`IActionResult`), so NSwag regeneration should produce no diff; check that.
- The three spoof endpoints call `SendMasterGameUpdates` directly. Spoof edit passes the test game as both snapshots, with the current year.
- LocalDatabaseTool builds a disabled `DiscordPushService`, which returns before it reads any rows, so it needs no change.

As built:
- **`DiscordPushService`:**
  - The old send body is now the private `PostMasterGameUpdates`. Its parameters are named after the old fields, so the body's diff is just the dropped underscores and the dropped `Clear()` calls.
  - `SendMasterGameUpdates(newGames, scoreUpdates, edits)` is the public entry for the spoofs. `SendPendingMasterGameUpdates()` returns a `MasterGameUpdatesSendResult`.
  - With nothing pending, it skips the post, which saves loading every channel's league years every two hours for no messages.
- **Status wording:** `bot unavailable, nothing sent`, because `StartBot` returns false both when the bot is disabled and when it never becomes ready. Otherwise `none pending`, or `2 new games, 5 score updates, 1 edit`.
- **`InterLeagueService.GetEditMessage`** checks `minorEdit` before the `MasterGameYear` lookup, which gives the same result with one query fewer.
- **Dropped dependencies:** `InterLeagueService` and `CriticScoreRefresher` no longer take `DiscordPushService`. LocalDatabaseTool's `new InterLeagueService(...)` drops the argument; that's its only change.
- **Repo:** `CreateMasterGame` always queues a `NewGame` row. Only `InterLeagueService` calls it; the seeding tools insert games through `MySQLMasterGameUpdater`.
- **NSwag:** regenerating left the TypeScript client unchanged.
- **Committed as 120a2c95c.** All 298 integration tests pass, and they run against the Docker database.
  - Their five game creations each queued a `NewGame` row, so the insert inside the create transaction works against real MySQL. The stored snapshot is camelCase with ISO dates.
  - They queued no edits, because their edits hit the gate. The read, send and delete paths still need the local run below.
  - Those five rows are still pending in the Docker database. A worker with a bot token pointed at it would announce them.
- **Not run:** the worker's own start-up check. Step 2 only removes constructor dependencies, which can't make resolution fail.

After Step 1, two follow-ups, each committed alone:
- The deletes were renamed after their purpose (26d83ebe4): `DeleteSentMasterGameUpdates(ids)` deletes the rows a send read, and `ClearPendingMasterGameEdits()` is the Clear button.
- `DeletePendingMasterGameUpdate(Guid)` was added for Step 3b (f4478c2f2). It returns false when the row is already gone, following `IDiscordRepo.DeleteLeagueChannel`.

### Step 3a: Keep sent rows, with the number of channels each went to
Steve's addition. Sent rows stay in the table as history, for seeing which kinds of update reach the most channels.
- **Migration** `2026-09-29_001_pendingMasterGameUpdateSentCount.sql`: add `SentCount int NULL`.
  - Null means not sent yet. After a send, it's the number of channels the update went to, which can be 0 when no channel found it relevant.
  - The `QueuedTimestamp` index becomes `(SentCount, QueuedTimestamp)`, since the pending read is now `WHERE SentCount IS NULL ORDER BY QueuedTimestamp`.
- **Counting, in `PostMasterGameUpdates`.** It returns a count per update ID, with every ID it was given present, even at 0. For each channel it actually posts to (a text channel it found, with at least one message), each update that contributed text counts once:
  - A new game counts wherever it was relevant.
  - An edit counts wherever it was relevant.
  - A score update counts only if it produced a line. When one game has several score updates in a batch, only the last is shown, so the earlier ones count 0. A change under 1 point also shows nothing, so it counts 0.
- **Repo:**
  - `DeleteSentMasterGameUpdates(ids)` becomes `MarkMasterGameUpdatesSent(IReadOnlyDictionary<Guid, int> sentCounts)`: one `UPDATE ... SET SentCount` per row, in one transaction.
  - `GetPendingMasterGameUpdates` reads only `SentCount IS NULL`. Old rows are never rebuilt, so a tag deleted long after a send can't break a later one.
  - `ClearPendingMasterGameEdits` and `DeletePendingMasterGameUpdate` gain `AND SentCount IS NULL`, so neither can erase history. `DeletePendingMasterGameUpdate` then returns false for an update that was already sent, as well as for one already deleted.
- `SendMasterGameUpdatesImmediately` ignores the counts; the spoof messages are never stored.
- **Crash semantics** are unchanged: counts are written after the posting, so a crash mid-send leaves the rows null and they go out again next time.
- **Decision: posted or delivered?** Posting goes through `TrySendMessageAsync`, and `RateLimitMessages` returns only a total failure count, which this path ignores today.
  - Recommended: count channels posted to, and log a warning with the failure count. Failures are rare, and the stats are about relevance.
  - Alternative: count only confirmed deliveries, which needs a per-message result from `RateLimitMessages`, a shared utility.
- **Decision: rename the table?** Once it keeps history, "pending" is only half right. The same migration could rename it to `tbl_discord_mastergameupdate`, with the entity and repo names to match (`PendingUpdateID` → `MasterGameUpdateID` and so on). Nothing is deployed yet, so this is the cheapest time.
- **Decision: add `SentTimestamp`?** `QueuedTimestamp` stays. A sent time would show how long updates waited, but the stats above don't need it.

### Step 3b: A page of pending updates
An admin console page lists what is waiting to go to Discord, and deletes one update that was queued by mistake. It replaces the earlier idea of a one-line pending count. It shows unsent rows only; a stats view of sent rows can come later.
- **Listing with IDs.** Decided: each message record carries its `PendingUpdateID`, added in Step 2. The page reads the same `GetPendingMasterGameUpdates` the sender does. The spoof endpoints' messages get IDs that are never stored, which is harmless.
  - The rejected alternative was a second read returning one summary per row. It would have meant two read paths, and moving `PendingMasterGameUpdateType` into Lib.
- **Who: fact checkers.** The endpoints go on `FactCheckerController`, beside the Clear button, since fact checkers make the edits this page corrects.
- **API.** List and delete endpoints with `[ProducesResponseType<T>]`, then regenerate NSwag. A delete that finds nothing returns an error that says the update was already sent or deleted.
- **Page.**
  - One row per update: queued time, kind, the game as a link, and the details (old → new score, or the change strings).
  - A delete button per row. The "Clear Edit Game Discord Queue" button could move here too.
  - Site-consistent Bootstrap, like the rest of the admin console.
- **Race.** Deleting a row that a running send has already read doesn't stop that send. The update posts, and then there's no row left to record its count on. The window is the length of one send, and the page doesn't need to guard against it.

## Open questions
- **Rows while the bot is disabled.** Locally, with no bot token, rows pile up, much as the in-memory bags did. They're harmless, but a local run with a token set would then send the backlog.
- **Beta restores from prod snapshots.** Prod's pending rows arrive with the restore. Beta's bot only reaches guilds it's in, so this is at most a few duplicates in a guild both bots share. `TestDataScrubber` could truncate the table if that matters.

## Found along the way, not changing
- **Edit relevance checks the pre-edit game.** `ExistingGameIsRelevant` gets the pre-edit game *and* its pre-edit status as `prevReleaseStatus`, so the override in `IsReleaseStatusRelevant` adds almost nothing. A game moving *into* the year is filtered out of channels that hide will-not-release news. Passing the edited game looks like the intent (594f3f794 / f551b13b9, January 2023). Both snapshots are stored, so fixing it later needs no schema change.
- **Edits write the averaged score as the raw score.** `EditMasterGame` and `CreateMasterGame` save `new MasterGameEntity(masterGame)`, whose `CriticScore` is the computed one. A game with no raw score and scored sub-games gets their average written to `tbl_mastergame.CriticScore` on its next edit, and it stays frozen there.
- A merged-away game's pending update still sends, linking to a game that no longer exists, as it would from memory today.

## Verification
- Every step: `dotnet build src/FantasyCritic.slnx` with zero warnings, `dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj`, and `scripts/Format.ps1 -Check`.
- Step 1: the migration runs against Docker MySQL (`docker compose -f infrastructure/docker-compose-mysql.yaml up`); the snapshot round-trip test passes.
- Step 2:
  - Integration tests. `InterLeagueService` and `DiscordPushService` change, and the test host validates DI on build.
  - Start the worker locally once for its own ValidateOnBuild.
  - Locally, create a game and make a non-minor edit through the fact checker UI, and confirm the rows appear.
  - Run RefreshCaches from the admin console. With a local bot token, confirm the messages post and the rows go. Without one, confirm the rows stay.
  - Run each spoof endpoint and confirm pending rows are untouched.
  - Clear Edit Game Discord Queue removes only edit rows.
- Step 3a: unit tests for the per-update counts, if `PostMasterGameUpdates`' channel loop can be separated from the Discord client; otherwise review. Locally, run RefreshCaches with a bot token, and confirm the rows stay with `SentCount` set and the next run finds nothing pending. Clear Edit Game Discord Queue leaves sent edits alone.
- Step 3b: integration tests for the list and delete endpoints, through the generated client. Locally, queue an edit, see it on the page, delete it, and confirm RefreshCaches doesn't send it. Deleting it a second time reports it's already gone.
- Local data is never edited by hand to set up a test.
