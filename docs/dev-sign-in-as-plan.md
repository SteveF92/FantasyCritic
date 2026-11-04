# Dev sign-in-as plan

## Goal

Let anyone working locally, Claude included, sign in as any user in the local database by visiting a URL, with no
password. It works against the seeded Docker database and against an imported production snapshot, and it changes no
user's data. It must not exist outside a developer's machine.

## Why not a known password for everyone

The cleaner could set every password to one value written in the repo. A leaked dump would not be any worse for it (it
is better than real hashes, which can be cracked offline). But a cleaned copy serving a site others can reach, like beta,
would let anyone sign in as any account, admins included, with a password anyone can read. It also still means finding a
user's email and typing it into the login form. Sign-in-as needs no password, and it changes nothing about the
database.

## Decisions

- **The endpoint:** `GET /dev/sign-in-as?userId={guid}&returnUrl={path}`. A minimal API endpoint, not a controller,
  because `MapControllers` maps every controller it finds and cannot leave one out by environment. It looks the user up
  with `FantasyCriticUserManager.FindByIdAsync` and calls `FantasyCriticSignInManager.SignInAsync(user, isPersistent:
  false)`, which caches the user to the session exactly as a normal login does. Then it sends a `LocalRedirect` to
  `returnUrl`, or to `/` when none is given. `LocalRedirect` refuses anything off-site.
- **By user ID only.** On a snapshot, the ID comes from a query on the local database, and an ID doesn't need anyone's
  real email. Looking users up by email or display name, or "sign in as this league's manager", can come later if they
  turn out to be wanted.
- **GET, not POST,** so it is just a URL to visit. Cross-site requests are not a concern: the worst another site could do
  is sign the browser in to someone's *local* copy.
- **Four guards, all required:**
  1. **The environment is Development.** The endpoint is mapped only inside the `IsDevelopment()` branch of
     `ConfigurePipeline`. In any other environment the route does not exist, and the request falls through to the SPA.
  2. **The database is local.** It is mapped only when the server in `RepositoryConfiguration.ConnectionString` is
     `localhost`, `127.0.0.1` or `::1`. This is the guard that holds if production is ever started with
     `ASPNETCORE_ENVIRONMENT=Development` by mistake: production's database is on RDS. The connection string is read
     from the registered `RepositoryConfiguration`, not `WebOptions`, so it is what the app actually connects to.
  3. **The request comes from this machine.** `Connection.RemoteIpAddress` must be loopback, or the response is 404. This
     covers a dev server bound to `0.0.0.0` on a shared network.

  4. **The database says so.** A new `AllowSignInAs` bit in `tbl_meta_systemwidesettings`, default `0`, must be `1`.
     It is read on every request, so turning it off takes effect at once. Only two things set it, and both work only on
     local Docker databases: LocalDatabaseTool, which seeds `fantasycritic`, and the snapshot manager's local import and
     clean (options 5 and 8), which write to `fantasycritic-fromsnapshot`. `MySQLBetaCleaner` doesn't set it, because it
     also cleans beta. Production and beta keep the migration's default of `0`. This guard is about the data, not the
     host: a production database, or a copy of one that hasn't been cleaned locally, refuses sign-in even on a
     developer's machine.

  Together these mean it works for `dotnet run` and not in the `docker-compose-complete` stack. There, the database
  server is `mysql`, and requests from the host arrive from the Docker bridge, not loopback. That is acceptable: the
  compose stack is for checking images, not for debugging as a user.
- **Not in the API surface.** `.ExcludeFromDescription()`, so NSwag never puts it in the OpenAPI document or the
  generated clients. `Regenerate-ApiClient.ps1` runs the app in Development, so without this it might show up there.
- **Logged as a warning** each time it is used, with the user ID, so the dev log shows that a session started this way.
- **A user that doesn't exist, or one with `IsDeleted` set,** gets 404, not a sign-in.

## Where the code goes

