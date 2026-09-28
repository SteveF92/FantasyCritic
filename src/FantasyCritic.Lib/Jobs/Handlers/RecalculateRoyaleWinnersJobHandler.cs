using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs.Utilities;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RecalculateRoyaleWinnersJobHandler : IFantasyCriticJobHandler
{
    private readonly IRoyaleRepo _royaleRepo;
    private readonly ILogger<RecalculateRoyaleWinnersJobHandler> _logger;

    public RecalculateRoyaleWinnersJobHandler(IRoyaleRepo royaleRepo, ILogger<RecalculateRoyaleWinnersJobHandler> logger)
    {
        _royaleRepo = royaleRepo;
        _logger = logger;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.RecalculateRoyaleWinners;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var calculatedQuarters = await RoyaleJobUtilities.CalculateMissingWinners(_royaleRepo, _logger, cancellationToken);
        await context.UpdateDetailedStatus(RoyaleJobUtilities.DescribeWinners(calculatedQuarters));
        return Result.Success();
    }
}
