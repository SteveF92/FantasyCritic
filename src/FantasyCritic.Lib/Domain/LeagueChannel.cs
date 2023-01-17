using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.Lib.Domain;
public record LeagueChannel(LeagueYear LeagueYear, ulong GuildID, ulong ChannelID, bool SendLeagueMasterGameUpdates, ulong? BidAlertRoleID);
public record MinimalLeagueChannel(Guid LeagueID, ulong GuildID, ulong ChannelID, bool SendLeagueMasterGameUpdates, ulong? BidAlertRoleID);
public record GameNewsChannel(ulong GuildID, ulong ChannelID, GameNewsSetting GameNewsSetting);
