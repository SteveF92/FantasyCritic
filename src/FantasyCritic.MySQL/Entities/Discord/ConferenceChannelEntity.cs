using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.MySQL.Entities.Discord;
internal class ConferenceChannelEntity
{
    public ConferenceChannelEntity()
    {

    }

    public ConferenceChannelEntity(ulong guildID, ulong channelID, Guid conferenceID)
    {
        GuildID = guildID;
        ChannelID = channelID;
        ConferenceID = conferenceID;
    }

    public ulong ChannelID { get; set; }
    public Guid ConferenceID { get; set; }
    public ulong GuildID { get; set; }

    public ConferenceChannel ToDomain(ConferenceYear conferenceYear)
    {
        return new ConferenceChannel(conferenceYear, GuildID, ChannelID);
    }

    public MinimalConferenceChannel ToMinimalDomain() => new MinimalConferenceChannel(ConferenceID, GuildID, ChannelID);
}
