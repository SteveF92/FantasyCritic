using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Web.Models.Responses;

public class TopBidsAndDropsSetViewModel
{
    public TopBidsAndDropsSetViewModel(IReadOnlyList<TopBidsAndDropsGameViewModel> data, LocalDate processDate)
    {
        Data = data.GroupToDictionary(x => x.MasterGameYear.Year);
        YearWithMostData = Data.MaxBy(x => x.Value.Count).Key;
        ProcessDate = processDate;
    }

    public IReadOnlyDictionary<int, IReadOnlyList<TopBidsAndDropsGameViewModel>> Data { get; }
    public int YearWithMostData { get; }
    public LocalDate ProcessDate { get; }
}

public class TopBidsAndDropsGameViewModel
{
    public TopBidsAndDropsGameViewModel(TopBidsAndDropsGame domain, LocalDate currentDate)
    {
        ProcessDate = domain.ProcessDate;
        MasterGameYear = new MasterGameYearViewModel(domain.MasterGameYear, currentDate);

        TotalStandardBidCount = domain.TotalStandardBidCount;
        SuccessfulStandardBids = domain.SuccessfulStandardBids;
        FailedStandardBids = domain.FailedStandardBids;
        TotalStandardBidLeagues = domain.TotalStandardBidLeagues;
        TotalStandardBidAmount = domain.TotalStandardBidAmount;
        TotalCounterPickBidCount = domain.TotalCounterPickBidCount;
        SuccessfulCounterPickBids = domain.SuccessfulCounterPickBids;
        FailedCounterPickBids = domain.FailedCounterPickBids;
        TotalCounterPickBidLeagues = domain.TotalCounterPickBidLeagues;
        TotalCounterPickBidAmount = domain.TotalCounterPickBidAmount;
        TotalDropCount = domain.TotalDropCount;
        SuccessfulDrops = domain.SuccessfulDrops;
        FailedDrops = domain.FailedDrops;
    }

    public LocalDate ProcessDate { get; }
    public MasterGameYearViewModel MasterGameYear { get; }
    public int TotalStandardBidCount { get; }
    public int SuccessfulStandardBids { get; }
    public int FailedStandardBids { get; }
    public int TotalStandardBidLeagues { get; }
    public int TotalStandardBidAmount { get; }
    public int TotalCounterPickBidCount { get; }
    public int SuccessfulCounterPickBids { get; }
    public int FailedCounterPickBids { get; }
    public int TotalCounterPickBidLeagues { get; }
    public int TotalCounterPickBidAmount { get; }
    public int TotalDropCount { get; }
    public int SuccessfulDrops { get; }
    public int FailedDrops { get; }
}
