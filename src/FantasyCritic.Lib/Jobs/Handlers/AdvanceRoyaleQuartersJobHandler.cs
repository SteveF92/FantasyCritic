using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs.Utilities;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class AdvanceRoyaleQuartersJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.AdvanceRoyaleQuarters;
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
        var nycNow = _clock.GetCurrentInstant().InZone(TimeExtensions.EasternTimeZone);

        //Finish any quarters whose end date has passed.
        var supportedQuarters = await _royaleRepo.GetYearQuarters();
        foreach (var supportedQuarter in supportedQuarters)
        {
            if (supportedQuarter.Finished)
            {
                continue;
            }

            var endDate = supportedQuarter.YearQuarter.LastDateOfQuarter;
            if (nycNow.Date > endDate)
            {
                _logger.LogInformation($"Automatically setting {supportedQuarter} as finished because date/time is: {nycNow}");
                await _royaleRepo.FinishQuarter(supportedQuarter);
            }
        }

        //Calculate winners for any finished quarters that don't have one yet. This reloads the quarters, so it sees the ones just finished above.
        await RoyaleJobUtilities.RecalculateRoyaleWinners(_royaleRepo);

        //Start the next quarter as we approach it.
        supportedQuarters = await _royaleRepo.GetYearQuarters();
        var latestQuarter = supportedQuarters.WhereMax(x => x.YearQuarter).Single();
        var nextQuarter = latestQuarter.YearQuarter.NextQuarter;
        var dayToStartNextQuarter = nextQuarter.FirstDateOfQuarter.Minus(Period.FromDays(15));
        if (nycNow.Date > dayToStartNextQuarter)
        {
            await _royaleRepo.StartNewQuarter(nextQuarter);
        }

        return Result.Success();
    }
}
