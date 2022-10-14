namespace FantasyCritic.MySQL.Entities;
internal class LeagueChannelEntity
{
    public LeagueChannelEntity(Guid leagueID, ulong guildID, ulong channelID)
    {
        LeagueID = leagueID;
        GuildID = guildID;
        ChannelID = channelID;
    }

    public Guid LeagueID { get; set; }
    public ulong GuildID { get; set; }
    public ulong ChannelID { get; set; }

    public LeagueChannel ToDomain(LeagueYear leagueYear)
    {
        return new LeagueChannel(leagueYear, GuildID, ChannelID);
    }
}
