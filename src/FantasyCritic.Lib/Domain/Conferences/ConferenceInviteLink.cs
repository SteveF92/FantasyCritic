namespace FantasyCritic.Lib.Domain.Conferences;
public record ConferenceInviteLink(Guid InviteID, Conference Conference, Guid InviteCode, bool Active);
