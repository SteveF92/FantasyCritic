# MasterGameUpdater Year/Quarter Sync Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Extend `FantasyCritic.MasterGameUpdater` to sync `tbl_meta_supportedyear` and `tbl_royale_supportedquarter` from the production API into the local Docker DB, so integration tests never run against a stale set of years or quarters.

**Architecture:** A new `MySQLLocalSetupSyncer` class in `FantasyCritic.MySQL/SyncingRepos/` owns the SQL upsert logic. `MasterGameUpdater/Program.cs` gets two new methods that call the production API and hand results to the syncer. No changes to `DatabaseUpdater`, no new projects.

**Tech Stack:** C# / .NET 10, Dapper, MySqlConnector, NodaTime, Newtonsoft.Json (already used by the tool for master game deserialization).

---

## File Map

| Action | File | Purpose |
|--------|------|---------|
| Create | `src/FantasyCritic.MySQL/SyncingRepos/MySQLLocalSetupSyncer.cs` | `UpsertSupportedYears` and `UpsertRoyaleYearQuarters` SQL upserts |
| Modify | `src/FantasyCritic.MasterGameUpdater/Program.cs` | Add `UpdateSupportedYears()` and `UpdateRoyaleQuarters()` + call them from `Main` |

---

## Task 1: Create `MySQLLocalSetupSyncer`

**Files:**
- Create: `src/FantasyCritic.MySQL/SyncingRepos/MySQLLocalSetupSyncer.cs`

- [ ] **Step 1: Create the file**

```csharp
using FantasyCritic.MySQL.Entities;
using Serilog;

namespace FantasyCritic.MySQL.SyncingRepos;

public class MySQLLocalSetupSyncer
{
    private static readonly ILogger _logger = Log.ForContext<MySQLLocalSetupSyncer>();

    private readonly string _connectionString;

    public MySQLLocalSetupSyncer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task UpsertSupportedYears(IEnumerable<SupportedYearEntity> years)
    {
        const string sql = """
            INSERT INTO tbl_meta_supportedyear
                (Year, OpenForCreation, OpenForPlay, OpenForBetaUsers, StartDate, Finished)
            VALUES
                (@Year, @OpenForCreation, @OpenForPlay, @OpenForBetaUsers, @StartDate, @Finished)
            ON DUPLICATE KEY UPDATE
                OpenForCreation  = VALUES(OpenForCreation),
                OpenForPlay      = VALUES(OpenForPlay),
                OpenForBetaUsers = VALUES(OpenForBetaUsers),
                StartDate        = VALUES(StartDate),
                Finished         = VALUES(Finished);
            """;

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, years);
        _logger.Information("Upserted {Count} supported years.", years.Count());
    }

    public async Task UpsertRoyaleYearQuarters(IEnumerable<RoyaleYearQuarterEntity> quarters)
    {
        const string sql = """
            INSERT INTO tbl_royale_supportedquarter
                (Year, Quarter, OpenForPlay, Finished, WinningUser)
            VALUES
                (@Year, @Quarter, @OpenForPlay, @Finished, NULL)
            ON DUPLICATE KEY UPDATE
                OpenForPlay = VALUES(OpenForPlay),
                Finished    = VALUES(Finished);
            """;

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, quarters);
        _logger.Information("Upserted {Count} royale year quarters.", quarters.Count());
    }
}
```

Note: `SupportedYearEntity` already exists at `src/FantasyCritic.MySQL/Entities/SupportedYearEntity.cs`. `RoyaleYearQuarterEntity` already exists at `src/FantasyCritic.MySQL/Entities/RoyaleYearQuarterEntity.cs` — we reuse it with `WinningUser = null`, which the SQL ignores on upsert anyway.

- [ ] **Step 2: Verify it compiles**

```
dotnet build src/FantasyCritic.MySQL/FantasyCritic.MySQL.csproj
```

Expected: no errors.

- [ ] **Step 3: Commit**

```
git add src/FantasyCritic.MySQL/SyncingRepos/MySQLLocalSetupSyncer.cs
git commit -m "Add MySQLLocalSetupSyncer with year and quarter upsert methods."
```

---

## Task 2: Add `UpdateSupportedYears` and `UpdateRoyaleQuarters` to `Program.cs`

**Files:**
- Modify: `src/FantasyCritic.MasterGameUpdater/Program.cs`

- [ ] **Step 1: Add private response records and the two fetch+upsert methods**

The full updated `Program.cs` should look like this (only the additions are called out with comments):

