using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Models.Responses;

public class PlayerWithPublisherViewModel
{
    public PlayerWithPublisherViewModel(Guid inviteID, string inviteName)
    {
        InviteID = inviteID;
        InviteName = inviteName;
    }

    public PlayerWithPublisherViewModel(LeagueYear leagueYear, MinimalFantasyCriticUser user, bool removable)
    {
        User = new PlayerViewModel(leagueYear.League, user, removable);
    }

    public PlayerWithPublisherViewModel(LeagueYear leagueYear, MinimalFantasyCriticUser user, Publisher publisher, LocalDate currentDate,
        SystemWideValues systemWideValues, bool userIsInLeague, bool userIsInvitedToLeague,
        bool removable, bool previousYearWinner, int ranking, int projectedRanking)
    {
        User = new PlayerViewModel(leagueYear.League, user, removable);
        Publisher = new MinimalPublisherViewModel(leagueYear, publisher, currentDate, userIsInLeague, userIsInvitedToLeague, systemWideValues);
        TotalFantasyPoints = publisher.GetTotalFantasyPoints(leagueYear.SupportedYear, leagueYear.Options);
        ProjectedFantasyPoints = publisher.GetProjectedFantasyPoints(leagueYear, systemWideValues, currentDate);
        PreviousYearWinner = previousYearWinner;
        Ranking = ranking;
        ProjectedRanking = projectedRanking;
        DraftPosition = publisher.DraftPosition;
    }

    public Guid? InviteID { get; }
    public string? InviteName { get; }
    public PlayerViewModel? User { get; }
    public MinimalPublisherViewModel? Publisher { get; }
    public decimal? TotalFantasyPoints { get; }
    public decimal? ProjectedFantasyPoints { get; }
    public bool? PreviousYearWinner { get; }
    public int Ranking { get; }
    public int ProjectedRanking { get; }
    public int DraftPosition { get; }
}
