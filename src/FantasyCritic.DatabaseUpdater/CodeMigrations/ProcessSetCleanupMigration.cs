using System.Data;
using Dapper;
using DbUp.Engine;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.MySQL;
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
        var allActionEntities = (await connection.QueryAsync<LeagueActionWithLeagueYearEntity>(actionSql)).ToList();
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
        var allDropEntities = await connection.QueryAsync<DropRequestWithLeagueYearEntity>(dropSql);
        var dropsByDate = allDropEntities.ToLookup(x => x.Timestamp.ToEasternDate());

        Instant previousProcessInstant = Instant.MinValue;
        LocalDate previousProcessDate = LocalDate.MinIsoValue;
        foreach (var date in datesWithUnassignedActions)
        {
            if (date == new LocalDate(2019, 12, 10))
            {
                //I know this is just a test processing set, skip it.
                continue;
            }

            var actionsOnDate = actionsByDate[date].OrderBy(x => x.Timestamp).ToList();
            var actionsLeagueLookup = actionsOnDate.ToLookup(x => new LeagueYearKey(x.LeagueID, x.Year));

            var actionProcessingTimestampToUse = actionsOnDate.OrderBy(x => x.Timestamp).First().Timestamp;

            var bidsToInclude = GetEntitiesUpToTimestamp(bidsByDate, date, actionProcessingTimestampToUse, previousProcessDate, previousProcessInstant);
            var dropsToInclude = GetEntitiesUpToTimestamp(dropsByDate, date, actionProcessingTimestampToUse, previousProcessDate, previousProcessInstant);
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
                var bidsForSiteYear = bidsByLeagueYear.GetValueOrDefault(siteYear, new List<PickupBidWithLeagueYearEntity>());
                var dropsForSiteYear = dropsByLeagueYear.GetValueOrDefault(siteYear, new List<DropRequestWithLeagueYearEntity>());

                var actionProcessingSetToMake = CreateActionProcessingSetEntity(siteYear, date, actionProcessingTimestampToUse, actionsOnDate, bidsForSiteYear, dropsForSiteYear);

                List<PickupBidWithLeagueYearEntity> pickupBidsToUpdate = new List<PickupBidWithLeagueYearEntity>();
                var bidsByLeague = bidsToInclude.GroupBy(x => new LeagueYearKey(x.LeagueID, x.Year));
                foreach (var bidsForLeague in bidsByLeague)
                {
                    var actions = actionsLeagueLookup[bidsForLeague.Key];
                }

                List<DropRequestWithLeagueYearEntity> dropsToUpdate = new List<DropRequestWithLeagueYearEntity>();
                var dropsByLeague = dropsToInclude.GroupBy(x => new LeagueYearKey(x.LeagueID, x.Year));
                foreach (var dropsForLeague in dropsByLeague)
                {
                    var actions = actionsLeagueLookup[dropsForLeague.Key];

                }

                if (pickupBidsToUpdate.Any() || dropsToUpdate.Any())
                {
                    await UpdateDropsAndBids(connection, pickupBidsToUpdate, dropsToUpdate);
                    actionProcessingSetsMade.Add(actionProcessingSetToMake);
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

    private async Task UpdateDropsAndBids(MySqlConnection connection, List<PickupBidWithLeagueYearEntity> bidsToUpdate, List<DropRequestWithLeagueYearEntity> dropsToUpdate)
    {
        var transaction = await connection.BeginTransactionAsync();

        if (bidsToUpdate.Any())
        {
            string bidUpdateSql = """
                                  UPDATE tbl_league_pickupbid UPDATE ProcessSetID = @ProcessSetID, Outcome = @Outcome WHERE BidID = @BidID;
                                  """;
            await connection.ExecuteAsync(bidUpdateSql, bidsToUpdate, transaction);
        }


        if (dropsToUpdate.Any())
        {
            string dropUpdateSql = """
                                  UPDATE tbl_league_droprequest UPDATE ProcessSetID = @ProcessSetID WHERE DropRequestID = @DropRequestID;
                                  """;
            await connection.ExecuteAsync(dropUpdateSql, dropsToUpdate, transaction);
        }

        await transaction.CommitAsync();
    }

    private static ActionProcessingSetEntity CreateActionProcessingSetEntity(int siteYear, LocalDate date, Instant actionProcessingTimeToUse,
        IReadOnlyList<LeagueActionWithLeagueYearEntity> actions, IReadOnlyList<PickupBidWithLeagueYearEntity> bids, IReadOnlyList<DropRequestWithLeagueYearEntity> drops)
    {
        (string namePrefix, ActionProcessingSetType type) = GetActionProcessingSetNamePrefix(siteYear, date, actions, bids, drops);

        var actionProcessingSetToMake = new ActionProcessingSetEntity()
        {
            ProcessSetID = Guid.NewGuid(),
            ProcessName = $"{namePrefix} ({date})",
            ProcessTime = actionProcessingTimeToUse,
            ActionProcessingSetType = type
        };

        return actionProcessingSetToMake;
    }

    private static (string Prefix, ActionProcessingSetType Type) GetActionProcessingSetNamePrefix(int siteYear, LocalDate date,
        IReadOnlyList<LeagueActionWithLeagueYearEntity> actions, 
        IReadOnlyList<PickupBidWithLeagueYearEntity> bids, IReadOnlyList<DropRequestWithLeagueYearEntity> drops)
    {
        if (siteYear == 2019)
        {
            if (drops.Any())
            {
                throw new Exception("Drops didn't exist in 2019");
            }

            return ("Bid Processing", ActionProcessingSetType.Bids);
        }

        var theDayICombinedDropsAndBids = new LocalDate(2020, 12, 19);
        if (siteYear == 2020 && date < theDayICombinedDropsAndBids)
        {
            if (date.DayOfWeek == IsoDayOfWeek.Sunday && drops.Any())
            {
                return ("Drop Processing", ActionProcessingSetType.Drops);
            }

            if (date.DayOfWeek == IsoDayOfWeek.Monday && bids.Any())
            {
                return ("Bid Processing", ActionProcessingSetType.Bids);
            }

            throw new Exception($"Unclear processing set for {date}");
        }

        return ("Drop/Bid Processing", ActionProcessingSetType.All);
    }

    private static List<T> GetEntitiesUpToTimestamp<T>(ILookup<LocalDate, T> lookup, LocalDate processingDate,
        Instant actionProcessingInstant, LocalDate previousProcessDate, Instant previousProcessInstant)
        where T : ITimestampEntity
    {
        var entitiesForRelevantDates = new List<T>();
        var datesBetween = new DateInterval(previousProcessDate, processingDate);
        foreach (var dateToPull in datesBetween)
        {
            entitiesForRelevantDates.AddRange(lookup[dateToPull]);
        }

        var filteredEntities = entitiesForRelevantDates.Where(x => x.Timestamp > previousProcessInstant && x.Timestamp <= actionProcessingInstant).ToList();
        return filteredEntities;
    }
}

public enum ActionProcessingSetType
{
    Bids,
    Drops,
    All
}

internal class ActionProcessingSetEntity
{
    public Guid ProcessSetID { get; set; }
    public Instant ProcessTime { get; set; }
    public string ProcessName { get; set; } = null!;
    public ActionProcessingSetType ActionProcessingSetType { get; set; }
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
