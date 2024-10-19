using FantasyCritic.Lib.Domain;

namespace FantasyCritic.MySQL.Entities;
internal class TopBidsAndDropsEntity
{
    public TopBidsAndDropsEntity()
    {

    }

    public TopBidsAndDropsEntity(TopBidsAndDropsGame domain)
    {
        Year = domain.MasterGameYear.Year;
        ProcessDate = domain.ProcessDate;
        MasterGameID = domain.MasterGameYear.MasterGame.MasterGameID;

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

    public int Year { get; set; }
    public LocalDate ProcessDate { get; set; }
    public Guid MasterGameID { get; set; }

    public int TotalStandardBidCount { get; set; }
    public int SuccessfulStandardBids { get; set; }
    public int FailedStandardBids { get; set; }
    public int TotalStandardBidLeagues { get; set; }
    public int TotalStandardBidAmount { get; set; }

    public int TotalCounterPickBidCount { get; set; }
    public int SuccessfulCounterPickBids { get; set; }
    public int FailedCounterPickBids { get; set; }
    public int TotalCounterPickBidLeagues { get; set; }
    public int TotalCounterPickBidAmount { get; set; }

    public int TotalDropCount { get; set; }
    public int SuccessfulDrops { get; set; }
    public int FailedDrops { get; set; }

    public TopBidsAndDropsEntity WithNewMasterGameID(Guid masterGameID)
    {
        return new TopBidsAndDropsEntity()
        {
            Year = Year,
            ProcessDate = ProcessDate,
            MasterGameID = masterGameID,
            TotalStandardBidCount = TotalStandardBidCount,
            SuccessfulStandardBids = SuccessfulStandardBids,
            FailedStandardBids = FailedStandardBids,
            TotalStandardBidLeagues = TotalStandardBidLeagues,
            TotalStandardBidAmount = TotalStandardBidAmount,
            TotalCounterPickBidCount = TotalCounterPickBidCount,
            SuccessfulCounterPickBids = SuccessfulCounterPickBids,
            FailedCounterPickBids = FailedCounterPickBids,
            TotalCounterPickBidLeagues = TotalCounterPickBidLeagues,
            TotalCounterPickBidAmount = TotalCounterPickBidAmount,
            TotalDropCount = TotalDropCount,
            SuccessfulDrops = SuccessfulDrops,
            FailedDrops = FailedDrops,
        };
    }

    public TopBidsAndDropsEntity MergeWith(TopBidsAndDropsEntity removeGameData)
    {
        return new TopBidsAndDropsEntity()
        {
            Year = Year,
            ProcessDate = ProcessDate,
            MasterGameID = MasterGameID,
            TotalStandardBidCount = TotalStandardBidCount + removeGameData.TotalStandardBidCount,
            SuccessfulStandardBids = SuccessfulStandardBids + removeGameData.SuccessfulStandardBids,
            FailedStandardBids = FailedStandardBids + removeGameData.FailedStandardBids,
            TotalStandardBidLeagues = TotalStandardBidLeagues + removeGameData.TotalStandardBidLeagues,
            TotalStandardBidAmount = TotalStandardBidAmount + removeGameData.TotalStandardBidAmount,
            TotalCounterPickBidCount = TotalCounterPickBidCount + removeGameData.TotalCounterPickBidCount,
            SuccessfulCounterPickBids = SuccessfulCounterPickBids + removeGameData.SuccessfulCounterPickBids,
            FailedCounterPickBids = FailedCounterPickBids + removeGameData.FailedCounterPickBids,
            TotalCounterPickBidLeagues = TotalCounterPickBidLeagues + removeGameData.TotalCounterPickBidLeagues,
            TotalCounterPickBidAmount = TotalCounterPickBidAmount + removeGameData.TotalCounterPickBidAmount,
            TotalDropCount = TotalDropCount + removeGameData.TotalDropCount,
            SuccessfulDrops = SuccessfulDrops + removeGameData.SuccessfulDrops,
            FailedDrops = FailedDrops + removeGameData.FailedDrops,
        };
    }

    public TopBidsAndDropsGame ToDomain(MasterGameYear masterGameYear)
    {
        return new TopBidsAndDropsGame
        {
            ProcessDate = ProcessDate,
            MasterGameYear = masterGameYear,
            TotalStandardBidCount = TotalStandardBidCount,
            SuccessfulStandardBids = SuccessfulStandardBids,
            FailedStandardBids = FailedStandardBids,
            TotalStandardBidLeagues = TotalStandardBidLeagues,
            TotalStandardBidAmount = TotalStandardBidAmount,
            TotalCounterPickBidCount = TotalCounterPickBidCount,
            SuccessfulCounterPickBids = SuccessfulCounterPickBids,
            FailedCounterPickBids = FailedCounterPickBids,
            TotalCounterPickBidLeagues = TotalCounterPickBidLeagues,
            TotalCounterPickBidAmount = TotalCounterPickBidAmount,
            TotalDropCount = TotalDropCount,
            SuccessfulDrops = SuccessfulDrops,
            FailedDrops = FailedDrops,
        };
    }
}
