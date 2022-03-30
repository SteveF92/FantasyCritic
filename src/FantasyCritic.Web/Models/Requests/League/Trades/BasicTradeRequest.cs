namespace FantasyCritic.Web.Models.Requests.League.Trades;

public record BasicTradeRequest(Guid LeagueID, int Year, Guid TradeID);
