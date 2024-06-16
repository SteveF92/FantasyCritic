using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain;

public record LeagueInvite(Guid InviteID, Guid LeagueID, string EmailAddress, MinimalFantasyCriticUser? InviteUser) : ILeagueInvite;

public record CompleteLeagueInvite(Guid InviteID, Guid LeagueID, string LeagueName, string EmailAddress,
    MinimalFantasyCriticUser? InviteUser, int ActiveYear, VeryMinimalFantasyCriticUser LeagueManager) : ILeagueInvite
{
    public string InviteName => InviteUser?.DisplayName ?? EmailAddress;
    public LeagueInvite ToMinimal() => new LeagueInvite(InviteID, LeagueID, EmailAddress, InviteUser);
}

public interface ILeagueInvite
{
    Guid InviteID { get; }
    Guid LeagueID { get; }
    string EmailAddress { get; }
    MinimalFantasyCriticUser? InviteUser { get; }
}
