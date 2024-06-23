using FantasyCritic.Lib.Identity;

namespace FantasyCritic.MySQL.Entities;

internal class LeagueInviteEntity
{
    public LeagueInviteEntity()
    {

    }

    public LeagueInviteEntity(LeagueInvite domain)
    {
        InviteID = domain.InviteID;
        LeagueID = domain.LeagueID;

        EmailAddress = domain.EmailAddress;

        UserID = domain.InviteUser?.UserID;
    }

    public Guid InviteID { get; set; }
    public Guid LeagueID { get; set; }
    public string EmailAddress { get; set; } = null!;
    public Guid? UserID { get; set; }

    public string? InviteUserName { get; set; }
    public string? InviteUserEmailAddress { get; set; }

    public LeagueInvite ToDomain()
    {
        MinimalFantasyCriticUser? inviteUser = null;
        if (UserID.HasValue)
        {
            inviteUser = new MinimalFantasyCriticUser(UserID.Value, InviteUserName!, InviteUserEmailAddress!);
        }

        return new LeagueInvite(InviteID, LeagueID, EmailAddress, inviteUser);
    }
}

internal class CompleteLeagueInviteEntity
{
    public Guid InviteID { get; set; }
    public Guid LeagueID { get; set; }
    public string EmailAddress { get; set; } = null!;
    public Guid? UserID { get; set; }

    public string LeagueName { get; set; } = null!;
    public int ActiveYear { get; set; }

    public string? InviteUserName { get; set; }
    public string? InviteUserEmailAddress { get; set; }

    public Guid ManagerUserID { get; set; }
    public string ManagerUserName { get; set; } = null!;

    public CompleteLeagueInvite ToDomain()
    {
        MinimalFantasyCriticUser? inviteUser = null;
        if (UserID.HasValue)
        {
            inviteUser = new MinimalFantasyCriticUser(UserID.Value, InviteUserName!, InviteUserEmailAddress!);
        }

        var leagueManager = new VeryMinimalFantasyCriticUser(ManagerUserID, ManagerUserName);
        return new CompleteLeagueInvite(InviteID, LeagueID, LeagueName, EmailAddress, inviteUser, ActiveYear, leagueManager);
    }
}
