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
        LeagueManager = league.LeagueManager.Id;
        ConferenceID = league.ConferenceID;
        ConferenceName = league.ConferenceName;
        PublicLeague = league.PublicLeague;
        TestLeague = league.TestLeague;
        CustomRulesLeague = league.CustomRulesLeague;
        NumberOfFollowers = league.NumberOfFollowers;
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

    public League ToDomain(FantasyCriticUser manager, IEnumerable<int> years)
    {
        League parameters = new League(LeagueID, LeagueName, manager, ConferenceID, ConferenceName, years, PublicLeague, TestLeague, CustomRulesLeague, Archived, NumberOfFollowers);
        return parameters;
    }
}
