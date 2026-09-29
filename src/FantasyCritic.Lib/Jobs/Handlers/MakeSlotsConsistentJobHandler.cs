using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class MakeSlotsConsistentJobHandler : IFantasyCriticJobHandler
{
    private readonly IFantasyCriticRepo _fantasyCriticRepo;

    public MakeSlotsConsistentJobHandler(IFantasyCriticRepo fantasyCriticRepo)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.MakeSlotsConsistent;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var supportedYears = await _fantasyCriticRepo.GetSupportedYears();
        var currentYear = supportedYears.Where(x => !x.Finished && x.OpenForPlay).MaxBy(x => x.Year);
        await _fantasyCriticRepo.ManualMakePublisherGameSlotsConsistent(currentYear!.Year);

        return Result.Success();
    }
}
