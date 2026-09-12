# Phase 3 setup: Docker on the box

One-time setup that has to happen **before** the first containerized deploy. This is the
AWS-console and SSH half of
[Phase 3](deployment-modernization-roadmap.md#phase-3-docker-on-the-box) and
[Phase 4a](deployment-modernization-roadmap.md#phase-4a-discord-bot-as-its-own-container);
the repository half is already committed.

Placeholders, continuing from the [Phase 1 runbook](deployment-phase-1-setup.md):

| Placeholder | Meaning |
|---|---|
| `<ACCOUNT_ID>` | AWS account id |
| `<REGION>` | `us-east-1` |
| `<REGISTRY>` | `<ACCOUNT_ID>.dkr.ecr.<REGION>.amazonaws.com` |
| `<BUCKET>` | The release bucket from Phase 1 |
| `<PROD_INSTANCE_ID>` / `<BETA_INSTANCE_ID>` | The two EC2 instance ids |

---

## What this replaces

| Before (Phase 1 and 2) | After |
|---|---|
| A ~300 MB self-contained publish bundle in S3 | Three images in ECR; the bundle is a few kilobytes |
| `systemd` runs `FantasyCritic.Web` from `current/web` | `docker compose up -d` runs `web` and `discord-bot` |
| Rollback is a symlink swap | Rollback is re-running the previous release's `deploy.sh` |
| The web process also runs the Discord command gateway | A separate `discord-bot` container does |
| The box needs the .NET runtime shipped inside every bundle | The box needs Docker and nothing language-specific |

nginx, certbot and the SSM deploy path are unchanged. The web container publishes
`127.0.0.1:5000`, which is exactly where `api_proxy.conf` was already pointing.

The Phase 2 maintenance page is unchanged **as code**, but its nginx half was a manual,
per-instance step, so confirm it was actually done on the instance you are converting — see
step 6 below. It is easy to assume "unchanged" means "present".

---

## 1. ECR repositories

Three, one per image. Scan-on-push is free and closes a backlog item.

```bash
for repo in fantasycritic-web fantasycritic-database-updater fantasycritic-discord-bot; do
  aws ecr create-repository \
    --repository-name "$repo" \
    --region <REGION> \
    --image-scanning-configuration scanOnPush=true \
    --image-tag-mutability IMMUTABLE
done
```

`IMMUTABLE` is deliberate: release tags are unique timestamps, and a tag that cannot be
overwritten means the tag recorded in `/opt/fantasy-critic/.env` always names the exact image
that was deployed.

Images are a few hundred megabytes each and every deploy pushes three, so each repository
needs a lifecycle policy or the bill grows quietly:

```json
{
  "rules": [
    {
      "rulePriority": 1,
      "description": "Keep the 15 most recent images",
      "selection": {
        "tagStatus": "any",
        "countType": "imageCountMoreThan",
        "countNumber": 15
      },
      "action": { "type": "expire" }
    }
  ]
}
```

```bash
for repo in fantasycritic-web fantasycritic-database-updater fantasycritic-discord-bot; do
  aws ecr put-lifecycle-policy --repository-name "$repo" \
    --lifecycle-policy-text file://ecr-lifecycle.json --region <REGION>
done
```

Beta and production share these repositories. Tags are unique per run, so nothing collides,
and an image that has been tested on beta can be deployed to production without rebuilding.

## 2. IAM

**The deploy role** (the one GitHub assumes via OIDC, from Phase 1) needs to push:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "EcrAuth",
      "Effect": "Allow",
      "Action": "ecr:GetAuthorizationToken",
      "Resource": "*"
    },
    {
      "Sid": "EcrPush",
      "Effect": "Allow",
      "Action": [
        "ecr:BatchCheckLayerAvailability",
        "ecr:CompleteLayerUpload",
        "ecr:InitiateLayerUpload",
        "ecr:PutImage",
        "ecr:UploadLayerPart",
        "ecr:BatchGetImage",
        "ecr:GetDownloadUrlForLayer"
      ],
      "Resource": [
        "arn:aws:ecr:<REGION>:<ACCOUNT_ID>:repository/fantasycritic-web",
        "arn:aws:ecr:<REGION>:<ACCOUNT_ID>:repository/fantasycritic-database-updater",
        "arn:aws:ecr:<REGION>:<ACCOUNT_ID>:repository/fantasycritic-discord-bot"
      ]
    }
  ]
}
```

**The instance role** needs to pull. `ecr:GetAuthorizationToken` on `*` plus
`ecr:BatchGetImage`, `ecr:GetDownloadUrlForLayer` and `ecr:BatchCheckLayerAvailability` on the
three repository ARNs. The managed policy `AmazonEC2ContainerRegistryReadOnly` covers this if
you would rather not write it out.

## 3. Raise the IMDSv2 hop limit — do not skip this

Containers reach the instance IAM role through the instance metadata service. With the default
PUT response hop limit of 1, a packet from inside a container has already used its hop getting
out of the container network, so the metadata request is dropped. **Every process would then
fail at startup**, because all three load their configuration from Secrets Manager.

```bash
aws ec2 modify-instance-metadata-options \
  --instance-id <PROD_INSTANCE_ID> \
  --http-put-response-hop-limit 2 \
  --http-tokens required \
  --http-endpoint enabled \
  --region <REGION>
