namespace FantasyCritic.Lib.Domain;
public record LeagueChannel(LeagueYear LeagueYear, ulong GuildID, ulong ChannelID, bool IsGameNewsEnabled);
public record MinimalLeagueChannel(Guid LeagueID, ulong GuildID, ulong ChannelID);
