namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record CreateSpecialAuctionRequest(Guid LeagueID, int Year, Guid MasterGameID, Instant ScheduledEndTime);
