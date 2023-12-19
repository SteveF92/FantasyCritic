using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.Web.Models.Responses.Conferences;

public class ConferenceYearViewModel
{
    public ConferenceYearViewModel(ConferenceViewModel conferenceViewModel, ConferenceYear domain, IEnumerable<ConferenceLeagueYear> conferenceLeagueYears, IReadOnlySet<Guid> leaguesThatPlayerIsIn)
    {
        Conference = conferenceViewModel;
        Year = domain.Year;
        SupportedYear = new SupportedYearViewModel(domain.SupportedYear);
        OpenForDrafting = domain.OpenForDrafting;
        LeagueYears = conferenceLeagueYears.Select(x => new ConferenceLeagueYearViewModel(x, leaguesThatPlayerIsIn.Contains(x.League.LeagueID))).ToList();
        UserIsInAtLeastOneLeague = LeagueYears.Any(x => x.UserIsInLeague);
    }

    public ConferenceViewModel Conference { get; }
    public int Year { get; }
    public SupportedYearViewModel SupportedYear { get; }
    public bool OpenForDrafting { get; }
    public IReadOnlyList<ConferenceLeagueYearViewModel> LeagueYears { get; }
    public bool UserIsInAtLeastOneLeague { get; }
}
