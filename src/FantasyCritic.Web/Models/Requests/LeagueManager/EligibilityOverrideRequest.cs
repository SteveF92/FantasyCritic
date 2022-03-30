namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record EligibilityOverrideRequest(Guid LeagueID, int Year, Guid MasterGameID, bool? Eligible);
