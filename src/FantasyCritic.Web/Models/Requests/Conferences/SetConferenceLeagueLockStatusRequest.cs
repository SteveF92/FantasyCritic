namespace FantasyCritic.Web.Models.Requests.Conferences;

public record SetConferenceLeagueLockStatusRequest(Guid ConferenceID, int Year, Guid LeagueID, bool Locked);
