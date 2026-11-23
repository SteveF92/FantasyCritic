# Client refresh (second pass)

The TypeScript conversion ([typescript-conversion-plan.md](typescript-conversion-plan.md)) finished the move
to Vue 3, bootstrap-vue-next and Bootstrap 5. This pass leaves the transition period behind: make the best
codebase we can with the packages we have now, instead of one that imitates Vue 2, Bootstrap 4 and
BootstrapVue 2.

This isn't a redesign, and the user experience shouldn't really change. Think of it as a mid-model refresh:
where Bootstrap 5's defaults are no worse than what we have, take them, and accept the light visual
update that comes with them.

The work stays on `vue3-migration`. The plan is written a step at a time: each step's details are
filled in when we reach it, then built, tested, committed and reviewed before the next.

## Goals

1. Remove packages and tooling we don't need any more.
2. Simplify the markup: remove elements that do nothing.
3. Make the markup semantic: the right element in the right place.
4. Move the page column (`<div class="col-md-10 offset-md-1 col-sm-12">` and its variants) up into
   `App.vue`.
5. Make the league tag selector on the create pages work on phones.
6. Build repeated code into components, composables or helpers.
7. Simplify the CSS and SCSS. Take Bootstrap 5's defaults where they aren't worse, instead of keeping
   Bootstrap 4's.
8. Look into file-based routing.
9. Catch what the first pass missed, especially patterns settled late in it that earlier pages don't
   follow.
10. More unit, component and UI tests.

### Out of scope

- **Breaking API changes.** They stay in [api-breaking-changes-plan.md](api-breaking-changes-plan.md),
  together with removing the obsolete `PublisherGameViewModel.Linked`.
- **A redesign.** New layouts, new colours or new flows.

## Decisions so far

- **Composables are allowed now** for helpers that hold state (a request's busy and error state, ignoring
  stale responses). Components stay on the Options API and call them from `setup()`. Pure functions still
  go in `utilities/`.
- **One page column.** The `col-md-10 offset-md-1` and `col-lg-10 offset-lg-1` versions are probably
  merged into one; each page is checked when it's reached.
- **Bootstrap 4 values** (gutters, container widths, font stack, square buttons) are decided one group
  at a time in step 10, not up front.

## Where we start (2026-11-22)

- 43 of 44 views open with a column wrapper: `col-md-10 offset-md-1 col-sm-12` (41), `col-lg-10
  offset-lg-1` (about 16, some with `text-well`), and one-offs on Welcome and nested `col-10 offset-1`
  blocks. Admin Console has none.
- `scss/bootstrap-overrides.scss` is about 320 lines, nearly all of it keeping Bootstrap 4 behaviour.
  `scss/custom-bootstrap.scss` sets about 45 variables to Bootstrap 4's values. The overrides are
  load-bearing: `.form-group` has 197 uses in 70 files, `plain-check` 38, `input-group-btn` 12,
  `form-row` 7, and many templates put columns outside a `.row` or use `.row` as a plain flex wrapper.
- `css/site.css`, `footer.css` and `modal.css` are plain CSS imported globally from `App.vue`.
- 149 raw `<div class="alert">` against 136 `<b-alert>`; 85 components show `errorInfo` the same way.
  105 `<br>`. No `<nav>` or `<header>`, one `<section>`.
