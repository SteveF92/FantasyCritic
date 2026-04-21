namespace FantasyCritic.Web.Models.Responses.Royale;

public class RoyaleGroupDataViewModel
{
    public RoyaleGroupDataViewModel(RoyaleGroupViewModel group, IReadOnlyList<RoyaleGroupMemberListItemViewModel> members, RoyaleYearQuarterViewModel activeQuarter)
    {
        Group = group;
        Members = members;
        ActiveQuarter = activeQuarter;
    }

    public RoyaleGroupViewModel Group { get; }
    public IReadOnlyList<RoyaleGroupMemberListItemViewModel> Members { get; }
    public RoyaleYearQuarterViewModel ActiveQuarter { get; }
}
