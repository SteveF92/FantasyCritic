# Operations

Running Fantasy Critic day to day, once an instance has been through
[the Phase 3 setup](deployment-phase-3-setup.md).

## Where things live

Everything is under `/opt/fantasy-critic`, which is where Compose finds both the stack
definition and its variables. Run the commands below from that directory.

| Path | What it is |
|---|---|
| `docker-compose.yaml` | The stack. Reinstalled from the bundle by every deploy. |
| `.env` | Environment name, AWS region, ECR registry, and the live `IMAGE_TAG`. Root-only. |
| `RELEASE` | Which release is live. Bind-mounted into `web` and shown in the admin console. |
| `maintenance.sh` / `maintenance.html` | Maintenance page control, at a fixed path. |
| `releases/<id>/` | Past release bundles, kept so any of them can redeploy itself. |
| `/var/log/fantasy-critic/` | Serilog rolling files, owned by uid 1654. |

Two long-running services: **`web`** and **`discord-bot`**. A third, **`database-updater`**, is
a one-off job behind the `migrate` profile — it never runs on its own.

## Everyday commands

What is running:

```bash
cd /opt/fantasy-critic && sudo docker compose ps
```

Stop the site:

```bash
cd /opt/fantasy-critic && sudo docker compose stop web
```

Start it again:

```bash
cd /opt/fantasy-critic && sudo docker compose start web
```

Restart it:

```bash
cd /opt/fantasy-critic && sudo docker compose restart web
```

The Discord bot is the same with `discord-bot` in place of `web`:

```bash
cd /opt/fantasy-critic && sudo docker compose restart discord-bot
```

Both at once:

```bash
cd /opt/fantasy-critic && sudo docker compose restart web discord-bot
```

After editing `docker-compose.yaml` or `.env`, recreate rather than restart — a restart reuses
the old container with the old settings:

```bash
cd /opt/fantasy-critic && sudo docker compose up -d --force-recreate web discord-bot
```

Two things worth knowing:

- **Name the services.** A bare `docker compose up -d` is only safe because the migrator sits
  behind a profile; naming them is the habit that keeps it safe.
- **A container you stopped by hand stays stopped** across a reboot or a Docker restart. That
  is what `restart: unless-stopped` means, and it is what you want during planned work — but
  nothing will bring it back for you.

Stopping `web` shows the maintenance page by itself: nginx gets a connection refused and maps
it to the branded page under a 503.

## Logs

```bash
cd /opt/fantasy-critic && sudo docker compose logs -f web
```

```bash
cd /opt/fantasy-critic && sudo docker compose logs -f discord-bot
```

```bash
sudo tail -f /var/log/fantasy-critic/web/log-my.txt
```

`log-my.txt` is filtered to Fantasy Critic's own code, which is usually what you want.
`log-all.txt` and `log-warning.txt` sit beside it. Everything also goes to Grafana Loki.

## Health

```bash
curl -sS -o /dev/null -w 'health=%{http_code}\n' http://127.0.0.1:5000/health
```

```bash
curl -sS -o /dev/null -w 'ready=%{http_code}\n' http://127.0.0.1:5000/health/ready
```

`/health` proves the process is serving and runs no checks. `/health/ready` additionally proves
MySQL is reachable. Both answer over plain HTTP and bypass the HTTPS redirect, so they work
from the box without TLS.

## Deploying

Actions → **Deploy** → Run workflow. Pick the ref and the environment deliberately; the
selector defaults to the repository's default branch, and the workflow that runs is the one on
the ref you choose.

Options worth knowing:

- **`skip_migrations`** — for a code-only redeploy. A rollback should always use it.
- **`skip_snapshot`** — production takes a pre-migration RDS snapshot and waits for it, which
  is usually the slowest part of the run. Skip it only when you know the release has no
  migrations.

## Rolling back

Every release directory keeps its own `deploy.sh`, compose file and maintenance page, so
running an old one restores all of it:

```bash
ls /opt/fantasy-critic/releases
```

```bash
sudo FC_SKIP_MIGRATIONS=true /opt/fantasy-critic/releases/<previous-id>/deploy.sh
```

It works out its own image tag from the directory name, so nothing else needs passing in.

`FC_SKIP_MIGRATIONS=true` matters: the migrator only rolls forward. If the failed deploy ran
migrations, restore the pre-deploy RDS snapshot as well — the workflow run summary names it.

## Maintenance page

For planned work, put the page up before you start:

```bash
sudo /opt/fantasy-critic/maintenance.sh on
```

```bash
sudo /opt/fantasy-critic/maintenance.sh off
```

```bash
sudo /opt/fantasy-critic/maintenance.sh status
```

`status` reports all three things that have to be true:

```
Maintenance page: OFF
Page installed:    /var/www/maintenance/maintenance.html
nginx include:     /etc/nginx/maintenance.conf
```

Deploys raise and lower the page automatically, and any failed deploy deliberately leaves it
raised.

## Running migrations by hand

```bash
cd /opt/fantasy-critic && sudo docker compose run --rm database-updater
```

Take an RDS snapshot first, and stop `web` first — by this project's convention migrations are
not backward compatible, so the old code must not be running against the new schema.

## Which release is live

```bash
cat /opt/fantasy-critic/RELEASE
```

Gives the release id, commit, build and deploy times, and a link to the workflow run. The admin
console shows the same thing.

## After a reboot

Nothing to do. Docker starts at boot and `restart: unless-stopped` brings `web` and
`discord-bot` back. The migrator does not run. The exception is a container you had stopped by
hand, which stays stopped.

## Troubleshooting

| Symptom | Check |
|---|---|
| Site down, `/health` answers | The app is up but something after it is failing. Check `docker compose logs web` and nginx's error log. |
| `ERR_TOO_MANY_REDIRECTS` | nginx is not passing `X-Forwarded-Proto`, or is not being trusted. Confirm the site config sets it. |
| Bad gateway instead of the maintenance page | `sudo /opt/fantasy-critic/maintenance.sh status` — almost always the `include` line is missing from the site config. |
| Deploy reports success, site unusable | The deploy only probes `/health`, which bypasses the HTTPS redirect. Load a real page. |
| Slash commands answered twice | Two bot gateways on one token. Exactly one `discord-bot` container may run per bot token. |
| Slash commands not answered, notifications fine | `discord-bot` is down; `web` owns notifications. `docker compose ps`. |
| Bot restarting in a loop | It exits when no bot token is configured. Check `docker compose logs discord-bot` and the environment's secret. |
| Containers cannot start, Secrets Manager errors | The IMDSv2 hop limit has been reset to 1. See step 3 of the setup guide. |
| Nothing in `/var/log/fantasy-critic` | The directory is not owned by uid 1654. |
| Admin console shows no release info | `/opt/fantasy-critic/RELEASE` is missing, or Docker created it as a directory. |
