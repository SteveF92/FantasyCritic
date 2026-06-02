# C# API Client Autogen Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create `FantasyCritic.ApiClient`, a NuGet-ready C# project containing a Kiota-generated typed HTTP client, and wire it into the integration test project.

**Architecture:** Kiota is installed as a pinned local .NET tool. `kiota generate` runs manually via `scripts/Regenerate-ApiClient.ps1` and outputs into `src/FantasyCritic.ApiClient/Generated/`. Generated files are committed so CI never needs Kiota installed. `FantasyCritic.IntegrationTests` references the new project.

**Tech Stack:** Microsoft Kiota (`microsoft.openapi.kiota`), Kiota runtime NuGet packages, .NET local tools manifest (`.config/dotnet-tools.json`), PowerShell

---

## File Map

| Action | Path |
|--------|------|
| Create | `.config/dotnet-tools.json` |
| Create | `src/FantasyCritic.ApiClient/FantasyCritic.ApiClient.csproj` |
| Create | `src/FantasyCritic.ApiClient/Generated/` ← kiota output |
| Create | `scripts/Regenerate-ApiClient.ps1` |
| Modify | `src/FantasyCritic.slnx` |
| Modify | `src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj` |

---

### Task 1: Bootstrap the .NET local tool manifest and install Kiota

**Files:**
- Create: `.config/dotnet-tools.json`

- [ ] **Step 1: Create the tools manifest**

Run from the repo root (`I:/CodeProjects/FantasyCritic`):

```powershell
dotnet new tool-manifest
```

Expected output:
```
The template "Dotnet local tool manifest file" was created successfully.
```

This creates `.config/dotnet-tools.json` with an empty tools section.

- [ ] **Step 2: Install Kiota as a local tool**

```powershell
dotnet tool install --local microsoft.openapi.kiota
```

Expected output (version number will vary — this pins whatever is current):
```
You can invoke the tool from this directory using the following commands: 'dotnet tool run kiota' or 'dotnet kiota'.
Tool 'microsoft.openapi.kiota' (version 'X.X.X') was successfully installed.
```

The `.config/dotnet-tools.json` file now looks like:

```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "microsoft.openapi.kiota": {
      "version": "<resolved-at-install-time>",
      "commands": [
        "kiota"
      ]
    }
  }
}
```

- [ ] **Step 3: Verify the tool runs**

```powershell
dotnet tool run kiota -- --version
```

Expected: prints the Kiota version number and exits 0.

- [ ] **Step 4: Commit**

```powershell
git add .config/dotnet-tools.json
git commit -m "Add Kiota as a local dotnet tool."
```

---

### Task 2: Create the `FantasyCritic.ApiClient` project

**Files:**
- Create: `src/FantasyCritic.ApiClient/FantasyCritic.ApiClient.csproj`
- Modify: `src/FantasyCritic.slnx`

- [ ] **Step 1: Create the project directory and `.csproj`**

Create `src/FantasyCritic.ApiClient/FantasyCritic.ApiClient.csproj` with this exact content:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <RootNamespace>FantasyCritic.ApiClient</RootNamespace>

    <!-- NuGet publishing metadata -->
    <IsPackable>true</IsPackable>
    <PackageId>FantasyCritic.ApiClient</PackageId>
    <Authors>Steve Fallon</Authors>
    <Company>Fantasy Critic</Company>
    <Description>C# client for the FantasyCritic API, auto-generated from the OpenAPI spec.</Description>
    <PackageTags>fantasyCritic;api;client;openapi;kiota</PackageTags>
    <RepositoryUrl>https://github.com/SteveF92/FantasyCritic</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
  </PropertyGroup>

  <!-- Generated code produces warnings we don't own; suppress rather than fail the build. -->
  <PropertyGroup>
    <WarningLevel>5</WarningLevel>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
  </PropertyGroup>

