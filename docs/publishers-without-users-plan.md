# Publishers with no user: replacing Remove Player / Remove Publisher

## Context

Today "Remove a Player" and "Delete A User's Publisher" both go through `FullyRemovePublisher`. It hard-deletes the publisher's games, former games, league actions, bids, drops, queue and statistics, and nulls it out of trades. "Remove a Player" then has two paths:
- **"Removable" players** (no publisher in any year where a draft has started): it also deletes them from every year.
- **Everyone else:** it deletes their publisher in the most recent year.

This is how managers wiped players out of league history when those players just weren't returning. It's also why the warnings and the secret phrase exist.

The two real needs are:
- removing a pre-draft no-show completely
- cutting off a disruptive player mid-year

Neither should destroy history, and both should leave competitive balance alone unless the manager explicitly chooses otherwise.

### New model
- **Manage Active Players** and **Reassign Publisher** stay as they are.
- **`Publisher.User` becomes nullable.** A new manager action, **Disconnect Player**, detaches a user from their publisher in the current year:
  - the user is marked inactive in that year;
  - their pending bids, drop requests and queue are cancelled, and their open trades become `RejectedByManager`;
  - the publisher stays in the league with all of its history.
- **Remove Publisher** works only on a publisher with no user, in a year that isn't finished. After the draft it warns that this is *not recommended*: unreleased games become available for pickup. The choice stays with the manager.
- **Remove Player from League** works only for a member with no publisher in any year. Nothing is lost, so there are no warnings.
- There is **no way to remove a publisher from a finished year**. The `Removable` concept and the secret phrase are deleted.

### Decisions already made
- **Pending activity:** cancel all of it when the player is disconnected.
- **Drafts:** a publisher with no user is skipped automatically, using the existing auto-skip mechanism (`DraftFunctions.ShouldSkipPublisher` → `PicksToSkip` → `DraftService.PersistAutoSkips`).
- **Season winner:** if a publisher with no user finishes first, the winner is the top publisher that has a user. `LeagueYear.GetWinningPublisher` is the one rule, and every place that shows a winner follows it: the recorded winner, the league page's "The winner is" banner and standings highlight, the conference standings highlight, and the trophy in the Discord final standings. A publisher with no user keeps its rank everywhere.
- **No-show flow:** Disconnect → Remove Publisher → Remove Player from League.
- **Removing a disconnected player** (found in Step 7): a disconnected publisher no longer carries its player's ID. So a player whose only publisher was disconnected can be removed from the league right away, and their publisher stays. A player with a publisher in any other year can't be removed.
- **A publisher with no user stays fully visible** (settled in Step 3):
  - **League standings:** it has its own row, shown as "User Disconnected", and is ranked with everyone else. It can top the standings, but it can't win.
  - **League page:** it stays in `leagueYear.publishers`.
  - **Consolidated export:** included in full, with a null user.
  - **All-time stats:** it appears in the publishers table, but not in the per-player stats, which are grouped by player.

## Workflow

- Work on a new branch off `main`, e.g. `publishers-without-users`.
- Commit this plan to `docs/publishers-without-users-plan.md` first. Commit it again whenever it's revised.
- Do one step at a time: build, test, commit, then stop for review.

---

## Step 1: Delete the old removal system (Claude)

Be aggressive; rewriting later is fine. The step ends with the solution building with zero warnings, unit tests passing and the NSwag clients regenerated.

**Backend: delete**
- In `Web/Controllers/API/LeagueManagerController.cs`, delete the `RemovePlayer` (:406) and `RemovePublisher` (:565) endpoints.
- Delete `Web/Models/Requests/LeagueManager/PlayerRemoveRequest.cs` and `PublisherRemoveRequest.cs`.
- In `Lib/Services/LeagueMemberService.cs`, delete `FullyRemovePlayerFromLeague`, `RemovePlayerFromLeagueYear` and `GetUsersWithRemoveStatus`.
- In `Lib/Services/PublisherService.cs`, delete `FullyRemovePublisher`.
- Delete `IFantasyCriticRepo.FullyRemovePublisher` and `MySQLFantasyCriticRepo.FullyRemovePublisher` (:1872). Step 6 rewrites the deletion fresh.
- **The `Removable` concept:**
  - delete `Lib/Identity/FantasyCriticUserRemovable.cs`
  - delete `DomainConversionUtilities.ConvertUserRemovableEntities` (:297)
  - delete `GetUsersWithRemoveStatus` (repo and interface)
  - delete `PlayerViewModel.Removable`
  - replace every `IReadOnlyList<FantasyCriticUserRemovable> PlayersInLeague` with `IReadOnlyList<FantasyCriticUser>`. That covers `Web/Helpers/HelperRecords.cs`, `CombinedLeagueYearUserStatus`, `LeagueViewModel`, `ConsolidatedLeagueDataViewModel` and `BaseLeagueController:47`.
