using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Models.Responses.Conferences;

public class ConferenceYearViewModel
{
    public ConferenceYearViewModel(ConferenceViewModel conferenceViewModel, ConferenceYear domain, IEnumerable<ConferenceLeagueYear> conferenceLeagueYears,
        IReadOnlyList<ConferencePlayer> conferencePlayers, FantasyCriticUser? currentUser)
    {
        Conference = conferenceViewModel;
        Year = domain.Year;
        SupportedYear = new SupportedYearViewModel(domain.SupportedYear);
        OpenForDrafting = domain.OpenForDrafting;

        LeagueYears = conferenceLeagueYears.Select(x =>
            new ConferenceLeagueYearViewModel(x,
                conferencePlayers.Where(y => y.YearsActiveIn.Contains(x.LeagueYearKey)).ToList(), currentUser,
                x.League.LeagueID == domain.Conference.PrimaryLeagueID))
            .OrderByDescending(x => x.IsPrimaryLeague).ToList();

        UserIsInAtLeastOneLeague = LeagueYears.Any(x => x.UserIsInLeague);
    }

    public ConferenceViewModel Conference { get; }
    public int Year { get; }
    public SupportedYearViewModel SupportedYear { get; }
    public bool OpenForDrafting { get; }
    public IReadOnlyList<ConferenceLeagueYearViewModel> LeagueYears { get; }
    public bool UserIsInAtLeastOneLeague { get; }
}
