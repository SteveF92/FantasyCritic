# Vue 3: league and conference pages pass

The last ten pages, compared with main and fixed. Everything that doesn't submit a POST is in scope
here. Testing the requests themselves is the next step.

## Pages

1. League (`/league/:id/:year`), including its modals
2. Publisher
3. Edit League
4. Manage Drafts
5. League History
6. League All-Time Stats
7. Conference
8. Conference History
9. Create League
10. Create Conference

## Setup

Same as `vue3-read-only-pages-plan.md`: main on Vite 44478, vue3-migration on Vite 44477, both on the
backend at 4554 and `fantasycritic-fromsnapshot`, signed in through `/dev/sign-in-as`, compared in
headless Chrome at 1280 and 375.

The harness blocks every non-GET request to `/api` and `/Account` and logs it, so no action can write.

## Leagues used

| State | League |
|---|---|
| Mid-season, manager and player | The Original Memers (2026) |
| Finished year | The Original Memers (2024) |
| Active draft | Draft Start Test |
| Second draft pending | Primary Test League |
| Pre-draft, in a conference | Test Conference League |
| Player only, inactive | Ace Attorneys |
| Manager's message showing | Unofficial Official |

## Rules

- Steve asked for everything that doesn't need a POST to be fixed, so JavaScript fixes are in scope this
  time, not just notes.
- Forms get opened, filled and searched, but not submitted.
- Commit in logical groups.

## Report (2026-11-05)

Every page was compared with main at 1280 and 375, by screenshot and by the position, size and style of
every element with text. The harness blocked all writes, and no blocked request showed up in any run.

| Page | Checked | Result |
|---|---|---|
| League | Seven leagues in different states; every action in the sidebar opened at 1280; one league's at 375; searching and selecting a game, sorting, toggles, year select, game and hover popovers | Matches |
| Publisher | Own and other's publisher; cover view; sort mode with sorted columns and dropped games; move mode up to confirming | Matches |
| Edit League | Mid-season and pre-draft; special slots added, removed, tags chosen; roster size warnings; checkboxes | Matches |
| Manage Drafts | Read, edit and delete-confirm views | Matches |
| League History | Bid/drop cards, sorting | Matches |
| League All-Time Stats | Sorting, game popovers | Matches |
| Conference | Player and manager; every manager action opened; year select, sorting | Matches |
| Conference History | Player and manager | Matches |
| Create League | Standard, Multi Draft and One Shot, experience levels, settings changes | Matches |
| Create Conference | Standard, Multi Draft and One Shot | Matches |

"Matches" means no differences except antialiasing (some tables draw text with ClearType on one side and
greyscale on the other, which shows up most at 375) and League Options being 1px higher at 375.

The Vitest suite passes (45 tests) and the production build compiles.

### JavaScript fixes

- `structuredClone(toRaw(x))` throws on a plain array holding reactive objects, which blanked Edit League and
  manager conference pages. `deepClone` in `utilities/` replaces all 14 calls.
- `b-table` throws on null `items`, which blanked League History.
- `b-table` reads `item[key]` rather than following a path, so a column keyed `masterGame.gameName` sorted as
  nothing and `yearsWon.length` showed blank. App.vue now gives every table `defaultTableComparer`; the
  `.length` columns have accessors.
- `hide-footer` is `no-footer` (29 modals had stray Cancel and OK buttons).
- `@input` on the possible-games tables and `masterGameTagSelector` is `@update:model-value`, and
  `@dismissed` on `b-alert` is `@close`.
- `$bvModal.show` is a ref's `show()` in `currentBidsForm`, `currentDropsForm` and `gameQueueForm`.
- `v-b-popover...rightbottom`, a placement bootstrap-vue-next ignores, is `.right`.

### Site-wide styling

All in `bootstrap-overrides.scss`, `custom-bootstrap.scss` and `modal.css`. Other pages get these too:

- `bg-*` on table rows and cells shows through Bootstrap 5's cell paint. Row variants use the theme color with
  white text.
- `.rounded-pill` has `.badge-pill`'s padding; `text-bg-info` and `text-bg-warning` have white text.
- Inputs keep the browser's look and spacing, including date fields. Selects with `.form-control` keep their
  height, disabled selects are faded and grey, and a select with both classes has one arrow.
- `.input-group` inputs don't stretch to a taller button, and `.input-group-btn` doesn't overlap the input.
- `plain-check` restores Bootstrap 4's plain checkbox for the forms that don't use `b-form-checkbox`.
- The modal close button is the text × again.
- Hidden sort text no longer widens the whole page on phones.

### For the next step: requests

Changed here but not run, since they only matter once a request is sent or there's data to act on:

- `@close` on the league and conference manager message, which POSTs the dismissal.
- The cancel bid and cancel drop confirmations. No league used here had a pending bid or drop to cancel.
- Everything behind a submit button.

Not exercised, for lack of data in the snapshot leagues: outstanding invites, active trades and votes,
pending bids and drops, and the Start Draft and Draft Complete modals.

### Still open, outside these ten pages

- `$bvModal.msgBoxConfirm` and `$bvModal.show` still throw in `leagueTable` (unfollow, on the home page),
  `recentJobsTable`, `adminConsole`, `adminSiteAnnouncements`, `pendingMasterGameUpdates` and `royaleGroup`.
- `recentJobsTable` listens for `@input` on a multiselect, which never fires.
- At 375, Support Tickets' filters are narrower than main's and Master Game Request's table is wider. On the Admin
  Console the Requests and Change Requests buttons are 4px narrower at 1280.

### Left as is

- `b-pagination` with exactly as many pages as its limit (5) shows `1 2 3 4 …` where main shows `1 2 3 4 5`.
  bootstrap-vue-next compares `pages < limit`. Page 5 is reachable with the next and last buttons.
- Sorting a column on Publisher in sort mode logs `Duplicate keys found during update` from `primary-key`, with
  no visible effect. Main doesn't log it.
