# EditorConfig incremental adoption

Adopting rules from `orig-editorconfig-branch` one at a time via format, not manual edits.

## Workflow

1. Add rule to `src/.editorconfig` at `:warning`
2. Run `dotnet msbuild FantasyCritic.slnx -t:Format` (from `src/`)
3. Commit the rule + any resulting code changes
4. Repeat

Format applies `dotnet format style --severity warn` — only rules at warning severity or higher are auto-fixed.

## Current state (after reset to `65a50550`)

HEAD is **`52fd5b8b`** — `Wire NUnit2045 (Assert.EnterMultipleScope) into the format scripts.`

### Applied via format (keep)

| Rule | Commit | Format result |
|------|--------|---------------|
| Remove `this.` qualification | `fe2c5eed` | 18 domain `Equals` files auto-fixed |
| Predefined type aliases (`int`/`string` vs `Int32`/`String`) | `65a50550` | Rule added; codebase already conformed — no code changes |
| File-scoped namespaces (`csharp_style_namespace_declarations = file_scoped`) | `a1a3a01a` | Rule added; codebase already conformed (0 block-scoped namespaces) — no code changes |
| `Assert.EnterMultipleScope` (NUnit2045), scoped to `FantasyCritic.Test`/`FantasyCritic.IntegrationTests` | `6525eb66` | 29 test files auto-fixed (5 unit, 24 integration). `dotnet msbuild -t:Format` did not pick up NUnit2045 (3rd-party analyzer, not covered by the `style` command) — required targeted `dotnet format analyzers <csproj> --diagnostics NUnit2045 --severity warn`, run repeatedly (~5x) until `--verify-no-changes` passed clean, since each pass only fixes non-overlapping groups per file. Verified: full unit test suite (2234 passed) and integration suite (234 passed) green after the change. |

### Reverted (do not re-apply manually)

| Rule | Why it was reverted |
|------|---------------------|
| Private `_` field naming | Format cannot fix IDE1006 solution-wide: `NamingStyleCodeFixProvider doesn't support Fix All in Solution`. A manual 32-file rename was attempted and rolled back. |
| Unused private / mark static (IDE0051, IDE0052, IDE0059) | Uncommitted partial work was discarded on reset. Format auto-fixed almost nothing; IDE0052 has no code fix. |

## Remaining work

### Confirmed goals (from `orig-editorconfig-branch` review)

| # | Rule | Format-only viable? | Notes |
|---|------|---------------------|-------|
| 1 | Predefined type aliases | ✅ Done | `65a50550` |
| 2 | Remove `this.` | ✅ Done | `fe2c5eed` |
| 3 | Private `_` field naming | ❌ Blocked at solution scope | Per-project `dotnet format` or IDE per-file fixes only; no solution-wide auto-fix |
| 4 | Unused private / mark static | ⚠️ Mostly blocked | IDE0052 (unread private fields) has **no code fix**; static marking on repos did not run at warn severity |
| 5 | `Assert.EnterMultipleScope` (NUnit2045) | ✅ Done | `6525eb66`; see below for the targeted-format workflow required |
| 6 | File-scoped namespaces (`csharp_style_namespace_declarations`) | ✅ Done | `a1a3a01a` — found via full diff of `orig-editorconfig-branch` `.editorconfig` vs current; had been missed in earlier triage. Rule added; codebase already conformed — no code changes |

A full `git diff orig-editorconfig-branch:src/.editorconfig main:src/.editorconfig` was reviewed line-by-line (2026-07-12) to confirm no other rules from the orig branch were missed. Result: only file-scoped namespaces (above) had been overlooked; everything else in the orig branch's `.editorconfig` maps to a row in this table, the skip list below, or a guard/opt-out that main already matches (e.g. disabling expression-bodied members, `var` for built-ins, primary constructors, switch expressions, `CA2007`).

### Assert.EnterMultipleScope (NUnit2045) — done

Applied in `6525eb66`. Scoped to test projects in `src/.editorconfig`:

```editorconfig
[{FantasyCritic.Test/**,FantasyCritic.IntegrationTests/**}.cs]
dotnet_diagnostic.NUnit2045.severity = warning
```

`dotnet msbuild FantasyCritic.slnx -t:Format` did **not** fix these — NUnit2045 comes from the NUnit.Analyzers 3rd-party package, which the MSBuild `Format` target's `style` command doesn't cover. Had to target it directly per project with the `analyzers` subcommand (note: `--severity` takes `warn`, not `warning`):

