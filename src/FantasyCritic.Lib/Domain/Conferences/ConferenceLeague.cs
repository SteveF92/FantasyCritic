using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain.Conferences;
public class ConferenceLeague
{
    public ConferenceLeague(Guid leagueID, string leagueName, FantasyCriticUser leagueManager)
    {
        LeagueID = leagueID;
        LeagueName = leagueName;
        LeagueManager = leagueManager;
    }

    public Guid LeagueID { get; }
    public string LeagueName { get; }
    public FantasyCriticUser LeagueManager { get; }
}
