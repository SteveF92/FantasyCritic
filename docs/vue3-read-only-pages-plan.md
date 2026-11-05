# Vue 3: read-only pages pass

The home page pass (7c5d16f9f, 04091b6a1, 1cf4faf08) made the logged-in home page match main.
This pass does the same for the read-only pages, overnight, without Steve.

## Pages

1. Top Bids and Drops (`/topBidsAndDrops`)
2. Public Leagues (`/publicLeagues`)
3. Master Games List (`/games`)
4. Advanced Game Stats (`/advancedGameStats`)
5. Most Desired Reviews (`/mostDesiredReviews`)
6. Game Changes (`/gameChanges`)
7. Master Game detail (`/mastergame/:id`)
8. A second, logged-in look at the static pages already checked logged out: Site Updates, How
   to Play, FAQ, About, Contact, Community, Discord Bot, Fantasy Critic Plus.

Nothing past these tonight.

## Setup

- Main is the baseline: a detached worktree at `.worktrees/main-compare`, Vite on 44478. The
  vue3-migration side is Steve's Vite on 44477. Both use Steve's backend on 4554 and the
  `fantasycritic-fromsnapshot` database, signed in as Steve through `/dev/sign-in-as`.
- The browser pane may be hidden overnight, which stalls transitions and animations. So the
  comparison runs in headless Chrome driven over CDP from a scratchpad script: sign in through
  the backend, load the same URL on both ports, wait for the network to settle, then take
  full-page screenshots and a layout dump. The layout dump records every element that has its
  own text, with its position and size, so the two sides can be matched by text and
  compared.
- Widths: 1280 first, then 375 for the mobile layout.

## For each page

1. Diff the layout dumps and screenshots. List every element that moved or changed size, and
   compare computed styles where the look differs.
2. Read the vue3 console for errors and compat warnings.
3. Use every control that only reads: tabs, sorting, paging, year and filter selects, toggles,
   popovers, links. Compare what each one does on both sides.
4. Fix, then re-run the diff until the page matches.
5. Run Prettier and ESLint on the touched files, and the Vitest suite if a component with tests
   changed.
6. Commit the page alone. The message notes any change that applies site-wide.

## Rules

- **Read only.** Don't submit forms or take any action that writes to the database.
- **What Claude changes:** SCSS, CSS and template markup to make a page look like main,
  including bootstrap-vue-next prop renames that only affect appearance (`custom-class` to
  `class`, `align`, `placement`, `teleport-to`).
- **What Claude notes for Steve instead:** anything that changes how the JavaScript works:
  event handlers, filter or sort logic, data flow, `@change` to `@update:model-value`. When in
  doubt, it counts as JS. Notes read like a migration doc entry: what changed in the library, the
  symptom, and where. No tailored fix until Steve asks.
- If a page is blocked, note it and move on to another part of the page or the next page.
- Fix page-level problems in the page or its components. A Bootstrap 5 default that differs from
  Bootstrap 4 is fixed once in `custom-bootstrap.scss` or `bootstrap-overrides.scss`, as on the
  home page.
- Where main itself looks broken (like the Bootstrap-Vue 2 popover drawn off screen), keep the
  vue3 behaviour that works and note it for Steve rather than copying the bug.
- Anything that needs Steve's call goes in the report, not into code.
- Charts: the dev server blanks charts (a RangeError that also happens in production), so check
  any chart with a production build.
- Vite's watcher on `I:` sometimes misses an edit. If a change doesn't show, touch the file.
- No pushes, merges or dependency changes.

## Already noted for Steve

Fixed by Steve on 2026-11-05:

- Top Bids and Drops: the end-of-year season alert was a `<b-alert show>`. In bootstrap-vue-next,
  `show` only animates the alert in on mount; visibility comes from `modelValue`. Now
  `:model-value="true"` (5840abbd2), which shows it from the start with no fade, as Bootstrap-Vue 2
  did.
