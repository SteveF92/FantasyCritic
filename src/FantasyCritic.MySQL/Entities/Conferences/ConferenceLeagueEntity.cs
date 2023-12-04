using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.MySQL.Entities.Conferences;
internal class ConferenceLeagueEntity
{
    public Guid LeagueID { get; set; }
    public string LeagueName { get; set; } = null!;
    public Guid LeagueManager { get; set; }

    public ConferenceLeague ToDomain(FantasyCriticUser leagueManager)
    {
        return new ConferenceLeague(LeagueID, LeagueName, leagueManager);
    }
}
