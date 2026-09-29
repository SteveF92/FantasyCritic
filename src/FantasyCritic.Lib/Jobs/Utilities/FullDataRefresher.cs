namespace FantasyCritic.Lib.Jobs.Utilities;

internal class FullDataRefresher
{
    private readonly CriticScoreRefresher _criticScoreRefresher;
    private readonly GGInfoRefresher _ggInfoRefresher;
    private readonly CacheRefresher _cacheRefresher;
    private readonly FantasyPointsUpdater _fantasyPointsUpdater;

    public FullDataRefresher(CriticScoreRefresher criticScoreRefresher, GGInfoRefresher ggInfoRefresher, CacheRefresher cacheRefresher,
        FantasyPointsUpdater fantasyPointsUpdater)
    {
        _criticScoreRefresher = criticScoreRefresher;
        _ggInfoRefresher = ggInfoRefresher;
        _cacheRefresher = cacheRefresher;
        _fantasyPointsUpdater = fantasyPointsUpdater;
    }

    //Each step appends its own clause to the job's status, so a run that stops partway says which steps finished.
    public async Task FullDataRefresh(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _criticScoreRefresher.RefreshCriticInfo(context, cancellationToken);
        await _ggInfoRefresher.RefreshGGInfo(false, context, cancellationToken);
        await _cacheRefresher.RefreshCaches(context, cancellationToken);
        await _fantasyPointsUpdater.UpdateFantasyPoints(context, cancellationToken);
    }
}
