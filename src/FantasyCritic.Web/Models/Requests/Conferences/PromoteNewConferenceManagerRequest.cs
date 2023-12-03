namespace FantasyCritic.Web.Models.Requests.Conferences;

public record PromoteNewConferenceManagerRequest(Guid ConferenceID, Guid NewManagerUserID);
