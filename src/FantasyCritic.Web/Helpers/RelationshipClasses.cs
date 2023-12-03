namespace FantasyCritic.Web.Helpers;
public class LeagueUserRelationship
{
    public LeagueUserRelationship(LeagueInvite? leagueInvite, bool inLeague, bool leagueManager, bool isAdmin)
    {
        LeagueInvite = leagueInvite;
        InLeague = inLeague;
        LeagueManager = leagueManager;
        IsAdmin = isAdmin;
    }

    public LeagueInvite? LeagueInvite { get; }
    public bool InvitedToLeague => LeagueInvite is not null;
    public bool InLeague { get; }
    public bool InOrInvitedToLeague => InvitedToLeague || InLeague;
    public bool LeagueManager { get; }
    public bool IsAdmin { get; }
    public bool HasPermissionToViewLeague => InvitedToLeague || InLeague || IsAdmin;
}

public class LeagueYearUserRelationship
{
    public LeagueYearUserRelationship(LeagueInvite? leagueInvite, bool inLeague, bool activeInYear, bool leagueManager, bool isAdmin)
    {
        LeagueInvite = leagueInvite;
        InLeague = inLeague;
        ActiveInYear = activeInYear;
        LeagueManager = leagueManager;
        IsAdmin = isAdmin;
    }

    public LeagueInvite? LeagueInvite { get; }
    public bool InvitedToLeague => LeagueInvite is not null;
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
    public static readonly RequiredRelationship AllowAnonymous = new RequiredRelationship(false, false, false, false, false);
    public static readonly RequiredRelationship LoggedIn = new RequiredRelationship(true, false, false, false, false);
    public static readonly RequiredRelationship InLeague = new RequiredRelationship(true, true, false, false, false);
    public static readonly RequiredRelationship ActiveInYear = new RequiredRelationship(true, true, true, false, false);
    public static readonly RequiredRelationship BePublisher = new RequiredRelationship(true, true, true, false, true);
    public static readonly RequiredRelationship LeagueManager = new RequiredRelationship(true, true, true, true, false);

    private RequiredRelationship(bool mustBeLoggedIn, bool mustBeInOrInvitedToLeague, bool mustBeActiveInYear, bool mustBeLeagueManager, bool mustBePublisher)
    {
        MustBeLoggedIn = mustBeLoggedIn;
        MustBeInOrInvitedToLeague = mustBeInOrInvitedToLeague;
        MustBeActiveInYear = mustBeActiveInYear;
        MustBeLeagueManager = mustBeLeagueManager;
        MustBePublisher = mustBePublisher;
    }

    public bool MustBeLoggedIn { get; }
    public bool MustBeInOrInvitedToLeague { get; }
    public bool MustBeActiveInYear { get; }
    public bool MustBeLeagueManager { get; }
    public bool MustBePublisher { get; }
}

public class ConferenceUserRelationship
{
    public ConferenceUserRelationship(bool inConference, bool conferenceManager, bool isAdmin)
    {
        InConference = inConference;
        ConferenceManager = conferenceManager;
        IsAdmin = isAdmin;
    }

    public bool InConference { get; }
    public bool ConferenceManager { get; }
    public bool IsAdmin { get; }
}

public class ConferenceRequiredRelationship
{
    public static readonly ConferenceRequiredRelationship AllowAnonymous = new ConferenceRequiredRelationship(false, false, false);
    public static readonly ConferenceRequiredRelationship LoggedIn = new ConferenceRequiredRelationship(true, false, false);
    public static readonly ConferenceRequiredRelationship InConference = new ConferenceRequiredRelationship(true, true, false);
    public static readonly ConferenceRequiredRelationship ConferenceManager = new ConferenceRequiredRelationship(true, true, true);

    private ConferenceRequiredRelationship(bool mustBeLoggedIn, bool mustBeInConference, bool mustBeConferenceManager)
    {
        MustBeLoggedIn = mustBeLoggedIn;
        MustBeInConference = mustBeInConference;
        MustBeConferenceManager = mustBeConferenceManager;
    }

    public bool MustBeLoggedIn { get; }
    public bool MustBeInConference { get; }
    public bool MustBeConferenceManager { get; }
}
