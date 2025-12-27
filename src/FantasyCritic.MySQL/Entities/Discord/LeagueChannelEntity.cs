using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.MySQL.Entities.Discord;
internal class LeagueChannelEntity
{
    public LeagueChannelEntity()
    {

    }

    public LeagueChannelEntity(ulong guildID, ulong channelID, Guid leagueID, bool showPickedGameNews, bool showEligibleGameNews, bool showIneligibleGameNews, NotableMissSetting notableMissSetting, ulong? bidAlertRoleID, int? year)
    {
        GuildID = guildID;
        ChannelID = channelID;
        LeagueID = leagueID;
        ShowPickedGameNews = showPickedGameNews;
        ShowEligibleGameNews = showEligibleGameNews;
        ShowIneligibleGameNews = showIneligibleGameNews;
        NotableMissSetting = notableMissSetting.Value;
        BidAlertRoleID = bidAlertRoleID;
        Year = year;
    }

    public ulong ChannelID { get; set; }
    public Guid LeagueID { get; set; }
    public ulong GuildID { get; set; }
    public bool ShowPickedGameNews { get; set; }
    public bool ShowEligibleGameNews { get; set; }
    public bool ShowIneligibleGameNews { get; set; }
    public string NotableMissSetting { get; set; } = null!;
    public ulong? BidAlertRoleID { get; set; }
    public int? Year { get; set; }

    public LeagueChannel ToDomain(LeagueYear leagueYear)
    {
        return new LeagueChannel(leagueYear, GuildID, ChannelID, ShowPickedGameNews, ShowEligibleGameNews, ShowIneligibleGameNews, Lib.Discord.Models.NotableMissSetting.FromValue(NotableMissSetting), BidAlertRoleID, Year);
    }

    public MinimalLeagueChannel ToMinimalDomain()
    {
        return new MinimalLeagueChannel(LeagueID, GuildID, ChannelID, ShowPickedGameNews, ShowEligibleGameNews, ShowIneligibleGameNews, Lib.Discord.Models.NotableMissSetting.FromValue(NotableMissSetting), BidAlertRoleID, Year);
    }
}
