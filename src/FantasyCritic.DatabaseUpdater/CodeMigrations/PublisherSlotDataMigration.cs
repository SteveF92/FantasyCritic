using System.Data;
using System.Diagnostics;
using Dapper;
using DbUp.Engine;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.MySQL;
using MySqlConnector;
using NodaTime;
using Serilog;

namespace FantasyCritic.DatabaseUpdater.CodeMigrations;

/// <summary>
/// Publisher slots as data, part 2 of 3 (docs/publisher-slot-plan.md, Step 1).
///
/// Fills tbl_league_slot and tbl_league_slotrequirestag from the old StandardGames / CounterPicks columns and
/// tbl_league_specialgameslot, then points every tbl_league_publishergame row at its slot. Runs between
/// 2026-10-09_000_publisherSlotSchema.sql, which creates the tables and the nullable column, and
/// 2026-10-09_002_publisherSlotCleanup.sql, which makes the column NOT NULL, indexes it and drops the old model.
///
/// Special slots have always been the last slots of the roster, so a special slot at position P in a league year
/// with S standard games and N special slots becomes standard slot number S - N + P.
///
/// This migration deliberately uses no repository, entity or domain type. Those change shape in the steps that
/// follow, and this code has to keep running on a fresh database forever. It talks to the old and new tables with
/// raw SQL and its own row classes, which also means it needs no prerequisites script.
///
/// It is safe to rerun at any point. The guards run first and write nothing. The slot rows go in as one
/// transaction, and a rerun that finds them already there checks their counts and moves on. The backfill commits
/// one year at a time (a million-row update in one transaction crawls on a small buffer pool), only touching games
/// that have no slot yet, so a rerun picks up where it stopped.
/// </summary>
public class PublisherSlotDataMigration : IScript
{
    private const int OffendersToLog = 50;

    private readonly string _connectionString;
    private readonly ILogger _logger;

    public PublisherSlotDataMigration(RepositoryConfiguration repositoryConfiguration, ILogger logger)
    {
        _connectionString = ConnectionStringUtilities.WithDefaultCommandTimeout(repositoryConfiguration.ConnectionString, Duration.FromMinutes(30));
        _logger = logger;
    }

    public string ProvideScript(Func<IDbCommand> dbCommandFactory)
    {
        // IScript.ProvideScript is synchronous (DbUp has no async IScript overload).
        return ProvideScriptAsync().GetAwaiter().GetResult();
    }

    private async Task<string> ProvideScriptAsync()
    {
        _logger.Information("Starting PublisherSlotDataMigration.");

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var leagueYears = (await connection.QueryAsync<LeagueYearRow>(
            "SELECT LeagueID, Year, StandardGames, CounterPicks FROM tbl_league_year;")).ToList();
        var specialSlotTags = (await connection.QueryAsync<SpecialSlotTagRow>(
            "SELECT DISTINCT LeagueID, Year, SpecialSlotPosition, Tag FROM tbl_league_specialgameslot WHERE Tag IS NOT NULL;")).ToList();
        var specialSlotTagsByLeagueYear = specialSlotTags.ToLookup(x => (x.LeagueID, x.Year));
        _logger.Information("Found {LeagueYearCount} league years, {LeagueYearsWithSpecialSlots} of them with special slots ({SpecialSlotTagCount} special slot tags).",
            leagueYears.Count, specialSlotTagsByLeagueYear.Count, specialSlotTags.Count);

        // Guards first, so a failure leaves nothing to clean up.
        CheckSpecialSlotsFitTheRoster(leagueYears, specialSlotTagsByLeagueYear);
        await CheckEveryGameIsInsideTheRoster(connection);

        await InsertSlotRows(connection, leagueYears, specialSlotTagsByLeagueYear, specialSlotTags.Count);
        await LinkGamesToSlots(connection, leagueYears);

        _logger.Information("PublisherSlotDataMigration completed successfully.");
        return string.Empty;
    }

