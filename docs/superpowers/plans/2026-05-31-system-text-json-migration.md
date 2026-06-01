# System.Text.Json Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace Newtonsoft.Json with System.Text.Json as the sole serializer so that `Microsoft.AspNetCore.OpenApi` generates accurate schemas for NodaTime types.

**Architecture:** Two logical commits: first add typed response models to replace `JObject` parsing in the two external-API services (no Newtonsoft changes yet), then perform the full serializer swap across MVC, session caching, and entity serialization, and remove Newtonsoft packages.

**Tech Stack:** .NET 10, ASP.NET Core MVC, System.Text.Json, NodaTime 3.x, NodaTime.Serialization.SystemTextJson

---

## File Map

### Commit 1 — Typed external-API models
| Action | Path |
|--------|------|
| Create | `src/FantasyCritic.Lib/OpenCritic/OpenCriticGameResponse.cs` |
| Modify | `src/FantasyCritic.Lib/OpenCritic/OpenCriticService.cs` |
| Create | `src/FantasyCritic.Lib/GG/GGGraphQLEnvelope.cs` |
| Modify | `src/FantasyCritic.Lib/GG/GGService.cs` |

### Commit 2 — Serializer swap
| Action | Path |
|--------|------|
| Create | `src/FantasyCritic.Lib/FantasyCriticJsonOptions.cs` |
| Modify | `src/FantasyCritic.Web/HostingExtensions.cs` |
| Modify | `src/FantasyCritic.Web/Models/RoundTrip/LeagueYearSettingsViewModel.cs` |
| Modify | `src/FantasyCritic.Web/Models/RoundTrip/SpecialGameSlotViewModel.cs` |
| Modify | `src/FantasyCritic.Web/Controllers/FantasyCriticController.cs` |
| Modify | `src/FantasyCritic.Lib/Identity/FantasyCriticSignInManager.cs` |
| Modify | `src/FantasyCritic.Web/Controllers/API/AccountController.cs` |
| Modify | `src/FantasyCritic.Web/Controllers/API/LeagueController.cs` |
| Modify | `src/FantasyCritic.Lib/SharedSerialization/Database/MasterGameTagEntity.cs` |
| Modify | `src/FantasyCritic.Lib/FantasyCritic.Lib.csproj` |
| Modify | `src/FantasyCritic.Web/FantasyCritic.Web.csproj` |
| Modify | `.cursor/rules/fantasy-critic-architecture.mdc` |

---

## Task 1: Add OpenCritic typed response model

**Files:**
- Create: `src/FantasyCritic.Lib/OpenCritic/OpenCriticGameResponse.cs`

- [ ] **Step 1: Create the typed response model**

```csharp
namespace FantasyCritic.Lib.OpenCritic;

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

Write this as `src/FantasyCritic.Lib/OpenCritic/OpenCriticGameResponse.cs`.

---

## Task 2: Update OpenCriticService to use typed model

**Files:**
- Modify: `src/FantasyCritic.Lib/OpenCritic/OpenCriticService.cs`

- [ ] **Step 1: Replace JObject parsing with typed deserialization**

Replace the entire file content with:

```csharp
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using FantasyCritic.Lib.Extensions;

namespace FantasyCritic.Lib.OpenCritic;

public class OpenCriticService : IOpenCriticService
{
    private readonly HttpClient _client;
    private readonly ILogger<OpenCriticService> _logger;
    private readonly LocalDate DefaultOpenCriticReleaseDate = new LocalDate(2020, 12, 31);

