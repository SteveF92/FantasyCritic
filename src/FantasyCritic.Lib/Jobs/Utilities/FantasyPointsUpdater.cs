using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Domain.Calculations;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Utilities;

internal class FantasyPointsUpdater
{
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly FantasyCriticService _fantasyCriticService;
    private readonly RoyaleService _royaleService;
    private readonly IDiscordRepo _discordRepo;
    private readonly DiscordPushService _discordPushService;
    private readonly IClock _clock;
    private readonly ILogger<FantasyPointsUpdater> _logger;

    public FantasyPointsUpdater(IFantasyCriticRepo fantasyCriticRepo, FantasyCriticService fantasyCriticService,
        RoyaleService royaleService, IDiscordRepo discordRepo, DiscordPushService discordPushService, IClock clock, ILogger<FantasyPointsUpdater> logger)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _fantasyCriticService = fantasyCriticService;
        _royaleService = royaleService;
        _discordRepo = discordRepo;
        _discordPushService = discordPushService;
        _clock = clock;
        _logger = logger;
    }

    public async Task UpdateFantasyPoints(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating fantasy points.");

        var supportedYears = await _fantasyCriticRepo.GetSupportedYears();
        var activeYears = supportedYears.Where(x => x.OpenForPlay && !x.Finished);
        List<int> updatedYears = [];
        foreach (var activeYear in activeYears)
        {
            await context.AddTemporaryStatus($"Fantasy points: updating {activeYear.Year}.");
            IReadOnlyList<LeagueYear> leagueYears = await _fantasyCriticRepo.GetLeagueYears(activeYear.Year);
            var calculatedStats = _fantasyCriticService.GetCalculatedStatsForYear(activeYear.Year, leagueYears, false);
            //No check between the write and the Discord messages: the messages are a diff against the old stats, so a stop there would lose them.
            cancellationToken.ThrowIfCancellationRequested();
            await _fantasyCriticRepo.UpdatePublisherGameCalculatedStats(calculatedStats.PublisherGameCalculatedStats);
            await PushDiscordScoreChangeMessages(leagueYears, calculatedStats.PublisherGameCalculatedStats);
            updatedYears.Add(activeYear.Year);
        }

        var today = _clock.GetToday();
        var finishedYears = supportedYears.Where(x => x.Finished);
        List<int> winnerYears = [];
        foreach (var finishedYear in finishedYears)
        {
            var lastDayOfFinishedYear = new LocalDate(finishedYear.Year, 12, 31);
            var dayToStopCheckingThisYear = lastDayOfFinishedYear.PlusDays(30);
            if (today > dayToStopCheckingThisYear)
            {
                //We don't need to keep updating old years after a certain point.
                continue;
            }

            await context.AddTemporaryStatus($"Fantasy points: updating {finishedYear.Year} winners.");
            IReadOnlyList<LeagueYear> leagueYears = await _fantasyCriticRepo.GetLeagueYears(finishedYear.Year);
            var calculatedStats = _fantasyCriticService.GetCalculatedStatsForYear(finishedYear.Year, leagueYears, false);
            cancellationToken.ThrowIfCancellationRequested();
            await _fantasyCriticRepo.UpdateLeagueWinners(calculatedStats.WinningUsers, false);
            winnerYears.Add(finishedYear.Year);
        }

        var supportedQuarters = await _royaleService.GetYearQuarters();
        List<string> updatedQuarters = [];
        foreach (var supportedQuarter in supportedQuarters)
        {
            if (!supportedQuarter.OpenForPlay)
            {
                continue;
            }

            if (supportedQuarter.Finished && today > supportedQuarter.GracePeriodEndDate)
            {
                continue;
            }

            await context.AddTemporaryStatus($"Fantasy points: updating Royale {supportedQuarter.YearQuarter}.");
            cancellationToken.ThrowIfCancellationRequested();
            await _royaleService.UpdateFantasyPoints(supportedQuarter.YearQuarter);
            updatedQuarters.Add(supportedQuarter.YearQuarter.ToString());
        }

        _logger.LogInformation("Updated fantasy points for {UpdatedYears}, league winners for {WinnerYears}, and Royale for {UpdatedQuarters}.",
            updatedYears, winnerYears, updatedQuarters);
        await context.AppendDetailedStatus(DescribeUpdate(updatedYears, winnerYears, updatedQuarters));
    }

    private static string DescribeUpdate(IReadOnlyList<int> updatedYears, IReadOnlyList<int> winnerYears, IReadOnlyList<string> updatedQuarters)
    {
        List<string> parts = [];
        if (updatedYears.Count > 0)
        {
            parts.Add($"updated {string.Join(", ", updatedYears)}");
        }

        if (winnerYears.Count > 0)
        {
            parts.Add($"winners updated for {string.Join(", ", winnerYears)}");
        }

        if (updatedQuarters.Count > 0)
        {
            parts.Add($"Royale updated for {string.Join(", ", updatedQuarters)}");
        }

        return parts.Count == 0 ? "Fantasy points: nothing to update." : $"Fantasy points: {string.Join("; ", parts)}.";
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
