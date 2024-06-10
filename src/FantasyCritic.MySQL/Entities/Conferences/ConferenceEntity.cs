using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.MySQL.Entities.Conferences;

internal class ConferenceEntity
{
    public ConferenceEntity()
    {

    }

    public ConferenceEntity(Conference domain)
    {
        ConferenceID = domain.ConferenceID;
        ConferenceName = domain.ConferenceName;
        ConferenceManager = domain.ConferenceManager.UserID;
        PrimaryLeagueID = domain.PrimaryLeagueID;
        CustomRulesConference = domain.CustomRulesConference;
    }

    public Guid ConferenceID { get; set; }
    public string ConferenceName { get; set; } = null!;
    public Guid ConferenceManager { get; set; }
    public Guid PrimaryLeagueID { get; set; }
    public bool CustomRulesConference { get; set; }

    public Conference ToDomain(MinimalFantasyCriticUser manager, IEnumerable<int> years, IEnumerable<Guid> leaguesInConference)
    {
        return new Conference(ConferenceID, ConferenceName, manager, years, CustomRulesConference, PrimaryLeagueID, leaguesInConference);
    }
}
    
