using FantasyCritic.Lib.Domain.Trades;
using FantasyCritic.Lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class ExpireTradesJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.ExpireTrades;
    public static FantasyCriticJobPriority Priority => FantasyCriticJobPriority.Independent;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.Hourly;

    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IClock _clock;
    private readonly ILogger<ExpireTradesJobHandler> _logger;

    public ExpireTradesJobHandler(IFantasyCriticRepo fantasyCriticRepo, IClock clock, ILogger<ExpireTradesJobHandler> logger)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var now = _clock.GetCurrentInstant();
        var supportedYears = await _fantasyCriticRepo.GetSupportedYears();
        var currentYear = supportedYears.Where(x => !x.Finished && x.OpenForPlay).MaxBy(x => x.Year);
        IReadOnlyList<Trade> trades = await _fantasyCriticRepo.GetTradesForYear(currentYear!.Year);

        var activeTrades = trades.Where(x => x.Status.IsActive).ToList();
        List<Trade> tradesToExpire = [];
        foreach (var trade in activeTrades)
        {
            var tradeExpirationTime = trade.GetExpirationTime();
            if (!tradeExpirationTime.HasValue)
            {
                continue;
            }

            if (now > tradeExpirationTime)
            {
                tradesToExpire.Add(trade);
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        await _fantasyCriticRepo.ExpireTrades(tradesToExpire, now);

        if (tradesToExpire.Count == 0)
        {
            _logger.LogDebug("No trades to expire in {Year}: {ActiveTradeCount} active.", currentYear.Year, activeTrades.Count);
            await context.UpdateDetailedStatus($"No trades to expire in {currentYear.Year}.");
        }
        else
        {
            _logger.LogInformation("Expired {ExpiredTradeCount} of {ActiveTradeCount} active trades in {Year}.", tradesToExpire.Count, activeTrades.Count, currentYear.Year);
            await context.UpdateDetailedStatus($"Expired {tradesToExpire.Count} trades in {currentYear.Year}.");
        }

        return Result.Success();
    }
}
