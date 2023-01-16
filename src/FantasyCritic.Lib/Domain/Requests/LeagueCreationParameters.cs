using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain.Requests;

public class LeagueCreationParameters
{
    public LeagueCreationParameters(FantasyCriticUser manager, string leagueName, bool publicLeague, bool testLeague, bool customRulesLeague, LeagueYearParameters leagueYearParameters)
    {
        Manager = manager;
        LeagueName = leagueName;
        PublicLeague = publicLeague;
        TestLeague = testLeague;
        CustomRulesLeague = customRulesLeague;
        LeagueYearParameters = leagueYearParameters;
    }

    public FantasyCriticUser Manager { get; }
    public string LeagueName { get; }
    public bool PublicLeague { get; }
    public bool TestLeague { get; }
    public bool CustomRulesLeague { get; }
    public LeagueYearParameters LeagueYearParameters { get; }
}
