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

### 6. Post-draft player actions
Fixture: `DraftToCompletionAsync` on a bidding scenario.
- Bids: make a bid (an invalid amount disables it); Current Bids lists it; drag to reorder
  priority and save; edit the amount; cancel through the confirm modal. Counter pick bid.
- Drops: request a drop, then cancel it through the confirm.
- Watchlist: add, edit notes, drag to reorder, remove.
- Trades: propose; the counterparty sees and accepts it; reject flow; trade votes render.
- Publisher page: move mode reorders slots; the snapshot button produces a download.
- League options, eligibility and tag overrides modals open read-only.

### 7. Post-draft manager actions
Special auction (datetime-local input), claim game, remove publisher game, associate game,
manually score, will-not-release, edit publishers, eligibility overrides, tag overrides (drag
between the tag lists), reassign publisher (typed confirmation), disconnect player, transfer
manager, start new year.

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
