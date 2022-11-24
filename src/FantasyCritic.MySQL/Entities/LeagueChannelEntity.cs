namespace FantasyCritic.MySQL.Entities;
internal class LeagueChannelEntity
{
    public LeagueChannelEntity()
    {
        
    }

    public LeagueChannelEntity(Guid leagueID, ulong guildID, ulong channelID, DiscordGameNewsSetting gameNewsSetting)
    {
        LeagueID = leagueID;
        GuildID = guildID;
        ChannelID = channelID;
        GameNewsSetting = gameNewsSetting.Value;
    }

    public Guid LeagueID { get; set; }
    public ulong GuildID { get; set; }
    public ulong ChannelID { get; set; }
    public string GameNewsSetting { get; set; } = null!;

    public LeagueChannel ToDomain(LeagueYear leagueYear)
    {
        return new LeagueChannel(leagueYear, GuildID, ChannelID, DiscordGameNewsSetting.FromValue(GameNewsSetting));
    }

    public MinimalLeagueChannel ToMinimalDomain()
    {
        return new MinimalLeagueChannel(LeagueID, GuildID, ChannelID, DiscordGameNewsSetting.FromValue(GameNewsSetting));
    }
}
