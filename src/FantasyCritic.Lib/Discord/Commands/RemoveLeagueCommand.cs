using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Discord.Commands;
public class RemoveLeagueCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly IDiscordFormatter _discordFormatter;

    public RemoveLeagueCommand(IDiscordRepo discordRepo,
        IDiscordFormatter discordFormatter)
    {
        _discordRepo = discordRepo;
        _discordFormatter = discordFormatter;
    }

    [SlashCommand("remove-league", "Removes the configuration for the league associated with the current channel.")]
    public async Task RemoveLeague()
    {
        var wasDeleted = await _discordRepo.DeleteLeagueChannel(Context.Guild.Id, Context.Channel.Id);
        if (!wasDeleted)
        {
            await RespondAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Removing League Configuration",
                "Could not remove league configuration. It's possible that it wasn't set up.",
                Context.User));
            return;
        }

        await RespondAsync(embed: _discordFormatter.BuildRegularEmbed(
            "Removed League Configuration",
            "Channel configuration removed.",
            Context.User));
    }
}