```powershell
dotnet format analyzers src/FantasyCritic.Test/FantasyCritic.Test.csproj --diagnostics NUnit2045 --severity warn
dotnet format analyzers src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj --diagnostics NUnit2045 --severity warn
```

The fixer only resolves non-overlapping violation groups per pass, so a single run left some adjacent `Assert.That` runs unwrapped (and unwrapped groups fail the build under `TreatWarningsAsErrors`). Ran the command ~5 times per project until `--verify-no-changes` passed clean on both.

Effect: wraps groups of independent `Assert.That` calls in `using (Assert.EnterMultipleScope()) { ... }` so all failures report in one test run. Result: 29 files changed (5 unit, 24 integration) — close to orig branch's ~32 files. Full test suites verified green after: `FantasyCritic.Test` (2234 passed), `FantasyCritic.IntegrationTests` (234 passed, run with `-c Release` against the local MySQL Docker container).

**Now automated** (`52fd5b8b`): `scripts/Format.ps1` and `scripts/format.sh` run this check/fix loop for you via new `FormatAnalyzers`/`FormatAnalyzersCheck` MSBuild targets in `src/Directory.Solution.targets`. Running the format script once now applies (or, with `-Check`, verifies) every agreed rule in one command — no need to hand-run the `dotnet format analyzers` commands above anymore.

One gotcha hit along the way: `dotnet format analyzers` wrote CRLF line endings for the newly-inserted `using (Assert.EnterMultipleScope())` blocks on Windows, violating `end_of_line = lf`. A follow-up `dotnet msbuild -t:Format` (whitespace pass) fixed it — worth remembering if a future analyzer-based rule shows the same symptom.

### Explicitly skipping

- Null propagation (`?.`)
- Broader pattern matching (`is < 2 or > 20`, `csharp_style_prefer_pattern_matching`)
- IDE0037 anonymous-type member inference (`DraftID = draft.DraftID` → `draft.DraftID`)

## Suggested order (format-only, realistic)

1. ~~**Assert.EnterMultipleScope**~~ — done (`6525eb66`)
2. **Unused private / mark static** — only if willing to accept manual cleanup for IDE0052 violations format cannot fix
3. **Private `_` naming** — skip or defer unless approach changes (per-project format, IDE fixes)

All format-only-viable rules identified from `orig-editorconfig-branch` are now applied. What remains (unused private/static, private `_` naming) requires manual cleanup beyond what the formatter can do — see "Format limitations discovered" below.

## Format limitations discovered

| Diagnostic | Issue |
|------------|-------|
| IDE1006 (naming) | `NamingStyleCodeFixProvider` does not support Fix All in Solution |
| IDE0052 | No associated code fix — unused private fields must be removed by hand |
| IDE0051 / IDE0059 | Little or no auto-fix at solution scope with `--severity warn` |
| NUnit2045 | 3rd-party analyzer diagnostic — not covered by `dotnet msbuild -t:Format` (`style` command); needs `dotnet format analyzers <csproj> --diagnostics NUnit2045 --severity warn` per project, run repeatedly until `--verify-no-changes` is clean. Automated in `scripts/Format.ps1` / `scripts/format.sh` (`52fd5b8b`) via the `FormatAnalyzers`/`FormatAnalyzersCheck` targets, so this is now handled by the normal format script. |

## Reference: orig branch vs main

`orig-editorconfig-branch` used `dotnet format --severity info` (all suggestions), which bundled many changes including NUnit analyzer fixes. Main uses `--severity warn` for incremental, reviewable commits.

## ClientApp format/lint pipeline (Prettier + ESLint) — done

Goal driving this: one command (`scripts/Format.ps1` / `scripts/format.sh`, with `-Check` for CI-style verification) that applies or verifies *every* agreed rule, C# and ClientApp alike. The C# side already worked; the ClientApp side turned out to be completely broken in several independent ways. Fixed 2026-07-12, HEAD **`49497ce9`**, ten commits (oldest/simplest → newest/most involved):

