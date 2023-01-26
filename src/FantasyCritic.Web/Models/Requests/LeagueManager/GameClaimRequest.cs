namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record ClaimGameRequest(Guid PublisherID, string GameName, bool CounterPick, Guid? MasterGameID, bool ManagerOverride, bool AllowIneligibleSlot);
