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
#     maintenance.sh           fixed path, so raising the page by hand needs no release id:
#     maintenance.html           sudo /opt/fantasy-critic/maintenance.sh on|off|status
#     releases/<release-id>/   the extracted bundle, kept so an old release can redeploy itself
#
# It also installs /etc/nginx/maintenance.conf from the bundle, so that snippet tracks the
# repository. The one `include` line in the certbot-managed site file stays manual; this script
# warns loudly when nothing loads the snippet.
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
#   FC_IMAGE_TAG=<tag>        the ECR tag to deploy; defaults to this release directory's name
#   FC_SKIP_MIGRATIONS=true   skip the database migrator (front-end-only redeploys, rollbacks)
#   FC_KEEP_RELEASES=<n>      how many old release directories to retain (default 10)

set -euo pipefail

readonly APP_ROOT=/opt/fantasy-critic
readonly RELEASES_DIR="$APP_ROOT/releases"
readonly COMPOSE_FILE="$APP_ROOT/docker-compose.yaml"
readonly ENV_FILE="$APP_ROOT/.env"
readonly RELEASE_FILE="$APP_ROOT/RELEASE"
readonly NGINX_SNIPPET=/etc/nginx/maintenance.conf
readonly HEALTH_URL=http://127.0.0.1:5000/health
readonly KEEP_RELEASES="${FC_KEEP_RELEASES:-10}"

RELEASE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
readonly RELEASE_DIR
RELEASE_ID="$(basename "$RELEASE_DIR")"
readonly RELEASE_ID
# Installed to a fixed path next to the compose file, rather than used from the release
# directory, so that turning the page on by hand does not mean first working out which release
# is live. maintenance.sh copies the page from whatever directory it is sitting in, so the two
# files travel together.
readonly MAINTENANCE="$APP_ROOT/maintenance.sh"

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

# Keeps /etc/nginx/maintenance.conf in step with the repository.
#
# This used to be a copy someone pasted onto the box once, which meant an instance that was
# powered off when Phase 2 went out silently never got it — and the only symptom was a bad
# gateway instead of the maintenance page, months later, during a deploy. Shipping it in the
# bundle makes the snippet a code change like any other.
#
# The `include` line in the site file stays manual: that file is certbot-managed and its shape
# differs per instance, so this reports on it rather than editing it.
install_nginx_snippet() {
    if ! command -v nginx >/dev/null 2>&1; then
        log "No nginx on this host; skipping the maintenance snippet."
        return 0
    fi

    local source="$RELEASE_DIR/nginx_maintenance.conf"
    if [ ! -f "$source" ]; then
        log "WARNING: this bundle has no nginx_maintenance.conf; leaving $NGINX_SNIPPET as it is."
        return 0
    fi

    if [ -f "$NGINX_SNIPPET" ] && cmp -s "$source" "$NGINX_SNIPPET"; then
        log "nginx maintenance snippet already matches this release."
    else
        log "Installing $NGINX_SNIPPET"
        local backup=""
        if [ -f "$NGINX_SNIPPET" ]; then
            backup="$NGINX_SNIPPET.deploy-backup"
            cp -p "$NGINX_SNIPPET" "$backup"
        fi
        install -m 644 "$source" "$NGINX_SNIPPET"

        # A snippet that does not parse would take the site down at the next reload — including
        # a reload nobody remembers doing, weeks later. Validate here, put the old one back if
        # it fails, and stop before anything else is touched.
        if ! nginx -t >/dev/null 2>&1; then
            # Capture why while the bad file is still in place. Running `nginx -t` after the
            # revert would report the restored config as fine, immediately above an error
            # saying it failed.
            local nginx_error
            nginx_error="$(nginx -t 2>&1 || true)"
            if [ -n "$backup" ]; then
                mv -f "$backup" "$NGINX_SNIPPET"
            else
                rm -f "$NGINX_SNIPPET"
            fi
            printf '%s\n' "$nginx_error" >&2
            fail "The nginx maintenance snippet in this release fails 'nginx -t'. Reverted; the site has not been touched."
        fi
        rm -f "$backup"
        systemctl reload nginx
        log "nginx reloaded."
    fi

    # `nginx -T` prints a "# configuration file <path>:" header for every file it actually
    # loads, so this is asking whether a server block really includes it, not whether it exists.
    if nginx -T 2>/dev/null | grep -q "configuration file $NGINX_SNIPPET"; then
        log "nginx includes the maintenance snippet."
        return 0
    fi

    cat >&2 <<WARN

  ======================================================================
  WARNING: no server block includes $NGINX_SNIPPET

  The file is installed and valid, but nothing loads it, so this deploy
  will show a bad gateway instead of the maintenance page.

  Add this line inside the TLS server block, above the location blocks:

      include $NGINX_SNIPPET;

  then: sudo nginx -t && sudo systemctl reload nginx
  ======================================================================

WARN
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

# Defaults to this release's own id. The release directory is named after it and the images
# were pushed under it, so a rollback — which is just running an older release's copy of this
# script — needs nothing passed in. The workflow still sets it explicitly.
IMAGE_TAG="${FC_IMAGE_TAG:-$RELEASE_ID}"
[ -n "$IMAGE_TAG" ] || fail "Could not work out which image tag to deploy."
readonly IMAGE_TAG

[ -f "$RELEASE_DIR/docker-compose.yaml" ] || fail "No docker-compose.yaml in the bundle at $RELEASE_DIR."
[ -f "$RELEASE_DIR/maintenance.html" ] && [ -f "$RELEASE_DIR/maintenance.sh" ] \
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
  cd $APP_ROOT && docker compose up -d web discord-bot && $MAINTENANCE off
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

# The maintenance page and its script go to fixed paths so a human never has to look up the
# live release id to raise the page. They are installed from this release, so editing the
# wording stays an ordinary code change that ships with a deploy, and a rollback puts back the
# copy that shipped with the release it is rolling back to.
install -m 755 "$RELEASE_DIR/maintenance.sh" "$MAINTENANCE"
install -m 644 "$RELEASE_DIR/maintenance.html" "$APP_ROOT/maintenance.html"

# Before the page is needed, and before anything is stopped: a failure here must not leave the
# site down.
install_nginx_snippet

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

# Copies the page into the directory nginx serves it from, then raises the flag. Both files
# were put at their fixed paths above, so this is already this release's wording.
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
