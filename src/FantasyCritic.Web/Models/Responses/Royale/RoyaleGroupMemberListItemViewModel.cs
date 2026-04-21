using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Web.Models.Responses.Royale;

public class RoyaleGroupMemberListItemViewModel
{
    public RoyaleGroupMemberListItemViewModel(RoyaleGroupMemberWithLifetimeStats stats)
    {
        UserID = stats.User.UserID;
        DisplayName = stats.User.DisplayName;
        QuartersParticipated = stats.QuartersParticipated;
        TotalPoints = stats.TotalPoints;
        AverageRankWithinGroup = stats.AverageRankWithinGroup;
        AverageRankOverall = stats.AverageRankOverall;
    }

    public Guid UserID { get; }
    public string DisplayName { get; }
    public int QuartersParticipated { get; }
    public decimal TotalPoints { get; }
    public double? AverageRankWithinGroup { get; }
    public double? AverageRankOverall { get; }
}
