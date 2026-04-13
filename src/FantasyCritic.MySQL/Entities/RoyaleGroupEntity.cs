using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Royale;

namespace FantasyCritic.MySQL.Entities;

internal class RoyaleGroupEntity
{
    public RoyaleGroupEntity()
    {
    }

    public RoyaleGroupEntity(RoyaleGroup domain)
    {
        GroupID = domain.GroupID;
        GroupName = domain.GroupName;
        ManagerUserID = domain.Manager?.UserID;
        GroupType = domain.GroupType.Value;
        LeagueID = domain.LeagueID;
        ConferenceID = domain.ConferenceID;
        RuleSetType = domain.RuleSetType;
        CreatedTimestamp = domain.CreatedTimestamp.ToDateTimeUtc();
    }

    public Guid GroupID { get; set; }
    public string GroupName { get; set; } = null!;
    public Guid? ManagerUserID { get; set; }
    public string GroupType { get; set; } = null!;
    public Guid? LeagueID { get; set; }
    public Guid? ConferenceID { get; set; }
    public string? RuleSetType { get; set; }
    public DateTime CreatedTimestamp { get; set; }

    public string? ManagerDisplayName { get; set; }

    public RoyaleGroup ToDomain()
    {
        VeryMinimalFantasyCriticUser? manager = ManagerUserID.HasValue
            ? new VeryMinimalFantasyCriticUser(ManagerUserID.Value, ManagerDisplayName ?? "System")
            : null;
        var instant = Instant.FromDateTimeUtc(DateTime.SpecifyKind(CreatedTimestamp, DateTimeKind.Utc));
        return new RoyaleGroup(GroupID, GroupName, manager, RoyaleGroupType.FromValue(GroupType), LeagueID, ConferenceID, RuleSetType, instant);
    }
}
