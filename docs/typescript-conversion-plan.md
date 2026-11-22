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
  another button's label.
- **Handlers with optional parameters:** call them, as in `@click="hide()"`. `@click="hide"` passes the
  `MouseEvent` as the first argument, and type-check rejects it on a native element.
- **`$refs`:** cast with `this.$refs.x as InstanceType<typeof Child> | undefined`.
- **`b-table` rows:** if the `primary-key` field can be `0` (slot numbers start at 0), pass a getter
  that returns a string, as in `playerGameTable`'s `getRowKey`. bootstrap-vue-next treats a falsy key as
  missing and keys the row by its index, which collides with another row's key after sorting. Highlight
  rows with a `tbody-tr-class` function, not by writing `_rowVariant` onto the items, which are often the
  store's objects. A field key with a dot (`user.emailAddress`) renders nothing on its own; give it a
  `#cell(...)` slot.
- **`b-table` row details** are row expansion in bootstrap-vue-next 1.x: a `#row-expansion` slot, and the
  cell slots get `toggleExpansion()` and `expansionShowing`. Open rows are tracked by `primary-key`, so they
  stay open when the items are reloaded. `#row-details`, `detailsShowing` and `_showDetails` do nothing.
  The expansion row has no class of its own; `tbody-tr-class` gets it with the type `'row-expansion'`.
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

### 9. Critics Royale (done)

- `royalePublisherGraph` (already TS, on `RoyalePublisherViewModel` since cf7ea5938): fix `chart-id`
  (see "Found along the way")
- `criticsRoyale` (+ `royaleGroupsWidget`, `royalePodium`, `criticsRoyaleInfo`,
  `modals/createRoyalePublisherForm`)
- `royalePublisher` (+ `possibleRoyaleMasterGamesTable`, `modals/royalePurchaseGameForm`,
  `modals/sellRoyaleGameModal`, `modals/royaleChangePublisherNameForm`/`IconForm`/`SloganForm`)
- `royaleHistory`
- `royaleGroup` (+ `modals/royaleGroupInviteLinksModal`), `royaleGroupQuarter`

### 10. League creation and settings (done)

`DraftCreationSettings`, `leagueCreationPresets`, `leagueTagSelector`, `specialGameSlotSelector`,
`leagueYearSettings`; then `createLeague`, `createConference`, `editLeague`, `manageDrafts`.

- Server: `LeagueYearSettingsViewModel` split into `LeagueYearSettingsRequest` (no `LeagueID`,
  `HasSpecialSlots` or `OneShotMode`) and `LeagueYearSettingsResponse`, which keeps the old shape for the
  League page. Integration tests send fetched options back with `ToRequest()`.
- `leagueYearSettings` and `leagueTagSelector` are real v-models: every edit emits a new object, with no
  internal copy. `leagueCreationPresets` is a v-model of `PresetInput`, and `computePreset` returns a full
  request.

### 11. Publisher (done)

`gameTables/gameNameColumn`, `gameTables/publisherCoverViewSlot`, `gameTables/publisherCoverView`,
`gameTables/playerGameTable`, then `publisher`. Test sort mode and move mode.

- Server: `PublisherViewModel.FormerGameSlots` sends former games as `PublisherSlotViewModel`s with
  `Former` set, numbered after the real slots. `ProjectedFantasyPoints` is nullable, and `null` for a
  former slot. `PublisherGameViewModel.Linked` is `[Obsolete]`: a `masterGame` means linked. Remove it
  once Vue 3 ships.
- `gameNameColumn` takes `leagueYear` in place of `supportedYear`, `hasSpecialSlots` and
  `counterPickDeadline`. `minimalPlayerGameSlotRow` (step 13a) already passes it.
- `html2canvas` is replaced by `modern-screenshot`, through `utilities/shareImage.ts`. Both share
  buttons use it: cover art mode here, and `minimalPlayerGameTable` (step 13a) on the League page.

### 12. League history and stats (done)

- `leagueHistory` (+ `tradeSummary`, `leagueActionSet`)
- `leagueAllTimeStats` (+ `hallOfFameGame`, `hallOfFameSection`)

### 13. League page

The League page has 64 components of its own, so it's split into four steps. Each one still tests
on the League page.

**13a. Display components (done).** `leagueYearStandings`, `countdown`, `bidCountdowns`,
`draftReadinessBanner`, `draftStatusHeader`, `gameTables/searchSlotTypeBadge`,
`gameTables/minimalPlayerGameSlotRow`, `gameTables/minimalPlayerGameTable`, `leagueGameSummary`,
`specialAuctionInfo`, `activeBids`, `possibleMasterGamesTable`, `possibleCounterPicksTable`.

- Server: the invite queries return the invited user as `InviteUserName`/`InviteUserEmailAddress`, the
  names `LeagueInviteEntity` reads.
- `utilities/masterGameTags.ts` has the tag lookups (`getMatchingTag`, `getMatchingTags`) and the
  striped multi-tag badge colour, which `searchSlotTypeBadge`, `publisherCoverViewSlot` and
  `specialGameSlotSelector` each had a copy of.
- The league store's getters have return types, and it has `requiredSupportedYear` beside
  `requiredLeagueYear` and `requiredLeague`.

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

### 14. Conference (done)

- `conference`, with `conferenceYearStandings`, `conferenceManagerActions` and the ten
  `modals/conference/*` modals
- `conferenceHistory`

- Server: `ConferenceController.AvailableYears` and `GetConference` declare their 200 responses.
  `ConferenceCloneTests` uses the typed `GetConferenceAsync` instead of deserializing a raw URL.
  `GetLeagueYearsInConferenceYear`'s ambiguous `Year` is fixed (see "Found along the way").
- The conference store has `requiredConferenceYear` and `requiredConference`, like the league store.

### 15. Admin pages (done)

- `adminConsole` (+ `recentJobsTable`, `serviceMonitor`), `adminSupportTickets`,
  `adminSiteAnnouncements`
- `activeMasterGameRequests`, `activeMasterGameChangeRequests`, `pendingMasterGameUpdates`
- `masterGameCreator`, `masterGameEditor`, `actionProcessingDryRunResults`

- Server: `AdminController.OpenSupportTicket` declares its `SupportTicketViewModel` response; the
  generated clients typed it as a file.
- No component imports `axios` any more, and every component with a script is TypeScript, so step 16
  only has to remove the package.
- The dry run's game lists are a small table of their own (name, release date, tags) instead of
  `masterGamesTable`, whose year columns mean nothing there.

### 16. Lock it in

- Remove `axios` from `package.json`.
- Add ESLint's `vue/block-lang` rule requiring `lang="ts"`.
- Type the named routes. Declare a `RouteNamedMap` in vue-router's `TypesConfig` with a
  `RouteRecordInfo` for each of the 43 named routes in `router/routes.ts` (name, path, raw params and
  resolved params). Then a `{ name, params }` given to `router-link`, `:to` or `router.push` is checked:
  a misspelled route name, a missing param or a wrong param name is an error. It's all or nothing: with
  a map, any name not in it is an error. Until then, a computed that returns a route is annotated
  `RouteLocationRaw`, which checks nothing about the name or params; with the map, it can be
  `RouteLocationRaw<'manageDrafts'>`.
