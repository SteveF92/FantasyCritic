namespace FantasyCritic.Web.Models.Responses;
public record LeagueActionForCsvExport(DateOnly ProcessDate, string ProcessName, string PublisherName, string ActionType, string GameName,
    uint BidAmount, int Priority, bool? Successful, string? Outcome, decimal? ProjectedPointsAtTimeOfBid, bool CounterPick, string? ConditionalDropGame);
