namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record SetUnderReviewStatusRequest(Guid LeagueID, int Year, bool UnderReview);
