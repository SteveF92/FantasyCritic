using Discord;
using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Interfaces;
using JetBrains.Annotations;

namespace FantasyCritic.Lib.Discord.Commands;
public class SetBidAlertRoleCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly IDiscordFormatter _discordFormatter;

    public SetBidAlertRoleCommand(IDiscordRepo discordRepo,
        IDiscordFormatter discordFormatter)
    {
        _discordRepo = discordRepo;
        _discordFormatter = discordFormatter;
    }

    [UsedImplicitly]
    [SlashCommand("set-bid-alert-role", "Set a role to mention when bid information is posted.")]
    public async Task SetBidAlertRole(
        [Summary("role", "The role to mention when bid information is posted")] IRole? role = null
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

        if (role == null)
        {
            await _discordRepo.SetBidAlertRoleId(leagueChannel.LeagueID, Context.Guild.Id, Context.Channel.Id, null);

            await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
                "Bid Information Alert Role Configuration Removed",
                "Bid information updates will no longer mention any role.",
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
            if (!(Context.User as IGuildUser)?.GuildPermissions.MentionEveryone == true)
            {
                await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                    "The @everyone role is not Mentionable",
                    "You do not have permission to use the @everyone role. Please ask someone with the appropriate permissions to make this role mentionable and try again, or try a different role.",
                    Context.User));
                return;
            }
        }
        else if (!roleInGuild.IsMentionable)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Role is not Mentionable",
                "The chosen role is not mentionable. Please ask someone with the appropriate permissions to make this role mentionable and try again.",
                Context.User));
            return;
        }

        await _discordRepo.SetBidAlertRoleId(leagueChannel.LeagueID, Context.Guild.Id, Context.Channel.Id, roleInGuild.Id);

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
            "Bid Information Alert Role Configuration Saved",
            $"Bid information updates will now mention {roleInGuild.Mention}.",
            Context.User));
    }
}
