using FantasyCritic.Lib.Jobs.Utilities;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RefreshGGInfoJobHandler : IFantasyCriticJobHandler
{
    private readonly GGInfoRefresher _ggInfoRefresher;

    public RefreshGGInfoJobHandler(GGInfoRefresher ggInfoRefresher)
    {
        _ggInfoRefresher = ggInfoRefresher;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.RefreshGGInfo;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        //Deep, as the fact checker's button has always been. FullDataRefresh does the shallow refresh.
        await _ggInfoRefresher.RefreshGGInfo(deepRefresh: true);
        return Result.Success();
    }
}
