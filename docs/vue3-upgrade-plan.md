# Vue 3 Upgrade Plan

> Upgrading `FantasyCritic.VueClientApp` from Vue 2.7 to Vue 3 in place, with no parallel
> site. This replaces Phase 8 of the [deployment roadmap](deployment-modernization-roadmap.md),
> which planned a second Vue 3 site on `new.fantasycritic.games`.

Started October 2026.

---

## Where we're going

- Vue 3
- bootstrap-vue-next and Bootstrap 5
- Pinia
- TypeScript
- The generated NSwag client for every API call
- Tests
- A more consistently structured front end

**Not** a rework of the user experience. The target is close to 1:1 with today's site.

## Approach

- **In place, following the official [migration build guide](https://v3-migration.vuejs.org/migration-build.html).**
  The page's version numbers and dates are stale (it mentions Vue 3.1 and says the build could
  stop being published after 2021), but `@vue/compat` still ships with every Vue release.
- **Do as much as possible on Vue 2.7 first.** Vue 2.7 already has the Composition API,
  `<script setup>` and TypeScript support, and Pinia 2, vue-chartjs 5 and vue-draggable-plus all
  run on it. Each change merges and ships on its own, so nothing sits on a branch long enough to
  rot.
- **The flip: Vue 3 with `@vue/compat`, keeping bootstrap-vue and Bootstrap 4.** bootstrap-vue
  2.23 added support for the compat build, so the UI library does not have to change at the same
  time as the framework. The guide says shipping on the compat build is acceptable, so the flip
  can reach production once the app runs cleanly. From then on the console warnings can only go
  down.
- **The UI swap: bootstrap-vue to bootstrap-vue-next and Bootstrap 4 to 5, together.** These
  two cannot be split, because bootstrap-vue needs Bootstrap 4's CSS and bootstrap-vue-next needs
  Bootstrap 5's. `@vue/compat` goes after this.

### Why the 2023 attempt stalled

`origin/vue-3` did the whole mechanical migration in three days (29–31 March 2023): filters,
`.sync`, slots, `.native`, deep selectors, Vuex 4, Router 4. It then stalled in June and July on
bootstrap-vue-next 0.14 and its modals, while production kept being merged into a long-lived
branch. Two lessons:

1. Don't swap the UI library during the compat flip. bootstrap-vue-next is now 1.x, but the
   swap is still its own stage.
2. Don't keep a long-lived branch. Ship each piece of the Vue 2.7 work as it's done, and keep the
   flip branch short.

---

## Done

All on `vue3-upgrade` as of 2026-10-28, except Pinia, which is on `vue3-upgrade-pinia` as of
2026-11-01, and tests, which are on `vue3-upgrade-final-prep` as of 2026-11-01. The first
production release of this work is pending.

