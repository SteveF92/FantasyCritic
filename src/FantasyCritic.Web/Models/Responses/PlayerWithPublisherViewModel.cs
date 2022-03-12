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
        SupportedYear supportedYear, bool removable, bool previousYearWinner, IReadOnlySet<Guid> counterPickedPublisherGameIDs)
    {
        User = new PlayerViewModel(leagueYear.League, user, removable);
        Publisher = new PublisherViewModel(publisher, currentDate, userIsInLeague, userIsInvitedToLeague, systemWideValues, supportedYear.Finished, counterPickedPublisherGameIDs);
        TotalFantasyPoints = publisher.TotalFantasyPoints;

        var ineligiblePointsShouldCount = !SupportedYear.Year2022FeatureSupported(leagueYear.Year);
        SimpleProjectedFantasyPoints = publisher.GetProjectedFantasyPoints(systemWideValues, true, currentDate, ineligiblePointsShouldCount);
        AdvancedProjectedFantasyPoints = publisher.GetProjectedFantasyPoints(systemWideValues, false, currentDate, ineligiblePointsShouldCount);
        PreviousYearWinner = previousYearWinner;
    }

    public Guid InviteID { get; }
    public string InviteName { get; }
    public PlayerViewModel User { get; }
    public PublisherViewModel Publisher { get; }
    public decimal TotalFantasyPoints { get; }
    public decimal SimpleProjectedFantasyPoints { get; }
    public decimal AdvancedProjectedFantasyPoints { get; }
    public bool PreviousYearWinner { get; }
}
