#!/usr/bin/env bash
#
# On-instance deploy script. Ships inside the release bundle and is executed by SSM as root,
# so the instance never needs a checkout of this repository.
#
# Phase 3 replaced the self-contained publish bundle with container images. What travels in the
# bundle now is only this script, the compose file, the maintenance page and the release
# metadata — a few kilobytes instead of a few hundred megabytes. The application itself comes
# from ECR.
#
# What the instance needs: Docker (with the compose plugin), the AWS CLI, nginx and curl.
# No .NET, no Node, no git.
#
# Layout it maintains:
#
#   /opt/fantasy-critic/
#     docker-compose.yaml      installed from the release being deployed
#     .env                     environment + registry + IMAGE_TAG (created during setup)
#     RELEASE                  bind-mounted into the web container, read by the admin console
#     releases/<release-id>/   the extracted bundle, kept so an old release can redeploy itself
#
# Rolling back is running the previous release's copy of this script, which puts back both its
# image tag and its compose file:
#
#   sudo FC_SKIP_MIGRATIONS=true /opt/fantasy-critic/releases/<previous-id>/deploy.sh
#
# The maintenance page is raised before the containers stop and lowered once the new release is
# healthy. Any failure below deliberately leaves it raised.
#
# Environment:
#   FC_IMAGE_TAG=<tag>        required; the ECR tag to deploy (the release id)
#   FC_SKIP_MIGRATIONS=true   skip the database migrator (front-end-only redeploys, rollbacks)
#   FC_KEEP_RELEASES=<n>      how many old release directories to retain (default 10)

set -euo pipefail

readonly APP_ROOT=/opt/fantasy-critic
readonly RELEASES_DIR="$APP_ROOT/releases"
readonly COMPOSE_FILE="$APP_ROOT/docker-compose.yaml"
readonly ENV_FILE="$APP_ROOT/.env"
readonly RELEASE_FILE="$APP_ROOT/RELEASE"
readonly HEALTH_URL=http://127.0.0.1:5000/health
readonly KEEP_RELEASES="${FC_KEEP_RELEASES:-10}"

RELEASE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
readonly RELEASE_DIR
RELEASE_ID="$(basename "$RELEASE_DIR")"
readonly RELEASE_ID
readonly MAINTENANCE="$RELEASE_DIR/maintenance.sh"

log() {
    printf '[%s] %s\n' "$(date -u +%H:%M:%S)" "$*"
}

fail() {
    printf '[%s] ERROR: %s\n' "$(date -u +%H:%M:%S)" "$*" >&2
    exit 1
}

compose() {
    docker compose --project-directory "$APP_ROOT" -f "$COMPOSE_FILE" "$@"
}

# `docker compose ps --status` is too new to rely on across compose plugin versions, so ask
# the daemon directly.
web_is_running() {
    local container_id
    container_id="$(compose ps -q web 2>/dev/null || true)"
    [ -n "$container_id" ] || return 1
    [ "$(docker inspect -f '{{.State.Running}}' "$container_id" 2>/dev/null || echo false)" = "true" ]
}

# ------------------------------------------------------------------------------------------
# Preconditions
# ------------------------------------------------------------------------------------------
# All of these are checked before the site is touched. Nothing below this block should be able
# to take the site down and then discover it cannot bring it back.

if [ "$(id -u)" -ne 0 ]; then
    fail "Must run as root (SSM Run Command does this for you)."
fi

command -v docker >/dev/null 2>&1 || fail "docker is not installed on this host."
docker compose version >/dev/null 2>&1 || fail "the docker compose plugin is not installed on this host."
command -v aws >/dev/null 2>&1 || fail "the AWS CLI is not on PATH ($PATH)."
command -v curl >/dev/null 2>&1 || fail "curl is not installed on this host."

