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
- **Node goes to the current LTS (26)** locally, in CI and in the web Dockerfile, so all three
  match. Vitest 4 needs Node 20+, so 22 would work, but there's no reason to stay behind.
- Tests import `describe`/`it`/`expect` from `vitest` explicitly rather than turning on globals,
  so no ambient types are needed.
- Tests run through the existing `@/` alias, so they read the same as app code.

The client package lives at `src/FantasyCritic.VueClientApp`; paths below are relative to it
unless they start at the repo root.

## Steps

One commit per step, stopping for review after each, per CLAUDE.md. The plan itself gets
committed to `docs/client-tests-plan.md` first.

### 1. Node 26 LTS — done

- Local: Node 26.13.0 through nvm-for-Windows (`nvm install` / `nvm use`), whose symlink is on
  the system PATH, so Visual Studio and `dotnet run` get it too, not only shells. The stale
  nvm-sh v16 is deleted.
- `.github/actions/setup-build/action.yml`: `node-version: '26'`.
- `src/FantasyCritic.Web/Dockerfile`: `setup_22.x` → `setup_26.x`.
- Verified on 26.13.0: `npm ci`, `npm run build`, `npm run lint:check` and
  `npm run format:check` pass. The Docker image is unbuilt until CI's image job runs.

### 2. Vitest and the first leagueStore tests — done

- devDependencies: `vitest` 5 (supports Vite 6.4) and `jsdom` 30.
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
- Verified: 29 tests pass in about 1.4 seconds warm (the first run took 19, loading jsdom).
  Loosening the seven-day check to `< 7` and making counter pick bidding always open after the
  reveal each failed the expected tests. Lint and the Prettier check pass, and `tsc` reports
  nothing in `tests/` (its one error, in `src/api/clients.ts`, was already there).

### 3. @vue/test-utils v1 and a first component test — done

- devDependency `@vue/test-utils@^1.3.6`. npm installs its `vue-template-compiler` peer at
  2.7.16, matching `vue`; that's the final Vue 2 release, so the two can't drift apart.
- `tests/shims-vue.d.ts` declares `*.vue` modules so a `.ts` test can import a component. A
  `vue-tsc` check would replace it.
- `tests/components/dateInput.test.ts`: mount `@/components/dateInput.vue` with a value and
  check the input shows it and follows a new one; set a date and check it emits `input` with
  `YYYY-MM-DD`; clear it and check it emits `null`, not `''`. Small, but it proves SFC
  compilation and mounting work, which is what moves to v2 at the flip.
- Verified: 33 tests pass with no Vue warnings; emitting `''` on clear fails the clearing test.
  Lint, Prettier and `tsc` are clean apart from the existing `clients.ts` error.
- `npm audit` goes from 22 to 27 findings, all moderate and dev-only, from test-utils' own
  dependencies (`vue-template-compiler`, and `js-beautify` → `glob`/`editorconfig`). There's no
  fixed v1; they leave with it at the flip.

### 4. CI and docs — done, CI run unverified

- `.github/actions/build-and-test/action.yml`: a "Client unit tests" step after "Unit tests",
  in `src/FantasyCritic.VueClientApp`, running
  `npm test -- --reporter=default --reporter=junit --outputFile.junit=${{ github.workspace }}/test-results/client.xml`
  so the existing upload step picks it up. It runs whenever "Generate API clients" succeeded
  (that step now has `id: generate`), so a failing .NET test project doesn't hide client
  results, but a broken build doesn't produce a confusing missing-module failure. `deploy.yml`
  uses this action too, so deploys gate on the client tests as well.
- `CLAUDE.md`: `npm test` / `npm run test:watch` on the client app line, and stopping the dev
  server before `npm ci`.
- `README.md`: a short client tests paragraph under the integration tests, for contributors.
- `.cursor/rules/fantasy-critic-frontend.mdc`: a Tests section (where they live, how store tests
  are set up), and "mixins" dropped from the folders it names.
- `docs/vue3-upgrade-plan.md`: Tests moved from "Before the flip" to the Done table.
- Verified locally: the CI command passes and writes a 33-test `client.xml`. **Left:** push the
  branch and confirm the step runs on GitHub and the results artifact contains `client.xml`.

## Files

- `src/FantasyCritic.VueClientApp/package.json`, `vitest.config.js` (new), `tsconfig.json`,
  `eslint.config.mjs`
- `src/FantasyCritic.VueClientApp/tests/store/leagueStore.test.ts` (new),
  `tests/components/dateInput.test.ts` (new), `tests/shims-vue.d.ts` (new)
- `.github/actions/setup-build/action.yml`, `.github/actions/build-and-test/action.yml`
- `src/FantasyCritic.Web/Dockerfile`
- `CLAUDE.md`, `README.md`, `.cursor/rules/fantasy-critic-frontend.mdc`,
  `docs/vue3-upgrade-plan.md`, `docs/client-tests-plan.md` (new)

## Notes

- Stop the Vite dev server before `npm ci`: it holds `esbuild.exe` open, and `npm ci` fails
  after it has already deleted half of `node_modules`.
- `scripts/Format.ps1` / `format.sh` call `npm run format(:check)`, so widening those scripts
  covers `tests/` with no change to the PowerShell or shell scripts.
