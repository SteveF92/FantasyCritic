namespace FantasyCritic.Web.Models.Requests.League.Trades;

public record TradeVoteRequest(Guid LeagueID, int Year, Guid TradeID, bool Approved, string? Comment);
