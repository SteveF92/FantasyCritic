using Discord;

namespace FantasyCritic.Lib.Discord.Handlers;

public class RoleHandler
{
    public bool CanAdministrate(IGuild guild, IUser user, ILeagueChannel? leagueChannel, bool[]? otherPermissions = null)
    {
        if (leagueChannel == null)
        {
            // no existing channel to administrate, allowing this for game news only channels
            return true;
        }
        return
            user is IGuildUser guildUser
            && (guildUser.GuildPermissions.Administrator
            || leagueChannel.BotAdminRoleID == null //allowed for default behaviour
            || guildUser.RoleIds.Any(r => r == leagueChannel.BotAdminRoleID) //checking allowed roles
            || (otherPermissions != null && otherPermissions.Any(p => p))); //checking manually passed in permissions (if used)
    }
}
