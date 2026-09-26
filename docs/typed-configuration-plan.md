# Typed configuration plan

Written 2026-09-22 on the `typed-configuration` branch (first called `config-redo`), which branches from
`991787042` ("Group the loose config keys under Discord and Patreon").

**Done 2026-09-25.** All eight steps are on `typed-configuration`, merged into `job-system`. Before it deploys,
the secret store needs the changes under "Secret store and user secrets" below: from step 5 on, a host missing
one refuses to start.

## Goal

Every configuration value is read once, into a strongly typed object, and every application has its own root
options type made of the sections it needs. After this, no `AddFantasyCritic*` method and no host startup
contains a configuration key as a string literal.

A missing key becomes a loud startup failure that names it, instead of a `null!`, or a `"secret"`
placeholder, that goes unnoticed until something uses it.

## Prior art: an earlier attempt

This had been built once already, on a branch since discarded: four commits to build it, then 22 after two AI
reviews to fix it. Both reviews called the architecture sound and the result not production-safe, and the
fixes that followed are the lessons this plan builds in from the start:

- **Placeholders passed validation.** Every appsettings file ships `"secret"`, so a key missing from Secrets
  Manager binds to the placeholder and a null-or-blank check lets it through. The fix was a threshold: each
  value says from which environment on the placeholder stops being acceptable. That design is taken as is.
- **Patreon's client id existed at two paths**, and the two could drift. Step 1 exists because of this.
- **Web has two configuration roots.** `FantasyCriticConfigurationLoader.Load` builds the one that feeds DI.
  `builder.Configuration` feeds `IConfiguration` injection and the integration test factory's
  `ConfigureAppConfiguration`. So a test override never reaches typed options. Moving `AdminController` onto
  typed options without accounting for this makes every clock endpoint return 404 under test. The factory
  already works around it by replacing DI singletons; that becomes its only mechanism.
- **The migrator validated before it had a logger**, so a failure exited 1 with no output. Its logging moves
  onto the shared helpers before any validation lands.
- **There were no tests** until late, and the inverted threshold is exactly what they catch. The tests land
  with the validation code, not after it.

That attempt served as a reference implementation: `MissingConfiguration`, the threshold and its options
tests were lifted from it rather than written again. Its history was not the model; this plan's steps were.

## Design

**Section options** mirror appsettings sections: immutable records with `required` properties, in
`Lib/Configuration`. Each implements `IOptionsSection` and reports the paths it is missing through
`MissingConfiguration`.

**Root options** belong to their host project: `WebOptions` in Web, `WorkerOptions` in Worker, and so on.
A root is the list of its sections and loose keys, so the root type *is* that host's schema and its
appsettings file is an instance of it.

**The `Add*` methods take sections directly**, e.g. `AddFantasyCriticAdminServices(options.Aws, options.OpenCritic, options.Authentication.Patreon)`.
A host cannot register email without a `PostmarkOptions` to pass, which makes "appsettings lists every key the
host reads" something the compiler checks.

**Lib's service records** stay where they combine configuration with something that is not configuration:
`RepositoryConfiguration` (`IClock`), `FantasyCriticDiscordConfiguration` (`IsDevelopment`),
`EnvironmentConfiguration`. A record that is a field-for-field copy of a section is replaced by the section:
`PatreonConfig` becomes the Patreon options.

**Validation** collects every missing path and reports them together, so a secret migration that missed three
keys fails once, naming all three. A value is missing if it is blank, or if it is the `"secret"` placeholder and
the environment is at or past the value's `requiredFrom` (Development < Beta < Production). OAuth keys are
required from Production, since Web only registers the providers there. The bot token and the API keys are
required from Beta. The connection string is required everywhere. Loki keys only have to be present: empty
turns the sink off.

## Steps

Each step builds, passes unit and integration tests, and is committed alone. Stop for review before the next.

