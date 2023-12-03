
namespace FantasyCritic.Lib.Domain.Conferences;
public class ConferenceYear
{
    public ConferenceYear(Conference conference, SupportedYear supportedYear, bool openForDrafting)
    {
        Conference = conference;
        SupportedYear = supportedYear;
        OpenForDrafting = openForDrafting;
    }

    public Conference Conference { get; }
    public SupportedYear SupportedYear { get; }
    public bool OpenForDrafting { get; }

    public int Year => SupportedYear.Year;
}
