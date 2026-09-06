# Phase 1 setup: GitHub Actions builds, the server only runs

Everything in this document is one-time setup that has to happen **before** the first run of
the Deploy workflow. It is the AWS-console and SSH half of
[Phase 1](deployment-modernization-roadmap.md#phase-1-github-actions-builds-the-server-only-runs);
the repository half is already committed.

Placeholders used throughout:

| Placeholder | Meaning |
|---|---|
| `<ACCOUNT_ID>` | AWS account id |
| `<REGION>` | `us-east-1` |
| `<BUCKET>` | New S3 bucket for release bundles, e.g. `fantasycritic-releases` |
| `<PROD_INSTANCE_ID>` / `<BETA_INSTANCE_ID>` | The two EC2 instance ids |
| `<PROD_DB_INSTANCE>` | Production RDS instance identifier |

---

## What this replaces

| Before | After |
|---|---|
| `git push origin main:production`, SSH in, `git pull`, `./linuxUpdateSite.sh` | Actions tab → **Deploy** → Run workflow |
| Server has .NET SDK, .NET runtime, Node, git, NSwag | Server has nginx, certbot, systemd, SSM agent, AWS CLI |
| Site lives in `/var/www/fantasy-critic`, overwritten in place | Site lives in `/opt/fantasy-critic/releases/<id>`, with a `current` symlink |
| Rollback means rebuilding an old commit on the box | Rollback is a symlink swap and a restart |
| Pre-deploy RDS snapshot depends on remembering | The pipeline takes one and waits for it |

`linuxUpdateSite.sh` is intentionally left in the repo as a fallback for the first few
deploys. Delete it once you trust the pipeline.

---

## 1. S3 bucket for release bundles

A self-contained publish of the web app is a few hundred megabytes, so the bundles need a
lifecycle rule or they will accumulate.

```bash
aws s3api create-bucket --bucket <BUCKET> --region us-east-1
aws s3api put-public-access-block --bucket <BUCKET> \
  --public-access-block-configuration \
  "BlockPublicAcls=true,IgnorePublicAcls=true,BlockPublicPolicy=true,RestrictPublicBuckets=true"
```

Lifecycle rule — expire anything under `releases/` after 30 days:

```json
{
  "Rules": [
    {
      "ID": "expire-release-bundles",
      "Status": "Enabled",
      "Filter": { "Prefix": "releases/" },
      "Expiration": { "Days": 30 },
      "AbortIncompleteMultipartUpload": { "DaysAfterInitiation": 3 }
    }
  ]
}
```

```bash
aws s3api put-bucket-lifecycle-configuration --bucket <BUCKET> \
  --lifecycle-configuration file://lifecycle.json
```

Cost at this retention is single-digit cents per month.

---

## 2. GitHub OIDC provider

This is what lets the workflow get temporary AWS credentials without any access key stored
in GitHub. One provider per account; if one already exists, skip to step 3.

**IAM → Identity providers → Add provider → OpenID Connect**

- Provider URL: `https://token.actions.githubusercontent.com`
- Audience: `sts.amazonaws.com`

---

## 3. The deploy role

**IAM → Roles → Create role → Custom trust policy.**

The `sub` condition is the important part. It restricts the role to workflow runs that
declare `environment: production` or `environment: beta` — which only `deploy.yml` does.
A pull request from a fork cannot match it, so contributor PRs can never reach AWS.

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Federated": "arn:aws:iam::<ACCOUNT_ID>:oidc-provider/token.actions.githubusercontent.com"
      },
      "Action": "sts:AssumeRoleWithWebIdentity",
      "Condition": {
        "StringEquals": {
          "token.actions.githubusercontent.com:aud": "sts.amazonaws.com",
          "token.actions.githubusercontent.com:sub": [
            "repo:SteveF92/FantasyCritic:environment:production",
            "repo:SteveF92/FantasyCritic:environment:beta"
          ]
        }
      }
    }
  ]
}
```

Attach this inline permissions policy:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "PublishReleaseBundles",
      "Effect": "Allow",
      "Action": ["s3:PutObject", "s3:GetObject", "s3:AbortMultipartUpload"],
      "Resource": "arn:aws:s3:::<BUCKET>/releases/*"
    },
    {
      "Sid": "ListReleaseBucket",
      "Effect": "Allow",
      "Action": "s3:ListBucket",
      "Resource": "arn:aws:s3:::<BUCKET>"
    },
    {
      "Sid": "DescribeTargets",
      "Effect": "Allow",
      "Action": [
        "ec2:DescribeInstances",
        "ec2:DescribeInstanceStatus",
        "ssm:DescribeInstanceInformation"
      ],
      "Resource": "*"
    },
    {
      "Sid": "StartBetaInstance",
      "Effect": "Allow",
      "Action": "ec2:StartInstances",
      "Resource": "arn:aws:ec2:<REGION>:<ACCOUNT_ID>:instance/<BETA_INSTANCE_ID>"
    },
    {
      "Sid": "RunDeployCommand",
      "Effect": "Allow",
      "Action": "ssm:SendCommand",
      "Resource": [
        "arn:aws:ec2:<REGION>:<ACCOUNT_ID>:instance/<PROD_INSTANCE_ID>",
        "arn:aws:ec2:<REGION>:<ACCOUNT_ID>:instance/<BETA_INSTANCE_ID>",
        "arn:aws:ssm:<REGION>::document/AWS-RunShellScript"
      ]
    },
    {
      "Sid": "ReadCommandResults",
      "Effect": "Allow",
      "Action": ["ssm:GetCommandInvocation", "ssm:ListCommandInvocations"],
      "Resource": "*"
    },
    {
      "Sid": "PreMigrationSnapshot",
      "Effect": "Allow",
      "Action": ["rds:CreateDBSnapshot", "rds:AddTagsToResource"],
      "Resource": [
        "arn:aws:rds:<REGION>:<ACCOUNT_ID>:db:<PROD_DB_INSTANCE>",
        "arn:aws:rds:<REGION>:<ACCOUNT_ID>:snapshot:predeploy-*"
      ]
    },
    {
      "Sid": "WaitForSnapshot",
      "Effect": "Allow",
      "Action": "rds:DescribeDBSnapshots",
      "Resource": "*"
    }
  ]
}
```

