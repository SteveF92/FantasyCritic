using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.Web.Models.Responses.Conferences;

public class ConferenceLeagueYearViewModel
{
    public ConferenceLeagueYearViewModel(ConferenceLeagueYear domain, bool userIsInLeague)
    {
        LeagueID = domain.League.LeagueID;
        LeagueName = domain.League.LeagueName;
        Year = domain.Year;
        LeagueManager = new PlayerViewModel(domain.League, domain.League.LeagueManager, false);
        UserIsInLeague = userIsInLeague;
    }

    public Guid LeagueID { get; }
    public string LeagueName { get; }
    public int Year { get; }
    public PlayerViewModel LeagueManager { get; }
    public bool UserIsInLeague { get; }
}
