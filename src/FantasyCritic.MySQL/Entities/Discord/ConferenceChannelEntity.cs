using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.MySQL.Entities.Discord;

internal class ConferenceChannelEntity
{
    public ConferenceChannelEntity()
    {

    }

    public ConferenceChannelEntity(ulong guildID, ulong channelID, Guid conferenceID, bool sendLeagueNews)
    {
        GuildID = guildID;
        ChannelID = channelID;
        ConferenceID = conferenceID;
        SendLeagueNews = sendLeagueNews;
    }

    public ulong ChannelID { get; set; }
    public Guid ConferenceID { get; set; }
    public ulong GuildID { get; set; }
    public bool SendLeagueNews { get; set; }

    public ConferenceChannel ToDomain(ConferenceYear conferenceYear)
    {
        return new ConferenceChannel(conferenceYear, GuildID, ChannelID, SendLeagueNews);
    }

    public MinimalConferenceChannel ToMinimalDomain() => new(ConferenceID, GuildID, ChannelID, SendLeagueNews);
}
