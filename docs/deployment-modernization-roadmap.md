# Deployment Modernization Roadmap

> A phased plan for moving Fantasy Critic from "SSH in and build on the box" to a
> containerized, infrastructure-as-code deployment on AWS, ending in a position where a
> Vue 3 rewrite can happen incrementally alongside the live site.
>
> Each phase is self-contained. The intent is to ship a phase, live with it while doing
> unrelated feature work, and take the next step when ready. This is explicitly **not** a
> speed run to the end state.

Written September 2026.

---

## Where we start

- Deploying means pushing to the `production` branch, SSHing into a single Ubuntu EC2
  instance, running `git pull`, and running `linuxUpdateSite.sh`. That script stops the
  systemd service, builds and runs the DatabaseUpdater, regenerates the NSwag clients,
  publishes the Web project, swaps the files, and restarts the service.
- The server has the full .NET SDK, Node, git, nginx, and certbot installed and managed by
  hand. Let's Encrypt renewal runs on the box.
- MySQL is on RDS. Logs go to Grafana Cloud via the Serilog Loki sink, plus rolling files
  on the box.
- Secrets are one JSON blob per environment in AWS Secrets Manager, loaded at startup and
  overridable by environment variables.
- A beta server exists but is kept powered off except when something is worth testing.
- There is no CI of any kind.
- AWS spend is roughly $250/month. Ceiling is $350, and going there needs justification.

### Constraints and decisions that shape the plan

- **A few minutes of downtime per deploy is acceptable.** A maintenance page is enough.
  We do not want backward-compatible (expand/contract) migrations. This means we never
  need two web instances serving at once, which keeps ECS cheap and simple.
- **Integration tests do not run in CI.** Solo developer, six-minute run, not worth the
  compute. Build and unit tests run on every pull request and before every deploy.
- **This is not a pager site.** Users can wait if the site is down for a while. Alerting is
  a backlog item, not a phase.
- **Snapshots are already a habit.** `FantasyCritic.RdsSnapshotManager` creates production
  snapshots, restores them to beta with scrubbing, and publishes dumps to S3 and Google
  Cloud. Nothing major happens without a snapshot first, and production has been rolled
  back from one at least once. The pipeline can automate the pre-migration snapshot so the
  habit does not depend on remembering.
- **Cost alerts already exist** in AWS Budgets.
- **Windows hosting is dead.** `UpdateSite.ps1` and Windows log paths are leftovers.
- **The Discord push service stays in the web app.** Only the command-handling bot moves
  to its own process. Both use the same bot token; Discord permits multiple gateway
  sessions per token, and we already run two today.

### What the code tells us

- **The web process does four jobs.** Outside Development it hosts the HTTP API, the cron
  scheduler (`SchedulerHostedService`), the Discord bot gateway (`DiscordHostedService`),
  and the SignalR draft hub. It also holds an in-process master game cache. Anything that
  runs two copies of the web app, even briefly, runs two schedulers and two bots. The
  process split is therefore a **prerequisite** for ECS, not a follow-up.
- **Some things are already container-ready.** Data Protection keys live in MySQL, so
  multi-instance cookie auth works. Secrets load from Secrets Manager with env-var
  overrides. Dockerfiles exist for Web and DatabaseUpdater, and
  `infrastructure/docker-compose-complete.yaml` already runs the migrator as a job before
  the web starts.
- **The Razor pages are the ASP.NET Identity UI.** Login, external login callbacks, 2FA,
  email confirmation, account management. Replacing them must precede splitting the API
  and front end onto different hosts, because those auth flows are what break first.
- **The server does build work it shouldn't.** npm, NSwag client generation (both
  generated clients are gitignored), and two `dotnet publish` runs happen on the
  production box. The root cause of past forced server rebuilds was the .NET SDK breaking
  on the server. Removing the SDK from the box removes that failure mode entirely.
- **The standalone `FantasyCritic.DiscordBot` project is stale.** Hand-rolled DI, no
  Secrets Manager, no Loki, different config shape than Web. Splitting the bot out is a
  rewrite of that host, not a switch flip.

---

## Changes to the original list

