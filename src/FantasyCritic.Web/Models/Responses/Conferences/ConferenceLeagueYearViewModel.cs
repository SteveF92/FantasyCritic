using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.Web.Models.Responses.Conferences;

public class ConferenceLeagueYearViewModel
{
    public ConferenceLeagueYearViewModel(ConferenceLeagueYear domain)
    {
        LeagueID = domain.League.LeagueID;
        LeagueName = domain.League.LeagueName;
        Year = domain.Year;
        LeagueManager = new PlayerViewModel(domain.League, domain.League.LeagueManager, false);
    }

    public Guid LeagueID { get; }
    public string LeagueName { get; }
    public int Year { get; }
    public PlayerViewModel LeagueManager { get; }
}
