using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Handlers;
using FantasyCritic.Lib.Interfaces;
using JetBrains.Annotations;

namespace FantasyCritic.Lib.Discord.Commands;

public class RemoveConferenceCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly RoleHandler _roleHandler;

    public RemoveConferenceCommand(IDiscordRepo discordRepo,
        IDiscordFormatter discordFormatter,
        RoleHandler roleHandler)
    {
        _discordRepo = discordRepo;
        _discordFormatter = discordFormatter;
        _roleHandler = roleHandler;
    }

    [UsedImplicitly]
    [SlashCommand("remove-conference", "Removes the configuration for the conference associated with the current channel.")]
    public async Task RemoveConference()
    {
        await DeferAsync();

        var wasDeleted = await _discordRepo.DeleteConferenceChannel(Context.Guild.Id, Context.Channel.Id);
        if (!wasDeleted)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Removing Conference Configuration",
                "Could not remove conference configuration. It's possible that it wasn't set up.",
                Context.User));
            return;
        }

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
            "Removed Conference Configuration",
            "Channel configuration removed.",
            Context.User));
    }
}
