using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Web.Models.Responses.Royale;

public class RoyaleGroupQuarterViewModel
{
    public RoyaleGroupQuarterViewModel(RoyaleGroup group, int year, int quarter, IReadOnlyList<RoyaleGroupMemberDisplayRow> memberRows,
        LocalDate currentDate, IEnumerable<MasterGameTag> allMasterGameTags)
    {
        GroupID = group.GroupID;
        GroupName = group.GroupName;
        ManagerUserID = group.Manager?.UserID;
        ManagerDisplayName = group.Manager?.DisplayName;
        GroupType = group.GroupType.ToString();
        LeagueID = group.LeagueID;
        ConferenceID = group.ConferenceID;
        RuleSetType = group.RuleSetType;
        Year = year;
        Quarter = quarter;

        int ranking = 1;
        var orderedRows = memberRows
            .OrderByDescending(r => r.Publisher?.GetTotalFantasyPoints() ?? -1m)
            .ToList();

        var members = new List<RoyaleGroupMemberRankedViewModel>();
        foreach (var row in orderedRows)
        {
            int? rank = null;
            if (row.Publisher is not null && row.Publisher.GetTotalFantasyPoints() > 0)
            {
                rank = ranking;
                ranking++;
            }
            members.Add(new RoyaleGroupMemberRankedViewModel(row, rank, currentDate, allMasterGameTags));
        }

        Members = members;
    }

    public Guid GroupID { get; }
    public string GroupName { get; }
    public Guid? ManagerUserID { get; }
    public string? ManagerDisplayName { get; }
    public string GroupType { get; }
    public Guid? LeagueID { get; }
    public Guid? ConferenceID { get; }
    public string? RuleSetType { get; }
    public int Year { get; }
    public int Quarter { get; }
    public IReadOnlyList<RoyaleGroupMemberRankedViewModel> Members { get; }
}

public class RoyaleGroupMemberRankedViewModel
{
    public RoyaleGroupMemberRankedViewModel(RoyaleGroupMemberDisplayRow row, int? ranking, LocalDate currentDate, IEnumerable<MasterGameTag> allMasterGameTags)
    {
        UserID = row.User.UserID;
        DisplayName = row.User.DisplayName;
        HasPublisher = row.Publisher is not null;
        PublisherID = row.Publisher?.PublisherID;
        PublisherName = row.Publisher?.PublisherName;
        TotalFantasyPoints = row.Publisher?.GetTotalFantasyPoints();
        Ranking = ranking;

        PublisherGames = (row.Publisher?.PublisherGames.Select(x => new RoyalePublisherGameViewModel(x, row.Publisher.YearQuarter, currentDate, allMasterGameTags, false)).ToList()) ?? new List<RoyalePublisherGameViewModel>();
        Statistics = row.Statistics.Select(x => new RoyalePublisherStatisticsViewModel(x)).ToList();
    }

    public Guid UserID { get; }
    public string DisplayName { get; }
    public bool HasPublisher { get; }
    public Guid? PublisherID { get; }
    public string? PublisherName { get; }
    public decimal? TotalFantasyPoints { get; }
    public int? Ranking { get; }
    public IReadOnlyList<RoyalePublisherGameViewModel> PublisherGames { get; }
    public IReadOnlyList<RoyalePublisherStatisticsViewModel> Statistics { get; }
}
