using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Models.Responses.Conferences;

public class ConferenceViewModel
{
    public ConferenceViewModel(Conference domain, bool isManager, bool userIsInConference)
    {
        ConferenceID = domain.ConferenceID;
        ConferenceName = domain.ConferenceName;
        ConferenceManager = new ConferencePlayerViewModel(domain, domain.ConferenceManager);
        IsManager = isManager;
        Years = domain.Years;
        ActiveYear = domain.Years.Max();
        CustomRulesConference = domain.CustomRulesConference;
        UserIsInConference = userIsInConference;
    }
    
    public ConferenceViewModel(Conference domain, bool isManager, bool userIsInConference, IEnumerable<FantasyCriticUser> players, IEnumerable<ConferenceLeague> conferenceLeagues)
    {
        ConferenceID = domain.ConferenceID;
        ConferenceName = domain.ConferenceName;
        ConferenceManager = new ConferencePlayerViewModel(domain, domain.ConferenceManager);
        IsManager = isManager;
        Years = domain.Years;
        ActiveYear = domain.Years.Max();
        CustomRulesConference = domain.CustomRulesConference;
        UserIsInConference = userIsInConference;

        Players = players.Select(x => new ConferencePlayerViewModel(domain, x)).ToList();

        LeaguesInConference = conferenceLeagues.Select(x => new ConferenceLeagueViewModel(x)).ToList();
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

    public IReadOnlyList<ConferencePlayerViewModel>? Players { get; }
    public ConferenceLeagueViewModel? PrimaryLeague { get; }
    public IReadOnlyList<ConferenceLeagueViewModel>? LeaguesInConference { get; }
}
