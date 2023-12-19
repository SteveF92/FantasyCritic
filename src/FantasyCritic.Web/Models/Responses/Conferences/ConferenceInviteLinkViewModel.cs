using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.Web.Models.Responses.Conferences;

public class ConferenceInviteLinkViewModel
{
    public ConferenceInviteLinkViewModel(ConferenceInviteLink domain, int currentYear, string baseURL)
    {
        InviteID = domain.InviteID;
        ConferenceID = domain.Conference.ConferenceID;
        InviteCode = domain.InviteCode;
        FullInviteLink = baseURL + $"/conference/{ConferenceID}/{currentYear}?inviteCode={InviteCode}";
    }

    public Guid InviteID { get; }
    public Guid ConferenceID { get; }
    public Guid InviteCode { get; }
    public string FullInviteLink { get; }
}
