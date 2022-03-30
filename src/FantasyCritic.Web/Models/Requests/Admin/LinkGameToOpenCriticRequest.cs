namespace FantasyCritic.Web.Models.Requests.Admin;

public record LinkGameToGGRequest(Guid MasterGameID, string GGToken);