</Project>
```

- [ ] **Step 2: Add Kiota runtime package references**

Run from `src/FantasyCritic.ApiClient/`:

```powershell
dotnet add package Microsoft.Kiota.Abstractions
dotnet add package Microsoft.Kiota.Http.HttpClientLibrary
dotnet add package Microsoft.Kiota.Serialization.Json
dotnet add package Microsoft.Kiota.Serialization.Text
dotnet add package Microsoft.Kiota.Serialization.Form
dotnet add package Microsoft.Kiota.Serialization.Multipart
```

Each command resolves the latest version compatible with `net10.0` and adds a `<PackageReference>` to the `.csproj` automatically. All six Kiota packages are versioned together and will resolve to the same major version.

- [ ] **Step 3: Add the project to the solution**

Open `src/FantasyCritic.slnx` and add `FantasyCritic.ApiClient` as a top-level project alongside `FantasyCritic.Web`, `FantasyCritic.Lib`, and `FantasyCritic.MySQL`. The final file should look like:

```xml
<Solution>
  <Folder Name="/ExternalServices/">
    <Project Path="FantasyCritic.AWS/FantasyCritic.AWS.csproj" />
    <Project Path="FantasyCritic.Postmark/FantasyCritic.Postmark.csproj" />
  </Folder>
  <Folder Name="/Solution Items/">
    <File Path=".editorconfig" />
  </Folder>
  <Folder Name="/Tests/">
    <Project Path="FantasyCritic.FakeRepo/FantasyCritic.FakeRepo.csproj" />
    <Project Path="FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj" />
    <Project Path="FantasyCritic.Test/FantasyCritic.Test.csproj" />
  </Folder>
  <Folder Name="/Utilities/">
    <Project Path="FantasyCritic.BetaSync/FantasyCritic.BetaSync.csproj" />
    <Project Path="FantasyCritic.DBUtility/FantasyCritic.DBUtility.csproj" />
    <Project Path="FantasyCritic.LocalDatabaseTool/FantasyCritic.LocalDatabaseTool.csproj" />
    <Project Path="FantasyCritic.TestDataScrubber/FantasyCritic.TestDataScrubber.csproj" />
  </Folder>
  <Project Path="FantasyCritic.ApiClient/FantasyCritic.ApiClient.csproj" />
  <Project Path="FantasyCritic.DatabaseUpdater/FantasyCritic.DatabaseUpdater.csproj" />
  <Project Path="FantasyCritic.DiscordBot/FantasyCritic.DiscordBot.csproj" />
  <Project Path="FantasyCritic.EmailTemplates/FantasyCritic.EmailTemplates.csproj" />
  <Project Path="FantasyCritic.Lib/FantasyCritic.Lib.csproj" />
  <Project Path="FantasyCritic.MySQL/FantasyCritic.MySQL.csproj" />
  <Project Path="FantasyCritic.Web/FantasyCritic.Web.csproj" />
</Solution>
```

- [ ] **Step 4: Verify the empty project builds**

Run from repo root:

```powershell
dotnet build src/FantasyCritic.ApiClient/FantasyCritic.ApiClient.csproj
```

Expected: `Build succeeded.` with 0 errors. There are no `.cs` files yet — that is fine at this stage.

- [ ] **Step 5: Commit**

```powershell
git add src/FantasyCritic.ApiClient/FantasyCritic.ApiClient.csproj
git add src/FantasyCritic.slnx
git commit -m "Add FantasyCritic.ApiClient project scaffold."
```

---

### Task 3: Run the first client generation

**Files:**
- Create: `src/FantasyCritic.ApiClient/Generated/` (all contents Kiota-generated)

- [ ] **Step 1: Build the Web project to ensure the spec is up to date**

Run from repo root:

```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj
```

This triggers `Microsoft.Extensions.ApiDescription.Server` and writes a fresh `src/FantasyCritic.Web/wwwroot/openapi.json`. The build must succeed before proceeding.

- [ ] **Step 2: Run `kiota generate`**

Run from repo root:

```powershell
dotnet tool run kiota -- generate `
  -l CSharp `
  -d src/FantasyCritic.Web/wwwroot/openapi.json `
  -o src/FantasyCritic.ApiClient/Generated `
  -n FantasyCritic.ApiClient `
  --class-name FantasyCriticApiClient `
  --clean-output
```

Flag explanations:
- `-l CSharp` — generate C# output
- `-d` — path to the OpenAPI spec
- `-o` — output directory (inside the project, so `--clean-output` never touches the `.csproj`)
- `-n FantasyCritic.ApiClient` — root namespace for all generated types
- `--class-name FantasyCriticApiClient` — explicit name for the top-level client class (overrides the default derived from the spec title `"FantasyCritic.Web | v1"` which would produce an ugly name)
- `--clean-output` — wipes `Generated/` before regenerating, preventing stale files from removed endpoints

Expected: Kiota prints a summary of generated files and exits 0. The `Generated/` directory will contain:

```
src/FantasyCritic.ApiClient/Generated/
  FantasyCriticApiClient.cs
  kiota-lock.json
  Models/
    ...  (one file per response/request schema)
  Api/
    ...  (one folder per path segment, one file per endpoint)
