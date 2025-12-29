using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Handlers;
using FantasyCritic.Lib.Interfaces;
using JetBrains.Annotations;

namespace FantasyCritic.Lib.Discord.Commands;

public class RemoveLeagueCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly RoleHandler _roleHandler;

    public RemoveLeagueCommand(IDiscordRepo discordRepo,
        IDiscordFormatter discordFormatter,
        RoleHandler roleHandler)
    {
        _discordRepo = discordRepo;
        _discordFormatter = discordFormatter;
        _roleHandler = roleHandler;
    }

    [UsedImplicitly]
    [SlashCommand("remove-league", "Removes the configuration for the league associated with the current channel.")]
    public async Task RemoveLeague()
    {
        await DeferAsync();

        var leagueChannel = await _discordRepo.GetMinimalLeagueChannel(Context.Guild.Id, Context.Channel.Id);
        if (!_roleHandler.CanAdministrate(Context.Guild, Context.User, leagueChannel))
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Cannot Manage The Bot",
                "You do not have permission to adjust bot configurations.",
                Context.User));
            return;
        }

        var wasDeleted = await _discordRepo.DeleteLeagueChannel(Context.Guild.Id, Context.Channel.Id);
        if (!wasDeleted)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Removing League Configuration",
                "Could not remove league configuration. It's possible that it wasn't set up.",
                Context.User));
            return;
        }

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
            "Removed League Configuration",
            "Channel configuration removed.",
            Context.User));
    }
}