```

Repeat for `<BETA_INSTANCE_ID>`. Verify from inside a container on the box:

```bash
docker run --rm public.ecr.aws/aws-cli/aws-cli sts get-caller-identity
```

That should print the instance role's assumed-role ARN. If it times out, the hop limit did not
take effect.

## 4. Install Docker on both instances

```bash
sudo apt-get update
sudo apt-get install -y ca-certificates curl
sudo install -m 0755 -d /etc/apt/keyrings
sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
sudo chmod a+r /etc/apt/keyrings/docker.asc
echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] \
  https://download.docker.com/linux/ubuntu $(. /etc/os-release && echo "$VERSION_CODENAME") stable" \
  | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
sudo apt-get update
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
sudo systemctl enable --now docker
```

`systemctl enable docker` is what makes `restart: unless-stopped` bring the site back after a
reboot. There is no application systemd unit any more.

Cap the log driver so container logs cannot fill the disk — Loki is the real destination:

```bash
sudo tee /etc/docker/daemon.json > /dev/null <<'JSON'
{
  "log-driver": "json-file",
  "log-opts": { "max-size": "20m", "max-file": "3" }
}
JSON
sudo systemctl restart docker
```

## 5. Prepare `/opt/fantasy-critic`

The compose file, `RELEASE`, and the release directories are written by `deploy.sh`. Only the
env file has to exist first, because it holds the things the pipeline does not know: which
environment this box is, and which registry to pull from.

**Production:**

```bash
sudo mkdir -p /opt/fantasy-critic/releases
sudo tee /opt/fantasy-critic/.env > /dev/null <<'ENV'
ASPNETCORE_ENVIRONMENT=Production
AWS_REGION=us-east-1
ECR_REGISTRY=<REGISTRY>
IMAGE_TAG=
ENV
sudo chmod 600 /opt/fantasy-critic/.env
```

**Beta** is identical except `ASPNETCORE_ENVIRONMENT=Staging` — which is what `Program.cs` maps
to the `beta` secret in Secrets Manager.

`IMAGE_TAG` is left empty; the first deploy fills it in and every deploy after that rewrites
it. It is also what a rollback reads to name the previous release.

## 6. Confirm the Phase 2 nginx include is on this instance

The maintenance page has two halves. `maintenance.sh` ships in every release and handles the
flag file and the HTML. The half that turns a stopped container into that page lives in nginx,
and installing it was a **manual, per-instance step** in
[Phase 2](deployment-phase-2-setup.md) — so an instance that was powered off when Phase 2 went
out does not have it, and nothing in this phase adds it.

Without it, stopping `web` serves a bare 502 instead of the page, and raising the flag does
nothing at all. Check:

```bash
sudo nginx -T 2>/dev/null | grep -n maintenance
```

Expect to see both the `include` line and the `location = /__maintenance.html` block. Nothing
back means it was never installed; do [Phase 2 steps 2 and 3](deployment-phase-2-setup.md) on
this instance, then:

```bash
sudo nginx -t && sudo systemctl reload nginx
```

Prove it end to end after the first deploy by stopping the site and expecting a **503**
carrying the branded page, not a 502:

```bash
cd /opt/fantasy-critic && sudo docker compose stop web
```

## 7. Log directory ownership

The containers run as the .NET images' non-root user (uid **1654**) and Serilog writes rolling
files to `/var/log/fantasy-critic`, which the compose file bind-mounts. A bind mount takes the
host's ownership, so without this the file sinks silently write nothing:

```bash
sudo mkdir -p /var/log/fantasy-critic
sudo chown -R 1654 /var/log/fantasy-critic
```

Loki still receives everything regardless; these files are the local copy you read over SSH
when Grafana is not to hand.

## 8. Retire the systemd unit

Do this **immediately before** the first containerized deploy, not earlier — until then the
Phase 1 path is still what is serving the site.

```bash
sudo systemctl disable --now fantasy-critic.service
sudo rm /etc/systemd/system/fantasy-critic.service
sudo systemctl daemon-reload
```

`infrastructure/fantasy-critic.service` has been deleted from the repository along with it. If
the first deploy goes badly and you need the old path back, the unit file is recoverable from
git history and the Phase 1 release directories are still on the box:

```bash
git show 65b173fd8:infrastructure/fantasy-critic.service
```

## 9. GitHub configuration

Phase 1 already set `AWS_ROLE_ARN`, `AWS_REGION`, `EC2_INSTANCE_ID`, `RELEASE_BUCKET`,
`RDS_INSTANCE_IDENTIFIER` and `SITE_URL` per environment. Phase 3 adds nothing: the registry
comes from `aws-actions/amazon-ecr-login`, which derives it from the assumed role's account.

## 10. First deploy

Deploy to **beta** first. It exercises the whole path — ECR pull, hop limit, log ownership, the
env file — against a box nobody is using.

```
Actions → Deploy → Run workflow → environment: beta
```

Then production. During the run, watch for:

- `Logging in to <REGISTRY>` — if this fails, the instance role cannot pull.
- `Running database migrator` — this is the container that proves Secrets Manager works from
  inside Docker, i.e. that step 3 took effect.
- `Healthy.` — the web container answered `127.0.0.1:5000/health`.

Afterwards, confirm both long-running containers are up and that only one bot is answering:

```bash
cd /opt/fantasy-critic && sudo docker compose ps
```

Run a slash command in Discord and check it is answered exactly once, and confirm a push
notification still arrives (they come from the web container, not the bot).

## 11. Strip the box

Once a containerized deploy has held for a few days:

```bash
# The .NET runtime, if it was ever installed as a package.
sudo apt-get purge -y 'dotnet*' 'aspnetcore*' && sudo apt-get autoremove -y

