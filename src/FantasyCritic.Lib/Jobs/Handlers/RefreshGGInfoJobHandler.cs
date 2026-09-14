using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RefreshGGInfoJobHandler : IFantasyCriticJobHandler
{
    private readonly AdminService _adminService;

    public RefreshGGInfoJobHandler(AdminService adminService)
    {
        _adminService = adminService;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.RefreshGGInfo;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        //Deep, as the fact checker's button has always been. FullDataRefresh does the shallow refresh.
        await _adminService.RefreshGGInfo(deepRefresh: true);
        return Result.Success();
    }
}