- **Stored procedures:**
  - `sp_getusersinleague` and `sp_getcombinedleagueyearuserstatus` both have a result set for years plus `AnyDraftStarted` and one for publisher `UserID`/`Year`. These exist only to compute `Removable`, so delete them.
  - Adjust the readers in `MySQLFantasyCriticRepo` (:1479 area) and `MySQLCombinedDataRepo` (:368, :534).
  - If nothing else needs it, `sp_getusersinleague` then collapses to a `GetUsersInLeague` equivalent and can be deleted. Check whether an idempotent proc that is no longer needed must be dropped explicitly.
- **Keep:**
  - `RemovePlayerFromLeague`, which the admin test-league `DeleteLeague` still uses.
  - `ReassignPublisher`.
  - `SetPlayerActiveStatus`, though its guard message "You must remove a player's publisher…" (:615) becomes "…disconnect the player from their publisher…".

**Tests: delete**
- `IntegrationTests/Tests/League/MultiDraft/MultiDraftRemovePublisherTests.cs`. Its renumbering test is rewritten in Step 6.
- In `MultiDraftReadTests.cs`, delete `Players_AreRemovable_WhenNoDraftHasStarted` and `Players_AreNotRemovable_AfterADraftHasStarted`.

**Frontend: delete**
- Delete `components/modals/removePlayerModal.vue` and `removePublisherModal.vue`.
- In `components/leagueActions.vue`, delete their imports (:258, :260), their component registrations, the mounted modal tags, and the menu items "Delete A User's Publisher" and "Remove a Player".
- Delete any client reads of `player.removable`.

Then run `dotnet build`, `scripts/Regenerate-ApiClient.ps1`, `npm run lint`, the unit tests, and the integration tests that still exist.

## Step 2: Migration to make `UserID` nullable (Claude)

- Add `DatabaseUpdater/Scripts/Sequential/2026-10-04_000_publisherUserNullable.sql`, modelled on `2026-09-18_000_mastergameyearNotesNullable.sql`:
  `ALTER TABLE tbl_league_publisher MODIFY COLUMN UserID char(36) NULL;`
- The existing keys can stay as they are:
  - `Unique_League_Year_User` allows multiple NULLs.
  - `FK_tblpublisher_tblleaguehasuser (LeagueID, UserID)` is not enforced on NULL rows. This is what lets a disconnected user's `tbl_league_hasuser` row be deleted later.
- Verify against Docker MySQL (`docker compose -f infrastructure/docker-compose-mysql.yaml up`). If MySQL refuses to modify a column that is in a foreign key, the script drops and re-adds the FK around the `MODIFY`.
- No code changes in this step. Nothing writes NULL yet, so it's safe to deploy alone.

## Step 3: Make `Publisher.User` nullable and get it compiling (Steve leads)

Steve makes `Publisher.User` a `FantasyCriticUser?` and works through the compiler errors. Claude reviews and helps on request. Below are Claude's thoughts on the decision sites, beyond the mechanical `?.` fixes.

**Persistence**
- `PublisherEntity.UserID` becomes `Guid?` (:21, :37, :46).
- `DomainConversionUtilities.cs:65` and the single-publisher load `MySQLFantasyCriticRepo.cs:2225` need null-aware lookups.
- `ReassignPublisher` repo (:1847): skip the "set old user inactive" statement when there is no old user. This makes Reassign the way to re-attach a publisher with no user to a member.
- `CreatePublisher` insert (:1958) never writes NULL.

