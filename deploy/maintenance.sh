#!/usr/bin/env bash
#
# Maintenance page control. Ships inside the release bundle alongside the page itself, and
# deploy.sh installs both to fixed paths, so on the instance this is:
#
#   sudo /opt/fantasy-critic/maintenance.sh on
#   sudo /opt/fantasy-critic/maintenance.sh off
#   /opt/fantasy-critic/maintenance.sh status
#
# `install` copies the page from whatever directory this script is sitting in, so the two
# files have to stay together.
#
# deploy.sh calls `install` and `on` before stopping the service and `off` once the new
# release reports healthy. The other reason to use it is planned work: turn the page on by
# hand, do the work, turn it off.
#
# nginx tests the flag file per request (infrastructure/nginx_maintenance.conf), so nothing
# here reloads or even talks to nginx.

set -euo pipefail

readonly FLAG=/var/www/maintenance.on
readonly PAGE_DIR=/var/www/maintenance
readonly PAGE="$PAGE_DIR/maintenance.html"
readonly SNIPPET=/etc/nginx/maintenance.conf

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
readonly SCRIPT_DIR

usage() {
    cat >&2 <<USAGE
Usage: maintenance.sh <command>

  install   Copy the page from this release to $PAGE (root)
  on        Show the maintenance page to everyone (root)
  off       Stop showing it (root)
  status    Report whether the page is on, and whether it is installed
USAGE
    exit 64
}

require_root() {
    if [ "$(id -u)" -ne 0 ]; then
        echo "ERROR: '$1' needs root." >&2
        exit 1
    fi
}

case "${1:-}" in
    install)
        require_root install
        source_page="$SCRIPT_DIR/maintenance.html"
        if [ ! -f "$source_page" ]; then
            echo "ERROR: no maintenance.html next to this script ($SCRIPT_DIR)." >&2
            exit 1
        fi
        mkdir -p "$PAGE_DIR"
        # Via a temp file in the same directory so a request landing mid-copy can never see a
        # half-written page.
        install -m 644 "$source_page" "$PAGE.tmp"
        mv -f "$PAGE.tmp" "$PAGE"
        echo "Installed $PAGE"
        ;;

    on)
        require_root on
        if [ ! -f "$PAGE" ]; then
            echo "WARNING: $PAGE does not exist; nginx will serve a bare 503." >&2
        fi
        mkdir -p "$(dirname "$FLAG")"
        touch "$FLAG"
        echo "Maintenance page is ON."
        ;;

    off)
        require_root off
        rm -f "$FLAG"
        echo "Maintenance page is OFF."
        ;;

    status)
        if [ -f "$FLAG" ]; then
            echo "Maintenance page: ON (flag $FLAG, set $(date -u -r "$FLAG" +%Y-%m-%dT%H:%M:%SZ))"
        else
            echo "Maintenance page: OFF"
        fi
        if [ -f "$PAGE" ]; then
            echo "Page installed:    $PAGE"
        else
            echo "Page installed:    NO — $PAGE is missing, nginx would serve a bare 503"
        fi

        # The half that is easiest to be missing without noticing: the file can be present and
        # still not loaded by any server block, in which case nothing above has any effect.
        if ! command -v nginx >/dev/null 2>&1; then
            echo "nginx include:     unknown (no nginx on PATH)"
        elif [ "$(id -u)" -ne 0 ]; then
            echo "nginx include:     unknown (re-run with sudo; reading the config needs root)"
        elif nginx -T 2>/dev/null | grep -q "configuration file $SNIPPET"; then
            echo "nginx include:     $SNIPPET"
        else
            echo "nginx include:     NO — nothing includes $SNIPPET, so neither the flag above"
            echo "                   nor a stopped app will show this page"
        fi
        ;;

    *)
        usage
        ;;
esac
