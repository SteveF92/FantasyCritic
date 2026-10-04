using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs.Utilities;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RecalculateRoyaleWinnersJobHandler : IFantasyCriticJobHandler
{
    private readonly IRoyaleRepo _royaleRepo;
    private readonly IClock _clock;
    private readonly ILogger<RecalculateRoyaleWinnersJobHandler> _logger;

    public RecalculateRoyaleWinnersJobHandler(IRoyaleRepo royaleRepo, IClock clock, ILogger<RecalculateRoyaleWinnersJobHandler> logger)
    {
        _royaleRepo = royaleRepo;
        _clock = clock;
        _logger = logger;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.RecalculateRoyaleWinners;
    public static FantasyCriticJobPriority Priority => FantasyCriticJobPriority.StrictlyDependant;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var calculatedQuarters = await RoyaleJobUtilities.CalculateMissingWinners(_royaleRepo, _clock.GetToday(), _logger, cancellationToken);
        await context.UpdateDetailedStatus(RoyaleJobUtilities.DescribeWinners(calculatedQuarters));
        return Result.Success();
    }
}
