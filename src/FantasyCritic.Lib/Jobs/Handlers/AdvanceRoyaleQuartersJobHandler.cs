using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs.Utilities;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class AdvanceRoyaleQuartersJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.AdvanceRoyaleQuarters;
    public static FantasyCriticJobPriority Priority => FantasyCriticJobPriority.TimeCritical;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.AtOnePastMidnightEastern;

    private readonly IRoyaleRepo _royaleRepo;
    private readonly IClock _clock;
    private readonly ILogger<AdvanceRoyaleQuartersJobHandler> _logger;

    public AdvanceRoyaleQuartersJobHandler(IRoyaleRepo royaleRepo, IClock clock, ILogger<AdvanceRoyaleQuartersJobHandler> logger)
    {
        _royaleRepo = royaleRepo;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var easternDate = _clock.GetCurrentInstant().InZone(TimeExtensions.EasternTimeZone).Date;

        //Finish any quarters whose end date has passed.
        var supportedQuarters = await _royaleRepo.GetYearQuarters();
        List<string> finishedQuarters = [];
        foreach (var supportedQuarter in supportedQuarters)
        {
            if (supportedQuarter.Finished)
            {
                continue;
            }

            var endDate = supportedQuarter.YearQuarter.LastDateOfQuarter;
            if (easternDate > endDate)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _logger.LogInformation("Finishing Royale quarter {YearQuarter}: it ended on {EndDate} and the Eastern date is {EasternDate}.",
                    supportedQuarter.YearQuarter, endDate.ToISOString(), easternDate.ToISOString());
                await _royaleRepo.FinishQuarter(supportedQuarter);
                finishedQuarters.Add(supportedQuarter.YearQuarter.ToString());
            }
        }

        if (finishedQuarters.Count == 0)
        {
            _logger.LogDebug("No Royale quarters to finish.");
            await context.AppendDetailedStatus("No quarters to finish.");
        }
        else
        {
            await context.AppendDetailedStatus($"Finished {string.Join(", ", finishedQuarters)}.");
        }

        //Calculate winners for any quarters past their grace period that don't have one yet.
        var calculatedQuarters = await RoyaleJobUtilities.CalculateMissingWinners(_royaleRepo, easternDate, _logger, cancellationToken);
        await context.AppendDetailedStatus(RoyaleJobUtilities.DescribeWinners(calculatedQuarters));

        //Start the next quarter as we approach it.
        supportedQuarters = await _royaleRepo.GetYearQuarters();
        var latestQuarter = supportedQuarters.WhereMax(x => x.YearQuarter).Single();
        var nextQuarter = latestQuarter.YearQuarter.NextQuarter;
        var dayToStartNextQuarter = nextQuarter.FirstDateOfQuarter.Minus(Period.FromDays(15));
        if (easternDate > dayToStartNextQuarter)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation("Starting Royale quarter {YearQuarter}: it opens after {OpenAfterDate} and the Eastern date is {EasternDate}.",
                nextQuarter, dayToStartNextQuarter.ToISOString(), easternDate.ToISOString());
            await _royaleRepo.StartNewQuarter(nextQuarter);
            await context.AppendDetailedStatus($"Started {nextQuarter}.");
        }
        else
        {
            _logger.LogDebug("Not starting Royale quarter {YearQuarter} yet: it opens after {OpenAfterDate}.", nextQuarter, dayToStartNextQuarter.ToISOString());
            await context.AppendDetailedStatus($"{nextQuarter} opens after {dayToStartNextQuarter.ToISOString()}.");
        }

        return Result.Success();
    }
}
