using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.Lib.Domain;

public record MinimalLeagueChannel(Guid LeagueID, ulong GuildID, ulong ChannelID, bool SendLeagueMasterGameUpdates, bool SendNotableMisses, ulong? BidAlertRoleID) : IDiscordChannel
{
    public DiscordChannelKey ChannelKey => new DiscordChannelKey(GuildID, ChannelID);

    public MultiYearLeagueChannel ToMultiYearLeagueChannel(IReadOnlyList<LeagueYear> activeLeagueYears)
        => new MultiYearLeagueChannel(LeagueID, activeLeagueYears, GuildID, ChannelID, SendLeagueMasterGameUpdates, SendNotableMisses, BidAlertRoleID);
}

public record LeagueChannel(LeagueYear LeagueYear, ulong GuildID, ulong ChannelID, bool SendLeagueMasterGameUpdates, bool SendNotableMisses, ulong? BidAlertRoleID);

public record MultiYearLeagueChannel(Guid LeagueID, IReadOnlyList<LeagueYear> ActiveLeagueYears, ulong GuildID, ulong ChannelID, bool SendLeagueMasterGameUpdates, bool SendNotableMisses, ulong? BidAlertRoleID);

public record GameNewsChannel(ulong GuildID, ulong ChannelID, GameNewsSetting GameNewsSetting, IReadOnlyList<MasterGameTag> SkippedTags)
{
    public DiscordChannelKey ChannelKey => new DiscordChannelKey(GuildID, ChannelID);
}

public class CombinedChannel
{
    public CombinedChannel(MultiYearLeagueChannel? leagueChannel, GameNewsChannel? gameNewsChannel)
    {
        if (leagueChannel is null && gameNewsChannel is null)
        {
            throw new Exception("Both channel options cannot be null");
        }

        if (leagueChannel is not null)
        {
            GuildID = leagueChannel.GuildID;
            ChannelID = leagueChannel.ChannelID;
            LeagueID = leagueChannel.LeagueID;
            SendLeagueMasterGameUpdates = leagueChannel.SendLeagueMasterGameUpdates;
            SendNotableMisses = leagueChannel.SendNotableMisses;
            ActiveLeagueYears = leagueChannel.ActiveLeagueYears;
        }

        if (gameNewsChannel is not null)
        {
            GuildID = gameNewsChannel.GuildID;
            ChannelID = gameNewsChannel.ChannelID;
            GameNewsSetting = gameNewsChannel.GameNewsSetting;
            SkippedTags = gameNewsChannel.SkippedTags;
        }
        else
        {
            GameNewsSetting = GameNewsSetting.Off;
            SkippedTags = new List<MasterGameTag>();
        }
    }

    public ulong GuildID { get; }
    public ulong ChannelID { get; }
    public Guid? LeagueID { get; }
    public IReadOnlyList<LeagueYear>? ActiveLeagueYears { get; }
    public bool SendLeagueMasterGameUpdates { get; }
    public bool SendNotableMisses { get; }
    public GameNewsSetting GameNewsSetting { get; }
    public IReadOnlyList<MasterGameTag> SkippedTags { get; }

    public DiscordChannelKey ChannelKey => new DiscordChannelKey(GuildID, ChannelID);

    public CombinedChannelGameSetting CombinedSetting => new CombinedChannelGameSetting(SendLeagueMasterGameUpdates, SendNotableMisses, GameNewsSetting, SkippedTags);
}
