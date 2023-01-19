namespace FantasyCritic.MySQL.Entities;
internal class LeagueChannelEntity
{
    public LeagueChannelEntity()
    {

    }

    public LeagueChannelEntity(Guid leagueID, ulong guildID, ulong channelID, bool sendLeagueMasterGameUpdates, ulong? bidAlertRoleID)
    {
        LeagueID = leagueID;
        GuildID = guildID;
        ChannelID = channelID;
        SendLeagueMasterGameUpdates = sendLeagueMasterGameUpdates;
        BidAlertRoleID = bidAlertRoleID;
    }

    public Guid LeagueID { get; set; }
    public ulong GuildID { get; set; }
    public ulong ChannelID { get; set; }
    public bool SendLeagueMasterGameUpdates { get; set; }
    public bool SendNotableMisses { get; set; }
    public int MinimumLeagueYear { get; set; }
    public ulong? BidAlertRoleID { get; set; }

    public LeagueChannel ToDomain(LeagueYear leagueYear)
    {
        return new LeagueChannel(leagueYear, GuildID, ChannelID, SendLeagueMasterGameUpdates, SendNotableMisses, BidAlertRoleID);
    }

    public MinimalLeagueChannel ToMinimalDomain()
    {
        return new MinimalLeagueChannel(LeagueID, GuildID, ChannelID, SendLeagueMasterGameUpdates, SendNotableMisses, BidAlertRoleID);
    }
}
