using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Royale;
using FantasyCritic.Lib.Services;
using FantasyCritic.MySQL.Entities;

namespace FantasyCritic.MySQL;

public class MySQLDailyStatsRepo : IDailyStatsRepo
{
    private readonly string _connectionString;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IRoyaleRepo _royaleRepo;

    public MySQLDailyStatsRepo(RepositoryConfiguration configuration, IFantasyCriticRepo fantasyCriticRepo, IRoyaleRepo royaleRepo)
    {
        _connectionString = configuration.ConnectionString;
        _fantasyCriticRepo = fantasyCriticRepo;
        _royaleRepo = royaleRepo;
    }

    public async Task UpdateDailyStats(IEnumerable<SupportedYear> activeYears, IEnumerable<RoyaleYearQuarter> royaleQuarters,
        LocalDate currentDate, SystemWideValues systemWideValues)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        foreach (var activeYear in activeYears)
        {
            await UpdateDailyPublisherStatistics(activeYear.Year, currentDate, systemWideValues, connection, transaction);
            await UpdateDailyStatistics(activeYear.Year, currentDate, connection, transaction);
        }

        foreach (var supportedQuarter in royaleQuarters)
        {
            var inGracePeriod = supportedQuarter.YearQuarter.LastDateOfQuarter.PlusDays(RoyaleService.POST_QUARTER_GRACE_DAYS) >= currentDate;
            if ((supportedQuarter.OpenForPlay && !supportedQuarter.Finished) || inGracePeriod)
            {
                await UpdateDailyRoyalePublisherStatistics(supportedQuarter, currentDate, connection, transaction);
            }
        }

        await transaction.CommitAsync();
    }

    private async Task UpdateDailyPublisherStatistics(int year, LocalDate currentDate, SystemWideValues systemWideValues,
        MySqlConnection connection, MySqlTransaction transaction)
    {
        var leagueYears = await _fantasyCriticRepo.GetLeagueYears(year);

        var statistics = leagueYears
            .Where(x => x.PlayStatus.DraftFinished)
            .SelectMany(ly => ly.Publishers.Select(p => new { LeagueYear = ly, Publisher = p }))
            .Select(item => item.Publisher.GetPublisherStatistics(currentDate, item.LeagueYear, systemWideValues))
            .ToList();

        var statisticsEntities = statistics.Select(x => new PublisherStatisticsEntity(x)).ToList();

        await connection.BulkInsertAsync(statisticsEntities, "tbl_league_publisherstatistics", 500, transaction, insertIgnore: true);
    }

    private async Task UpdateDailyStatistics(int year, LocalDate currentDate, MySqlConnection connection, MySqlTransaction transaction)
    {
        const string sql =
        """
        INSERT INTO tbl_caching_mastergameyearstatistics
        (
            `Year`,
            `MasterGameID`,
            `Date`,
            `PercentStandardGame`,
            `PercentCounterPick`,
            `EligiblePercentStandardGame`,
            `AdjustedPercentCounterPick`,
            `NumberOfBids`,
            `TotalBidAmount`,
            `BidPercentile`,
            `AverageDraftPosition`,
            `AverageWinningBid`,
            `HypeFactor`,
            `DateAdjustedHypeFactor`,
            `PeakHypeFactor`,
            `LinearRegressionHypeFactor`
        )
        SELECT
            mg.`Year`,
            mg.`MasterGameID`,
            @currentDate AS `Date`,
            mg.`PercentStandardGame`,
            mg.`PercentCounterPick`,
            mg.`EligiblePercentStandardGame`,
            mg.`AdjustedPercentCounterPick`,
            mg.`NumberOfBids`,
            mg.`TotalBidAmount`,
            mg.`BidPercentile`,
            mg.`AverageDraftPosition`,
            mg.`AverageWinningBid`,
            mg.`HypeFactor`,
            mg.`DateAdjustedHypeFactor`,
            mg.`PeakHypeFactor`,
            mg.`LinearRegressionHypeFactor`
        FROM tbl_caching_mastergameyear mg
        WHERE mg.`Year` = @year
        ON DUPLICATE KEY UPDATE
            `PercentStandardGame` = VALUES(`PercentStandardGame`),
            `PercentCounterPick` = VALUES(`PercentCounterPick`),
            `EligiblePercentStandardGame` = VALUES(`EligiblePercentStandardGame`),
            `AdjustedPercentCounterPick` = VALUES(`AdjustedPercentCounterPick`),
            `NumberOfBids` = VALUES(`NumberOfBids`),
            `TotalBidAmount` = VALUES(`TotalBidAmount`),
            `BidPercentile` = VALUES(`BidPercentile`),
            `AverageDraftPosition` = VALUES(`AverageDraftPosition`),
            `AverageWinningBid` = VALUES(`AverageWinningBid`),
            `HypeFactor` = VALUES(`HypeFactor`),
            `DateAdjustedHypeFactor` = VALUES(`DateAdjustedHypeFactor`),
            `PeakHypeFactor` = VALUES(`PeakHypeFactor`),
            `LinearRegressionHypeFactor` = VALUES(`LinearRegressionHypeFactor`);
        """;

        var param = new
        {
            year,
            currentDate
        };

        await connection.ExecuteAsync(sql, param, transaction);
    }

    private async Task UpdateDailyRoyalePublisherStatistics(RoyaleYearQuarter supportedQuarter, LocalDate currentDate,
        MySqlConnection connection, MySqlTransaction transaction)
    {
        var publishers = await _royaleRepo.GetAllPublishers(supportedQuarter.YearQuarter.Year, supportedQuarter.YearQuarter.Quarter);

        var statisticsEntities = publishers
            .Select(x => new RoyalePublisherStatisticsEntity() { PublisherID = x.PublisherID, Date = currentDate, FantasyPoints = x.GetTotalFantasyPoints() })
            .ToList();

        await connection.BulkInsertAsync(statisticsEntities, "tbl_royale_publisherstatistics", 500, transaction, insertIgnore: true);
    }
}
