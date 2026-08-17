using System.Data;
using Dapper;
using DbUp.Engine;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Enums;
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
    private IFantasyCriticRepo _fantasyCriticRepo;

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
        _fantasyCriticRepo = new MySQLFantasyCriticRepo(_repositoryConfiguration, userStore, masterGameRepo, combinedDataRepo);

        var masterGames = await masterGameRepo.GetMasterGames();
        var masterGameDictionary = masterGames.ToDictionary(x => x.MasterGameID);
        var actionProcessingSets = await _fantasyCriticRepo.GetActionProcessingSets();

        var connection = new MySqlConnection(_repositoryConfiguration.ConnectionString);
        await connection.OpenAsync();


        //For repeatability in testing
        var epoch = new ZonedDateTime(new LocalDateTime(2022, 2, 13, 0, 0 ,0), DateTimeZone.Utc, new Offset()).ToInstant();
        var newlyCreatedActionProcessingSets = actionProcessingSets.Where(x => x.ProcessTime < epoch).ToList();
        var processSetParam = new
        {
            processSetIDs = newlyCreatedActionProcessingSets.Select(x => x.ProcessSetID).ToList()
        };

        var undoTransaction = await connection.BeginTransactionAsync();
        try
        {
            await connection.ExecuteAsync("UPDATE tbl_league_pickupbid SET ProcessSetId = NULL, Outcome = NULL WHERE ProcessSetId IN @processSetIDs;", processSetParam, transaction: undoTransaction);
            await connection.ExecuteAsync("UPDATE tbl_league_droprequest SET ProcessSetId = NULL WHERE ProcessSetId IN @processSetIDs;", processSetParam, transaction: undoTransaction);
            await connection.ExecuteAsync("DELETE FROM tbl_meta_actionprocessingset WHERE ProcessSetId IN @processSetIDs;", processSetParam, transaction: undoTransaction);
            await undoTransaction.CommitAsync();
        }
        catch (Exception e)
        {
            await undoTransaction.RollbackAsync();
            throw;
        }

        string actionSql = """
                           SELECT tbl_league_action.*, tbl_league_publisher.LeagueID, tbl_league_publisher.Year , tbl_league_publisher.PublisherName
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
                        SELECT tbl_league_pickupbid.*, tbl_league_publisher.LeagueID, tbl_league_publisher.Year, tbl_league_publisher.PublisherName, tbl_mastergame.GameName 
                        FROM tbl_league_pickupbid
                        JOIN tbl_league_publisher on tbl_league_publisher.PublisherID = tbl_league_pickupbid.PublisherID
                        JOIN tbl_mastergame on tbl_mastergame.MasterGameID = tbl_league_pickupbid.MasterGameID
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

        ZonedDateTime previousBidProcessInstant = new LocalDateTime(2017, 2, 1, 12, 0, 0).InZoneStrictly(TimeExtensions.EasternTimeZone);
        LocalDate previousBidProcessDate = new LocalDate(2017, 2, 1);
        ZonedDateTime previousDropProcessInstant = new LocalDateTime(2017, 2, 1, 12, 0, 0).InZoneStrictly(TimeExtensions.EasternTimeZone);
        LocalDate previousDropProcessDate = new LocalDate(2017, 2, 1);

        List<ActionProcessingSetEntity> actionProcessingSetsToInsert = new List<ActionProcessingSetEntity>();
        List<PickupBidWithLeagueYearEntity> pickupBidsToUpdate = new List<PickupBidWithLeagueYearEntity>();
        List<DropRequestWithLeagueYearEntity> dropsToUpdate = new List<DropRequestWithLeagueYearEntity>();

        foreach (var date in datesWithUnassignedActions)
        {
            if (date == new LocalDate(2019, 12, 10))
            {
                //I know this is just a test processing set, skip it.
                continue;
            }

            var actionsOnDate = actionsByDate[date].OrderBy(x => x.Timestamp).ToList();
            var actionsLeagueLookup = actionsOnDate.ToLookup(x => new LeagueYearKey(x.LeagueID, x.Year));

            var bidActionProcessingTimestampToUse = actionsOnDate.Where(x => x.Timestamp.ToEasternDate() == date && x.ActionType.Contains("Pickup")).OrderBy(x => x.Timestamp).FirstOrDefault()?.Timestamp.InZone(TimeExtensions.EasternTimeZone);
            var dropActionProcessingTimestampToUse = actionsOnDate.Where(x => x.Timestamp.ToEasternDate() == date && x.ActionType.Contains("Drop")).OrderBy(x => x.Timestamp).FirstOrDefault()?.Timestamp.InZone(TimeExtensions.EasternTimeZone);
            if (bidActionProcessingTimestampToUse is null)
            {
                if (actionsOnDate.Any(x => x.ActionType.Contains("Pickup")))
                {
                    throw new Exception("Could not find pickup timestamp");
                }
                bidActionProcessingTimestampToUse = dropActionProcessingTimestampToUse;
            }
            if (dropActionProcessingTimestampToUse is null)
            {
                if (actionsOnDate.Any(x => x.ActionType.Contains("Drop")))
                {
                    throw new Exception("Could not find drop timestamp");
                }
                dropActionProcessingTimestampToUse = bidActionProcessingTimestampToUse;
            }

            var bidsToInclude = GetEntitiesUpToTimestamp(bidsByDate, date, bidActionProcessingTimestampToUse.Value, previousBidProcessDate, previousBidProcessInstant);
            var dropsToInclude = GetEntitiesUpToTimestamp(dropsByDate, date, dropActionProcessingTimestampToUse.Value, previousDropProcessDate, previousDropProcessInstant);
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
                if (siteYear == 2019 && date == new LocalDate(2019, 12, 22))
                {
                    //This shouldn't actually be here, it's drop processing for 2020.
                    continue;
                }
                var leagueYearDictionary = await GetLeagueYearDictionaryForYear(siteYear);
                var bidsForSiteYear = bidsByLeagueYear.GetValueOrDefault(siteYear, new List<PickupBidWithLeagueYearEntity>());
                var dropsForSiteYear = dropsByLeagueYear.GetValueOrDefault(siteYear, new List<DropRequestWithLeagueYearEntity>());

                List<ActionProcessingSetEntity> actionProcessingSetsOnDate = GetActionProcessingSetsOnDate(siteYear, date, bidActionProcessingTimestampToUse.Value.ToInstant(), dropActionProcessingTimestampToUse.Value.ToInstant(), bidsForSiteYear, dropsForSiteYear);
                foreach (var actionProcessingSetToMake in actionProcessingSetsOnDate)
                {
                    List<PickupBidWithLeagueYearEntity> pickupBidsInThisSet = new List<PickupBidWithLeagueYearEntity>();
                    List<DropRequestWithLeagueYearEntity> dropsInThisSet = new List<DropRequestWithLeagueYearEntity>();
                    if (actionProcessingSetToMake.ActionProcessingSetType is ActionProcessingSetType.All or ActionProcessingSetType.Bids)
                    {
                        var bidsByLeague = bidsForSiteYear.GroupBy(x => new LeagueYearKey(x.LeagueID, x.Year));
                        foreach (var bidsForLeague in bidsByLeague)
                        {
                            var actionsForLeague = actionsLeagueLookup[bidsForLeague.Key].ToList();
                            var leagueYear = leagueYearDictionary[bidsForLeague.Key.LeagueID];
                            var processedBidsForLeague = GetProcessedBids(actionProcessingSetToMake, bidsForLeague.ToList(), actionsForLeague, masterGameDictionary, leagueYear);
                            pickupBidsInThisSet.AddRange(processedBidsForLeague);
                        }
                    }

                    if (actionProcessingSetToMake.ActionProcessingSetType is ActionProcessingSetType.All or ActionProcessingSetType.Drops)
                    {
                        var dropsByLeague = dropsForSiteYear.GroupBy(x => new LeagueYearKey(x.LeagueID, x.Year));
                        foreach (var dropsForLeague in dropsByLeague)
                        {
                            var processedDropsForLeague = GetProcessedDrops(actionProcessingSetToMake, dropsForLeague.ToList());
                            dropsInThisSet.AddRange(processedDropsForLeague);
                        }
                    }

                    if (pickupBidsInThisSet.Any() || dropsInThisSet.Any())
                    {
                        actionProcessingSetsMade.Add(actionProcessingSetToMake);
                        pickupBidsToUpdate.AddRange(pickupBidsInThisSet);
                        dropsToUpdate.AddRange(dropsInThisSet);
                    }
                }
            }

            if (actionProcessingSetsMade.Any(x => x.ActionProcessingSetType == ActionProcessingSetType.All || x.ActionProcessingSetType == ActionProcessingSetType.Bids))
            {
                previousBidProcessDate = date;
                previousBidProcessInstant = actionProcessingSetsMade.First(x => x.ActionProcessingSetType == ActionProcessingSetType.All || x.ActionProcessingSetType == ActionProcessingSetType.Bids).ProcessTime.InZone(TimeExtensions.EasternTimeZone);
            }
            if (actionProcessingSetsMade.Any(x => x.ActionProcessingSetType == ActionProcessingSetType.All || x.ActionProcessingSetType == ActionProcessingSetType.Drops))
            {
                previousDropProcessDate = date;
                previousDropProcessInstant = actionProcessingSetsMade.First(x => x.ActionProcessingSetType == ActionProcessingSetType.All || x.ActionProcessingSetType == ActionProcessingSetType.Drops).ProcessTime.InZone(TimeExtensions.EasternTimeZone);
            }

            if (!actionProcessingSetsMade.Any())
            {
                continue;
            }

            actionProcessingSetsToInsert.AddRange(actionProcessingSetsMade);
        }

        await UpdateDropsAndBids(connection, actionProcessingSetsToInsert, pickupBidsToUpdate, dropsToUpdate);

        throw new Exception();
        return string.Empty;
    }

    private List<ActionProcessingSetEntity> GetActionProcessingSetsOnDate(int siteYear, LocalDate date, Instant bidActionProcessingTimestampToUse, Instant dropActionProcessingTimestampToUse,
        IReadOnlyList<PickupBidWithLeagueYearEntity> bids, IReadOnlyList<DropRequestWithLeagueYearEntity> drops)
    {
        if (date == new LocalDate(2020, 8, 31))
        {
            //Special case when I processed drops after midnight
            return
            [
                new ActionProcessingSetEntity()
                {
                    ProcessSetID = Guid.NewGuid(),
                    ProcessName = $"Drop Processing ({date})",
                    ProcessTime = dropActionProcessingTimestampToUse,
                    ActionProcessingSetType = ActionProcessingSetType.Drops
                },
                new ActionProcessingSetEntity()
                {
                    ProcessSetID = Guid.NewGuid(),
                    ProcessName = $"Bid Processing ({date})",
                    ProcessTime = bidActionProcessingTimestampToUse,
                    ActionProcessingSetType = ActionProcessingSetType.Bids
                },
            ];
        }

        var actionProcessingSetToMake = CreateActionProcessingSetEntity(siteYear, date, bidActionProcessingTimestampToUse, dropActionProcessingTimestampToUse, bids, drops);
        return [actionProcessingSetToMake];
    }

    private Dictionary<int, Dictionary<Guid, LeagueYear>> _leagueYearDictionaries = new Dictionary<int, Dictionary<Guid, LeagueYear>>();

    private async Task<Dictionary<Guid, LeagueYear>> GetLeagueYearDictionaryForYear(int year)
    {
        if (_leagueYearDictionaries.TryGetValue(year, out var dictionary))
        {
            return dictionary;
        }

        var leagueYears = await _fantasyCriticRepo.GetLeagueYears(year, true);
        var leagueYearDictionary = leagueYears.ToDictionary(x => x.League.LeagueID);
        _leagueYearDictionaries.Add(year, leagueYearDictionary);
        return leagueYearDictionary;
    }

    private List<PickupBidWithLeagueYearEntity> GetProcessedBids(ActionProcessingSetEntity actionProcessingSetToMake,
        List<PickupBidWithLeagueYearEntity> bids, List<LeagueActionWithLeagueYearEntity> actionsForLeague,
        Dictionary<Guid, MasterGame> masterGameDictionary, LeagueYear leagueYear)
    {
        foreach (var bid in bids)
        {
            var masterGame = masterGameDictionary[bid.MasterGameID];
            var allBidsForGame = bids.Where(x => x.MasterGameID == masterGame.MasterGameID).ToList();

            bid.ProcessSetID = actionProcessingSetToMake.ProcessSetID;
            bid.Outcome = GetOutcomeString(bid, allBidsForGame, actionsForLeague, leagueYear);
        }

        return bids;
    }

    private static string GetOutcomeString(PickupBidWithLeagueYearEntity bid, List<PickupBidWithLeagueYearEntity> allBidsForGame,
        List<LeagueActionWithLeagueYearEntity> actionsForLeague, LeagueYear leagueYear)
    {
        var bidActionPairs = GetBidActionPairs(allBidsForGame, actionsForLeague);
        var thisPair = bidActionPairs.Single(x => x.Bid.BidID == bid.BidID);
        var otherBidActionPairs = bidActionPairs.Where(x => x.Bid.BidID != bid.BidID).ToList();
        var usersBidsForThisGame = bidActionPairs
            .Where(x => x.Bid.PublisherID == thisPair.Bid.PublisherID &&
                        x.Bid.MasterGameID == thisPair.Bid.MasterGameID)
            .OrderByDescending(x => x.Bid.Successful)
            .ThenByDescending(x => x.Bid.BidAmount)
            .ThenBy(x => x.Bid.Priority)
            .ToList();
        var usersBestBidForThisGame = usersBidsForThisGame.First();
        var usersOtherBidsForThisGame = usersBidsForThisGame.Where(x => x.Bid.BidID != usersBestBidForThisGame.Bid.BidID).ToList();
        if (usersOtherBidsForThisGame.Select(x => x.Bid.BidID).Contains(bid.BidID))
        {
            return "You cannot have multiple bids for the same game. This bid has been ignored.";
        }

        var validOtherBids = otherBidActionPairs
            .Where(x => x.IsValidBid(usersOtherBidsForThisGame
                .Select(y => y.Bid.BidID).Contains(x.Bid.BidID)))
            .ToList();

        if (bid.BidID.ToString() == "907dacab-e1fe-4a70-9103-8c114c6490eb")
        {
            //This is a weird case where two master games that were later merged were both bid on.
            //Star Wars: Squadrons and Project Maverick (Unannounced Star Wars)
            return "No competing bids for this game.";
        }

        if (bid.Successful == true && !validOtherBids.Any())
        {
            return "No competing bids for this game.";
        }

        if (thisPair.Action.Description.EndsWith("Failure reason: Publisher was outbid."))
        {
            return thisPair.Action.GetTrimmedDescription("Failure reason: ", false);
        }

        if (thisPair.Action.Description.EndsWith("Failure reason: No roster spots available."))
        {
            return thisPair.Action.GetTrimmedDescription("Failure reason: ", false);
        }

        if (thisPair.Action.Description.EndsWith("Failure reason: Not enough budget."))
        {
            return thisPair.Action.GetTrimmedDescription("Failure reason: ", false);
        }

        if (bid.ConditionalDropMasterGameID.HasValue && thisPair.Action.Description.Contains("Attempted to conditionally drop"))
        {
            return thisPair.Action.GetTrimmedDescription("Failure reason: ", false);
        }

        if (bid.Successful == false && validOtherBids.Any(x => x.Bid.BidAmount == bid.BidAmount))
        {
            return "Bid lost on tiebreakers.";
        }

        if (bid.Successful == false && thisPair.Action.Description.Contains("Failure reason: Bid is below the minimum bid amount."))
        {
            return "Bid is below the minimum bid amount.";
        }

        if (bid.Successful == true && validOtherBids.All(x => x.Bid.BidAmount < bid.BidAmount))
        {
            return "This bid was the highest bid.";
        }

        if (bid.Successful == true && validOtherBids.All(x => x.Bid.BidAmount <= bid.BidAmount))
        {
            if (leagueYear.Options.TiebreakSystem.Equals(TiebreakSystem.LowestProjectedPoints))
            {
                return "This publisher has the lowest projected points. (Not including this game)";
            }
            if (leagueYear.Options.TiebreakSystem.Equals(TiebreakSystem.EarliestBid))
            {
                return "This bid was placed earliest.";
            }
        }

        if (thisPair.Action.Description.Contains("Failure reason: Game is no longer eligible"))
        {
            return thisPair.Action.GetTrimmedDescription("Failure reason: ", false);
        }

        if (thisPair.Action.Description.Contains("Failure reason: No roster spots available"))
        {
            return thisPair.Action.GetTrimmedDescription("Failure reason: ", false);
        }

        throw new Exception("Unknown situation");
    }

    private static List<BidActionPair> GetBidActionPairs(List<PickupBidWithLeagueYearEntity> bids, List<LeagueActionWithLeagueYearEntity> actions)
    {
        if (!actions.Any())
        {
            throw new Exception();
        }
        var bidActionPairs = new List<BidActionPair>();

        foreach (var bid in bids)
        {
            List<string> possibleGameNames = [bid.GameName];
            if (ProcessSetCleanupResources.OldGameNameMappings.TryGetValue(bid.GameName, out var oldNames))
            {
                possibleGameNames.AddRange(oldNames);
            }

            var matchingActionsForGame = actions.Where(x => possibleGameNames.Any(y => x.Description.Contains($"'{y}'"))).ToList();
            var matchingActionsForPublisher = matchingActionsForGame.Where(x => x.PublisherID == bid.PublisherID).ToList();
            var matchingActionForPublisher = matchingActionsForPublisher.FirstOrDefault(x => x.SuccessMatchesBid(bid));
            if (matchingActionForPublisher is null)
            {
                throw new Exception("Cannot match game");
            }
            bidActionPairs.Add(new BidActionPair(bid, matchingActionForPublisher));
        }

        return bidActionPairs;
    }

    private List<DropRequestWithLeagueYearEntity> GetProcessedDrops(ActionProcessingSetEntity actionProcessingSetToMake, 
        List<DropRequestWithLeagueYearEntity> drops)
    {
        foreach (var drop in drops)
        {
            drop.ProcessSetID = actionProcessingSetToMake.ProcessSetID;
        }

        return drops;
    }

    private async Task UpdateDropsAndBids(MySqlConnection connection, List<ActionProcessingSetEntity> actionProcessingSetsToInsert, 
        List<PickupBidWithLeagueYearEntity> bidsToUpdate, List<DropRequestWithLeagueYearEntity> dropsToUpdate)
    {
        var transaction = await connection.BeginTransactionAsync();

        try
        {
            if (actionProcessingSetsToInsert.Any())
            {
                await connection.BulkInsertAsync(actionProcessingSetsToInsert, "tbl_meta_actionprocessingset", 500, transaction, ["ActionProcessingSetType", "EasternDateTime"]);
            }

            if (bidsToUpdate.Any())
            {
                string bidUpdateSql = """
                                      UPDATE tbl_league_pickupbid SET ProcessSetID = @ProcessSetID, Outcome = @Outcome WHERE BidID = @BidID;
                                      """;
                await connection.ExecuteAsync(bidUpdateSql, bidsToUpdate, transaction);
            }

            if (dropsToUpdate.Any())
            {
                string dropUpdateSql = """
                                       UPDATE tbl_league_droprequest SET ProcessSetID = @ProcessSetID WHERE DropRequestID = @DropRequestID;
                                       """;
                await connection.ExecuteAsync(dropUpdateSql, dropsToUpdate, transaction);
            }

            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            Console.WriteLine(e);
            throw;
        }
    }

    private static ActionProcessingSetEntity CreateActionProcessingSetEntity(int siteYear, LocalDate date, Instant bidActionProcessingTimestampToUse, Instant dropActionProcessingTimestampToUse,
        IReadOnlyList<PickupBidWithLeagueYearEntity> bids, IReadOnlyList<DropRequestWithLeagueYearEntity> drops)
    {
        (string namePrefix, ActionProcessingSetType type) = GetActionProcessingSetNamePrefix(siteYear, date, bids, drops);

        var actionProcessingSetToMake = new ActionProcessingSetEntity()
        {
            ProcessSetID = Guid.NewGuid(),
            ProcessName = $"{namePrefix} ({date})",
            ProcessTime = (type == ActionProcessingSetType.All || type == ActionProcessingSetType.Bids) ? bidActionProcessingTimestampToUse : dropActionProcessingTimestampToUse,
            ActionProcessingSetType = type
        };

        return actionProcessingSetToMake;
    }

    private static (string Prefix, ActionProcessingSetType Type) GetActionProcessingSetNamePrefix(int siteYear, LocalDate date,
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
        ZonedDateTime actionProcessingInstant, LocalDate previousProcessDate, ZonedDateTime previousProcessInstant)
        where T : ITimestampEntity
    {
        var entitiesForRelevantDates = new List<T>();
        var datesBetween = new DateInterval(previousProcessDate, processingDate);
        foreach (var dateToPull in datesBetween)
        {
            entitiesForRelevantDates.AddRange(lookup[dateToPull]);
        }

        var filteredEntities = entitiesForRelevantDates.Where(x => x.Timestamp > previousProcessInstant.ToInstant() && x.Timestamp <= actionProcessingInstant.ToInstant()).ToList();
        return filteredEntities;
    }
}

public enum ActionProcessingSetType
{
    Bids,
    Drops,
    All
}

internal record BidActionPair(PickupBidWithLeagueYearEntity Bid, LeagueActionWithLeagueYearEntity Action)
{
    public bool IsValidBid(bool isDuplicateBid)
    {
        if (isDuplicateBid)
        {
            return false;
        }

        if (Action.Description.EndsWith("Failure reason: No roster spots available."))
        {
            return false;
        }

        if (Action.Description.EndsWith("Failure reason: Not enough budget."))
        {
            return false;
        }

        return true;
    }
}

internal class ActionProcessingSetEntity
{
    public Guid ProcessSetID { get; set; }
    public Instant ProcessTime { get; set; }
    public string ProcessName { get; set; } = null!;
    public ActionProcessingSetType ActionProcessingSetType { get; set; }
    public string EasternDateTime => ProcessTime.ToEasternDateTime().ToString();

    public override string ToString()
    {
        return ProcessName;
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

    public string PublisherName { get; init; }
    public string EasternDateTime => Timestamp.ToEasternDateTime().ToString();

    public override string ToString()
    {
        return $"{ActionType}|{PublisherName}|{Description}";
    }

    public string GetTrimmedDescription(string startFromSubstring, bool includePrefix)
    {
        var indexOf = Description.IndexOf(startFromSubstring);
        if (indexOf == -1)
        {
            return Description;
        }

        if (includePrefix)
        {
            return Description.Substring(indexOf);
        }
        return Description.Substring(indexOf + startFromSubstring.Length);
    }

    public bool SuccessMatchesBid(PickupBidWithLeagueYearEntity bid)
    {
        return bid.Successful == Description.StartsWith("Acquired");
    }
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

    public string PublisherName { get; set; }
    public string GameName { get; set; }
    public string EasternDateTime => Timestamp.ToEasternDateTime().ToString();

    public override string ToString()
    {
        return $"{PublisherName}|{GameName}|{CounterPick}|{BidAmount}|{Priority}|{Successful}";
    }
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
