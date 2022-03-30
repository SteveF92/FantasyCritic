namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public class CreateInviteRequest
{
    public CreateInviteRequest(Guid leagueID)
    {
        LeagueID = leagueID;
    }

    public Guid LeagueID { get; }

    public string? InviteEmail { get; init; }
    public string? InviteDisplayName { get; init; }
    public int? InviteDisplayNumber { get; init; }

    public bool IsDisplayNameInvite()
    {
        if (!string.IsNullOrWhiteSpace(InviteDisplayName) && InviteDisplayNumber != null)
        {
            return true;
        }

        return false;
    }
}