    private async Task InsertSlotRows(MySqlConnection connection, IReadOnlyList<LeagueYearRow> leagueYears,
        ILookup<(Guid LeagueID, int Year), SpecialSlotTagRow> specialSlotTagsByLeagueYear, int expectedSlotTagCount)
    {
        var expectedSlotCount = leagueYears.Sum(x => x.StandardGames + x.CounterPicks);
        var existingSlotCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM tbl_league_slot;");
        var existingSlotTagCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM tbl_league_slotrequirestag;");
        if (existingSlotCount == expectedSlotCount && existingSlotTagCount == expectedSlotTagCount && expectedSlotCount > 0)
        {
            _logger.Information("Slot rows are already present ({SlotCount} slots, {SlotTagCount} slot tags); resuming at the backfill.", existingSlotCount, existingSlotTagCount);
            return;
        }

        if (existingSlotCount > 0 || existingSlotTagCount > 0)
        {
            throw new InvalidOperationException(
                $"tbl_league_slot has {existingSlotCount} rows and tbl_league_slotrequirestag has {existingSlotTagCount}, but {expectedSlotCount} and {expectedSlotTagCount} were expected. " +
                "Delete both tables' rows and rerun the migrator.");
        }

        var slots = new List<LeagueSlotRow>();
        var slotTags = new List<LeagueSlotTagRow>();
        foreach (var leagueYear in leagueYears)
        {
            var standardSlots = Enumerable.Range(0, leagueYear.StandardGames)
                .Select(slotNumber => new LeagueSlotRow(leagueYear, counterPick: false, slotNumber))
                .ToList();
            var counterPickSlots = Enumerable.Range(0, leagueYear.CounterPicks)
                .Select(slotNumber => new LeagueSlotRow(leagueYear, counterPick: true, slotNumber));
            slots.AddRange(standardSlots);
            slots.AddRange(counterPickSlots);

            var tagsForLeagueYear = specialSlotTagsByLeagueYear[(leagueYear.LeagueID, leagueYear.Year)].ToList();
            var specialSlotCount = tagsForLeagueYear.Select(x => x.SpecialSlotPosition).Distinct().Count();
            foreach (var specialSlotTag in tagsForLeagueYear)
            {
                var slotNumber = leagueYear.StandardGames - specialSlotCount + specialSlotTag.SpecialSlotPosition;
                slotTags.Add(new LeagueSlotTagRow(standardSlots[slotNumber].LeagueSlotID, specialSlotTag.Tag));
            }
        }

        var stopwatch = Stopwatch.StartNew();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.BulkInsertAsync(slots, "tbl_league_slot", 500, transaction);
        await connection.BulkInsertAsync(slotTags, "tbl_league_slotrequirestag", 500, transaction);
        await transaction.CommitAsync();
        _logger.Information("Inserted {SlotCount} slots ({StandardSlotCount} standard, {CounterPickSlotCount} counter pick) and {SlotTagCount} slot tags in {Seconds:F1}s.",
            slots.Count, slots.Count(x => !x.CounterPick), slots.Count(x => x.CounterPick), slotTags.Count, stopwatch.Elapsed.TotalSeconds);
    }

    private async Task LinkGamesToSlots(MySqlConnection connection, IReadOnlyList<LeagueYearRow> leagueYears)
    {
        // One UPDATE per year, each committed on its own: the same work as a single statement, but it stays well
        // inside the command timeout, the log shows progress, and a rerun only has the unlinked games left to do.
        const string backfillSql = """
            UPDATE tbl_league_publishergame pg
            JOIN tbl_league_publisher p ON p.PublisherID = pg.PublisherID
            JOIN tbl_league_slot s
                ON s.LeagueID = p.LeagueID
                AND s.Year = p.Year
                AND s.CounterPick = pg.CounterPick
                AND s.SlotNumber = pg.SlotNumber
            SET pg.LeagueSlotID = s.LeagueSlotID
            WHERE p.Year = @year AND pg.LeagueSlotID IS NULL;
            """;
        foreach (var year in leagueYears.Select(x => x.Year).Distinct().OrderBy(x => x))
        {
            var stopwatch = Stopwatch.StartNew();
            await using var transaction = await connection.BeginTransactionAsync();
            var gamesLinked = await connection.ExecuteAsync(backfillSql, new { year }, transaction);
            await transaction.CommitAsync();
            _logger.Information("Linked {GameCount} publisher games for {Year} in {Seconds:F1}s.", gamesLinked, year, stopwatch.Elapsed.TotalSeconds);
        }

        var totalGames = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM tbl_league_publishergame;");
        var gamesWithoutSlot = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM tbl_league_publishergame WHERE LeagueSlotID IS NULL;");
        if (gamesWithoutSlot > 0)
        {
            throw new InvalidOperationException($"{gamesWithoutSlot} of {totalGames} publisher games have no slot after the backfill. The roster check should have caught this. Rerunning the migrator retries only those games.");
        }

        _logger.Information("All {GameCount} publisher games are linked to their slots.", totalGames);
    }

