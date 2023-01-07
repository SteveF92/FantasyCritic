using Discord;
using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Discord.Commands;
public class SetPublicBidAlertRoleCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly IDiscordFormatter _discordFormatter;

    public SetPublicBidAlertRoleCommand(IDiscordRepo discordRepo,
        IDiscordFormatter discordFormatter)
    {
        _discordRepo = discordRepo;
        _discordFormatter = discordFormatter;
    }

    [SlashCommand("set-public-bid-role", "Set a role to mention when public bids are posted.")]
    public async Task SetPublicBidAlertRole(
        [Summary("role", "The role to mention when public bids are posted")] IRole? role = null
        )
    {
        await DeferAsync();
        var leagueChannel = await _discordRepo.GetMinimalLeagueChannel(Context.Guild.Id, Context.Channel.Id);
        if (leagueChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Finding League Configuration",
                "No league configuration found for this channel.",
                Context.User));
            return;
        }

        if (role == null)
        {
            await _discordRepo.SetPublicBidAlertRoleId(leagueChannel.LeagueID, Context.Guild.Id, Context.Channel.Id, null);

            await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
                "Public Bid Alert Role Configuration Removed",
                "Public Bid updates will no longer mention any role.",
                Context.User));
            return;
        }

        var roleInGuild = Context.Guild.Roles.FirstOrDefault(r => role.Id == r.Id);
        if (roleInGuild == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "No Role Found",
                "The chosen role was not found on this server.",
                Context.User));
            return;
        }

        if (roleInGuild == Context.Guild.EveryoneRole)
        {
            if (!(Context.User as IGuildUser)?.GuildPermissions.MentionEveryone == true)
            {
                await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                    "The @everyone role is not Mentionable",
                    "You do not have permission to use the @everyone role. Please ask someone with the appropriate permissions to make this role mentionable and try again, or try a different role.",
                    Context.User));
                return;
            }
        }
        else if (!roleInGuild.IsMentionable)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Role is not Mentionable",
                "The chosen role is not mentionable. Please ask someone with the appropriate permissions to make this role mentionable and try again.",
                Context.User));
            return;
        }

        await _discordRepo.SetPublicBidAlertRoleId(leagueChannel.LeagueID, Context.Guild.Id, Context.Channel.Id, roleInGuild.Id);

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
            "Public Bid Alert Role Configuration Saved",
            $"Public Bid updates will now mention {roleInGuild.Mention}.",
            Context.User));
    }
}