# Phase 1 release directories, which held ~300 MB each.
sudo find /opt/fantasy-critic/releases -maxdepth 2 -name web -type d -printf '%h\n' \
  | xargs -r sudo rm -rf
```

Node, git and the NSwag tool left with Phase 1 and should already be gone.

---

## Operating it afterwards

### Deploying

Unchanged: Actions → **Deploy** → Run workflow, pick the ref and the environment.

### Rolling back

Every release directory keeps its own `deploy.sh` and the compose file it shipped with, so a
rollback restores both:

```bash
ls /opt/fantasy-critic/releases
sudo FC_SKIP_MIGRATIONS=true /opt/fantasy-critic/releases/<previous-id>/deploy.sh
```

`FC_SKIP_MIGRATIONS=true` matters: the migrator only rolls forward. If the failed deploy ran
migrations, restore the pre-deploy RDS snapshot as well — the workflow names it in the run
summary.

### Starting and stopping

All of these run from `/opt/fantasy-critic`, where Compose finds `docker-compose.yaml` and
`.env`. The project is `fantasy-critic` and the two long-running services are `web` and
`discord-bot`.

```bash
cd /opt/fantasy-critic && sudo docker compose ps
```

```bash
cd /opt/fantasy-critic && sudo docker compose stop web
```

```bash
cd /opt/fantasy-critic && sudo docker compose start web
```

Substitute `discord-bot` for the bot, name both to act on both, or `restart` in place of
`stop`/`start`. To pick up an edited compose file or `.env`, recreate rather than restart:

```bash
cd /opt/fantasy-critic && sudo docker compose up -d --force-recreate web discord-bot
```

Two things to know:

- **Always name the services.** A bare `docker compose up -d` is safe only because
  `database-updater` sits behind a profile; naming them is the habit that keeps it safe if that
  ever changes.
- **`restart: unless-stopped` means a container you stopped by hand stays stopped** across a
  reboot or a Docker restart. That is what you want for planned work, but nothing will bring it
  back for you.

Stopping `web` shows the maintenance page on its own: nginx gets a connection refused and
`error_page 502 503 =503` maps it to the branded page. That only holds if step 6 was done.

### Reading logs

```bash
cd /opt/fantasy-critic && sudo docker compose logs -f web
```

```bash
cd /opt/fantasy-critic && sudo docker compose logs -f discord-bot
```

```bash
sudo tail -f /var/log/fantasy-critic/web/log-my.txt
```

### Forcing the maintenance page for planned work

The script and the page are installed at fixed paths by every deploy, so this needs no release
id:

```bash
sudo /opt/fantasy-critic/maintenance.sh on
```

```bash
sudo /opt/fantasy-critic/maintenance.sh off
```

```bash
sudo /opt/fantasy-critic/maintenance.sh status
```

`status` reports both whether the flag is raised and whether the page is actually installed,
which is the quicker of the two halves to get wrong.

### Running the migrator on its own

```bash
cd /opt/fantasy-critic && sudo docker compose run --rm database-updater
```

That works despite the `migrate` profile — explicitly targeting a profiled service enables its
profile. Take a snapshot first, and stop `web` first if the migration is not backward
compatible, which by the project's own convention it is not.

---

## Deferred to later phases

- **nginx and certbot stay on the host.** They leave in Phase 5, when the ALB terminates TLS.
- **The maintenance page stays an nginx flag file.** It becomes an ALB listener rule in Phase 5.
- **`/opt/fantasy-critic/.env` stays hand-written.** Phase 5's launch template writes it from
  user-data, and Phase 6 drops it entirely for task definitions.
- **The instance is still hand-configured.** Everything above is what Phase 5 turns into
  Terraform and user-data.
