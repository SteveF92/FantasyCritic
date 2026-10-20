# Split the unit test project

## Context

`FantasyCritic.Test` grew to reference every host, so Lib cannot be tested on its own. The first
cut is done (8494347d3): the appsettings tests moved to `FantasyCritic.ConfigurationTests`, which
references every host on purpose, and `Test` dropped CommandLine and DatabaseUpdater.

The goal is for `Test` to reference only Lib, MySQL and FakeRepo. These are the tests that still need
something else:

| Needs | Tests | New home |
|---|---|---|
| Hosting | `BootstrapLoggerTests`, `GrafanaLokiTests`, `DiscordCommandModuleTests`, `ServiceRegistrationTests` (+ `TestHostEnvironment`) | `FantasyCritic.HostingTests` |
| RdsSnapshotManager | `DatabaseEmptyCheckerTests`, `LocalDatabaseConnectionGuardTests`, `LocalSnapshotConnectionStringTests`, `RdsInstanceLookupTests`, `RdsSnapshotManagerOptionsValidatorTests` | `FantasyCritic.RdsSnapshotManagerTests` |
| Web | `ErrorLogCounterTests`, `GrafanaLogLinksTests` | `FantasyCritic.WebTests` |
| Worker | `WorkerHealthCheckTests` | `FantasyCritic.WorkerTests` |
| DiscordBot | `DiscordBotHealthCheckTests` | `FantasyCritic.DiscordBotTests` |

Each new project references only the project it tests. Hosting comes in with every host, so `Test`
only stops depending on it once the last host reference is gone.

`GrafanaLogLinksTests.WorkerFlows_MatchTheWorkers` checks the web app's copy of the worker's flow
names against `WorkerLogging.Flows`, so it needs Web and Worker both. It joins the app label test in
`ConfigurationTests`, which already references every host and checks the web app's copies of the
hosts' names. Its fixture gets a broader name to fit both.

## How we work through this

**Each step ends the same way: build with zero warnings, run the unit test projects and
`scripts/Format.ps1 -Check`, commit that step alone, then STOP and review with Steve before starting
the next step.**

## Step 0: Commit the plan

The split was agreed in chat, so Step 1 follows straight on.

## Step 1: `FantasyCritic.HostingTests`

- New project referencing `FantasyCritic.Hosting`, with the same NUnit packages and warning
  settings as `ConfigurationTests`.
- Move `BootstrapLoggerTests`, `GrafanaLokiTests`, `DiscordCommandModuleTests`,
  `ServiceRegistrationTests` and `TestHostEnvironment`; the namespace becomes
  `FantasyCritic.HostingTests`.
- Hosting's `InternalsVisibleTo` names `HostingTests` in place of `Test`.
- So that later steps need no tooling changes, make the tooling pick up test projects by name:
  - CI's unit test step runs every `src/FantasyCritic.Test` and `src/FantasyCritic.*Tests` project
    except `IntegrationTests`, one trx file each. That replaces the two steps there now.
  - `Directory.Solution.targets` finds the projects for the NUnit2045 analyzer pass with a
    `FantasyCritic.*Tests` wildcard.
  - The `.editorconfig` NUnit2045 section uses the same pattern.

Commit → review.

## Step 2: `FantasyCritic.RdsSnapshotManagerTests`

- New project referencing `FantasyCritic.RdsSnapshotManager`; move its five test files.
- `Test` drops its RdsSnapshotManager reference.
- The test command in `src/FantasyCritic.RdsSnapshotManager/README.md` points at the new project.
  Its filter also names `BackupRemoteKeyBuilder` and `RdsSnapshotIdentifierValidator` tests,
  which are Lib tests and stay in `Test`, so the README gives both commands.

Commit → review.

## Step 3: `FantasyCritic.WebTests`

- New project referencing `FantasyCritic.Web`; move `ErrorLogCounterTests` and
  `GrafanaLogLinksTests`.
- `WorkerFlows_MatchTheWorkers` moves into `ConfigurationTests` next to the app label test (see
  Context).
- `Test` drops its Web reference.

Commit → review.

## Step 4: `FantasyCritic.WorkerTests`

- New project referencing `FantasyCritic.Worker`; move `WorkerHealthCheckTests`.
- `Test` drops its Worker reference.

Commit → review.

## Step 5: `FantasyCritic.DiscordBotTests`, and `Test` down to Lib, MySQL and FakeRepo

- New project referencing `FantasyCritic.DiscordBot`; move `DiscordBotHealthCheckTests`.
- `Test` drops its DiscordBot reference and references Lib, MySQL and FakeRepo directly, since MySQL
  only reached it through the hosts until now.
- `CLAUDE.md`: the unit test commands list the new projects.

Commit → review.
