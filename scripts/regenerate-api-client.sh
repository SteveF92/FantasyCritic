#!/usr/bin/env bash
# Regenerates the FantasyCritic C# and TypeScript API clients from the Web project's OpenAPI metadata.
#
# Usage:
#   # From repo root:
#   scripts/regenerate-api-client.sh
#
#   # After an API change (the script builds the Web project itself):
#   scripts/regenerate-api-client.sh
#   dotnet build

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
CLIENT_DIR="$REPO_ROOT/src/FantasyCritic.ApiClient"

# Verify NSwag is available as a local tool.
if ! dotnet tool run nswag -- version &>/dev/null; then
    echo "ERROR: NSwag local tool not found. Run:" >&2
    echo "  dotnet tool restore" >&2
    exit 1
fi

NSWAG_VERSION=$(dotnet tool run nswag -- version 2>&1 | head -1)
echo "NSwag version: $NSWAG_VERSION"
echo "Output:"
echo "  $CLIENT_DIR/Generated/FantasyCriticClients.cs"
echo "  $REPO_ROOT/src/FantasyCritic.VueClientApp/src/api/generated/FantasyCriticClients.ts"
echo ""
NSWAG_CONFIGURATION="${NSWAG_CONFIGURATION:-Debug}"

# nswag.json uses noBuild, so it reflects over whatever DLL is on disk. Build the Web project
# first, in the same configuration, or the clients are generated from a stale API surface.
# (A no-op when CI or the Docker build has just built it.)
echo "Building Web project ($NSWAG_CONFIGURATION)..."
if ! dotnet build "$REPO_ROOT/src/FantasyCritic.Web/FantasyCritic.Web.csproj" -c "$NSWAG_CONFIGURATION" --nologo -v quiet; then
    echo "ERROR: Web project build failed. If the dev server is running it may be locking the DLLs." >&2
    exit 1
fi

echo "Generating..."

# NSwag bootstraps the Web host without launchSettings.json; force Development so
# Program.cs skips AWS Secrets Manager (some machines set ASPNETCORE_ENVIRONMENT globally).
export ASPNETCORE_ENVIRONMENT=Development
export DOTNET_ENVIRONMENT=Development

cd "$CLIENT_DIR"
dotnet tool run nswag -- run nswag.json "/variables:Configuration=$NSWAG_CONFIGURATION"

echo ""
echo "Done. Build the solution to verify, then run your tests:"
echo "  dotnet build"
