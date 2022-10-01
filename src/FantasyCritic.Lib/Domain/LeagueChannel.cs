namespace FantasyCritic.Lib.Domain;
public class LeagueChannel
{
    public LeagueChannel(LeagueYear leagueYear, string channelId)
    {
        LeagueYear = leagueYear;
        ChannelID = channelId;
    }

    public LeagueYear LeagueYear { get; set; }
    public string ChannelID { get; set; }
}
