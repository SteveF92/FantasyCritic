# System.Text.Json Migration Design

**Date:** 2026-05-31  
**Status:** Approved

## Goal

Migrate FantasyCritic from Newtonsoft.Json to `System.Text.Json` (STJ) as the sole JSON serializer. The primary driver is enabling `Microsoft.AspNetCore.OpenApi` for accurate OpenAPI spec generation — that package reads `JsonSerializerOptions` from the same MVC registration, so NodaTime types produce wrong schemas (complex object instead of ISO string) until the MVC pipeline is on STJ.

Downstream goals unlocked after this migration:
- Build-time OpenAPI spec output via `Microsoft.Extensions.ApiDescription.Server`
- Kiota-generated C#/TypeScript clients from the spec
- `ActionResult<T>` controller returns for accurate response schemas

## Current State

The codebase is fully Newtonsoft-based:
- MVC/API: `AddNewtonsoftJson` + `NodaTime.Serialization.JsonNet`
- Zero STJ usage in application code
- No custom `JsonConverter` classes
- Two types use `[JsonConstructor]` (Newtonsoft namespace)
- ~10 `JsonConvert` direct call sites across Web, Lib, LocalDatabaseTool, IntegrationTests
- `JObject` / LINQ-to-JSON manual parsing in `OpenCriticService` and `GGService`
- Verify/Argon in unit tests — uses a Newtonsoft fork, completely independent of MVC serializer

## Approach

Two commits. No behavior change to the API wire format (camelCase naming and NodaTime ISO 8601 format are identical in both serializer ecosystems).

## Commit 1 — Typed External API Models

**Purpose:** Replace `JObject` parsing with proper typed models that STJ can deserialize. Pure addition — Newtonsoft remains untouched. Zero risk.

### OpenCriticService

Add `OpenCriticGameResponse.cs` in `FantasyCritic.Lib/OpenCritic/`:

```csharp
public class OpenCriticGameResponse
{
    public string? Name { get; set; }
    public string? FirstReleaseDate { get; set; }
    public decimal? TopCriticScore { get; set; }
    public decimal? AverageScore { get; set; }
    public int? NumReviews { get; set; }
    public string? Url { get; set; }
}
```

`OpenCriticService.GetOpenCriticGame` switches from `GetStringAsync` + `JObject.Parse` to `GetFromJsonAsync<OpenCriticGameResponse>`. The score fallback logic (`-1` sentinel → `averageScore`) and `LocalDate` conversion remain in the service method unchanged.

### GGService

The GraphQL response is a 3-level envelope. Add two wrapper types alongside the existing `GGGameResponse` in `FantasyCritic.Lib/GG/`:

```
GGGraphQLResponse     { Data: GGGraphQLData? }
GGGraphQLData         { GetGameByToken: GGGameResponse? }   ← [JsonPropertyName("getGameByToken")]
GGGameResponse        (already exists)
```

`GGService.GetGGGame`:
- Request body: `JsonSerializer.Serialize(queryObject)` (no Newtonsoft needed here either)
- Response: deserialize directly to `GGGraphQLResponse`, navigate typed properties — no `JObject` navigation, no `.ToObject<T>()`

**Files changed in Commit 1:**
- `FantasyCritic.Lib/OpenCritic/OpenCriticGameResponse.cs` (new)
- `FantasyCritic.Lib/GG/GGGraphQLResponse.cs` (new, contains `GGGraphQLResponse` and `GGGraphQLData`)
- `FantasyCritic.Lib/OpenCritic/OpenCriticService.cs` (updated)
- `FantasyCritic.Lib/GG/GGService.cs` (updated)

## Commit 2 — Serializer Swap

**Purpose:** Replace `AddNewtonsoftJson` with `AddJsonOptions` (STJ), replace all `JsonConvert` call sites, remove Newtonsoft packages.

### New infrastructure: shared JsonSerializerOptions

Add `FantasyCriticJsonOptions.cs` in `FantasyCritic.Lib/`:

```csharp
public static class FantasyCriticJsonOptions
{
    public static JsonSerializerOptions Create() =>
        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            .ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

    public static JsonSerializerOptions CreateIndented() =>
        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true }
            .ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
}
```

All direct `JsonSerializer` calls use these options. MVC registration uses the same configuration. This keeps the format consistent across the whole app.

### MVC registration