| Area | Change |
|---|---|
| Template syntax | The last `slot="..."` is now `v-slot`. |
| Lint ratchet | All 18 `vue/no-deprecated-*` rules the codebase already passes are on as errors, in their own block of `eslint.config.mjs`. |
| Removed | `register-service-worker` (unused), `vuex-router-sync` (nothing read `state.route`), `vue-gtag` (it reported to a Universal Analytics property, dead since July 2023), and the `bootstrap.bundle.js` import (bootstrap-vue does that work; dropping it took jQuery out of the bundle, about 180 KB of JS). |
| Replaced with our own code | `vue-clipboard2` with `navigator.clipboard`, `grapheme-breaker-mjs` with `Intl.Segmenter`, `vue-awesome-countdown` with `components/countdown.vue`, and `vue-js-toggle-button` with `components/toggleSwitch.vue`. |
| Housekeeping | `npm audit fix` (axios 1.21; only low-severity advisories against Vue 2 itself remain), `chart.js` as a direct dependency, the unused vee-validate `password` rule deleted. |
| Fixed in passing | `transferManagerModal.vue` threw for logged-out viewers of public leagues. `tradeSummary.vue` threw for logged-out viewers of trade votes, so league history's votes tables failed to render. The league year API had not sent `settings.oneShotMode` since March 2023, so the client treated every league as not one-shot. Names read but never defined, found once the mixins were gone: dismissing a conference manager's message and deleting a league manager's message from league history called methods that don't exist, so the page didn't refresh; the under review toast always said the league was no longer under review; Enter in the counter pick bid form called a missing `searchGame`; the conference player assignment's "draft started" locks read a misspelt `data` key, so they weren't reactive; and the master game change request page dropped its error when the game failed to load. Dead computed properties reading undefined names went from the counter pick, drop and super drop forms, and the player draft form sends `counterPick: false` rather than an undefined value. |
| Charts | `vue-chartjs` 5, Chart.js 4 and `chartjs-plugin-annotation` 3, on the `data`/`options` props. The chart options and data are `markRaw`, which stops Vue 2's reactivity recursing into Chart.js objects (the `Maximum call stack size exceeded` the game page's stats chart threw). `royalePublisherGraph.vue` reads the chart from the component's `chart` ref. Checked on a production build against live data: both charts draw, with no console errors. **Check charts with `vite build` and `vite preview`, not the dev server**, which leaves them blank. |
| Drag and drop | `vuedraggable` 2 to `vue-draggable-plus` 0.6 (`VueDraggable`, `v-model`) in `assignPlayers`, `currentBidsForm`, `editDraftOrderForm`, `gameQueueForm` and `leagueTagSelector`. The draft order list lost its `transition-group` (it has Sortable's `animation` instead), and the tag selector's list headers moved out of the draggable. Builds and lints clean, and the component renders and attaches Sortable on Vue 2.7; **drag-and-drop itself is untested, since it needs a logged-in session.** |
| State | Vuex 3 to **Pinia 2.3** (`~2.3.1`; Pinia 3 dropped Vue 2), led by Steve. A 1:1 port of all six stores at once rather than store by store: same stores, state and action names. Mutations became actions or direct assignments, and getters that only returned state were dropped, since Pinia exposes state directly. The mixins stay, on Pinia's `mapState`. The stores are now `.ts` files (`auth.ts` became `authStore.ts`) but have no types yet. `store/pinia.js` holds the one Pinia instance, which the router guard passes to each `useXStore(pinia)`, because the router's first navigation runs before Pinia is active. `adminStore` and `authStore` use the NSwag client; the other four are still on axios. Lints and builds clean. |
| Filters | The 11 `Vue.filter`s in `filters.js` became plain functions in `utilities/formatters.ts` (`formatScore`, `formatCurrency`, `formatDateTime`, `formatDateTimeAt`, `formatDateIso`, `formatDateLong`, `formatYesNo`, `formatApprovedRejected`, `formatPercent`, `formatThousands`, `selectTextFromPossibleOptions`), with the behaviour copied unchanged. All 139 uses in 49 files are now `{{ formatX(value, args) }}`, and each component imports what it uses and lists it under `methods`, the same way it already does for `globalFunctions`. `filters.js` is gone and `vue/no-deprecated-filter` is on. `globalFunctions.ts` still has `roundNumber`, `formatMoney` and `formatLongDate`, which differ from `formatScore`, `formatCurrency` and `formatDateLong` only in how they treat empty and non-numeric input; folding them together is a possible tidy-up. Lints and builds clean, and the games table on a production API renders scores and percentages correctly. |
| Datepickers | The 14 `b-form-datepicker`s (master game creator, editor and request, and league year settings) became `components/dateInput.vue`, a native `<input type="date" class="form-control">`. bootstrap-vue-next has no datepicker, and the draft pages already used native date inputs. The value is the same `YYYY-MM-DD` string, so nothing that reads it changed. The wrapper emits `null` instead of `''` when the input is cleared, since a native input can be cleared from the keyboard and `''` isn't a valid `LocalDate`. Each input now has the `id` its label's `for` already named. Checked by mounting the component on the dev server (typing, clearing and setting from the model); **the four pages themselves are unchecked, since they need a logged-in session.** |
| Toasts | Every toast goes through `makeToast(message, variant = 'info')` in `utilities/toast.ts`, a plain function that adds the toast to `toastStore`. `components/toastHost.vue`, mounted once in `App.vue`, renders the store's toasts as declarative `b-toast`s and removes each when it hides. The old `makeToast` lived in `BasicMixin`, built its body with Vue 2's `$createElement`, and toasted through the calling component's `$bvToast`, which destroys the toast along with the component. The two direct `$bvToast` calls (critics royale's error, the royale group invite link) now use the shared function, so they have its close icon too. Checked on the dev server: toasts render in both variants, the close icon closes only its own toast, auto-hide removes them from the store, and a toast survives a route change. |
| Date-time pickers | The two `flat-pickr`s (site announcements and special auctions) are native `<input type="datetime-local">`s, and `vue-flatpickr-component` and `flatpickr` are gone. The value is a local `yyyy-MM-ddTHH:mm` string, which `new Date(...)` already read as local time, so the `toISOString()` calls are unchanged. Editing an announcement formats its stored instant with Luxon instead of passing the picker a `Date`. Checked by mounting the announcements page on the dev server: a stored UTC time shows in local time and converts back to the same instant. The special auctions modal is unchecked, since it needs a league manager. |
| Mixins | All five are gone, along with `mixins/` and the global `Vue.mixin`. `GGMixin` became typed functions in `utilities/ggLinks.ts`. The conference, publisher and league mixins' derived values became getters on their stores, `notifyAction` became a store action that refreshes and then toasts, and `decimalsToShow` is an `authStore` getter; the draft modals that toasted without refreshing call `makeToast`. Each component maps only the state and actions it reads, found by compiling its template and collecting its `this.` uses and `watch` keys. `BasicMixin` was global, so the 29 components that relied on it, `topNavBar.vue` among them with no script of its own, now map the `authStore` and `interLeagueStore` state they read and import `formatLongDate`/`formatLongDateTime`. The league getters were compared against the old mixin code on 120 public leagues and 297 synthetic draft and bidding states, with no differences beyond `undefined` becoming `null` or `false`. With no mixins left, every name a component reads must be one it defines, and a static check over all components confirms that for everything the mixins provided. Public pages and the league page's 50 modals show no Vue warnings logged out. |
| Tests | Vitest 5 with `@vue/test-utils` v1 and jsdom, set up in [client-tests-plan.md](client-tests-plan.md). Tests live in `tests/`, mirroring `src/`; `npm test` runs them, and CI runs them after generating the API clients, so deploys gate on them too. They need the generated TypeScript client because the stores import it. 33 so far: `counterPickBiddingAllowed`, `hasPendingOrActiveDraft` and the `pendingDraft*` getters on `leagueStore`, with the clock fixed, and `dateInput.vue`, which proves components compile and mount. Node moved to 26, the current LTS, locally, in CI and in the web image. test-utils v1 adds five moderate dev-only `npm audit` findings, which go when it moves to v2 at the flip. |

