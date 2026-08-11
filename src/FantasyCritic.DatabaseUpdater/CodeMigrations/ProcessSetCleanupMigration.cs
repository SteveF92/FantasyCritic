using System.Data;
using System.Diagnostics;
using Dapper;
using DbUp.Engine;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.SharedSerialization.Database;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.MySQL;
using FantasyCritic.MySQL.Entities;
using MySqlConnector;
using NodaTime;

namespace FantasyCritic.DatabaseUpdater.CodeMigrations;

/// <summary>
/// Template for a code-based migration: use this when a data fix/cleanup is too complex to express
/// as a plain SQL script but still needs to run (once) as part of a normal DbUp deployment.
///
/// Registered in Program.cs. Once it completes without throwing, it's journaled exactly like a SQL
/// script from Scripts/Sequential — it only ever runs (to completion) once per database, tracked by
/// the name it's registered under.
///
/// Note: this executes against its own repo-managed connection(s), not the same connection/
/// transaction DbUp uses for journaling (available via <paramref name="dbCommandFactory"/> in
/// <see cref="ProvideScript"/> if you need to run raw SQL directly against that connection instead).
/// </summary>
public class ProcessSetCleanupMigration : IScript
{
    private readonly RepositoryConfiguration _repositoryConfiguration;

    public ProcessSetCleanupMigration(RepositoryConfiguration repositoryConfiguration)
    {
        _repositoryConfiguration = repositoryConfiguration;
    }

    public string ProvideScript(Func<IDbCommand> dbCommandFactory)
    {
        // IScript.ProvideScript is synchronous (DbUp has no async IScript overload), so async repo
        // calls have to be made from an async helper and blocked on here.
        return ProvideScriptAsync(dbCommandFactory).GetAwaiter().GetResult();
    }

    private async Task<string> ProvideScriptAsync(Func<IDbCommand> dbCommandFactory)
    {
        IFantasyCriticUserStore userStore = new MySQLFantasyCriticUserStore(_repositoryConfiguration);
        IMasterGameRepo masterGameRepo = new MySQLMasterGameRepo(_repositoryConfiguration, userStore, _repositoryConfiguration.Clock);
        ICombinedDataRepo combinedDataRepo = new MySQLCombinedDataRepo(_repositoryConfiguration, userStore);
        IFantasyCriticRepo fantasyCriticRepo = new MySQLFantasyCriticRepo(_repositoryConfiguration, userStore, masterGameRepo, combinedDataRepo);

        var actionProcessingSets = await fantasyCriticRepo.GetActionProcessingSets();
        var connection = new MySqlConnection(_repositoryConfiguration.ConnectionString);
        await connection.OpenAsync();

        string actionSql = """
                           SELECT tbl_league_action.*, tbl_league_publisher.LeagueID, tbl_league_publisher.Year 
                           FROM tbl_league_action 
                           JOIN tbl_league_publisher on tbl_league_publisher.PublisherID = tbl_league_action.PublisherID
                           WHERE tbl_league_action.Timestamp <= '2022-02-06 00:27:19' AND ActionType IN
                           (
                           'Pickup Successful',
                           'Pickup Failed',
                           'Drop Successful',
                           'Drop Failed',
                           'Counter Pick Pickup Successful',
                           'Counter Pick Pickup Failed'
                           );
                           """;
        var allActionEntities = await connection.QueryAsync<LeagueActionWithLeagueYearEntity>(actionSql);
        var actionsByDate = allActionEntities.ToLookup(x => x.Timestamp.ToEasternDate());
        var datesWithUnassignedActions = allActionEntities.Select(x => x.Timestamp.ToEasternDate()).Distinct().OrderBy(x => x).ToList();


        string bidSql = """
                        SELECT tbl_league_pickupbid.*, tbl_league_publisher.LeagueID, tbl_league_publisher.Year 
                        FROM tbl_league_pickupbid
                        JOIN tbl_league_publisher on tbl_league_publisher.PublisherID = tbl_league_pickupbid.PublisherID
                        WHERE ProcessSetId IS NULL AND Successful IS NOT NULL;
                        """;
        var allBidEntities = await connection.QueryAsync<PickupBidWithLeagueYearEntity>(bidSql);
        var bidsByDate = allBidEntities.ToLookup(x => x.Timestamp.ToEasternDate());

        string dropSql = """
                         SELECT tbl_league_droprequest.*, tbl_league_publisher.LeagueID, tbl_league_publisher.Year
                         FROM tbl_league_droprequest
                         JOIN tbl_league_publisher on tbl_league_publisher.PublisherID = tbl_league_droprequest.PublisherID
                         WHERE ProcessSetId IS NULL AND Successful IS NOT NULL;
                         """;
        var allDropEntities = await connection.QueryAsync<DropRequestWithLeagueYearEntity>(bidSql);
        var dropsByDate = allDropEntities.ToLookup(x => x.Timestamp.ToEasternDate());

        Instant previousProcessInstant = Instant.MinValue;
        LocalDate previousProcessDate = LocalDate.MinIsoValue;
        foreach (var date in datesWithUnassignedActions)
        {
            var actionsOnDate = actionsByDate[date];
            var actionsLeagueLookup = actionsOnDate.ToLookup(x => new LeagueYearKey(x.LeagueID, x.Year));

            var bidsToInclude = GetEntitiesUpToTimestamp(bidsByDate, previousProcessInstant, previousProcessDate);
            var dropsToInclude = GetEntitiesUpToTimestamp(dropsByDate, previousProcessInstant, previousProcessDate);
            if (!bidsToInclude.Any() && !dropsToInclude.Any())
            {
                throw new Exception($"Invalid Date {date}");
            }

            var bidsByLeagueYear = bidsToInclude.GroupToDictionary(x => x.Year);
            var dropsByLeagueYear = dropsToInclude.GroupToDictionary(x => x.Year);
            var siteYears = bidsByLeagueYear.Keys.Concat(dropsByLeagueYear.Keys).OrderBy(x => x).Distinct().ToList();
            var actionProcessingSetsMade = new List<ActionProcessingSetEntity>();

            foreach (var siteYear in siteYears)
            {
                var bidsForSiteYear = bidsByLeagueYear[siteYear];
                var dropsForSiteYear = dropsByLeagueYear[siteYear];
                var actionProcessingSetToMake = CreateActionProcessingSetEntity(siteYear, date, bidsForSiteYear, dropsForSiteYear);
                actionProcessingSetsMade.Add(actionProcessingSetToMake);

                var bidsByLeague = bidsToInclude.GroupBy(x => new LeagueYearKey(x.LeagueID, x.Year));
                foreach (var bidsForLeague in bidsByLeague)
                {
                    var actions = actionsLeagueLookup[bidsForLeague.Key];
                }

                var dropsByLeague = dropsToInclude.GroupBy(x => new LeagueYearKey(x.LeagueID, x.Year));
                foreach (var dropsForLeague in dropsByLeague)
                {
                    var actions = actionsLeagueLookup[dropsForLeague.Key];

                }
            }

            if (!actionProcessingSetsMade.Any())
            {
                continue;
            }

            previousProcessDate = date;
            previousProcessInstant = actionProcessingSetsMade.First().ProcessTime;
        }


        // Unconditional throw so this never gets journaled as applied while under development —
        // safe to re-run against a local/refreshable DB as many times as needed. Remove this once
        // the logic above is verified and you're ready to let it complete for real.
        throw new InvalidOperationException("ProcessSetCleanup is not ready to be marked as done yet.");
    }

