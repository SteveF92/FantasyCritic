using FantasyCritic.Lib.Jobs.Handlers;

namespace FantasyCritic.Lib.Jobs;

public class FantasyCriticJobRegistry
{
    public FantasyCriticJobRegistry(IEnumerable<FantasyCriticJobDefinition> definitions)
    {
        var definitionList = definitions.ToList();
        Validate(definitionList);

        Definitions = definitionList;
        Schedules = definitionList
            .Where(x => x.Schedule is not null)
            .ToDictionary(x => x.JobType, x => x.Schedule!);
    }

    public IReadOnlyList<FantasyCriticJobDefinition> Definitions { get; }
    public IReadOnlyDictionary<FantasyCriticJobType, FantasyCriticJobSchedule> Schedules { get; }

    //The one list of handlers. Adding a job type means adding a class and a line here; Validate fails startup if either is forgotten.
    public static FantasyCriticJobRegistry Create() => new(
    [
        FantasyCriticJobDefinition.ForCron<ExpireTradesJobHandler>(),
        FantasyCriticJobDefinition.ForCron<FullDataRefreshJobHandler>(),
        FantasyCriticJobDefinition.ForCron<GrantSuperDropsJobHandler>(),
        FantasyCriticJobDefinition.For<MakeSlotsConsistentJobHandler>(),
        FantasyCriticJobDefinition.For<PrepareForActionProcessingJobHandler>(),
        FantasyCriticJobDefinition.For<ProcessActionsJobHandler>(),
        FantasyCriticJobDefinition.ForCron<ProcessSpecialAuctionsJobHandler>(),
        FantasyCriticJobDefinition.ForCron<PushGameReleaseMessagesJobHandler>(),
        FantasyCriticJobDefinition.ForCron<PushPublicBiddingMessagesJobHandler>(),
        FantasyCriticJobDefinition.For<RecalculateLastSeasonWinnersJobHandler>(),
        FantasyCriticJobDefinition.For<RecalculateRoyaleWinnersJobHandler>(),
        FantasyCriticJobDefinition.For<RecomputeRulesBasedRoyaleGroupsJobHandler>(),
        FantasyCriticJobDefinition.For<RefreshCachesJobHandler>(),
        FantasyCriticJobDefinition.For<RefreshCriticScoresJobHandler>(),
        FantasyCriticJobDefinition.For<RefreshGGInfoJobHandler>(),
        FantasyCriticJobDefinition.ForCron<RefreshPatreonInfoJobHandler>(),
        FantasyCriticJobDefinition.ForCron<SendPublicBiddingEmailsJobHandler>(),
        FantasyCriticJobDefinition.ForCron<SendReleasingThisWeekUpdateJobHandler>(),
        FantasyCriticJobDefinition.ForCron<SetTimeFlagsJobHandler>(),
        FantasyCriticJobDefinition.For<SnapshotDatabaseJobHandler>(),
        FantasyCriticJobDefinition.ForCron<UpdateDailyPublisherStatisticsJobHandler>(),
        FantasyCriticJobDefinition.For<UpdateFantasyPointsJobHandler>(),
        FantasyCriticJobDefinition.For<UpdateTopBidsAndDropsJobHandler>(),
    ]);

    private static void Validate(IReadOnlyList<FantasyCriticJobDefinition> definitions)
    {
        var duplicates = definitions.GroupBy(x => x.JobType).Where(x => x.Count() > 1).Select(x => x.Key.Value).ToList();
        if (duplicates.Any())
        {
            throw new InvalidOperationException($"More than one handler is registered for job types: {string.Join(", ", duplicates)}.");
        }

        var missing = FantasyCriticJobType.GetAllPossibleValues().Except(definitions.Select(x => x.JobType)).Select(x => x.Value).ToList();
        if (missing.Any())
        {
            throw new InvalidOperationException($"No handler is registered for job types: {string.Join(", ", missing)}.");
        }

        //For<T> compiles for a cron handler too, and would silently drop its schedule.
        var cronRegisteredWithoutSchedule = definitions
            .Where(x => x.Schedule is null && typeof(IFantasyCriticCronJobHandler).IsAssignableFrom(x.HandlerType))
            .Select(x => x.HandlerType.Name)
            .ToList();
        if (cronRegisteredWithoutSchedule.Any())
        {
            throw new InvalidOperationException($"Cron handlers registered without their schedule (use ForCron): {string.Join(", ", cronRegisteredWithoutSchedule)}.");
        }
    }
}
