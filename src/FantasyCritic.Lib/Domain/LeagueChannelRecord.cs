using FantasyCritic.Lib.Discord.Interfaces;
using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.Lib.Domain;

public record MinimalLeagueChannelRecord(
    ulong GuildID,
    ulong ChannelID,
    Guid LeagueID,
    ulong? BidAlertRoleID) : IDiscordChannel
{
    public ulong GuildID { get; set; } = GuildID;
    public ulong ChannelID { get; set; } = ChannelID;
    public Guid LeagueID { get; set; } = LeagueID;
    public DiscordChannelKey ChannelKey => new DiscordChannelKey(GuildID, ChannelID);
    public ulong? BidAlertRoleID { get; set; } = BidAlertRoleID;

}

public record LeagueChannelRecord(
    ulong GuildID,
    ulong ChannelID,
    Guid LeagueID,
    LeagueYear CurrentYear,
    IReadOnlyList<LeagueYear> ActiveLeagueYears,
    LeagueGameNewsSettingsRecord? LeagueGameNewsSettings,
    ulong? BidAlertRoleID
)
{
    public DiscordChannelKey ChannelKey => new DiscordChannelKey(GuildID, ChannelID);
}

public record GameNewsOnlyChannelRecord(
    ulong GuildID,
    ulong ChannelID,
    IReadOnlyList<MasterGameTag> SkippedTags,
    GameNewsSettingsRecord? GameNewsSettings)
{
    public DiscordChannelKey ChannelKey => new DiscordChannelKey(GuildID, ChannelID);
}