IMAGE_TAG="${FC_IMAGE_TAG:-}"
[ -n "$IMAGE_TAG" ] || fail "FC_IMAGE_TAG is not set. It should be the release id that was pushed to ECR."
readonly IMAGE_TAG

[ -f "$RELEASE_DIR/docker-compose.yaml" ] || fail "No docker-compose.yaml in the bundle at $RELEASE_DIR."
[ -f "$RELEASE_DIR/maintenance.html" ] && [ -f "$MAINTENANCE" ] \
    || fail "No maintenance page in the bundle — refusing to deploy without one."
[ -f "$ENV_FILE" ] || fail "$ENV_FILE does not exist. It is created once during Phase 3 setup; see docs/deployment-phase-3-setup.md."

# Read the keys this script needs out of the env file rather than sourcing it. Compose reads
# the file itself, so nothing here has to export anything; and sourcing would both execute
# whatever the file contains and collide with IMAGE_TAG, which is set above from FC_IMAGE_TAG.
env_value() {
    grep -E "^$1=" "$ENV_FILE" | tail -1 | cut -d= -f2- || true
}

ECR_REGISTRY="$(env_value ECR_REGISTRY)"
AWS_REGION="$(env_value AWS_REGION)"
DEPLOY_ENVIRONMENT="$(env_value ASPNETCORE_ENVIRONMENT)"
readonly ECR_REGISTRY AWS_REGION DEPLOY_ENVIRONMENT
[ -n "$ECR_REGISTRY" ] || fail "ECR_REGISTRY is not set in $ENV_FILE."
[ -n "$AWS_REGION" ] || fail "AWS_REGION is not set in $ENV_FILE."
[ -n "$DEPLOY_ENVIRONMENT" ] || fail "ASPNETCORE_ENVIRONMENT is not set in $ENV_FILE."

log "Deploying release $RELEASE_ID to $DEPLOY_ENVIRONMENT"
if [ -f "$RELEASE_DIR/RELEASE" ]; then
    sed 's/^/  /' "$RELEASE_DIR/RELEASE"
fi

# The tag that is live right now, captured before anything changes, so failures can name the
# rollback. Empty on the first deploy through this path.
PREVIOUS_TAG="$(env_value IMAGE_TAG)"
readonly PREVIOUS_TAG
if [ -n "$PREVIOUS_TAG" ]; then
    log "Currently live: $PREVIOUS_TAG"
else
    log "No IMAGE_TAG recorded — this looks like the first deploy through this path."
fi

rollback_hint() {
    if [ -n "$PREVIOUS_TAG" ] && [ -x "$RELEASES_DIR/$PREVIOUS_TAG/deploy.sh" ]; then
        cat >&2 <<HINT

To roll back:
  sudo FC_SKIP_MIGRATIONS=true $RELEASES_DIR/$PREVIOUS_TAG/deploy.sh

That restores the previous release's image tag and its compose file, and lowers the
maintenance page on the way out.

Note that this rolls back code only. If the migrator ran, restore the pre-deploy RDS
snapshot as well.
HINT
    elif [ -n "$PREVIOUS_TAG" ]; then
        cat >&2 <<HINT

To roll back, put IMAGE_TAG=$PREVIOUS_TAG back in $ENV_FILE and run:
  cd $APP_ROOT && docker compose up -d web discord-bot && ./maintenance.sh off
HINT
    fi
}

# ------------------------------------------------------------------------------------------
# Pull
# ------------------------------------------------------------------------------------------
# Everything that can fail without downtime happens before the maintenance page goes up: the
# registry login, the compose file, and the image pull itself.

log "Logging in to $ECR_REGISTRY"
aws ecr get-login-password --region "$AWS_REGION" \
    | docker login --username AWS --password-stdin "$ECR_REGISTRY" >/dev/null

install -m 644 "$RELEASE_DIR/docker-compose.yaml" "$COMPOSE_FILE"

