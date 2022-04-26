namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record DraftOrderRequest(Guid LeagueID, int Year, string DraftOrderType, List<Guid>? ManualPublisherDraftPositions);
