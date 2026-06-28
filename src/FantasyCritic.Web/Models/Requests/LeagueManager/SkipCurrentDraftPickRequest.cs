namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record SkipCurrentDraftPickRequest(Guid LeagueID, int Year, Guid DraftID);
