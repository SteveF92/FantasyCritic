using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.UrlBuilders;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Discord.Commands;
public class GetLeagueCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IClock _clock;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly string _baseAddress;

    public GetLeagueCommand(IDiscordRepo discordRepo,
        IFantasyCriticRepo fantasyCriticRepo,
        IClock clock,
        IDiscordFormatter discordFormatter,
        FantasyCriticSettings fantasyCriticSettings)
    {
        _discordRepo = discordRepo;
        _fantasyCriticRepo = fantasyCriticRepo;
        _clock = clock;
        _discordFormatter = discordFormatter;
        _baseAddress = fantasyCriticSettings.BaseAddress;
    }

    [SlashCommand("league", "Get league information.")]
    public async Task GetLeague(
        [Summary("year", "The year for the league (if not entered, defaults to the current year).")] int? year = null
        )
    {
        await DeferAsync();
        var dateToCheck = _clock.GetGameEffectiveDate(year);

        var systemWideValues = await _fantasyCriticRepo.GetSystemWideValues();
        var leagueChannel = await _discordRepo.GetLeagueChannel(Context.Guild.Id, Context.Channel.Id, dateToCheck.Year);
        if (leagueChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Finding League Configuration",
                "No league configuration found for this channel. You may have to specify a year if your league is for an upcoming year.",
                Context.User));
            return;
        }

        var previousYearWinner = await _fantasyCriticRepo.GetLeagueYearWinner(leagueChannel.LeagueYear.League.LeagueID, leagueChannel.LeagueYear.Year - 1);

        var rankedPublishers = leagueChannel.LeagueYear.Publishers.OrderByDescending(p
            => p.GetTotalFantasyPoints(leagueChannel.LeagueYear.SupportedYear, leagueChannel.LeagueYear.Options));

        var publisherLines =
            rankedPublishers
                .Select((publisher, index) =>
                {
                    var totalPoints = publisher
                        .GetTotalFantasyPoints(leagueChannel.LeagueYear.SupportedYear, leagueChannel.LeagueYear.Options);

                    var projectedPoints = publisher
                        .GetProjectedFantasyPoints(leagueChannel.LeagueYear,
                            systemWideValues, dateToCheck);

                    return BuildPublisherLine(index + 1, publisher, totalPoints, projectedPoints, dateToCheck, previousYearWinner);
                });

        var leagueUrl = new LeagueUrlBuilder(_baseAddress, leagueChannel.LeagueYear.League.LeagueID,
            leagueChannel.LeagueYear.Year)
            .BuildUrl();

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
            $"{leagueChannel.LeagueYear.League.LeagueName} {leagueChannel.LeagueYear.Year}",
            string.Join("\n", publisherLines),
            Context.User,
            url: leagueUrl));
    }

    private static string BuildPublisherLine(int rank, Publisher publisher, decimal totalPoints, decimal projectedPoints, LocalDate currentDate, FantasyCriticUser? previousYearWinner)
    {
        var totalGames = publisher.PublisherGames
            .Count(x => !x.CounterPick);
        var allWillRelease = publisher.PublisherGames
            .Where(x => !x.CounterPick)
            .Where(x => x.MasterGame is not null)
            .Count(x => x.WillRelease());

        var gamesReleased = publisher.PublisherGames
            .Where(x => !x.CounterPick)
            .Where(x => x.MasterGame is not null)
            .Count(x => x.MasterGame!.MasterGame.IsReleased(currentDate));

        var publisherLine = $"**{rank}.** ";
        if (!string.IsNullOrEmpty(publisher.PublisherIcon))
        {
            publisherLine += $"{publisher.PublisherIcon} ";
        }

        var crownEmoji = "";
        if (previousYearWinner is not null && publisher.User.Id == previousYearWinner.Id)
        {
            crownEmoji = " 👑";
        }
        
        publisherLine += $"**{publisher.GetPublisherAndUserDisplayName()}**{crownEmoji}\n";
        publisherLine += $"> **{Math.Round(totalPoints, 1)} points** ";
        publisherLine += $"*(Projected: {Math.Round(projectedPoints, 1)})*\n";
        publisherLine += $"> {gamesReleased}/{allWillRelease} games released";
        var willNotRelease = totalGames - allWillRelease;
        if (willNotRelease != 0)
        {
            publisherLine += $" (and {willNotRelease} that will not release)";
        }

        return publisherLine;
    }
}