```csharp
using DiscordDotNetUtilities;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.GG;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Patreon;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.SharedSerialization.API;
using FantasyCritic.MySQL;
using FantasyCritic.MySQL.DapperTypeMaps;
using FantasyCritic.MySQL.Entities;
using FantasyCritic.MySQL.SyncingRepos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using Serilog;
using System.ComponentModel.Design;
using System.Reflection;

namespace FantasyCritic.MasterGameUpdater;

public static class Program
{
    private static string _localConnectionString = null!;
    private static string _baseAddress = null!;
    private static Guid _addedByUserIDOverride;

    private static readonly IClock _clock = SystemClock.Instance;

    static async Task Main()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .AddEnvironmentVariables()
            .Build();

        _localConnectionString = configuration["localConnectionString"]!;
        _baseAddress = configuration["baseAddress"]!;
        _addedByUserIDOverride = Guid.Parse(configuration["addedByUserIDOverride"]!);

        JsonConvert.DefaultSettings = () => new JsonSerializerSettings().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        DapperNodaTimeSetup.SetupDapperNodaTimeMappings();

        await UpdateMasterGames();
        await UpdateSupportedYears();
        await UpdateRoyaleQuarters();
    }

    // ── existing master game methods (unchanged) ──────────────────────────

    private static async Task UpdateMasterGames()
    {
        RepositoryConfiguration localRepoConfig = new RepositoryConfiguration(_localConnectionString, _clock);
        MySQLFantasyCriticUserStore localUserStore = new MySQLFantasyCriticUserStore(localRepoConfig);
        MySQLMasterGameRepo localMasterGameRepo = new MySQLMasterGameRepo(localRepoConfig, localUserStore, _clock);
        MySQLMasterGameUpdater gameUpdater = new MySQLMasterGameUpdater(_localConnectionString);
        AdminService localAdminService = GetAdminService();

        Log.Information("Getting master games from production");
        var productionMasterGameTags = await GetTagsFromAPI();
        var productionMasterGames = await GetMasterGamesFromAPI(productionMasterGameTags);
        var localMasterGameTags = await localMasterGameRepo.GetMasterGameTags();
        var localMasterGames = await localMasterGameRepo.GetMasterGames();
        await gameUpdater.UpdateMasterGames(productionMasterGameTags, productionMasterGames, localMasterGameTags, localMasterGames, _addedByUserIDOverride);
        await localAdminService.RefreshCaches();
    }

    private static async Task<IReadOnlyList<MasterGameTag>> GetTagsFromAPI()
    {
        HttpClient client = new HttpClient() { BaseAddress = new Uri(_baseAddress) };
        var tagsString = await client.GetStringAsync("api/Game/GetMasterGameTags");
        var objects = JsonConvert.DeserializeObject<List<MasterGameTagViewModel>>(tagsString)!;
        var domains = objects.Select(x => x.ToDomain()).ToList();
        return domains;
    }

    private static async Task<IReadOnlyList<MasterGame>> GetMasterGamesFromAPI(IReadOnlyList<MasterGameTag> tags)
    {
        var tagDictionary = tags.ToDictionary(x => x.Name);
        HttpClient client = new HttpClient() { BaseAddress = new Uri(_baseAddress) };
        var gamesString = await client.GetStringAsync("api/Game/MasterGame");
        var objects = JsonConvert.DeserializeObject<List<MasterGameViewModel>>(gamesString)!;
        var domains = objects.Select(x => x.ToDomain(tagDictionary)).ToList();
        return domains;
    }

    private static AdminService GetAdminService()
    {
        FantasyCriticUserManager userManager = null!;
        RepositoryConfiguration localRepoConfig = new RepositoryConfiguration(_localConnectionString, _clock);
        IFantasyCriticUserStore localUserStore = new MySQLFantasyCriticUserStore(localRepoConfig);
        IMasterGameRepo masterGameRepo = new MySQLMasterGameRepo(localRepoConfig, localUserStore, _clock);
        ICombinedDataRepo combinedDataRepo = new MySQLCombinedDataRepo(localRepoConfig, localUserStore);
        IFantasyCriticRepo fantasyCriticRepo = new MySQLFantasyCriticRepo(localRepoConfig, localUserStore, masterGameRepo, combinedDataRepo);
        IConferenceRepo conferenceRepo = new MySQLConferenceRepo(localRepoConfig, localUserStore, masterGameRepo, combinedDataRepo);
        IDiscordRepo discordRepo = new MySQLDiscordRepo(localRepoConfig, fantasyCriticRepo, masterGameRepo, conferenceRepo, combinedDataRepo, _clock);
        IRoyaleRepo royaleRepo = new MySQLRoyaleRepo(localRepoConfig, localUserStore, masterGameRepo);
        IDailyStatsRepo dailyStatsRepo = new MySQLDailyStatsRepo(localRepoConfig, fantasyCriticRepo, royaleRepo);
        DiscordPushService discordPushService = new DiscordPushService(new FantasyCriticDiscordConfiguration("", _baseAddress, true, null), _clock, new ServiceContainer(), new DiscordFormatter());
        InterLeagueService interLeagueService = new InterLeagueService(fantasyCriticRepo, combinedDataRepo, masterGameRepo, _clock, discordPushService);
        LeagueMemberService leagueMemberService = new LeagueMemberService(null!, fantasyCriticRepo, combinedDataRepo);
        GameAcquisitionService gameAcquisitionService = new GameAcquisitionService(fantasyCriticRepo, masterGameRepo, _clock, discordPushService);
        FantasyCriticService fantasyCriticService = new FantasyCriticService(leagueMemberService, interLeagueService, discordPushService, gameAcquisitionService, fantasyCriticRepo, combinedDataRepo, discordRepo, _clock);
        IOpenCriticService openCriticService = null!;
        IGGService ggService = null!;
        PatreonService patreonService = null!;
        IRDSManager rdsManager = null!;
        RoyaleService royaleService = new RoyaleService(royaleRepo, _clock, masterGameRepo);
        IHypeFactorService hypeFactorService = new DefaultHypeFactorService();

        return new AdminService(fantasyCriticService, userManager, fantasyCriticRepo, masterGameRepo, interLeagueService,
            openCriticService, ggService, patreonService, _clock, rdsManager, royaleService, hypeFactorService, discordPushService, discordRepo, dailyStatsRepo);
    }

    // ── new: supported years ──────────────────────────────────────────────

    private static async Task UpdateSupportedYears()
    {
        Log.Information("Getting supported years from production");
        HttpClient client = new HttpClient() { BaseAddress = new Uri(_baseAddress) };
        var json = await client.GetStringAsync("api/Game/SupportedYears");
        var responses = JsonConvert.DeserializeObject<List<SupportedYearResponse>>(json)!;

        var entities = responses.Select(r => new SupportedYearEntity
        {
            Year             = r.Year,
            OpenForCreation  = r.OpenForCreation,
            OpenForPlay      = r.OpenForPlay,
            OpenForBetaUsers = false,
            StartDate        = LocalDate.FromDateTime(r.StartDate),
            Finished         = r.Finished
        }).ToList();

        var syncer = new MySQLLocalSetupSyncer(_localConnectionString);
        await syncer.UpsertSupportedYears(entities);
    }

    // ── new: royale year quarters ─────────────────────────────────────────

    private static async Task UpdateRoyaleQuarters()
    {
        Log.Information("Getting royale year quarters from production");
        HttpClient client = new HttpClient() { BaseAddress = new Uri(_baseAddress) };
        var json = await client.GetStringAsync("api/Royale/RoyaleQuarters");
        var responses = JsonConvert.DeserializeObject<List<RoyaleQuarterResponse>>(json)!;

        var entities = responses.Select(r => new RoyaleYearQuarterEntity
        {
            Year        = r.Year,
            Quarter     = r.Quarter,
            OpenForPlay = r.OpenForPlay,
            Finished    = r.Finished,
            WinningUser = null
        }).ToList();

        var syncer = new MySQLLocalSetupSyncer(_localConnectionString);
        await syncer.UpsertRoyaleYearQuarters(entities);
    }

    // ── local deserialization records (production API shapes) ─────────────

    private record SupportedYearResponse(int Year, bool OpenForCreation, bool OpenForPlay,
        DateTime StartDate, bool Finished);

    private record RoyaleQuarterResponse(int Year, int Quarter, bool OpenForPlay, bool Finished);
}
```

- [ ] **Step 2: Build to verify no compile errors**

```
dotnet build src/FantasyCritic.MasterGameUpdater/FantasyCritic.MasterGameUpdater.csproj
```

Expected: no errors.

- [ ] **Step 3: Run against local Docker DB and verify results**

With the Docker DB running (port 3307):

```
dotnet run --project src/FantasyCritic.MasterGameUpdater
```

Then connect to the local DB and verify:

```sql
SELECT * FROM tbl_meta_supportedyear ORDER BY Year;
-- Should include 2026 with OpenForPlay = 1, Finished = 0

SELECT * FROM tbl_royale_supportedquarter ORDER BY Year, Quarter;
-- Should include the current quarter (e.g. 2026 Q2) with OpenForPlay = 1, Finished = 0

-- Re-run the tool to confirm idempotency (no duplicate key errors, no row count changes)
```

- [ ] **Step 4: Commit**

```
git add src/FantasyCritic.MasterGameUpdater/Program.cs
git commit -m "Sync supported years and royale quarters from production API in MasterGameUpdater."
```