| Commit | What |
|--------|------|
| `b47fbe2b` | Fixed the actual bug that started this: `criticsRoyaleInfo.vue` had `<ul>` nested inside `<p>` (invalid HTML — browsers implicitly close `<p>` before block content), which crashed Prettier's parser outright. |
| `10d905bc` | `eslint.config.mjs` imported `@typescript-eslint/eslint-plugin` (whose `configs.recommended` is a single object) instead of the `typescript-eslint` meta-package (array of flat configs) the `...tseslint.configs.recommended` spread needs. Crashed every ESLint run with `TypeError: ... is not iterable`. ESLint had apparently not run successfully in a while. |
| `b615d283` | `--ignore-path .gitignore` was removed entirely in ESLint 9's flat config (`Invalid option`). Moved the intent (skip `dist/` and the NSwag-generated API client, mirroring root `.gitignore`) into `eslint.config.mjs`'s own `ignores` array. |
| `db86e57f` | Once ESLint could run, it lit up `no-undef` on `process`/Node globals in root scripts (`aspnetcore-https.js`, `vite.*.config.js`, `eslint.config.mjs` itself — all Node context, not browser) and in `basicMixin.js` (`process.env.NODE_ENV`, which Vite statically replaces at build time even in browser code). Added the missing globals. |
| `ce2693d8` | Biggest false-positive source: `eslint-plugin-vue`'s `flat/essential` preset targets **Vue 3** and flags valid Vue 2 syntax (`.sync`/`.native` modifiers, filters) as errors. This app runs Vue 2.7. Switched to the dedicated `flat/vue2-essential` preset — dropped errors from **217 → 16**. |
| `7a66c231` | The original symptom: `npm exec -- prettier --check src/` (and the equivalent `eslint` invocation) dumped megabytes of raw matched-file content to stdout on this machine/npm version instead of terse pass/fail output — even on a fully clean run. `npm run <script>` doesn't have this problem. Added `format:check`/`lint:check` npm scripts mirroring the existing `format`/`lint` scripts, and switched `Format.ps1`/`format.sh` to call them instead of `npm exec --`. |
| `5b0f147a` | Fixed the remaining mechanical, zero-behavior-change findings: unused `catch (error)` bindings (6 files, → ES2019 optional catch binding) and `prefer-const` (2 files). |
| `ad590901` | Two files aliased `let outerScope = this` to reach the component from a `function()` callback passed to `.forEach`. Converted to arrow functions (preserve `this` lexically) and dropped the alias. |
| `2ff1c500` | Deleted `validUnannounced() {}` in `masterGameRequest.vue` — an empty computed property, always returning `undefined`, referenced nowhere else. Dead stub from an earlier iteration of the page. |
| `49497ce9` | `royalePublisherGraph.vue`'s `releaseAnnotations` computed getter called `this.$set(...)` on `byDate`, a plain local object created fresh inside the getter (never part of Vue's reactive data). `$set` exists only to make reactive-object property adds trigger re-renders, which is moot for a non-reactive local — replaced with a plain assignment. Fixes `vue/no-side-effects-in-computed-properties` and `vue/no-deprecated-delete-set` with no behavior change. |

Verified after each stage: `scripts/Format.ps1` and `scripts/Format.ps1 -Check` both exit 0 end-to-end (C# whitespace/style/NUnit2045 + ClientApp prettier + eslint), and `npm run build` still succeeds.

### What's next

- **C# side**: the two blocked items from "Remaining work" above (private `_` field naming, unused-private/mark-static) are unchanged — still need a manual approach since format can't fix them solution-wide.
- **ClientApp side**: the pipeline is clean at `flat/vue2-essential` (ESLint's "essential" tier — Vue's baseline error-prevention rules). Stepping up to `flat/vue2-strongly-recommended` or `flat/vue2-recommended` would surface more style-level findings; not done here since it wasn't part of the original ask — worth a deliberate follow-up if wanted.
- **No CI enforcement found** (no `.github/workflows`, no `azure-pipelines.yml` in this repo) — `Format.ps1 -Check` / `format.sh -Check` aren't wired into any pipeline yet, so nothing currently stops this from drifting out of sync again. Worth deciding whether/where to add that check if this repo's CI lives elsewhere.
- Build output surfaced a large, unrelated pile of Sass legacy `@import`/`darken()`/`lighten()` deprecation warnings (Dart Sass 3.0 removal). Not blocking anything today; noted here only because it was seen in passing and is a separate future cleanup (modern `@use`/`color.scale`), not part of this effort.
