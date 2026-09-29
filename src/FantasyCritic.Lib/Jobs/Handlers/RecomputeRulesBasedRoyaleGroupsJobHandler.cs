using FantasyCritic.Lib.Royale;
using FantasyCritic.Lib.Services;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RecomputeRulesBasedRoyaleGroupsJobHandler : IFantasyCriticJobHandler
{
    private readonly RoyaleService _royaleService;
    private readonly ILogger<RecomputeRulesBasedRoyaleGroupsJobHandler> _logger;

    public RecomputeRulesBasedRoyaleGroupsJobHandler(RoyaleService royaleService, ILogger<RecomputeRulesBasedRoyaleGroupsJobHandler> logger)
    {
        _royaleService = royaleService;
        _logger = logger;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.RecomputeRulesBasedRoyaleGroups;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var rulesBasedGroups = await _royaleService.GetAllRoyaleGroupsByType(RoyaleGroupType.RulesBased);
        foreach (var group in rulesBasedGroups)
        {
            var memberIDs = await ComputeRulesBasedMembers(group);
            await _royaleService.SetRoyaleGroupMembers(group.GroupID, memberIDs);
            _logger.LogInformation("Recomputed rules-based Royale group {GroupName} with {Count} members.", group.GroupName, memberIDs.Count);
        }

        return Result.Success();
    }

    private async Task<IReadOnlyList<Guid>> ComputeRulesBasedMembers(RoyaleGroup group)
    {
        return group.RuleSetType switch
        {
            "PreviousWinners" => await ComputePreviousWinners(),
            _ => new List<Guid>()
        };
    }

    private async Task<IReadOnlyList<Guid>> ComputePreviousWinners()
    {
        var quarters = await _royaleService.GetYearQuarters();
        return quarters
            .Where(q => q.WinningUser is not null)
            .Select(q => q.WinningUser!.UserID)
            .Distinct()
            .ToList();
    }
}