- Routes are hand-written in `router/routes.ts` (43 named routes, 524 lines). Typed routes
  (`RouteNamedMap`, from the first pass's step 16) were never done.
- Client tests: 9 Vitest files, one of them a component test. The UI suite covers the main flows.

## Principles

- **Removing a Bootstrap 4 override is not a visual change.** Fix the markup that depends on it, delete
  the rule, and the screenshots don't move. Taking a Bootstrap 5 default *is* a visual change. Keep the
  two in separate commits so they can be reviewed differently.
- **Every visible change is logged** in [Found along the way](#found-along-the-way), as in the first pass.
- **Foundations before the page pass.** Routing, the page layout and the shared components decide what
  every page looks like, so they come first, and each page is reworked once.
- **Tests come with the work:** Vitest tests for each new component, composable or helper; UI tests and
  an accessibility scan for each page family in the page pass.

## Steps

### Phase A: quick wins

#### 1. Packages and tooling

Decided:

- **One Vite config.** `vite.config.js` absorbs `vite.build.config.js`, `vite.client.config.js` and
  `vitest.config.js`. The default mode serves over HTTPS with the dev certificate and proxies to local
  Web; `--mode client` proxies to production on port 5001; the build has no server settings; the Vitest
  settings sit in the same file. The certificate is read only when it's needed, which is why the build
  used to have a file of its own. It stays JavaScript: as TypeScript it would need `@types/node` and a
  second tsconfig to be checked at all.
- **`npm run preview`** serves the last build against production (`--mode client`), as the Vue 3
  upgrade's notes used it.
- **Delete** `jsconfig.json` (`tsconfig.json` has the same path), and the unused fonts: Raleway, imported
  in `custom-bootstrap.scss`, and Material Icons, loaded in `index.html`. Every page fetched them and none
  used them. Roboto stays for now (see step 10).
- **`index.html`:** points at `main.ts`, not `main.js`; drops the Internet Explorer `X-UA-Compatible` tag
  and a stray `data-react-helmet` attribute.
- **ESLint:** drop the block of Vue 3 deprecation rules that `flat/essential` already turns on, the
  unused `__dirname` and `process` globals, and the `--ext` flags the flat config doesn't need.
- **Stale wording:** the `tsconfig.json` comments and redundant options, the `.esproj`'s "Vue 2", and
  CLAUDE.md's Frontend section.

Kept or moved:

- **`vue-multiselect` stays.** bootstrap-vue-next has no searchable multi-select, and building one is
  new UI, not a quick win. Its styling can be looked at in step 9 or 10.
- **vee-validate stays, with `@vee-validate/rules` and `@vee-validate/i18n`.** It's the usual choice in
  Vue, and i18n is its documented source of messages for string rules. What needs fixing is how we use
  it, which goes on step 3's checklist.
- **Stylesheet order** moves to step 9. `bootstrap-vue-next.css` is imported in `main.ts` after `App`, so
  it loads after our overrides and site CSS. Putting it straight after Bootstrap is the clear order,
  but it could change what wins, and there's no screenshot check until step 4.
- **Lazy-loaded routes** move to step 5. `routes.ts` imports all 45 views, so the app is one 2 MB
  script.

`luxon` stays.

#### 2. League tag selector on phones

At phone width the create pages scroll sideways because the two tag columns don't fit (logged in the
first pass, step 10). `leagueTagSelector` is used once, in `leagueYearSettings`, so this covers both
Create League and Edit League. At 375px the page is 550px wide: the lists sit side by side in a flex
row and size to their badges, and the longer badges wrap under the drag icon.

Decided:

- **Stack the lists below `md`:** a `row g-2` with a `col-12 col-md-6` for each list. From `md` up they
  stay side by side, but at equal widths; today each list is as wide as its longest badge.
- **A Ban or Allow button on every tag**, at the end of its row, at every width. It's the way to move a
  tag on a phone, and drag and drop has no keyboard equivalent either. Its accessible name includes the
  tag ("Ban Port"). A banned tag goes on the end of the banned list, as a drop there would; the allowed
  list is always in the store's order, as now.
- **Touch drag waits for a long press** (Sortable's `delay` with `delayOnTouchOnly`). With the lists the
  full width of a phone, a drag that starts on touch would take every swipe over them, and the page
  couldn't scroll. The mouse drags at once, as now.
- **The tag row is a flex row** (icon, badge, button), so a badge no longer wraps under its icon. A long
  badge's text wraps inside it instead, so the button stays in the row.
- **The drop limits table** on the same pages was the other thing wider than a phone: it had
  `table-small`, which isn't a Bootstrap class, so it kept the full cell padding. It's `table-sm` now.

Tests: a Vitest component test for the buttons; a UI test that bans and allows a tag on a phone-sized
Create League page, checks the page doesn't scroll sideways, and creates the league with the change; and a
UI test that drags a tag between the lists with the mouse.

As built:

- `delay-on-touch-only` has to be bound (`:delay-on-touch-only="true"`). vue-draggable-plus passes its
  attributes to Sortable as they are, so the bare attribute arrives as `""`, which is false, and the mouse
  waited for the long press too.
- `Ui.DragAsync` releases just inside the target's top edge. That reorders one list but doesn't move a
  tag into the other, so the drag test has its own helper that rests over the middle of a row first. A
  drag through the browser pane's tool doesn't start a native drag at all, so it can't check this by hand.
- **`CreateLeagueTests` had been failing since 2026-11-15**, when the create page gained its "Select
  Preset" step and lost its "Game Settings" heading. `StartAsync` now presses Select Preset and waits for
  "Select a different preset".

#### 3. List the late conventions

A survey, not a code change. Go through the first pass's Conventions and its log from step 9 on, and list
what later steps did that earlier ones don't. For example: disabling a submit button while its request
runs, clearing errors when a modal opens or closes, rejecting names of only spaces, `type="number"` for
numbers, labels pointing at their inputs, ignoring stale responses. The result is the checklist the page
pass (step 11) applies to each page. Anything that's a straight search-and-fix gets fixed here.

Already on the list, from step 1, for the 23 `ValidationField`s in 13 files:

- **A readable `label`** on each field. Messages use the field's `name`, so bids say "The bidAmount field"
  and draft settings "The gamesToDraft-0 field".
- **One statement of the rules.** Only 5 files have a `ValidationForm`; the other 8 show vee-validate's
  messages but decide whether to submit with their own checks (`leagueYearIsValid`, `canPlaceBid`). Wrap
  them in a `ValidationForm` and disable submit on its `meta.valid`, unless a rule can't be expressed there.
- **Errors tied to their inputs** with `aria-describedby`, as only 2 fields do today.

### Phase B: foundations

#### 4. Visual safety net

The screenshot scripts from the first pass lived in session scratchpads and are gone. Commit a tool that
captures every page at 1600, 800 and 375px and diffs two runs, so "no visual change" can be checked
rather than eyeballed. Add an accessibility scan (axe) to the UI suite to measure goal 3.

#### 5. Routing

Spike file-based routing. vue-router 5 has it built in, and it generates typed route names and params,
which covers the typed routes step 16 skipped. What the spike has to answer:

- Every URL stays exactly as it is (Discord bot links, emails and bookmarks point at them).
- Where the `props: (route) => ...` parsing and the route `meta` (title, sign-in) go: `definePage` or
  the component.
- How layouts per route fit (step 6).
- Lazy loading: every view is imported up front today, so the whole app is one 2 MB script. Either way,
  routes should load their views lazily.

If it's worth it, move the views into the new tree before the page pass, so files move once. If not,
declare `RouteNamedMap` by hand.

#### 6. Page layout in App.vue

The page column moves from each view to `App.vue`, with a default and an opt-out for the pages that
need a different width (Welcome, Admin Console). Use route meta or a layout from step 5. Merging the
`md-10` and `lg-10` versions changes the width between 768 and 991px; log it.

#### 7. Shared components and composables

Survey the duplication first, then build what's worth it, with tests. Likely candidates:

- An error alert (the 85 `errorInfo` alerts), and one convention for raw alerts vs `b-alert`.
- A loading state.
- A request composable: busy, error through `getApiErrorMessage`, and ignoring a response that's no
  longer the latest. That fixes the "Not scheduled" race in the first pass's log.
- The game search that the bid, draft, claim and associate forms each repeat.
- A modal form shell: clear on open and close, disable while busy, show the error.
- The `text-well` section.

Building them here doesn't adopt them everywhere; the page pass does that.

### Phase C: global CSS

#### 8. Remove the Bootstrap 4 overrides

One override at a time: fix the markup that needs it across the app, delete the rule, and check the
screenshots don't move. For example `.form-group` → `mb-3`, `plain-check` → `form-check`, `form-row` →
`row g-2`, `input-group-btn` wrappers removed, columns put inside rows, flex `.row`s replaced with
`d-flex`.

#### 9. Site CSS into SCSS

`site.css`, `footer.css` and `modal.css` become SCSS, using Bootstrap's variables where one exists (for
example `$btn-border-radius: 0` instead of a rule). Hand-written utilities that Bootstrap has
(`.lg-screen-minimum` → `d-none d-lg-block`, `.text-black`) go.

Set the stylesheet order explicitly in one place: Bootstrap, bootstrap-vue-next, our overrides, then the
site's own CSS. Today `bootstrap-vue-next.css` loads last, because `main.ts` imports it after `App`.

#### 10. Bootstrap 5 defaults

The visible part. Return `custom-bootstrap.scss`'s Bootstrap 4 values to Bootstrap 5's defaults a group
at a time: gutters, container widths, font stack and RFS, card and alert padding, border radii, form
controls, table padding, sort icons, badges. Each group gets before and after screenshots, and is kept
or reverted.

The font stack group includes `index.html`'s Google Fonts link to Roboto. Roboto is fourth in the stack,
so the download only shows where Apple's and Segoe UI aren't installed and Roboto isn't either (mostly
Linux desktops).

