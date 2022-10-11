using Discord;
using Discord.WebSocket;
using FantasyCritic.Discord.Interfaces;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using NodaTime;

namespace FantasyCritic.Discord.Commands;
public class SetLeagueChannelCommand : ICommand
{
    private const string LeagueIdParameterName = "league_id";
    public string Name => "set-league";
    public string Description => "Sets the league to be associated with the current channel.";
    public SlashCommandOptionBuilder[] Options => new SlashCommandOptionBuilder[]
    {
        new()
        {
            Name = LeagueIdParameterName,
            Description = "The id for your league from the URL - https://www.fantasycritic.games/league/LEAGUE_ID_HERE/2022.",
            Type = ApplicationCommandOptionType.String,

            IsRequired = true
        }
    };

    private readonly IDiscordRepo _discordRepo;
    private readonly IClock _clock;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;

    public SetLeagueChannelCommand(IDiscordRepo discordRepo,
        IClock clock,
        IDiscordFormatter discordFormatter,
        IFantasyCriticRepo fantasyCriticRepo)
    {
        _discordRepo = discordRepo;
        _clock = clock;
        _discordFormatter = discordFormatter;
        _fantasyCriticRepo = fantasyCriticRepo;
    }

    public async Task HandleCommand(SocketSlashCommand command)
    {
        var dateToCheck = _clock.GetToday();

        var leagueId = command.Data.Options
            .First(o => o.Name == LeagueIdParameterName)
            .Value
            .ToString()!
            .ToLower()
            .Trim();

        if (string.IsNullOrEmpty(leagueId))
        {
            await command.RespondAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Setting League",
                "Error: A league ID is required.",
                command.User));
            return;
        }

        if (!Guid.TryParse(leagueId, out var leagueGuid))
        {
            await command.RespondAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Setting League",
                $"`{leagueId}` is not a valid league ID.",
                command.User));
            return;
        }

        var league = await _fantasyCriticRepo.GetLeague(leagueGuid);

        if (league == null)
        {
            await command.RespondAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Setting League",
                $"No league was found for the league ID {leagueId}.",
                command.User));
            return;
        }

        await _discordRepo.SetLeagueChannel(new Guid(leagueId), command.Channel.Id.ToString(), dateToCheck.Year);

        await command.RespondAsync(embed: _discordFormatter.BuildRegularEmbed(
            "Setting Channel League",
            $"Channel Configured for {league.LeagueName}",
            command.User));
    }
}
