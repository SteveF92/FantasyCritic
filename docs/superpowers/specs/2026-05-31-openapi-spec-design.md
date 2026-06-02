# OpenAPI Spec Design

**Date:** 2026-05-31  
**Status:** Approved

## Goals

1. The entire OpenAPI spec is auto-generated from the C# source — never edited manually. The code is the source of truth.
2. As much as possible, lean on actual compile-time types. Strengthening controller return types is an explicit part of the work.
3. The spec will eventually be used to auto-generate a C# client for integration tests and a TypeScript client for the frontend. Those client generation steps are future work; this design sets up the foundation.

## Tooling Choice

**Microsoft.AspNetCore.OpenApi** (official ASP.NET Core package, introduced in .NET 9, Microsoft's strategic replacement for Swashbuckle) for spec generation.

**Microsoft.Extensions.ApiDescription.Server** for build-time JSON file output via MSBuild — no server process required.

**Microsoft Kiota** (future, Phase 3/4) for client generation from the spec — the official Microsoft API client generator, used by the Graph SDK. Supports both C# and TypeScript targets.

### Why not Swashbuckle?

Microsoft explicitly removed Swashbuckle from their project templates in .NET 9 in favor of `Microsoft.AspNetCore.OpenApi`. The project prefers officially supported tooling.

### Why not NSwag?

Third-party. Both spec generation and client generation are covered by officially supported Microsoft tooling.

### Spec format: JSON

All generators emit JSON natively. YAML adds parsing edge cases and a conversion step with no benefit for this use case.

## Prerequisite: STJ Migration

`Microsoft.AspNetCore.OpenApi` generates schemas using System.Text.Json's view of your types. The project migrated from Newtonsoft.Json to System.Text.Json (including `NodaTime.Serialization.SystemTextJson`) prior to this work. The `JsonSerializerOptions` configured in `AddControllers().AddJsonOptions(...)` — including camelCase policy and NodaTime support — are read automatically by the OpenAPI package.

## Phasing

### Phase 1 — Strengthen Controller Return Types (no new packages)

All work is purely within `FantasyCritic.Web/Controllers/API/`. No NuGet packages are added in this phase.

#### Return type conventions

**Rule 1: `ActionResult<T>` for any action with a success body.**  
Any action whose happy path returns `Ok(someModel)` is changed to `Task<ActionResult<T>>` where `T` is the named ViewModel type. The `FailedResult` passthrough pattern used by `BaseLeagueController` is not a blocker: `ActionResult<T>` has an implicit conversion from `IActionResult`, so `return record.FailedResult;` compiles as-is inside a `Task<ActionResult<T>>` method.

**Rule 2: `IActionResult` for command actions with no success body.**  
Actions that only return `Ok()` with no body (plus possibly error codes) keep `IActionResult`. A `T` cannot be expressed. Add `[ProducesResponseType(StatusCodes.Status200OK)]` so the spec documents the 200 explicitly.

**Rule 3: `[ProducesResponseType]` for every non-success status code.**  
For any action that can return `NotFound()`, `BadRequest(...)`, `Forbid()`, `Unauthorized()`, or `StatusCode(403)`, add the corresponding attribute. The generator infers only the typed success response from the return type signature; all other status codes require explicit annotation.

```csharp
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
```

**Rule 4: Explicit HTTP verb attribute on every action.**  
Every action gets `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, or `[HttpDelete]`. Currently many read actions have no verb attribute; ASP.NET accepts any verb for them. The OpenAPI generator requires an explicit verb to produce a valid spec, and explicit verbs are a correctness improvement regardless.

**Rule 5: No anonymous types in `Ok(new { … })`.**  
The generator cannot produce a schema for an anonymous type. All anonymous response projections require a named ViewModel class. New classes follow the existing pattern: `src/FantasyCritic.Web/Models/Responses/` with appropriate subfolders (`Royale/`, `Conferences/`, etc.). Identified occurrences:
- `CombinedDataController.BasicData` → new `BasicDataViewModel`
- `CombinedDataController.HomePageData` → new `HomePageDataViewModel`
- `RoyaleController.RoyaleData` → new `RoyaleQuarterDataViewModel` (inside `Royale/`)
- `RoyaleController.UserRoyaleHistory` → new `UserRoyaleHistoryViewModel` (inside `Royale/`)
- `RoyaleGroupController` create-response (`new { GroupID }`) → new `CreatedRoyaleGroupViewModel` (inside `Royale/`)

**Rule 6: File and content actions keep `IActionResult` with `[Produces]`.**  
`FileStreamResult`, `File(bytes, contentType, fileName)`, and `Content(xml, "application/rss+xml")` have no typed response body. They keep `IActionResult` and gain `[Produces("text/csv")]` / `[Produces("application/zip")]` / `[Produces("application/rss+xml")]` to document the media type in the spec.

**Rule 7: `CreatedAtAction` responses use `ActionResult<T>` with a 201 annotation.**  
Actions in `FactCheckerController` that return `CreatedAtAction(...)` with a model body become `Task<ActionResult<T>>` and add `[ProducesResponseType(typeof(T), StatusCodes.Status201Created)]`.

#### Scope

| Category | Approx. count | Action |
|---|---|---|
| `IActionResult` → `ActionResult<T>` (has body) | ~140 | Convert return type |
| `IActionResult` stays (command, no body) | ~48 | Add `[ProducesResponseType(200)]` |
| Anonymous `Ok(new { })` → named DTO | ~6 | New ViewModel class, update action |
| Missing HTTP verb attributes | ~60 actions | Add `[HttpGet]`/`[HttpPost]` |
| File/RSS download actions | ~4 | Add `[Produces(...)]`, keep `IActionResult` |
| Already `ActionResult<T>` — consistency cleanup | ~52 | Spot-check, add missing `[ProducesResponseType]` |

#### What Phase 1 does NOT include

- No new NuGet packages
- No XML doc comments (deferred — the spec generates without them)
- No changes to integration tests or any other project

---

### Phase 2 — Add OpenAPI Packages and Build-Time Output

#### NuGet packages (added to `FantasyCritic.Web.csproj`)

```xml
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="..." />
<PackageReference Include="Microsoft.Extensions.ApiDescription.Server" Version="..." />
```

Use `dotnet add package Microsoft.AspNetCore.OpenApi` and `dotnet add package Microsoft.Extensions.ApiDescription.Server` at implementation time to resolve the latest stable version compatible with `net10.0`. Both packages are part of the ASP.NET Core platform and are versioned together.

#### Service registration (`HostingExtensions.ConfigureServices`)

```csharp
builder.Services.AddOpenApi();
```

`AddOpenApi()` automatically reads the `JsonSerializerOptions` from `AddControllers().AddJsonOptions(...)` — no additional schema configuration is needed for NodaTime types or camelCase.

#### Middleware (`HostingExtensions.ConfigurePipeline`)

Only map the OpenAPI endpoint in development — there is no reason to expose the spec in production:

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // serves at /openapi/v1.json
}
```

#### Build-time spec output (`FantasyCritic.Web.csproj`)

```xml
<PropertyGroup>
  <OpenApiGenerateDocuments>true</OpenApiGenerateDocuments>
  <OpenApiDocumentsDirectory>$(MSBuildThisFileDirectory)../../openapi</OpenApiDocumentsDirectory>
</PropertyGroup>
```

`dotnet build` writes `openapi/openapi.json` to the repo root. The file is committed to the repository so the API contract is:
- Diffable in git (contract changes are visible in PRs)
- Usable by client generators without a running server
- Validatable in CI (build and check the file is unchanged / valid)

#### XML doc comments (deferred)

Enabling `<GenerateDocumentationFile>true</GenerateDocumentationFile>` and wiring the XML file to OpenAPI via a document transformer would populate operation summaries and descriptions in the spec. This is useful for client consumers but not required for a functional spec. Deferred to a follow-up.

---

### Phase 3 — C# Client Generation via Kiota (Future)

When integration tests need a typed HTTP client:

```bash
dotnet tool install microsoft.openapi.kiota --global
kiota generate -l CSharp -d openapi/openapi.json -o src/FantasyCritic.IntegrationTests/Generated -n FantasyCritic.IntegrationTests.Client
```

The generated client is a typed `HttpClient`-based SDK. Regeneration is a build/CI step triggered by changes to `openapi/openapi.json`.

---

### Phase 4 — TypeScript Client Generation via Kiota (Future)

When the Vue frontend is ready for typed API client generation:

```bash
kiota generate -l TypeScript -d openapi/openapi.json -o src/FantasyCritic.Web/ClientApp/src/generated
```

The approach (Kiota vs `openapi-typescript` vs another generator) can be reassessed at that time based on how the TypeScript output fits the frontend's fetch/composable patterns.

---

## Key Design Principles

- **Code is the source of truth.** The spec is derived output, never edited manually.
- **No manual schema overrides.** If the spec is wrong, the C# types or attributes are wrong — fix those instead.
- **Compile-time guarantees first.** `ActionResult<T>` over `IActionResult` everywhere a body type can be expressed.
- **Phases are independently shippable.** Phase 1 has value on its own (type safety improvements). Phase 2 has value on its own (spec as CI artifact). Client generation layers on top without requiring further server-side changes.
