using FantasyCritic.Lib.Jobs.Utilities;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RecalculateRoyaleWinnersJobHandler : IFantasyCriticJobHandler
{
    private readonly RoyaleService _royaleService;

    public RecalculateRoyaleWinnersJobHandler(RoyaleService royaleService)
    {
        _royaleService = royaleService;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.RecalculateRoyaleWinners;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await RoyaleJobUtilities.RecalculateRoyaleWinners(_royaleService);
        return Result.Success();
    }
}
