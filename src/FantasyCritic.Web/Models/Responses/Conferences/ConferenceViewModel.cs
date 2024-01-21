using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.Web.Models.Responses.Conferences;

public class ConferenceViewModel
{
    public ConferenceViewModel(Conference domain, bool isManager, bool userIsInConference, IReadOnlyList<ConferencePlayer> players, IEnumerable<ConferenceLeague> conferenceLeagues)
    {
        ConferenceID = domain.ConferenceID;
        ConferenceName = domain.ConferenceName;
        ConferenceManager = new ConferencePlayerViewModel(domain, players.Single(x => x.User.Id == domain.ConferenceManager.Id));
        IsManager = isManager;
        Years = domain.Years;
        ActiveYear = domain.Years.Max();
        CustomRulesConference = domain.CustomRulesConference;
        UserIsInConference = userIsInConference;

        Players = players.Select(x => new ConferencePlayerViewModel(domain, x)).ToList();

        LeaguesInConference = conferenceLeagues.Select(x => new ConferenceLeagueViewModel(x))
            .OrderByDescending(x => x.LeagueID == domain.PrimaryLeagueID)
            .ThenBy(x => x.LeagueName)
            .ToList();
        PrimaryLeague = LeaguesInConference.Single(x => x.LeagueID == domain.PrimaryLeagueID);
    }

    public Guid ConferenceID { get; }
    public string ConferenceName { get; }
    public ConferencePlayerViewModel ConferenceManager { get; }
    public bool IsManager { get; }
    public IReadOnlyList<int> Years { get; }
    public int ActiveYear { get; }
    public bool CustomRulesConference { get; }
    public bool UserIsInConference { get; }

    public IReadOnlyList<ConferencePlayerViewModel> Players { get; }
    public ConferenceLeagueViewModel PrimaryLeague { get; }
    public IReadOnlyList<ConferenceLeagueViewModel> LeaguesInConference { get; }
}
