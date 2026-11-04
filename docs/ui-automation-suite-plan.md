# UI automation suite (Playwright .NET)

## Context

The Vue 3 migration (`vue3-migration`, 45 commits ahead of `main`, and `main` hasn't moved
since it branched) has reached the write-heavy pages: league, draft, modals, conference. So far
pages have been checked by hand or with throwaway headless-Chrome scripts in a scratchpad. A
committed browser suite, written and proven green against the Vue 2.7 site on `main`, gives a
baseline. Run on `vue3-migration`, it shows what broke. It covers the basics plus the areas
most likely to break in the migration: modals and their footers, custom `v-model` components,
`@change`/`@input` payloads, sortable/filterable/paginated tables, popovers, dismissible
alerts, `$bvModal` confirms, drag and drop, toasts, vee-validate forms, SignalR draft updates,
charts and router behaviour.

Decisions taken with Steve:
- **Playwright .NET** in a new NUnit project, `FantasyCritic.UiTests`. It hosts Web on real
  Kestrel through `WebApplicationFactory.UseKestrel()` (in Mvc.Testing 10.0.5). Data is set up
  with the typed C# API client and the existing fixture builders, then the browser drives the UI.
- **Shared test code moves to a new library**, `FantasyCritic.IntegrationTestSupport`, which
  both test projects reference.
- **What fails a test:** uncaught page exceptions, `console.error` and 5xx responses from
  `/api`. Vue, compat and bootstrap-vue-next warnings are collected into a report but don't fail
  tests.
- **Scope:** the basics, league creation and settings, draft, post-draft player and manager
  actions, conference pages, Critics Royale, and the master game request forms. The admin pages
  are out.

Principles:
- **The suite never touches `src/FantasyCritic.ClientAppVue2`.** The same tests run unchanged
  on both branches, and merging `main` into `vue3-migration` stays conflict-free (the branch has
  no changes outside the client app).
- **Locators use ARIA roles, labels, visible text and the existing static `id`s** (modal ids,
  `#leagueName`, `#searchGameName`, …). No Bootstrap class selectors, since BS4→BS5 renames them.
  Where one can't be avoided, it lives in one helper (`Ui.cs`), so it's fixed in one place. No
  `_uid`-based ids. No `data-testid`, since adding them on main would mean client edits.
- **A test is done once it passes on `main` three runs in a row.** A test that's only green
  sometimes is worse than none for telling migration breakage from flakiness.

## Architecture

- **`FantasyCritic.IntegrationTestSupport`** (class library) gets the factory,
  `CapturingEmailSender`, `NullEmailSender`, `TestRemoteAddressStartupFilter`,
  `UnusedDatabaseDumper`, `AntiForgeryHelper` and everything in `IntegrationTests/Helpers/`
  (`ApiSession`, `LeagueFixtureBuilder`/`LeagueFixture`, `LeagueScenario`, `MockedLivePlayer`,
  `LeagueTestHelpers`, `LeaguePickupActions`, `SlotTestHelpers`, `JobTestHelpers`, …). These go
  from `internal` to `public`, keeping their namespaces, so existing tests only gain a project
  reference. `IntegrationTestBase` stays in IntegrationTests. Each test project keeps its own
  `appsettings.json` content link.
- **The factory** gains an option to start on Kestrel at `http://127.0.0.1:0`. Plain HTTP is
  fine: in Development the auth cookie is `SameAsRequest`. `CreateClient()` then talks to the
  real port, so `ApiSession` works unchanged against the server the browser uses.