    private void CheckSpecialSlotsFitTheRoster(IReadOnlyList<LeagueYearRow> leagueYears, ILookup<(Guid LeagueID, int Year), SpecialSlotTagRow> specialSlotTagsByLeagueYear)
    {
        var leagueYearsByKey = leagueYears.ToDictionary(x => (x.LeagueID, x.Year));
        var problems = new List<string>();
        foreach (var group in specialSlotTagsByLeagueYear)
        {
            var positions = group.Select(x => x.SpecialSlotPosition).Distinct().OrderBy(x => x).ToList();
            if (!leagueYearsByKey.TryGetValue(group.Key, out var leagueYear))
            {
                problems.Add($"League {group.Key.LeagueID} year {group.Key.Year} has special slots but no league year row.");
                continue;
            }

            var expectedPositions = Enumerable.Range(0, positions.Count);
            if (!positions.SequenceEqual(expectedPositions))
            {
                problems.Add($"League {leagueYear.LeagueID} year {leagueYear.Year} has special slot positions [{string.Join(", ", positions)}], expected 0..{positions.Count - 1}.");
            }

            if (positions.Count > leagueYear.StandardGames)
            {
                problems.Add($"League {leagueYear.LeagueID} year {leagueYear.Year} has {positions.Count} special slots but only {leagueYear.StandardGames} standard games.");
            }
        }

        if (problems.Count == 0)
        {
            return;
        }

        foreach (var problem in problems)
        {
            _logger.Error("Special slot does not fit the roster: {Problem}", problem);
        }

        throw new InvalidOperationException($"{problems.Count} league year(s) have special slots that do not fit inside the roster. Nothing was written; see the log for each one.");
    }

    private async Task CheckEveryGameIsInsideTheRoster(MySqlConnection connection)
    {
        const string sql = """
            SELECT p.LeagueID, p.Year, pg.PublisherID, pg.PublisherGameID, pg.GameName, pg.CounterPick, pg.SlotNumber,
                   IF(pg.CounterPick, ly.CounterPicks, ly.StandardGames) AS SlotCount
            FROM tbl_league_publishergame pg
            JOIN tbl_league_publisher p ON p.PublisherID = pg.PublisherID
            JOIN tbl_league_year ly ON ly.LeagueID = p.LeagueID AND ly.Year = p.Year
            WHERE pg.SlotNumber < 0 OR pg.SlotNumber >= IF(pg.CounterPick, ly.CounterPicks, ly.StandardGames)
            ORDER BY p.LeagueID, p.Year, pg.PublisherID, pg.CounterPick, pg.SlotNumber;
            """;
        var offenders = (await connection.QueryAsync<GameOutsideRosterRow>(sql)).ToList();
        if (offenders.Count == 0)
        {
            return;
        }

        foreach (var offender in offenders.Take(OffendersToLog))
        {
            _logger.Error("Game outside the roster: league {LeagueID} year {Year} publisher {PublisherID} game {PublisherGameID} '{GameName}' is {SlotKind} slot {SlotNumber} of {SlotCount}.",
                offender.LeagueID, offender.Year, offender.PublisherID, offender.PublisherGameID, offender.GameName,
                offender.CounterPick ? "counter pick" : "standard", offender.SlotNumber, offender.SlotCount);
        }

        if (offenders.Count > OffendersToLog)
        {
            _logger.Error("...and {MoreCount} more games outside the roster.", offenders.Count - OffendersToLog);
        }

        throw new InvalidOperationException(
            $"{offenders.Count} publisher game(s) sit outside the roster (SlotNumber past StandardGames or CounterPicks). " +
            "Nothing was written. Fix their SlotNumber or remove them, then rerun the migrator; see the log for each game.");
    }

    // Row classes mirror the tables as they are at this point in the migration history. They are intentionally not
    // the shared entity types, which change shape in later steps.

    private sealed class LeagueYearRow
    {
        public Guid LeagueID { get; set; }
        public int Year { get; set; }
        public int StandardGames { get; set; }
        public int CounterPicks { get; set; }
    }

    private sealed class SpecialSlotTagRow
    {
        public Guid LeagueID { get; set; }
        public int Year { get; set; }
        public int SpecialSlotPosition { get; set; }
        public string Tag { get; set; } = null!;
    }

    private sealed class GameOutsideRosterRow
    {
        public Guid LeagueID { get; set; }
        public int Year { get; set; }
        public Guid PublisherID { get; set; }
        public Guid PublisherGameID { get; set; }
        public string GameName { get; set; } = null!;
        public bool CounterPick { get; set; }
        public int SlotNumber { get; set; }
        public int SlotCount { get; set; }
    }

    private sealed class LeagueSlotRow
    {
        public LeagueSlotRow(LeagueYearRow leagueYear, bool counterPick, int slotNumber)
        {
            LeagueSlotID = Guid.NewGuid();
            LeagueID = leagueYear.LeagueID;
            Year = leagueYear.Year;
            CounterPick = counterPick;
            SlotNumber = slotNumber;
        }

        public Guid LeagueSlotID { get; }
        public Guid LeagueID { get; }
        public int Year { get; }
        public bool CounterPick { get; }
        public int SlotNumber { get; }
        public string? SlotName => null;
    }

    private sealed class LeagueSlotTagRow
    {
        public LeagueSlotTagRow(Guid leagueSlotID, string tag)
        {
            LeagueSlotID = leagueSlotID;
            Tag = tag;
        }

        public Guid LeagueSlotID { get; }
        public string Tag { get; }
    }
}
