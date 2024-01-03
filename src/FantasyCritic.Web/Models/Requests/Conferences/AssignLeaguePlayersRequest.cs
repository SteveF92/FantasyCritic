namespace FantasyCritic.Web.Models.Requests.Conferences;

public record AssignLeaguePlayersRequest(Guid ConferenceID, Dictionary<Guid, List<Guid>> LeagueAssignments);
