using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.MySQL.Entities.Conferences;

internal class ConferenceInviteLinkEntity
{
    public ConferenceInviteLinkEntity()
    {

    }

    public ConferenceInviteLinkEntity(ConferenceInviteLink domain)
    {
        InviteID = domain.InviteID;
        ConferenceID = domain.Conference.ConferenceID;
        InviteCode = domain.InviteCode;
        Active = domain.Active;
    }

    public Guid InviteID { get; set; }
    public Guid ConferenceID { get; set; }
    public Guid InviteCode { get; set; }
    public bool Active { get; set; }

    public ConferenceInviteLink ToDomain(Conference conference)
    {
        return new ConferenceInviteLink(InviteID, conference, InviteCode, Active);
    }
}
