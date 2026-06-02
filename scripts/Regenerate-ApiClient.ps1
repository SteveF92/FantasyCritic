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
#>

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

Write-Host "Kiota version: $($toolCheck | Select-Object -First 1)"
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
    --exclude-backward-compatible `
    --clean-output

if ($LASTEXITCODE -ne 0) {
    Write-Error "kiota generate failed (exit code $LASTEXITCODE)."
    exit 1
}

Write-Host ""
Write-Host "Done. Build the solution to verify, then run your tests:"
Write-Host "  dotnet build"