1. **Put every value at exactly one path, spelled one way.** An audit of all five appsettings files, all
   three compose files, the integration test factory and every C# read found one split and one leftover:
   - Patreon's campaign id moves into `Authentication:Patreon`, next to the client id and secret. Patreon is
     both a login provider and an API integration, and one section beats two that can drift.
   - The commented-out `Discord__DevServerId` line in `docker-compose-complete.yaml`, missed when
     `DevServerId` was removed.

   Every configuration key becomes PascalCase, and initialisms are written as words: `Aws`, `Rds`, `Id`. So
   `AWS:region` becomes `Aws:Region`, `apiKey` becomes `ApiKey`, `rdsInstanceName` becomes `RdsInstanceName`,
   and the campaign id lands as `CampaignId`. This covers every appsettings file, including the satellite
   tools', the compose files, and the C# that still reads keys by string. It is only for keys: C# names
   keep their own conventions, and step 6 replaces those string reads anyway.

   Two things look like keys and are data, so they keep their spelling. `Grafana:Loki:Labels:app` is sent as
   the Loki label name, and label names are case-sensitive, so `App` would break every `{app="…"}` query. The
   `production` and `beta` entries under the snapshot manager's `RdsInstances` are names the tool shows and
   matches on.

   Casing costs nothing in the secret store: configuration keys are case-insensitive, so a secret that still
   says `AWS:region` binds to `Aws:Region`. Only the campaign id's move needs a secret edit.

   The worker's `appsettings.Development.json` goes too. It is the template's `Logging` block, the one copy
   the appsettings cleanup missed, and Serilog's own minimum already does what it says.
2. **Split Patreon out of Core.** Since `7a495b3e2` moved the Plus role refresh onto `PatreonService`, nothing
   in Core needs Patreon. Its only consumers are `AdminService` and Web's external-logins page, so
   `AddFantasyCriticAdminServices` registers it, through a private `AddFantasyCriticPatreon` that no host can
   call a second time. The Discord bot drops its `Authentication:Patreon` section entirely. This changes which
   sections each host needs, so it comes before the root types are written down.

   Discord push stays in Core. Seven of Core's own domain services inject `DiscordPushService`, so taking it
   out would leave `AddFantasyCriticCore` unable to build a graph on its own, and every caller would have to
   remember a second registration. That is the trap the earlier attempt's review flagged. Nothing needs push without
   Core, so the split would buy nothing.
3. **Section options, `MissingConfiguration`, the threshold, and their tests.** No host code touched. The
   tests bind sections out of each host's shipped appsettings, linked into the test output, with a secret
   layered on top where a test needs one, and assert the exact missing paths per environment.

   Validation takes a `FantasyCriticEnvironment`, not an `IHostEnvironment`, so Lib needs no hosting
   reference and a test states its environment in one word. A reported path is built from `nameof` the
   property it checks, so it cannot drift from the key the binder fills.

   OpenCritic's placeholder was `"key"`, which the threshold would read as a real value: Beta would have
   started without one. It is `"secret"` like every other.
4. **The migrator onto the shared logging.** `DatabaseUpdater` builds its own `LoggerConfiguration` with its own
   copy of the Loki block, and never sets `Log.Logger`. It moves onto `FantasyCriticLogging` so the next step's
   failures have somewhere to go.

   Its `try` now covers loading configuration too, which is where a secret store failure lands, and a failure
   there logs `Fatal` and exits 1. It references Hosting, as the command line tool does, so its Dockerfile
   restores Hosting, EmailTemplates and Postmark. `WriteToGrafanaLoki` takes the environment's name rather
   than an `IHostEnvironment`, since that is all it reads and all the migrator has.

   Every host's first logger held the log files open while the real one was built, so the rolling file sink
   started new files beside them: each run split across two files and used two of `log-all`'s three retained.
   A separate commit after this step moves all five hosts onto a Serilog bootstrap logger, which is closed
   before its replacement is built, and which loggers taken from it at startup follow to the replacement.
5. **Every host binds, validates and fails loudly.** All five root types. Each host validates right after
   `FantasyCriticConfigurationLoader.Load`, logs `Fatal` and exits 1. The bot's hand-rolled token check goes.
   DI still reads `IConfiguration`; this step only adds the schema and the failure.
   Hosting gains the one mapping from `IHostEnvironment` to `FantasyCriticEnvironment`, where Staging is Beta.
   The tests bind each host's whole shipped appsettings into its root type, in every environment.

   Each root implements `IHostOptions`, and `GetValidOptions` binds and validates in one call, so no host carries
   its own copy of that. An environment name that is neither Development nor Staging maps to Production, as the
   loader treats it as deployed: a misspelt name validates strictly. The bot requires a real token even in
   Development, where the other hosts accept the placeholder and leave Discord push off, because answering
   commands is its whole job; that replaces its hand-rolled check. Web's `Main` returned nothing, so a Web that
   failed at startup exited 0; it now returns 1.
6. **The `Add*` methods take sections.** The string reads in `ServiceCollectionExtensions` go, and
   `PatreonConfig` becomes the Patreon options.

   Each host takes its validated options once and passes sections from them. The migrator's read of its admin
   connection string moves onto its options here too, being the same change in a host rather than in Hosting.
   Web's `ConfigureServices` takes `WebOptions` alongside `IConfiguration` until step 7 moves its last string reads.
