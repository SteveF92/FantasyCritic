namespace FantasyCritic.Web.Helpers;
public class LeagueUserRelationship
{
    public LeagueUserRelationship(Maybe<LeagueInvite> leagueInvite, bool inLeague, bool leagueManager, bool isAdmin)
    {
        LeagueInvite = leagueInvite;
        InLeague = inLeague;
        LeagueManager = leagueManager;
        IsAdmin = isAdmin;
    }

    public Maybe<LeagueInvite> LeagueInvite { get; }
    public bool InvitedToLeague => LeagueInvite.HasValue;
    public bool InLeague { get; }
    public bool InOrInvitedToLeague => InvitedToLeague || InLeague;
    public bool LeagueManager { get; }
    public bool IsAdmin { get; }
    public bool HasPermissionToViewLeague => InvitedToLeague || InLeague || IsAdmin;
}

public class LeagueYearUserRelationship
{
    public LeagueYearUserRelationship(bool invitedToLeague, bool inLeague, bool activeInYear, bool leagueManager, bool isAdmin)
    {
        InvitedToLeague = invitedToLeague;
        InLeague = inLeague;
        ActiveInYear = activeInYear;
        LeagueManager = leagueManager;
        IsAdmin = isAdmin;
    }

    public bool InvitedToLeague { get; }
    public bool InLeague { get; }
    public bool ActiveInYear { get; }
    public bool LeagueManager { get; }
    public bool IsAdmin { get; }
    public bool HasPermissionToViewLeague => InvitedToLeague || InLeague || IsAdmin;
}

public class PublisherUserRelationship
{
    public PublisherUserRelationship(LeagueYearUserRelationship leagueYearRelationship, bool isPublisher)
    {
        InvitedToLeague = leagueYearRelationship.InvitedToLeague;
        InLeague = leagueYearRelationship.InLeague;
        ActiveInYear = leagueYearRelationship.ActiveInYear;
        LeagueManager = leagueYearRelationship.LeagueManager;
        IsAdmin = leagueYearRelationship.IsAdmin;
        IsPublisher = isPublisher;
    }

    public bool InvitedToLeague { get; }
    public bool InLeague { get; }
    public bool ActiveInYear { get; }
    public bool LeagueManager { get; }
    public bool IsPublisher { get; }
    public bool IsAdmin { get; }
    public bool HasPermissionToViewLeague => InvitedToLeague || InLeague || IsAdmin;
}

public class RequiredRelationship
{
    public static RequiredRelationship AllowAnonymous => new RequiredRelationship(false, false, false, false, false, true);
    public static RequiredRelationship LoggedIn => new RequiredRelationship(true, false, false, false, false, true);
    public static RequiredRelationship ActiveInYear => new RequiredRelationship(true, true, true, false, false, false);
    public static RequiredRelationship BePublisher => new RequiredRelationship(true, true, true, false, true, false);
    public static RequiredRelationship LeagueManager => new RequiredRelationship(true, true, true, true, false, false);

    private RequiredRelationship(bool mustBeLoggedIn, bool mustBeInOrInvitedToLeague, bool mustBeActiveInYear, bool mustBeLeagueManager, bool mustBePublisher, bool allowIfAdmin)
    {
        MustBeLoggedIn = mustBeLoggedIn;
        MustBeInOrInvitedToLeague = mustBeInOrInvitedToLeague;
        MustBeActiveInYear = mustBeActiveInYear;
        MustBeLeagueManager = mustBeLeagueManager;
        MustBePublisher = mustBePublisher;
        AllowIfAdmin = allowIfAdmin;
    }

    public bool MustBeLoggedIn { get; }
    public bool MustBeInOrInvitedToLeague { get; }
    public bool MustBeActiveInYear { get; }
    public bool MustBeLeagueManager { get; }
    public bool MustBePublisher { get; }
    public bool AllowIfAdmin { get; }
};
