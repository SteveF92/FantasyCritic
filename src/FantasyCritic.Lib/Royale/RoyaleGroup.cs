using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Royale;

public class RoyaleGroup
{
    public RoyaleGroup(Guid groupID, string groupName, VeryMinimalFantasyCriticUser? manager,
        RoyaleGroupType groupType, Guid? leagueID, string? ruleSetType, Instant createdTimestamp)
    {
        GroupID = groupID;
        GroupName = groupName;
        Manager = manager;
        GroupType = groupType;
        LeagueID = leagueID;
        RuleSetType = ruleSetType;
        CreatedTimestamp = createdTimestamp;
    }

    public Guid GroupID { get; }
    public string GroupName { get; }
    public VeryMinimalFantasyCriticUser? Manager { get; }
    public RoyaleGroupType GroupType { get; }
    public Guid? LeagueID { get; }
    public string? RuleSetType { get; }
    public Instant CreatedTimestamp { get; }
}
