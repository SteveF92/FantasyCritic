namespace FantasyCritic.Lib.Domain.Conferences;
public class ConferenceLeagueYear
{
    public ConferenceLeagueYear(ConferenceLeague league, int year, bool conferenceLocked, bool draftStarted, bool draftFinished)
    {
        League = league;
        Year = year;

        ConferenceLocked = conferenceLocked;
        DraftStarted = draftStarted;
        DraftFinished = draftFinished;
    }

    public ConferenceLeague League { get; }
    public int Year { get; }
    public LeagueYearKey LeagueYearKey => new LeagueYearKey(League.LeagueID, Year);

    public bool ConferenceLocked { get; }
    public bool DraftStarted { get; }
    public bool DraftFinished { get; }
}
