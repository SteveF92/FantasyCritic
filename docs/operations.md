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

Three long-running services: **`web`**, **`discord-bot`** and **`worker`**. Two more are one-off
jobs that never run on their own: **`database-updater`**, behind the `migrate` profile, and
**`command-line`**, behind `tools`, which deploys use to talk to the database.

`worker` is the job system: it owns the schedule, so every cron job and every admin console
button runs there rather than in `web`. Stopping it does not affect the site, and the public
site gives no sign that it is down — it just quietly stops doing anything on a timer. The
**Services** panel in the admin console is where it shows.

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

The Discord bot and the worker are the same with their own names in place of `web`:

```bash
cd /opt/fantasy-critic && sudo docker compose restart discord-bot
```

```bash
cd /opt/fantasy-critic && sudo docker compose restart worker
```

Restarting the worker is safe at any time it is not mid-job: it re-seeds its schedule from 30
minutes back, so a slot that fell inside the restart is still attempted, and the unique
constraint stops it enqueueing a job twice. A job that was *running* when it went down is a
different matter — see the troubleshooting table. To be sure it is not mid-job, turn it off
first and wait for **Off**: see [The worker](#the-worker) below.

All at once:

```bash
cd /opt/fantasy-critic && sudo docker compose restart web discord-bot worker
```

After editing `docker-compose.yaml` or `.env`, recreate rather than restart — a restart reuses
the old container with the old settings:

```bash
cd /opt/fantasy-critic && sudo docker compose up -d --force-recreate web discord-bot worker
```

Two things worth knowing:

- **Name the services.** A bare `docker compose up -d` is only safe because the migrator sits
  behind a profile; naming them is the habit that keeps it safe.
- **A container you stopped by hand stays stopped** across a reboot or a Docker restart. That
  is what `restart: unless-stopped` means, and it is what you want during planned work — but
  nothing will bring it back for you.

Stopping `web` shows the maintenance page by itself: nginx gets a connection refused and maps
it to the branded page under a 503.

## The worker

The admin console's **Services** panel shows the worker and the Discord bot. It has a Refresh
button and no timer. For the worker it shows one of:

| State | Means |
|---|---|
| **Running** | Healthy and pulling jobs. |
| **Draining** | Turned off, but a job is still Running or Cancelling. |
| **Off** | Turned off and nothing is running. Safe to restart or stop the container. |
| **Unhealthy** | It answered, but its job runner has not read the database for over a minute. |
| **Unreachable** | It did not answer at all: the container is down or still starting. |

**Turn Off Worker** does not stop the container. It clears one flag,
`WorkerShouldPullNewJobs` in `tbl_meta_systemwidesettings`, and the worker reads that flag
before every poll. So it finishes the job it has, then sits idle with its health endpoint
still answering — which is how the panel can tell Off from down. While it is off the worker
logs "Intentionally not running new jobs because WorkerShouldPullNewJobs is FALSE" every five
seconds to `log-jobrunner.txt`, so the log says why nothing is happening.

The scheduler is not affected. Cron jobs keep being queued while the worker is off, and the
whole backlog runs, oldest first, when it is turned back on. After a long spell off that can
mean the same ten-minute job many times over.

Draining and Off are worked out from the job table, not from what the worker reports. A row
stuck on Running after the worker was killed therefore reads as Draining forever — see the
troubleshooting table.

The same thing from the box, without the console:

```bash
cd /opt/fantasy-critic && sudo docker compose run --rm -T command-line worker-should-pull
```

```bash
cd /opt/fantasy-critic && sudo docker compose run --rm -T command-line worker-stop-pulling
```

```bash
cd /opt/fantasy-critic && sudo docker compose run --rm -T command-line worker-wait-idle
```

```bash
cd /opt/fantasy-critic && sudo docker compose run --rm -T command-line worker-start-pulling
```

`worker-should-pull` prints `true` or `false`. `worker-wait-idle` waits up to 30 minutes
(`--timeout-minutes <n>` changes that) and exits non-zero if a job is still running at the
end. These are what a deploy runs.

## Logs

```bash
cd /opt/fantasy-critic && sudo docker compose logs -f web
```

```bash
cd /opt/fantasy-critic && sudo docker compose logs -f discord-bot
```

```bash
cd /opt/fantasy-critic && sudo docker compose logs -f worker
```

```bash
sudo tail -f /var/log/fantasy-critic/web/log-my.txt
```

`log-my.txt` is filtered to Fantasy Critic's own code, which is usually what you want.
`log-all.txt` and `log-warning.txt` sit beside it. The worker writes to
`/var/log/fantasy-critic/worker/`, where its three loops — `Scheduler`, `JobRunner` and
`Canceller` — also get a file each, so following a job does not mean reading the scheduler's
heartbeat around it. The deploy's `command-line` runs write to
`/var/log/fantasy-critic/commandline/`. Everything also goes to Grafana Loki.

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

The worker and the Discord bot each answer `GET /health` on port 8080 too, but only inside the
compose network — neither publishes a port. Docker probes them every 15 seconds, so the quick
look is the `(healthy)` or `(unhealthy)` beside each in:

```bash
cd /opt/fantasy-critic && sudo docker compose ps
```

For the detail behind it — the same JSON the admin console's Services panel shows:

```bash
cd /opt/fantasy-critic && sudo docker compose exec worker curl -sS http://localhost:8080/health
```

```bash
cd /opt/fantasy-critic && sudo docker compose exec discord-bot curl -sS http://localhost:8080/health
```

The worker is healthy when its job runner has read the database within the last minute, or is
inside a job; the process merely being up is not enough. The bot is healthy when its gateway
connection is up. `unhealthy` is only a label: on this host nothing restarts a container for
it. A process that *exits* is restarted, as before.

## Deploying

Actions → **Deploy** → Run workflow. Pick the ref and the environment deliberately; the
selector defaults to the repository's default branch, and the workflow that runs is the one on
the ref you choose.

A deploy does not kill a running job. Before it touches anything it turns the worker off,
exactly as the console button does, and waits up to 30 minutes for the job it has to finish.
The site stays up for all of that; the maintenance page only goes up afterwards. If the job
is still going when the wait runs out, the deploy stops there with nothing changed and turns
the worker back on. It also turns it back on once the new release is in — unless it was
already off when the deploy began, in which case it is left off.

Options worth knowing:

- **`skip_drain`** — do not wait; a running job is killed. For a job that is hung, for a row
  stuck on Running that the wait would otherwise sit on for the full 30 minutes, and for the
  first deploy of the job system to an instance, which has no job table to read yet.
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

Nothing to do. Docker starts at boot and `restart: unless-stopped` brings `web`,
`discord-bot` and `worker` back. The migrator does not run. The exception is a container you had stopped by
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
| Nothing scheduled has happened, site fine | Look at the admin console's Services panel. **Off** means it was turned off — by someone, or by a deploy that could not turn it back on, which the deploy output says loudly. Turn On Worker fixes it. **Unreachable** means `worker` is down: `docker compose ps`, then `docker compose logs worker`. |
| Admin console button sticks on "Queued" | Same thing: nothing is consuming the queue. The job will run whenever the worker comes back. |
| A job is stuck "Running" and nothing is happening | The worker was killed mid-job. Nothing sweeps those rows yet, and cancelling from the console will not settle one either — the canceller only trips tokens a live worker holds. Until the sweep is built, finish the row by hand in `tbl_job`. While it sits there the console's button for that job type refuses, a turned-off worker reads as Draining rather than Off, and a deploy waits its full 30 minutes on it and then stops — use `skip_drain`. Cron runs of the job type are unaffected. |
| Deploy stops at "Draining the job worker" | "A job was still running when the wait ran out": a real long job, or a stuck Running row (above). Nothing was touched and the worker is back on. Redeploy once the job is done, or with `skip_drain`. Any other error there means the `command-line` image could not reach the database — its log lines are in the deploy output's stderr. |
| Worker or bot shows `(unhealthy)` in `docker compose ps` | `docker compose exec <service> curl -sS http://localhost:8080/health` says why. For the worker it is nearly always the database being unreachable; it recovers by itself when the database does. |
| Containers cannot start, Secrets Manager errors | The IMDSv2 hop limit has been reset to 1. See step 3 of the setup guide. |
| Nothing in `/var/log/fantasy-critic` | The directory is not owned by uid 1654. |
| Admin console shows no release info | `/opt/fantasy-critic/RELEASE` is missing, or Docker created it as a directory. |
