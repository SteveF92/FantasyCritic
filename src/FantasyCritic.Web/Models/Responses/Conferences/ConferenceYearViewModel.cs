using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.Web.Models.Responses.Conferences;

public class ConferenceYearViewModel
{
    public ConferenceYearViewModel(ConferenceViewModel conferenceViewModel, ConferenceYear domain, IEnumerable<ConferenceLeagueYear> conferenceLeagueYears)
    {
        Conference = conferenceViewModel;
        Year = domain.Year;
        SupportedYear = new SupportedYearViewModel(domain.SupportedYear);
        OpenForDrafting = domain.OpenForDrafting;
        LeagueYears = conferenceLeagueYears.Select(x => new ConferenceLeagueYearViewModel(x)).ToList();
    }

    public ConferenceViewModel Conference { get; }
    public int Year { get; }
    public SupportedYearViewModel SupportedYear { get; }
    public bool OpenForDrafting { get; }
    public IReadOnlyList<ConferenceLeagueYearViewModel> LeagueYears { get; }
}
