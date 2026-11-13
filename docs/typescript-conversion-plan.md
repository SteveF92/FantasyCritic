# TypeScript conversion and generated client

Convert every component in the client app to `<script lang="ts">` and replace every direct `axios` call
with the NSwag-generated client. 4 of the 173 `.vue` files are already TypeScript; 96 import axios.

This is cooperative: Steve works through the steps, and calls Claude in for review or when stuck.

## Goals

1. **Catch bugs that are already there.** Typed props, typed API responses and typed request objects
   turn mismatches into compile errors. Anything found goes in [Found along the way](#found-along-the-way).
2. **Review the branch page by page against main.** `vue3-migration` has changed almost everything, so
   each step's manual test is a full check of that page against main: what it shows *and* every action
   it can take, including writes.
3. **Leave the code checkable.** At the end, `npm run type-check` covers every component and template,
   and the API contract breaks the build when it changes.

Comparing against main doesn't mean matching it 1:1. If embracing a new default behaviour makes for
cleaner code, markup or CSS, and the appearance is not worse, embrace it. Dropping the Font Awesome 6
width override is the example. Be aware of each deviation, and log it in
[Found along the way](#found-along-the-way).

Options API stays. Each component becomes `defineComponent({...})`, matching
[royalePublisherGraph.vue](../src/FantasyCritic.VueClientApp/src/components/royalePublisherGraph.vue).

## Order: bottom-up within each page

Convert the deepest components first and work up to the view. vue-tsc checks a parent's template
bindings (`<child :foo="x" @bar="...">`) against the child's props and emits only when the child is
typed. Bottom-up, each parent's conversion checks the children done before it. A JS parent using a TS
child is fine in between, because JS templates aren't checked.

The deepest components are shared, so the first steps convert the shared leaves, and the page steps
after them only cover each page's own subtree. A shared component that belongs to one family of pages
(for example the league settings components) goes with the first page of that family.

## Conventions

```ts
import { defineComponent, type PropType } from 'vue';
import { leagueClient, type LeagueYearViewModel, type MasterGameYearViewModel } from '@/api/clients';
import { getApiErrorMessage } from '@/utilities/apiError';

export default defineComponent({
  props: {
    leagueYear: { type: Object as PropType<LeagueYearViewModel>, required: true }
  },
  emits: {
    gameSelected: (_game: MasterGameYearViewModel) => true
  },
  data() {
    return {
      selectedGame: null as MasterGameYearViewModel | null,
      errorInfo: ''
    };
  },
  computed: {
    hasSelection(): boolean {
      return this.selectedGame !== null;
    }
  },
  methods: {
    async dropGame(): Promise<void> {
      try {
        await leagueClient.dropGame({ ... });
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error);
      }
    }
  }
});
```

- **Types come from `@/api/clients`** (the generated view models and requests). The hand-written
  copies in `src/models/` are gone (cf7ea5938). Types that are only about the UI and not the API, such
  as `PopoverContent`, go in `src/types/vueTypes.ts`.
- **Props:** `as PropType<T>` for objects and arrays. Make a prop `required: true` when every parent
  passes it, rather than optional plus `!` at each use. A route with `props: true` passes strings;
  routes that parse their params pass numbers. Match what the route gives. If a parent renders the
  child before its data loads and passes `null` meanwhile, the prop is `T | null` with `default: null`,
  and the child shows its own loading state (`topBidsAndDropsWidget`). A string prop with a few fixed
  values gets a union, as in `String as PropType<'user' | 'league'>`.
- **Computed:** annotate every return type. The Options API needs it to type `this`. A computed that
  returns a list returns `[]` when there's no data, not `null`; `b-table`'s `items` doesn't accept `null`.
- **Data:** declare a `<Component>Data` interface and annotate `data(): <Component>Data` (as in
  `views/masterGame.vue`), or cast the initial values (`null as T | null`, `[] as T[]`). A value that's
  `null` only until the page loads stays `T | null`. In the template, put that value itself in the
  section's `v-if`; vue-tsc narrows only what the `v-if` tests, not something derived from it. Keep
  function parameters non-null and check before calling. `this` inside `data()` only knows props, so a
  starting value from a store reads the store directly: `useInterLeagueStore().supportedYears...`.
- **Constants:** a value goes in `setup()` if it never changes and doesn't depend on props or state,
  such as a table's field list. Annotate it, because otherwise its type is only checked where it's used,
  and that check misses typos. Return the value itself, not a function that returns it. A value returned
  from `setup()` isn't reactive, so anything reassigned or mutated stays in `data()`, and anything derived
  from props or the store stays in `computed`. A constant only the script uses (not the template) goes
  at module level instead, outside the component.
- **Emits:** declare every event in the object form, with a typed validator that returns `true`. It's
  the only Options API form that types the payload, both for `$emit` and for the parent's handler.
  Prefix the validator's parameters with `_`. ESLint ignores `_` arguments in `.vue` files for this.
  The validator's parameters are the payload's types; its return value is only Vue's dev-mode
  "is this payload valid?" check, which is why it's `true`.
- **Generated enums** (`RecentMasterGameChangeMode` and the like) are numeric and exported as values
  from `api/clients.ts`. Compare against the enum, never a string. A template that uses one needs it
  returned from `setup()`.
- **Route query values** are `string | null | (string | null)[]`. Check with
  `typeof value === 'string'`, not `as string`.
- **vee-validate:** to clear a form after a successful submit, give `ValidationForm` a `ref="form"` and
  call `(this.$refs.form as InstanceType<typeof ValidationForm>).resetForm()` after clearing the bound
  data. Without it, emptying a required field shows its "required" error.
- **Shared link helpers:** `utilities/ggLinks.ts` and `utilities/openCriticLinks.ts`. `openCriticLink`
  returns `undefined` when there's no ID, because `:href` takes `string | undefined`, not `null`.
- **Fallthrough attributes** (`alt` on `BImg`, `title` on a component that doesn't declare it) are
  fine. vue-tsc's `checkUnknownProps` is off because it would flag them.
- **Clickable icons:** `font-awesome-icon` doesn't declare `onClick`, so `@click` on one is a type error.
  Put the icon in `<button type="button" class="font-awesome-button" aria-label="...">` and move `@click`,
  `:disabled`, `title` and `v-b-popover` to the button, as in `components/toastHost.vue`. The aria-label is
  needed because the button has no text, and it names the action ("Archive league"), not a copy of
  another button's label. Still to do: `recentJobsTable.vue` and `modals/conference/assignPlayers.vue`.
- **Handlers with optional parameters:** call them, as in `@click="hide()"`. `@click="hide"` passes the
  `MouseEvent` as the first argument, and type-check rejects it on a native element.
- **`$refs`:** cast with `this.$refs.x as InstanceType<typeof Child> | undefined`.
- **Requests:** build the generated request type (a typed object literal), not an untyped object.
- **Errors:** `getApiErrorMessage(error, '<what failed>')` from step 2, never `error.response.data`.
  Show it in a basic `<b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>`
  (show the message, not a generic line), and clear `errorInfo` at the start of each load or submit so a
  later success removes it. A page whose spinner waits on data shows the alert in place of the spinner,
  so a failure doesn't spin forever.
- **No escape hatches:** no `any`, no `as unknown as T`, no `@ts-ignore`/`@ts-expect-error`. If the types
  fight back, that's often a real bug or a wrong spec. Stop and call Claude.

### When the generated client is missing something

- **No client method for an endpoint, or a method that returns `Promise<void>` when the endpoint
  returns data:** fix the controller. Add `[ProducesResponseType<T>(StatusCodes.Status200OK)]` to the
  action, then regenerate:
  ```powershell
  dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj
  scripts/Regenerate-ApiClient.ps1
  ```
- **A generated type that's wrong** (for example a nullable field typed as non-null): fix the C# view
  model's nullability, not the TypeScript.
- Backend changes go in their own commit inside the step, so they're easy to review.

## Each step

1. Convert the step's components bottom-up, swapping axios for the client as you go.
2. `npm run type-check`, `npm run lint`, `npm test`. After any backend change, also run
   `dotnet build src/FantasyCritic.slnx`.
3. Manually test every page the step touches against main. Use both the read views and every action,
   including writes.
4. Log every bug found and every deliberate change in behaviour or appearance in
   [Found along the way](#found-along-the-way).
5. Commit. Call Claude to review before starting the next step.

### What Claude checks in review

- Escape hatches (`any`, casts that hide `null`, non-null `!` where a guard belongs).
- Prop `required`/optional matches every parent that uses the component.
- Every axios call is gone and the client call has the same parameters and the same handling of
  success and failure.
- Runtime changes not listed in the log.
- Backend changes: the `ProducesResponseType` is the real response type, and the nullability fixes are
  correct.

### Testing setup

Run main beside the branch as in the earlier passes: a main worktree with Vite on 44478, this branch
on 44477, both against the same backend, signed in through `/dev/sign-in-as`. Writes are now in scope,
so use a local database you don't mind changing. A write changes the data both sides read, so for each
action compare the result on each side, using a different target for each (another game, another
publisher), not the same state twice.

Pages behind a live draft (the draft modals, SignalR updates) need a league in draft. The
"Draft Start Test" league in `fantasycritic-fromsnapshot` was one.

`scripts/Run-UiTests.ps1` covers some flows end to end. Run it after the shared steps and after the
League steps. `-Filter "FullyQualifiedName~NavigationTests"` runs one fixture; it builds Release, so
it can run while the dev servers are up.

What worked in steps 6 to 8:
- **Error paths and writes without touching data:** in the browser console, wrap `window.fetch` so one
  endpoint returns a fake response (a 500, a `ValidationProblemDetails` 400, or a 200), then navigate
  away and back with the router. Reload the page afterwards. The generated client uses `fetch`; pages
  still on axios can't be faked this way.
- **Missing data:** add rows to a real response the same way (fake invites on the home page).
- **Snapshot database:** Most Desired Reviews and all-time Most Dreams Dashed time out locally; those
  failures aren't the code under test.
- **Console:** after loading a page, check for Vue warnings, not just errors. "Invalid prop" and
  duplicate-key warnings are often the first sign of a wrong type.

## Steps

### 1. Type-checking setup (done)

- `components.d.ts` is in `tsconfig.json`'s `include`. It sits at the project root, outside `src/**`,
  so vue-tsc didn't see it, and bootstrap-vue-next components (`BModal`, `BButton`, ...) and
  `RouterLink` weren't checked in templates.
- `src/globalComponents.d.ts` declares the components `main.ts` registers: `FontAwesomeIcon`,
  `FontAwesomeLayers`, `FontAwesomeLayersText`, `ValidationField`, `ValidationForm`.
- `vueCompilerOptions` turns on `checkUnknownComponents`, `checkUnknownEvents` and
  `checkUnknownDirectives`. Not `strictTemplates`: it also turns on `checkUnknownProps`, which flags
  valid fallthrough attributes (it failed on `alt` on `BImg` in `community`/`fantasyCriticPlus` and
  `name` on an `<a>` in `about`).
- Checked with a throwaway component: an unknown component, an unknown directive, and wrong prop types
  on `BButton`, `font-awesome-icon`, `ValidationField` and `router-link` are all errors now. An unknown
  event on `BButton` still isn't, because bootstrap-vue-next accepts any listener.
- ESLint ignores `_`-prefixed arguments in `.vue` files, for the emits validators.
- Nothing new surfaced in the four components that are already TS. Moving them off `src/models/` is
  component work, so it's in steps 5 (`siteAnnouncementsWidget`, `siteUpdates`) and 9
  (`royalePublisherGraph`, then delete `src/models/`).
- Also removed an unused `RouteLocationNormalized` import in `router/routes.ts` that failed lint.

### 2. API error helper (done)

`utilities/apiError.ts`: `getApiErrorMessage(error: unknown, fallback = 'Something went wrong.'): string`,
tested in `tests/apiError.test.ts`.

What the server sent (2026-11-07, Development, from the browser through `fetch`):

| Case | Status | Content type | Body |
|---|---|---|---|
| `BadRequest("message")`, no `Accept` header | 400 | `text/plain` | `You cannot have a blank name.` |
| `BadRequest("message")`, `Accept: application/json` | 400 | `application/json` | `"You cannot have a blank name."` |
| Missing required field, malformed JSON | 400 | `application/problem+json` | `ValidationProblemDetails` with `errors` |
| Bare `BadRequest()` | 400 | `application/problem+json` | `ProblemDetails`, title "Bad Request", no message |
| Signed out | 401 | none | empty |
| Not an admin | 403 | none | empty |
| Unknown route | 404 | none | empty |

The generated client sends `Accept: application/json` only for operations with a response type, so
both `BadRequest` forms happen. Every `BadRequest(x)` in the controllers passes a string (a literal or
a `Result`'s `Error`). A 500 is the developer exception page here and the `/Error` page in production.

The helper:

- **4xx:** returns the server's message: plain text as is, a JSON string unquoted, and a validation
  failure's messages joined with spaces. Anything else (an empty body, `ProblemDetails` with no
  messages, HTML, JSON that doesn't parse) returns `fallback`.
- **5xx:** always returns `fallback`.
- **Anything that isn't an `ApiException`** is rethrown: network failures (`TypeError: Failed to
  fetch`) and bugs in the calling code reach the console instead of turning into a message. The axios
  code also threw there, since `error.response` was `undefined`. The UI suite fails on console errors,
  so they show up.

Callers that had their own fallback text pass it as `fallback`.

The stores were already TypeScript. One store shows a message, and it uses the helper:
`publisherStore.confirmPositions` sets `moveGameError`. The other stores' catches branch on status (auth
on 401, league on 401/403) or set a flag, and stay as they are.

### 3. Shared leaves: master game display (done)

`masterGameTagBadge` → `masterGameSummary` → `masterGamePopover`. Used on 25, 16 and 15 pages.
Test on Master Games, League and Home.

### 4. Shared leaves: small widgets (done)

`collapseCard`, `toggleSwitch`, `dateInput`, `statusBadge`, `siteCounts`, `gameTables/slotTypeBadge`,
`masterGameDetails`, `masterGameTagSelector`, `modals/leagueOptionsModal`.

### 5. App shell and static pages (done)

`App.vue`, `topNavBar`, `footerModule`, `toastHost`; `welcome` (+ `homeFAQ`), `discordBot`, `faq`,
`howToPlay`, `contact`, `community`, `fantasyCriticPlus`, `notFound`. `siteAnnouncementsWidget` and
`siteUpdates` are already on `SiteAnnouncementViewModel` (cf7ea5938). Test both signed in and signed
out.

### 6. Master game pages (done)

- `masterGames` (+ `gameTables/masterGamesTable`)
- `masterGame` (+ `masterGameYearStats`)
- `recentMasterGameChanges`, `mostDesiredReviews`, `topBidsAndDrops`, `publicLeagues`
- `advancedGameStats` (+ `gameStats/longestTenuredGamesPanel`, `gameStats/mostDreamsDashedPanel`)

### 7. Request pages (done)

`masterGameRequest`, `masterGameChangeRequest`: create, delete and dismiss requests.

### 8. Home (done)

`home` (+ `leagueTable`, `conferenceTable`, `topBidsAndDropsWidget`, `gameNews`).

### 9. Critics Royale

- `royalePublisherGraph` (already TS, on `RoyalePublisherViewModel` since cf7ea5938): fix `chart-id`
  (see "Found along the way")
- `criticsRoyale` (+ `royaleGroupsWidget`, `royalePodium`, `criticsRoyaleInfo`,
  `modals/createRoyalePublisherForm`)
- `royalePublisher` (+ `possibleRoyaleMasterGamesTable`, `modals/royalePurchaseGameForm`,
  `modals/sellRoyaleGameModal`, `modals/royaleChangePublisherNameForm`/`IconForm`/`SloganForm`)
- `royaleHistory`
- `royaleGroup` (+ `modals/royaleGroupInviteLinksModal`), `royaleGroupQuarter`

### 10. League creation and settings

`DraftCreationSettings`, `leagueCreationPresets`, `leagueTagSelector`, `specialGameSlotSelector`,
`leagueYearSettings`; then `createLeague`, `createConference`, `editLeague`, `manageDrafts`.

### 11. Publisher

`gameTables/gameNameColumn`, `gameTables/publisherCoverViewSlot`, `gameTables/publisherCoverView`,
`gameTables/playerGameTable`, then `publisher`. Test sort mode and move mode.

### 12. League history and stats

- `leagueHistory` (+ `tradeSummary`, `leagueActionSet`)
- `leagueAllTimeStats` (+ `hallOfFameGame`, `hallOfFameSection`)

### 13. League page

The League page has 64 components of its own, so it's split into four steps. Each one still tests
on the League page.

**13a. Display components.** `leagueYearStandings`, `countdown`, `bidCountdowns`,
`draftReadinessBanner`, `draftStatusHeader`, `gameTables/searchSlotTypeBadge`,
`gameTables/minimalPlayerGameSlotRow`, `gameTables/minimalPlayerGameTable`, `leagueGameSummary`,
`specialAuctionInfo`, `activeBids`, `possibleMasterGamesTable`, `possibleCounterPicksTable`.

**13b. Player modals.**
- Bids and drops: `bidGameForm`, `bidCounterPickForm`, `currentBidsForm`, `dropGameForm`,
  `superDropGameForm`, `currentDropsForm`, `gameQueueForm`.
- Trades: `proposeTradeForm`, `activeTradesModal`.
- Publisher: `createPublisherForm`, `changePublisherNameForm`/`IconForm`/`SloganForm`.
- Draft: `editAutoDraftForm`, `playerDraftGameForm`, `playerDraftCounterPickForm`.
- Info: `specialAuctionsModal`, `eligibilityOverridesModal`, `tagOverridesModal`.

**13c. Manager modals.**
- Players and publishers: `invitePlayerForm`, `manageActivePlayersForm`, `createPublisherForUserForm`,
  `managerEditPublishersForm`, `reassignPublisherModal`, `disconnectPlayerModal`,
  `removePublisherModal`, `removePlayerModal`, `transferManagerModal`.
- Games: `managerClaimGameForm`, `managerAssociateGameForm`, `removeGameForm`, `manuallyScoreGameForm`,
  `manuallySetWillNotReleaseForm`, `manageEligibilityOverridesModal`, `manageTagOverridesModal`.
- Draft: `managerSetAutoDraftForm`, `managerDraftGameForm`, `managerDraftCounterPickForm`,
  `editDraftOrderForm`, `setPauseModal`, `resetDraftModal`, `undoLastDraftActionModal`,
  `skipCurrentDraftPickModal`, `draftScheduleModal`, `startDraftModal`, `draftCompleteModal`.
- League: `changeLeagueOptionsForm`, `addNewLeagueYearForm`, `managerMessageModal`, `underReviewModal`.

**13d. `leagueActions` and `league` itself**, including the SignalR draft updates.

### 14. Conference

- `conference`, with `conferenceYearStandings`, `conferenceManagerActions` and the ten
  `modals/conference/*` modals
- `conferenceHistory`

### 15. Admin pages

- `adminConsole` (+ `recentJobsTable`, `serviceMonitor`), `adminSupportTickets`,
  `adminSiteAnnouncements`
- `activeMasterGameRequests`, `activeMasterGameChangeRequests`, `pendingMasterGameUpdates`
- `masterGameCreator`, `masterGameEditor`, `actionProcessingDryRunResults`

### 16. Lock it in

- Remove `axios` from `package.json`.
- Add ESLint's `vue/block-lang` rule requiring `lang="ts"`.
- Confirm no `.js` is left under `src/`, and no `any` or `@ts-` comments outside generated code.
- Run the UI test suite.

Then make the API changes held in [api-breaking-changes-plan.md](api-breaking-changes-plan.md).

## Found along the way

Bugs found by the conversion, and every deliberate behaviour change. "On main" says whether production
has the bug too.

| Step | Where | What | On main | Fix |
|---|---|---|---|---|
| — | `views/masterGameRequest.vue` `sendMasterGameRequestRequest` | Reads `error.response.data` on an `ApiException`, whose `response` is a string, so `errorInfo` is `undefined` and a failed request shows no error | Yes | Done: `getApiErrorMessage`, and the alert now shows the message instead of a generic one |
| 1 | `components/royalePublisherGraph.vue` | Passes `chart-id` to vue-chartjs 5's `Line`, which has no `chartId` prop, so it falls through as a `chart-id` attribute instead of setting the canvas id (found with `checkUnknownProps` on). TypeScript reports only the first excess property, so check `height` and `styles` too | Yes | Graph done: `chart-id`, `height` and `styles` removed, with its `chartHeight` prop; the containers set the height. `criticsRoyale` no longer passes them; `royalePublisher` and `royaleGroupQuarter` still pass `chart-id` and `chart-height` |
| 2 | `store/publisherStore.ts` `confirmPositions` | Now uses `getApiErrorMessage`. Changes: a 403 or 404 shows "Something went wrong." where it showed nothing (empty body); a 500 shows that instead of the error page's text; a network failure is rethrown after leaving move mode, where it showed "TypeError: Failed to fetch" | — | Done |
| 2 | `store/publisherStore.ts` | `moveGameError` is never cleared, not by a later successful move and not by `clearPublisherStoreData`, so after one failed move the error stays up, even on another publisher's page | Yes | Done: cleared when a move is confirmed and in `clearPublisherStoreData`; `tests/store/publisherStore.test.ts` |
| 4 | `components/leagueActions.vue` | Passes `:possible-league-options` to `leagueOptionsModal`, which has no such prop and reads the options from the store, so the value falls through as an attribute. `checkUnknownProps` is off, so type-check won't flag it. It's the only use of the `possibleLeagueOptions` `mapState` entry, so remove both | Yes | Step 13d |
| 4 | `views/masterGameCreator.vue` | Passes `supportedYears.find(...)` to `leagueOptionsModal`'s required `supportedYear`. `find` can return `undefined`, and the modal reads `supportedYear.year`. Type-check should flag it once the page is converted; decide then what happens if no year matches | Yes | Step 15 |
| 5 | `components/toastHost.vue` | The close icon is now a native button named "Close", so a keyboard can reach it. `hide` is now called with no arguments, where it was passed the `MouseEvent` as its trigger | — | Done |
| 5 | `components/toastHost.vue` | Toasts had no fixed position on this branch, so they rendered in page flow just above the footer, off-screen on any long page. bootstrap-vue-next doesn't position a declarative `b-toast` the way BootstrapVue's default toaster did. Also, `.toast-body` didn't fill the toast, so the close button sat against the message | No | Done: pinned top right with `toast-container`; `.toast-body` grows |
| 5 | `components/toastHost.vue` | Toasts never auto-hide on this branch. `visible` shows a `b-toast` without starting its countdown, which only runs when `modelValue` is a number, so a toast stayed until it was closed, even though `vue3-upgrade-plan.md` records auto-hide as checked | No | Done: `:model-value="5000" no-progress` in place of `visible`. Hovering pauses the countdown |
| 5 | `css/site.css`, `views/welcome.vue` | Dropped the overrides that kept Font Awesome 6's icon and layer widths, along with the `left-*` transforms that compensated for them, so icons now use Font Awesome 7's 1.25em default. It looks better on the welcome page. Other pages get checked and adjusted as their steps come up | — | Done |
| 6 | `components/gameTables/masterGamesTable.vue`, `views/actionProcessingDryRunResults.vue` | The table's `gameRows` computed turned rows with `error` red by writing to its prop. Only the dry run's games have `error`, so that page now marks its own rows and the table no longer does | — | Done |
| 6 | `views/actionProcessingDryRunResults.vue` | Passes `MasterGameViewModel[]` to `masterGamesTable`, which takes `MasterGameYearViewModel[]`, and omits the required `show-year-stats`. The year columns mean nothing there, so the page probably wants its own small table. Type-check will flag the prop once the page is converted | Yes | Step 15 |
| 6 | `components/masterGamePopover.vue`, `components/masterGameSummary.vue` | Both took only `MasterGameYearViewModel`, but pages such as Recent Changes pass a `MasterGameViewModel`, which has no year stats. The popover showed "0%", "Undrafted" and "Unhyped..." for every such game. Both now take either type, and the summary hides the year rows when there's no `year` | Yes | Done |
| 6 | `views/masterGame.vue` | The sub-games section called `releaseDate` and `openCriticLink` methods that `b3e56008f` (June 2019) moved into `masterGameDetails`, so any game with sub-games has failed to render that section since. It now uses `formatDateLong` and its own `openCriticLink` | Yes | Done |
| 6 | `views/masterGame.vue` | Every failed API call set `error`, which nothing displayed, so an unknown master game ID gave a blank page. Errors now go through `getApiErrorMessage` and show in a `b-alert` at the top of the page. A 404 shows "Something went wrong." | Yes | Done |
| 6 | `GameController.LeagueYearsWithMasterGame` | `LeagueName` was nullable from the domain class through the view model and generated client, though `tbl_league.LeagueName` is `NOT NULL` and the query inner-joins it. It's non-nullable throughout now, and the domain type is a record that the view model is built from | — | Done |
| 6 | `components/masterGameYearStats.vue` | Like `royalePublisherGraph`, it passed vue-chartjs 4's `chart-id`, `height` and `styles` to vue-chartjs 5's `Line`, which has none of them, so they fell through to the canvas as attributes (`styles="[object Object]"`). Removed; the 520px container already sets the height. The tooltip's percent check now looks up the series by dataset index instead of a custom `percentageScale` dataset property | Yes | Done |
| 6 | `views/topBidsAndDrops.vue` | A failed load set `errorInfo`, which nothing displayed, and left the spinner up forever. Errors now show in a `b-alert` in place of the spinner. Both year dropdowns had `id="year-dropdown"`, which clashed when the end-of-year alert showed; the alert's is now `year-within-process-date-dropdown` | Yes | Done |
| 6 | `views/publicLeagues.vue` | A failed load wasn't caught, so the page showed nothing. It now shows a `b-alert`. The pagination's `aria-controls="my-table"` pointed at no element; the table is now `public-leagues-table` | Yes | Done |
| 6 | `components/gameStats/longestTenuredGamesPanel.vue`, `components/gameStats/mostDreamsDashedPanel.vue` | A failed load wasn't caught, so the spinner stayed up forever. Errors now show in a `b-alert`. These panels and `topBidsAndDrops` clear the error at the start of each load, so it goes away when a later load succeeds | Yes | Done |
| 6 | `mostDreamsDashedPanel`, `masterGames`, `masterGameYearStats`, `publicLeagues`, `topBidsAndDrops` | Changing a dropdown while an earlier load is still running lets whichever response arrives last win. Seen on Most Dreams Dashed: a slow all-time request failed after the 2018 request succeeded, and its alert replaced the 2018 results. Fix by ignoring responses for anything other than the current selection | Yes | Not scheduled |
| — | `views/criticsRoyale.vue`, `components/possibleRoyaleMasterGamesTable.vue`, `components/possibleMasterGamesTable.vue` | Each `b-pagination` has `aria-controls="my-table"`, but no element has that id. Give each table a real id and point `aria-controls` at it, or drop the attribute. `gameNews` was done in step 8 (`game-news-table`) | Yes | `criticsRoyale` done (`royale-standings-table`); the other two in steps 9 and 13a |
| 9 | `components/modals/createRoyalePublisherForm.vue` | A failed create was swallowed, so the modal did nothing. A name of only spaces passes the button's check and gets "You cannot have a blank name." from the server. The error now shows in a `b-alert` in the modal, cleared on each submit and when the modal closes | Yes | Done |
| 9 | `views/criticsRoyale.vue` | A failed load wasn't caught, so the leaderboard spinner stayed up forever. Errors now show in a `b-alert` in place of the spinner | Yes | Done |
| 9 | `views/criticsRoyale.vue` | Switching quarters kept the pagination page, so page 4 of one quarter showed an empty table in a quarter with fewer publishers, and the old quarter's standings, podium, chart and "View My Publisher" stayed up until the new ones arrived. Each load now goes back to page 1 and clears them, so the spinner shows while loading | Yes | Done |
| 9 | `components/royaleGroupsWidget.vue` | "No groups yet." and "No featured groups." showed while the groups were still loading. They now wait for the load | Yes | Done |
| 8 | `components/gameNews.vue` | The league pills were keyed on `leaguePublisherSet.LeagueID`, but the API sends `leagueID`, so every pill's key was `undefined` and a game in two or more of the user's leagues had duplicate keys | Yes | Done |
| 7 | `views/masterGameChangeRequest.vue` | A failed request showed "An error has occurred with your request." instead of the server's message, and the error stayed after a later success. After a successful submit, the cleared note showed "The Request Note field is required" because the form was never reset | Error text: Yes. Reset: not checked | Done: shows `errorInfo`, clears it per submit, calls `resetForm()` |
| 8 | `views/home.vue` | A failed home page load set `errorInfo`, which nothing displayed. It now shows in a `b-alert` | Yes | Done |
| 8 | `views/home.vue`, `components/leagueInviteTable.vue` | The League Invites tab passed `CompleteLeagueInviteViewModel` rows to `leagueTable`, which takes `LeagueWithStatusViewModel`. Invites now have their own table (envelope icon, league link, manager), so `leagueTable` keeps its exact type. No visible change | — | Done |
| 8 | `components/leagueTable.vue` | The archive and un-archive icons are now native buttons named "Archive league" and "Un-archive league", so a keyboard can reach them. Icon margins and transforms were adjusted for Font Awesome 7 | — | Done |
| — | `router/index.ts` | vue-router 5 warned on every navigation that `next()` in guards is deprecated. The guard now returns `'/home'`, `true` or `false`. The signed-out redirect to login now cancels the navigation, so `afterEach` skips the title when a navigation fails | — | Done |
