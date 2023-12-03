using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.Web.Models.Responses.Conferences;

public class ConferenceLeagueYearViewModel
{
    public ConferenceLeagueYearViewModel(ConferenceLeagueYear domain)
    {
        LeagueID = domain.League.LeagueID;
        LeagueName = domain.League.LeagueName;
        Year = domain.SupportedYear.Year;
    }

    public Guid LeagueID { get; }
    public string LeagueName { get; }
    public int Year { get; }
}
