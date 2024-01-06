namespace FantasyCritic.Lib.Domain.Conferences;
public class ConferenceLeagueYear
{
    public ConferenceLeagueYear(ConferenceLeague league, int year, bool draftStarted, bool draftFinished)
    {
        League = league;
        Year = year;

        DraftStarted = draftStarted;
        DraftFinished = draftFinished;
    }

    public ConferenceLeague League { get; }
    public int Year { get; } 
    public LeagueYearKey LeagueYearKey => new LeagueYearKey(League.LeagueID, Year);

    public bool DraftStarted { get; }
    public bool DraftFinished { get; }
}
