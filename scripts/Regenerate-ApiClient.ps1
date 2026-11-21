<#
.SYNOPSIS
    Regenerates the FantasyCritic C# and TypeScript API clients from the Web project's OpenAPI metadata.

.DESCRIPTION
    Runs `nswag run` using the pinned local tool from .config/dotnet-tools.json.
    Builds the Web project first (nswag.json uses noBuild, so it would otherwise read a stale DLL). Outputs:
      - src/FantasyCritic.ApiClient/Generated/FantasyCriticClients.cs
      - src/FantasyCritic.VueClientApp/src/api/generated/FantasyCriticClients.ts

.EXAMPLE
    # From repo root:
    scripts/Regenerate-ApiClient.ps1

    # After an API change (the script builds the Web project itself):
    scripts/Regenerate-ApiClient.ps1
    dotnet build
#>

$repoRoot  = Resolve-Path (Join-Path $PSScriptRoot "..")
$clientDir = Join-Path $repoRoot "src/FantasyCritic.ApiClient"

# Verify NSwag is available as a local tool.
$toolCheck = dotnet tool run nswag -- version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "NSwag local tool not found. Run:`n  dotnet tool restore"
    exit 1
}

Write-Host "NSwag version: $($toolCheck | Select-Object -First 1)"
Write-Host "Output:"
Write-Host "  $clientDir\Generated\FantasyCriticClients.cs"
Write-Host "  $repoRoot\src\FantasyCritic.VueClientApp\src\api\generated\FantasyCriticClients.ts"
Write-Host ""
$nswagConfiguration = if ($env:NSWAG_CONFIGURATION) { $env:NSWAG_CONFIGURATION } else { "Debug" }

# nswag.json uses noBuild, so it reflects over whatever DLL is on disk. Build the Web project
# first, in the same configuration, or the clients are generated from a stale API surface.
Write-Host "Building Web project ($nswagConfiguration)..."
dotnet build (Join-Path $repoRoot "src/FantasyCritic.Web/FantasyCritic.Web.csproj") -c $nswagConfiguration --nologo -v quiet
if ($LASTEXITCODE -ne 0) {
    Write-Error "Web project build failed (exit code $LASTEXITCODE). If the dev server is running it may be locking the DLLs."
    exit 1
}

Write-Host "Generating..."

# NSwag bootstraps the Web host without launchSettings.json; force Development so
# Program.cs skips AWS Secrets Manager (some machines set ASPNETCORE_ENVIRONMENT globally).
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:DOTNET_ENVIRONMENT = "Development"

Push-Location $clientDir
try {
    dotnet tool run nswag -- run nswag.json "/variables:Configuration=$nswagConfiguration"
    if ($LASTEXITCODE -ne 0) {
        Write-Error "nswag run failed (exit code $LASTEXITCODE)."
        exit 1
    }
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "Done. Build the solution to verify, then run your tests:"
Write-Host "  dotnet build"
