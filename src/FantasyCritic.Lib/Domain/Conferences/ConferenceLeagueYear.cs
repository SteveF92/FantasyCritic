namespace FantasyCritic.Lib.Domain.Conferences;
public class ConferenceLeagueYear
{
    public ConferenceLeagueYear(ConferenceLeague league, SupportedYear supportedYear)
    {
        League = league;
        SupportedYear = supportedYear;
    }

    public ConferenceLeague League { get; }
    public SupportedYear SupportedYear { get; }
}
