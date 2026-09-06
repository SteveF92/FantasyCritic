#!/usr/bin/env bash
#
# On-instance deploy script. Ships inside the release bundle and is executed by SSM as root,
# so the instance never needs a checkout of this repository.
#
# This is what is left of linuxUpdateSite.sh once the building moves to GitHub Actions:
# no dotnet, no npm, no NSwag, no git. Both binaries below are self-contained, so nothing
# here depends on a .NET install existing on the box.
#
# Layout it maintains:
#
#   /opt/fantasy-critic/
#     releases/<release-id>/   web/  dbup/  deploy.sh  RELEASE
#     current -> releases/<release-id>
#
# systemd points at /opt/fantasy-critic/current/web, so a rollback is a symlink swap.
#
# Environment:
#   FC_SKIP_MIGRATIONS=true   skip the database migrator (front-end-only redeploys)
#   FC_KEEP_RELEASES=<n>      how many old releases to retain (default 5)

set -euo pipefail

readonly APP_ROOT=/opt/fantasy-critic
readonly RELEASES_DIR="$APP_ROOT/releases"
readonly CURRENT_LINK="$APP_ROOT/current"
readonly SERVICE=fantasy-critic.service
readonly HEALTH_URL=http://127.0.0.1:5000/health
readonly KEEP_RELEASES="${FC_KEEP_RELEASES:-5}"

RELEASE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
readonly RELEASE_DIR
readonly RELEASE_ID="$(basename "$RELEASE_DIR")"

log() {
    printf '[%s] %s\n' "$(date -u +%H:%M:%S)" "$*"
}

fail() {
    printf '[%s] ERROR: %s\n' "$(date -u +%H:%M:%S)" "$*" >&2
    exit 1
}

if [ "$(id -u)" -ne 0 ]; then
    fail "Must run as root (SSM Run Command does this for you)."
fi

if [ ! -x "$RELEASE_DIR/web/FantasyCritic.Web" ]; then
    fail "No web binary at $RELEASE_DIR/web/FantasyCritic.Web — the bundle is incomplete."
fi

log "Deploying release $RELEASE_ID"
if [ -f "$RELEASE_DIR/RELEASE" ]; then
    sed 's/^/  /' "$RELEASE_DIR/RELEASE"
fi

# The previous release, captured before anything changes, so failures can name the rollback.
PREVIOUS_RELEASE=""
if [ -L "$CURRENT_LINK" ]; then
    PREVIOUS_RELEASE="$(basename "$(readlink -f "$CURRENT_LINK")")"
    log "Currently live: $PREVIOUS_RELEASE"
else
    log "No current release — this looks like the first deploy through this path."
fi
readonly PREVIOUS_RELEASE

rollback_hint() {
    if [ -n "$PREVIOUS_RELEASE" ]; then
        cat >&2 <<HINT

To roll back:
  ln -sfnT $RELEASES_DIR/$PREVIOUS_RELEASE $CURRENT_LINK
  systemctl start $SERVICE

Note that this rolls back code only. If the migrator ran, restore the pre-deploy RDS
snapshot as well.
HINT
    fi
}

# Read the environment from the service unit, so this script behaves correctly on both the
# production and beta instances without being told which one it is on.
ASPNETCORE_ENVIRONMENT="$(systemctl show "$SERVICE" -p Environment --value \
    | tr ' ' '\n' | grep '^ASPNETCORE_ENVIRONMENT=' | cut -d= -f2 || true)"
ASPNETCORE_ENVIRONMENT="${ASPNETCORE_ENVIRONMENT:-Production}"
export ASPNETCORE_ENVIRONMENT
log "ASPNETCORE_ENVIRONMENT=$ASPNETCORE_ENVIRONMENT"

# ------------------------------------------------------------------------------------------
# Stop
# ------------------------------------------------------------------------------------------
# Migrations are deliberately not expand/contract compatible, so the app must be down before
# the schema changes. A few minutes of downtime is the accepted trade.

log "Stopping $SERVICE"
systemctl stop "$SERVICE"

# ------------------------------------------------------------------------------------------
# Migrate
# ------------------------------------------------------------------------------------------

if [ "${FC_SKIP_MIGRATIONS:-false}" = "true" ]; then
    log "Skipping migrations (FC_SKIP_MIGRATIONS=true)"
else
    log "Running database migrator"
    chmod +x "$RELEASE_DIR/dbup/FantasyCritic.DatabaseUpdater"
    if ! (cd "$RELEASE_DIR/dbup" && ./FantasyCritic.DatabaseUpdater); then
        # Deliberately leaving the site stopped. A failed migration may be half-applied, and
        # DDL in MySQL is not transactional, so starting the old code against the new schema
        # is not obviously safer than staying down. Fix forward or restore the snapshot.
        printf '\n' >&2
        echo "Database migration failed. The site has been left stopped on purpose." >&2
        rollback_hint
        exit 1
    fi
    log "Migrations complete"
fi

# ------------------------------------------------------------------------------------------
# Swap
# ------------------------------------------------------------------------------------------

log "Pointing $CURRENT_LINK at $RELEASE_ID"
chmod +x "$RELEASE_DIR/web/FantasyCritic.Web"
ln -sfnT "$RELEASE_DIR" "$CURRENT_LINK"

# ------------------------------------------------------------------------------------------
# Start
# ------------------------------------------------------------------------------------------

log "Starting $SERVICE"
systemctl start "$SERVICE"

log "Waiting for $HEALTH_URL"
healthy=false
for _ in $(seq 1 60); do
    sleep 2
    if curl -fsS --max-time 5 "$HEALTH_URL" >/dev/null 2>&1; then
        healthy=true
        break
    fi
    if ! systemctl is-active --quiet "$SERVICE"; then
        break
    fi
done

if [ "$healthy" != "true" ]; then
    echo "The service did not become healthy. Last 50 journal lines:" >&2
    journalctl -u "$SERVICE" -n 50 --no-pager >&2 || true
    rollback_hint
    exit 1
fi

log "Healthy."

# ------------------------------------------------------------------------------------------
# Prune
# ------------------------------------------------------------------------------------------
# Self-contained bundles are a few hundred megabytes each, so old releases cannot accumulate
# forever. Keep enough to roll back more than once.

log "Pruning old releases (keeping $KEEP_RELEASES)"
# shellcheck disable=SC2012 # release ids sort lexicographically by construction (UTC timestamp prefix)
ls -1 "$RELEASES_DIR" | sort -r | tail -n "+$((KEEP_RELEASES + 1))" | while read -r old; do
    if [ "$old" = "$RELEASE_ID" ] || [ "$old" = "$PREVIOUS_RELEASE" ]; then
        continue
    fi
    log "  removing $old"
    rm -rf "${RELEASES_DIR:?}/$old"
done

log "Release $RELEASE_ID is live."
