#!/usr/bin/env bash
# Applies all deterministic formatting rules for FantasyCritic (C# and ClientApp).
#
# Usage:
#   # From repo root:
#   scripts/format.sh
#
#   # Verify only; exit non-zero if anything would change:
#   scripts/format.sh --check

set -euo pipefail

CHECK=0
if [[ "${1:-}" == "--check" ]]; then
    CHECK=1
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
SRC_DIR="$REPO_ROOT/src"
SOLUTION="$SRC_DIR/FantasyCritic.slnx"
CLIENT_APP_DIR="$SRC_DIR/FantasyCritic.Web/ClientApp"

run_step() {
    local label="$1"
    shift
    echo ""
    echo "==> $label"
    "$@"
}

echo "FantasyCritic format"
echo "  Repo: $REPO_ROOT"
if [[ "$CHECK" -eq 1 ]]; then
    echo "  Mode: check (no writes)"
fi

FORMAT_TARGET="Format"
if [[ "$CHECK" -eq 1 ]]; then
    FORMAT_TARGET="FormatCheck"
fi

run_step "C# (dotnet msbuild -t:$FORMAT_TARGET)" dotnet msbuild "$SOLUTION" "-t:$FORMAT_TARGET"

if [[ ! -d "$CLIENT_APP_DIR/node_modules" ]]; then
    echo "ERROR: ClientApp dependencies not installed. Run:" >&2
    echo "  cd src/FantasyCritic.Web/ClientApp" >&2
    echo "  npm install" >&2
    exit 1
fi

if [[ "$CHECK" -eq 1 ]]; then
    (
        cd "$CLIENT_APP_DIR"
        run_step "ClientApp (prettier --check)" npm exec -- prettier --check src/
        run_step "ClientApp (eslint)" npm exec -- eslint . --ext .vue,.js,.jsx,.cjs,.mjs,.ts,.tsx,.cts,.mts --ignore-path .gitignore
    )
else
    (
        cd "$CLIENT_APP_DIR"
        run_step "ClientApp (prettier)" npm run format
        run_step "ClientApp (eslint --fix)" npm run lint
    )
fi

echo ""
echo "Done."
