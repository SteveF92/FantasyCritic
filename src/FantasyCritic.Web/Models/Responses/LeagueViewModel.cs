using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Models.Responses;

public class LeagueViewModel
{
    public LeagueViewModel(League league, bool isManager, bool userIsInLeague, bool userIsFollowingLeague)
    {
        LeagueID = league.LeagueID;
        LeagueName = league.LeagueName;
        LeagueManager = new PlayerViewModel(league.LeagueID, league.LeagueName, league.LeagueManager, false);
        ConferenceID = league.ConferenceID;
        ConferenceName = league.ConferenceName;
        IsManager = isManager;
        Archived = league.Archived;
        Years = league.Years.Select(x => x.Year).ToList();
        ActiveYear = Years.Max();
        PublicLeague = league.PublicLeague;
        TestLeague = league.TestLeague;
        UserIsInLeague = userIsInLeague;
        UserIsFollowingLeague = userIsFollowingLeague;
        NumberOfFollowers = league.NumberOfFollowers;
    }

    public LeagueViewModel(League league, bool isManager, IEnumerable<FantasyCriticUserRemovable> players, LeagueInvite? outstandingInvite,
        FantasyCriticUser? currentUser, bool userIsInLeague, bool userIsFollowingLeague)
    {
        LeagueID = league.LeagueID;
        LeagueName = league.LeagueName;
        LeagueManager = new PlayerViewModel(league.LeagueID, league.LeagueName, league.LeagueManager, false);
        ConferenceID = league.ConferenceID;
        ConferenceName = league.ConferenceName;
        IsManager = isManager;
        Archived = league.Archived;
        Years = league.Years.Select(x => x.Year).ToList();
        ActiveYear = Years.Max();

        if (outstandingInvite is not null && currentUser is not null)
        {
            OutstandingInvite = new LeagueInviteViewModel(outstandingInvite);
        }

        Players = players.Select(x => new PlayerViewModel(league.LeagueID, league.LeagueName, x.User, x.Removable)).ToList();
        PublicLeague = league.PublicLeague;
        TestLeague = league.TestLeague;
        CustomRulesLeague = league.CustomRulesLeague;
        UserIsInLeague = userIsInLeague;
        UserIsFollowingLeague = userIsFollowingLeague;
        NumberOfFollowers = league.NumberOfFollowers;
    }

    public Guid LeagueID { get; }
    public string LeagueName { get; }
    public PlayerViewModel LeagueManager { get; }
    public Guid? ConferenceID { get; }
    public string? ConferenceName { get; }
    public bool IsManager { get; }
    public IReadOnlyList<PlayerViewModel>? Players { get; }
    public LeagueInviteViewModel? OutstandingInvite { get; }
    public IReadOnlyList<int> Years { get; }
    public int ActiveYear { get; }
    public bool PublicLeague { get; }
    public bool TestLeague { get; }
    public bool CustomRulesLeague { get; }
    public bool Archived { get; }
    public bool UserIsInLeague { get; }
    public bool UserIsFollowingLeague { get; }
    public int NumberOfFollowers { get; }
}
