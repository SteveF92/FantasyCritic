#!/usr/bin/env bash
# Regenerates the FantasyCritic C# and TypeScript API clients from the Web project's OpenAPI metadata.
#
# Usage:
#   # From repo root:
#   scripts/regenerate-api-client.sh
#
#   # After an API change:
#   dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj
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
echo "  $REPO_ROOT/src/FantasyCritic.Web/ClientApp/src/api/generated/FantasyCriticClients.ts"
echo ""
echo "Generating..."

cd "$CLIENT_DIR"
dotnet tool run nswag -- run nswag.json

echo ""
echo "Done. Build the solution to verify, then run your tests:"
echo "  dotnet build"
