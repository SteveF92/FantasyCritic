namespace FantasyCritic.Web.Models.Requests.Admin;

public record LinkGameToOpenCriticRequest(Guid MasterGameID, int OpenCriticID);
