namespace FantasyCritic.Web.Models.Requests.Conferences;

public record AssignLeaguePlayersRequest(Guid ConferenceID, Dictionary<Guid, Guid> LeagueAssignments);
