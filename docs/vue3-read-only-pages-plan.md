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

## Report

Added to this file when the pass ends: for each page, what was fixed (with commit), what was
left and why, and questions for Steve.
