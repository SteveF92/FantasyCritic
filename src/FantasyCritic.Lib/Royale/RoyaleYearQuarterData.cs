using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Royale;

public record RoyaleYearQuarterData(IReadOnlyList<RoyaleYearQuarter> AllYearQuarters, RoyaleYearQuarter ActiveYearQuarter, IReadOnlyList<RoyalePublisher> RoyalePublishers, Dictionary<Guid, List<RoyalePublisherStatistics>> TopPublisherStatistics)
{
    public RoyaleYearQuarter? GetPodiumYearQuarter()
    {
        if (ActiveYearQuarter.WinningUser is not null)
        {
            return ActiveYearQuarter;
        }

        if (ActiveYearQuarter.Finished)
        {
            return null;
        }

        var previousYearQuarter = AllYearQuarters.SingleOrDefault(x => x.YearQuarter.Equals(ActiveYearQuarter.YearQuarter.LastQuarter));
        if (previousYearQuarter?.WinningUser is null)
        {
            return null;
        }

        return previousYearQuarter;
    }
}

public record RoyalePublisherData(RoyalePublisher RoyalePublisher, IReadOnlyList<RoyaleAction> RoyaleActions, IReadOnlyList<RoyaleYearQuarter> QuartersWonByUser,
    IReadOnlyList<MasterGameYear> MasterGameYears, IReadOnlyList<MasterGameTag> MasterGameTags, IReadOnlyList<RoyalePublisherStatistics> Statistics);
public record RoyalePodiumEntry(Guid PublisherID, VeryMinimalFantasyCriticUser User, string PublisherName, string? PublisherIcon, decimal TotalFantasyPoints, int Ranking);
public record RoyalePublisherHistoryEntry(Guid PublisherID, int Year, int Quarter, string PublisherName, string? PublisherIcon, decimal TotalFantasyPoints, int? Ranking);
