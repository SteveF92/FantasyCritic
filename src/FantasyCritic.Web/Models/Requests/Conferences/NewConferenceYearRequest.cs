namespace FantasyCritic.Web.Models.Requests.Conferences;

public record NewConferenceYearRequest(Guid ConferenceID, int Year, List<Guid> LeaguesToRenew);
