using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.Web.Models.Responses.Conferences;

public class ConferenceLeagueViewModel
{
    public ConferenceLeagueViewModel(ConferenceLeague domain)
    {
        LeagueID = domain.LeagueID;
        LeagueName = domain.LeagueName;
        LeagueManager = new PlayerViewModel(domain.LeagueID, domain.LeagueName, domain.LeagueManager, false);
    }

    public Guid LeagueID { get; }
    public string LeagueName { get; }
    public PlayerViewModel LeagueManager { get; }
}
