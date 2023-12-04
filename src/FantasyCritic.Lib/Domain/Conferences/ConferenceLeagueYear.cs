namespace FantasyCritic.Lib.Domain.Conferences;
public class ConferenceLeagueYear
{
    public ConferenceLeagueYear(ConferenceLeague league, int year)
    {
        League = league;
        Year = year;
    }

    public ConferenceLeague League { get; }
    public int Year { get; }
}