    public OpenCriticService(HttpClient client, ILogger<OpenCriticService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<OpenCriticGame?> GetOpenCriticGame(int openCriticGameID)
    {
        try
        {
            var response = await _client.GetFromJsonAsync<OpenCriticGameResponse>($"game/{openCriticGameID}");
            if (response is null)
            {
                return null;
            }

            var gameName = response.Name ?? "Unknown Open Critic Game";

            LocalDate? earliestReleaseDate = null;
            if (!string.IsNullOrWhiteSpace(response.FirstReleaseDate) &&
                DateTime.TryParse(response.FirstReleaseDate, out var parsedDate))
            {
                earliestReleaseDate = LocalDate.FromDateTime(parsedDate);
                if (earliestReleaseDate == DefaultOpenCriticReleaseDate)
                {
                    earliestReleaseDate = null;
                }
            }

            var score = response.TopCriticScore;
            if (score == -1m)
            {
                score = response.AverageScore;
                if (score != -1m)
                {
                    _logger.LogInformation($"Using averageScore for game: {openCriticGameID}");
                }
                else
                {
                    score = null;
                }
            }

            bool hasAnyReviews = response.NumReviews.HasValue && response.NumReviews > 0;
            string? slug = response.Url?.SubstringStartingFromLastInstanceOf("/");

            return new OpenCriticGame(openCriticGameID, gameName, score, earliestReleaseDate, hasAnyReviews, slug);
        }
        catch (HttpRequestException httpEx)
        {
            if (httpEx.Message == "Response status code does not indicate success: 404 (Not Found).")
            {
                return null;
            }
            _logger.LogError(httpEx, $"Getting an open critic game failed: {openCriticGameID}");
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Getting an open critic game failed: {openCriticGameID}");
            throw;
        }
    }
}
```

---

## Task 3: Add GG GraphQL envelope types

**Files:**
- Create: `src/FantasyCritic.Lib/GG/GGGraphQLEnvelope.cs`

- [ ] **Step 1: Create the envelope types**

The GG API responds with a 3-level GraphQL envelope wrapping the actual game data. Create `src/FantasyCritic.Lib/GG/GGGraphQLEnvelope.cs`:

```csharp
using System.Text.Json.Serialization;

namespace FantasyCritic.Lib.GG;

public class GGGraphQLResponse
{
    public GGGraphQLData? Data { get; set; }
}

public class GGGraphQLData
{
    [JsonPropertyName("getGameByToken")]
    public GGGameResponse? GetGameByToken { get; set; }
}
```

Note: `GGGameResponse` already exists in `src/FantasyCritic.Lib/GG/GGGameResponse.cs` — do not modify it.

---

## Task 4: Update GGService to use typed models

**Files:**
- Modify: `src/FantasyCritic.Lib/GG/GGService.cs`

- [ ] **Step 1: Replace JObject parsing with typed deserialization**

Replace the entire file content with:

```csharp
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.GG;

public class GGService : IGGService
{
    private readonly HttpClient _client;
    private readonly ILogger<GGService> _logger;

