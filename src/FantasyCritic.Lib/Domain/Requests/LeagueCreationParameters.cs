using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain.Requests;

public class LeagueCreationParameters
{
    public LeagueCreationParameters(FantasyCriticUser manager, string leagueName, bool publicLeague, bool testLeague, LeagueYearParameters leagueYearParameters)
    {
        Manager = manager;
        LeagueName = leagueName;
        PublicLeague = publicLeague;
        TestLeague = testLeague;
        LeagueYearParameters = leagueYearParameters;
    }

    public FantasyCriticUser Manager { get; }
    public string LeagueName { get; }
    public bool PublicLeague { get; }
    public bool TestLeague { get; }
    public LeagueYearParameters LeagueYearParameters { get; }
}