```

- [ ] **Step 3: Build the project to verify generated code compiles**

```powershell
dotnet build src/FantasyCritic.ApiClient/FantasyCritic.ApiClient.csproj
```

Expected: `Build succeeded.` with 0 errors. (Warnings are allowed — `TreatWarningsAsErrors` is `false` for this project.)

If the build fails with missing type errors, it is likely a Kiota runtime package version mismatch. Check that all six runtime packages were added and that their versions are mutually compatible (they are all versioned together; adding them via `dotnet add package` in sequence should resolve consistent versions, but if not, align them all to the same major version).

- [ ] **Step 4: Commit the generated files**

```powershell
git add src/FantasyCritic.ApiClient/Generated/
git commit -m "Add initial Kiota-generated C# API client."
```

---

### Task 4: Write the `Regenerate-ApiClient.ps1` script

**Files:**
- Create: `scripts/Regenerate-ApiClient.ps1`

- [ ] **Step 1: Create the `scripts/` directory if it does not exist, then create the script**

Create `scripts/Regenerate-ApiClient.ps1` with this exact content:

```powershell
<#
.SYNOPSIS
    Regenerates the FantasyCritic.ApiClient from the committed OpenAPI spec.

.DESCRIPTION
    Runs `kiota generate` using the pinned local tool from .config/dotnet-tools.json.
    The Web project must have been built first so that wwwroot/openapi.json is up to date.

.EXAMPLE
    # From repo root:
    scripts/Regenerate-ApiClient.ps1

    # After an API change:
    dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj
    scripts/Regenerate-ApiClient.ps1
    dotnet build
    git add -A && git commit -m "Regenerate API client after <describe change>."
#>

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$specPath  = Join-Path $repoRoot "src/FantasyCritic.Web/wwwroot/openapi.json"
$outputPath = Join-Path $repoRoot "src/FantasyCritic.ApiClient/Generated"

if (-not (Test-Path $specPath)) {
    Write-Error "OpenAPI spec not found at '$specPath'. Build FantasyCritic.Web first:`n  dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj"
    exit 1
}

# Verify Kiota is available as a local tool.
$toolCheck = dotnet tool run kiota -- --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "Kiota local tool not found. Run:`n  dotnet tool restore"
    exit 1
}

Write-Host "Kiota version: $toolCheck"
Write-Host "Spec:   $specPath"
Write-Host "Output: $outputPath"
Write-Host ""
Write-Host "Generating..."

dotnet tool run kiota -- generate `
    -l CSharp `
    -d $specPath `
    -o $outputPath `
    -n "FantasyCritic.ApiClient" `
    --class-name "FantasyCriticApiClient" `
    --clean-output

if ($LASTEXITCODE -ne 0) {
    Write-Error "kiota generate failed (exit code $LASTEXITCODE)."
    exit 1
}

Write-Host ""
Write-Host "Done. Review the diff, build the solution, then commit:"
Write-Host "  dotnet build"
Write-Host "  git add -A && git commit -m `"Regenerate API client after <describe change>.`""
```

- [ ] **Step 2: Verify the script runs end-to-end**

Run from repo root:

```powershell
scripts/Regenerate-ApiClient.ps1
```

Expected: the script prints the Kiota version, spec path, output path, runs generation, and prints the done message. Exit code 0.

- [ ] **Step 3: Build to confirm the output still compiles after a re-run**

```powershell
dotnet build src/FantasyCritic.ApiClient/FantasyCritic.ApiClient.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```powershell
git add scripts/Regenerate-ApiClient.ps1
git commit -m "Add Regenerate-ApiClient.ps1 script."
```

---

### Task 5: Wire `FantasyCritic.IntegrationTests` to the client project

**Files:**
- Modify: `src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj`

- [ ] **Step 1: Add the project reference**

Open `src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj`. In the existing `<ItemGroup>` that contains the `<ProjectReference>` to `FantasyCritic.Web`, add one more entry so it reads:

```xml
<ItemGroup>
  <ProjectReference Include="..\FantasyCritic.Web\FantasyCritic.Web.csproj" />
  <ProjectReference Include="..\FantasyCritic.ApiClient\FantasyCritic.ApiClient.csproj" />
</ItemGroup>
```

- [ ] **Step 2: Build the full solution**

```powershell
dotnet build src/FantasyCritic.slnx
```

Expected: `Build succeeded.` with 0 errors. All projects build, including `FantasyCritic.IntegrationTests` which now has access to `FantasyCriticApiClient`.

- [ ] **Step 3: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj
git commit -m "Wire FantasyCritic.IntegrationTests to ApiClient project."
```

---

## Post-Implementation Notes

**To create a NuGet package when ready:**
```powershell
dotnet pack src/FantasyCritic.ApiClient/FantasyCritic.ApiClient.csproj -c Release -p:Version=1.0.0
dotnet nuget push src/FantasyCritic.ApiClient/bin/Release/FantasyCritic.ApiClient.1.0.0.nupkg --api-key <key> --source https://api.nuget.org/v3/index.json
```

**To use the client in an integration test** (example — no test changes required in this plan):
```csharp
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Abstractions.Authentication;
using FantasyCritic.ApiClient;

var httpClient = _factory.CreateClient();
var adapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider(), httpClient: httpClient);
var client = new FantasyCriticApiClient(adapter);

// Example call:
var currentUser = await client.Api.Account.CurrentUser.GetAsync();
```
