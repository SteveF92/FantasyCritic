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

    public async Task FullDataRefresh()
    {
        await _criticScoreRefresher.RefreshCriticInfo();
        await _ggInfoRefresher.RefreshGGInfo(false);
        await _cacheRefresher.RefreshCaches();
        await _fantasyPointsUpdater.UpdateFantasyPoints();
    }
}
