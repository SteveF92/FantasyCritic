using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class GrantSuperDropsJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.GrantSuperDrops;
    public static FantasyCriticJobPriority Priority => FantasyCriticJobPriority.DependantAndDependedUpon;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.Hourly.WithCalendarGuard(instant => instant.ShouldGrantSuperDrops());

    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly DiscordPushService _discordPushService;
    private readonly IClock _clock;
    private readonly ILogger<GrantSuperDropsJobHandler> _logger;

    public GrantSuperDropsJobHandler(IFantasyCriticRepo fantasyCriticRepo, DiscordPushService discordPushService, IClock clock,
        ILogger<GrantSuperDropsJobHandler> logger)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _discordPushService = discordPushService;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        SystemWideValues systemWideValues = await _fantasyCriticRepo.GetSystemWideValues();
        var now = _clock.GetCurrentInstant();
        var supportedYears = await _fantasyCriticRepo.GetSupportedYears();
        var currentYear = supportedYears.Where(x => !x.Finished && x.OpenForPlay).MinBy(x => x.Year);
        IReadOnlyList<LeagueYear> allLeagueYears = await _fantasyCriticRepo.GetLeagueYears(currentYear!.Year);
        var leagueYearsWithSuperDrops = allLeagueYears.Where(x => x.IsFirstDraftFinished && x.Options.GrantSuperDrops).ToList();

        var allLeagueActions = await _fantasyCriticRepo.GetLeagueActions(currentYear.Year);
        var automatedGrantActions = allLeagueActions.Where(x => x.ActionType == "Granted Super Drop");
        var publishersAlreadyGranted = automatedGrantActions.Select(x => x.Publisher.PublisherID).ToHashSet();

        List<Publisher> publishersToGrantSuperDrop = [];
        List<LeagueAction> superDropActions = [];
        foreach (var leagueYear in leagueYearsWithSuperDrops)
        {
            if (!leagueYear.Publishers.Any())
            {
                continue;
            }

            var superDropPointCutoff = leagueYear.GetSuperDropPointCuttoff(systemWideValues);
            if (!superDropPointCutoff.HasValue)
            {
                continue;
            }

            var publishersWithProjectedPoints = leagueYear.Publishers.ToDictionary(x => x, y => y.GetProjectedFantasyPoints(leagueYear, systemWideValues));
            var publishersWithLowScores = publishersWithProjectedPoints.Where(x => x.Value < superDropPointCutoff).ToList();
            List<LeagueAction> actions = [];
            foreach (var publisher in publishersWithLowScores)
            {
                if (publishersAlreadyGranted.Contains(publisher.Key.PublisherID))
                {
                    continue;
                }

                actions.Add(new LeagueAction(publisher.Key, now, "Granted Super Drop", "Granted one super drop due to league standings.", false));
                publishersToGrantSuperDrop.Add(publisher.Key);
            }
            superDropActions.AddRange(actions);
        }

        //No check between the grant and its messages: the next run skips publishers already granted, so their messages would never go out.
        cancellationToken.ThrowIfCancellationRequested();
        await _fantasyCriticRepo.GrantSuperDrops(publishersToGrantSuperDrop, superDropActions);
        await _discordPushService.SendSuperDropMessages(publishersToGrantSuperDrop);

        if (publishersToGrantSuperDrop.Count == 0)
        {
            _logger.LogDebug("No super drops to grant for {Year}: {LeagueCount} leagues grant them.", currentYear.Year, leagueYearsWithSuperDrops.Count);
            await context.UpdateDetailedStatus($"No super drops to grant for {currentYear.Year}.");
        }
        else
        {
            var leagueCount = publishersToGrantSuperDrop.Select(x => x.LeagueYearKey).Distinct().Count();
            _logger.LogInformation("Granted super drops to {PublisherCount} publishers in {LeagueCount} leagues for {Year}.",
                publishersToGrantSuperDrop.Count, leagueCount, currentYear.Year);
            await context.UpdateDetailedStatus($"Granted super drops to {publishersToGrantSuperDrop.Count} publishers in {leagueCount} leagues for {currentYear.Year}.");
        }

        return Result.Success();
    }
}
