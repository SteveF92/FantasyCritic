# Phase 3 setup: converting an instance to Docker

Everything that has to be true on an EC2 instance before its first containerized deploy, in
the order it has to be true. This is the AWS-console and SSH half of
[Phase 3](deployment-modernization-roadmap.md#phase-3-docker-on-the-box) and
[Phase 4a](deployment-modernization-roadmap.md#phase-4a-discord-bot-as-its-own-container); the
repository half is already committed.

Day-to-day commands after this is done live in [operations.md](operations.md).

**Part 1 is account-wide and happens once.** If beta is already running containers, skip to
Part 2.

| Placeholder | Meaning |
|---|---|
| `<ACCOUNT_ID>` | AWS account id |
| `<REGION>` | `us-east-1` |
| `<REGISTRY>` | `<ACCOUNT_ID>.dkr.ecr.<REGION>.amazonaws.com` |
| `<INSTANCE_ID>` | The EC2 instance being converted |

## What you end up with

Ubuntu, Docker, nginx, certbot and the SSM agent on the box, and nothing else. Three images
come from ECR: `web` and `discord-bot` run as services, `database-updater` runs as a one-off
job during deploys. nginx keeps terminating TLS and proxying to `127.0.0.1:5000`, exactly as
before.

---

# Part 1 — account-wide, once

## 1. ECR repositories

```bash
for repo in fantasycritic-web fantasycritic-database-updater fantasycritic-discord-bot; do aws ecr create-repository --repository-name "$repo" --region <REGION> --image-scanning-configuration scanOnPush=true --image-tag-mutability IMMUTABLE; done
```

`IMMUTABLE` means the tag recorded in `/opt/fantasy-critic/.env` always names the exact image
that was deployed. Beta and production share these repositories, so an image tested on beta
goes to production without being rebuilt.

Every deploy pushes three images of a few hundred megabytes, so add a lifecycle policy. Save
this as `ecr-lifecycle.json`:

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
for repo in fantasycritic-web fantasycritic-database-updater fantasycritic-discord-bot; do aws ecr put-lifecycle-policy --repository-name "$repo" --lifecycle-policy-text file://ecr-lifecycle.json --region <REGION>; done
```

## 2. IAM

**The deploy role** — the one GitHub assumes via OIDC — needs to push. If beta and production
use separate roles, both need this:

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

**Each instance role** needs to pull — attach the managed policy
`AmazonEC2ContainerRegistryReadOnly`.

Nothing changes in GitHub. The registry address comes from `aws-actions/amazon-ecr-login`,
which derives it from the assumed role's account.

---

# Part 2 — on the instance

## 3. Raise the IMDSv2 hop limit

Containers reach the instance IAM role through the instance metadata service. At the default
hop limit of 1, a request from inside a container has already spent its hop leaving the
container network and is dropped. Since all three processes load their configuration from
Secrets Manager, every one of them then fails at startup. This is the most likely reason for a
first deploy to fail.

```bash
aws ec2 modify-instance-metadata-options --instance-id <INSTANCE_ID> --http-put-response-hop-limit 2 --http-tokens required --http-endpoint enabled --region <REGION>
```

Verify from inside a container on the box. This must print the instance role's assumed-role
ARN rather than timing out:

```bash
docker run --rm public.ecr.aws/aws-cli/aws-cli sts get-caller-identity
```

## 4. Install Docker

```bash
sudo apt-get update && sudo apt-get install -y ca-certificates curl
```

```bash
sudo install -m 0755 -d /etc/apt/keyrings && sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc && sudo chmod a+r /etc/apt/keyrings/docker.asc
```

```bash
echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
```

```bash
sudo apt-get update && sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
```

```bash
sudo systemctl enable --now docker
```

`systemctl enable docker` is what brings the site back after a reboot — there is no
application systemd unit.

Cap the log driver so container logs cannot fill the disk. Create `/etc/docker/daemon.json`
containing:

```json
{
  "log-driver": "json-file",
  "log-opts": { "max-size": "20m", "max-file": "3" }
}
```

```bash
sudo systemctl restart docker
```

## 5. Create `/opt/fantasy-critic/.env`

`deploy.sh` writes everything else in this directory. Only the env file has to exist first,
because it holds the two things the pipeline cannot know: which environment this box is, and
which registry to pull from.

```bash
sudo mkdir -p /opt/fantasy-critic/releases
```

Create `/opt/fantasy-critic/.env` containing:

```
ASPNETCORE_ENVIRONMENT=Production
AWS_REGION=us-east-1
ECR_REGISTRY=<REGISTRY>
IMAGE_TAG=
```

```bash
sudo chmod 600 /opt/fantasy-critic/.env
```

Beta is identical except `ASPNETCORE_ENVIRONMENT=Staging`, which is what selects the `beta`
secret in Secrets Manager.

Leave `IMAGE_TAG` empty. Every deploy rewrites it, and a rollback reads it to name the
previous release.

## 6. Log directory ownership

The containers run as the .NET images' non-root user, uid **1654**, and Serilog writes rolling
files to `/var/log/fantasy-critic`, which the compose file bind-mounts. A bind mount takes the
host's ownership, so without this the file sinks write nothing:

```bash
sudo mkdir -p /var/log/fantasy-critic && sudo chown -R 1654 /var/log/fantasy-critic
```

Loki receives everything regardless; these files are the local copy you read over SSH.

## 7. Add the nginx `include` line

`deploy.sh` installs `/etc/nginx/maintenance.conf` from the release bundle, validates it with
`nginx -t`, and reloads nginx when it changes. The one thing it cannot do is edit the
certbot-managed site file, so add a single line there by hand.

Open the site config. It is in `/etc/nginx/sites-available/`, named after whatever is linked
from `/etc/nginx/sites-enabled/` — or in `/etc/nginx/conf.d/` if `sites-enabled` is empty.
Find this line:

```nginx
    listen 443 ssl http2; # managed by Certbot
```

and add one line directly underneath it:

```nginx
    include /etc/nginx/maintenance.conf;
```

It only has to be inside that same `server { ... }` block, which it will be. Leave the port-80
server block alone if it only redirects to HTTPS; if it proxies to the app, give it the same
line.

```bash
sudo nginx -t && sudo systemctl reload nginx
```

If `nginx -t` fails, the line landed in the wrong place. Nothing changes on the live site until
the reload, so a bad edit cannot take the site down.

Until this line exists, a stopped `web` container serves a bad gateway instead of the
maintenance page, and the maintenance flag does nothing at all. `deploy.sh` prints a loud
warning on every deploy while it is missing.

## 8. Retire the systemd unit

Do this **immediately before** the first containerized deploy — until then it is what serves
the site.

```bash
sudo systemctl disable --now fantasy-critic.service
```

```bash
sudo rm /etc/systemd/system/fantasy-critic.service && sudo systemctl daemon-reload
```

If the first deploy goes badly, the unit file is recoverable with
`git show 65b173fd8:infrastructure/fantasy-critic.service`, and the pre-Phase-3 release
directories are still under `/opt/fantasy-critic/releases/`.

---

# Part 3 — deploy and verify

## 9. Deploy

```
Actions → Deploy → Run workflow → Use workflow from: <ref> → environment: production
```

The branch selector defaults to the repository's default branch, and the workflow that runs is
the one on the ref you pick. Choose it deliberately.

Three lines in the run are worth watching:

| Line | Proves |
|---|---|
| `Logging in to <REGISTRY>` | the instance role can pull |
| `Running database migrator` | Secrets Manager works from inside a container, so step 3 took effect |
| `Healthy.` | `web` answered `127.0.0.1:5000/health` |

## 10. Verify

```bash
cd /opt/fantasy-critic && sudo docker compose ps
```

`web` and `discord-bot` up. `database-updater` will not be listed — it is a job, not a service.

```bash
sudo /opt/fantasy-critic/maintenance.sh status
```

All three lines should be healthy, in particular `nginx include: /etc/nginx/maintenance.conf`.

```bash
curl -sS -o /dev/null -w 'ready=%{http_code}\n' http://127.0.0.1:5000/health/ready
```

`200` proves the database connection, not merely that the process is serving.

Then load the site in a browser, run a Discord slash command and confirm it is answered exactly
once, and confirm a push notification still arrives — those come from the `web` container, not
the bot.

## 11. Strip the box

Once the containerized deploy has held for a few days:

```bash
sudo apt-get purge -y 'dotnet*' 'aspnetcore*' && sudo apt-get autoremove -y
```

```bash
sudo find /opt/fantasy-critic/releases -maxdepth 2 -name web -type d -printf '%h\n' | xargs -r sudo rm -rf
```

The second command removes pre-Phase-3 release directories, which held a few hundred megabytes
each. Node, git and the NSwag tool should already be gone.

---

## Deferred to later phases

- **nginx and certbot stay on the host.** They leave in Phase 5, when the ALB terminates TLS.
- **The maintenance page stays an nginx flag file.** It becomes an ALB listener rule in Phase 5.
- **`/opt/fantasy-critic/.env` stays hand-written.** Phase 5's launch template writes it from
  user-data; Phase 6 drops it for task definitions.
- **The instance is still hand-configured.** Everything above is what Phase 5 turns into
  Terraform.
