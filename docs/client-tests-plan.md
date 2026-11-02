# Vitest for the client app

## Context

`docs/vue3-upgrade-plan.md` lists **Tests** under "Before the flip": there are none in the
client app. Set up Vitest with `@vue/test-utils` v1 now (v2 comes with the flip), and start with
the `leagueStore` getters that came out of `LeagueMixin` — `counterPickBiddingAllowed` and the
`pendingDraft*` set. The point is a safety net before the Vue 3 flip changes the framework under
them, and a place for tests to accumulate as files are touched.

Decisions taken:
- **Tests live in `tests/`**, mirroring `src/` (`tests/store/leagueStore.test.ts`). Colocation's
  only real advantage is seeing the test beside the file; not worth overriding the preference.
- **Node goes to the current LTS (24)** locally, in CI and in the web Dockerfile, so all three
  match. Vitest 4 needs Node 20+, so 22 would work, but there's no reason to stay behind.
- Tests import `describe`/`it`/`expect` from `vitest` explicitly rather than turning on globals,
  so no ambient types are needed.
- Tests run through the existing `@/` alias, so they read the same as app code.

The client package lives at `src/FantasyCritic.ClientAppVue2`; paths below are relative to it
unless they start at the repo root.

## Steps

One commit per step, stopping for review after each, per CLAUDE.md. The plan itself gets
committed to `docs/client-tests-plan.md` first.

### 1. Node 24 LTS

- Local: Steve installs Node 24 LTS (e.g. `winget upgrade OpenJS.NodeJS.LTS`, or the nodejs.org
  installer). Worth also deleting the stale nvm v16 that shadows it in the Bash tool.
- `.github/actions/setup-build/action.yml`: `node-version: '24'`.
- `src/FantasyCritic.Web/Dockerfile`: `setup_22.x` → `setup_24.x`.
- Verify: `node -v` in PowerShell; `npm ci`, `npm run build`, `npm run lint:check` still pass.

### 2. Vitest and the first leagueStore tests

- devDependencies: `vitest` (4.x; supports Vite 6) and `jsdom`.
- New `vitest.config.js`, separate from `vite.config.js` (which reads the dev HTTPS cert at load
  and would fail in CI). Same shape as `vite.build.config.js` — `@vitejs/plugin-vue2` and the
  `@` alias — plus `test: { environment: 'jsdom', include: ['tests/**/*.test.{js,ts}'] }`.
- `package.json` scripts: `"test": "vitest run"`, `"test:watch": "vitest"`. Widen
  `format`/`format:check` to `prettier --write src/ tests/` and `--check src/ tests/`.
- `tsconfig.json`: add `tests/**/*.ts` to `include`.
- `eslint.config.mjs`: add `vitest.config.js` to the Node-globals file list. ESLint already lints
  `.`, so `tests/` is covered; the Vue 3 deprecation block targets `src/**` only, which is fine
  for tests.
- `tests/store/leagueStore.test.ts`:
  - `beforeEach(() => setActivePinia(createPinia()))`, then `useLeagueStore()` and assign
    `store.leagueYear` directly — getters only read state, so no axios or DOM is involved.
  - A small local factory for a minimal `leagueYear` (settings, `drafts`, `publishers`,
    `enableBids`, `supportedYear`, `publicBiddingGames`) with overrides per test.
  - `counterPickBiddingAllowed`: false when bidding isn't allowed (first draft unfinished, an
    active draft, bids disabled); true for pickup systems other than
    `SemiPublicBiddingSemiPublicCounterPicks`; true when there's no `publicBiddingGames`; follows
    `anyHiddenCounterPickBids` otherwise.
  - The `pendingDraft*` set, with `vi.useFakeTimers()` + `vi.setSystemTime(...)` so Luxon's
    `DateTime.local()` is fixed: `pendingDraftIsWithinSevenDays` (no date, 7 days out, 8 days
    out), `pendingDraftIsOverdue`, `pendingDraftIsImminent` (first draft; order set with no date;
    within seven days), `pendingDraftCalloutVisible` (hidden during an active draft; first;
    overdue; within seven days), and `hasPendingOrActiveDraft`.
- Importing `leagueStore` pulls in `authStore` → `@/api/clients` → the gitignored NSwag
  TypeScript client, so tests need the generated client to exist, as the build already does.
- Verify: `npm test` passes; break a getter on purpose and see it fail; `npm run lint:check` and
  `npm run format:check` pass.

### 3. @vue/test-utils v1 and a first component test

- devDependency `@vue/test-utils@^1.3.6` (its `vue-template-compiler` peer must match the
  installed `vue` 2.7.x exactly; pin it if npm doesn't pick the same version).
- `tests/components/dateInput.test.ts`: mount `@/components/dateInput.vue` with a value and
  check the input shows it; set a date and check it emits `input` with `YYYY-MM-DD`; clear it and
  check it emits `null`, not `''`. Small, but it proves SFC compilation and mounting work, which
  is what moves to v2 at the flip.
- Verify: `npm test` runs both files green.

### 4. CI and docs

- `.github/actions/build-and-test/action.yml`: a "Client unit tests" step after "Unit tests"
  (the generated TS client exists by then), in `src/FantasyCritic.ClientAppVue2`, running
  `npm test -- --reporter=default --reporter=junit --outputFile=${{ github.workspace }}/test-results/client.xml`
  so the existing upload step picks it up. `deploy.yml` uses this action too, so deploys gate on
  the client tests as well.
- `CLAUDE.md`: add `npm test` / `npm run test:watch` to the client app line.
- `docs/vue3-upgrade-plan.md`: move Tests from "Before the flip" to the Done table (what's
  covered, where tests live, that `@vue/test-utils` moves to v2 at the flip — already listed
  there), and note Node 24.
- Verify: push the branch and confirm the CI step runs and the results artifact contains
  `client.xml`.

## Files

- `src/FantasyCritic.ClientAppVue2/package.json`, `vitest.config.js` (new), `tsconfig.json`,
  `eslint.config.mjs`
- `src/FantasyCritic.ClientAppVue2/tests/store/leagueStore.test.ts` (new),
  `tests/components/dateInput.test.ts` (new)
- `.github/actions/setup-build/action.yml`, `.github/actions/build-and-test/action.yml`
- `src/FantasyCritic.Web/Dockerfile`
- `CLAUDE.md`, `docs/vue3-upgrade-plan.md`, `docs/client-tests-plan.md` (new)

## Notes

- Run npm through the PowerShell tool, not Bash (the Bash tool resolves the stale nvm Node).
- `scripts/Format.ps1` / `format.sh` call `npm run format(:check)`, so widening those scripts
  covers `tests/` with no change to the PowerShell or shell scripts.