`ec2:DescribeInstances` and the SSM read actions do not support resource-level permissions,
hence the `"*"` on those two statements.

Note the role can start the beta instance but not stop it — the roadmap says beta gets left
running after a deploy, and shutting it down stays a deliberate manual act.

Copy the role ARN; it goes into GitHub in step 6.

---

## 4. Instance roles

Both instances already have an instance profile (they read Secrets Manager). Each needs two
additions.

**a. SSM.** Attach the AWS managed policy `AmazonSSMManagedInstanceCore` to the instance
role. This is what lets the deploy reach the box with no inbound port open at all.

**b. Read the release bucket.** Add this inline policy to the same role:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": "s3:GetObject",
      "Resource": "arn:aws:s3:::<BUCKET>/releases/*"
    }
  ]
}
```

Then confirm SSM sees each instance. On Ubuntu AMIs the agent ships as a snap and is
usually already running:

```bash
sudo snap services amazon-ssm-agent
sudo systemctl status snap.amazon-ssm-agent.amazon-ssm-agent.service
```

From your workstation, the instance should appear here within a couple of minutes of the
policy attaching (the agent caches credentials, so a `sudo snap restart amazon-ssm-agent`
speeds it up):

```bash
aws ssm describe-instance-information \
  --query 'InstanceInformationList[].{Id:InstanceId,Ping:PingStatus,Version:AgentVersion}'
