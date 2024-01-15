using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.Lib.Domain;

public record ConferenceChannel(ConferenceYear ConferenceYear, ulong GuildID, ulong ChannelID);
