# CSharpier evaluation

Evaluation of [CSharpier](https://csharpier.com) as a C# formatter for FantasyCritic, conducted July 2026. **Conclusion: not recommended for this repo** — stick with `.editorconfig` + `dotnet format` (already in use on the `formatting-rules` branch).

## What we tried

1. Installed CSharpier as a local dotnet tool (pinned in `.config/dotnet-tools.json`, same pattern as NSwag).
2. Added `.csharpierrc.json` at repo root — intentionally **separate from** `src/.editorconfig` so IDE/analyzer rules were not disturbed. CSharpier prefers `.csharpierrc` over `.editorconfig` when both exist.
3. Seeded CSharpier config from existing conventions:
   - `src/.editorconfig` — indent (4 for `.cs`, 2 for project/XML), LF, charset
   - `src/FantasyCritic.Web/ClientApp/.prettierrc` — `printWidth: 200`, `useTabs: false`, `endOfLine: lf`, `htmlWhitespaceSensitivity: ignore` → mapped to CSharpier's `xmlWhitespaceSensitivity`
4. Pilot file: `src/FantasyCritic.Web/HostingExtensions.cs` — complex DI setup, familiar to the team.

## Line endings

Configs were inconsistent at the start of the evaluation:

| Source | Original | Aligned to |
|--------|----------|------------|
| `src/.editorconfig` | `crlf` | `lf` |
| `src/.gitattributes` | `eol=lf` | (unchanged) |
| `.prettierrc` | `lf` | (unchanged) |
| `.csharpierrc.json` | `lf` | (unchanged) |

On disk, C# files were often CRLF (IDE honoring editorconfig) while Vue files were LF. **Recommendation: LF everywhere** — matches git attributes, Prettier, and cross-platform CI. Developing on Windows does not require CRLF.

## Pilot results (`HostingExtensions.cs`)

Running `dotnet tool run csharpier -- check src` reported **~856 files** that would change on first format — expected for a codebase that has never used CSharpier.

On `HostingExtensions.cs` specifically, CSharpier produced many changes the team disliked, including:

- Collapsing expanded object initializers to one line
- Reflowing lambdas, method chains, and `AddPolicy` calls
- Reordering `using` directives

### Object initializer style (dealbreaker)

Preferred style (existing codebase):

```csharp
DiscordSocketConfig socketConfig = new()
{
    GatewayIntents = GatewayIntents.AllUnprivileged
};
var fantasyCriticSettings = new FantasyCriticSettings
{
    BaseAddress = baseAddress
};
```

CSharpier output:

```csharp
DiscordSocketConfig socketConfig = new() { GatewayIntents = GatewayIntents.AllUnprivileged };
var fantasyCriticSettings = new FantasyCriticSettings { BaseAddress = baseAddress };
```

**This is not configurable.** CSharpier uses fixed heuristics:

- 1 property → one line
- 2 properties → one line if under `printWidth`
- 3+ properties (or over `printWidth`) → breaks to Allman

Lowering `printWidth` does not expand single-property initializers. The maintainer has [explicitly declined](https://github.com/belav/csharpier/issues/691) adding brace-style options (Prettier-style option philosophy).

Workarounds exist (`// csharpier-ignore`, `.csharpierignore`) but do not scale for a codebase with this style.

`dotnet format` with `csharp_new_line_before_open_brace = all` was also tested and did **not** force expansion of single-property object initializers.

## CSharpier vs `dotnet format`

| | **CSharpier** | **`dotnet format`** |
|---|---|---|
| Config | Tiny (`.csharpierrc`) — ~5 options, no more planned | Full `.editorconfig` support |
| Speed | Fast — no MSBuild workspace | Slower — loads solution via MSBuild |
| Opinion | Strong — one output, take it or leave it | Flexible — team rules via editorconfig |
| Format-on-save (VS Code/Cursor) | Good extension story | No first-class format-on-save; IDE or CI/pre-commit |
| Official | Third-party | Ships with .NET SDK |
| Coexistence with analyzers | Second authority; can fight `.editorconfig` | Same rule set as IDE analyzers |

### What CSharpier gives you

- Speed and simplicity
- Deterministic output (no IDE variance)
- Prettier-like "stop debating formatting" philosophy
- Easy CI: `dotnet csharpier check src`

### What it costs

- Cannot tune brace/object-initializer layout
- Large first-run diff (~856 files)
- Conflicts with existing C# conventions
- Adds a second formatting authority alongside `.editorconfig` + analyzers

## Popularity (vs Prettier in JS/TS)

CSharpier is **not** to .NET what Prettier is to JavaScript/TypeScript.

| | **Prettier** | **CSharpier** |
|---|---|---|
| VS Code extension installs | ~70 million | ~343 thousand |
| Package downloads | ~116M/week (npm) | ~300K–2M per NuGet release version |
| GitHub stars | ~50k+ | ~2.2k |
| Default-choice status | Effectively universal in modern JS/TS | Niche but active; most .NET teams use IDE + `.editorconfig` |

## Recommendation

**Do not adopt CSharpier for FantasyCritic.**

Better fit for this repo:

1. **Prettier** — keep for `ClientApp` (already configured).
2. **`src/.editorconfig`** — extend incrementally with C# style/format rules (see `editorconfig-incremental-adoption.md`).
3. **`dotnet format`** — enforce via `dotnet msbuild FantasyCritic.slnx -t:Format` or CI; respects editorconfig.
4. **IDE format-on-save** — Rider/Visual Studio respect editorconfig rules.

CSharpier only makes sense if the team is willing to adopt its opinions wholesale — including collapsed simple object initializers — in exchange for speed and zero formatting debate.

## Artifacts from the evaluation

These were added during the evaluation and can be removed if not keeping CSharpier:

- `.config/dotnet-tools.json` — `csharpier` tool entry (since removed on `formatting-rules` branch)
- `.csharpierrc.json` — CSharpier config (since removed)

The `end_of_line = lf` change in `src/.editorconfig` was kept — it aligns git attributes, Prettier, and IDE settings.

## Commands reference (if revisiting)

```powershell
dotnet tool install csharpier          # local tool
dotnet tool run csharpier -- check src # dry run
dotnet tool run csharpier -- format src
```

## Related plans

- `editorconfig-incremental-adoption.md` — ongoing `.editorconfig` rule adoption via `dotnet format`