`HostingExtensions.cs`:
```csharp
// Before
.AddNewtonsoftJson(options =>
    options.SerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb))

// After
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
})
```

> **Important:** `AddNewtonsoftJson` applies camelCase by default; `AddJsonOptions` does not — `PropertyNamingPolicy = CamelCase` must be set explicitly or the API output silently switches to PascalCase and breaks the frontend.

### Touch points

| File | Change |
|------|--------|
| `HostingExtensions.cs` | `AddNewtonsoftJson` → `AddJsonOptions` + NodaTime |
| `Models/RoundTrip/LeagueYearSettingsViewModel.cs` | `using Newtonsoft.Json` → `using System.Text.Json.Serialization` on `[JsonConstructor]` |
| `Models/RoundTrip/SpecialGameSlotViewModel.cs` | Same |
| `Controllers/FantasyCriticController.cs` | `JsonConvert` session cache → `JsonSerializer` with `FantasyCriticJsonOptions.Create()` |
| `Identity/FantasyCriticSignInManager.cs` | Same session cache swap |
| `Controllers/API/AccountController.cs` | `JsonConvert` roles session → `JsonSerializer` (no NodaTime options needed for `string[]`) |
| `Controllers/API/LeagueController.cs` (ZIP export) | `ZipExportJsonSettings` → `FantasyCriticJsonOptions.CreateIndented()` |
| `Controllers/API/LeagueController.cs` (base64 slot) | `JsonConvert.DeserializeObject<T>` → `JsonSerializer.Deserialize<T>` |
| `Lib/SharedSerialization/Database/MasterGameTagEntity.cs` | `JsonConvert` for `Examples` DB column → `JsonSerializer` |
| `LocalDatabaseTool/Program.cs` | `JsonConvert.DefaultSettings` global → explicit options on each call site |
| `IntegrationTests/Helpers/ApiSession.cs` | `JsonConvert` → `JsonSerializer` |
| `IntegrationTests/ProductionStats/ProductionGameStatsCache.cs` | Same |

### Package changes

| Project | Remove | Add |
|---------|--------|-----|
| `FantasyCritic.Lib` | `Newtonsoft.Json`, `NodaTime.Serialization.JsonNet` | `NodaTime.Serialization.SystemTextJson` |
| `FantasyCritic.Web` | `Microsoft.AspNetCore.Mvc.NewtonsoftJson`, `NodaTime.Serialization.JsonNet` | — |
| `FantasyCritic.LocalDatabaseTool` | `NodaTime.Serialization.JsonNet` | — |
| `FantasyCritic.IntegrationTests` | `Newtonsoft.Json` | — |

Package removal is the verification step — any missed `JsonConvert` / `JObject` / Newtonsoft using directive becomes a compile error.

### Architecture rule update

`fantasy-critic-architecture.mdc` documents Newtonsoft as the JSON serializer. Update to reflect STJ + `NodaTime.Serialization.SystemTextJson`.

## What Does Not Change

- **Verify/Argon** (`FantasyCritic.Test`): Uses Argon (Newtonsoft fork) for snapshot serialization. Completely independent of the MVC serializer — untouched.
- **API wire format**: Newtonsoft MVC and STJ MVC both produce camelCase. NodaTime ISO 8601 format is identical across both packages. Frontend requires no changes.
- **Dapper/NodaTime DB mappings**: `DapperNodaTimeSetup.SetupDapperNodaTimeMappings()` is database-level, not JSON — untouched.
- **SignalR**: Sends only method name strings, no complex payloads — unaffected.

## Risk Factors

- **Case sensitivity**: STJ deserialization is case-sensitive by default. The MVC integration sets `PropertyNameCaseInsensitive = true` automatically, so request body binding is fine. Direct `JsonSerializer.Deserialize` calls for internal data (session cache, DB column) use data we wrote ourselves — consistent casing is guaranteed.
- **Session invalidation**: Existing user sessions store Newtonsoft-serialized JSON. Switching the serializer will make in-flight sessions unreadable at deployment time, causing a one-time forced re-login for any active users. Acceptable for this project.
- **`-1` sentinel in OpenCritic scores**: The current `JObject` parsing handles the `-1 = no score` sentinel explicitly. The typed model preserves this — `TopCriticScore` and `AverageScore` come in as `decimal?` and the existing service logic handles the sentinel check identically.
