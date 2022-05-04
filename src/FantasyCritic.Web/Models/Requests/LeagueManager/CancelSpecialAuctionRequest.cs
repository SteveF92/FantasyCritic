namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record CancelSpecialAuctionRequest(Guid LeagueID, int Year, Guid MasterGameID);
