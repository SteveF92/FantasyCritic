<#
.SYNOPSIS
    Regenerates the FantasyCritic.ApiClient from the Web project's OpenAPI metadata.

.DESCRIPTION
    Runs `nswag run` using the pinned local tool from .config/dotnet-tools.json.
    The Web project must have been built first.

.EXAMPLE
    # From repo root:
    scripts/Regenerate-ApiClient.ps1

    # After an API change:
    dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj
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
Write-Host "Output: $clientDir\Generated\FantasyCriticClients.cs"
Write-Host ""
Write-Host "Generating..."

Push-Location $clientDir
try {
    dotnet tool run nswag -- run nswag.json
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
