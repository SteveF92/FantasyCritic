using FantasyCritic.Lib.Discord.GameNewsSettings;

namespace FantasyCritic.MySQL.Entities;
internal class LeagueChannelEntity
{
    public LeagueChannelEntity()
    {

    }

    public LeagueChannelEntity(Guid leagueID, ulong guildID, ulong channelID, DiscordGameNewsSetting gameNewsSetting, ulong? publicBidAlertRoleID)
    {
        LeagueID = leagueID;
        GuildID = guildID;
        ChannelID = channelID;
        GameNewsSetting = gameNewsSetting.Name;
        PublicBidAlertRoleID = publicBidAlertRoleID;
    }

    public Guid LeagueID { get; set; }
    public ulong GuildID { get; set; }
    public ulong ChannelID { get; set; }
    public string GameNewsSetting { get; set; } = null!;
    public ulong? PublicBidAlertRoleID { get; set; }

    public LeagueChannel ToDomain(LeagueYear leagueYear)
    {
        return new LeagueChannel(leagueYear, GuildID, ChannelID, DiscordGameNewsSetting.FromName(GameNewsSetting), PublicBidAlertRoleID);
    }

    public MinimalLeagueChannel ToMinimalDomain()
    {
        return new MinimalLeagueChannel(LeagueID, GuildID, ChannelID, DiscordGameNewsSetting.FromName(GameNewsSetting), PublicBidAlertRoleID);
    }
}
