using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.Lib.Domain;

public record ConferenceChannel(ConferenceYear ConferenceYear, ulong GuildID, ulong ChannelID);

public record MinimalConferenceChannel(Guid ConferenceID, ulong GuildID, ulong ChannelID) : IDiscordChannel;
