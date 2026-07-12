<#
.SYNOPSIS
    Applies all deterministic formatting rules for FantasyCritic (C# and ClientApp).

.DESCRIPTION
    Runs dotnet format whitespace against src/.editorconfig (indent, EOL, final newline,
    trim trailing whitespace), then Prettier and ESLint --fix on the Vue client app.

    Use -Style to also apply C# code-style rules configured at warning severity or higher
    in .editorconfig (for incremental style adoption).

    Use -Check to verify formatting without writing changes (for CI).

.EXAMPLE
    # From repo root:
    scripts/Format.ps1

.EXAMPLE
    # Verify only; exit non-zero if anything would change:
    scripts/Format.ps1 -Check
#>

param(
    [switch]$Check,
    [switch]$Style
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$srcDir = Join-Path $repoRoot "src"
$solution = Join-Path $srcDir "FantasyCritic.slnx"
$clientAppDir = Join-Path $srcDir "FantasyCritic.Web\ClientApp"
$formatTarget = if ($Check) { "FormatCheck" } else { "Format" }

function Invoke-Step {
    param(
        [string]$Label,
        [scriptblock]$Action
    )

    Write-Host ""
    Write-Host "==> $Label"
    & $Action
    if ($LASTEXITCODE -ne 0) {
        throw "$Label failed (exit code $LASTEXITCODE)."
    }
}

Write-Host "FantasyCritic format"
Write-Host "  Repo: $repoRoot"
if ($Check) {
    Write-Host "  Mode: check (no writes)"
}

Invoke-Step "C# whitespace (dotnet msbuild -t:$formatTarget)" {
    Push-Location $srcDir
    try {
        dotnet msbuild $solution "-t:$formatTarget"
    }
    finally {
        Pop-Location
    }
}

if ($Style) {
    $styleTarget = if ($Check) { "FormatStyleCheck" } else { "FormatStyle" }
    Invoke-Step "C# style (dotnet msbuild -t:$styleTarget)" {
        Push-Location $srcDir
        try {
            dotnet msbuild $solution "-t:$styleTarget"
        }
        finally {
            Pop-Location
        }
    }
}

if (-not (Test-Path (Join-Path $clientAppDir "node_modules"))) {
    throw "ClientApp dependencies not installed. Run:`n  cd src/FantasyCritic.Web/ClientApp`n  npm install"
}

if ($Check) {
    Invoke-Step "ClientApp (prettier --check)" {
        Push-Location $clientAppDir
        try {
            npm exec -- prettier --check src/
        }
        finally {
            Pop-Location
        }
    }

    Invoke-Step "ClientApp (eslint)" {
        Push-Location $clientAppDir
        try {
            npm exec -- eslint . --ext .vue,.js,.jsx,.cjs,.mjs,.ts,.tsx,.cts,.mts --ignore-path .gitignore
        }
        finally {
            Pop-Location
        }
    }
}
else {
    Invoke-Step "ClientApp (prettier)" {
        Push-Location $clientAppDir
        try {
            npm run format
        }
        finally {
            Pop-Location
        }
    }

    Invoke-Step "ClientApp (eslint --fix)" {
        Push-Location $clientAppDir
        try {
            npm run lint
        }
        finally {
            Pop-Location
        }
    }
}

Write-Host ""
Write-Host "Done."
