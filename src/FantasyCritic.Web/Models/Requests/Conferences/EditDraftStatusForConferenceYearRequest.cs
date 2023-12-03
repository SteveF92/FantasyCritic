namespace FantasyCritic.Web.Models.Requests.Conferences;

public record EditDraftStatusForConferenceYearRequest(Guid ConferenceID, int Year, bool OpenForDrafting);
