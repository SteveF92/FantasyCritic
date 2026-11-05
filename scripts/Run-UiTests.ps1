<#
.SYNOPSIS
    Builds the client app and runs the Playwright UI tests against it.

.DESCRIPTION
    Regenerates the API clients from a Release build of Web, builds the client app for production
    (npm run build), then runs src/FantasyCritic.UiTests in Release. Release keeps clear of the
    Debug DLLs a running dev server holds.

    The tests use the Docker MySQL on port 3307 and the installed Google Chrome. Start the database first:
      docker compose -f infrastructure/docker-compose-mysql.yaml up

    Failures leave a screenshot and a Playwright trace under src/FantasyCritic.UiTests/TestResults/ui/,
    along with console-warnings.md for the whole run. Open a trace with:
      src/FantasyCritic.UiTests/bin/Release/net10.0/playwright.ps1 show-trace <trace.zip>

.EXAMPLE
    # From repo root:
    scripts/Run-UiTests.ps1

.EXAMPLE
    # One fixture, with the browser visible, reusing the last client build:
    scripts/Run-UiTests.ps1 -Filter "FullyQualifiedName~HarnessTests" -Headed -SkipClientBuild

.EXAMPLE
    # One test, leaving Chrome open at the end until Resume is pressed in the Playwright Inspector:
    scripts/Run-UiTests.ps1 -Filter "FullyQualifiedName~CreateLeagueTests.StandardLeagueFromPreset" -Pause -SkipClientBuild
#>

param(
    [string]$Filter,
    [switch]$Headed,
    [switch]$Pause,
    [switch]$SkipClientBuild
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$srcDir = Join-Path $repoRoot "src"
$clientAppDir = Join-Path $srcDir "FantasyCritic.ClientAppVue2"
$uiTestsProject = Join-Path $srcDir "FantasyCritic.UiTests/FantasyCritic.UiTests.csproj"

function Invoke-Step {
    param(
        [string]$Label,
        [scriptblock]$Action
    )

    Write-Host ""
    Write-Host "==> $Label"
    # npm and Vite write warnings to stderr, which Windows PowerShell turns into errors when output is redirected.
    # A step fails on its exit code instead.
    $ErrorActionPreference = "Continue"
    & $Action
    if ($LASTEXITCODE -ne 0) {
        throw "$Label failed (exit code $LASTEXITCODE)."
    }
}

if (-not $SkipClientBuild) {
    Invoke-Step "Build Web (Release)" {
        dotnet build (Join-Path $srcDir "FantasyCritic.Web/FantasyCritic.Web.csproj") -c Release
    }

    Invoke-Step "Regenerate API clients" {
        $env:NSWAG_CONFIGURATION = "Release"
        try {
            & (Join-Path $PSScriptRoot "Regenerate-ApiClient.ps1")
        }
        finally {
            Remove-Item Env:NSWAG_CONFIGURATION
        }
    }

    Invoke-Step "Build the client app (npm run build)" {
        Push-Location $clientAppDir
        try {
            npm run build
        }
        finally {
            Pop-Location
        }
    }
}

Invoke-Step "Build UI tests (Release)" {
    dotnet build $uiTestsProject -c Release
}

$testArguments = @($uiTestsProject, "-c", "Release", "--no-build")
if ($Filter) {
    $testArguments += @("--filter", $Filter)
}

if ($Headed) {
    $env:UI_TESTS_HEADED = "1"
}

if ($Pause) {
    $env:UI_TESTS_PAUSE = "1"
}

try {
    Invoke-Step "Run UI tests" {
        dotnet test @testArguments
    }
}
finally {
    if ($Headed) {
        Remove-Item Env:UI_TESTS_HEADED
    }

    if ($Pause) {
        Remove-Item Env:UI_TESTS_PAUSE
    }
}
