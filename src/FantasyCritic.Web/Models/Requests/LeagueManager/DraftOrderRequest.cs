namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record DraftOrderRequest(Guid LeagueID, int Year, Guid DraftID, string DraftOrderType, List<Guid>? ManualPublisherDraftPositions);
