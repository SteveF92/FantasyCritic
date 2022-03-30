namespace FantasyCritic.Web.Models.Requests.Admin;

public record MergeGameRequest(Guid RemoveMasterGameID, Guid MergeIntoMasterGameID);