    public GGService(HttpClient client, ILogger<GGService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<GGGame?> GetGGGame(string ggToken)
    {
        try
        {
            const string query = @"query getGameByToken($token: String!) {
                                    getGameByToken(token: $token) {
                                        id
                                        name
                                        coverPath
                                        token
                                        slug
                                    }
                                }";
            var queryObject = new
            {
                query,
                variables = new { token = ggToken },
                operationName = "getGameByToken"
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(JsonSerializer.Serialize(queryObject), Encoding.UTF8, "application/json")
            };

            string responseString;
            using (var response = await _client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                responseString = await response.Content.ReadAsStringAsync();
            }

            var parsed = JsonSerializer.Deserialize<GGGraphQLResponse>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var typedData = parsed?.Data?.GetGameByToken;
            if (typedData is null)
            {
                return null;
            }

            string? coverPath = null;
            if (!string.IsNullOrWhiteSpace(typedData.CoverPath))
            {
                var split = typedData.CoverPath.Split("/");
                coverPath = split.Last();
            }

            if (typedData.Token is null || typedData.Slug is null)
            {
                return null;
            }

            return new GGGame(typedData.Token, typedData.Slug, coverPath);
        }
        catch (HttpRequestException httpEx)
        {
            if (httpEx.Message == "Response status code does not indicate success: 404 (Not Found).")
            {
                return null;
            }
            _logger.LogError(httpEx, $"Getting an GG| game failed: {ggToken}");
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Getting an GG| game failed: {ggToken}");
            throw;
        }
    }
}
```

---

## Task 5: Build and commit Commit 1

**Files:** (none new)

- [ ] **Step 1: Build the solution**

```
dotnet build src/FantasyCritic.slnx
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 2: Commit**

```
git add src/FantasyCritic.Lib/OpenCritic/OpenCriticGameResponse.cs
git add src/FantasyCritic.Lib/OpenCritic/OpenCriticService.cs
git add src/FantasyCritic.Lib/GG/GGGraphQLEnvelope.cs
git add src/FantasyCritic.Lib/GG/GGService.cs
git commit -m "Replace JObject parsing in OpenCritic/GG services with typed STJ models."
```

---

## Task 6: Add shared JsonSerializerOptions

**Files:**
- Create: `src/FantasyCritic.Lib/FantasyCriticJsonOptions.cs`

This new class provides pre-configured, cached `JsonSerializerOptions` instances used across all direct `JsonSerializer` call sites and by the MVC registration. `JsonSerializerOptions` should be created once and reused — creating it per-call is expensive.

- [ ] **Step 1: Create the options class**

```csharp
using System.Text.Json;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;

namespace FantasyCritic.Lib;

public static class FantasyCriticJsonOptions
{
    public static readonly JsonSerializerOptions Default = CreateDefault();
    public static readonly JsonSerializerOptions Indented = CreateIndented();

    private static JsonSerializerOptions CreateDefault() =>
        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            .ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

    private static JsonSerializerOptions CreateIndented() =>
        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true }
            .ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
}
```

Write this to `src/FantasyCritic.Lib/FantasyCriticJsonOptions.cs`.

This file will not compile until Task 13 adds the `NodaTime.Serialization.SystemTextJson` package to `FantasyCritic.Lib.csproj`. That is expected — complete all Tasks 6–12 before building in Task 14.

---

## Task 7: Swap MVC registration in HostingExtensions

**Files:**
- Modify: `src/FantasyCritic.Web/HostingExtensions.cs`

- [ ] **Step 1: Replace the using directive**

In `HostingExtensions.cs`, replace:
```csharp
using NodaTime.Serialization.JsonNet;
```
with:
```csharp
using System.Text.Json;
using NodaTime.Serialization.SystemTextJson;
```

- [ ] **Step 2: Swap AddNewtonsoftJson with AddJsonOptions**

Find the block (around line 303):
```csharp
services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
    })
    .AddControllersAsServices();
```

Replace with:
```csharp
services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
    })
    .AddControllersAsServices();
```

---

## Task 8: Fix [JsonConstructor] attribute namespaces

**Files:**
- Modify: `src/FantasyCritic.Web/Models/RoundTrip/LeagueYearSettingsViewModel.cs`
- Modify: `src/FantasyCritic.Web/Models/RoundTrip/SpecialGameSlotViewModel.cs`

Both files use `[JsonConstructor]` from `Newtonsoft.Json`. The attribute has the same name in `System.Text.Json.Serialization` — only the `using` directive changes.

- [ ] **Step 1: Update LeagueYearSettingsViewModel.cs**

Replace:
```csharp
using Newtonsoft.Json;
```
with:
```csharp
using System.Text.Json.Serialization;
```

- [ ] **Step 2: Update SpecialGameSlotViewModel.cs**

Replace:
```csharp
using Newtonsoft.Json;
```
with:
```csharp
using System.Text.Json.Serialization;
```

---

## Task 9: Swap session JSON in FantasyCriticController and SignInManager

**Files:**
- Modify: `src/FantasyCritic.Web/Controllers/FantasyCriticController.cs`
- Modify: `src/FantasyCritic.Lib/Identity/FantasyCriticSignInManager.cs`

Both files serialize/deserialize `FantasyCriticUserEntity` to the session store. `FantasyCriticUserEntity` contains a `LastChangedCredentials` field of type `Instant` (NodaTime), so NodaTime-aware options are required.

- [ ] **Step 1: Update FantasyCriticController.cs usings**

Replace:
```csharp
using Newtonsoft.Json;
using NodaTime.Serialization.JsonNet;
```
with:
```csharp
using System.Text.Json;
```

(`FantasyCriticJsonOptions` is in `FantasyCritic.Lib` which is already referenced by `FantasyCritic.Web`, so no new using is needed if you use the fully-qualified-friendly namespace — but adding `using FantasyCritic.Lib;` is cleaner.)

Add:
```csharp
using FantasyCritic.Lib;
```

- [ ] **Step 2: Swap deserialize call in FantasyCriticController.cs**

Replace (around line 43):
```csharp
var deserialized = JsonConvert.DeserializeObject<FantasyCriticUserEntity>(sessionUserString, new JsonSerializerSettings().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));
```
with:
```csharp
var deserialized = JsonSerializer.Deserialize<FantasyCriticUserEntity>(sessionUserString, FantasyCriticJsonOptions.Default);
```

- [ ] **Step 3: Swap serialize call in FantasyCriticController.cs**

Replace (around line 58):
```csharp
var jsonString = JsonConvert.SerializeObject(serializable, new JsonSerializerSettings().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));
```
with:
```csharp
var jsonString = JsonSerializer.Serialize(serializable, FantasyCriticJsonOptions.Default);
```

- [ ] **Step 4: Update FantasyCriticSignInManager.cs usings**

Replace:
```csharp
using Newtonsoft.Json;
using NodaTime.Serialization.JsonNet;
```
with:
```csharp
using System.Text.Json;
using FantasyCritic.Lib;
```

- [ ] **Step 5: Swap serialize call in FantasyCriticSignInManager.cs**

Replace (around line 60 in `CacheUserToSession`):
```csharp
var jsonString = JsonConvert.SerializeObject(serializable, new JsonSerializerSettings().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));
```
with:
```csharp
var jsonString = JsonSerializer.Serialize(serializable, FantasyCriticJsonOptions.Default);
```

---

## Task 10: Swap session JSON in AccountController

**Files:**
- Modify: `src/FantasyCritic.Web/Controllers/API/AccountController.cs`

This file serializes/deserializes `string[]` to the session store — no NodaTime types involved, so default STJ options suffice.

- [ ] **Step 1: Update usings**

Replace:
```csharp
using Newtonsoft.Json;
```
with:
```csharp
using System.Text.Json;
```

- [ ] **Step 2: Swap deserialize call**

Replace (around line 33):
```csharp
userRoles = JsonConvert.DeserializeObject<string[]>(sessionUserRoles)!;
```
with:
```csharp
userRoles = JsonSerializer.Deserialize<string[]>(sessionUserRoles)!;
```

- [ ] **Step 3: Swap serialize call**

Replace (around line 38):
```csharp
var jsonString = JsonConvert.SerializeObject(userRoles);
```
with:
```csharp
var jsonString = JsonSerializer.Serialize(userRoles);
```

---

## Task 11: Swap JSON in LeagueController

**Files:**
- Modify: `src/FantasyCritic.Web/Controllers/API/LeagueController.cs`

This file has three `JsonConvert` uses: ZIP export (serializing large view models with NodaTime types, indented formatting), base64-encoded slot info (deserialize), and a private zip-entry helper (serialize with same indented settings).

- [ ] **Step 1: Update usings**

Replace:
```csharp
using Newtonsoft.Json;
using NodaTime.Serialization.JsonNet;
```
with:
```csharp
using System.Text.Json;
using FantasyCritic.Lib;
```

- [ ] **Step 2: Replace static settings field and factory method**

Replace:
```csharp
private static readonly JsonSerializerSettings ZipExportJsonSettings = CreateZipExportJsonSettings();

private static JsonSerializerSettings CreateZipExportJsonSettings()
{
    JsonSerializerSettings settings = new JsonSerializerSettings { Formatting = Formatting.Indented };
    return settings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
}
```
with:
```csharp
private static readonly JsonSerializerOptions ZipExportJsonOptions = FantasyCriticJsonOptions.Indented;
```

- [ ] **Step 3: Swap ZIP export serialize call**

Replace (around line 546):
```csharp
string json = JsonConvert.SerializeObject(yearViewModel, ZipExportJsonSettings);
```
with:
```csharp
string json = JsonSerializer.Serialize(yearViewModel, ZipExportJsonOptions);
```

- [ ] **Step 4: Swap base64 slot deserialize call**

Replace (around line 1159):
```csharp
PublisherSingleSlotRequirementsViewModel? slotInfoObject = JsonConvert.DeserializeObject<PublisherSingleSlotRequirementsViewModel>(slotInfoJSON);
```
with:
```csharp
PublisherSingleSlotRequirementsViewModel? slotInfoObject = JsonSerializer.Deserialize<PublisherSingleSlotRequirementsViewModel>(slotInfoJSON);
```

- [ ] **Step 5: Swap zip-entry helper serialize call**

Replace (around line 1734 in `AddConsolidatedExportZipEntry`):
```csharp
string json = JsonConvert.SerializeObject(payload, ZipExportJsonSettings);
```
with:
```csharp
string json = JsonSerializer.Serialize(payload, ZipExportJsonOptions);
```

---

## Task 12: Swap JSON in MasterGameTagEntity

**Files:**
- Modify: `src/FantasyCritic.Lib/SharedSerialization/Database/MasterGameTagEntity.cs`

This entity stores the `Examples` field as a JSON string in the database column. The serialized format for `List<string>` is identical in Newtonsoft and STJ (`["a","b"]`), so existing database rows are unaffected.

- [ ] **Step 1: Update usings**

Replace:
```csharp
using Newtonsoft.Json;
```
with:
```csharp
using System.Text.Json;
```

- [ ] **Step 2: Swap serialize in constructor**

Replace (around line 21):
```csharp
Examples = JsonConvert.SerializeObject(domain.Examples);
```
with:
```csharp
Examples = JsonSerializer.Serialize(domain.Examples);
```

- [ ] **Step 3: Swap deserialize in ToDomain**

Replace (around line 37):
```csharp
var examples = JsonConvert.DeserializeObject<List<string>>(Examples)!;
```
with:
```csharp
var examples = JsonSerializer.Deserialize<List<string>>(Examples)!;
```

---

## Task 13: Update package references and remove Newtonsoft

**Files:**
- Modify: `src/FantasyCritic.Lib/FantasyCritic.Lib.csproj`
- Modify: `src/FantasyCritic.Web/FantasyCritic.Web.csproj`

- [ ] **Step 1: Update FantasyCritic.Lib.csproj**

Remove these two `<PackageReference>` lines:
```xml
<PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
<PackageReference Include="NodaTime.Serialization.JsonNet" Version="3.2.1" />
```

Add this line in their place:
```xml
<PackageReference Include="NodaTime.Serialization.SystemTextJson" Version="1.3.0" />
```

> Check [NuGet](https://www.nuget.org/packages/NodaTime.Serialization.SystemTextJson) for the latest stable version and use that instead of `1.3.0` if a newer one is available.

- [ ] **Step 2: Update FantasyCritic.Web.csproj**

Remove these two `<PackageReference>` lines:
```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc.NewtonsoftJson" Version="10.0.5" />
<PackageReference Include="NodaTime.Serialization.JsonNet" Version="3.2.1" />
```

No new packages needed in Web — it references Lib, which now carries `NodaTime.Serialization.SystemTextJson`.

- [ ] **Step 3: Restore packages**

```
dotnet restore src/FantasyCritic.slnx
```

Expected: Restore succeeded, all packages resolved.

---

## Task 14: Build to verify and commit Commit 2

**Files:** (none new)

- [ ] **Step 1: Build the solution**

```
dotnet build src/FantasyCritic.slnx
```

Expected: Build succeeded, 0 errors. If there are any remaining `JsonConvert`, `JObject`, `using Newtonsoft.Json`, or `using NodaTime.Serialization.JsonNet` references, they will appear as compile errors here — fix them before committing.

- [ ] **Step 2: Run unit tests**

```
dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj
```

Expected: All tests pass. (The `FantasyCritic.Test` project uses Verify/Argon for snapshots — these are unaffected by the MVC serializer change.)

- [ ] **Step 3: Commit**

```
git add src/FantasyCritic.Lib/FantasyCriticJsonOptions.cs
git add src/FantasyCritic.Lib/FantasyCritic.Lib.csproj
git add src/FantasyCritic.Lib/SharedSerialization/Database/MasterGameTagEntity.cs
git add src/FantasyCritic.Lib/Identity/FantasyCriticSignInManager.cs
git add src/FantasyCritic.Web/FantasyCritic.Web.csproj
git add src/FantasyCritic.Web/HostingExtensions.cs
git add src/FantasyCritic.Web/Models/RoundTrip/LeagueYearSettingsViewModel.cs
git add src/FantasyCritic.Web/Models/RoundTrip/SpecialGameSlotViewModel.cs
git add src/FantasyCritic.Web/Controllers/FantasyCriticController.cs
git add src/FantasyCritic.Web/Controllers/API/AccountController.cs
git add src/FantasyCritic.Web/Controllers/API/LeagueController.cs
git commit -m "Migrate from Newtonsoft.Json to System.Text.Json."
```

---

## Task 15: Update architecture rule

**Files:**
- Modify: `.cursor/rules/fantasy-critic-architecture.mdc`

- [ ] **Step 1: Update the ASP.NET JSON section**

Find the section that reads:
```
## ASP.NET JSON

- **FantasyCritic.Web** uses **Newtonsoft.Json** for MVC/API (`Microsoft.AspNetCore.Mvc.NewtonsoftJson`, **NodaTime.Serialization.JsonNet**). Match Newtonsoft attributes and serializers when changing API contracts—do not assume `System.Text.Json` is the default for controllers.
```

Replace with:
```
## ASP.NET JSON

- **FantasyCritic.Web** uses **System.Text.Json** for MVC/API (configured via `AddJsonOptions` with **NodaTime.Serialization.SystemTextJson**). Use `System.Text.Json.Serialization` attributes (`[JsonPropertyName]`, `[JsonConstructor]`, etc.) when changing API contracts. The shared `FantasyCriticJsonOptions` class in `FantasyCritic.Lib` provides pre-configured `JsonSerializerOptions` instances (camelCase + NodaTime) for all direct `JsonSerializer` call sites.
```

- [ ] **Step 2: Commit**

```
git add .cursor/rules/fantasy-critic-architecture.mdc
git commit -m "Update architecture rule: document System.Text.Json as the JSON serializer."
```
