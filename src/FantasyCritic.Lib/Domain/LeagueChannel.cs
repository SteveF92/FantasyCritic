using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.Lib.Domain;
public record LeagueChannel(LeagueYear LeagueYear, ulong GuildID, ulong ChannelID, bool SendLeagueMasterGameUpdates, ulong? BidAlertRoleID);
public record MinimalLeagueChannel(Guid LeagueID, ulong GuildID, ulong ChannelID, bool SendLeagueMasterGameUpdates, ulong? BidAlertRoleID);
public record GameNewsChannel(ulong GuildID, ulong ChannelID, GameNewsSetting GameNewsSetting);

public class CombinedChannel
{
    public CombinedChannel(MinimalLeagueChannel? leagueChannel, GameNewsChannel? gameNewsChannel)
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
    public bool SendLeagueMasterGameUpdates { get; }
    public GameNewsSetting? GameNewsSetting { get; }

    public CombinedChannelGameSetting CombinedSetting => new CombinedChannelGameSetting(SendLeagueMasterGameUpdates, GameNewsSetting);
}