**Display**
- Change `Publisher.GetPublisherAndUserDisplayName()` once; that fixes about 30 callers (DraftService, Discord, view models). Suggested text: `"{PublisherName} (No Player)"`.
- `PublisherViewModel`, `PlayerPublisherViewModel` and `AllTimeStatsPublisherViewModel`: `UserID` becomes `Guid?`, and `PlayerName` gets a fallback, or becomes `string?` so the client chooses the text.
- Trade view model user IDs and names become nullable.

**Standings rows**
- `LeagueYearViewModel.cs:56-72` and `ConsolidatedLeagueDataViewModel.cs:51, 81-93` build one row per active user, so a publisher with no user disappears from the standings.
- Suggest building rows from the publishers plus the active users without a publisher, instead of from users alone.
- On the client, `user == null` currently means "invite sent". The row needs a distinct state, e.g. add a `PlayerWithPublisherViewModel` flag, or check `!user && publisher` in `leagueYearStandings.vue`.

**Authorization**
- `BaseLeagueController.cs:257, :290` and `LeagueYear.GetUserPublisher` (:185) compare user IDs. With `User?.Id` they fail closed, which is correct.
- Trade proposer/counter-party checks (`LeagueController.cs:1773-1936`) and `Trade.UserIsInvolved` behave the same way.
- Trade vote eligibility should skip publishers with no user.

**Drafts**
- `DraftFunctions.ShouldSkipPublisher` (:445): return true when `publisher.User is null`. The existing auto-skip then handles it, including the league action "Draft Pick Skipped".
- `DraftFunctions.cs:9, :42` require publisher count == active user count. Change this to "every active user has a publisher". Publishers with no user are allowed on top.
- `DraftFunctions.cs:226` orders the draft from last year's standings by user. Skip last year's publishers that have no user.
- `DiscordPushService.cs:1546-1552`: don't ping a user for the next pick when the next publisher has no user.

**Scoring and stats**
- Winner: in `FantasyCriticService.cs:294`, take the top-ranked publisher with `User is not null`.
- `AllTimeStatsService.cs:54` groups by user. Exclude publishers with no user, since all-time stats are per player.
- `GameUtilities.cs:36` `GetTimesCounterPicked`: compare `PublisherID`, not user.
- Conference code (`ConferenceService.cs:134, 158, 334`; `MySQLConferenceRepo.cs:163, 645`): treat a publisher with no user as nobody's.

**Discord**
- Name-change message `DiscordPushService.cs:588`, trade mentions `:1013, :1020`, and the "my publisher" lookups in `PublisherCommand` and `TopAvailableGamesCommand`: handle null; most just fall through.

**Client**
- Update ownership checks only where they could crash: `publisherMixin.userIsPublisher` and `minimalPlayerGameTable` are fine, because comparing to a null `userID` is false.
- `tradeSummary.vue:204` maps `leagueYear.players` to `x.user.userID` and throws on invite rows. This bug exists today; fix it here.
- Show a fallback wherever `publisher.playerName` is displayed.

When the solution builds with zero warnings and unit tests pass, pick the plan back up.

---

## Step 4: Disconnect Player action, backend and modal (Claude)

**Backend**
- `LeagueManagerController.DisconnectPlayer`, with request `DisconnectPlayerRequest(Guid PublisherID)`. Use `GetExistingLeagueYearAndPublisher` with `ActionProcessingModeBehavior.Ban`, `RequiredRelationship.LeagueManager` and `RequiredYearStatus.YearNotFinishedNoDraftsActive`, the same as Reassign.
- Reject a publisher that already has no user, and reject the manager's own publisher (the manager can transfer the manager role first).
- `PublisherService.DisconnectPlayer` → one repo transaction (`MySQLFantasyCriticRepo.DisconnectPlayer`) that:
  - sets `UserID = NULL`
  - deletes the user's `tbl_league_activeplayer` row for that year
  - deletes the publisher's pending `tbl_league_pickupbid` and `tbl_league_droprequest` rows and its `tbl_league_publisherqueue` rows
  - sets the publisher's open trades (`Proposed`/`Accepted`) to `RejectedByManager`
  - inserts a manager `LeagueAction`: "Player {name} was disconnected from {publisher} by the league manager."