```

`PingStatus: Online` for both instances is the gate for everything below.

---

## 5. Prepare the instances

Do this on **beta first**, then production. Beta has to be powered on for this.

**a. AWS CLI.** The deploy script downloads its own bundle from S3, so the CLI must exist on
the box:

```bash
aws --version || {
  sudo snap install aws-cli --classic
}
```

Verifying this over SSH is not sufficient, and this bit me on the first beta deploy. SSM Run
Command uses a *non-login* shell, so it never sources `/etc/profile.d/apps-bin-path.sh` and
`/snap/bin` is not on its PATH — `aws` works perfectly when you SSH in and is invisible to
the deploy. The workflow now prepends `/usr/local/bin` and `/snap/bin` itself and fails with
an explicit message if `aws` is still missing, so this should be handled. To check the way
the deploy actually sees it:

```bash
aws ssm send-command --instance-ids <INSTANCE_ID>   --document-name AWS-RunShellScript   --parameters 'commands=["command -v aws || echo NOT-ON-PATH; echo PATH=$PATH"]'   --query 'Command.CommandId' --output text
# then, with that id:
aws ssm get-command-invocation --command-id <ID> --instance-id <INSTANCE_ID>   --query StandardOutputContent --output text
```

**b. Directory layout.**

```bash
sudo mkdir -p /opt/fantasy-critic/releases
sudo chown root:root /opt/fantasy-critic
```

**c. Log and data directories** (these already exist, just confirm the service user can
write them):

```bash
ls -ld /var/log/fantasy-critic /var/lib/fantasy-critic
```

**d. systemd unit.** Compare the current unit against
[`infrastructure/fantasy-critic.service`](../infrastructure/fantasy-critic.service):

```bash
systemctl cat fantasy-critic.service
```

Carry over the `User`/`Group` and any extra `Environment=` lines from the existing unit, and
take the new `WorkingDirectory`, `ExecStart`, and `ASPNETCORE_ENVIRONMENT` from the template.
On beta, `ASPNETCORE_ENVIRONMENT` must be `Staging` — `Program.cs` maps that to the `beta`
secret, and `deploy.sh` reads the value back out of the unit to run the migrator against the
right database.

```bash
sudo cp fantasy-critic.service /etc/systemd/system/fantasy-critic.service
sudo systemctl daemon-reload
```

Do **not** restart yet — `/opt/fantasy-critic/current` does not exist until the first deploy,
so the service will fail to start until then. That is expected and is why beta goes first.

**e. nginx** needs no change. It already proxies to `127.0.0.1:5000`, which the new unit
keeps.

---

## 6. GitHub configuration

**Settings → Environments.** Create two: `production` and `beta`. The workflow's
`environment:` key selects one at run time, which is both how the variables below resolve
and how the OIDC `sub` claim gets its value.

Add these as **variables** (not secrets — none of them are sensitive, and variables are
visible in run logs, which is useful):

| Variable | `production` | `beta` |
|---|---|---|
| `AWS_ROLE_ARN` | the role from step 3 | same |
| `AWS_REGION` | `us-east-1` | `us-east-1` |
| `RELEASE_BUCKET` | `<BUCKET>` | `<BUCKET>` |
| `EC2_INSTANCE_ID` | `<PROD_INSTANCE_ID>` | `<BETA_INSTANCE_ID>` |
| `RDS_INSTANCE_IDENTIFIER` | `<PROD_DB_INSTANCE>` | *(unset — beta takes no snapshot)* |
| `SITE_URL` | `https://www.fantasycritic.games` | beta's URL, or leave unset to skip the probe |

Optionally add a **required reviewer** to the `production` environment. That turns every
production deploy into a one-click approval prompt. The workflow needs no change for this.

---

## 7. First deploy

**Beta first.** Actions → Deploy → Run workflow → branch `main`, environment `beta`.

Watch for:

- The build and unit tests pass on the runner (~6 minutes).
- The workflow starts the beta instance and waits for SSM to report online.
- The SSM step's output group shows `deploy.sh` stopping the service, running the migrator,
  swapping the symlink, and reporting `Healthy.`

