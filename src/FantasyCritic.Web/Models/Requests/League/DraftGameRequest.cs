namespace FantasyCritic.Web.Models.Requests.League;

public record DraftGameRequest(Guid PublisherID, string GameName, bool CounterPick, Guid? MasterGameID, bool AllowIneligibleSlot);