if grep -qE '^IMAGE_TAG=' "$ENV_FILE"; then
    sed -i -E "s|^IMAGE_TAG=.*|IMAGE_TAG=$IMAGE_TAG|" "$ENV_FILE"
else
    echo "IMAGE_TAG=$IMAGE_TAG" >> "$ENV_FILE"
fi

# The web container bind-mounts this file. Docker creates a *directory* in place of a missing
# bind-mount source, which the app would then fail to read, so it has to exist before anything
# starts. Written now rather than after the swap because there is no swap any more.
if [ -f "$RELEASE_DIR/RELEASE" ]; then
    install -m 644 "$RELEASE_DIR/RELEASE" "$RELEASE_FILE"
else
    : > "$RELEASE_FILE"
fi

# --profile migrate is what makes this pull the migrator image too. A plain `pull` only covers
# services with no profile, which would leave the migrator to download later — during the
# downtime window, which is the one place this script tries never to do slow work.
log "Pulling images tagged $IMAGE_TAG"
compose --profile migrate pull --quiet

# ------------------------------------------------------------------------------------------
# Stop
# ------------------------------------------------------------------------------------------
# Migrations are deliberately not expand/contract compatible, so the app must be down before
# the schema changes. A few minutes of downtime is the accepted trade.

# The page has to be installed from this release before it is raised, so that editing it is
# an ordinary code change that ships with a deploy rather than a file hand-copied to the box.
chmod +x "$MAINTENANCE"
"$MAINTENANCE" install
"$MAINTENANCE" on

log "Stopping containers"
compose down --remove-orphans

# ------------------------------------------------------------------------------------------
# Migrate
# ------------------------------------------------------------------------------------------

if [ "${FC_SKIP_MIGRATIONS:-false}" = "true" ]; then
    log "Skipping migrations (FC_SKIP_MIGRATIONS=true)"
else
    log "Running database migrator"
    if ! compose run --rm database-updater; then
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
# Start
# ------------------------------------------------------------------------------------------

# Stamp when this release went live. The web container reads RELEASE at startup and shows it in
# the admin console, so this has to be written before it starts. Appending rather than
# rewriting means a re-run of this script leaves a history in the file; the app takes the last
# value.
echo "deployed_at=$(date -u +%Y-%m-%dT%H:%M:%SZ)" >> "$RELEASE_FILE"

log "Starting web and discord-bot"
compose up -d web discord-bot

log "Waiting for $HEALTH_URL"
healthy=false
for _ in $(seq 1 60); do
    sleep 2
    if curl -fsS --max-time 5 "$HEALTH_URL" >/dev/null 2>&1; then
        healthy=true
        break
    fi
    # No point waiting out the full two minutes if the container has already given up.
    if ! web_is_running; then
        break
    fi
done

if [ "$healthy" != "true" ]; then
    echo "The web container did not become healthy. Last 50 log lines:" >&2
    compose logs --tail 50 web >&2 || true
    echo "Discord bot, last 20 lines:" >&2
    compose logs --tail 20 discord-bot >&2 || true
    rollback_hint
    exit 1
fi

log "Healthy."

"$MAINTENANCE" off

# ------------------------------------------------------------------------------------------
# Prune
# ------------------------------------------------------------------------------------------
# Three images per release at a few hundred megabytes each fills a disk quickly.

log "Pruning unused images"
docker image prune --force >/dev/null || true

log "Pruning old release directories (keeping $KEEP_RELEASES)"
# shellcheck disable=SC2012 # release ids sort lexicographically by construction (UTC timestamp prefix)
ls -1 "$RELEASES_DIR" 2>/dev/null | sort -r | tail -n "+$((KEEP_RELEASES + 1))" | while read -r old; do
    if [ "$old" = "$RELEASE_ID" ] || [ "$old" = "$PREVIOUS_TAG" ]; then
        continue
    fi
    log "  removing $old"
    rm -rf "${RELEASES_DIR:?}/$old"
done

log "Release $RELEASE_ID is live."
