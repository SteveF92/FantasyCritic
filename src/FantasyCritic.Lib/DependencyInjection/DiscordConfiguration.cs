namespace FantasyCritic.Lib.DependencyInjection;
public record FantasyCriticDiscordConfiguration(string BotToken, string BaseAddress, bool IsDevelopment, ulong? DevDiscordServerId);
