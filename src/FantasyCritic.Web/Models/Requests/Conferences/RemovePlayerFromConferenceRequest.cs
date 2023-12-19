namespace FantasyCritic.Web.Models.Requests.Conferences;

public record RemovePlayerFromConferenceRequest(Guid ConferenceID, Guid UserID);
