using Discord;
using Discord.WebSocket;
using FantasyCritic.Discord.Interfaces;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using NodaTime;

namespace FantasyCritic.Discord.Commands;
public class RemoveLeagueCommand : ICommand
{
    public string Name => "remove-league";
    public string Description => "Removes the configuration for the league associated with the current channel.";
    public SlashCommandOptionBuilder[] Options => new SlashCommandOptionBuilder[] { };
    
    private readonly IDiscordRepo _discordRepo;
    private readonly IClock _clock;
    private readonly IDiscordFormatter _discordFormatter;

    public RemoveLeagueCommand(IDiscordRepo discordRepo,
        IClock clock,
        IDiscordFormatter discordFormatter)
    {
        _discordRepo = discordRepo;
        _clock = clock;
        _discordFormatter = discordFormatter;
    }

    public async Task HandleCommand(SocketSlashCommand command)
    {
        var dateToCheck = _clock.GetToday();

        var leagueChannel = await _discordRepo.GetLeagueChannel(command.Channel.Id.ToString(), dateToCheck.Year);
        if (leagueChannel == null)
        {
            await command.RespondAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Removing League Configuration",
                "No league configuration found for this channel.",
                command.User));
            return;
        }

        await _discordRepo.DeleteLeagueChannel(command.Channel.Id.ToString());

        await command.RespondAsync(embed: _discordFormatter.BuildRegularEmbed(
            "Removed League Configuration",
            "Channel configuration removed.",
            command.User));
    }
}
