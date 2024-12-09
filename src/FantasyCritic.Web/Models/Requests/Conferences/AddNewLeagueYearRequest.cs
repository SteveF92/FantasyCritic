namespace FantasyCritic.Web.Models.Requests.Conferences;

public record AddNewLeagueYearRequest(Guid ConferenceID, int Year,  Guid LeagueID);
