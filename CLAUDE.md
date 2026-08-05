# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Fantasy Critic (fantasycritic.games) — fantasy football, but for video games. Players draft upcoming games and score based on OpenCritic review scores. ASP.NET Core (.NET 10) backend + Vue 2.7 SPA, backed by MySQL.

Detailed conventions live in `.cursor/rules/*.mdc` and `.cursor/skills/` — they apply here too. This file summarizes the essentials.

## Environment

Development happens on Windows; the shell is PowerShell, not bash. Use PowerShell syntax for interactive commands (`$env:FOO = "bar"`, `;` not `&&`, `.\script.ps1`). Cross-platform `.sh` scripts are fine as committed source files.

## Commands

```powershell
# Start MySQL (port 3307) + run DbUp migrations + seed game data — safe to re-run anytime
docker compose -f infrastructure/docker-compose-mysql.yaml up

# Run the web app (https://localhost:44477); starts the Vite client automatically in dev
dotnet run --project src/FantasyCritic.Web/FantasyCritic.Web.csproj

# Build everything
dotnet build src/FantasyCritic.slnx

# Unit tests (no DB required)
dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj

# Single test / fixture
dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj --filter "FullyQualifiedName~EligibilityTests"

# Integration tests (Docker MySQL must be up; use -c Release if the dev web server is
# running, since the dev server holds Debug DLL locks)
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release

# Regenerate NSwag API clients — required after ANY API surface change, before integration tests
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj
scripts/Regenerate-ApiClient.ps1

# Format everything (C# editorconfig + NUnit analyzer + ClientApp Prettier/ESLint)
scripts/Format.ps1          # apply
scripts/Format.ps1 -Check   # verify only (CI-style)
```

ClientApp (`src/FantasyCritic.Web/ClientApp`): `npm install` first; `npm run lint` / `npm run format` for JS-only passes; `npm run client` runs the SPA standalone against the production API.

## Architecture

### Solution layering (where code belongs)

| Project | Role |
|---|---|
| **FantasyCritic.Lib** | Domain model (`Domain/`), business services (`Services/`), repository *interfaces* (`Interfaces/`), scheduled task logic (`Scheduling/`), Discord bot logic (`Discord/`). Domain stays pure: no DB column names, no JSON attribute coupling. |
| **FantasyCritic.MySQL** | Dapper repositories (`*Repo`) implementing Lib interfaces. Row types under `Entities/` end in `Entity`. |
| **FantasyCritic.Web** | API controllers (`Controllers/API/`), Web-owned `ViewModel`/`Request`/`Response` models, SignalR `UpdateHub` (live draft updates), and hosts the Vue SPA. Uses System.Text.Json + NodaTime serialization; shared `JsonSerializerOptions` come from `FantasyCriticJsonOptions` in Lib. |
| **FantasyCritic.Lib/SharedSerialization** | The *deliberate* home for Entity/ViewModel shapes shared across projects. Not legacy — but only for types that genuinely cross project boundaries. |
| **FantasyCritic.FakeRepo** | In-memory repository doubles for unit tests. |
| **FantasyCritic.DatabaseUpdater** | DbUp migrations. Schema changes happen **only** here: new scripts in `Scripts/Sequential/` with the next dated filename. Never hand-edit the DB. |
| **FantasyCritic.ApiClient** | NSwag-generated C# client (gitignored output) used by integration tests. |
| **FantasyCritic.DiscordBot** | Standalone bot host; the actual command/push logic lives in Lib. |

Satellite console tools: LocalDatabaseTool (seeds dev DB from prod data), RdsSnapshotManager (import prod snapshots), MasterGameUpdater, BetaSync, DBUtility, TestDataScrubber. Supporting libs: AWS (Secrets Manager/S3), Postmark + EmailTemplates (email), OpenCritic/Patreon/GG clients inside Lib.

### Typed API contract (NSwag)

Integration tests and the Vue client both consume clients generated from the OpenAPI spec. The whole point is compile-time breakage on contract changes — never use raw URLs, anonymous objects, or hand-rolled request types in tests. If a generated client method returns `Task` instead of `Task<T>`, add `[ProducesResponseType<T>(StatusCodes.Status200OK)]` to the controller action rather than accepting an untyped method. See `.cursor/rules/integration-tests.mdc` for the full test conventions (fixtures, `LeagueFixtureBuilder`, `NewUser()`, etc.).

### Frontend

Vue 2.7 (Options API — match the file you're editing) + Vite + Bootstrap 4/bootstrap-vue + Vuex + Luxon + axios. Generated TS client at `ClientApp/src/api/generated/` with hand-written singletons in `ClientApp/src/api/clients.ts`.

## C# conventions

- **No `Async` suffix** on app/repo/service methods (`GetLeague`, not `GetLeagueAsync`).
- **`Result` / `Result<T>`** (CSharpFunctionalExtensions) or domain result types for expected failures — don't throw for control flow. No `out`/`ref` params.
- **Read-only collection surfaces**: `IReadOnlyList<T>` / `IReadOnlyDictionary<,>` on public APIs; build with `List<T>` locally, copy `IEnumerable<T>` ctor params once via `.ToList()`.
- **NodaTime** (`Instant`, `LocalDate`) for all time in domain and persistence — not `DateTime`.
- String-backed domain enums extend `TypeSafeEnum<T>` (`Lib/Enums`), not bare `enum` or raw strings.
- `class` for entities with identity/behavior; `record` for immutable bundles and calculation results.
- No static mutable state.
- **TreatWarningsAsErrors** is on across the main projects — the solution currently builds with zero warnings; keep it that way.

## Plans

Implementation plans are committed to `.cursor/plans/` (commit the plan file before and after edits so revisions are diffable in history).
