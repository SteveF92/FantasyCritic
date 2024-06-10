using FantasyCritic.Lib.Identity;

namespace FantasyCritic.MySQL.Entities;
internal class LeagueWithManagerEntity
{
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
    public bool MostRecentYearOneShot { get; set; }

    //Manager Fields
    public string DisplayName { get; set; } = null!;

    public League ToDomain(IEnumerable<int> years)
    {
        var manager = ToManagerDomain();
        League parameters = new League(LeagueID, LeagueName, manager, ConferenceID, ConferenceName, years, PublicLeague, TestLeague, CustomRulesLeague, Archived, NumberOfFollowers);
        return parameters;
    }

    private FantasyCriticUser ToManagerDomain()
    {
        var generalSettings = new GeneralUserSettings(false);
        FantasyCriticUser domain = new FantasyCriticUser(LeagueManager, DisplayName, null, 0, "", "", false, null, null, false, null, Instant.MinValue, generalSettings, false);
        return domain;
    }
}
