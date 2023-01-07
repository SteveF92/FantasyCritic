using FantasyCritic.Lib.Discord.GameNewsSettings;

namespace FantasyCritic.Lib.Domain;
public record LeagueChannel(LeagueYear LeagueYear, ulong GuildID, ulong ChannelID, DiscordGameNewsSetting GameNewsSetting, ulong? PublicBidAlertRoleID);
public record MinimalLeagueChannel(Guid LeagueID, ulong GuildID, ulong ChannelID, DiscordGameNewsSetting GameNewsSetting, ulong? BidAlertRoleID);
