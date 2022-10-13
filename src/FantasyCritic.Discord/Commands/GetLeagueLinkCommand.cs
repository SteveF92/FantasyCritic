using Discord.Interactions;
using FantasyCritic.Discord.Interfaces;
using FantasyCritic.Discord.Models;
using FantasyCritic.Discord.UrlBuilders;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using NodaTime;

namespace FantasyCritic.Discord.Commands;
public class GetLeagueLinkCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly IClock _clock;
    private readonly IDiscordParameterParser _parameterParser;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly string _baseAddress;

    public GetLeagueLinkCommand(IDiscordRepo discordRepo,
        IClock clock,
        IDiscordParameterParser parameterParser,
        IDiscordFormatter discordFormatter,
        FantasyCriticSettings fantasyCriticSettings)
    {
        _discordRepo = discordRepo;
        _clock = clock;
        _parameterParser = parameterParser;
        _discordFormatter = discordFormatter;
        _baseAddress = fantasyCriticSettings.BaseAddress;
    }

    [SlashCommand("link", "Get a link to the league.")]
    public async Task GetLeagueLink(
        [Summary("year", "The year for the league (if not entered, defaults to the current year).")] int? year = null
        )
    {
        var dateToCheck = _parameterParser.GetDateFromProvidedYear(year) ?? _clock.GetToday();

        var leagueChannel = await _discordRepo.GetLeagueChannel(Context.Channel.Id.ToString(), dateToCheck.Year);
        if (leagueChannel == null)
        {
            await RespondAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Getting League Link",
                "No league configuration found for this channel.",
                Context.User));
            return;
        }

        var leagueUrlBuilder = new LeagueUrlBuilder(_baseAddress, leagueChannel.LeagueYear.League.LeagueID, leagueChannel.LeagueYear.Year);
        var leagueUrl = leagueUrlBuilder.BuildUrl();

        await RespondAsync(embed: _discordFormatter.BuildRegularEmbed(
            $"Click here to visit the site for the league {leagueChannel.LeagueYear.League.LeagueName} ({leagueChannel.LeagueYear.Year})",
            "",
            Context.User,
            url: leagueUrl));
    }
}