**Still to verify by hand:**

- Drag-and-drop in all five places above, including dragging between lists in the conference
  player assignment and the tag selector, and the draggable table rows in bids and the watchlist.
- A Copy button in a real browser. The built-in browser pane refuses clipboard writes.
- The admin console's two toggles, on beta only, since flipping them really turns action
  processing on or off.
- Every page that reads a store, since the Pinia port changed them all at once. The router guard
  logged in and logged out; the league page's toggles and bid reveal, league history; your own
  publisher page's move mode and toggles; a conference page and modal; the home page's admin task
  count; royale pages; the admin console's bid times.
- The date inputs on the master game creator, editor and request pages and in league year
  settings: an existing date loads, a new one saves, and Clear still works.
- Creating a special auction, and creating and editing a site announcement, with the new
  date-time inputs.
- After the mixins: a manager action and a player action on a league page refresh it and toast,
  the draft modals toast during a draft, the nav bar logged in, and the admin console's sections
  for each role.

---

## Before the flip (on Vue 2.7)

Everything here can ship on Vue 2.7, and almost all of it is independent. A rough order:
small dependency swaps first, then the structural work, which is where TypeScript and the NSwag
client come in. Counts are as of 2026-10-28.

### Dependencies

| Package | Where | Move |
|---|---|---|
| `vee-validate` 3 | 22 `ValidationProvider` in 13 files, 7 `ValidationObserver` | **Done at the flip** (2026-11-03): vee-validate 4 as a 1:1 swap. See [vee-validate-4-plan.md](vee-validate-4-plan.md). |

### Code

- **axios to the generated NSwag client.** 100 files import axios; 10 use `@/api/clients`. Not a
  flip blocker, since axios works on Vue 3, but the target is the typed client everywhere. Do it
  store by store (`conferenceStore`, `interLeagueStore`, `leagueStore` and `publisherStore` are
  still on axios), and component by component as files are touched. The admin console and job
  pages are already on it and are the pattern to follow.
- **TypeScript.** 4 of 171 components use `lang="ts"`. Seven hand-written models (`models/`)
  duplicate generated types. Adding a `vue-tsc` type check to CI (it supports a Vue 2.7 target)
  gives the conversions teeth. Convert as files are touched.
- **Bootstrap classes that work in both versions.** `form-group` becomes `mb-3`: Bootstrap 4's
  `form-group` is exactly a 1rem bottom margin, `mb-3` is the same in both versions, and no CSS
  selects `.form-group`. That's 172 uses in 63 files. The other Bootstrap 5 renames (`ml-` to
  `ms-`, `badge-*` to `bg-*`, `text-left` to `text-start`, `sr-only`) don't exist in Bootstrap 4
  and wait for the UI swap. `form-row`/`form-inline` (4 uses) need a look.