### Phase D: page by page

#### 11. Page families

In the first pass's order: shared leaves, app shell and static pages, master game pages, request pages,
Home, Critics Royale, league creation and settings, Publisher, league history and stats, the League page
(split as before), Conference, admin pages. On each page:

- Remove elements that do nothing.
- Semantic markup: `<header>`, `<nav>`, `<main>` and `<footer>` in the app shell; a `<section>` with its
  heading; heading levels in order; `<dl>` for label and value lists; `<ul>` for lists of links; buttons
  for anything clickable; margins instead of `<br>`.
- Use the step 7 components.
- Apply step 3's checklist.
- UI tests for the family, and a clean accessibility scan.

## Found along the way

Bugs found, and every deliberate change in behaviour or appearance.

| Step | Where | What | Fix |
|---|---|---|---|
| 2 | `leagueTagSelector.vue` | At phone width the tag lists sat side by side and the create and edit pages scrolled sideways (first pass, step 10) | The lists stack below `md` |
| 2 | `leagueTagSelector.vue` | From `md` up the two lists are now equal widths; each was as wide as its longest badge | Visible change |
| 2 | `leagueTagSelector.vue` | Each tag row has a Ban or Allow button, so rows are a few pixels taller; at phone width the longer badges wrap onto two lines | New control |
| 2 | `leagueTagSelector.vue` | On touch, a drag starts after a long press, so a swipe over the lists scrolls the page | Behaviour change on touch only |
| 2 | `CreateLeagueTests` | Every test failed at its start since the "Select Preset" step (2026-11-15): it waited for a "Game Settings" heading that no longer exists | `StartAsync` presses Select Preset |
| 2 | `leagueYearSettings.vue` | The drop limits table's `table-small` isn't a class, so the table kept full padding and was 341px wide in a 275px column at phone width | `table-sm`: smaller cell padding at every width |
