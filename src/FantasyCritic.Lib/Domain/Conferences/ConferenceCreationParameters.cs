using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain.Conferences;
public class ConferenceCreationParameters
{
    public ConferenceCreationParameters(FantasyCriticUser manager, string conferenceName, string primaryLeagueName, bool customRulesConference, LeagueYearParameters leagueYearParameters)
    {
        Manager = manager;
        ConferenceName = conferenceName;
        PrimaryLeagueName = primaryLeagueName;
        CustomRulesConference = customRulesConference;
        LeagueYearParameters = leagueYearParameters;
    }

    public FantasyCriticUser Manager { get; }
    public string ConferenceName { get; }
    public string PrimaryLeagueName { get; }
    public bool CustomRulesConference { get; }
    public LeagueYearParameters LeagueYearParameters { get; }
}
