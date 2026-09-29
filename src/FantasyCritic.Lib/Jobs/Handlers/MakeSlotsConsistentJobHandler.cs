using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class MakeSlotsConsistentJobHandler : IFantasyCriticJobHandler
{
    private readonly InterLeagueService _interLeagueService;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;

    public MakeSlotsConsistentJobHandler(InterLeagueService interLeagueService, IFantasyCriticRepo fantasyCriticRepo)
    {
        _interLeagueService = interLeagueService;
        _fantasyCriticRepo = fantasyCriticRepo;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.MakeSlotsConsistent;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var supportedYears = await _interLeagueService.GetSupportedYears();
        var currentYear = supportedYears.Where(x => !x.Finished && x.OpenForPlay).MaxBy(x => x.Year);
        await _fantasyCriticRepo.ManualMakePublisherGameSlotsConsistent(currentYear!.Year);

        return Result.Success();
    }
}
