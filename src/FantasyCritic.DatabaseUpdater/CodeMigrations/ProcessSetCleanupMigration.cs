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
    private const string FailureReasonPrefix = "Failure reason: ";

    private readonly List<string> _tiebreakDiagnostics = new();
    private readonly List<string> _conflictDiagnostics = new();
    private readonly List<string> _selfCompetitionDiagnostics = new();
    private readonly HashSet<string> _unmatchedGameDiagnostics = new();

    private readonly RepositoryConfiguration _repositoryConfiguration;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IMasterGameRepo _masterGameRepo;

    public ProcessSetCleanupMigration(RepositoryConfiguration repositoryConfiguration)
    {
        _repositoryConfiguration = repositoryConfiguration;

        IFantasyCriticUserStore userStore = new MySQLFantasyCriticUserStore(repositoryConfiguration);
        _masterGameRepo = new MySQLMasterGameRepo(repositoryConfiguration, userStore, repositoryConfiguration.Clock);
        ICombinedDataRepo combinedDataRepo = new MySQLCombinedDataRepo(repositoryConfiguration, userStore);
        _fantasyCriticRepo = new MySQLFantasyCriticRepo(repositoryConfiguration, userStore, _masterGameRepo, combinedDataRepo);
    }

    public string ProvideScript(Func<IDbCommand> dbCommandFactory)
    {
        // IScript.ProvideScript is synchronous (DbUp has no async IScript overload), so async repo
        // calls have to be made from an async helper and blocked on here.
        return ProvideScriptAsync(dbCommandFactory).GetAwaiter().GetResult();
    }

    private async Task<string> ProvideScriptAsync(Func<IDbCommand> dbCommandFactory)
    {
        var masterGames = await _masterGameRepo.GetMasterGames();
        var masterGameDictionary = masterGames.ToDictionary(x => x.MasterGameID);
        var actionProcessingSets = await _fantasyCriticRepo.GetActionProcessingSets();

        var connection = new MySqlConnection(_repositoryConfiguration.ConnectionString);
        await connection.OpenAsync();


        //For repeatability in testing
        var epoch = new ZonedDateTime(new LocalDateTime(2022, 2, 11, 0, 0 ,0), DateTimeZone.Utc, new Offset()).ToInstant();
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
                           WHERE tbl_league_action.Timestamp <= '2022-02-07 00:00:00' AND ActionType IN
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
        var allBidEntities = (await connection.QueryAsync<PickupBidWithLeagueYearEntity>(bidSql)).ToList();
        var bidsByDate = allBidEntities.ToLookup(x => x.Timestamp.ToEasternDate());

        string dropSql = """
                         SELECT tbl_league_droprequest.*, tbl_league_publisher.LeagueID, tbl_league_publisher.Year
                         FROM tbl_league_droprequest
                         JOIN tbl_league_publisher on tbl_league_publisher.PublisherID = tbl_league_droprequest.PublisherID
                         WHERE ProcessSetId IS NULL AND Successful IS NOT NULL;
                         """;
        var allDropEntities = (await connection.QueryAsync<DropRequestWithLeagueYearEntity>(dropSql)).ToList();
        var dropsByDate = allDropEntities.ToLookup(x => x.Timestamp.ToEasternDate());

        if (!allBidEntities.Any() && !allDropEntities.Any())
        {
            //Either this already ran and the journal insert didn't stick, or this database never had the old data.
            return string.Empty;
        }

        ZonedDateTime previousBidProcessInstant = new LocalDateTime(2017, 2, 1, 12, 0, 0).InZoneStrictly(TimeExtensions.EasternTimeZone);
        LocalDate previousBidProcessDate = new LocalDate(2017, 2, 1);
        ZonedDateTime previousDropProcessInstant = new LocalDateTime(2017, 2, 1, 12, 0, 0).InZoneStrictly(TimeExtensions.EasternTimeZone);
        LocalDate previousDropProcessDate = new LocalDate(2017, 2, 1);

        List<ActionProcessingSetEntity> actionProcessingSetsToInsert = new List<ActionProcessingSetEntity>();
        List<PickupBidWithLeagueYearEntity> pickupBidsToUpdate = new List<PickupBidWithLeagueYearEntity>();
        List<DropRequestWithLeagueYearEntity> dropsToUpdate = new List<DropRequestWithLeagueYearEntity>();
        HashSet<Guid> deliberatelySkippedBidIDs = new HashSet<Guid>();
        HashSet<Guid> deliberatelySkippedDropIDs = new HashSet<Guid>();

        foreach (var date in datesWithUnassignedActions)
        {
            if (date == new LocalDate(2019, 12, 10))
            {
                //I know this is just a test processing set, skip it.
                continue;
            }

            var actionsOnDate = actionsByDate[date].OrderBy(x => x.Timestamp).ToList();
            var actionsLeagueLookup = actionsOnDate.ToLookup(x => new LeagueYearKey(x.LeagueID, x.Year));

            //A date can have only pickups or only drops, in which case both windows close at the same moment.
            Instant? firstPickupActionTime = actionsOnDate.Where(x => x.ActionType.Contains("Pickup")).Select(x => (Instant?)x.Timestamp).Min();
            Instant? firstDropActionTime = actionsOnDate.Where(x => x.ActionType.Contains("Drop")).Select(x => (Instant?)x.Timestamp).Min();
            if (firstPickupActionTime is null && firstDropActionTime is null)
            {
                throw new Exception($"No pickup or drop actions on {date}");
            }

            var bidActionProcessingTimestampToUse = (firstPickupActionTime ?? firstDropActionTime!.Value).InZone(TimeExtensions.EasternTimeZone);
            var dropActionProcessingTimestampToUse = (firstDropActionTime ?? firstPickupActionTime!.Value).InZone(TimeExtensions.EasternTimeZone);

            var bidsToInclude = GetEntitiesUpToTimestamp(bidsByDate, date, bidActionProcessingTimestampToUse, previousBidProcessDate, previousBidProcessInstant);
            var dropsToInclude = GetEntitiesUpToTimestamp(dropsByDate, date, dropActionProcessingTimestampToUse, previousDropProcessDate, previousDropProcessInstant);
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
                if (siteYear == 2019 && date == new LocalDate(2019, 12, 22))
                {
                    //This shouldn't actually be here, it's drop processing for 2020.
                    deliberatelySkippedBidIDs.UnionWith(bidsForSiteYear.Select(x => x.BidID));
                    deliberatelySkippedDropIDs.UnionWith(dropsForSiteYear.Select(x => x.DropRequestID));
                    continue;
                }

                var leagueYearDictionary = await GetLeagueYearDictionaryForYear(siteYear);

                List<ActionProcessingSetEntity> actionProcessingSetsOnDate = GetActionProcessingSetsOnDate(siteYear, date, bidActionProcessingTimestampToUse.ToInstant(), dropActionProcessingTimestampToUse.ToInstant(), bidsForSiteYear, dropsForSiteYear);
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

        Console.WriteLine($"TIEBREAK DIAGNOSTICS: {_tiebreakDiagnostics.Count} winning bids were decided by a tie at the top bid amount.");
        foreach (var tiebreakDiagnostic in _tiebreakDiagnostics)
        {
            Console.WriteLine(tiebreakDiagnostic);
        }

        Console.WriteLine($"CONFLICT DIAGNOSTICS: {_conflictDiagnostics.Count} winning bids were beaten by a higher valid bid in the same run.");
        foreach (var conflictDiagnostic in _conflictDiagnostics)
        {
            Console.WriteLine(conflictDiagnostic);
        }

        Console.WriteLine($"SELF-COMPETITION DIAGNOSTICS: {_selfCompetitionDiagnostics.Count} winning bids competed against another bid from the same publisher.");
        foreach (var selfCompetitionDiagnostic in _selfCompetitionDiagnostics)
        {
            Console.WriteLine(selfCompetitionDiagnostic);
        }

        Console.WriteLine($"UNMATCHED GAME DIAGNOSTICS: {_unmatchedGameDiagnostics.Count} bids could not be matched to a league action.");
        foreach (var unmatchedGameDiagnostic in _unmatchedGameDiagnostics)
        {
            Console.WriteLine(unmatchedGameDiagnostic);
        }

        VerifyEverythingWasAssigned(allBidEntities, allDropEntities, deliberatelySkippedBidIDs, deliberatelySkippedDropIDs);
        await UpdateDropsAndBids(connection, actionProcessingSetsToInsert, pickupBidsToUpdate, dropsToUpdate);

        return string.Empty;
    }

    private static void VerifyEverythingWasAssigned(IReadOnlyList<PickupBidWithLeagueYearEntity> allBidEntities,
        IReadOnlyList<DropRequestWithLeagueYearEntity> allDropEntities, HashSet<Guid> deliberatelySkippedBidIDs, HashSet<Guid> deliberatelySkippedDropIDs)
    {
        var unassignedBids = allBidEntities.Where(x => x.ProcessSetID is null && !deliberatelySkippedBidIDs.Contains(x.BidID)).ToList();
        var unassignedDrops = allDropEntities.Where(x => x.ProcessSetID is null && !deliberatelySkippedDropIDs.Contains(x.DropRequestID)).ToList();
        if (!unassignedBids.Any() && !unassignedDrops.Any())
        {
            return;
        }

        var bidDates = string.Join(", ", unassignedBids.Select(x => x.Timestamp.ToEasternDate()).Distinct().OrderBy(x => x));
        var dropDates = string.Join(", ", unassignedDrops.Select(x => x.Timestamp.ToEasternDate()).Distinct().OrderBy(x => x));
        throw new Exception($"{unassignedBids.Count} bids and {unassignedDrops.Count} drops were never assigned to a process set. " +
                            $"Bid dates: [{bidDates}]. Drop dates: [{dropDates}].");
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

    private string GetOutcomeString(PickupBidWithLeagueYearEntity bid, List<PickupBidWithLeagueYearEntity> allBidsForGame,
        List<LeagueActionWithLeagueYearEntity> actionsForLeague, LeagueYear leagueYear)
    {
        var bidActionPairs = GetBidActionPairs(allBidsForGame, actionsForLeague);
        var thisPair = bidActionPairs.SingleOrDefault(x => x.Bid.BidID == bid.BidID);
        if (thisPair is null)
        {
            return "UNMATCHED-DIAGNOSTIC-PLACEHOLDER";
        }

        //A failed bid's outcome was stored verbatim as the failure reason on its action, so there is nothing to infer.
        if (bid.Successful == false)
        {
            if (!thisPair.Action.Description.Contains(FailureReasonPrefix))
            {
                throw new Exception($"Failed bid {bid.BidID} has no failure reason: {thisPair.Action.Description}");
            }

            return thisPair.Action.GetTrimmedDescription(FailureReasonPrefix, false);
        }

        if (ProcessSetCleanupResources.BidsWithNoRealCompetition.Contains(bid.BidID))
        {
            return "No competing bids for this game.";
        }

        var validOtherBids = bidActionPairs.Where(x => x.Bid.BidID != bid.BidID && x.WasAValidBid()).ToList();
        var selfCompetingBids = validOtherBids.Where(x => x.Bid.PublisherID == bid.PublisherID).ToList();
        if (selfCompetingBids.Any())
        {
            _selfCompetitionDiagnostics.Add($"{leagueYear.Key} | '{bid.GameName}' | {bid.BidID} ${bid.BidAmount} ({bid.PublisherName}) | Against itself: " +
                                            string.Join(", ", selfCompetingBids.Select(x => $"{x.Bid.BidID} ${x.Bid.BidAmount}")));
        }

        if (!validOtherBids.Any())
        {
            return "No competing bids for this game.";
        }

        if (validOtherBids.All(x => x.Bid.BidAmount < bid.BidAmount))
        {
            return "This bid was the highest bid.";
        }

        var higherBids = validOtherBids.Where(x => x.Bid.BidAmount > bid.BidAmount).ToList();
        if (higherBids.Any())
        {
            _conflictDiagnostics.Add($"{leagueYear.Key} | '{bid.GameName}' | Won: {bid.BidID} ${bid.BidAmount} ({bid.PublisherName}) | Beaten by: " +
                                     string.Join(", ", higherBids.Select(x => $"${x.Bid.BidAmount} ({x.Bid.PublisherName}){(x.Bid.PublisherID == bid.PublisherID ? " SAME-PUBLISHER" : "")}")));
            return "No competing bids for this game.";
        }

        var tiedBids = validOtherBids.Where(x => x.Bid.BidAmount == bid.BidAmount).ToList();
        _tiebreakDiagnostics.Add($"{leagueYear.Key} | '{bid.GameName}' | ${bid.BidAmount} | Winner: {bid.PublisherName} @ {bid.EasternDateTime} | " +
                                 $"Tied with: {string.Join(", ", tiedBids.Select(x => $"{x.Bid.PublisherName} @ {x.Bid.EasternDateTime}"))}");

        //A configurable tiebreak system did not exist until 2022-03-03, which is after everything this migration
        //touches. Until then the processor always broke ties on lowest projected points, so the league year's current
        //TiebreakSystem says nothing about what actually happened here.
        return "This publisher has the lowest projected points. (Not including this game)";
    }

    private List<BidActionPair> GetBidActionPairs(List<PickupBidWithLeagueYearEntity> bids, List<LeagueActionWithLeagueYearEntity> actions)
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
                _unmatchedGameDiagnostics.Add($"'{bid.GameName}' | {bid.LeagueID} {bid.Year} | {bid.BidID} ${bid.BidAmount} ({bid.PublisherName}) | placed {bid.EasternDateTime} | successful={bid.Successful} | " +
                                              $"candidate actions for publisher: {string.Join(" ~~ ", matchingActionsForGame.Where(x => x.PublisherID == bid.PublisherID).Select(x => x.Description))}");
                continue;
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

        var theDayICombinedDropsAndBids = new LocalDate(2020, 12, 13);
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
    /// <summary>
    /// Whether this bid made it as far as the auction. Only bids that passed validation got that far, and for
    /// everything this migration touches the auction only ever rejected them with this one message, so this is an
    /// exact reconstruction of the valid bid set. ("Bid lost on tiebreakers." was not added until 2023.)
    /// </summary>
    public bool WasAValidBid()
    {
        if (Bid.Successful == true)
        {
            return true;
        }

        return Action.Description.EndsWith("Failure reason: Publisher was outbid.");
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

    public required string PublisherName { get; init; }
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

    public string PublisherName { get; set; } = null!;
    public string GameName { get; set; } = null!;
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
