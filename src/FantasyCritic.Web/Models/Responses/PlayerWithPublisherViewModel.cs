using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Models.Responses;

public class PlayerWithPublisherViewModel
{
    public PlayerWithPublisherViewModel(Guid inviteID, string inviteName)
    {
        InviteID = inviteID;
        InviteName = inviteName;
    }

    public PlayerWithPublisherViewModel(LeagueYear leagueYear, MinimalFantasyCriticUser user)
    {
        User = new PlayerViewModel(leagueYear.League.LeagueID, leagueYear.League.LeagueName, user);
    }

    public PlayerWithPublisherViewModel(LeagueYear leagueYear, MinimalFantasyCriticUser? user, Publisher publisher, LocalDate currentDate,
        SystemWideValues systemWideValues, bool userIsInLeague, bool userIsInvitedToLeague,
        bool previousYearWinner, int ranking, int projectedRanking)
    {
        if (user is not null)
        {
            User = new PlayerViewModel(leagueYear.League.LeagueID, leagueYear.League.LeagueName, user);
        }

        Publisher = new PlayerPublisherViewModel(leagueYear, publisher, currentDate, userIsInLeague, userIsInvitedToLeague, systemWideValues);
        TotalFantasyPoints = publisher.GetTotalFantasyPoints(leagueYear.SupportedYear, leagueYear.Options);
        ProjectedFantasyPoints = publisher.GetProjectedFantasyPoints(leagueYear, systemWideValues);
        PreviousYearWinner = previousYearWinner;
        Ranking = ranking;
        ProjectedRanking = projectedRanking;
        DraftPosition = publisher.GetDraftPosition(leagueYear.DraftForPublisherDisplayOrder.DraftID);
    }

    public Guid? InviteID { get; }
    public string? InviteName { get; }
    public PlayerViewModel? User { get; }
    public PlayerPublisherViewModel? Publisher { get; }
    public decimal? TotalFantasyPoints { get; }
    public decimal? ProjectedFantasyPoints { get; }
    public bool? PreviousYearWinner { get; }
    public int Ranking { get; }
    public int ProjectedRanking { get; }
    public int DraftPosition { get; }
}