- **Serving the SPA:** the suite tests the **production build** (`npm run build` → the client
  app's `dist/`). That's the bundle that ships, and it's the only one where charts draw. The UI
  factory adds a startup filter that serves `dist/` as static files before the existing
  pipeline, and points `MapFallbackToFile("index.html")` at `dist/index.html` by giving the
  environment a `CompositeFileProvider(dist, wwwroot)` web root. The fixture fails loudly when
  `dist/index.html` is missing, or older than the newest file under the client's `src/`, with
  the command to run.
- **Browser:** the installed Google Chrome (`Channel = "chrome"`), so there's no browser
  download. Headless by default; `UI_TESTS_HEADED=1` shows it. One browser per run (in a
  `[SetUpFixture]`), one factory per fixture (as `IntegrationTestBase` does now), and a new
  `BrowserContext` per user per test. Multi-user tests (manager plus player watching a draft)
  open two contexts.
- **Sign-in:** `/dev/sign-in-as?userId=…` in each context. The factory meets all four guards:
  Development, a localhost DB, loopback, and `AllowSignInAs=1` from the seed. Users come from
  `ApiSession.RegisterAsync` (`CapturingEmailSender` confirms them). One test uses the real
  `/Account/Login` form.
- **Clock:** the server runs on `AdjustableClock`. Where the client computes from "now"
  (pending draft callouts, royale quarters), `page.Clock` is set to the same instant.
- **`UiTestBase`** gives each test: `NewSignedInPage(user)`, console/pageerror/response capture
  that fails the test in `TearDown`, a Playwright trace and full-page screenshot saved to
  `TestResults/ui/<test>/` on failure, and warnings appended to
  `TestResults/ui/console-warnings.md`, grouped by message with the pages and tests where each
  appeared.
- **`Ui.cs` helpers:** `Modal(page, title)` (role `dialog` by accessible name), `SidebarAction(page,
  text)`, `Toast(page)`, `TableRows(table)`, `ExpectNoJsSource(locator)` and the like, so tests
  read as user actions.
- **Data** goes into the shared Docker MySQL (`fantasycritic` on :3307), with fresh GUID users
  and leagues per test, as the integration tests do. It needs
  `docker compose -f infrastructure/docker-compose-mysql.yaml up` first.
- **`scripts/Run-UiTests.ps1`:** regenerate the API clients, `npm run build`, then
  `dotnet test src/FantasyCritic.UiTests -c Release`, passing through `-Filter` and `-Headed`.
  The suite isn't in CI, like the integration tests: no MySQL service there.

## Steps

One commit per step, then stop for review.

### 0. Commit this plan as `docs/ui-automation-suite-plan.md` — done (15028d0de)

### 1. Extract `FantasyCritic.IntegrationTestSupport` — done (f547ddfe9)
Move the files listed above, make them public, add the project to `FantasyCritic.slnx`
(`/Tests/`), and reference it from IntegrationTests. No behaviour change. Verify:
`dotnet build src/FantasyCritic.slnx` with zero warnings, and the integration tests pass in
Release. Any failures get checked against `_schemaversion` for drift from other branches.

Verified: zero warnings; 365 integration tests pass; `_schemaversion` matches the branch.

### 2. UiTests skeleton — done
Project (`Microsoft.Playwright`, NUnit, TreatWarningsAsErrors), Kestrel option on the factory,
`dist/` serving, `UiTestBase`, `Ui.cs`, the console report, `Run-UiTests.ps1`, and two tests:
the logged-out welcome page renders, and a signed-in user lands on `/home`. Also a deliberate
check that a thrown page error fails a test. Update CLAUDE.md's Commands and project table.
Verify: green three times; a trace appears when a test is forced to fail.

As built:
- `UiWebApplicationFactory` subclasses the shared factory, which is no longer sealed. It
  listens on `127.0.0.1:0` and reports `http://localhost:<port>`, a host Web's redirect-to-www
  rule leaves alone.
- **Sessions need their own client on Kestrel.** The factory's `CreateClient` ends in a socket
  handler that follows redirects itself whatever `AllowAutoRedirect` says, so `RegisterAsync`
  saw the SPA's `index.html` instead of its 302. The shared factory gained a virtual
  `CreateSessionClient()`, which `ApiSession` now uses. The UI factory overrides it with a
  `SocketsHttpHandler` that doesn't follow redirects and has its own cookie container.
- **Failures:** Chrome's `Failed to load resource` console errors are ignored, since the API
  returns 401 and 400 by design (every logged-out page's `currentUser` is a 401). What fails a
  test is a 5xx from anything, or a 4xx from a same-origin non-API path, such as a missing
  asset or a sign-in-as 404. Page exceptions and other console errors fail as planned.
- `Ui.cs` waits for step 3, since nothing uses a helper yet.
- **Vue's production build strips its warnings.** On main the warnings report reads "None".
  That's expected, and it will be true of `@vue/compat`'s deprecation warnings too. So for
  step 10's compat checklist, build the client with `NODE_ENV=development` (a dev-mode bundle)
  or read the dev server's console.
- The tests are `Tests/HarnessTests.cs`. The third checks that both a `console.error` and an
  uncaught exception are caught, then clears them so the test passes.

Verified: three green runs (about 4 s for the three tests, plus about 10 s of client build); a
stale `dist/` stops the run in setup, naming the newer file; a broken assertion leaves a
full-page screenshot and a trace.

### 3. Public and read-only pages
- **Route smoke:** each public and logged-in read-only route (static pages, `/games`,
  `/gameChanges`, `/mostDesiredReviews`, `/advancedGameStats`, `/publicLeagues`,
  `/topBidsAndDrops`, `/mastergame/:id`, plus a fixture league's `/league`, `/leagueHistory`,
  `/leagueAllTimeStats` and `/publisher/:id`) renders its heading with no errors, at 1280 and
  375 wide.
- **Router:** an unknown path goes to `/404`; `/longestTenuredGames` redirects and scrolls to
  its section; logged-out `/home` goes to `/Account/Login`; logged-in `/` goes to `/home`;
  browser back/forward.
- **Nav:** links logged out and in, the user dropdown, and the mobile toggler opening and
  closing the collapse.
- **Master Games:** sorting each numeric column both ways gives numeric order (decimals
  included), the text filter narrows rows and the page count, paging works, the game popover
  opens with the game's details, and the tag popover opens.
- **Public Leagues:** the year select changes the list; search plus a year change keeps the
  page count right.
- **Advanced Game Stats** year select and checkbox, the **Game Changes** filter, and on
  **Master Game detail** the year menu, popovers whose text is never JavaScript source, and
  the stats chart canvas having drawn pixels.
- **Login:** the real `/Account/Login` form, then the SPA shows the user in the nav.

**Done.** 92 tests in `Tests/ReadOnly/` (91 pass, 1 skipped for a bug on main), about 2 minutes
for the whole suite.

As built:
- **Planted data.** The local database is too flat for some checks: every hype factor is 0,
  only the current year has master games, and there are no year stats, desired reviews or
  public leagues of known shape. Those tests answer the page's API call with JSON the test
  builds (`Ui.PlantJsonAsync`), usually starting from a real response so the rows keep their
  real shape. This checks how the client renders and handles data, which is what the
  migration changes. Planted tests are: hype factor sort (91.0912 and 91.56 together, so a
  string comparison would order them wrongly), the sort selects, the filter, the game popover,
  Public Leagues (12 leagues this year, 3 in 2025), the unannounced-games checkbox, Most
  Desired Reviews' release windows, and the Master Game year stats.
- **Locators** are roles, labels and text throughout. The exceptions, all in tests: `tbody tr`
  for rows, the info icon's `svg` and the chart's `canvas`. `Ui.cs` has the shared ones,
  including `Popover` (the tooltip role, which both libraries use) and `ColumnTextsAsync`.
- **What it doesn't check:** the `/games` "Show only unreleased games" checkbox and the Plus
  league selector. Top Bids and Drops has no local data beyond its heading. Hover states.
- **Known problems** (`KnownProblems.cs`) don't fail tests but are listed in
  `console-warnings.md`. Both are on main, outside the client migration:
  - in Development the Razor pages load `/lib/jquery-validation*`, which isn't in the
    repository (production uses a CDN), so every login page 404s twice;
  - logged out, `/games` logs an `AxiosError` 401, because it asks for the user's leagues.

Found on main while writing these, and left alone:
- Loading `/longestTenuredGames` directly ends at `/404` instead of following its redirect.
  `OldLongestTenuredPathRedirectsToItsSection` is `[Ignore]`d with that reason.
- Logged out, `/404` and `/longestTenuredGames` go to the login page, because neither route is
  public, so the router tests sign in first.
- Public Leagues' pager counts every league for the year, whatever the search shows
  (`:total-rows="rows"`, the unfiltered length). The tests don't assert page counts under a
  search.

### 4. League creation and settings
- `/createLeague` for each game mode (Standard, Multi Draft, One Shot). The preset fills the
  settings. In Multi Draft, "Add Another Draft" and "Remove Draft" work and the date inputs keep
  their values. The advanced settings collapse opens. Out-of-range standard games disables
  Create. Submitting lands on the league page with the chosen settings.
- `/editLeague`: existing values load into every field; changing them, including the
  "unlimited" checkboxes, saves and survives a reload.
- `/manageDrafts`: add, edit and delete a draft.
- Manager message: post it, it shows as a dismissible alert, dismiss it, and it stays dismissed.
- Change league options; the league year select switches years; Follow/Unfollow; Copy League
  ID puts the ID on the clipboard (with the clipboard permission granted); the unfollow confirm
  on the home league table.

**Done.** 12 tests in `Tests/League/`: `CreateLeagueTests` (5), `LeagueSettingsTests` (2) and
`LeagueHeaderTests` (5). The whole suite is 103 passing and 1 skipped, in about 3 minutes.

As built:
- `UiTestBase.NewLeagueAsync` makes a league per test through `LeagueFixtureBuilder` and disposes
  it afterwards. Created leagues are Test Leagues, so they stay out of the site's stats.
- `editLeague` checks the settings round trip: standard games, the "Will Release" unlimited
  checkbox (whose number input then goes away) and "Will Not Release", all reloaded. The
  unlimited checkboxes have no label, so the test finds them by their table row.
- `manageDrafts` labels have no `for`, so `Ui.InputAfterLabel` finds the input after the label
  text.
- The year select test plants a previous year onto the league, since a league only gets a second
  year once the next one opens.
- The "Advanced Settings" toggle is an `h3`, but bootstrap-vue's `v-b-toggle` gives it the button
  role, so the test finds it by its text.

Found on main while writing these, and left alone:
- **The league year name is lost when the same edit changes a setting.**
  `FantasyCriticService.EditLeagueYear` writes the name through `ChangeLeagueYearName` only in
  the branch where nothing else changed; the `EditLeagueYear` UPDATE doesn't include
  `LeagueYearName`. The test saves the name in an edit of its own.
- **`draftStatusHeader.vue` throws for a league member with no publisher**, as on a league
  that was just created: `userHasAutoDraft` is true when `userPublisher` is undefined, and the
  template then reads `userPublisher.autoDraftMode`. It's in `KnownProblems.cs`.
- `createLeague.vue`'s year label says `for="intialYear"`, so it doesn't label the select.

### 5. Pre-draft and draft
- Invite by email (an invalid address disables the button); the invitee, in a second context,
  accepts. An invite link joins through `?inviteCode=`.
- Create publisher (the modal's own footer button submits), then change its name.
- Draft order: manual by drag and drop, saved, and the banner shows that order; Random.
- Start the draft. The player's page shows their turn without a reload (SignalR).
- Draft a game: search, Select a row, submit. The toast appears, and the other context's page
  updates. Draft from Top Available and from the watchlist.
- Counter pick phase (`DraftUntilCounterPickPhaseAsync`): the counter pick form.
- Auto draft: change the mode, save, and reopening shows it saved; the manager's per-player auto
  draft.
- Manager: pause/resume, undo last pick, skip turn and reset draft (typed confirmations), manager
  draft with override.
- The last pick, made through the API by the other player, opens the draft complete modal.

**Done.** 11 tests in `Tests/Draft/`: `InviteAndPublisherTests` (3), `DraftOrderTests` (2),
`DraftingTests` (3) and `DraftControlTests` (3). The whole suite is 114 passing and 1 skipped, in
about 5 minutes.

As built:
- `DraftTestBase` finds whose turn it is and a game the server will accept: the first available
  game from the API's Top Available list, which is what `MockedLivePlayer` drafts. Not every top
  game in the local database is draftable, so the UI picks that one by name.
- The live test runs three browsers at once: the manager starts the draft, then the first
  player's pick shows on the second player's page with no reload, and the other way round. The
  first pick searches; the second picks straight from Top Available. One opens the modal from
  the header button and the other from the sidebar. Pages open after the draft starts, because
  the league page connects to SignalR only when it loads with a draft running.
- The draft complete dialog test leaves the manager's page open while
  `DraftToCompletionAsync` makes the last picks through the API.
- `ManagerDraftControls` drafts for the next player, then pauses, undoes, skips (OK is disabled
  until "SKIP TURN" is typed), resumes and resets (the same for "RESET DRAFT"). Each step is
  checked by its toast and the page state.
- Drag and drop uses `Ui.DragAsync`, which moves the real mouse in steps.
- `Ui.SidebarAction` finds the visible `li`: bootstrap-vue gives every `v-b-modal` item the
  button role, and the header's "Draft Game" and "Draft Counter Pick" buttons share their text.
  `Ui.ButtonElement` finds the real button.
- Not covered: drafting from the watchlist (step 6 covers the watchlist), the manager's
  override checkbox (it only shows when a pick fails), and the manager counter pick form.

### 6. Post-draft player actions
Fixture: `DraftToCompletionAsync` on a bidding scenario.
- Bids: make a bid (an invalid amount disables it); Current Bids lists it; drag to reorder
  priority and save; edit the amount; cancel through the confirm modal. Counter pick bid.
- Drops: request a drop, then cancel it through the confirm.
- Watchlist: add, edit notes, drag to reorder, remove.
- Trades: propose; the counterparty sees and accepts it; reject flow; trade votes render.
- Publisher page: move mode reorders slots; the snapshot button produces a download.
- League options, eligibility and tag overrides modals open read-only.

**Done.** 10 tests in `Tests/PostDraft/`: `BidTests` (3), `DropAndWatchlistTests` (2),
`TradeTests` (2) and `LeagueViewTests` (3), plus `PostDraftTestBase`. The whole suite is 124
passing and 1 skipped, in about 6½ minutes.

As built:
- **The leagues.** `PostDraftTestBase.PostDraftScenario` has 4 standard games with 2 drafted, 2
  counter picks with 1 drafted, and bids, drops and trades on, so a drafted league has open slots
  of both kinds. It drafts two players, or three for trades, so the manager can vote on a trade
  between the other two. Move mode uses `SlotTestHelpers.Scenario` with one special slot.
- **Bids:** placing a bid from Top Available; then, on two bids set up through the API,
  reordering by dragging with "Change Priorities" (kept after a reload), editing the amount, and
  cancelling one through the nested confirm modal; and a counter pick bid.
- **On main, the bid form and Current Bids stay open** after a bid or an edit. The tests close
  the bid form themselves and keep working in Current Bids.
- **Drops:** request a drop, then cancel it through the confirm modal in My Pending Drops.
- **Watchlist:** add two games from Top Available, drag the second to the top and save it
  (kept after a reload), write a note in the nested notes modal (`$bvModal.show`), and remove a
  game.
- **Trades:** the proposer picks the counterparty and a game on each side. The counterparty
  accepts from the "View Trades" alert. The manager votes with a comment, sees it under Votes,
  and executes, after which the API shows the games swapped. A second test rejects the trade.
- **The projections toggle** (the custom `toggleSwitch` v-model) switches every publisher's
  total to projected points and back.
- **Move mode** moves a game into an empty regular slot ahead of the special slot. The API
  confirms the new slot number.
- **Not covered:** the planned snapshot test. The Share button is Plus-only and calls
  `navigator.share`, so nothing is downloaded. Bid amount validation isn't covered either:
  nothing disables Place Bid for an invalid amount; the provider shows a message instead.
- `Ui.DragAsync` scrolls the source into view first. The watchlist sits below a long table in
  its modal, and mouse coordinates are the viewport's.
- `ConfirmDialog` finds the nameless nested confirm modal by its question, and takes the last
  match in case bootstrap-vue-next renders it inside the outer dialog.

### 7. Post-draft manager actions
Special auction (datetime-local input), claim game, remove publisher game, associate game,
manually score, will-not-release, edit publishers, eligibility overrides, tag overrides (drag
between the tag lists), reassign publisher (typed confirmation), disconnect player, transfer
manager, start new year.

**Done.** 7 tests in `Tests/PostDraft/`: `ManagerRosterTests` (3) and `ManagerPlayerTests` (4). The
whole suite is 131 passing and 1 skipped, in about 7¾ minutes.

As built:
- **Rosters:**
  - add a game to a player, then remove it;
  - add an unlisted game, link it to a master game, then score it by hand (after linking, the
    publisher game takes the master game's name);
  - mark a drafted game as won't release;
  - ban a game, then reset its eligibility;
  - override a game's tags through `masterGameTagSelector`, then reset them.
  Each step is checked by its toast, the modal's own table, or the API.
- **The tag override uses vue-multiselect, not drag and drop.** The plan was wrong here:
  dragging between tag lists is `leagueTagSelector`, in the league settings. The test types the
  tag name into the multiselect and presses Enter.
- **Players and publishers:**
  - edit a publisher's name and budget;
  - disconnect a player, then reassign their publisher to a player who joined after the draft
    (inactive, so assignable), with the typed "Reassign Publisher" confirmation;
  - hand the league to another player, after which the Manage League section disappears;
  - create a special auction with the `datetime-local` input, then cancel it.
- `Ui.SelectOptionContainingAsync` and `PostDraftTestBase.SearchAndPickAsync` are now shared
  helpers.
- **Not covered:** Start New Year, because the next year isn't open in the local database, and
  removing a publisher with no player.
- The manager's game search is fuzzy: a search for a name that doesn't exist still lists games,
  so "No games were found." doesn't appear. "Select unlisted game." is offered either way.

### 8. Conference
Create a conference; invite links (create, delete, join through `?inviteCode=`); add a league;
assign players by dragging between leagues, and lock/unlock; conference manager message and
dismiss; conference year select; settings, promote a conference manager, reassign a league
manager, renew a league for a new year. Conference setup goes through a small
`ConferenceFixtureBuilder` in the support library, on the typed client.

### 9. Critics Royale and master game requests
- Royale (server and `page.Clock` set inside an open quarter): create publisher; purchase a
  game; set budget; sell; change name; year and quarter dropdowns; leaderboard paging; groups
  (create, invite links, join by code, leave through the confirm, remove a member); the
  publisher chart canvas draws.
- `/masterGameRequest` and `/masterGameChangeRequest`: validation disables submit; the date
  input's value is kept; submitting shows the request in the user's list.

### 10. Run on `vue3-migration`
Steve merges `main` into `vue3-migration`, or OKs Claude doing it locally (no push). It touches
only the new projects and the moved test files. Build the vue3 client and run the suite. Report
each failure the way the read-only pass did: what changed in the library, the symptom, where,
with the trace. Per the standing rule, Claude fixes CSS and template-for-looks; JS behaviour is
noted for Steve. Also hand over the console-warnings report as a compat-removal checklist.

## Verification

- Step 1: solution builds clean; the integration test suite passes as before.
- Each later step: `scripts/Run-UiTests.ps1 -Filter <step's fixtures>` green three runs in a
  row on `main` headless, plus one headed run watched. A deliberately broken assertion shows a
  saved trace (`pwsh` `playwright show-trace`).
- `scripts/Format.ps1 -Check` passes.
- Step 10's output is the failure list on `vue3-migration`, which is the point of the suite.

## Critical files

- New: `src/FantasyCritic.IntegrationTestSupport/`, `src/FantasyCritic.UiTests/`
  (`UiTestBase.cs`, `Ui.cs`, `UiWebApplicationFactory.cs` or a Kestrel option on the shared
  factory, `Tests/<Area>/*Tests.cs`), `scripts/Run-UiTests.ps1`,
  `docs/ui-automation-suite-plan.md`.
- Moved: `src/FantasyCritic.IntegrationTests/FantasyCriticWebApplicationFactory.cs`,
  `CapturingEmailSender.cs`, `Helpers/*`.
- Edited: `src/FantasyCritic.slnx`, `src/FantasyCritic.IntegrationTests/*.csproj`, `CLAUDE.md`.
- Reused: `HostingExtensions.cs:349-371` (static assets + fallback the dist serving slots into),
  `Development/DevSignInEndpoint.cs`, `ApiSession.SignInAsAsync`/`RegisterAsync`,
  `LeagueFixtureBuilder` and the `LeagueScenario` presets, `MockedLivePlayer`.