7. **Web's own startup.** The five OAuth providers and `ServiceHealth` from `WebOptions`. `IntegrationTestMode`
   joins `EnvironmentConfiguration`, and `AdminController` stops injecting `IConfiguration`. The test factory
   drops `ConfigureAppConfiguration` and overrides only at the DI level, with `with` expressions on the
   registered records.

   `IntegrationTestMode` turned out not to be configuration: appsettings only ever said false, the factory alone
   turned it on, and on anywhere else the clock endpoints could only fail without the factory's `AdjustableClock`.
   So the key leaves appsettings and `WebOptions`, Core always registers it false, and the factory overrides it.
   `AddFantasyCriticCore` gains no Web-only parameter. `ServiceHealthConfiguration`, a copy of the section, gives
   way to `ServiceHealthOptions`, and `appsettings.Testing.json` goes with the configuration overrides nothing read.

   Those overrides had never reached Web's services, which are built from the loader's configuration: the
   placeholder bot token the factory set protected nothing, so a real token in a developer's user secrets would
   have been used. An integration test now checks each override reaches the record Web's services read.
8. **The logger, the loader and a sweep.** `WriteToGrafanaLoki` takes `GrafanaOptions`; the loader reads the AWS
   region typed, before the secret store. Remove `FantasyCriticSettings`, a second copy of `BaseAddress` for
   the bot. Confirm nothing outside the binding layer reads a configuration key by string.

   The migrator's `GetConfiguration` is a copy of `FantasyCriticConfigurationLoader` and moves onto it. It
   also reads `ASPNETCORE_ENVIRONMENT` before `DOTNET_ENVIRONMENT`, the reverse of every other host; they
   should agree. And the shared loader looks the region up before it adds environment variables, so the
   `Aws__Region` compose sets never decides which region's secret store is read: appsettings' value does.

   Web replaces its bootstrap logger only after the app is built, so its configuration failures reach the console
   and the log files but not Loki. It should replace it straight after loading configuration, as the other hosts do.

   Done in four commits. The logger is built from the bound options' Grafana section before they are validated,
   so a host refusing to start says why in Loki; `WriteToGrafanaLoki` therefore tolerates a missing or half-bound
   section. The loader reads the region from a configuration that includes environment variables, validated
   first, and the migrator uses the loader. On the environment variables, the claim above was half wrong:
   `WebApplication.CreateBuilder` lets `DOTNET_ENVIRONMENT` win, checked rather than assumed, so the migrator
   was the only host reading them the other way. Four hosts now take the name from one resolver; Web leaves it to
   the framework, which agrees. `FantasyCriticSettings` gave way to `EnvironmentConfiguration`, where Web's
   controllers already read the base address, and a test checks the bot's command modules, which Discord.Net
   builds only when a command runs. `DiscordPushService` uses the placeholder check validation uses.

   The sweep: no host, Hosting, Lib or the integration tests read a key by string. `IConfiguration` remains only
   in the loader, which produces it, and `GetValidOptions`, which binds it. `DBUtility` and `LocalDatabaseTool`
   still read four keys by string; they are satellite tools, out of scope below. `CLAUDE.md` describes the
   configuration design and where a new key goes.

## Secret store and user secrets

Production code today reads the names from before the regrouping. Against those, the full set of changes is:

| Before | After |
|---|---|
| `BotToken` | `Discord:BotToken` |
| `PatreonService:CampaignID` | `Authentication:Patreon:CampaignId` |
| `PatreonService:AccessToken`, `PatreonService:RefreshToken` | removed; the tokens live in `tbl_system_patreonkeys` |
| `DevDiscordServerId` | removed |

This supersedes the note in `991787042`, which gave the campaign id's new home as `Patreon:CampaignID`.

Old and new keys can sit side by side in the secret, so the new ones can go in before deploying and the old
ones come out after. Once step 5 lands, a key that is still missing fails startup and says which one, rather
than leaving the web app and worker quietly running without Discord or Patreon.

## Out of scope

- The *names* `ConnectionStrings` and `AllowedHosts`, which belong to the framework. `ConnectionStrings` is
  bound like any other section.
- The satellite tools: `LocalDatabaseTool`, `DBUtility`, `MasterGameUpdater`, `BetaSync`, `TestDataScrubber`.
  `RdsSnapshotManager` already has typed options.
- `IOptions<T>`. The registration methods need values at registration time and nothing reloads configuration
  at runtime.
