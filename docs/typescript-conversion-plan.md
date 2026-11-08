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
    gameSelected: (game: MasterGameYearViewModel) => true
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

- **Types come from `@/api/clients`** (the generated view models and requests). `src/models/` is
  hand-written copies of generated types and goes away in step 1.
- **Props:** `as PropType<T>` for objects and arrays. Make a prop `required: true` when every parent
  passes it, rather than optional plus `!` at each use. A route with `props: true` passes strings;
  routes that parse their params pass numbers. Match what the route gives.
- **Computed:** annotate every return type. The Options API needs it to type `this`.
- **Data:** initial `null`s and empty arrays need a cast: `null as T | null`, `[] as T[]`.
- **Emits:** declare every event. The object form with a typed validator gives the parent's handler its
  payload type. Step 1 settles the exact form that passes lint.
- **`$refs`:** cast with `this.$refs.x as InstanceType<typeof Child> | undefined`.
- **Requests:** build the generated request type (a typed object literal), not an untyped object.
- **Errors:** `getApiErrorMessage(error)` from step 2, never `error.response.data`.
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
4. Log every bug found and every deliberate behaviour change in [Found along the way](#found-along-the-way).
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
League steps.

## Steps

### 1. Type-checking setup

- Add `components.d.ts` to `tsconfig.json`'s `include`. It sits at the project root, outside `src/**`,
  so vue-tsc doesn't see it today and bootstrap-vue-next components (`BModal`, `BButton`, ...) and
  `RouterLink` aren't checked in templates.
- Declare the globally registered `FontAwesomeIcon`, `FontAwesomeLayers`, `FontAwesomeLayersText`,
  `ValidationField` and `ValidationForm` in a `GlobalComponents` augmentation.
- Turn on vue-tsc's strict template options (`vueCompilerOptions.strictTemplates`, or the individual
  `checkUnknownComponents`/`checkUnknownProps`/`checkUnknownEvents` flags), so an unknown component or
  prop in a TS template is an error instead of a silent `any`.
- Fix what this surfaces in the four components that are already TS: `royalePublisherGraph`,
  `siteAnnouncementsWidget`, `about`, `siteUpdates`.
- Point those components at the generated types and delete `src/models/`.
- Settle the emits form against the lint config.

### 2. API error helper

- Add `utilities/apiError.ts` with `getApiErrorMessage(error: unknown): string`, plus Vitest tests.
- `ApiException.response` is the raw response text. Check what the server actually sends for each
  failure shape before writing the helper:
  - `BadRequest("message")` under the client's `Accept: application/json` (probably a JSON-quoted
    string)
  - model validation failures (`ValidationProblemDetails`)
  - 401/403
  - 500
- Decide whether the helper handles network failures (a `TypeError` from `fetch`) or leaves them to
  the caller.

### 3. Shared leaves: master game display

`masterGameTagBadge` → `masterGameSummary` → `masterGamePopover`. Used on 25, 16 and 15 pages.
Test on Master Games, League and Home.

### 4. Shared leaves: small widgets

`collapseCard`, `toggleSwitch`, `dateInput`, `statusBadge`, `siteCounts`, `gameTables/slotTypeBadge`,
`masterGameDetails`, `masterGameTagSelector`, `modals/leagueOptionsModal`.

### 5. App shell and static pages

`App.vue`, `topNavBar`, `footerModule`, `toastHost`; `welcome` (+ `homeFAQ`), `discordBot`, `faq`,
`howToPlay`, `contact`, `community`, `fantasyCriticPlus`, `notFound`. Test both signed in and signed
out.

### 6. Master game pages

- `masterGames` (+ `gameTables/masterGamesTable`)
- `masterGame` (+ `masterGameYearStats`)
- `recentMasterGameChanges`, `mostDesiredReviews`, `topBidsAndDrops`, `publicLeagues`
- `advancedGameStats` (+ `gameStats/longestTenuredGamesPanel`, `gameStats/mostDreamsDashedPanel`)

### 7. Request pages

`masterGameRequest`, `masterGameChangeRequest`: create, delete and dismiss requests.

### 8. Home

`home` (+ `leagueTable`, `conferenceTable`, `topBidsAndDropsWidget`, `gameNews`).

### 9. Critics Royale

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

## Found along the way

Bugs found by the conversion, and every deliberate behaviour change. "On main" says whether production
has the bug too.

| Step | Where | What | On main | Fix |
|---|---|---|---|---|
| — | `views/masterGameRequest.vue` `sendMasterGameRequestRequest` | Reads `error.response.data` on an `ApiException`, whose `response` is a string, so `errorInfo` is `undefined` and a failed request shows no error | Yes | Step 7 |
