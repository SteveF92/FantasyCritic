#!/usr/bin/env bash
#
# On-instance deploy script. Ships inside the release bundle and is run by SSM as root, so the
# instance needs only Docker (with the compose plugin), the AWS CLI, nginx and curl. The
# application itself comes from ECR.
#
#   /opt/fantasy-critic/
#     docker-compose.yaml      installed from the release being deployed
#     .env                     environment, registry, and IMAGE_TAG: the release that is live
#     RELEASE                  bind-mounted into the web container, shown in the admin console
#     maintenance.sh/.html     at a fixed path, for raising the page by hand:
#                                sudo /opt/fantasy-critic/maintenance.sh on|off|status
#     releases/<release-id>/   the extracted bundle, kept so an old release can redeploy itself
#
# Rolling back is running the previous release's copy of this script, which puts back both its
# image tag and its compose file:
#
#   sudo FC_SKIP_MIGRATIONS=true /opt/fantasy-critic/releases/<previous-id>/deploy.sh
#
# Environment:
#   FC_IMAGE_TAG=<tag>        the ECR tag to deploy; defaults to this release directory's name
#   FC_SKIP_MIGRATIONS=true   skip the database migrator (front-end-only redeploys, rollbacks)
#   FC_SKIP_DRAIN=true        do not wait for the worker; a running job is killed. For a hung
#                             job, a stuck Running row, or the first deploy of the drain itself
#   FC_KEEP_RELEASES=<n>      how many old release directories to retain (default 10)

set -euo pipefail

readonly APP_ROOT=/opt/fantasy-critic
readonly RELEASES_DIR="$APP_ROOT/releases"
readonly COMPOSE_FILE="$APP_ROOT/docker-compose.yaml"
readonly ENV_FILE="$APP_ROOT/.env"
readonly RELEASE_FILE="$APP_ROOT/RELEASE"
readonly MAINTENANCE="$APP_ROOT/maintenance.sh"
readonly NGINX_SNIPPET=/etc/nginx/maintenance.conf
readonly HEALTH_URL=http://127.0.0.1:5000/health
readonly KEEP_RELEASES="${FC_KEEP_RELEASES:-10}"

RELEASE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
readonly RELEASE_DIR
RELEASE_ID="$(basename "$RELEASE_DIR")"
readonly RELEASE_ID
readonly DEPLOY_ENV_FILE="$RELEASE_DIR/.env.deploy"

# True while this deploy has job pulling turned off; the EXIT trap turns it back on.
RESTORE_WORKER_PULLING=false

main() {
    trap restore_worker_pulling EXIT

    check_preconditions
    read_settings
    announce_release

    # Everything that can fail without downtime happens before the maintenance page goes up.
    log_in_to_registry
    install_release_files
    install_nginx_snippet
    write_deploy_env_file
    pull_images
    drain_worker

    # Migrations are not expand/contract compatible, so the site is down while they run.
    raise_maintenance_page
    stop_containers
    run_migrations
    mark_release_live

    # Before the worker starts, so that it comes up pulling.
    local worker_left_off=false
    restore_worker_pulling || worker_left_off=true

    start_containers
    wait_for_web
    lower_maintenance_page

    check_worker
    if [ "$worker_left_off" = "true" ]; then
        fail "The worker is healthy but was left switched off: WorkerShouldPullNewJobs could not be turned back on."
    fi
    check_discord_bot

    prune
    log "Release $RELEASE_ID is live."
}

# ------------------------------------------------------------------------------------------
# Phases
# ------------------------------------------------------------------------------------------

check_preconditions() {
    if [ "$(id -u)" -ne 0 ]; then
        fail "Must run as root (SSM Run Command does this for you)."
    fi

    command -v docker >/dev/null 2>&1 || fail "docker is not installed on this host."
    docker compose version >/dev/null 2>&1 || fail "the docker compose plugin is not installed on this host."
    command -v aws >/dev/null 2>&1 || fail "the AWS CLI is not on PATH ($PATH)."
    command -v curl >/dev/null 2>&1 || fail "curl is not installed on this host."

    [ -f "$RELEASE_DIR/docker-compose.yaml" ] || fail "No docker-compose.yaml in the bundle at $RELEASE_DIR."
    [ -f "$RELEASE_DIR/maintenance.html" ] && [ -f "$RELEASE_DIR/maintenance.sh" ] \
        || fail "No maintenance page in the bundle — refusing to deploy without one."
    [ -f "$ENV_FILE" ] || fail "$ENV_FILE does not exist. It is created once during Phase 3 setup; see docs/deployment-phase-3-setup.md."
}

