using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.Lib.Domain;
public record LeagueChannel(LeagueYear LeagueYear, ulong GuildID, ulong ChannelID, bool SendLeagueMasterGameUpdates, bool SendNotableMisses, ulong? BidAlertRoleID);

public record MinimalLeagueChannel(Guid LeagueID, IReadOnlyList<int> ActiveYears, ulong GuildID, ulong ChannelID, bool SendLeagueMasterGameUpdates, bool SendNotableMisses, ulong? BidAlertRoleID)
{
    public DiscordChannelKey ChannelKey => new DiscordChannelKey(GuildID, ChannelID);
}

public record GameNewsChannel(ulong GuildID, ulong ChannelID, GameNewsSetting GameNewsSetting)
{
    public DiscordChannelKey ChannelKey => new DiscordChannelKey(GuildID, ChannelID);
}

public class CombinedChannel
{
    public CombinedChannel(MinimalLeagueChannel? leagueChannel, GameNewsChannel? gameNewsChannel, int currentYear)
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
            ActiveYears = leagueChannel.ActiveYears.ToList();
            SendLeagueMasterGameUpdates = leagueChannel.SendLeagueMasterGameUpdates;
            SendNotableMisses = leagueChannel.SendNotableMisses;
        }
        else
        {
            ActiveYears = new List<int>() { currentYear };
        }

        if (gameNewsChannel is not null)
        {
            GuildID = gameNewsChannel.GuildID;
            ChannelID = gameNewsChannel.ChannelID;
            GameNewsSetting = gameNewsChannel.GameNewsSetting;
        }
    }

    public ulong GuildID { get; }
    public ulong ChannelID { get; }
    public Guid? LeagueID { get; }
    public IReadOnlyList<int> ActiveYears { get; }
    public bool SendLeagueMasterGameUpdates { get; }
    public bool SendNotableMisses { get; }
    public GameNewsSetting? GameNewsSetting { get; }

    public CombinedChannelGameSetting CombinedSetting => new CombinedChannelGameSetting(SendLeagueMasterGameUpdates, GameNewsSetting);
}
