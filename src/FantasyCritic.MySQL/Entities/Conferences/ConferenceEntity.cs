using FantasyCritic.Lib;
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

    public string ConferenceManagerDisplayName { get; set; } = null!;
    public string ConferenceManagerEmailAddress { get; set; } = null!;

    public Conference ToDomain(IEnumerable<MinimalConferenceYearInfo> years, IEnumerable<Guid> leaguesInConference)
    {
        var conferenceManager = new MinimalFantasyCriticUser(ConferenceManager, ConferenceManagerDisplayName, ConferenceManagerEmailAddress);
        return new Conference(ConferenceID, ConferenceName, conferenceManager, years, CustomRulesConference, PrimaryLeagueID, leaguesInConference);
    }
}
    