| Piece | Home |
|---|---|
| `DevSignInEndpoint` (static `MapDevSignIn(this WebApplication)` and the endpoint handler) | Web `Development/` (new folder) |
| `DevSignInEndpoint.IsAllowed(IHostEnvironment, string connectionString)`, the first two guards as one pure check | same class |
| Call site | `HostingExtensions.ConfigurePipeline`, after `MapRazorPages` and before the SPA fallback |
| Guard unit tests | WebTests |
| Endpoint integration tests | IntegrationTests (the factory already runs Development against Docker on `localhost:3307`) |

## Steps

Each step gets its own commit, and work stops for review after each one.

### Step 1: The endpoint and its guards

- `DevSignInEndpoint` as described above, mapped from `ConfigurePipeline`.
- WebTests unit tests for `IsAllowed`: Development with localhost and with 127.0.0.1 is allowed. Development with
  `Server=mysql`, Development with an RDS hostname, Production with localhost, and Staging with localhost are refused.
- Check by hand that `/dev/sign-in-as?userId=00000000-0000-0000-0000-000000000000` signs in as `localadmin@example.com`
  on the seeded database, and that `/dev/sign-in-as` with an unknown ID gives 404.

### Step 2: Integration tests

- An integration test registers a user with `NewUser()`, then signs in through `/dev/sign-in-as` on a fresh
  `ApiSession`. It then calls an authenticated API (the current-user endpoint on `AccountClient`) and gets that user
  back.
- An unknown user ID gives 404. A non-local `returnUrl` doesn't redirect off-site.
- TestServer may not set `RemoteIpAddress`, so check what it sends. If it is null, the factory gets a startup filter that
  sets it to loopback, and one test leaves it as a non-loopback address and gets 404.
- `ApiSession` gets a `SignInAsAsync(Guid userId)` helper. This is the one route that isn't an NSwag client method,
  because it is deliberately left out of the API. Existing tests keep using real login. Only new tests that don't care
  how the user signed in may use the helper.

### Step 3: The AllowSignInAs flag

Added after Steps 1 and 2, as a fourth guard alongside the other three.

- Migration `2026-11-04_000_allowSignInAs.sql` adds `AllowSignInAs BIT(1) NOT NULL DEFAULT b'0'`.
- `SystemWideSettings` and its entity get `AllowSignInAs`. It is not added to any view model, so the API never shows it.
- The endpoint reads `IFantasyCriticRepo.GetSystemWideSettings()` on each request and returns 404 unless the flag is
  set. It checks the flag after the loopback check and before looking up the user.
- `MySQLLocalSetupSyncer.AllowSignInAs()` sets it. LocalDatabaseTool calls it after `EnsureLocalAdminUser`.
  `LocalImportService` and `LocalDatabaseCleanService` call it after the scrub.
- Snapshots are never migrated locally, so a snapshot taken before production has this migration has no column. In that
  case the snapshot manager logs a warning that sign-in-as stays off and finishes the import or clean as normal. Once
  production is migrated, its snapshots carry the column, set to `0`, and the local clean turns it on.
- Tests: the existing integration tests prove the flag is on in the seeded database. A unit test for the refusal isn't
  possible without a fake repository in the web host, so the refusal is checked by hand: run only the database updater
  (flag `0`, sign-in-as gives 404), then LocalDatabaseTool (flag `1`, sign-in-as works).

### Step 4: Documentation and a real run

- A short "Signing in locally" section in CLAUDE.md: the URL, the seeded local admin's ID, how to find a user's ID on a
  snapshot, and that it only works under `dotnet run`.
- From the browser pane, sign in as the local admin on the seeded database. Then, on an imported snapshot, sign in as a
  league manager picked by query, and load their league.

## Open questions

- **Deleted users on a snapshot.** The endpoint refuses users with `IsDeleted` set. But the cleaner marks every
  non-beta user deleted (49,454 of 49,463 on the current local snapshot). So on a snapshot, sign-in-as only reaches beta
  testers, which defeats "sign in as this league's manager". Dropping the `IsDeleted` check would fix that. It isn't
  one of the four guards: it only controls which users can be reached once the guards have passed.

- Should a fourth guard be an explicit opt-in config key (for example `Development:AllowSignInAs`, off by default)? The
  plan leaves it out, because the local-database guard already covers a misconfigured environment. It would also mean a
  new section record, a new key in Web's appsettings and a required-from environment, all for one dev convenience.
