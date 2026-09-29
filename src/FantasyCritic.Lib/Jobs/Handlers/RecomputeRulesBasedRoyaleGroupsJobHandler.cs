using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Royale;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RecomputeRulesBasedRoyaleGroupsJobHandler : IFantasyCriticJobHandler
{
    private readonly IRoyaleRepo _royaleRepo;
    private readonly ILogger<RecomputeRulesBasedRoyaleGroupsJobHandler> _logger;

    public RecomputeRulesBasedRoyaleGroupsJobHandler(IRoyaleRepo royaleRepo, ILogger<RecomputeRulesBasedRoyaleGroupsJobHandler> logger)
    {
        _royaleRepo = royaleRepo;
        _logger = logger;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.RecomputeRulesBasedRoyaleGroups;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var rulesBasedGroups = await _royaleRepo.GetAllRoyaleGroupsByType(RoyaleGroupType.RulesBased);
        if (rulesBasedGroups.Count == 0)
        {
            _logger.LogDebug("No rules-based Royale groups to recompute.");
            await context.AppendDetailedStatus("No rules-based groups.");
            return Result.Success();
        }

        foreach (var group in rulesBasedGroups)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var memberIDs = await ComputeRulesBasedMembers(group);
            await _royaleRepo.SetRoyaleGroupMembers(group.GroupID, memberIDs);
            _logger.LogInformation("Recomputed rules-based Royale group {GroupName} with {Count} members.", group.GroupName, memberIDs.Count);
            await context.AppendDetailedStatus($"{group.GroupName}: {memberIDs.Count} members.");
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
        var quarters = await _royaleRepo.GetYearQuarters();
        return quarters
            .Where(q => q.WinningUser is not null)
            .Select(q => q.WinningUser!.UserID)
            .Distinct()
            .ToList();
    }
}
