using FantasyCritic.Lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class MakeSlotsConsistentJobHandler : IFantasyCriticJobHandler
{
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly ILogger<MakeSlotsConsistentJobHandler> _logger;

    public MakeSlotsConsistentJobHandler(IFantasyCriticRepo fantasyCriticRepo, ILogger<MakeSlotsConsistentJobHandler> logger)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _logger = logger;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.MakeSlotsConsistent;
    public static FantasyCriticJobPriority Priority => FantasyCriticJobPriority.Independent;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var supportedYears = await _fantasyCriticRepo.GetSupportedYears();
        var currentYear = supportedYears.Where(x => !x.Finished && x.OpenForPlay).MaxBy(x => x.Year);
        cancellationToken.ThrowIfCancellationRequested();
        await _fantasyCriticRepo.ManualMakePublisherGameSlotsConsistent(currentYear!.Year);

        _logger.LogInformation("Made publisher slots consistent for {Year}.", currentYear.Year);
        await context.UpdateDetailedStatus($"Made publisher slots consistent for {currentYear.Year}.");
        return Result.Success();
    }
}
