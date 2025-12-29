using Discord;

namespace FantasyCritic.Lib.Discord.Handlers;

public class RoleHandler
{
    public bool CanAdministrate(IGuild guild, IUser user, ILeagueChannel? leagueChannel, bool[]? otherPermissions = null)
    {
        return
            leagueChannel != null
            && user is IGuildUser guildUser
            && (guildUser.GuildPermissions.Administrator
            || leagueChannel.BotAdminRoleID == null //allowed for default behaviour
            || guildUser.RoleIds.Any(r => r == leagueChannel.BotAdminRoleID)
            || (otherPermissions != null && otherPermissions.Any(p => p)));
    }
}
