using Discord;
using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Handlers;
using FantasyCritic.Lib.Interfaces;
using JetBrains.Annotations;

namespace FantasyCritic.Lib.Discord.Commands;

public class SetBotAdminRoleCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly RoleHandler _roleHandler;

    public SetBotAdminRoleCommand(IDiscordRepo discordRepo,
        IDiscordFormatter discordFormatter,
        RoleHandler roleHandler)
    {
        _discordRepo = discordRepo;
        _discordFormatter = discordFormatter;
        _roleHandler = roleHandler;
    }

    [UsedImplicitly]
    [SlashCommand("set-bot-admin-role", "Choose a role that is permitted to administer the bot.")]
    public async Task SetBotAdminRole(
        [Summary("role", "The role to that is permitted to administer the bot")] IRole? role = null
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

        if (role == null)
        {
            await _discordRepo.SetBotAdminRoleId(leagueChannel.LeagueID, Context.Guild.Id, Context.Channel.Id, null);

            await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
                "Bot Admin Role Configuration Removed",
                "The bot admin role restriction has been removed.",
                Context.User));
            return;
        }

        var roleInGuild = Context.Guild.Roles.FirstOrDefault(r => role.Id == r.Id);
        if (roleInGuild == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "No Role Found",
                "The chosen role was not found on this server.",
                Context.User));
            return;
        }

        if (roleInGuild == Context.Guild.EveryoneRole)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "The @everyone role can not be set as bot admin",
                "You cannot use the @everyone role as the bot admin. Please choose a valid role.",
                Context.User));
            return;

        }

        await _discordRepo.SetBotAdminRoleId(leagueChannel.LeagueID, Context.Guild.Id, Context.Channel.Id, roleInGuild.Id);

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
            "Bot Admin Role Configuration Saved",
            $"Bot administration is now restricted to users with the {roleInGuild.Mention} role (as well as the server administrator/owner).",
            Context.User));
    }
}
