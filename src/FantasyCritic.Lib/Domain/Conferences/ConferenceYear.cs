
namespace FantasyCritic.Lib.Domain.Conferences;
public class ConferenceYear
{
    public ConferenceYear(Conference conference, SupportedYear supportedYear)
    {
        Conference = conference;
        SupportedYear = supportedYear;
    }

    public Conference Conference { get; }
    public SupportedYear SupportedYear { get; }

    public int Year => SupportedYear.Year;
}
