using FantasyCritic.Lib.Domain.Trades;
using FantasyCritic.Lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class ExpireTradesJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.ExpireTrades;
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
        _logger.LogInformation("Expiring trades.");
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

        return Result.Success();
    }
}
