using FantasyCritic.Lib.Jobs.Utilities;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RefreshCriticScoresJobHandler : IFantasyCriticJobHandler
{
    private readonly CriticScoreRefresher _criticScoreRefresher;

    public RefreshCriticScoresJobHandler(CriticScoreRefresher criticScoreRefresher)
    {
        _criticScoreRefresher = criticScoreRefresher;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.RefreshCriticScores;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _criticScoreRefresher.RefreshCriticInfo();
        return Result.Success();
    }
}
