# Vue 3 Upgrade Plan

> Upgrading `FantasyCritic.ClientAppVue2` from Vue 2.7 to Vue 3 in place, with no parallel
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
2026-11-01. The first production release of this work is pending.

| Area | Change |
|---|---|
| Template syntax | The last `slot="..."` is now `v-slot`. |
| Lint ratchet | All 18 `vue/no-deprecated-*` rules the codebase already passes are on as errors, in their own block of `eslint.config.mjs`. |
| Removed | `register-service-worker` (unused), `vuex-router-sync` (nothing read `state.route`), `vue-gtag` (it reported to a Universal Analytics property, dead since July 2023), and the `bootstrap.bundle.js` import (bootstrap-vue does that work; dropping it took jQuery out of the bundle, about 180 KB of JS). |
| Replaced with our own code | `vue-clipboard2` with `navigator.clipboard`, `grapheme-breaker-mjs` with `Intl.Segmenter`, `vue-awesome-countdown` with `components/countdown.vue`, and `vue-js-toggle-button` with `components/toggleSwitch.vue`. |
| Housekeeping | `npm audit fix` (axios 1.21; only low-severity advisories against Vue 2 itself remain), `chart.js` as a direct dependency, the unused vee-validate `password` rule deleted. |
| Fixed in passing | `transferManagerModal.vue` threw for logged-out viewers of public leagues. |
| Charts | `vue-chartjs` 5, Chart.js 4 and `chartjs-plugin-annotation` 3, on the `data`/`options` props. The chart options and data are `markRaw`, which stops Vue 2's reactivity recursing into Chart.js objects (the `Maximum call stack size exceeded` the game page's stats chart threw). `royalePublisherGraph.vue` reads the chart from the component's `chart` ref. Checked on a production build against live data: both charts draw, with no console errors. **Check charts with `vite build` and `vite preview`, not the dev server**, which leaves them blank. |
| Drag and drop | `vuedraggable` 2 to `vue-draggable-plus` 0.6 (`VueDraggable`, `v-model`) in `assignPlayers`, `currentBidsForm`, `editDraftOrderForm`, `gameQueueForm` and `leagueTagSelector`. The draft order list lost its `transition-group` (it has Sortable's `animation` instead), and the tag selector's list headers moved out of the draggable. Builds and lints clean, and the component renders and attaches Sortable on Vue 2.7; **drag-and-drop itself is untested, since it needs a logged-in session.** |
| State | Vuex 3 to **Pinia 2.3** (`~2.3.1`; Pinia 3 dropped Vue 2), led by Steve. A 1:1 port of all six stores at once rather than store by store: same stores, state and action names. Mutations became actions or direct assignments, and getters that only returned state were dropped, since Pinia exposes state directly. The mixins stay, on Pinia's `mapState`. The stores are now `.ts` files (`auth.ts` became `authStore.ts`) but have no types yet. `store/pinia.js` holds the one Pinia instance, which the router guard passes to each `useXStore(pinia)`, because the router's first navigation runs before Pinia is active. `adminStore` and `authStore` use the NSwag client; the other four are still on axios. Lints and builds clean. |
| Filters | The 11 `Vue.filter`s in `filters.js` became plain functions in `utilities/formatters.ts` (`formatScore`, `formatCurrency`, `formatDateTime`, `formatDateTimeAt`, `formatDateIso`, `formatDateLong`, `formatYesNo`, `formatApprovedRejected`, `formatPercent`, `formatThousands`, `selectTextFromPossibleOptions`), with the behaviour copied unchanged. All 139 uses in 49 files are now `{{ formatX(value, args) }}`, and each component imports what it uses and lists it under `methods`, the same way it already does for `globalFunctions`. `filters.js` is gone and `vue/no-deprecated-filter` is on. `globalFunctions.ts` still has `roundNumber`, `formatMoney` and `formatLongDate`, which differ from `formatScore`, `formatCurrency` and `formatDateLong` only in how they treat empty and non-numeric input; folding them together is a possible tidy-up. Lints and builds clean, and the games table on a production API renders scores and percentages correctly. |

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

---

## Before the flip (on Vue 2.7)

Everything here can ship on Vue 2.7, and almost all of it is independent. A rough order:
small dependency swaps first, then the structural work, which is where TypeScript and the NSwag
client come in. Counts are as of 2026-10-28.

### Dependencies

| Package | Where | Move |
|---|---|---|
| `vee-validate` 3 | 22 `ValidationProvider` in 13 files, 7 `ValidationObserver` | **Decision needed.** The rules in use are `required`, `integer`, `min_value`, `max_value`, `email` and `min` (`max` is registered but unused), and the observers only read `invalid`. Replacing it with our own code now would also remove a compat risk; the alternative is vee-validate 4 after the flip, whose `<Field>`/`<Form>` have nearly the same slot shape. |

### Code

- **axios to the generated NSwag client.** 100 files import axios; 10 use `@/api/clients`. Not a
  flip blocker, since axios works on Vue 3, but the target is the typed client everywhere. Do it
  store by store (`conferenceStore`, `interLeagueStore`, `leagueStore` and `publisherStore` are
  still on axios), and component by component as files are touched. The admin console and job
  pages are already on it and are the pattern to follow.
- **Mixins to composables or plain functions.** 86 components use a mixin: `LeagueMixin` 61,
  `ConferenceMixin` 13, `GGMixin` 5, `PublisherMixin` 4, `BasicMixin` 4. `BasicMixin` is also
  registered globally with `Vue.mixin`, which is where `userInfo` and `makeToast` come from
  invisibly. Most of `LeagueMixin`'s computed properties are pure functions of `leagueYear`, so
  they can become plain, testable functions. The mixins read the stores through Pinia's
  `mapState`, which works on Vue 3 too, so this isn't a flip blocker.
- **TypeScript.** 4 of 171 components use `lang="ts"`. Seven hand-written models (`models/`)
  duplicate generated types. Adding a `vue-tsc` type check to CI (it supports a Vue 2.7 target)
  gives the conversions teeth. Convert as files are touched.
- **Tests.** There are none. Set up Vitest with `@vue/test-utils` v1 now and move to v2 at the
  flip. Start with the logic pulled out of `LeagueMixin`, for example `counterPickBiddingAllowed`.
- **Bootstrap classes that work in both versions.** `form-group` becomes `mb-3`: Bootstrap 4's
  `form-group` is exactly a 1rem bottom margin, `mb-3` is the same in both versions, and no CSS
  selects `.form-group`. That's 172 uses in 63 files. The other Bootstrap 5 renames (`ml-` to
  `ms-`, `badge-*` to `bg-*`, `text-left` to `text-start`, `sr-only`) don't exist in Bootstrap 4
  and wait for the UI swap. `form-row`/`form-inline` (4 uses) need a look.
