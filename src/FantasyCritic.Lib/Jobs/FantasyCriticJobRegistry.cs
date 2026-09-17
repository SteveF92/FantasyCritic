using FantasyCritic.Lib.Jobs.Handlers;

namespace FantasyCritic.Lib.Jobs;

public class FantasyCriticJobRegistry
{
    public FantasyCriticJobRegistry(IEnumerable<FantasyCriticJobDefinition> definitions,
        IReadOnlyDictionary<FantasyCriticJobType, IReadOnlyList<FantasyCriticJobType>> skipWhenDue)
    {
        var definitionList = definitions.ToList();
        Validate(definitionList);

        Definitions = definitionList;
        Schedules = definitionList
            .Where(x => x.Schedule is not null)
            .ToDictionary(x => x.JobType, x => x.Schedule!);

        ValidateSkipWhenDue(skipWhenDue, Schedules);
        SkipWhenDue = skipWhenDue.ToDictionary(x => x.Key, x => (IReadOnlyList<FantasyCriticJobType>)x.Value.ToList());
    }

    public IReadOnlyList<FantasyCriticJobDefinition> Definitions { get; }
    public IReadOnlyDictionary<FantasyCriticJobType, FantasyCriticJobSchedule> Schedules { get; }

    //A key's cron slot is skipped when any of its values is due in the same scheduler wake, because those jobs do the key's work themselves.
    public IReadOnlyDictionary<FantasyCriticJobType, IReadOnlyList<FantasyCriticJobType>> SkipWhenDue { get; }

    public IReadOnlyList<FantasyCriticJobType> GetDueJobsToDeferTo(FantasyCriticJobType jobType, IReadOnlySet<FantasyCriticJobType> dueJobTypes)
    {
        if (!SkipWhenDue.TryGetValue(jobType, out var deferTo))
        {
            return [];
        }

        return deferTo.Where(dueJobTypes.Contains).ToList();
    }

    public static FantasyCriticJobRegistry Create() => new FantasyCriticJobRegistry(CreateDefinitions(), CreateSkipWhenDue());

    private static Dictionary<FantasyCriticJobType, IReadOnlyList<FantasyCriticJobType>> CreateSkipWhenDue() => new()
    {
        //Prepare refreshes data itself, after action processing mode is on. The rollover refreshes critic info itself, before finishing the year.
        { FantasyCriticJobType.FullDataRefresh, [FantasyCriticJobType.PrepareForActionProcessing, FantasyCriticJobType.EndOfYearRollover] },
    };

    //The one list of handlers. Adding a job type means adding a class and a line here; Validate fails startup if either is forgotten.
    private static List<FantasyCriticJobDefinition> CreateDefinitions() => [
            FantasyCriticJobDefinition.ForCron<AdvanceRoyaleQuartersJobHandler>(),
            FantasyCriticJobDefinition.ForCron<EndOfYearRolloverJobHandler>(),
            FantasyCriticJobDefinition.ForCron<ExpireTradesJobHandler>(),
            FantasyCriticJobDefinition.ForCron<FullDataRefreshJobHandler>(),
            FantasyCriticJobDefinition.ForCron<GrantSuperDropsJobHandler>(),
            FantasyCriticJobDefinition.For<MakeSlotsConsistentJobHandler>(),
            FantasyCriticJobDefinition.ForCron<PrepareForActionProcessingJobHandler>(),
            FantasyCriticJobDefinition.For<ProcessActionsJobHandler>(),
            FantasyCriticJobDefinition.ForCron<ProcessSpecialAuctionsJobHandler>(),
            FantasyCriticJobDefinition.ForCron<PushGameReleaseMessagesJobHandler>(),
            FantasyCriticJobDefinition.For<RecalculateLastSeasonWinnersJobHandler>(),
            FantasyCriticJobDefinition.For<RecalculateRoyaleWinnersJobHandler>(),
            FantasyCriticJobDefinition.For<RecomputeRulesBasedRoyaleGroupsJobHandler>(),
            FantasyCriticJobDefinition.For<RefreshCachesJobHandler>(),
            FantasyCriticJobDefinition.For<RefreshCriticScoresJobHandler>(),
            FantasyCriticJobDefinition.For<RefreshGGInfoJobHandler>(),
            FantasyCriticJobDefinition.ForCron<RefreshPatreonInfoJobHandler>(),
            FantasyCriticJobDefinition.ForCron<SendAllPublicBiddingMessagesJobHandler>(),
            FantasyCriticJobDefinition.For<SendPublicBiddingDiscordMessagesJobHandler>(),
            FantasyCriticJobDefinition.For<SendPublicBiddingEmailsJobHandler>(),
            FantasyCriticJobDefinition.ForCron<SendReleasingThisWeekUpdateJobHandler>(),
            FantasyCriticJobDefinition.For<SnapshotDatabaseJobHandler>(),
            FantasyCriticJobDefinition.ForCron<UpdateDailyPublisherStatisticsJobHandler>(),
            FantasyCriticJobDefinition.For<UpdateFantasyPointsJobHandler>(),
            FantasyCriticJobDefinition.For<UpdateTopBidsAndDropsJobHandler>(),
        ];

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

    private static void ValidateSkipWhenDue(IReadOnlyDictionary<FantasyCriticJobType, IReadOnlyList<FantasyCriticJobType>> skipWhenDue,
        IReadOnlyDictionary<FantasyCriticJobType, FantasyCriticJobSchedule> schedules)
    {
        //A job without a schedule is never due in a scheduler wake, so an entry naming one would silently do nothing.
        var unscheduled = skipWhenDue.Keys.Concat(skipWhenDue.Values.SelectMany(x => x))
            .Where(x => !schedules.ContainsKey(x))
            .Select(x => x.Value)
            .Distinct()
            .ToList();
        if (unscheduled.Any())
        {
            throw new InvalidOperationException($"Skip-when-due entries name job types without a cron schedule: {string.Join(", ", unscheduled)}.");
        }

        var selfDeferring = skipWhenDue.Where(x => x.Value.Contains(x.Key)).Select(x => x.Key.Value).ToList();
        if (selfDeferring.Any())
        {
            throw new InvalidOperationException($"Job types cannot skip in favor of themselves: {string.Join(", ", selfDeferring)}.");
        }

        //One level deep: a job others defer to is never skipped itself, so there are no chains or cycles to reason about.
        var chained = skipWhenDue.Values.SelectMany(x => x).Where(skipWhenDue.ContainsKey).Select(x => x.Value).Distinct().ToList();
        if (chained.Any())
        {
            throw new InvalidOperationException($"Job types that others skip in favor of cannot have skip-when-due entries of their own: {string.Join(", ", chained)}.");
        }
    }
}
