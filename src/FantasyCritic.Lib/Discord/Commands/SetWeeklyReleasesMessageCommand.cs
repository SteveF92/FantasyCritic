using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Handlers;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using JetBrains.Annotations;

namespace FantasyCritic.Lib.Discord.Commands;

public class SetWeeklyReleasesMessageCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly RoleHandler _roleHandler;

    public SetWeeklyReleasesMessageCommand(IDiscordRepo discordRepo,
        IDiscordFormatter discordFormatter,
        RoleHandler roleHandler)
    {
        _discordRepo = discordRepo;
        _discordFormatter = discordFormatter;
        _roleHandler = roleHandler;
    }

    [UsedImplicitly]
    [SlashCommand("set-weekly-releases-message", "Turn on or off a weekly message for games releasing this week.")]
    public async Task SetWeeklyReleasesMessage(
        [Summary("set-enabled", "Set the weekly releases message on or off")] bool setEnabled
        )
    {
        await DeferAsync();
        var leagueChannel = await _discordRepo.GetMinimalLeagueChannel(Context.Guild.Id, Context.Channel.Id);
        if (leagueChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Finding League Configuration",
                "No league configuration found for this channel.",
                Context.User));
            return;
        }

        if (!_roleHandler.CanAdministrate(Context.Guild, Context.User, leagueChannel))
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Cannot Manage The Bot",
                "You do not have permission to adjust bot configurations.",
                Context.User));
            return;
        }

        await _discordRepo.SetSendWeeklyReleasesMessage(leagueChannel.LeagueID, Context.Guild.Id, Context.Channel.Id, setEnabled);

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
            "Weekly Releases Message Setting Updated",
            $"The Weekly Releases Message is set to **{(setEnabled ? "ON" : "OFF")}**."
            + (setEnabled
                ? $"\nIt will send to this channel on {TimeExtensions.ReleasingThisWeekNewsDay} at {TimeExtensions.ReleasingThisWeekNewsTime.ToTimeOnly()} Eastern"
                : "\nIt will no longer send to this channel."),

            Context.User));
    }
}
