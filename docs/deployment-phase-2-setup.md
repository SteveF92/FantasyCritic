# Phase 2 setup: the maintenance page

One-time setup for
[Phase 2](deployment-modernization-roadmap.md#phase-2-maintenance-page). The repository half
is already committed; this is the nginx edit on each box.

Do it on **beta first**, then production.

Because this is a per-instance edit rather than anything the pipeline carries, an instance that was powered off when this went out silently does not have it — and the symptom only shows up the next time something stops the app. Phase 3's runbook has a check for exactly that.

---

## What changes

| Before | After |
|---|---|
| A deploy shows nginx's bad gateway for the stop/migrate/start window | A branded page under a 503 |
| Planned maintenance means the site is simply broken while you work | `maintenance.sh on`, do the work, `maintenance.sh off` |

Nothing here touches the application. The page is static, self-contained, and served by
nginx from disk, which is the point: it has to work when the app does not.

---

## The pieces

| File | Where it lands | What it is |
|---|---|---|
| [`infrastructure/maintenance.html`](../infrastructure/maintenance.html) | `/var/www/maintenance/maintenance.html` | The page. Installed from the release bundle on every deploy. |
| [`infrastructure/nginx_maintenance.conf`](../infrastructure/nginx_maintenance.conf) | `/etc/nginx/maintenance.conf` | Included by the server block. The only manual step. |
| [`deploy/maintenance.sh`](../deploy/maintenance.sh) | ships in each release bundle | `install` / `on` / `off` / `status`. |
| — | `/var/www/maintenance.on` | The flag. Its existence is the whole switch. |

The flag is tested per request, so turning the page on or off takes effect immediately and
never reloads nginx.

---

## 1. Deploy first

Run the Deploy workflow once with this change in it, **before** touching nginx.

That order matters. The release bundle is what carries `maintenance.sh` and the page onto the
box, and `deploy.sh` installs the page to `/var/www/maintenance/maintenance.html` as part of
the run. Doing nginx first would mean a window where the include points at a page that is not
there yet, and users would get a bare 503 instead of the bad gateway they get today.

Nothing user-visible changes on this deploy: the flag gets raised and lowered, but no nginx
config reads it yet, so the window looks exactly like it does now.

Afterwards, confirm the page landed:

```bash
/opt/fantasy-critic/maintenance.sh status
```

## 2. Install the nginx snippet

As of Phase 3 `deploy.sh` installs this file from the release bundle on every deploy, so on
any instance that has had a containerized deploy it is already there and already matches the
repository. Do this by hand only on a box that has not.

```bash
sudo cp nginx_maintenance.conf /etc/nginx/maintenance.conf
```

(Copy it up with `scp`, or paste it through `aws ssm start-session --target <INSTANCE_ID>`.)

## 3. Include it from the server block

The live config is certbot-managed, so edit it rather than replacing it. Find the TLS server
block for `fantasycritic.games` — `sudo nginx -T | grep -n 'server_name\|listen'` will point
at the file — and add one line just inside it, above the `location` blocks:

```nginx
server {
    listen 443 ssl;
    server_name fantasycritic.games *.fantasycritic.games;

    include /etc/nginx/maintenance.conf;      # <-- add this

    location ~* /api/admin/ {
        ...
```

Add it to the plain `:80` block as well if that one proxies rather than redirecting to HTTPS.

[`infrastructure/nginx_nocertbot.txt`](../infrastructure/nginx_nocertbot.txt), the pre-certbot
reference config in this repository, already has the line in the right place.

```bash
sudo nginx -t && sudo systemctl reload nginx
```

## 4. Test it

With the site up:

```bash
sudo /opt/fantasy-critic/maintenance.sh on
curl -si https://www.fantasycritic.games/ | head -5   # 503, Retry-After: 300
```

Load it in a browser too — that is the only way to catch a page that renders wrong. Then:

```bash
sudo /opt/fantasy-critic/maintenance.sh off
curl -si https://www.fantasycritic.games/health | head -1   # 200
```

The bad gateway path is covered by the next deploy; watch the site during the window.

---

## Editing the wording

Edit `infrastructure/maintenance.html` and deploy. The page travels in the release bundle and
`deploy.sh` reinstalls it, so it can never drift from what is in the repository.

The file is a single self-contained document — no stylesheets, scripts, fonts or images are
fetched over the network, because everything it would fetch is exactly what is unavailable
when the page is being shown. The logo and background texture are data URIs in a `<style>`
block at the very bottom; everything above that block is meant to be edited by hand.

To preview a change, open the file directly in a browser. There is nothing to build.

---

## How it behaves

| Situation | Response |
|---|---|
| App down, flag off (a crash, or the deploy window before the flag is raised) | 503 + the page. `error_page 502 503 =503` rewrites nginx's bad gateway. |
| Flag on | 503 + the page, for every path including `/health`. |
| Flag off, app up | Normal proxying. |
| Flag on, page missing | nginx's built-in 503 body. `maintenance.sh on` warns when it sees this. |
| App itself returns a 502 or 503 | Passed through untouched — there is no `proxy_intercept_errors`. |

`/health` is not exempt while the flag is on. That is deliberate: during a deploy the site
genuinely is not serving, and an uptime check that says otherwise is worth less than one that
occasionally reports a planned outage. Revisit if the alerting backlog item ever gets noisy.

## During a deploy

`deploy.sh` raises the page before stopping the service and lowers it only after the new
release answers `/health`. Every failure path leaves it raised — a failed deploy means the
site is down, and the page is a better thing to be showing than a bad gateway. The rollback
instructions the script prints end with the command to lower it.

---

## Carried into later phases

- **Phase 3 (Docker).** nginx stays on the host, so this is untouched.
- **Phase 5 (ALB).** The roadmap moves the page to an ALB fixed-response listener rule, which
  is where the maintenance concept goes once nginx leaves. `maintenance.sh` becomes two AWS
  CLI calls that enable and disable that rule. Note that an ALB fixed response caps out at
  1024 bytes, so this page cannot be inlined into it: either the rule serves a tiny page and
  this one is kept for something else, or it redirects to the page on S3 — which is free once
  Phase 7c puts a bucket and a CloudFront distribution in front of the site anyway.