The original todo list had 18 items. Consolidated:

| Original | Outcome |
|---|---|
| 1. GitHub Actions deploy | **Phase 1.** No Docker required. |
| 2. Docker | **Phase 3.** |
| 3. CloudFront and/or load balancer | **Split.** ALB in Phase 5 (kills Let's Encrypt on the box). CloudFront in Phase 7c (only valuable once the SPA lives on S3). |
| 4. Automated server provisioning / Terraform | **Phase 5.** |
| 5. ECS | **Phase 6.** |
| 6. Compose with migration job before app | **Merged into Phase 3.** Already designed in the existing compose file. |
| 7. Better secrets storage | **Dropped.** The one-blob secret with env-var overrides is exactly what ECS wants. Helm/K8s secret stores solve a multi-team problem this project doesn't have. |
| 8. Discord bot as its own process | **Phase 4a.** Moved earlier; prerequisite for ECS. |
| 9. Maintenance mode | **Phase 2.** Cheap, high visibility. |
| 10. .NET Aspire | **Optional side quest** after Phase 4. Local AppHost is useful with four processes; deployment story is Azure-first; skip metrics. |
| 11. Hangfire in a dedicated process | **Phase 4b.** Moved earlier; prerequisite for ECS. |
| 12. Kubernetes | **Checkpoint decision** after Phase 6, not a phase. |
| 13 / 16. Eliminate Razor Pages | **Same step. Phase 7a.** |
| 14. Vue code out of ClientApp folder | **Phase 7b.** |
| 15. api.fantasycritic.games / www split | **Phase 7c.** |
| 17. new.fantasycritic.games for Vue 3 | **Phase 8.** |
| 18. Swap new/old, drop old | **Phase 8.** |

---

## The phases

### Phase 1: GitHub Actions builds; the server only runs

**Goal:** No SDK, runtime, Node, git, or NSwag on the production server. Deploys happen
from a push, not an SSH session.

**Pull request checks.** A separate workflow on every pull request: `dotnet tool restore`,
regenerate NSwag clients, build, run unit tests. No integration tests. This is the check
that runs on contributor PRs before they are reviewed.

**Trigger: manual only.** `workflow_dispatch` with an `environment` input
(`production` / `beta`); the ref is chosen in the Run workflow dialog. Decided against a
push-to-`production` trigger — nothing should deploy as a side effect of a push. The
`environment` input maps to a GitHub Environment, which is both where the per-environment
variables live and what the AWS role's OIDC trust policy is scoped to, so a fork's pull
request can never reach AWS.

**Build.** The deploy workflow:

1. `dotnet tool restore`, build Web, regenerate NSwag clients (`scripts/regenerate-api-client.sh`).
2. Run unit tests.
3. Publish Web and DatabaseUpdater as **self-contained `linux-x64`** bundles. Self-contained
   is the key detail: it removes the need for any .NET install on the server, which is the
   thing that broke twice and forced rebuilds.
4. Upload the bundles to S3.

**Deploy.** Use AWS Systems Manager Run Command, authenticated via a GitHub OIDC role, to
execute a slimmed deploy script on the instance: stop service, run migrator from the
bundle, swap folders, start service. No inbound port is opened; SSH stays locked to a
personal IP or can be closed. The SSM agent ships with Ubuntu AMIs and the instance already
has an IAM role (for Secrets Manager); it needs the SSM managed policy added.

**Pre-migration snapshot.** Before running the migrator, the pipeline calls
`CreateDBSnapshot` on the production instance (the same call `RdsSnapshotManager` makes)
and waits for it to become available. This turns an existing manual habit into a guarantee.
Skip it for beta deploys.

**Beta.** The workflow calls `aws ec2 start-instances` on the beta box, waits for SSM to
report it online, deploys, and leaves it running. No trigger on the `beta` branch — that
would boot a normally-off instance on every push.

**Notes.**

- Keep `linuxUpdateSite.sh` working as a fallback for the first few deploys.
- If the repo is public, GitHub Actions minutes are free. Unit tests should run regardless.
- Serilog file logs are unchanged in this phase.
- Releases land in `/opt/fantasy-critic/releases/<id>` with a `current` symlink that systemd
  points at, so rollback is a symlink swap rather than rebuilding an old commit on the box.
- The health endpoint from the backlog is folded in here, since the deploy script needs
  something real to poll after restarting the service. `/health` is liveness only;
  `/health/ready` also checks MySQL.
- Setup runbook: [deployment-phase-1-setup.md](deployment-phase-1-setup.md).

**Cost:** ~$0. S3 storage for bundles is pennies.

### Phase 2: Maintenance page

**Goal:** Users see a real page during the stop/migrate/start window instead of a bad
gateway error.

- nginx `error_page 502 503` serves a static maintenance page from the host.
- A flag file (e.g. `/var/www/maintenance.on`) that nginx checks lets the deploy script
  (or a human) force the page for planned work.
- The deploy script from Phase 1 touches and removes the flag.

This is small enough to ride along with Phase 1 but is the first thing users notice, so it
deserves its own deploy.

**Cost:** $0.

### Phase 3: Docker on the box

**Goal:** The deploy artifact is a container image. The server is Ubuntu + Docker + nginx +
certbot + SSM agent, and nothing else.

- CI builds the Web and DatabaseUpdater images (Dockerfiles already exist) and pushes to ECR.
- A production compose file on the server mirrors the existing complete compose:
  `docker compose run --rm database-updater`, then `docker compose up -d web`.
- nginx and certbot stay on the host for now.
- Deploy is the same SSM path with a different script: pull, run migrator, up.

**Gotchas.**

- Containers reaching the instance IAM role through IMDSv2 fail with the default hop limit
  of 1. Raise the hop limit to 2 or use host networking.
- File logs become ephemeral. Loki already has everything, so drop the file sinks or
  bind-mount the log directory.
- The Web Dockerfile currently bakes a dev connection string as a default env var. Remove
  it or make sure production overrides it.

**Cost:** ECR storage, a few cents.

### Phase 4a: Discord bot as its own container

**Goal:** The command-handling bot runs in its own process. The web app keeps the push
service.

- Rewrite `FantasyCritic.DiscordBot` onto the .NET generic host with a shared DI
  registration extension in Lib (so Web, Bot, and later Worker register the same services
  the same way), Secrets Manager config loading, and the Loki sink.
- Web drops `DiscordHostedService` and the scoped `DiscordSocketClient` / `DiscordBotService`
  registrations. `DiscordPushService` stays in Web on the same token.
- Add a Bot Dockerfile and compose service.

### Phase 4b: Hangfire worker

**Goal:** Scheduled tasks and long-running admin actions run in a dedicated worker process.
The web app enqueues and reports status.

- New `FantasyCritic.Worker` project hosts the Hangfire server.
- Web hosts the Hangfire client and dashboard (behind admin authorization).
- The `IScheduledTask` cron classes become Hangfire recurring jobs.
- Admin console buttons (`adminConsole.vue`) enqueue jobs and poll state instead of holding
  an HTTP request open. The 300-second nginx timeout overrides for `/api/admin`,
  `/api/factchecker`, and `/api/actionrunner` go away.

**Verify before committing.** Hangfire's MySQL storage provider is community maintained.
Check its release activity against the current Hangfire core. If it looks stale, Quartz.NET
with its MySQL job store is the fallback and fits the same worker shape.

**Hidden design item: the master game cache.** Today the web process clears its own
in-process cache after refresh tasks run in the same process. Once refreshes run in the
worker, the web cache goes stale until restart unless something invalidates it. Options: a
TTL on the cache, or a cheap "cache version" row in the database checked on read. Budget
time for this.

**Optional side quest after Phase 4:** a .NET Aspire AppHost for local development, since
there are now four processes (MySQL, Web, Worker, Bot) to start. Skip ServiceDefaults /
metrics unless they turn out useful. Do not use Aspire for deployment.

### Phase 5: Terraform and the load balancer

**Goal:** AWS infrastructure is code. TLS is AWS-managed. A server rebuild is
`terraform apply` plus one pipeline run.

- Import existing resources: VPC, security groups, RDS, IAM roles, elastic IP, Route 53
  zone (if DNS is there).
- Add an ALB with an ACM certificate. TLS terminates at the ALB. The instance security
  group only accepts traffic from the ALB. certbot leaves the server.
- The instance becomes a launch template with user-data that installs Docker and pulls the
  compose file from S3.
- The maintenance page moves to an ALB fixed-response listener rule the deploy script
  toggles.
- ECR, S3 buckets, and the GitHub OIDC role from Phase 1 also become Terraform.

Every resource here carries forward into ECS. Only the launch template gets replaced.

**Cost:** ~+$20/month for the ALB.

### Phase 6: ECS

**Goal:** No server to configure. Three services, one task each.

- Services: `web`, `worker`, `bot`, each at desired count 1.
- Deploy pipeline: set `web` desired count to 0, run DatabaseUpdater as a one-off task and
  wait for it, update task definitions, set `web` back to 1. This matches the downtime
  tolerance exactly and never runs two schedulers or two bots.
- Beta becomes a second set of services at desired count 0, turned on by the same manual
  workflow (which also starts the beta RDS instance).
- Logging: keep the Serilog Loki sink as the primary path. Use the `awslogs` driver with
  short retention only to capture crash output that never reaches Loki.
- Secrets: keep loading the JSON blob at startup. Optionally, reference individual JSON keys
  from the same secret as task-definition env vars; the code already supports overrides.

**Compute decision (made at the start of this phase, needs the current instance type):**

- **Fargate**, tasks in public subnets with public IPs (avoids the ~$35/month NAT gateway).
  Roughly $60/month for web (1 vCPU / 2 GB), worker (0.5 / 1 GB), bot (0.25 / 0.5 GB).
- **ECS on EC2**, autoscaling group of 1 with the current instance class. Adds nothing to
  the bill. The box is still an instance but has zero hand configuration.

**SignalR note:** with desired count 1 the in-memory hub is fine. If web ever needs 2+
tasks, add a Redis backplane or ALB stickiness. Not planned.

**Kubernetes checkpoint.** Revisit only if there is a real need for more than one node or
many environments. EKS starts at ~$73/month before nodes. k3s on a single instance gains
nothing over compose. Expected outcome: stay on ECS.

**Cost:** +$0 to +$60/month depending on the compute decision.

### Phase 7a: Identity pages to Vue

**Goal:** No Razor Pages. All auth UI is Vue against JSON endpoints.

- Replace login, registration, 2FA, email confirmation, forgot/reset password, and account
  management pages with API endpoints and Vue views.
- External login challenge and callback endpoints stay server-side; that is how OAuth
  redirects work.
- The built-in Identity API endpoints (`MapIdentityApi`) are a reference, but expect to
  write custom endpoints for cookie mode and external providers.
- Remove `AddRazorPages` / `MapRazorPages`. `RazorEmailBuilder` (email templating) is
  unrelated and stays.

Must precede Phase 7c.

### Phase 7b: Extract ClientApp

**Goal:** The Vue code is its own top-level project with its own build.

- Move `src/FantasyCritic.Web/ClientApp` to a dedicated project directory.
- Web still copies the build output into `wwwroot` at publish time, so nothing changes at
  runtime.
- Update the NSwag TypeScript output path and the Format/lint scripts.

### Phase 7c: api / www split; CloudFront arrives

**Goal:** `api.fantasycritic.games` serves the API from the ALB. `www.fantasycritic.games`
serves the SPA from S3 behind CloudFront.

- Subdomains are same-site, so `SameSite=Lax` cookies work with a cookie domain of
  `.fantasycritic.games`. Needed: CORS with credentials, updated OAuth redirect URIs at each
  provider, SignalR CORS, antiforgery review.
- CloudFront needs a function (or S3 error document) for SPA fallback routing.
- Web stops serving static files and `MapFallbackToFile`.
- Alternative considered: a single CloudFront distribution with two origins (`/api/*` to
  ALB, `/*` to S3) on one hostname avoids CORS entirely. Rejected because Phase 8 needs
  `api.*` as a distinct host anyway.

**Cost:** a few dollars for S3 and CloudFront.

### Phase 8: new, then old, then gone

**Goal:** A Vue 3 site is built incrementally in parallel and eventually replaces the Vue 2 site.

- `new.fantasycritic.games` is another S3 bucket and CloudFront distribution, allowed in
  the API's CORS policy. The shared auth cookie works across subdomains.
- When ready: point `www` at the new distribution, move the old one to
  `old.fantasycritic.games`.
- Eventually delete the old distribution and the Vue 2 project.

Almost no infrastructure work remains here. That is the point of the sequence.

**Decisions to make before writing any Vue 3.** The stack choices (TypeScript, Pinia,
Vue Router 4, a bootstrap-vue replacement, Vitest, possibly Playwright) appear nowhere else
in this plan and the UI library choice is the one that cannot be changed later. Also decide
the migration unit: a whole parallel site on `new.*`, or a strangler approach where one
CloudFront distribution routes path patterns to the Vue 3 bucket page by page under the
same URL. The strangler version is more genuinely incremental and avoids maintaining two
front ends for a long time. A per-user "try the new site" flag in the database makes either
cutover far less stressful than a DNS swap.

---

## Sequencing note: when to start the client work

Vue 2 has been end-of-life since December 2023 and bootstrap-vue is stuck there with it.
That is the only item in this plan with an external clock, and as written it sits behind
six infrastructure phases. Phases 7a and 7b depend on **no** infrastructure phase and could
start the week after Phase 1 ships.

Decision deferred: complete the first few infrastructure phases, then decide whether to
alternate infra and client phases from that point. The failure mode to avoid is the
well-known one where infrastructure absorbs two years and the rewrite never starts.

---

## Hard dependencies

- **4a and 4b before 6.** Nothing multi-process runs on ECS until the web app is only a
  web app.
- **5 before 6.** ECS is Terraform anyway.
- **7a before 7c.** Auth flows break first when hosts split.
- **7c before 8.** `new.*` needs `api.*` to exist.
- Everything else can be reordered or interleaved with feature work.

## Cost summary

| Phase | Monthly delta |
|---|---|
| 1, 2, 3, 4a, 4b | ~$0 |
| 5 (ALB) | ~+$20 |
| 6 (ECS) | +$0 on EC2, up to +$60 on Fargate |
| 7c, 8 (S3 + CloudFront) | a few dollars |

All within the $350 ceiling with room to spare.

## Backlog: hygiene items with no phase of their own

Cheap items that belong somewhere in the sequence but do not justify a phase. Pick them up
when a nearby phase makes them convenient.

- ~~**Health endpoint.**~~ Done in Phase 1. `/health` is liveness only (runs no checks) so
  a database blip never pulls the instance out of an ALB target group in Phase 5;
  `/health/ready` additionally proves MySQL is reachable.
- **Alerting (low priority).** An external uptime check against the health endpoint posting
  to Discord, and a Loki alert on error-level logs from `FantasyCritic.*`. Not a pager;
  a notification.
- **Dependency and vulnerability scanning.** Dependabot or Renovate for NuGet and npm,
  `dotnet list package --vulnerable` and `npm audit` in the PR workflow, and ECR basic image
  scanning (free) once images exist in Phase 3.
- **Terraform state** goes in an S3 backend with locking and never in the repository.
  Relevant the moment Phase 5 starts.
- **Remove Windows leftovers.** `UpdateSite.ps1` and the Windows branches of `LoggingPaths`.

## Answers to the pre-Phase-1 questions

- **Current EC2 instance type: `t3.large`** (2 vCPU, 8 GB). On-demand that is roughly
  $60/month, or about $38 with a one-year no-upfront Savings Plan. This sets the bar for the
  Phase 6 compute decision: Fargate at 1 vCPU / 2 GB for web plus the worker and bot lands
  near $60/month, so ECS-on-EC2 on the same instance class is the cheaper of the two and
  Fargate is closer to break-even than to the "+$60" worst case in the table above.
- **DNS is on Route 53.** The hosted zone is therefore in scope for the Phase 5 Terraform
  import, and the Phase 5 ACM certificate can use DNS validation with automatic record
  creation.
