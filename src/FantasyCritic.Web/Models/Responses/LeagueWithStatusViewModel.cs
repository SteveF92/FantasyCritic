namespace FantasyCritic.Web.Models.Responses;

public class LeagueWithStatusViewModel
{
    public LeagueWithStatusViewModel(League league, bool isManager, bool userIsInLeague, bool userIsFollowingLeague, bool mostRecentYearOneShotMode)
    {
        LeagueID = league.LeagueID;
        LeagueName = league.LeagueName;
        LeagueManager = new PlayerViewModel(league, league.LeagueManager, false);
        ConferenceID = league.ConferenceID;
        ConferenceName = league.ConferenceName;
        IsManager = isManager;
        Archived = league.Archived;
        Years = league.Years;
        ActiveYear = Years.Max();
        PublicLeague = league.PublicLeague;
        TestLeague = league.TestLeague;
        CustomRulesLeague = league.CustomRulesLeague;
        UserIsInLeague = userIsInLeague;
        UserIsFollowingLeague = userIsFollowingLeague;
        OneShotMode = mostRecentYearOneShotMode;
    }

    public Guid LeagueID { get; }
    public string LeagueName { get; }
    public PlayerViewModel LeagueManager { get; }
    public Guid? ConferenceID { get; }
    public string? ConferenceName { get; }
    public bool IsManager { get; }
    public IReadOnlyList<int> Years { get; }
    public int ActiveYear { get; }
    public bool PublicLeague { get; }
    public bool TestLeague { get; }
    public bool CustomRulesLeague { get; }
    public bool Archived { get; }
    public bool UserIsInLeague { get; }
    public bool UserIsFollowingLeague { get; }
    public bool OneShotMode { get; }
}
