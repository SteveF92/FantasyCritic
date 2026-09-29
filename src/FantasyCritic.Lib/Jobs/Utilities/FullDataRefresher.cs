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

    public async Task FullDataRefresh(CancellationToken cancellationToken)
    {
        await _criticScoreRefresher.RefreshCriticInfo(cancellationToken);
        await _ggInfoRefresher.RefreshGGInfo(false, cancellationToken);
        await _cacheRefresher.RefreshCaches(cancellationToken);
        await _fantasyPointsUpdater.UpdateFantasyPoints(cancellationToken);
    }
}