Then production, same thing with `environment: production`. The extra step there is the
pre-migration RDS snapshot, which is usually the longest part of the run.

Verify by hand afterwards:

```bash
curl -i https://www.fantasycritic.games/health        # 200, liveness only
curl -i https://www.fantasycritic.games/health/ready  # 200, also proves MySQL is reachable
```

---

## 8. Strip the server

Only after two or three clean deploys. This is the payoff — the .NET SDK breaking on the box
is what forced server rebuilds twice, and a self-contained bundle means nothing here is used
any more.

**First, pin libicu.** A self-contained .NET app still links the system ICU libraries for
globalization. `libicu` is on the box today only because the .NET apt packages pulled it in
as a dependency, which means `autoremove` will happily take it away with them — and the app
will fail to start with a `Couldn't find a valid ICU package` error. Mark it manually
installed before purging anything:

```bash
sudo apt-mark manual $(dpkg -l 'libicu[0-9]*' | awk '/^ii/ {print $2}')
```

Then:

```bash
sudo apt-get purge dotnet-sdk-* dotnet-runtime-* aspnetcore-runtime-* nodejs
sudo apt-get autoremove
sudo rm -rf ~/.dotnet ~/.nuget ~/.npm

# The old checkout and build areas.
rm -rf ~/FantasyCritic ~/BuildArea ~/DbUpArea

# The old site directory, once /opt/fantasy-critic/current has been serving for a while.
sudo rm -rf /var/www/fantasy-critic
```

SSH can also be locked down at this point — the security group rule for port 22 can be
narrowed to your IP or removed entirely, since SSM Session Manager gives you a shell without
it (`aws ssm start-session --target <PROD_INSTANCE_ID>`).

---

## Operating it afterwards

### Deploying

Actions → **Deploy** → Run workflow. Pick the branch or tag; that ref is what gets built.

Two inputs worth knowing:

- **Skip migrations** — for a front-end-only change. Still stops and starts the service.
- **Skip snapshot** — skips the pre-migration RDS snapshot on production. Use sparingly; the
  whole point of that step is that the habit no longer depends on remembering.

### Rolling back

The deploy keeps the last five releases on the box. Over SSM Session Manager or SSH:

```bash
ls /opt/fantasy-critic/releases
sudo systemctl stop fantasy-critic
sudo ln -sfnT /opt/fantasy-critic/releases/<older-release-id> /opt/fantasy-critic/current
sudo systemctl start fantasy-critic
```

That rolls back **code only**. If the failed deploy ran migrations, restore the
`predeploy-<release-id>` RDS snapshot too — the workflow names it in the run summary.

`FantasyCritic.RdsSnapshotManager` still handles restores; nothing about it changes here.

### When a deploy fails

`deploy.sh` leaves the site stopped on a migration failure, on purpose: a half-applied
migration is not something the old code should be started against, MySQL DDL is not
transactional, and the snapshot exists. The failure output includes the exact rollback
commands and the last 50 journal lines.

### What the CI workflow does

Separate from deploys, `ci.yml` runs on every push to `main` and every pull request: build,
regenerate both NSwag clients, unit tests, `scripts/format.sh --check`, and an advisory
dependency vulnerability scan. Integration tests still do not run in CI — they need a MySQL
container and about six minutes, which the roadmap rules out for a solo project.

---

## Deferred to later phases

- **Maintenance page** during the stop/migrate/start window — Phase 2. Right now users get
  nginx's 502 for those couple of minutes, same as today.
- **IMDSv2 hop limit** — only matters once the app runs in a container reaching the instance
  role, i.e. Phase 3.
- **Terraform** — Phase 5. Everything created above is a candidate for import then; the S3
  bucket and OIDC role are called out explicitly in the roadmap.
- **Alerting** on the new `/health` endpoint — backlog item.
