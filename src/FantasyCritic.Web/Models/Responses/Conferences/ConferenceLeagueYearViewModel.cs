using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Models.Responses.Conferences;

public class ConferenceLeagueYearViewModel
{
    public ConferenceLeagueYearViewModel(ConferenceLeagueYear domain, IReadOnlyList<ConferencePlayer> conferencePlayersInLeagueYear, FantasyCriticUser? currentUser)
    {
        LeagueID = domain.League.LeagueID;
        LeagueName = domain.League.LeagueName;
        Year = domain.Year;
        LeagueManager = new PlayerViewModel(domain.League, domain.League.LeagueManager, false);

        if (currentUser is not null)
        {
            UserIsInLeague = conferencePlayersInLeagueYear.Any(x => x.User.Id == currentUser.Id);
        }
    }

    public Guid LeagueID { get; }
    public string LeagueName { get; }
    public int Year { get; }
    public PlayerViewModel LeagueManager { get; }
    public bool UserIsInLeague { get; }
}
