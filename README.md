# Fantasy Critic

Fantasy Critic is like fantasy football...but for video games! You and several friends will create your own virtual video game "Publishers",
and then you will assemble a roster of the games you believe will review the best. Similar to fantasy football, the players will alternate
picking games in a snake draft style to start the year. At the end of the year, the winner is the team with the best lineup of video games
based on scores from <a href="https://opencritic.com/">opencritic.com</a>.

You can see the site at <a href="https://www.fantasycritic.games/">fantasycritic.games</a>

## Development Setup

### Option 1: Running the client side only.

ClientApp is the root directory for the front-end UI. In it contains a Vue.js client application. See the official [Vue.js documentation](https://vuejs.org/) for more
information. The project is configured to start its own instance of the client app in the background when the ASP.NET Core app starts in development mode. The client
app can also be run via webpack dev server, instead of the ASP.NET core app. Note, when using the webpack dev server, the client app will point to the
[official website](https://www.fantasycritic.games/), instead of the local ASP.NET core app.

```sh
> npm run client
```

### Option 2: Full Setup

You'll need to install a few things:

- .NET 10
- Docker Desktop
- A C# IDE (Visual Studio 2022+ or JetBrains Rider)

#### 1. Start the database

From the repo root, bring up MySQL and run all database migrations in one step:

```sh
docker compose -f infrastructure/docker-compose-mysql.yaml up
```

This will:
- Start a MySQL 8.4 instance on **port 3307** (to avoid conflicting with a local MySQL installation).
- Create the `fantasycritic` database and the required users automatically.
- Run **FantasyCritic.DatabaseUpdater** (DbUp), which applies all schema migrations under `src/FantasyCritic.DatabaseUpdater/Scripts/`.
- Run **FantasyCritic.LocalDatabaseTool**, which seeds your local database with the latest game data (announcements, release dates, scores, etc.) pulled from the live Fantasy Critic site.

The last two services exit on their own when done. You can re-run the compose command at any time to pick up new migrations or refresh game data; it is safe to run repeatedly.

#### 2. Run the web app

The `appsettings.json` for `FantasyCritic.Web` is already pre-configured to connect to the Docker MySQL instance, so no additional configuration is required for a basic setup. Open the solution in your IDE and run `FantasyCritic.Web`, or use the CLI:

```sh
dotnet run --project src/FantasyCritic.Web/FantasyCritic.Web.csproj
```

The site will be available at `https://localhost:44477` by default.

#### 3. Running the integration tests

The integration tests (`FantasyCritic.IntegrationTests`) drive the app through a generated typed HTTP client. That client must be regenerated any time the API changes. The generation requires the Web project to have been built first.

**First time (or after a `dotnet tool restore` is needed):**

```sh
dotnet tool restore
```

**Regenerate the API client, then run the tests:**

On Windows (PowerShell):
```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj
scripts/Regenerate-ApiClient.ps1
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj
```

On Linux/macOS:
```sh
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj
scripts/regenerate-api-client.sh
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj
```

The integration tests spin up the full ASP.NET Core stack in-process and talk to the Docker MySQL database, so the database must be running (step 1) before running the tests.

#### Extra notes

- If you need social login (Google, Discord, Twitch, etc.) you will need to configure the relevant OAuth credentials via [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets). For day-to-day backend development these are optional.
- Database migrations are managed by **FantasyCritic.DatabaseUpdater** (DbUp). New migration scripts go under `src/FantasyCritic.DatabaseUpdater/Scripts/Sequential/` following the existing date-based naming convention. Do **not** hand-edit the database directly.
- I use full Visual Studio for server-side development, but Visual Studio Code works better when working on the client-side (Vue.js) code.
