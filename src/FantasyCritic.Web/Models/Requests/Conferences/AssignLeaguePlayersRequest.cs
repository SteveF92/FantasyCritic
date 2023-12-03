namespace FantasyCritic.Web.Models.Requests.Conferences;

public record AssignLeaguePlayersRequest(Guid ConferenceID, int Year, Dictionary<Guid, Guid> LeagueAssignments);