read_settings() {
    IMAGE_TAG="${FC_IMAGE_TAG:-$RELEASE_ID}"
    ECR_REGISTRY="$(env_value ECR_REGISTRY)"
    AWS_REGION="$(env_value AWS_REGION)"
    DEPLOY_ENVIRONMENT="$(env_value ASPNETCORE_ENVIRONMENT)"
    PREVIOUS_TAG="$(env_value IMAGE_TAG)"
    readonly IMAGE_TAG ECR_REGISTRY AWS_REGION DEPLOY_ENVIRONMENT PREVIOUS_TAG

    [ -n "$IMAGE_TAG" ] || fail "Could not work out which image tag to deploy."
    [ -n "$ECR_REGISTRY" ] || fail "ECR_REGISTRY is not set in $ENV_FILE."
    [ -n "$AWS_REGION" ] || fail "AWS_REGION is not set in $ENV_FILE."
    [ -n "$DEPLOY_ENVIRONMENT" ] || fail "ASPNETCORE_ENVIRONMENT is not set in $ENV_FILE."
}

announce_release() {
    log "Deploying release $RELEASE_ID to $DEPLOY_ENVIRONMENT"
    if [ -f "$RELEASE_DIR/RELEASE" ]; then
        sed 's/^/  /' "$RELEASE_DIR/RELEASE"
    fi

    if [ -n "$PREVIOUS_TAG" ]; then
        log "Currently live: $PREVIOUS_TAG"
    else
        log "No IMAGE_TAG recorded — this looks like the first deploy through this path."
    fi
}

log_in_to_registry() {
    log "Logging in to $ECR_REGISTRY"
    aws ecr get-login-password --region "$AWS_REGION" \
        | docker login --username AWS --password-stdin "$ECR_REGISTRY" >/dev/null
}

# Installed from this release, so that a rollback puts back the copies that shipped with it.
install_release_files() {
    install -m 644 "$RELEASE_DIR/docker-compose.yaml" "$COMPOSE_FILE"
    install -m 755 "$RELEASE_DIR/maintenance.sh" "$MAINTENANCE"
    install -m 644 "$RELEASE_DIR/maintenance.html" "$APP_ROOT/maintenance.html"

    if [ -f "$RELEASE_DIR/RELEASE" ]; then
        install -m 644 "$RELEASE_DIR/RELEASE" "$RELEASE_FILE"
    else
        : > "$RELEASE_FILE"
    fi
}

# The `include` line in the site file stays manual: that file is certbot-managed and differs per
# instance, so this only reports whether anything loads the snippet.
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

        if ! nginx -t >/dev/null 2>&1; then
            # Captured before reverting, or it would report the restored config instead.
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

    # `nginx -T` prints a "# configuration file <path>:" header for each file it actually loads.
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

# A copy of .env that names this release, for compose() to read. It lets the pull, the drain
# and the migrator use the new images while .env itself still names the release that is live.
write_deploy_env_file() {
    install -m 600 /dev/null "$DEPLOY_ENV_FILE"
    { grep -vE '^IMAGE_TAG=' "$ENV_FILE" || true; echo "IMAGE_TAG=$IMAGE_TAG"; } > "$DEPLOY_ENV_FILE"
}

pull_images() {
    log "Pulling images tagged $IMAGE_TAG"
    # The profiles pull the migrator and command-line images too, so nothing downloads during
    # the downtime.
    compose --profile migrate --profile tools pull --quiet
}