- Confirm no `.js` is left under `src/`, and no `any` or `@ts-` comments outside generated code.
- Run the UI test suite.
- Scope the remaining unscoped `<style>` blocks. `App.vue`'s global styles and the `vue-multiselect` CSS
  imports stay global. Found while scoping `editDraftOrderForm` in 13c:
  - `.ranking-column` is defined in `criticsRoyale` and `royaleGroupQuarter` (50px) and `royaleHistory`
    (80px). They're global, so whichever page loaded last sets the width on all three.
  - `.reset-button-flex` is defined in both `leagueTagSelector` and `conference/assignPlayers`.
  - Classes given to `b-table` through a field's `thClass`, `tdClass` or `class`, or through
    `tbody-tr-class` (`playerGameTable` and the three royale pages), are rendered by `BTable`, so scoped
    rules need `:deep()`. A child component's root element and slot content written in the template do
    get the scope; `editDraftOrderForm`'s `VueDraggable` list is styled that way, and so is
    `leagueTagSelector`'s.
  - `minimalPlayerGameTable`'s `.score-column` is also used by its child `minimalPlayerGameSlotRow`, and
    `.share-button` by `views/publisher.vue`, which only gets it if the League page's styles already
    loaded.
  - `masterGameTagBadge` sets `.popover-header { color: black }`, which reaches every popover in the app.
    Popovers are teleported, so if that's meant to be global it belongs in `css/site.css`.
  - `masterGameCreator` and `masterGameRequest` style `.vue-slider-piecewise-label`, but there's no
    `vue-slider` dependency any more. Remove both.
  - `conference/assignPlayers` has `display: bcollapse`, which isn't valid CSS.

  Done. Every component's `<style>` is scoped except `App.vue`'s and the two `vue-multiselect` imports.
  Each changed page's computed styles were captured before and after; the only difference is the royale
  group page's Rank column (see "Found along the way"). `masterGameTagBadge`'s popover z-index uses
  `:global()` on the popover's own class, because `b-popover` teleports the popover without the scope.

Then make the API changes held in [api-breaking-changes-plan.md](api-breaking-changes-plan.md).

## Found along the way

Bugs found by the conversion, and every deliberate behaviour change. "On main" says whether production
has the bug too.

