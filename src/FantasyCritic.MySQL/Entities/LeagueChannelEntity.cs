namespace FantasyCritic.MySQL.Entities;
internal class LeagueChannelEntity
{
    public LeagueChannelEntity()
    {
        
    }

    public LeagueChannelEntity(Guid leagueID, ulong guildID, ulong channelID, bool isGameNewsEnabled)
    {
        LeagueID = leagueID;
        GuildID = guildID;
        ChannelID = channelID;
        IsGameNewsEnabled = isGameNewsEnabled;
    }

    public Guid LeagueID { get; set; }
    public ulong GuildID { get; set; }
    public ulong ChannelID { get; set; }
    public bool IsGameNewsEnabled { get; set; }

    public LeagueChannel ToDomain(LeagueYear leagueYear)
    {
        return new LeagueChannel(leagueYear, GuildID, ChannelID, IsGameNewsEnabled);
    }

    public MinimalLeagueChannel ToMinimalDomain()
    {
        return new MinimalLeagueChannel(LeagueID, GuildID, ChannelID, IsGameNewsEnabled);
    }
}
