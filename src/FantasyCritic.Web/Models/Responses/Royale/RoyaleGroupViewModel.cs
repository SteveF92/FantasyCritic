using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Web.Models.Responses.Royale;

public class RoyaleGroupViewModel
{
    public RoyaleGroupViewModel(RoyaleGroup group, int memberCount)
    {
        GroupID = group.GroupID;
        GroupName = group.GroupName;
        ManagerUserID = group.Manager?.UserID;
        ManagerDisplayName = group.Manager?.DisplayName;
        GroupType = group.GroupType.ToString();
        LeagueID = group.LeagueID;
        ConferenceID = group.ConferenceID;
        RuleSetType = group.RuleSetType;
        MemberCount = memberCount;
    }

    public Guid GroupID { get; }
    public string GroupName { get; }
    public Guid? ManagerUserID { get; }
    public string? ManagerDisplayName { get; }
    public string GroupType { get; }
    public Guid? LeagueID { get; }
    public Guid? ConferenceID { get; }
    public string? RuleSetType { get; }
    public int MemberCount { get; }
}
