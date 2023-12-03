namespace FantasyCritic.Web.Models.Requests.Conferences;

public record CreateConferenceInviteLinkRequest(Guid ConferenceID);
public record JoinConferenceWithInviteLinkRequest(Guid ConferenceID, Guid InviteCode);
public record DeleteConferenceInviteLinkRequest(Guid ConferenceID, Guid InviteID);
