namespace FantasyCritic.Web.Models.Requests.League;

public record SetArchiveStatusRequest(Guid LeagueID, bool Archive);
