using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Models.Responses;

public class PlayerWithPublisherViewModel
{
    public PlayerWithPublisherViewModel(Guid inviteID, string inviteName)
    {
        InviteID = inviteID;
        InviteName = inviteName;
    }

    public PlayerWithPublisherViewModel(LeagueYear leagueYear, FantasyCriticUser user, bool removable)
    {
        User = new PlayerViewModel(leagueYear.League, user, removable);
    }

    public PlayerWithPublisherViewModel(LeagueYear leagueYear, FantasyCriticUser user, Publisher publisher, LocalDate currentDate,
        SystemWideValues systemWideValues, bool userIsInLeague, bool userIsInvitedToLeague,
        bool removable, bool previousYearWinner)
    {
        User = new PlayerViewModel(leagueYear.League, user, removable);
        Publisher = new MinimalPublisherViewModel(leagueYear, publisher, currentDate, userIsInLeague, userIsInvitedToLeague, systemWideValues);
        TotalFantasyPoints = publisher.GetTotalFantasyPoints(leagueYear.SupportedYear, leagueYear.Options);

        var ineligiblePointsShouldCount = !SupportedYear.Year2022FeatureSupported(leagueYear.Year);
        SimpleProjectedFantasyPoints = publisher.GetProjectedFantasyPoints(systemWideValues, true, currentDate, ineligiblePointsShouldCount, leagueYear);
        AdvancedProjectedFantasyPoints = publisher.GetProjectedFantasyPoints(systemWideValues, false, currentDate, ineligiblePointsShouldCount, leagueYear);
        PreviousYearWinner = previousYearWinner;
    }

    public Guid? InviteID { get; }
    public string InviteName { get; }
    public PlayerViewModel User { get; }
    public MinimalPublisherViewModel Publisher { get; }
    public decimal TotalFantasyPoints { get; }
    public decimal SimpleProjectedFantasyPoints { get; }
    public decimal AdvancedProjectedFantasyPoints { get; }
    public bool PreviousYearWinner { get; }
}
