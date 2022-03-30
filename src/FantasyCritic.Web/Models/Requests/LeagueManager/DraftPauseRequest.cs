namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record DraftPauseRequest(Guid LeagueID, int Year, bool Pause);
