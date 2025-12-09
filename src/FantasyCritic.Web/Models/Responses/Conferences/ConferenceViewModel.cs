using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.Web.Models.Responses.Conferences;

public class ConferenceViewModel
{
    public ConferenceViewModel(Conference domain, bool isManager, bool userIsInConference, IReadOnlyList<ConferencePlayer> players, IEnumerable<ConferenceLeague> conferenceLeagues)
    {
        ConferenceID = domain.ConferenceID;
        ConferenceName = domain.ConferenceName;
        ConferenceManager = new ConferencePlayerViewModel(domain, players.Single(x => x.User.UserID == domain.ConferenceManager.UserID));
        IsManager = isManager;
        Years = domain.Years.Select(x => x.Year).ToList();

        var latestDraftStartedYear = domain.Years.Where(x => x.AtLeastOneDraftStarted).Max();
        var highestNonFinishedYear = domain.Years.Where(x => !x.Finished).Max();
        ActiveYear = latestDraftStartedYear?.Year ?? highestNonFinishedYear?.Year ?? Years.Max();

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
