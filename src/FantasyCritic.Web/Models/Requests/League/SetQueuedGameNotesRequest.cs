namespace FantasyCritic.Web.Models.Requests.League;

public record SetQueuedGameNotesRequest(Guid PublisherID, Guid MasterGameID, string? Notes);
