using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Models.Responses;

public class LeagueViewModel
{
    public LeagueViewModel(League league, bool isManager, bool userIsInLeague, bool userIsFollowingLeague)
    {
        LeagueID = league.LeagueID;
        LeagueName = league.LeagueName;
        LeagueManager = new PlayerViewModel(league, league.LeagueManager, false);
        IsManager = isManager;
        Archived = league.Archived;
        Years = league.Years;
        ActiveYear = Years.Max();
        PublicLeague = league.PublicLeague;
        TestLeague = league.TestLeague;
        UserIsInLeague = userIsInLeague;
        UserIsFollowingLeague = userIsFollowingLeague;
        NumberOfFollowers = league.NumberOfFollowers;
    }

    public LeagueViewModel(League league, bool isManager, IEnumerable<FantasyCriticUserRemovable> players, Maybe<LeagueInvite> outstandingInvite,
        Maybe<FantasyCriticUser> currentUser, bool userIsInLeague, bool userIsFollowingLeague)
    {
        LeagueID = league.LeagueID;
        LeagueName = league.LeagueName;
        LeagueManager = new PlayerViewModel(league, league.LeagueManager, false);
        IsManager = isManager;
        Archived = league.Archived;
        Years = league.Years;
        ActiveYear = Years.Max();

        if (outstandingInvite.HasValueTempoTemp && currentUser.HasValueTempoTemp)
        {
            OutstandingInvite = LeagueInviteViewModel.CreateWithDisplayName(outstandingInvite.ValueTempoTemp, currentUser.ValueTempoTemp);
        }

        Players = players.Select(x => new PlayerViewModel(league, x.User, x.Removable)).ToList();
        PublicLeague = league.PublicLeague;
        TestLeague = league.TestLeague;
        UserIsInLeague = userIsInLeague;
        UserIsFollowingLeague = userIsFollowingLeague;
        NumberOfFollowers = league.NumberOfFollowers;
    }

    public Guid LeagueID { get; }
    public string LeagueName { get; }
    public PlayerViewModel LeagueManager { get; }
    public bool IsManager { get; }
    public IReadOnlyList<PlayerViewModel> Players { get; }
    public LeagueInviteViewModel OutstandingInvite { get; }
    public IReadOnlyList<int> Years { get; }
    public int ActiveYear { get; }
    public bool PublicLeague { get; }
    public bool TestLeague { get; }
    public bool Archived { get; }
    public bool UserIsInLeague { get; }
    public bool UserIsFollowingLeague { get; }
    public int NumberOfFollowers { get; }
}
