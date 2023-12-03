namespace FantasyCritic.Web.Models.Requests.Conferences;

public record PromoteNewLeagueManagerWithinConferenceRequest(Guid ConferenceID, Guid LeagueID, Guid NewManagerUserID);
