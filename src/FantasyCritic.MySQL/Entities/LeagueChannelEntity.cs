namespace FantasyCritic.MySQL.Entities;
public class LeagueChannelEntity
{
    public LeagueChannelEntity(Guid leagueID, string channelID)
    {
        LeagueID = leagueID;
        ChannelID = channelID;
    }

    public Guid LeagueID { get; set; }
    public string ChannelID { get; set; }

    public LeagueChannel ToDomain(LeagueYear leagueYear)
    {
        return new LeagueChannel(leagueYear, ChannelID);
    }
}
