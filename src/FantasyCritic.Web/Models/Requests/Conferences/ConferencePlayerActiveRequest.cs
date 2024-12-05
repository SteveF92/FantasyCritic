namespace FantasyCritic.Web.Models.Requests.Conferences;

public record ConferencePlayerActiveRequest(Guid ConferenceID, int Year, Dictionary<Guid, bool> ActiveStatus);