- **Small items.**
  - `leagueActions.vue` mounts about 30 manager-only modals for every viewer, while publisher
    modals are already behind `v-if="userPublisher"`. Gating them behind `league.isManager`
    needs a check of which ones non-managers use (the active trades modal, for one).
  - `about.vue` still says the site uses Google Analytics.
  - `jsconfig.json` sits alongside `tsconfig.json`.
  - About 20 names are assigned with `this.x = ...` without being declared in `data`, so they
    aren't reactive. The static check from the mixin work lists them.

### The lint ratchet

As each Vue 3 deprecation reaches zero, turn its `vue/no-deprecated-*` rule on. What's still
non-zero:

| Rule | Count | When it can reach zero |
|---|---|---|
| `no-deprecated-v-bind-sync` | 48 in 19 files | At the flip (needs `v-model:prop`) |
| `no-deprecated-v-on-native-modifier` | 15 in 5 files | At the flip |
| `no-deprecated-destroyed-lifecycle` | 1 (`countdown.vue`) | At the flip (`beforeUnmount`) |

---

## The flip (Vue 3 + compat)

Follow the migration build guide. Packages that change together:

- `vue` 3 with `@vue/compat`, and `@vitejs/plugin-vue` in place of `@vitejs/plugin-vue2`
- eslint-plugin-vue's `flat/vue2-essential` preset becomes `flat/essential`
- `vue-router` 3 to 4 (Router 5 requires Vite 7 or later)
- `@fortawesome/vue-fontawesome` 2 to 3
- `vue-multiselect` 2 to 3 (2 components)
- `@vue/test-utils` v1 to v2
- bootstrap-vue **stays**, running on its compat support
- `vee-validate` 3 to 4, as a 1:1 swap. The components are registered as `ValidationField` and
  `ValidationForm`. See [vee-validate-4-plan.md](vee-validate-4-plan.md).

Code that changes with it:

- `.sync` (48) becomes `v-model:prop`, and `.native` (15) goes.
- Components with a custom `v-model` (9 `$emit('input')`) move to `modelValue` and
  `update:modelValue`.
- `beforeDestroy` in `countdown.vue` becomes `beforeUnmount`.
- `main.js` moves to `createApp`, and the `Vue.use` and `Vue.component` calls move onto the app.
- Transition class names change without a runtime warning; search for them.

Ship to production on compat once the app runs cleanly, then drive the warnings to zero.

## The UI swap

- bootstrap-vue 2 to bootstrap-vue-next 1.x, and Bootstrap 4 to 5, in one step. popper.js and
  jQuery (still in `node_modules` as Bootstrap 4's peer dependency) leave with them.
- The surface is mostly trivial in volume: 258 `b-button`, 54 `b-form-select`. The real work is
  in the modals (74 `b-modal`, 76 `v-b-modal`, 58 modal components, 62 `$refs`), the 47
  `b-table`s, the 53 `v-b-popover`s, and `$bvModal` (12), which becomes bootstrap-vue-next's
  composable. Toasts are all in `toastHost.vue`, so they change in one file.
- **Choose one modal pattern before touching any of them.** Modals are where 2023 stopped.
- The Bootstrap 5 class renames, and 24 deep selectors to recheck.
- Remove `@vue/compat`.

## After

The consistency pass: current Pinia, whatever axios is left, and TypeScript across the rest of
the app.

---

## Open decisions

- **Analytics:** a GA4 property, or none. Either way `about.vue` needs rewording.
- **Style for new components:** Options API or `<script setup lang="ts">`. `countdown.vue` and
  `toggleSwitch.vue` use the Options API in plain JS, to match the codebase.
- **Toggle colour:** `toggleSwitch.vue` is grey both on and off, as the old toggles always were
  (an orange "on" rule in `site.css` matched a class nothing applied). Orange for "on" is one CSS
  rule if wanted.

## Notes

- To check something against production data without the back end, `npm run client` serves the
  SPA on port 5001 with the API proxied to fantasycritic.games. `vite preview` with the same
  config serves a production build through the same proxy.
- To inspect Pinia stores on Vue 2.7, use the legacy Vue.js devtools extension (v6). The current
  Vue DevTools (v7) supports only Vue 3.
- The built-in browser pane refuses clipboard writes, and when it's hidden it stops animation
  frames, so CSS transitions and Chart.js drawing stall there. Measure with the DOM, or check in
  a real browser.
