namespace FantasyCritic.Web.Models.Requests.League;

public record ReorderPublisherGamesRequest(Guid PublisherID, Dictionary<int, Guid?> SlotStates);
