using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Models.Responses.Conferences;

public class ConferenceLeagueYearViewModel
{
    public ConferenceLeagueYearViewModel(LeagueYear domain, IReadOnlyList<ConferencePlayer> conferencePlayersInLeagueYear, FantasyCriticUser? currentUser, bool isPrimaryLeague)
    {
        LeagueID = domain.League.LeagueID;
        LeagueName = domain.League.LeagueName;
        Year = domain.Year;
        LeagueManager = new PlayerViewModel(domain.League.LeagueID, domain.League.LeagueName, domain.League.LeagueManager, false);

        if (currentUser is not null)
        {
            UserIsInLeague = conferencePlayersInLeagueYear.Any(x => x.User.UserID == currentUser.Id);
        }

        IsPrimaryLeague = isPrimaryLeague;

        ConferenceLocked = domain.ConferenceLocked.HasValue && domain.ConferenceLocked.Value;
        DraftStarted = domain.PlayStatus.PlayStarted;
        DraftFinished = domain.PlayStatus.DraftFinished;
    }

    public Guid LeagueID { get; }
    public string LeagueName { get; }
    public int Year { get; }
    public PlayerViewModel LeagueManager { get; }
    public bool UserIsInLeague { get; }
    public bool IsPrimaryLeague { get; }

    public bool ConferenceLocked { get; }
    public bool DraftStarted { get; }
    public bool DraftFinished { get; }
}
