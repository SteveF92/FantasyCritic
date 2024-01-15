using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.MySQL.Entities.Conferences;
internal class ConferenceLeagueYearEntity
{
    public Guid LeagueID { get; set; }
    public string LeagueName { get; set; } = null!;
    public Guid LeagueManager { get; set; }
    public int Year { get; set; }

    public bool DraftStarted { get; set; }
    public bool DraftFinished { get; set; }
    public bool ConferenceLocked { get; set; }

    public ConferenceLeagueYear ToDomain(FantasyCriticUser leagueManager)
    {
        var league = new ConferenceLeague(LeagueID, LeagueName, leagueManager);
        return new ConferenceLeagueYear(league, Year, ConferenceLocked, DraftStarted, DraftFinished);
    }
}
