using FantasyCritic.Lib.Identity;

namespace FantasyCritic.MySQL.Entities;

internal class LeagueEntity
{
    public LeagueEntity()
    {

    }

    public LeagueEntity(League league)
    {
        LeagueID = league.LeagueID;
        LeagueName = league.LeagueName;
        LeagueManager = league.LeagueManager.UserID;
        ConferenceID = league.ConferenceID;
        ConferenceName = league.ConferenceName;
        PublicLeague = league.PublicLeague;
        TestLeague = league.TestLeague;
        CustomRulesLeague = league.CustomRulesLeague;
        NumberOfFollowers = league.NumberOfFollowers;

        ManagerDisplayName = league.LeagueManager.DisplayName;
        ManagerEmailAddress = league.LeagueManager.EmailAddress;
    }

    public Guid LeagueID { get; set; }
    public string LeagueName { get; set; } = null!;
    public Guid LeagueManager { get; set; }
    public Guid? ConferenceID { get; set; }
    public string? ConferenceName { get; set; }
    public bool PublicLeague { get; set; }
    public bool TestLeague { get; set; }
    public bool CustomRulesLeague { get; set; }
    public int NumberOfFollowers { get; set; }
    public bool Archived { get; set; }

    public bool UserIsInLeague { get; set; }
    public bool UserIsFollowingLeague { get; set; }
    public bool UserIsActiveInMostRecentYearForLeague { get; set; }
    public bool LeagueIsActiveInActiveYear { get; set; }

    public string ManagerDisplayName { get; set; } = null!;
    public string ManagerEmailAddress { get; set; } = null!;

    public bool MostRecentYearOneShot { get; set; }
    
    public League ToDomain(IEnumerable<MinimalLeagueYearInfo> years)
    {
        var minimalManager = new MinimalFantasyCriticUser(LeagueManager, ManagerDisplayName, ManagerEmailAddress);
        League parameters = new League(LeagueID, LeagueName, minimalManager, ConferenceID, ConferenceName, years, PublicLeague, TestLeague, CustomRulesLeague, Archived, NumberOfFollowers);
        return parameters;
    }
}
