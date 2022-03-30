namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record AssociateGameRequest(Guid PublisherID, Guid PublisherGameID, Guid MasterGameID, bool ManagerOverride);
