namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record ChangeLeagueOptionsRequest(Guid LeagueID, string LeagueName, bool PublicLeague, bool TestLeague);