- **Datepickers.** 14 `b-form-datepicker`s in 4 files, mostly the master game creator and
  editor. bootstrap-vue-next has no datepicker. **Decision needed:** `b-form-input type="date"`
  exists in both libraries and could replace them now, at the cost of the browser's native picker
  instead of bootstrap-vue's calendar. flatpickr, already a dependency, is the other option.
- **Small items.**
  - `leagueActions.vue` mounts about 30 manager-only modals for every viewer, while publisher
    modals are already behind `v-if="userPublisher"`. Gating them behind `league.isManager`
    needs a check of which ones non-managers use (the active trades modal, for one).
  - `about.vue` still says the site uses Google Analytics.
  - The vee-validate `max` rule is registered but unused.
  - `jsconfig.json` sits alongside `tsconfig.json`.

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
- `vue-flatpickr-component` 8 to 12 (2 uses), or a native `datetime-local` input
- `@vue/test-utils` v1 to v2
- bootstrap-vue **stays**, running on its compat support

Code that changes with it:

- `.sync` (48) becomes `v-model:prop`, and `.native` (15) goes.
- Components with a custom `v-model` (9 `$emit('input')`) move to `modelValue` and
  `update:modelValue`.
- `beforeDestroy` in `countdown.vue` becomes `beforeUnmount`.
- `main.js` moves to `createApp`, and the `Vue.use`, `Vue.component` and `Vue.mixin` calls
  move onto the app.
- Transition class names change without a runtime warning; search for them.

Ship to production on compat once the app runs cleanly, then drive the warnings to zero.

## The UI swap

- bootstrap-vue 2 to bootstrap-vue-next 1.x, and Bootstrap 4 to 5, in one step. popper.js and
  jQuery (still in `node_modules` as Bootstrap 4's peer dependency) leave with them.
- The surface is mostly trivial in volume: 258 `b-button`, 54 `b-form-select`. The real work is
  in the modals (74 `b-modal`, 76 `v-b-modal`, 58 modal components, 62 `$refs`), the 47
  `b-table`s, the 53 `v-b-popover`s, and `$bvModal` (12), `$bvToast` (4) and `makeToast` (14),
  which become bootstrap-vue-next's composables.
- **Choose one modal pattern before touching any of them.** Modals are where 2023 stopped.
- The Bootstrap 5 class renames, and 24 deep selectors to recheck.
- Remove `@vue/compat`.

## After

The consistency pass: current Pinia, whatever axios is left, the remaining mixins, and
TypeScript across the rest of the app.

---

## Open decisions

- **vee-validate:** our own validation now, or vee-validate 4 after the flip.
- **Datepickers:** native `type="date"`, or flatpickr.
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