- Public Leagues: the page count stayed at the unfiltered total after the year changed while a
  search was active. bootstrap-vue-next emits `filtered` only when the filter or sort changes, not
  when `items` change. Changing the year now clears the search (eb565d9bf).
- Master Game detail: the Peak Hype Factor info popover (`masterGameYearStats.vue`) showed
  JavaScript source as its text. The `v-b-popover` value was an object with `title` and `content`
  functions and `html: true`; Bootstrap-Vue 2 called the functions, bootstrap-vue-next renders the
  values as plain text, has no `html` option, and warns that `content` is deprecated. Now plain
  `title` and `body` strings (f22652e21).
- Master Games: `masterGameTagBadge` passed `triggers="hover"`, which bootstrap-vue-next ignores.
  Removed (cf9440f0b); hover and focus are the default.
- Master Games: rows sorted out of order by Hype Factor (Forza Horizon 6, 91.09, above Marvel's
  Wolverine, 91.56). bootstrap-vue-next's default sort compares values as strings with
  `localeCompare(..., { numeric: true })`, which reads the digits after a decimal point as a whole
  number. The decimal columns now have `sortCompare: numericComparer` (bc4a7180c). Any other
  `b-table` column of decimals sorted without a `sortCompare` has the same problem.

Kept as is:

- Master Games and Public Leagues: sorting goes back to page 1, because bootstrap-vue-next emits
  `filtered` on sort changes too. Steve is keeping this.

Still open:

- Advanced Game Stats: every section logs `[Vue Router warn]: Couldn't find element using
  selector "#mostDreamsDashed" returned by scrollBehavior`. Vue Router 4 warns when
  `scrollBehavior` returns a hash selector that matches no element; Vue Router 3 ignored it.
- Master Game detail: the statistics chart wasn't checked. The dev server blanks it on both
  branches, and a production-build comparison wasn't set up tonight.
- Not client problems, seen on both branches against `fantasycritic-fromsnapshot`:
  `GET /api/game/GetMostDesiredReviews` and `GET /api/game/GetMostDreamsDashedGames` (All Time)
  return 500, so Most Desired Reviews and the all-time Most Dreams Dashed table couldn't be
  compared.

## Report (2026-11-04)

Every page was compared with main in headless Chrome at 1280 and 375, by screenshot and by the
position, size and style of every element with text. "Matches" means no differences except
antialiasing: some tables draw text with ClearType on one side and greyscale on the other. That
is a compositing difference, not CSS, and shows up at 375.

| Page | Result | Commit |
|---|---|---|
| Top Bids and Drops (all three tabs, month menu open) | Matches | 4beaf12ad |
| Public Leagues | Matches | 828a3df0c |
| Master Games (with game and tag popovers open) | Matches; sort order fixed by Steve | d6e2b7f1d, bc4a7180c |
| Advanced Game Stats (all three sections, year select, checkbox) | Matches | 5fdfc497c |
| Game Changes (with the "New games" filter) | Matches | 5fdfc497c |
| Most Desired Reviews | Filter buttons match; the table couldn't load (500) | 5fdfc497c |
| Master Game detail (released and unreleased game, year menu, tag and help popovers) | Matches; Peak Hype popover fixed by Steve | acfa0d482, f22652e21 |
| Static pages, logged in (Site Updates, How to Play, FAQ, About, Contact, Community, Discord Bot, Plus) | Matches, no changes needed | — |
| Home, rechecked after all of the above | Matches | — |

The production build compiles, and the Vitest suite passes (38 tests).

### Site-wide changes

Most of the work was Bootstrap 4 parity, in `custom-bootstrap.scss` and `bootstrap-overrides.scss`.
Other pages get these for free:

- Children of a `.row` without a column class keep their natural size and no padding.
  `.table-responsive` is full width.
- `b-form-group` has its 1rem bottom margin, and horizontal form groups have `.form-row`'s 10px
  gutters. Help text is a block and `#6c757d`.
- Form controls: `#495057` text, `#ced4da` borders, the double-triangle select arrow, and
  inline-block selects that are 31px in a small input group. The site's `.custom-select` rule
  covers `.form-select`.
