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

    public string LeagueManagerDisplayName { get; set; } = null!;
    public string LeagueManagerEmailAddress { get; set; } = null!;

    public ConferenceLeagueYear ToDomain()
    {
        var leagueManager = new MinimalFantasyCriticUser(LeagueManager, LeagueManagerDisplayName, LeagueManagerEmailAddress);
        var league = new ConferenceLeague(LeagueID, LeagueName, leagueManager);
        return new ConferenceLeagueYear(league, Year, ConferenceLocked, DraftStarted, DraftFinished);
    }
}
