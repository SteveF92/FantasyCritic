using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs.Utilities;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RecalculateRoyaleWinnersJobHandler : IFantasyCriticJobHandler
{
    private readonly IRoyaleRepo _royaleRepo;

    public RecalculateRoyaleWinnersJobHandler(IRoyaleRepo royaleRepo)
    {
        _royaleRepo = royaleRepo;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.RecalculateRoyaleWinners;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await RoyaleJobUtilities.RecalculateRoyaleWinners(_royaleRepo, cancellationToken);
        return Result.Success();
    }
}