- Checkboxes have no bottom margin; badges use Bootstrap 4's padding.
- Dropdowns: wrappers get `.btn-group`, menus open 2px below the toggle (`BDropdown` offset
  default on `<BApp>`), with Bootstrap 4's item padding and border.
- Popover titles are bold with a 1.2 line height and Bootstrap 4's background and padding.
- Sticky-header table headings are `#212529`. `$color-contrast-dark` is `#212529`.
- No smooth scrolling.

### Page-level template changes

- `label-align-sm="right"` is `"end"` (Master Games).
- `custom-class` is `class` on the tag badge popover.
- `mr-2` and `ml-1` are `me-2` and `ms-1` (Game Changes, home).
- The release-date help popovers have `.right`, Bootstrap-Vue 2's default placement.

### Left for Steve

The items under "Already noted for Steve" above. Claude changed none of them. Steve fixed all but
Advanced Game Stats on 2026-11-05.

### Not checked

- The Master Game statistics chart (see above).
- Most Desired Reviews' table, and the all-time Most Dreams Dashed table: their API calls fail on
  both branches.
- Interactions that write (none on these pages were exercised).

## Steve's fixes applied elsewhere (2026-11-05)

Steve's fixes above, applied by Claude everywhere else they came up. Not yet checked in a browser:
the league, publisher, royale and form pages they touch need a look.

| Fix | Where it came from | Commit |
|---|---|---|
| `<b-alert show>` is `:model-value="true"`, and a bound `:show` is `:model-value` (a bound `show` never hid or showed anything) | 5840abbd2 | ee025e7 |
| `@change` on `b-form-select` and `b-form-checkbox`, and `@input` on a `b-form-checkbox`, are `@update:model-value`. The royale group search box's `:value` and `@input` are `:model-value` and `@update:model-value`. The league and conference year selects were pushing a route with a DOM `Event` as the year. | 85051da | b8b3f0c |
| The Hall of Fame and publisher cover view game popovers drop `custom-class` and `triggers` for masterGamePopover's props (`click`, `placement="right"`, `teleport-to="body"`, `lazy`, `class`) | 1cf4faf0, cf9440f0b | 5ba531e |
| Every `v-b-popover` object with `title`/`content` functions is plain `title` and `body` strings. The ones that built HTML are `b-popover` components with the trigger in the `#target` slot and the markup in the template. Two unused `getFlexText` methods are gone. | f22652e21 | b82b7b4 |
| 22 more decimal `b-table` columns have `sortCompare: numericComparer`. `numericComparer` follows dotted keys and sorts missing values first, as Bootstrap-Vue 2 did. Unit tests in `tests/numericComparer.test.ts`. | bc4a7180c | 25919f5 |
| Master Games clears its search and resets its page count when the game list changes (league, eligibility, taken or unreleased filters) | eb565d9bf | 0aef622 |

Found while doing this, not changed:

- `league.vue`, `conference.vue` and `manageDrafts.vue` listen for `@dismissed` on dismissible
  alerts. bootstrap-vue-next has no `dismissed` event; the close button emits `close`, then `hide`
  and `hidden`. The alert still closes, but the league and conference manager message is never
  dismissed on the server, so it comes back on the next load. `@close` is the likely replacement.
- Eleven forms listen for `@input` on `possibleMasterGamesTable`, `possibleCounterPicksTable` and
  `possibleRoyaleMasterGamesTable` (`newGameSelected`, `addGameToQueue`). Those tables emit only
  `update:modelValue`, so the handlers never run. That's a Vue 3 `v-model` change, not
  bootstrap-vue-next. `@update:model-value` is the likely replacement.
- Master Games: checking or unchecking a "Filter On" box while a search is active doesn't update the
  page count, because the filter function changes but `filter` doesn't.
- `activeBids.vue` sorts by `maximumReleaseDate`, but the column key is
  `masterGame.maximumReleaseDate`, so the table loads unsorted. This was already wrong before the
  migration.
