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

    public string? UserName { get; set; }
    public string? UserEmailAddress { get; set; }

    public LeagueInvite ToDomain()
    {
        MinimalFantasyCriticUser? inviteUser = null;
        if (UserID.HasValue)
        {
            inviteUser = new MinimalFantasyCriticUser(UserID.Value, UserName!, UserEmailAddress!);
        }

        return new LeagueInvite(InviteID, LeagueID, EmailAddress, inviteUser);
    }
}
