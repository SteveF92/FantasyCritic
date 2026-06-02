# C# API Client Autogen Design

**Date:** 2026-06-01  
**Status:** Approved  
**Depends on:** `2026-05-31-openapi-spec-design.md` (OpenAPI spec must be in place)

## Goals

1. Provide a typed, NuGet-publishable C# client for the FantasyCritic HTTP API.
2. The client is generated from the committed `openapi.json` spec — never hand-authored.
3. The first consumer is `FantasyCritic.IntegrationTests`. Future consumers could include external tooling or a public NuGet package.
4. No build-time overhead: generated files are committed to source control; CI never needs the generator installed.

## Tooling

**Generator:** Microsoft Kiota (`microsoft.openapi.kiota`) — Microsoft's official API client generator, used by the Microsoft Graph SDK. Pinned as a local .NET tool via `.config/dotnet-tools.json` so the exact version used to produce the committed files is reproducible.

**Why not build-time generation:**  
The spec is itself *output* of the web build (`Microsoft.Extensions.ApiDescription.Server`). Chaining Kiota generation into the same MSBuild pass creates awkward ordering dependencies and adds ~15–30 seconds to every build. Treating regeneration as an intentional, manual step (like a DB migration) is cleaner: run it when the API changes, review the diff, commit both the updated spec and updated client together.

## Spec File Location

`src/FantasyCritic.Web/wwwroot/openapi.json` — unchanged from the OpenAPI setup. This path is:
- Generated at build time by `Microsoft.Extensions.ApiDescription.Server`
- Committed to git (API contract is diffable in PRs)
- Served publicly at `https://[domain]/openapi.json` via ASP.NET Core's static files middleware (no extra configuration needed)

## New Project: `FantasyCritic.ApiClient`

**Location:** `src/FantasyCritic.ApiClient/`

**Project file:** `FantasyCritic.ApiClient.csproj`
- `TargetFramework`: `net10.0`
- `IsPackable`: `true`
- NuGet metadata: `PackageId`, `Authors`, `Description`, `RepositoryUrl`, `PackageTags`
- `TreatWarningsAsErrors`: `false` — Kiota-generated code produces style warnings that are not ours to fix
- NuGet dependencies (Kiota runtime — these become public dependencies of the package):
  - `Microsoft.Kiota.Abstractions`
  - `Microsoft.Kiota.Http.HttpClientLibrary`
  - `Microsoft.Kiota.Serialization.Json`
  - `Microsoft.Kiota.Serialization.Text`
  - `Microsoft.Kiota.Serialization.Form`
  - `Microsoft.Kiota.Serialization.Multipart`

**Contents:**

```
src/FantasyCritic.ApiClient/
  FantasyCritic.ApiClient.csproj     ← handwritten (never touched by kiota)
  Generated/                         ← kiota output directory
    FantasyCriticApiClient.cs        ← top-level client entry point
    Models/                          ← generated model classes (ViewModels, Request shapes)
    Api/                             ← path-based fluent request builders
```

Kiota outputs into `Generated/` rather than the project root so that `--clean-output` (which wipes the entire output directory before regenerating) never touches the `.csproj` or any future non-generated files.

**Added to solution:** `src/FantasyCritic.slnx`

## Generation Workflow

### One-time setup

```powershell
dotnet tool restore   # installs kiota from .config/dotnet-tools.json
```

### Regenerating the client

Run any time after the OpenAPI spec changes (i.e. after building `FantasyCritic.Web`):

```powershell
scripts/Regenerate-ApiClient.ps1
```

Which wraps:

```powershell
dotnet tool run kiota -- generate -l CSharp `
  -d src/FantasyCritic.Web/wwwroot/openapi.json `
  -o src/FantasyCritic.ApiClient/Generated `
  -n FantasyCritic.ApiClient `
  --clean-output
```

`--clean-output` wipes the `Generated/` directory before regenerating, so removed endpoints don't leave stale files. Kiota is invoked via `dotnet tool run kiota` (local tool, pinned in `.config/dotnet-tools.json`) rather than a global install.

### Full workflow when an API endpoint changes

1. Modify controller / ViewModel in `FantasyCritic.Web`
2. `dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj` → updates `wwwroot/openapi.json`
3. `scripts/Regenerate-ApiClient.ps1` → updates `src/FantasyCritic.ApiClient/`
4. `dotnet build` → verify the solution compiles
5. Commit: spec diff + client diff together in one PR

### `scripts/Regenerate-ApiClient.ps1`

- Resolves all paths relative to the repo root (works from any working directory)
- Checks that `dotnet tool run kiota` succeeds; prints `dotnet tool restore` hint if not
- Runs `dotnet tool run kiota -- generate` with the parameters above
- Prints a brief summary on completion

## IntegrationTests Integration

`FantasyCritic.IntegrationTests` gains a `<ProjectReference>` to `FantasyCritic.ApiClient`. Existing manual HTTP helpers are not removed in this step — migration to the typed client happens incrementally, test fixture by test fixture.

### Usage pattern in tests

```csharp
var httpClient = _factory.CreateClient();
var adapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider(), httpClient: httpClient);
var client = new FantasyCriticApiClient(adapter);

var result = await client.Api.League.LeagueYear.GetAsync(r => {
    r.QueryParameters.LeagueID = leagueId;
    r.QueryParameters.Year = 2025;
});
```

For authenticated endpoints, swap `AnonymousAuthenticationProvider` for a bearer token provider backed on the test user's login cookie/token.

## Type Mapping Note

Kiota generates client models from the OpenAPI JSON Schema. NodaTime values (`LocalDate`, `Instant`, etc.) are serialized as strings by STJ, so the generated models represent those fields as `string?`. This is expected — the client models are wire-level representations, not domain types. Consumers should parse date strings as needed.

## Future: NuGet Publishing

When ready to publish:
1. Set a proper `Version` (or `VersionPrefix`/`VersionSuffix`) in the csproj
2. `dotnet pack src/FantasyCritic.ApiClient/FantasyCritic.ApiClient.csproj -c Release`
3. `dotnet nuget push` to NuGet.org

No structural changes to the project are needed — `IsPackable=true` is set from day one.

## Key Principles

- **Generator is the source of truth.** Never hand-edit generated files.
- **Regeneration is intentional.** Run the script, review the diff, commit. Same mental model as DB migrations.
- **CI stays simple.** Generated files are committed; no Kiota install required in the build pipeline.
- **NuGet-ready from day one.** Publishability is designed in, not bolted on later.
