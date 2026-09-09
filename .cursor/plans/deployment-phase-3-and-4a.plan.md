# Phase 3 (Docker) + Phase 4a (Discord bot process), together

Implements [Phase 3](../../docs/deployment-modernization-roadmap.md#phase-3-docker-on-the-box)
and [Phase 4a](../../docs/deployment-modernization-roadmap.md#phase-4a-discord-bot-as-its-own-container)
of the deployment modernization roadmap in one pass.

They are combined because 4a's deliverable is "the bot runs in its own container", which is
only meaningful once there is a container runtime on the box. Doing 3 first and 4a second
would mean writing the production compose file twice.

**End state:** the deploy artifact is three container images in ECR (web, database-updater,
discord-bot). The production instance runs Ubuntu + Docker + nginx + certbot + SSM agent and
nothing else. The web process no longer runs the Discord command gateway.

---

## Two findings that shape the work

**1. The Web Dockerfile cannot build from a clean checkout today.**
`src/FantasyCritic.ClientAppVue2/src/api/generated/FantasyCriticClients.ts` is gitignored and
produced by NSwag from the *compiled* Web assembly. The Dockerfile never runs that step, so
the vite build inside the image has nothing to import. It appeared to work when
`aea774c20` was verified locally only because the developer's working tree already contained
the generated file — Docker's build context includes gitignored files that exist on disk.

Related: the NSwag tool manifest (`.config/dotnet-tools.json`) lives at the repository root,
outside the current `../src` build context, so generation cannot be added without moving the
context.

**2. `DiscordPushService` already owns a second gateway session.** It constructs its own
`DiscordSocketClient` in its constructor rather than taking the DI one
(`src/FantasyCritic.Lib/Discord/DiscordPushService.cs:59`). Removing `DiscordHostedService`
and the scoped `DiscordSocketClient` / `InteractionService` / `DiscordBotService`
registrations from Web therefore cannot affect push notifications. This confirms the
roadmap's assumption that both halves can share one bot token.

---

## Decisions

**Image build: one Dockerfile per image, building from source, context = repository root.**
The alternative — publishing in CI and shipping a runtime-only image over the output — is
faster per run but leaves two definitions of the artifact to drift. Finding 1 above is
exactly the class of bug that produces, so the single reproducible definition wins. The
build context has to move to the repository root regardless, for the tool manifest.

**Shared DI registration: a new `FantasyCritic.Hosting` project.** The roadmap says "a shared
DI registration extension in Lib", but Lib cannot reference `FantasyCritic.MySQL` — that is
the reverse of the existing dependency, and the repository registrations are the whole point
of sharing. `FantasyCritic.Hosting` references Lib + MySQL + AWS and is what Web, the bot,
and the Phase 4b Worker all call.

---

## A. `FantasyCritic.Hosting` (new project)

References: `FantasyCritic.Lib`, `FantasyCritic.MySQL`, `FantasyCritic.AWS`. Serilog +
`Serilog.Sinks.Grafana.Loki` + `Serilog.Sinks.File` for the shared logging setup.

Deliberately **not** referenced: `FantasyCritic.Postmark` and `FantasyCritic.EmailTemplates`.
Email sending is Web's concern today and stays registered in Web; the Worker will revisit it
in 4b.

| Member | Contents |
|---|---|
| `AddFantasyCriticCore(services, configuration)` | `IClock`, `RepositoryConfiguration`, `EnvironmentConfiguration`, `PatreonConfig`, `FantasyCriticDiscordConfiguration`, every `MySQL*Repo` registration, and the domain services all hosts need (`InterLeagueService`, `PublisherService`, `ConferenceService`, `GameAcquisitionService`, `GameSearchingService`, …), `IDiscordFormatter`, `DiscordPushService`. |
| `AddFantasyCriticDiscordBot(services)` | `DiscordSocketConfig`, `FantasyCriticSettings`, `DiscordSocketClient`, `InteractionService`, `DiscordBotService`, `RoleHandler`, `DiscordHostedService`. Called only by the bot host. |
| `FantasyCriticConfigurationLoader.Load(environmentName, contentRootPath, assembly)` | The appsettings → user secrets → Secrets Manager → environment variables chain currently duplicated in `Web/Program.cs:64` and `DatabaseUpdater/Program.cs`, including the Staging→`beta` secret-name mapping. |
| `FantasyCriticLogging` | The console + rolling file bootstrap logger and the Loki reconfiguration, currently duplicated across Web and DatabaseUpdater. |

**Lifetime note.** Web registers `DiscordSocketClient` scoped; the standalone bot registers it
singleton. Singleton is correct for a gateway connection, and after this change only the bot
registers it at all, so `AddFantasyCriticDiscordBot` uses singleton with no conflict. The same
applies to `RoleHandler`, which Web has as singleton and the bot as scoped — verify its
dependencies during implementation and pick one.

**Web changes.** `HostingExtensions.ConfigureServices` calls `AddFantasyCriticCore` and drops
the block it replaces. Web-only registrations (Identity, authentication, authorization
policies, Data Protection, controllers, SignalR, Razor Pages, health checks, `BuildInfo`,
email, `AdminService`, the scheduler, the OpenCritic/GG HTTP clients) stay in
`HostingExtensions`. `TreatWarningsAsErrors` is on — the solution builds with zero warnings
and must keep doing so.

`FantasyCritic.Hosting` is added to `src/FantasyCritic.slnx` at the top level.

---

## B. Phase 4a: the bot becomes a real host

`src/FantasyCritic.DiscordBot/Program.cs` is a rewrite, not an edit. Today it is a hand-rolled
`ServiceCollection`, an `appsettings.json` read with no Secrets Manager, console-only Serilog,
and `Task.Delay(Timeout.Infinite)` as the host loop.

- `Host.CreateApplicationBuilder`, Serilog via `FantasyCriticLogging`, configuration via
  `FantasyCriticConfigurationLoader`, then `AddFantasyCriticCore` +
  `AddFantasyCriticDiscordBot`, then `await host.RunAsync()`.
- **Config key change:** the bot reads `ConnectionStrings:DefaultConnection` instead of its
  own `ConnectionString` key, so the existing per-environment Secrets Manager blob works
  unchanged. Its `appsettings.json` moves to the same shape (`AWS:region`,
  `ConnectionStrings:DefaultConnection`, `BaseAddress`).
- `LoggingPaths` gains a `DiscordBot` entry (`/var/log/fantasy-critic/discordbot`).
- `DiscordHostedService` stays in Lib. It currently opens a scope to resolve
  `DiscordBotService`; with a singleton registration that scope is pointless and goes away.
- **Web drops** `services.AddHostedService<DiscordHostedService>()` and the scoped
  `DiscordSocketClient` / `InteractionService` / `DiscordBotService` registrations
  (`HostingExtensions.cs:167-182`). `DiscordPushService` and
  `FantasyCriticDiscordConfiguration` stay — Web still pushes.
- `FantasyCriticSettings` is consumed only by the Discord command modules in Lib, so it
  becomes a bot-side registration.

**Cutover hazard:** between the old Web (which registers commands globally on `Ready`) and the
new bot, commands must not be handled twice. The deploy sequence below stops web before
starting the bot, so there is no overlap.

---

## C. Phase 3: images and the production compose file

### Dockerfiles

All four move to a **repository-root build context**. `src/.dockerignore` moves to
`./.dockerignore` and grows entries for `**/dist`, the generated API clients, `.git`,
`docs/`, and `src/FantasyCritic.Test/TestData` (135 MB of LFS fixtures that would otherwise be
uploaded to the daemon on every build). It must *not* exclude `.config/` or `scripts/`.

**`src/FantasyCritic.Web/Dockerfile`** — the one with real work:

```
restore  ->  build Web  ->  dotnet tool run nswag  ->  npm ci  ->  publish (esproj runs vite)  ->  runtime
```

- The NSwag stage needs `.config/dotnet-tools.json`, `src/FantasyCritic.ApiClient/nswag.json`,
  and `ASPNETCORE_ENVIRONMENT=Development` so `Program.cs` skips Secrets Manager while NSwag
  boots the host. `nswag.json` uses `noBuild: true`, so the Web build must precede it and the
  configurations must match.
- `npm ci` gets its own layer keyed on `package.json` + `package-lock.json` so a C# change
  does not reinstall node_modules. The esproj's `EnsureNodeModules` target then no-ops.
- **Remove the baked `ConnectionStrings__DefaultConnection` default** (`Dockerfile:52`). A
  development credential compiled into the production image is a footgun; production loads it
  from Secrets Manager and would only fall back to this on a misconfiguration, which should
  fail loudly instead.
- Build info: the `RELEASE` file is bind-mounted at `/app/RELEASE` by the deploy script rather
  than baked, because `deployed_at` is appended at deploy time. `BuildInfoReader` already
  checks the content root, so no code change.

**`src/FantasyCritic.DatabaseUpdater/Dockerfile`** — context bump only; `Scripts/**` is already
copied to the output and `GetScriptsRoot()` finds it at `/app/Scripts`.

**`src/FantasyCritic.DiscordBot/Dockerfile`** — new, modelled on the DatabaseUpdater one.

**`src/FantasyCritic.LocalDatabaseTool/Dockerfile`** — context bump only.

### Compose

- `infrastructure/docker-compose-complete.yaml`: contexts bumped to the repository root, plus a
  `discord-bot` service. Behavior otherwise unchanged.
- `infrastructure/docker-compose-production.yaml` (new): `web`, `database-updater` and
  `discord-bot` pulled from ECR by tag, no `build:` stanzas, no MySQL. Web publishes
  `127.0.0.1:5000:8080` so **nginx, `api_proxy.conf`, and the Phase 2 maintenance snippet are
  untouched**. `/var/log/fantasy-critic` is bind-mounted into web and bot.
- `/opt/fantasy-critic/.env` on the instance replaces the systemd unit as the place the deploy
  script reads the environment from: `ASPNETCORE_ENVIRONMENT`, `AWS_REGION`, `ECR_REGISTRY`,
  `IMAGE_TAG`. Compose picks it up automatically; the deploy script rewrites `IMAGE_TAG` and
  leaves the rest alone. `fantasy-critic.service` is retired.

### Pipeline

- New composite action `.github/actions/build-and-push-image`: ECR login,
  `docker/build-push-action` with `cache-from`/`cache-to: type=gha,mode=max`.
- `deploy.yml`: the two `dotnet publish --self-contained` steps and the ~300 MB tarball are
  replaced by three image builds. The S3 bundle shrinks to the compose file, the deploy
  script, `maintenance.sh`, `maintenance.html` and `RELEASE`. Images are tagged with the
  release id; `IMAGE_TAG` in `.env` is what a rollback changes.
- `ci.yml`: build the three images without pushing on pushes to `main`, so a Dockerfile break
  is caught before a deploy rather than during one. Not on pull requests — it would roughly
  double PR time for a file that changes rarely.
- ECR basic image scanning (free) is enabled on the repositories, closing the backlog item
  that was waiting for images to exist.

### `deploy/deploy.sh`

Rewritten against compose. Same shape, same failure discipline, same maintenance-page
handling:

```
maintenance install + on
docker compose pull
docker compose down            (web + bot; migrations are not expand/contract)
docker compose run --rm database-updater
docker compose up -d web discord-bot
poll http://127.0.0.1:5000/health
maintenance off
docker image prune
```

Every failure path still leaves the maintenance page raised and prints a rollback hint — now
"put the previous `IMAGE_TAG` back and re-run" rather than a symlink swap. `linuxUpdateSite.sh`
and the Phase 1 `deploy.sh` are kept in the repository for one or two deploys as a fallback,
the same way `linuxUpdateSite.sh` was kept through Phase 1.

---

## D. Instance and AWS setup (`docs/deployment-phase-3-setup.md`)

1. Three ECR repositories with lifecycle policies and scan-on-push.
2. Deploy role gains ECR push permissions; instance role gains ECR pull.
3. **IMDSv2 hop limit → 2** (`aws ec2 modify-instance-metadata-options`). With the default of
   1, containers cannot reach the instance role, so Secrets Manager loading fails at startup.
   This is the single most likely first-deploy failure.
4. Docker Engine + compose plugin installed; `/opt/fantasy-critic/.env` seeded.
5. `/var/log/fantasy-critic` chowned to the container UID (`$APP_UID`, 1654 in the Microsoft
   images) so the Serilog file sinks can write through the bind mount.
6. Strip the .NET runtime leftovers from the box.

## E. Roadmap update

Mark Phases 3 and 4a complete in `docs/deployment-modernization-roadmap.md`, link the setup
runbook, strike the "ECR basic image scanning" backlog item, and note under Phase 4b that
`FantasyCritic.Hosting` is where the Worker's registrations go.

---

## Verification

- `dotnet build src/FantasyCritic.slnx` clean, zero warnings (`TreatWarningsAsErrors`).
- `dotnet test src/FantasyCritic.Test` green.
- `scripts/Format.ps1 -Check` clean.
- **The clean-checkout test that finding 1 failed:** build the Web image from a fresh clone
  (or with `src/api/generated` and `node_modules` deleted) and confirm the SPA bundle is in
  `wwwroot` and imports the generated client.
- `docker compose -f infrastructure/docker-compose-complete.yaml up` brings up MySQL,
  migrator, web and bot locally.
- Site answers on `127.0.0.1:5000/health` and `/health/ready` from inside the compose network.
- Discord: a slash command is handled exactly once (bot container), and a push notification
  still arrives (web container).