# Stopping the containers kills whatever job is running, so turn job pulling off (the admin
# console's "Turn Off Worker") and wait for the current job to finish. The site stays up while
# this waits, and giving up stops the deploy with nothing touched.
#
# This runs the new command-line image against the unmigrated database. If a release changes
# the tables it reads, or on the first deploy of the job system, FC_SKIP_DRAIN=true gets past it.
drain_worker() {
    if [ "${FC_SKIP_DRAIN:-false}" = "true" ]; then
        log "Skipping the worker drain (FC_SKIP_DRAIN=true). A job that is running now will be killed."
        return 0
    fi

    log "Draining the job worker"
    local was_pulling
    was_pulling="$(command_line worker-should-pull)" \
        || fail "Could not read WorkerShouldPullNewJobs. The site has not been touched."

    case "$was_pulling" in
        true)
            command_line worker-stop-pulling \
                || fail "Could not turn WorkerShouldPullNewJobs off. The site has not been touched."
            RESTORE_WORKER_PULLING=true
            ;;
        false)
            log "The worker was already turned off, and will be left off after this deploy."
            ;;
        *)
            fail "Expected true or false for WorkerShouldPullNewJobs, got '$was_pulling'. The site has not been touched."
            ;;
    esac

    command_line worker-wait-idle \
        || fail "A job was still running when the wait ran out. The site has not been touched. Deploy again once it has finished, or with skip_drain to kill it."
    log "The worker is idle."
}

raise_maintenance_page() {
    "$MAINTENANCE" install
    "$MAINTENANCE" on
}

stop_containers() {
    log "Stopping containers"
    compose down --remove-orphans
}

run_migrations() {
    if [ "${FC_SKIP_MIGRATIONS:-false}" = "true" ]; then
        log "Skipping migrations (FC_SKIP_MIGRATIONS=true)"
        return 0
    fi

    log "Running database migrator"
    if ! compose run --rm database-updater; then
        # A failed migration may be half-applied (MySQL DDL is not transactional), so starting
        # the old code is no safer than staying down. Fix forward or restore the snapshot.
        printf '\n' >&2
        echo "Database migration failed. The site has been left stopped on purpose." >&2
        rollback_hint
        exit 1
    fi
    log "Migrations complete"
}

# .env names the new release only from here on. A deploy that stops earlier leaves it naming the
# release that is live, so a hand-typed `docker compose up` never starts new images on an
# unmigrated database.
mark_release_live() {
    echo "deployed_at=$(date -u +%Y-%m-%dT%H:%M:%SZ)" >> "$RELEASE_FILE"

    if grep -qE '^IMAGE_TAG=' "$ENV_FILE"; then
        sed -i -E "s|^IMAGE_TAG=.*|IMAGE_TAG=$IMAGE_TAG|" "$ENV_FILE"
    else
        echo "IMAGE_TAG=$IMAGE_TAG" >> "$ENV_FILE"
    fi
}

start_containers() {
    log "Starting web, discord-bot and worker"
    compose up -d web discord-bot worker
}

wait_for_web() {
    log "Waiting for $HEALTH_URL"
    for _ in $(seq 1 60); do
        sleep 2
        if curl -fsS --max-time 5 "$HEALTH_URL" >/dev/null 2>&1; then
            log "Healthy."
            return 0
        fi
        if ! service_is_running web; then
            break
        fi
    done

    echo "The web container did not become healthy. Last 50 log lines:" >&2
    compose logs --tail 50 web >&2 || true
    echo "Discord bot, last 20 lines:" >&2
    compose logs --tail 20 discord-bot >&2 || true
    echo "Worker, last 20 lines:" >&2
    compose logs --tail 20 worker >&2 || true
    rollback_hint
    exit 1
}

lower_maintenance_page() {
    "$MAINTENANCE" off
}

# A broken worker takes every scheduled job with it while the site looks healthy, so this fails
# the deploy. It runs after the maintenance page comes down because the site itself is fine.
check_worker() {
    log "Checking the worker"
    # Two minutes. The healthcheck allows 30 seconds to start and probes every 15.
    if wait_for_healthy worker 40; then
        log "Worker is healthy."
        return 0
    fi

    echo "The worker did not become healthy (Docker reports: $(service_health worker)). Last 50 log lines:" >&2
    compose logs --tail 50 worker >&2 || true
    cat >&2 <<WORKER

  ======================================================================
  WARNING: the site is up, but the job worker is not healthy.

  Nothing scheduled will happen until it is: no public bidding emails,
  no releasing-this-week posts, no trade expiry, no Patreon sync, and
  no admin console button will ever leave "Queued".

  Fix the cause above, then: cd $APP_ROOT && docker compose up -d worker
  ======================================================================

WORKER
    exit 1
}

