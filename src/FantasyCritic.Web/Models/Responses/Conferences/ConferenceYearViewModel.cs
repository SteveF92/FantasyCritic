using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Models.Responses.Conferences;

public class ConferenceYearViewModel
{
    public ConferenceYearViewModel(ConferenceViewModel conferenceViewModel, ConferenceYear domain, IEnumerable<LeagueYear> conferenceLeagueYears,
        IReadOnlyList<ConferencePlayer> conferencePlayers, FantasyCriticUser? accessingUser, IReadOnlyList<ConferenceYearStanding> standings, IEnumerable<ManagerMessage> managerMessages)
    {
        Conference = conferenceViewModel;
        Year = domain.Year;
        SupportedYear = new SupportedYearViewModel(domain.SupportedYear);

        LeagueYears = conferenceLeagueYears.Select(x =>
            new ConferenceLeagueYearViewModel(x,
                conferencePlayers.Where(y => y.LeagueYearsActiveIn.Contains(x.Key)).ToList(), accessingUser,
                x.League.LeagueID == domain.Conference.PrimaryLeagueID))
            .OrderByDescending(x => x.IsPrimaryLeague).ThenBy(x => x.LeagueName).ToList();

        UserIsInAtLeastOneLeague = LeagueYears.Any(x => x.UserIsInLeague);

        var publisherRankings = standings
            .Select(x => new
                {
                    x.PublisherID,
                    Ranking = standings.Count(y => y.TotalFantasyPoints > x.TotalFantasyPoints) + 1
                }
            )
            .ToDictionary(x => x.PublisherID, x => x.Ranking);

        var publisherProjectedRankings = standings
            .Select(x => new
                {
                    x.PublisherID,
                    Ranking = standings.Count(y => y.ProjectedFantasyPoints > x.ProjectedFantasyPoints) + 1
                }
            )
            .ToDictionary(x => x.PublisherID, x => x.Ranking);

        var standingVMs = new List<ConferenceYearStandingViewModel>();

        foreach (var standing in standings)
        {
            var ranking = publisherRankings[standing.PublisherID];
            var projectedRanking = publisherProjectedRankings[standing.PublisherID];
            standingVMs.Add(new ConferenceYearStandingViewModel(standing, ranking, projectedRanking));
        }

        Standings = standingVMs;
        PlayersForYear = conferencePlayers.Where(x => x.YearsActiveIn.Contains(Year)).Select(x => new ConferenceYearPlayerViewModel(domain, x)).ToList();

        ManagerMessages = managerMessages.Select(x => new ManagerMessageViewModel(x, x.IsDismissed(accessingUser))).OrderBy(x => x.Timestamp).ToList();
        if (!Conference.UserIsInConference)
        {
            ManagerMessages = ManagerMessages.Where(x => x.IsPublic).ToList();
        }
    }

    public ConferenceViewModel Conference { get; }
    public int Year { get; }
    public SupportedYearViewModel SupportedYear { get; }
    public IReadOnlyList<ConferenceLeagueYearViewModel> LeagueYears { get; }
    public bool UserIsInAtLeastOneLeague { get; }
    public IReadOnlyList<ConferenceYearStandingViewModel> Standings { get; }
    public IReadOnlyList<ConferenceYearPlayerViewModel> PlayersForYear { get; }
    public IReadOnlyList<ManagerMessageViewModel> ManagerMessages { get; }
}
