namespace FantasyCritic.MySQL.Entities;
internal class LeagueChannelEntity
{
    public LeagueChannelEntity()
    {

    }

    public LeagueChannelEntity(ulong guildID, ulong channelID, Guid leagueID, bool sendLeagueMasterGameUpdates, bool sendNotableMisses, ulong? bidAlertRoleID)
    {
        GuildID = guildID;
        ChannelID = channelID;
        LeagueID = leagueID;
        SendLeagueMasterGameUpdates = sendLeagueMasterGameUpdates;
        SendNotableMisses = sendNotableMisses;
        BidAlertRoleID = bidAlertRoleID;
    }

    public ulong ChannelID { get; set; }
    public Guid LeagueID { get; set; }
    public ulong GuildID { get; set; }
    public bool SendLeagueMasterGameUpdates { get; set; }
    public bool SendNotableMisses { get; set; }
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
