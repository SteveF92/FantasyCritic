namespace FantasyCritic.Web.Models.Requests.League.Trades;

public record ProposeTradeRequest(Guid ProposerPublisherID, Guid CounterPartyPublisherID, List<Guid> ProposerPublisherGameIDs,
    List<Guid> CounterPartyPublisherGameIDs, uint ProposerBudgetSendAmount, uint CounterPartyBudgetSendAmount, string Message);