| Step | Where | What | On main | Fix |
|---|---|---|---|---|
| — | `views/masterGameRequest.vue` `sendMasterGameRequestRequest` | Reads `error.response.data` on an `ApiException`, whose `response` is a string, so `errorInfo` is `undefined` and a failed request shows no error | Yes | Done: `getApiErrorMessage`, and the alert now shows the message instead of a generic one |
| 1 | `components/royalePublisherGraph.vue` | Passes `chart-id` to vue-chartjs 5's `Line`, which has no `chartId` prop, so it falls through as a `chart-id` attribute instead of setting the canvas id (found with `checkUnknownProps` on). TypeScript reports only the first excess property, so check `height` and `styles` too | Yes | Graph done: `chart-id`, `height` and `styles` removed, with its `chartHeight` prop; the containers set the height. No page passes them any more. Done |
| 2 | `store/publisherStore.ts` `confirmPositions` | Now uses `getApiErrorMessage`. Changes: a 403 or 404 shows "Something went wrong." where it showed nothing (empty body); a 500 shows that instead of the error page's text; a network failure is rethrown after leaving move mode, where it showed "TypeError: Failed to fetch" | — | Done |
| 2 | `store/publisherStore.ts` | `moveGameError` is never cleared, not by a later successful move and not by `clearPublisherStoreData`, so after one failed move the error stays up, even on another publisher's page | Yes | Done: cleared when a move is confirmed and in `clearPublisherStoreData`; `tests/store/publisherStore.test.ts` |
| 4 | `components/leagueActions.vue` | Passes `:possible-league-options` to `leagueOptionsModal`, which has no such prop and reads the options from the store, so the value falls through as an attribute. `checkUnknownProps` is off, so type-check won't flag it. It's the only use of the `possibleLeagueOptions` `mapState` entry, so remove both | Yes | Step 13d |
| 4 | `views/masterGameCreator.vue` | Passes `supportedYears.find(...)` to `leagueOptionsModal`'s required `supportedYear`. `find` can return `undefined`, and the modal reads `supportedYear.year`. Type-check should flag it once the page is converted; decide then what happens if no year matches | Yes | Done in step 15: a league whose year isn't supported keeps its link, without "(Settings)" or the modal |
| 5 | `components/toastHost.vue` | The close icon is now a native button named "Close", so a keyboard can reach it. `hide` is now called with no arguments, where it was passed the `MouseEvent` as its trigger | — | Done |
| 5 | `components/toastHost.vue` | Toasts had no fixed position on this branch, so they rendered in page flow just above the footer, off-screen on any long page. bootstrap-vue-next doesn't position a declarative `b-toast` the way BootstrapVue's default toaster did. Also, `.toast-body` didn't fill the toast, so the close button sat against the message | No | Done: pinned top right with `toast-container`; `.toast-body` grows |
| 5 | `components/toastHost.vue` | Toasts never auto-hide on this branch. `visible` shows a `b-toast` without starting its countdown, which only runs when `modelValue` is a number, so a toast stayed until it was closed, even though `vue3-upgrade-plan.md` records auto-hide as checked | No | Done: `:model-value="5000" no-progress` in place of `visible`. Hovering pauses the countdown |
| 5 | `css/site.css`, `views/welcome.vue` | Dropped the overrides that kept Font Awesome 6's icon and layer widths, along with the `left-*` transforms that compensated for them, so icons now use Font Awesome 7's 1.25em default. It looks better on the welcome page. Other pages get checked and adjusted as their steps come up | — | Done |
| 6 | `components/gameTables/masterGamesTable.vue`, `views/actionProcessingDryRunResults.vue` | The table's `gameRows` computed turned rows with `error` red by writing to its prop. Only the dry run's games have `error`, so that page now marks its own rows and the table no longer does | — | Done |
| 6 | `views/actionProcessingDryRunResults.vue` | Passes `MasterGameViewModel[]` to `masterGamesTable`, which takes `MasterGameYearViewModel[]`, and omits the required `show-year-stats`. The year columns mean nothing there, so the page probably wants its own small table. Type-check will flag the prop once the page is converted | Yes | Done in step 15: its own table of name, release date and tags, sortable by the first two, without the filter and paging controls. Failed games are red through `tbody-tr-class` |
| 6 | `components/masterGamePopover.vue`, `components/masterGameSummary.vue` | Both took only `MasterGameYearViewModel`, but pages such as Recent Changes pass a `MasterGameViewModel`, which has no year stats. The popover showed "0%", "Undrafted" and "Unhyped..." for every such game. Both now take either type, and the summary hides the year rows when there's no `year` | Yes | Done |
| 6 | `views/masterGame.vue` | The sub-games section called `releaseDate` and `openCriticLink` methods that `b3e56008f` (June 2019) moved into `masterGameDetails`, so any game with sub-games has failed to render that section since. It now uses `formatDateLong` and its own `openCriticLink` | Yes | Done |
| 6 | `views/masterGame.vue` | Every failed API call set `error`, which nothing displayed, so an unknown master game ID gave a blank page. Errors now go through `getApiErrorMessage` and show in a `b-alert` at the top of the page. A 404 shows "Something went wrong." | Yes | Done |
| 6 | `GameController.LeagueYearsWithMasterGame` | `LeagueName` was nullable from the domain class through the view model and generated client, though `tbl_league.LeagueName` is `NOT NULL` and the query inner-joins it. It's non-nullable throughout now, and the domain type is a record that the view model is built from | — | Done |
| 6 | `components/masterGameYearStats.vue` | Like `royalePublisherGraph`, it passed vue-chartjs 4's `chart-id`, `height` and `styles` to vue-chartjs 5's `Line`, which has none of them, so they fell through to the canvas as attributes (`styles="[object Object]"`). Removed; the 520px container already sets the height. The tooltip's percent check now looks up the series by dataset index instead of a custom `percentageScale` dataset property | Yes | Done |
| 6 | `views/topBidsAndDrops.vue` | A failed load set `errorInfo`, which nothing displayed, and left the spinner up forever. Errors now show in a `b-alert` in place of the spinner. Both year dropdowns had `id="year-dropdown"`, which clashed when the end-of-year alert showed; the alert's is now `year-within-process-date-dropdown` | Yes | Done |
| 6 | `views/publicLeagues.vue` | A failed load wasn't caught, so the page showed nothing. It now shows a `b-alert`. The pagination's `aria-controls="my-table"` pointed at no element; the table is now `public-leagues-table` | Yes | Done |
| 6 | `components/gameStats/longestTenuredGamesPanel.vue`, `components/gameStats/mostDreamsDashedPanel.vue` | A failed load wasn't caught, so the spinner stayed up forever. Errors now show in a `b-alert`. These panels and `topBidsAndDrops` clear the error at the start of each load, so it goes away when a later load succeeds | Yes | Done |
| 6 | `mostDreamsDashedPanel`, `masterGames`, `masterGameYearStats`, `publicLeagues`, `topBidsAndDrops` | Changing a dropdown while an earlier load is still running lets whichever response arrives last win. Seen on Most Dreams Dashed: a slow all-time request failed after the 2018 request succeeded, and its alert replaced the 2018 results. Fix by ignoring responses for anything other than the current selection | Yes | Not scheduled |
| — | `views/criticsRoyale.vue`, `components/possibleRoyaleMasterGamesTable.vue`, `components/possibleMasterGamesTable.vue` | Each `b-pagination` has `aria-controls="my-table"`, but no element has that id. Give each table a real id and point `aria-controls` at it, or drop the attribute. `gameNews` was done in step 8 (`game-news-table`) | Yes | Done: `criticsRoyale` (`royale-standings-table`) and `possibleRoyaleMasterGamesTable` (`possible-royale-master-games-table`) in step 9. In step 13a, `possibleMasterGamesTable` and `possibleCounterPicksTable` (whose `aria-controls="counter-pick-table"` also pointed at nothing) get an id from `useId()`, since several modals use them and more than one can be on the page |
| 9 | `components/modals/createRoyalePublisherForm.vue` | A failed create was swallowed, so the modal did nothing. A name of only spaces passes the button's check and gets "You cannot have a blank name." from the server. The error now shows in a `b-alert` in the modal, cleared on each submit and when the modal closes | Yes | Done |
| 9 | `views/criticsRoyale.vue` | A failed load wasn't caught, so the leaderboard spinner stayed up forever. Errors now show in a `b-alert` in place of the spinner | Yes | Done |
| 9 | `views/criticsRoyale.vue` | Switching quarters kept the pagination page, so page 4 of one quarter showed an empty table in a quarter with fewer publishers, and the old quarter's standings, podium, chart and "View My Publisher" stayed up until the new ones arrived. Each load now goes back to page 1 and clears them, so the spinner shows while loading | Yes | Done |
| 9 | `components/royaleGroupsWidget.vue` | "No groups yet." and "No featured groups." showed while the groups were still loading. They now wait for the load | Yes | Done |
| 9 | `components/modals/royaleChangePublisherNameForm.vue`, `IconForm`, `SloganForm` | A failed change was swallowed, so the modal did nothing. The error now shows in a `b-alert`, cleared on each submit and when the modal opens or closes | Yes | Done |
| 9 | `components/modals/royalePurchaseGameForm.vue` | A failed search or purchase wasn't caught. A purchase while the site is read-only during action processing gets a 400, and the Purchase button stayed disabled until the modal was reopened. Both now show a `b-alert` and re-enable the button. The unused `yearQuarter` prop is gone | Yes | Done |
| 9 | `components/modals/royalePurchaseGameForm.vue` | Selecting a game that can't be bought (already owned, locked, ineligible) showed only "Selected Game: X" and no Purchase button, with no reason; the Status column that says why is hidden below 992px. A warning now takes the Purchase button's place: "X can't be bought: " and the server's status. Select stays enabled on every row | Yes | Done |
| 9 | `views/royalePublisher.vue` | A failed load set `error`, which didn't exist and nothing displayed, so the page was blank. It now shows a `b-alert`. A sell or budget error stayed up after a later success; it's now cleared on each attempt | Yes | Done |
| 9 | `views/royalePublisher.vue` | The advertising budget input was a text box bound as a string; it's now a number input bound with `.number`, and its label points at it. The games table passed `currently-ineligible` to `masterGamePopover`, which has no such prop (nor did main's), so it was dropped. The won-quarters row tested the array itself, so an empty row rendered for every publisher; it now tests the length | Yes | Done |
| 9 | `views/royaleHistory.vue` | A failed load set the history to `null`, so the spinner stayed up forever. It now shows a `b-alert`. Moving to another player's history cleared nothing, so the old player's showed until the new one loaded | Yes | Done |
| 9 | `views/royaleGroup.vue`, `components/modals/royaleGroupInviteLinksModal.vue` | Removing a member or leaving the group wasn't caught, so a failure did nothing visible. A failed invite link load showed an empty list. All three now show a `b-alert` | Yes | Done |
| 9 | `views/royaleGroupQuarter.vue` | Switching quarters kept the old quarter's standings up until the new ones arrived, and if the new load failed they stayed up with no message. Each load now clears them and `notFound`, so the spinner shows while loading and a failure shows "Group or quarter not found." | Yes | Done |
| 9 | `components/modals/sellRoyaleGameModal.vue` | Typos: "recieve", "If the games is" | Yes | Done |
| 8 | `components/gameNews.vue` | The league pills were keyed on `leaguePublisherSet.LeagueID`, but the API sends `leagueID`, so every pill's key was `undefined` and a game in two or more of the user's leagues had duplicate keys | Yes | Done |
| 7 | `views/masterGameChangeRequest.vue` | A failed request showed "An error has occurred with your request." instead of the server's message, and the error stayed after a later success. After a successful submit, the cleared note showed "The Request Note field is required" because the form was never reset | Error text: Yes. Reset: not checked | Done: shows `errorInfo`, clears it per submit, calls `resetForm()` |
| 8 | `views/home.vue` | A failed home page load set `errorInfo`, which nothing displayed. It now shows in a `b-alert` | Yes | Done |
| 8 | `views/home.vue`, `components/leagueInviteTable.vue` | The League Invites tab passed `CompleteLeagueInviteViewModel` rows to `leagueTable`, which takes `LeagueWithStatusViewModel`. Invites now have their own table (envelope icon, league link, manager), so `leagueTable` keeps its exact type. No visible change | — | Done |
| 8 | `components/leagueTable.vue` | The archive and un-archive icons are now native buttons named "Archive league" and "Un-archive league", so a keyboard can reach them. Icon margins and transforms were adjusted for Font Awesome 7 | — | Done |
| — | `router/index.ts` | vue-router 5 warned on every navigation that `next()` in guards is deprecated. The guard now returns `'/home'`, `true` or `false`. The signed-out redirect to login now cancels the navigation, so `afterEach` skips the title when a navigation fails | — | Done |
| 10 | `components/leagueYearSettings.vue` | Total games, counter picks, minimum bid and the three droppable counts were text inputs with plain `v-model`, so after an edit they held strings such as `"6"`. The server read them anyway, but `DraftCreationSettings`'s `Number` prop got a string. They're `type="number"` now, with spinner arrows | Yes | Done |
| 10 | `components/leagueCreationPresets.vue` | The player count was a text input with `v-model.number`, so letters were stored as a string and produced a preset with `NaN` counts. It's `type="number"` now, and Select Preset needs a whole number from 2 to 20 | Yes | Done |
| 10 | `components/leagueTagSelector.vue` | Reset Changes didn't emit, so after a drag the lists reset but the dragged value was what got saved | Yes | Done |
| 10 | `components/leagueTagSelector.vue` | The lists were built once and never re-read, so re-applying a preset with a different experience level left the old lists up, and the next drag sent them back over the preset. The lists now come from the value | Yes | Done |
| 10 | `components/leagueTagSelector.vue` | Banned system or unknown tags were hidden and silently un-banned on save; they now show in Banned and stay banned. Only one league has ever banned a system tag (`Cancelled`, in 2022). Banned follows the saved order instead of the store's; Allowed can't be reordered, since there's nothing to save | Yes | Done |
| 10 | `components/specialGameSlotSelector.vue` | `getTags` drops tag names it can't find instead of pushing `undefined` | — | Done |
| 10 | `views/createLeague.vue`, `views/createConference.vue` | New flow: choose a preset with Select Preset before any settings show. The year has a default and is locked once a preset is chosen. "Select a different preset" throws away edits to the settings. A name of only spaces counts as missing, and a missing name says so instead of "Some of your settings are invalid". Errors show in a `b-alert`, cleared on each submit | — | Done |
| 10 | `views/createLeague.vue` | The "Custom Rules League" vs "Test League" explanation was a `b-alert` with no `show`, so it had never been visible. It shows now | Yes | Done |
| 10 | `components/leagueTagSelector.vue` | At phone width the create pages scroll sideways, because the two tag columns are wider than the screen | Not checked | Not scheduled |
| 10 | `views/editLeague.vue` | Passed no `game-mode` to `leagueYearSettings`, so a one-shot league was edited as `Standard`: the bidding section showed and `created()` turned `enableBids` on, which saving kept. It now passes One Shot, Multi Draft or Standard | Yes | Done |
| 10 | `components/DraftCreationSettings.vue` | Clearing a draft's date sets it to `""`, which was sent as is, and the server can't read `""` as a date, so the create or edit failed. A cleared date is now sent as no date | Yes | Done |
| 10 | `components/modals/addNewLeagueYearForm.vue`, `components/modals/conference/addNewConferenceYear.vue`, `views/editLeague.vue` | editLeague read a `freshSettings` query that nothing set. Starting a new league or conference year now sets it, so changing the standard game count recomputes the special slots, and the multi-draft "Visit the Manage Drafts page" alert is hidden | — | Done |
| 10 | `views/manageDrafts.vue` | Adding a draft now always sends "counter picks must be from this draft", where it was sent only when the draft had counter picks (the server defaults it to true). With no counter picks it has no effect | — | Done |
| 11 | `components/gameTables/playerGameTable.vue`, `PublisherViewModel` | Sort mode with "Include Dropped Games" built fake slots for former games on the client, missing most of a slot's fields, and numbered them with `Math.max(array)`, which is `NaN` when there's more than one slot. The server now sends `FormerGameSlots`. A former game's projection is still `~--`; the cover view hides the projection when it's `null` | Yes | Done |
| 11 | `components/gameTables/playerGameTable.vue` | Rows were keyed on `overallSlotNumber`. bootstrap-vue-next keys a row by its index when the key is falsy, and the first standard slot is 0, so after a sort two rows shared a key and extra rows appeared. The key is now the number as a string | Not checked | Done |
| 11 | `components/gameTables/playerGameTable.vue` | Warning rows were marked by writing `_rowVariant` onto the store's slot objects from inside a computed. A `tbody-tr-class` function does it now. No visible change | — | Done |
| 11 | `components/gameTables/gameNameColumn.vue` | Uses `masterGame` instead of `linked` to decide whether a game is linked | — | Done |
| 11 | `components/gameTables/gameNameColumn.vue` | Read `leagueYear` from the league store, so on the publisher page the counter-pick source icon looked in whichever league was last opened, and showed nothing if none was. It now takes `leagueYear` as a prop from its parent | Yes | Done |
| 11 | `views/publisher.vue` | A failed load wasn't caught, so the page was blank. The store now sets `errorInfo`, cleared on each load, and the page shows it in a `b-alert` | Yes | Done |
| 11 | `views/publisher.vue` | Previous and Next Publisher are hidden in a league with fewer than two publishers, where both linked to the same publisher | — | Done |
| 11 | `views/publisher.vue` `sharePublisher` | The watermark and the 1920px viewport were reset straight away, without waiting for `html2canvas`. They're now reset in a `finally` after the capture and the share, and the viewport goes back to its original content, where it was set to `width=device-width` and lost `initial-scale=1.0` | Not checked | Done |
| 11 | `utilities/shareImage.ts`, `views/publisher.vue`, `components/gameTables/minimalPlayerGameTable.vue` | `html2canvas` re-implemented CSS rendering and got things wrong. It's replaced by `modern-screenshot`, which lets the browser render the copy. A browser that can't share files, such as desktop Firefox, threw on `navigator.share`; it now downloads the PNG. Cancelling the share sheet is no longer an error | Yes | Done |
| 11 | `components/gameTables/minimalPlayerGameTable.vue` | `renderingSnapshot` was cleared before the capture finished, so whether the Details link and Share button were hidden in the image depended on timing. It's now cleared after the share | Yes | Done |
| 12 | `components/leagueActionSet.vue` | A conditional drop of a game with no master game showed an empty cell, because `masterGamePopover` rendered nothing without one. It now shows the game's name. `masterGamePopover` no longer checks for a master game; its callers do | Yes | Done |
| 12 | `components/tradeSummary.vue` | Errors now use `getApiErrorMessage`. The five trade actions share one fallback, "Something went wrong with your request.", which a 5xx now shows instead of the error page's text | — | Done |
| 12 | `views/leagueAllTimeStats.vue` | A failed load set `errorInfo`, but the page never showed it, so it stayed blank. It now shows in a `b-alert`, cleared on each load | Yes | Done |
| 13a | `sp_getcombinedleagueyearuserstatus`, `MySQLFantasyCriticRepo` `GetInvite`/`GetOutstandingInvitees` | The three invite queries returned the invited user's name and email as `UserName`/`UserEmailAddress`, but `LeagueInviteEntity` reads `InviteUserName`/`InviteUserEmailAddress`, so an invite to an existing user had a user with a null name. The standings showed "Invite Sent" with no name, and `inviteName` was null rather than the email address | Yes | Done |
| 13a | `components/leagueYearStandings.vue` | Rescinding an invite swallowed any failure. The error now shows in a `b-alert` above the table, cleared on each attempt | Yes | Done |
| 13a | `components/leagueYearStandings.vue` | The pre-draft "Games Drafted" column sorted on `gamesWillRelease` from the row, which only has it under `publisher`, so the sort did nothing. It now reads the publisher, and ties go by draft position | Yes | Done |
| 13a | `components/leagueYearStandings.vue` | Rows without a publisher (invites, players who haven't created one) have no points and a draft position of 0, so they sorted to the top by draft position, and by points they compared equal to every row and could land anywhere. They now stay at the bottom in either direction | Yes | Done |
| 13a | `components/leagueYearStandings.vue` | Once play started, rows without a publisher showed "-- - 0th" in both points columns, from a missing score and a ranking of 0. Those cells are now blank, like the row's other publisher columns | Yes | Done |
| 13a | `components/leagueYearStandings.vue` | The winner's row is highlighted with a `tbody-tr-class` function instead of writing `_rowVariant` onto the store's players. No visible change | — | Done |
| 13a | `components/activeBids.vue` | The default sort named `maximumReleaseDate`, but the column is `masterGame.maximumReleaseDate`, so it never applied: This Week's Public Bids showed the server's order with no sorted column. It now sorts by release date | Yes | Done |
| 13a | `components/possibleMasterGamesTable.vue` | The default sort named `dateAdjustedHypeFactor`, but the column is `masterGame.dateAdjustedHypeFactor`, so it never applied. That was what kept the server's order, which ranks games by how well they match the search; correcting the key sorted every game search modal by hype factor instead. The default sort is now removed, so the search order stays | Yes | Done |
| 13a | `components/gameTables/minimalPlayerGameTable.vue` | The rounding warning checked `userInfo.showDecimalPlaces`, but the field is `showDecimalPoints`, so it showed even to users who have decimal points turned on | Yes | Done |
| 13b | `components/modals/gameQueueForm.vue` | Failures were swallowed: a failed search showed nothing, and setting rankings, saving notes or removing a game wasn't caught at all. A failed add set `errorInfo`, which nothing displayed. Errors now show in a `b-alert` at the top of the modal, cleared on each search or write and when the modal closes. The spinner now shows during every write, not only an add | Yes | Done |
| 13b | `components/modals/gameQueueForm.vue` | The special slot badges were keyed on `overallSlotNumber`, which the slot requirements don't have, so every badge's key was `undefined`. They're keyed by index, and the selected badge is the selected slot rather than a separate index, as in `bidGameForm` | Yes | Done |
| 13b | `components/modals/gameQueueForm.vue` | "No games available to display." showed while a load was running and after a failed one. It now shows only when a load finishes with no games | Yes | Done |
| 13b | `components/modals/gameQueueForm.vue` | The watchlist table was copied from the store when the modal closed, so on reopening, statuses changed since then (a game taken by a bid or draft pick) were stale. The league year refresh on open didn't reach the table. Opening now re-reads the store's watchlist, and the refresh is gone | Yes | Done |
| 13b | `components/modals/proposeTradeForm.vue` | Rows were keyed on `item.publisherGameID`, which the `{ id, game }` row objects don't have, so every key was `undefined` and Vue patched the rows by position. They're keyed on `item.id` now. No visible change | Yes | Done |
| 13b | `components/modals/proposeTradeForm.vue` | The validation warning and the server error weren't cleared when the modal closed, so reopening it showed the old message. Closing now clears both | Yes | Done |
| 13b | `components/modals/proposeTradeForm.vue` | Errors use `getApiErrorMessage`. A 5xx shows "Something went wrong with your trade proposal." instead of the error page's text | — | Done |
| 13b | `components/modals/createPublisherForm.vue` | A failed create was swallowed, so the modal did nothing. The error now shows in a `b-alert`, cleared on each submit and when the modal closes. Create Publisher is disabled while the request runs, and for a name of only spaces, which the server rejects with "You cannot have a blank name." | Yes | Done |
| 13b | `views/league.vue` | Passed `:league-year` to `createPublisherForm`, which has no props and reads the store, so it fell through to the modal as an attribute. Removed | Yes | Done |
| 13b | `components/modals/changePublisherNameForm.vue`, `IconForm`, `SloganForm` | A failed change was swallowed, so the modal did nothing. The error now shows in a `b-alert`, cleared on each submit and when the modal opens or closes. The submit button is disabled while the request runs, and Change Name also for a name of only spaces. The name toast shows the trimmed name that was saved | Yes | Done |
| 13b | `components/modals/changePublisherSloganForm.vue` | With no slogan set, clicking Change Slogan without typing threw on `.trim()` of the missing slogan, so nothing happened. It now sends no slogan | Yes | Done |
| 13b | `components/modals/editAutoDraftForm.vue` | The mode and the watchlist box were read from the store once, when the League page loaded, so after a manager changed your auto draft (Edit Player Auto Draft) the modal showed the old setting, and saving put it back. They're now read each time the modal opens. A failed save was swallowed; it now shows in a `b-alert` | Yes | Done |
| 13b | `components/modals/playerDraftCounterPickForm.vue` | A failed load or draft was swallowed, and a failed load showed "No games are available for counter picks." Errors now show in a `b-alert`, and that message shows only after a load succeeds. A failed draft's errors stayed up when the modal was closed and reopened; closing now clears them | Yes | Done |
| 13b | `components/modals/specialAuctionsModal.vue` | The game search pasted the name into the query string unencoded, so `Ratchet & Clank` searched for `Ratchet ` and anything after a `#` was dropped. The generated client encodes it | Yes | Done |
| 13b | `components/modals/specialAuctionsModal.vue` | Create Special Auction with no end time sent `new Date(null)`, which is 1970-01-01. The button is now disabled until a game and an end time are set | Yes | Done |
| 13b | `components/modals/specialAuctionsModal.vue` | A failed search wasn't caught, and a failed create or cancel showed axios's raw response. Errors now use `getApiErrorMessage` and show in a `b-alert` at the top of the modal, cleared on each request and when the modal closes | Yes | Done |
| 13b | `components/modals/specialAuctionsModal.vue` | Creating an auction now closes the modal; cancelling one keeps it open. The search results hide once a game is selected, as in `bidGameForm`. Create and Cancel are disabled while a request runs. The end time is cleared when the modal closes | — | Done |
| 13c | `components/modals/setPauseModal.vue`, `resetDraftModal`, `undoLastDraftActionModal`, `skipCurrentDraftPickModal` | A failed request was swallowed (pause logged it to the console), so the modal closed and nothing happened. The modal still closes on OK; a failure now shows a red toast with the server's message | Yes | Done |
| 13c | `components/modals/setPauseModal.vue` | Sent a `draftID` that `DraftPauseRequest` doesn't have; the server pauses the active draft. It's no longer sent. No visible change | — | Done |
| 13c | `components/modals/managerSetAutoDraftForm.vue` | A failed save was swallowed, so the modal did nothing. It now shows in a `b-alert`, cleared on open and on each save. Set Auto Draft is disabled while the request runs | Yes | Done |
| 13c | `components/modals/managerDraftCounterPickForm.vue` | A failed load or draft was swallowed. Errors now show in a `b-alert`. The draft result wasn't cleared on close, so reopening showed the last error; closing now clears it, and choosing another game clears it and the override box. The submit button is disabled while the request runs | Yes | Done |
| 13c | `components/modals/managerDraftGameForm.vue` | Uses `playerDraftGameForm`'s search: a response for an older search is ignored, and a failed search or draft shows in a `b-alert`. A failed draft left the buttons disabled until the modal was reopened. The "Draft Unlisted Game" buttons are gone, as in `playerDraftGameForm`; "Select unlisted game" is in the alert under the results, which shows as soon as the top games load. The special slot badges were keyed on `overallSlotNumber`, which the slot requirements don't have. Choosing another game clears the override and ineligible-slot boxes. Typo: "Do you want draft this game anyway anyway?" | Yes | Done |
| 13c | `components/modals/managerDraftGameForm.vue`, `playerDraftGameForm.vue` | The slot search put the base64 slot in the query with `encodeURI`, which leaves `+` and `/` alone, so a `+` reached the server as a space. The generated client encodes it with `encodeURIComponent` | Yes | Done |
| 13c | `components/modals/transferManagerModal.vue` | A failed transfer wasn't caught, so the modal stayed open with no message. It now shows in a `b-alert`, cleared on each attempt and on close. Transfer League Manager is disabled until a new manager is chosen (clicking it with none threw a `TypeError`) and while the request runs | Yes | Done |
| 13c | `components/modals/removePlayerModal.vue`, `removePublisherModal`, `disconnectPlayerModal` | Errors use `getApiErrorMessage` and show in a `b-alert`; a 5xx shows the fallback instead of the error page's text. The submit button is disabled while the request runs | — | Done |
| 13c | `components/modals/reassignPublisherModal.vue` | The inactive players were keyed on `user.id`, which `PlayerViewModel` doesn't have, so every key was `undefined`. The typed confirmation wasn't cleared after a reassign or on close, so the next reassign started with "Reassign Publisher" already typed. Both are fixed. Errors use `getApiErrorMessage`, and the button is disabled while the request runs | Yes | Done |
| 13c | `components/modals/createPublisherForUserForm.vue` | The player options were keyed on `player.userID`, which `PlayerWithPublisherViewModel` doesn't have; the options are now the players' users. A failed create was swallowed; it now shows in a `b-alert`. Create Publisher is disabled until a player is chosen (clicking it with none threw a `TypeError`), for a name of only spaces, and while the request runs. The toast shows the trimmed name that was saved | Yes | Done |
| 13c | `components/modals/managerEditPublishersForm.vue` | The budget and the four drop counts were text inputs sent as strings; they're `type="number"` now. The error showed under the form and stayed up after closing or choosing another publisher; it's now a `b-alert` at the top, cleared on each submit, on choosing a publisher and on close. Edit Publisher is disabled while the request runs | Yes | Done |
| 13c | `components/modals/manageActivePlayersForm.vue` | Errors use `getApiErrorMessage`, and Set Active Players is disabled while the request runs. Each checkbox is labelled with its player. The rows are a list instead of an object keyed by user ID; no visible change | — | Done |
| 13c | `components/modals/invitePlayerForm.vue` | A failed load of the invite links was swallowed, showing no links. It now shows in a `b-alert`, as the other errors do, through `getApiErrorMessage`. Send Invite, Create Invite Link and Delete are disabled while a request runs | Yes | Done |
| 13c | `components/modals/managerClaimGameForm.vue` | "Select unlisted game." copied the search into `draftUnlistedGame`, which the form doesn't use, so the custom name box started empty; it now starts with the search text, as in the draft forms. A failed search or claim was swallowed; both show in a `b-alert`. The search sent the name unencoded, as `specialAuctionsModal` did. Choosing another game clears the result and the override and ineligible-slot boxes. Add and Search are disabled while a request runs. Typo: "add this game anyway anyway?" | Yes | Done |
| 13c | `components/modals/removeGameForm.vue` | The error showed under the Remove Game button and wasn't cleared on close, so reopening showed the old error. It's now a `b-alert` at the top, cleared on each attempt and on close. Remove Game is disabled while the request runs. The modal was wrapped in a `<form>` that did nothing; it's gone | Yes | Done |
| 13c | `components/modals/manuallySetWillNotReleaseForm.vue` | A failed request set `errorInfo`, which nothing displayed. It now shows in a `b-alert`. The buttons are disabled while the request runs | Yes | Done |
| 13c | `components/modals/manuallyScoreGameForm.vue` | The score was a text box sent as a string; it's `type="number"` now (decimals allowed). After ticking "Remove Manual Score?" and choosing a game with no manual score, the box was hidden but still ticked, so the button offered to remove a score that wasn't there. Choosing a game now clears it. The error showed under the button; it's now a `b-alert` at the top. The button is disabled while the request runs | Yes | Done |
| 13c | `components/modals/manageEligibilityOverridesModal.vue`, `manageTagOverridesModal` | A failed search was swallowed, and the search sent the name unencoded. Errors use `getApiErrorMessage` and show in a `b-alert` at the top instead of under the form. The buttons are disabled while a request runs. Toast typos: "had it's tags reset", "overriden" | Yes | Done |
| 13c | `components/modals/changeLeagueOptionsForm.vue` | The options were read from the store once, when the League page loaded, and closing reset only the name. So after unticking Public League and closing without saving, reopening showed it still unticked, and after a save elsewhere it showed the old values. They're now read each time the modal opens. The error is cleared on open. Change Settings is disabled for a name of only spaces and while the request runs | Yes | Done |
| 13c | `components/modals/changeLeagueOptionsForm.vue` | The "Custom Rules League" vs "Test League" explanation was a `b-alert` with no `model-value`, so it never showed, as on `createLeague` in step 10. It shows now | Yes | Done |
| 13c | `components/modals/managerMessageModal.vue` | A failed post was swallowed; it now shows in a `b-alert`, cleared on each post and on close. Post Message is disabled for a message of only spaces and while the request runs. The "Show message to users not in league?" label now points at its checkbox | Yes | Done |
| 13c | `components/modals/underReviewModal.vue` | Errors use `getApiErrorMessage`: a 5xx shows "Request failed." instead of the error page's text | — | Done |
| 13c | `LeagueManagerController` `AvailableYears`, `ManagerAssociateGame` | Neither declared a typed 200 response, so the generated clients returned `void`. They now declare `List<int>` and `ManagerClaimResultViewModel`. `ConferenceController.AvailableYears` has the same gap; step 14 | — | Done, and `ConferenceController.AvailableYears` and `GetConference` in step 14 |
| 13c | `components/modals/managerAssociateGameForm.vue` | A failed search or associate was swallowed, and the search sent the name unencoded. Both now show in a `b-alert`. Choosing another publisher kept the game chosen from the previous one, so the request paired one publisher with another's game; it now clears the game. Associate is disabled until a publisher game is chosen (submitting without one threw a `TypeError`) and while the request runs. Choosing another game clears the result and the override box | Yes | Done |
| 13c | `components/modals/addNewLeagueYearForm.vue` | A failed load or start set `error`, which nothing displayed, so the year list was empty or the button did nothing. Both now show in a `b-alert`. The chosen year is cleared each time the modal opens, and Start New Year is disabled while the request runs | Yes | Done |
| 13c | `components/modals/editDraftOrderForm.vue` | The order list was read from the store on load and on close, so a publisher added while the page was open didn't show until the modal had been closed once. It's now read each time the modal opens. A failed attempt's error and the expanded manual list stayed up after closing; opening now clears both. The order buttons are disabled while the request runs, so a double click no longer sends two requests (two random orders, for Random). Errors use `getApiErrorMessage`, so a 5xx shows the fallback instead of the error page's text. The manual rows are keyed on `publisherID` instead of `draftPosition`. Dragging works on a copy of the list, so it never reorders the store's publishers | Yes | Done |
| 13c | `components/modals/editDraftOrderForm.vue` | The styles are scoped. They were global, so `.list-group { min-height: 20px }` would reach any other Bootstrap list group once the League page had loaded; nothing else uses one today, so no visible change. `.ghost` was never applied (no `ghostClass` is set) and is gone. `.draft-order-item i` never matched, since Font Awesome renders an `svg`; the drag handle now gets the pointer cursor it meant to give | — | Done |
| 14 | `MySQLConferenceRepo.GetLeagueYearsInConferenceYear` | The `WHERE` used a bare `Year`, which became ambiguous when fe9c9def1 (May 2026) joined `tbl_league_draft`. `AssignLeaguePlayers` calls it for every earlier year of the conference, so Assign Players returned a 500 for any conference in its second year or later. The columns are qualified now. Checked against the snapshot; there's no integration test, because the test database has only one open year, so no conference can have two | Yes | Done |
| 14 | `components/conferenceYearStandings.vue` | In a finished year the table dropped the actual points and kept the projected ones, labelled "Points", while still sorting by actual points. A publisher whose projection didn't match their score showed the wrong total out of order (Kinda Funny Best Friends Conference 2025: Soonami Studios showed 61.67, 16th, at the bottom; actual 46.67, 19th). It now shows actual points as "Points", as the league standings do | Yes | Done |
| 14 | `components/conferenceYearStandings.vue` | The leader's ranking was meant to be bold, but the check read `item.publisher`, which conference standings rows don't have, so it never was. The "- 1st" in each points column is bold now | Yes | Done |
| 14 | `components/conferenceYearStandings.vue` | League Rank shows an ordinal ("1st") once play has started and is blank before, like the other rankings; it showed a bare number, and the template had a slot for a `projectedLeagueRanking` column that doesn't exist. The winner's row is highlighted with a `tbody-tr-class` function instead of writing `_rowVariant` onto the store's standings. Unused styles removed | — | Done |
| 14 | `components/modals/conference/assignPlayers.vue` | The lock state came from the store, which a lock or unlock didn't refresh, so after locking a league, closing and reopening showed it unlocked, and clicking the lock got "That league is already locked." A lock change now refreshes the conference year | Yes | Done |
| 14 | `components/modals/conference/assignPlayers.vue` | The lock and collapse icons are native buttons named "Lock/Unlock/Collapse/Expand <league>". Errors use `getApiErrorMessage` and show in a `b-alert`. The lock buttons and Assign Players to Leagues are disabled while a request runs. The leagues are a list instead of six objects keyed by league ID, and the lists are new arrays of the store's players, without `deepClone`. No visible change | — | Done |
| 14 | `components/modals/conference/addNewConferenceYear.vue` | Nothing displayed `errorInfo`, and a failed load of the years wrote to `error`, which doesn't exist, so failures did nothing visible. `clearData` cleared `newYear`, which doesn't exist, so the chosen year stayed. Add New Conference Year was enabled with no year and sent `""`. Errors now show in a `b-alert`, the year is cleared each time the modal opens, and the button is disabled until a year is chosen and while the request runs | Yes | Done |
| 14 | `components/modals/conference/addLeagueToConference.vue` | Errors use `getApiErrorMessage` and show at the top instead of under the form. Add New League is disabled for a name of only spaces, which the server rejects with "You cannot have a blank league name.", and while the request runs. The League Manager label points at its select | — | Done |
| 14 | `components/modals/conference/conferenceManagerMessageModal.vue` | A failed post was swallowed. It now shows in a `b-alert`, cleared on each post and on close. Post Message is disabled for a message of only spaces and while the request runs. The labels point at their fields | Yes | Done |
| 14 | `components/modals/conference/editConferenceSettings.vue` | The error wasn't cleared on close, so reopening showed the old one. The name and checkbox are read from the store each time the modal opens, instead of at page load and on close. Change Settings is disabled for a name of only spaces and while the request runs | Yes | Done |
| 14 | `components/modals/conference/invitePlayers.vue` | The error was never cleared, so it stayed after a later success and after closing. It's cleared on each request and on close, and shows in a `b-alert`. The buttons are disabled while a request runs | Yes | Done |
| 14 | `components/modals/conference/manageActivePlayers.vue` | Errors use `getApiErrorMessage` and show at the top. Set Active Players is disabled while the request runs. Each checkbox is labelled with its player. The rows are a list instead of an object keyed by user ID. No visible change | — | Done |
| 14 | `components/modals/conference/promoteNewConferenceManager.vue` | Clicking Transfer Conference Manager with no one chosen threw a `TypeError`, and the error stayed after closing. The button is disabled until a player is chosen and while the request runs, and closing clears the error | Yes | Done |
| 14 | `components/modals/conference/reassignLeagueManager.vue` | `clearData` was never hooked up, so the chosen league, manager and error stayed after closing. Clicking Transfer League Manager without both threw a `TypeError`. Closing now clears them, the button is disabled until both are chosen and while the request runs, and the labels point at their selects | Yes | Done |
| 14 | `components/modals/conference/renewLeagueForNewYear.vue` | Fetched the conference when the page loaded as well as on open; now only on open. A failed load wasn't caught. The chosen league wasn't cleared on close, and Renew League with none chosen threw a `TypeError`. Errors show in a `b-alert`, and the button is disabled until a league is chosen and while the request runs | Yes | Done |
| 14 | `views/conference.vue` | Dismissing the manager's message didn't catch a failure; it now shows in the page's `b-alert`, and the message stays. Joining and Enable Royale show the server's message through `getApiErrorMessage` | Yes | Done |
| 14 | `views/conferenceHistory.vue` | A failed delete was swallowed, and a failed load showed a blank page. Errors now show in a `b-alert`, and a failed load shows the conference page's error. Delete is disabled while a request runs | Yes | Done |
| 15 | `components/recentJobsTable.vue` | Details did nothing on this branch. bootstrap-vue-next 1.x renamed row details to row expansion, so the `#row-details` slot, `detailsShowing` and `_showDetails` were ignored, and the `b-table-has-details` classes the styles targeted don't exist. It uses `#row-expansion` now, tracked by `jobID`, so open jobs stay open across a refresh as before. The shared background and dashed divider are back | No | Done |
| 15 | `components/recentJobsTable.vue` | Choosing a job type or status in the filter boxes didn't refresh the table. vue-multiselect 3 has no `input` event, so `@input` caught the search box's native input events instead: typing in the search refreshed, choosing didn't. It listens to `update:model-value` now | No | Done |
| 15 | `components/recentJobsTable.vue` | A failed cancel's message was cleared by the refresh that follows it, so it never showed. It's shown after the refresh now | Yes | Done |
| 15 | `components/recentJobsTable.vue` | The row filter icons are native buttons named "Show only X jobs" and "Hide X jobs". `:disabled` on the icon did nothing; on the button it works. Where there's a mouse they're transparent until hovered, where they were `display: none`, so the keyboard can reach them, and they show when focused | — | Done |
| 15 | `adminConsole`, `recentJobsTable`, `serviceMonitor`, `pendingMasterGameUpdates` | Errors use `getApiErrorMessage`: a 5xx shows "Something went wrong." (or the caller's fallback) instead of the error page's text, and a network failure is rethrown instead of shown. The console's two failure alerts are one: "'X' failed: \<message\>" | — | Done |
| 15 | `views/adminConsole.vue` | Merge Games and Send Confirmation sent an empty ID as `null` and got a validation 400; they're disabled until the IDs are filled, and the labels point at their inputs. A failed load of the automated action processing setting wasn't caught; it shows in the banner. A failed change of that setting no longer re-reads it, which replaced the error; the switch goes back by itself | Yes | Done |
| 15 | `views/adminSupportTickets.vue` | A failed first load wasn't caught. Failures showed the error object and the axios response object instead of the server's message; there's one alert with the message now. "Opened" is a formatted date instead of the raw instant | Yes | Done |
| 15 | `views/adminSiteAnnouncements.vue` | Editing an announcement and clearing "Posted at" threw a `RangeError` from `new Date('').toISOString()`, shown as the error. Save changes is disabled until an edited announcement has a time, and the hint says so. Failures showed error objects instead of the server's message | Yes | Done |
| 15 | `views/activeMasterGameRequests.vue`, `activeMasterGameChangeRequests.vue` | A failed load was swallowed, and a failed response set `errorInfo`, which nothing declared or displayed. Both show in a `b-alert`. After a response the answered request stayed in the list and the form stayed open; the list now reloads, the form closes, and the success message names the game. Submit is disabled for a blank note, which the server requires, and while the request runs. The form's heading names the game | Yes | Done |
| 15 | `views/activeMasterGameChangeRequests.vue` | Link OpenCritic and Link GG\| showed on every request. With no ID the request had no `openCriticID`, which the server reads as 0, and no GG token. They show only when the request has one now, and a failed link shows its error instead of nothing. The unused `editGame` method is gone; the links use the shared helpers | Yes | Done |
| 15 | `views/masterGameCreator.vue` | Typing an OpenCritic ID and clearing it sent `""`, which the server can't read as a number; it's a number input now and an empty one isn't sent, nor an empty GG token. A failed create showed "An error has occurred with your request."; it shows the server's message. The estimated date was pasted into the parse request's query unencoded; the client encodes it. Submit is disabled while the request runs. Edit Master Game had `class="warning"` instead of `variant` | Yes | Done |
| 15 | `views/masterGameEditor.vue` | Saving sent back the critic score shown on the page. For a game with sub-games and no score of its own, that's the sub-games' average, so saving stored the average as the game's own score and it stopped following the sub-games. No critic score is sent now; the server keeps the game's own | Yes | Done |
| 15 | `views/masterGameEditor.vue` | Box Art File Name could be edited, but `EditMasterGameRequest` has no such field and the server keeps the old one. It's read-only now | Yes | Done |
| 15 | `views/masterGameEditor.vue` | The warning "Are you sure you want to clear the critic score while leaving OpenCritic Score Refreshing on?" showed when Sync with OpenCritic was off. It shows when it's on now, since the score refresh only runs for games with sync on, so a cleared score comes back | Yes | Done |
| 15 | `views/masterGameEditor.vue` | Clearing the minimum release date sent `null`, which the server can't read, so the save failed with a generic error. It's a required field now, with a message. Failures show the server's message instead of the axios response data. Changing the route reloads the change request and tags as well as the game. OpenCritic ID is a number input | Yes | Done |
| 15 | `views/actionProcessingDryRunResults.vue` | A failed dry run was swallowed and the spinner stayed up forever; it shows a `b-alert` now. "No actioned games." never showed, since it tested `.length` on an object; each game table says when it's empty instead | Yes | Done |
| 16 | `views/criticsRoyale.vue`, `royaleGroupQuarter.vue`, `royaleHistory.vue` | `.ranking-column` was global in all three, at 50px in the first two and 80px in Royale History, so the width depended on which page's styles had loaded. Seen on a royale group's quarter page, whose Rank column was 80px. Each page's own width applies now | Yes | Done |
| 16 | `components/masterGameTagBadge.vue`, `scss/custom-bootstrap.scss` | `.popover-header { color: black }` was global from the badge, and every popover in the app relied on it: without it, popover titles are white on the light grey header. It's `$popover-header-color` in the theme now. No visible change | — | Done |
| 16 | `components/gameTables/minimalPlayerGameTable.vue` | The totals row used `id="total-description"`, `total-description-text` and `total-column`, repeated in every publisher's table on the League page. They're classes now. The unused `.type-column` is gone, and `.score-column` is also in `minimalPlayerGameSlotRow`, whose cells it styles. No visible change | Yes | Done |
| 16 | `minimalPlayerGameTable.vue`, `views/publisher.vue` | `.share-button { color: white }` did nothing: the icon is in a `btn-secondary`, which is already white. The rule and the class are gone. The Publisher page's button only had the rule when the League page's styles had loaded | — | Done |
| 16 | `views/masterGameCreator.vue`, `views/masterGameRequest.vue`, `components/modals/conference/assignPlayers.vue` | The `vue-slider` label rules (no `vue-slider` dependency any more) and `display: bcollapse` (not valid CSS) are gone. No visible change | — | Done |
