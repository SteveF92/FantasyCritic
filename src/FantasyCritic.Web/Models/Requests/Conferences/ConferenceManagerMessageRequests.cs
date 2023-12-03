namespace FantasyCritic.Web.Models.Requests.Conferences;

public record PostNewConferenceManagerMessageRequest(Guid ConferenceID, int Year, string Message, bool IsPublic);
public record DeleteConferenceManagerMessageRequest(Guid ConferenceID, int Year, Guid MessageID);
public record DismissConferenceManagerMessageRequest(Guid MessageID);
