using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Domain.Calculations;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using Serilog;

namespace FantasyCritic.Lib.Jobs.Utilities;

internal class FantasyPointsUpdater
{
    private static readonly ILogger _logger = Log.ForContext<FantasyPointsUpdater>();

    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly FantasyCriticService _fantasyCriticService;
    private readonly RoyaleService _royaleService;
    private readonly IDiscordRepo _discordRepo;
    private readonly DiscordPushService _discordPushService;
    private readonly IClock _clock;

    public FantasyPointsUpdater(IFantasyCriticRepo fantasyCriticRepo, FantasyCriticService fantasyCriticService,
        RoyaleService royaleService, IDiscordRepo discordRepo, DiscordPushService discordPushService, IClock clock)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _fantasyCriticService = fantasyCriticService;
        _royaleService = royaleService;
        _discordRepo = discordRepo;
        _discordPushService = discordPushService;
        _clock = clock;
    }

    public async Task UpdateFantasyPoints(CancellationToken cancellationToken)
    {
        _logger.Information("Updating fantasy points");

        var supportedYears = await _fantasyCriticRepo.GetSupportedYears();
        var activeYears = supportedYears.Where(x => x.OpenForPlay && !x.Finished);
        foreach (var activeYear in activeYears)
        {
            IReadOnlyList<LeagueYear> leagueYears = await _fantasyCriticRepo.GetLeagueYears(activeYear.Year);
            var calculatedStats = _fantasyCriticService.GetCalculatedStatsForYear(activeYear.Year, leagueYears, false);
            //No check between the write and the Discord messages: the messages are a diff against the old stats, so a stop there would lose them.
            cancellationToken.ThrowIfCancellationRequested();
            await _fantasyCriticRepo.UpdatePublisherGameCalculatedStats(calculatedStats.PublisherGameCalculatedStats);
            await PushDiscordScoreChangeMessages(leagueYears, calculatedStats.PublisherGameCalculatedStats);
        }

        var today = _clock.GetToday();
        var finishedYears = supportedYears.Where(x => x.Finished);
        foreach (var finishedYear in finishedYears)
        {
            var lastDayOfFinishedYear = new LocalDate(finishedYear.Year, 12, 31);
            if (today.PlusDays(30) > lastDayOfFinishedYear)
            {
                //We don't need to keep updating old years after a certain point.
                continue;
            }

            IReadOnlyList<LeagueYear> leagueYears = await _fantasyCriticRepo.GetLeagueYears(finishedYear.Year);
            var calculatedStats = _fantasyCriticService.GetCalculatedStatsForYear(finishedYear.Year, leagueYears, false);
            cancellationToken.ThrowIfCancellationRequested();
            await _fantasyCriticRepo.UpdateLeagueWinners(calculatedStats.WinningUsers, false);
        }

        _logger.Information("Done updating fantasy points");
        _logger.Information("Updating royale fantasy points");

        var supportedQuarters = await _royaleService.GetYearQuarters();
        foreach (var supportedQuarter in supportedQuarters)
        {
            if (!supportedQuarter.OpenForPlay)
            {
                continue;
            }

            if (supportedQuarter.Finished)
            {
                if (SupportedYear.Year2026FeatureSupported(supportedQuarter.YearQuarter.Year))
                {
                    var gracePeriodDate = supportedQuarter.YearQuarter.LastDateOfQuarter.Plus(Period.FromDays(RoyaleService.POST_QUARTER_GRACE_DAYS));
                    if (today > gracePeriodDate)
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            await _royaleService.UpdateFantasyPoints(supportedQuarter.YearQuarter);
        }

        _logger.Information("Done updating royale fantasy points");
    }

    private async Task PushDiscordScoreChangeMessages(IReadOnlyList<LeagueYear> oldLeagueYears, IReadOnlyDictionary<Guid, PublisherGameCalculatedStats> calculatedStats)
    {
        var leagueChannels = await _discordRepo.GetAllLeagueChannels();
        var channelLookup = leagueChannels.ToLookup(x => x.LeagueID);

        foreach (var oldLeagueYear in oldLeagueYears)
        {
            var channels = channelLookup[oldLeagueYear.Key.LeagueID].ToList();
            if (!channels.Any())
            {
                continue;
            }
            var newLeagueYear = oldLeagueYear.GetUpdatedLeagueYearWithNewScores(calculatedStats);
            var scoreChanges = new LeagueYearScoreChanges(oldLeagueYear, newLeagueYear);
            IReadOnlyList<MinimalConferenceChannel> conferenceChannels = new List<MinimalConferenceChannel>();
            if (newLeagueYear.League.ConferenceID != null)
            {
                conferenceChannels = await _discordRepo.GetConferenceChannels(newLeagueYear.League.ConferenceID.Value);
            }
            await _discordPushService.SendLeagueYearScoreUpdateMessage(scoreChanges, channels, conferenceChannels);
        }
    }
}