    private static ActionProcessingSetEntity CreateActionProcessingSetEntity(int siteYear, LocalDate date, IReadOnlyList<PickupBidWithLeagueYearEntity> bids, IReadOnlyList<DropRequestWithLeagueYearEntity> drops)
    {
        var timeToUse = bids.Select(x => x.Timestamp).Concat(drops.Select(x => x.Timestamp)).OrderBy(x => x).First();

        string namePrefix = siteYear switch
        {
            2019 => "",
        };

        var actionProcessingSetToMake = new ActionProcessingSetEntity()
        {
            ProcessSetID = Guid.NewGuid(),
            ProcessName = "",
            ProcessTime = timeToUse
        };

        return actionProcessingSetToMake;
    }

    private static List<T> GetEntitiesUpToTimestamp<T>(ILookup<LocalDate, T> lookup, Instant upToInstant, LocalDate previousProcessDate) where T : ITimestampEntity
    {
        return new List<T>();
    }
}

internal interface ITimestampEntity
{
    Instant Timestamp { get; }
}

internal class LeagueActionWithLeagueYearEntity : ITimestampEntity
{
    public required Guid PublisherID { get; init; }
    public required Instant Timestamp { get; init; }
    public required string ActionType { get; init; }
    public required string Description { get; init; }
    public required bool ManagerAction { get; init; }
    public Guid LeagueID { get; init; }
    public int Year { get; init; }
}

internal class PickupBidWithLeagueYearEntity : ITimestampEntity
{
    public Guid BidID { get; set; }
    public Guid PublisherID { get; set; }
    public Guid MasterGameID { get; set; }
    public Guid? ConditionalDropMasterGameID { get; set; }
    public bool CounterPick { get; set; }
    public Instant Timestamp { get; set; }
    public int Priority { get; set; }
    public uint BidAmount { get; set; }
    public bool AllowIneligibleSlot { get; set; }
    public bool? Successful { get; set; }
    public Guid? ProcessSetID { get; set; }
    public string? Outcome { get; set; }
    public decimal? ProjectedPointsAtTimeOfBid { get; set; }
    public Guid LeagueID { get; set; }
    public int Year { get; set; }
}

internal class DropRequestWithLeagueYearEntity : ITimestampEntity
{
    public Guid DropRequestID { get; set; }
    public Guid PublisherID { get; set; }
    public Guid MasterGameID { get; set; }
    public Instant Timestamp { get; set; }
    public bool? Successful { get; set; }
    public Guid? ProcessSetID { get; set; }
    public Guid LeagueID { get; set; }
    public int Year { get; set; }
}