# The bot only answers slash commands, so it being down is worth a warning, not a failed deploy.
check_discord_bot() {
    log "Checking the Discord bot"
    if wait_for_healthy discord-bot 20; then
        log "Discord bot is healthy."
        return 0
    fi

    echo "WARNING: the Discord bot did not become healthy (Docker reports: $(service_health discord-bot)). Slash commands will not work. Last 20 log lines:" >&2
    compose logs --tail 20 discord-bot >&2 || true
}

prune() {
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
}

# ------------------------------------------------------------------------------------------
# Helpers
# ------------------------------------------------------------------------------------------

log() {
    printf '[%s] %s\n' "$(date -u +%H:%M:%S)" "$*"
}

fail() {
    printf '[%s] ERROR: %s\n' "$(date -u +%H:%M:%S)" "$*" >&2
    exit 1
}

# An explicit --env-file, because whether the shell environment or .env wins has differed
# between compose versions.
compose() {
    docker compose --project-directory "$APP_ROOT" --env-file "$DEPLOY_ENV_FILE" -f "$COMPOSE_FILE" "$@"
}

# A command's answer is the only thing it writes to stdout. -T because there is no terminal
# under SSM, and with one the answer and the logs would be merged.
command_line() {
    compose run --rm -T command-line "$@"
}

# Read rather than sourced: sourcing would run whatever .env contains and overwrite IMAGE_TAG.
env_value() {
    grep -E "^$1=" "$ENV_FILE" | tail -1 | cut -d= -f2- || true
}

# Leaves a worker that someone turned off in the admin console exactly as it was found: the
# drain only sets RESTORE_WORKER_PULLING if pulling was on.
restore_worker_pulling() {
    if [ "$RESTORE_WORKER_PULLING" != "true" ]; then
        return 0
    fi

    RESTORE_WORKER_PULLING=false
    log "Turning job pulling back on"
    if command_line worker-start-pulling; then
        return 0
    fi

    cat >&2 <<PULLING

  ======================================================================
  WARNING: could not turn WorkerShouldPullNewJobs back on.

  This deploy turned it off to drain the worker. Until it is on again
  the worker runs nothing. Use "Turn On Worker" in the admin console, or:

      cd $APP_ROOT && docker compose run --rm -T command-line worker-start-pulling
  ======================================================================

PULLING
    return 1
}

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
  cd $APP_ROOT && docker compose up -d web discord-bot worker && $MAINTENANCE off
HINT
    fi
}

# `docker compose ps --status` is too new to rely on across compose plugin versions.
service_is_running() {
    local container_id
    container_id="$(compose ps -q "$1" 2>/dev/null || true)"
    [ -n "$container_id" ] || return 1
    [ "$(docker inspect -f '{{.State.Running}}' "$container_id" 2>/dev/null || echo false)" = "true" ]
}

# starting, healthy or unhealthy; "none" if there is no container or its image has no healthcheck.
service_health() {
    local container_id
    container_id="$(compose ps -q "$1" 2>/dev/null || true)"
    [ -n "$container_id" ] || { echo none; return 0; }
    docker inspect -f '{{if .State.Health}}{{.State.Health.Status}}{{else}}none{{end}}' "$container_id" 2>/dev/null || echo none
}

# Gives up early on a crash loop: a freshly created container has a restart count of 0, so
# anything higher means it has already died at least once.
wait_for_healthy() {
    local service="$1" attempts="$2" container_id restarts
    for _ in $(seq 1 "$attempts"); do
        sleep 3
        if [ "$(service_health "$service")" = "healthy" ]; then
            return 0
        fi
        container_id="$(compose ps -q "$service" 2>/dev/null || true)"
        restarts="$(docker inspect -f '{{.RestartCount}}' "$container_id" 2>/dev/null || echo 0)"
        if [ "$restarts" != "0" ]; then
            echo "The $service container has restarted $restarts time(s) since it was created — it is crash looping." >&2
            return 1
        fi
    done
    return 1
}

main "$@"