- Check the existing manager-reject trade flow and reuse its repo call if it has side effects such as Discord or vote cleanup.
- Optionally send a Discord push, matching other manager actions.

**Frontend**
- New `disconnectPlayerModal.vue`, following the `reassignPublisherModal.vue` conventions:
  - publisher picker that lists publishers with a user
  - one explanatory `alert-warning`
  - `errorInfo` shown at the top
  - `notifyAction`
  - a plain confirm, no typed phrase
- Add a menu item in `leagueActions.vue` under Player Management, visible when `!leagueYear.supportedYear.finished`.
- In `leagueYearStandings.vue`, the finished-year row highlight and the bold ranking go to the top publisher by points (`topPublisher`). When that publisher has no user, the highlight should go to the top publisher that has a user instead (`topPublisher` in `leagueMixin.js` skips publishers with no user).

**Tests**
- Integration tests: disconnecting mid-season and pre-draft, the user becoming inactive, pending bids and trades being cancelled, the former owner getting 403 on publisher actions, and Reassign re-attaching the publisher afterwards.
- A unit test for `ShouldSkipPublisher` with a publisher that has no user.

## Step 5: Reassign accepts publishers with no user (Claude, small)

- Check that `PublisherService.ReassignPublisher` and the repo branch from Step 3 work with a publisher that has no user.
- `reassignPublisherModal.vue` already lists every publisher. Update its wording so that re-attaching a publisher with no user is an obvious use.
- Integration test: disconnect, then reassign to an inactive member.

## Step 6: Remove Publisher, publishers with no user only (Claude)

**Backend**
- `LeagueManagerController.RemovePublisher(RemovePublisherRequest(Guid PublisherID))`, with `RequiredYearStatus.YearNotFinishedNoDraftsActive`. Reject publishers that still have a user.
- Write a new repo deletion, modelled on the old one but written fresh:
  - delete games, former games, actions, statistics, draft-publisher rows and any remaining bids, drops and queue rows
  - null the publisher out of trades
  - renumber draft positions in every draft
- After this, finished years can't be reached by any removal path.

**Frontend**
- `removePublisherModal.vue`, rewritten:
  - the picker lists only publishers with no user
  - before the draft: a plain confirm
  - after the draft: an `alert-warning` saying this is not recommended, because its unreleased games become available for pickup and that changes the league's balance; the publisher can stay as-is safely
  - menu visibility: year not finished and some publisher has no user

**Tests**
- Integration tests:
  - rejected when the publisher has a user
  - removal before and after the draft
  - multi-draft renumbering, rewriting the deleted `RemovePlayer_MultiDraftLeague_RenumbersAllDrafts`

## Step 7: Remove Player from League, no publishers in any year (Claude)

**Backend**
- `LeagueManagerController.RemovePlayerFromLeague(RemovePlayerFromLeagueRequest(Guid LeagueID, Guid UserID))`.
- The service checks three things: the player is not the manager, the user is in the league, and the user has no `tbl_league_publisher` row in any year. A small repo query is enough; this replaces the old Removable calculation.
- Reuses `RemovePlayerFromLeague` (the `hasuser` and `activeplayer` deletes).

**Frontend**
- New `removePlayerModal.vue`:
  - the picker lists league members who aren't the manager
  - if the chosen player still has publishers, explain the order: Disconnect, then Remove Publisher (current year only)
  - otherwise a plain confirm

**Tests**
- Integration tests:
  - a no-show is removed before the draft
  - a player with history in a past year is rejected
  - the full no-show flow: disconnect, remove publisher, remove player

---

## Verification (each step)

- `dotnet build src/FantasyCritic.slnx` with zero warnings.
- `dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj`.
- For API changes: build Web, then `scripts/Regenerate-ApiClient.ps1`.
- With Docker MySQL up: `dotnet test src/FantasyCritic.IntegrationTests/... -c Release`.
- `scripts/Format.ps1 -Check`.
- Steps 4–7: run the app (`dotnet run --project src/FantasyCritic.Web/...`) against Docker. In a test league, walk through disconnect → standings show the publisher with no user → a later draft skips it → reassign or remove publisher → remove player.
