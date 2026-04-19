namespace FantasyCritic.Web.Models.Responses;

public class MasterGameYearStatisticsViewModel
{
    public MasterGameYearStatisticsViewModel(MasterGameYearStatistics domain, int year)
    {
        Date = domain.Date;
        PercentStandardGame = domain.PercentStandardGame;
        PercentCounterPick = domain.PercentCounterPick;
        EligiblePercentStandardGame = domain.EligiblePercentStandardGame;
        AdjustedPercentCounterPick = domain.AdjustedPercentCounterPick;
        NumberOfBids = domain.NumberOfBids;
        TotalBidAmount = domain.TotalBidAmount;
        BidPercentile = domain.BidPercentile;
        AverageDraftPosition = domain.AverageDraftPosition;
        AverageWinningBid = domain.AverageWinningBid;
        HypeFactor = domain.HypeFactor;
        DateAdjustedHypeFactor = domain.DateAdjustedHypeFactor;
        PeakHypeFactor = domain.PeakHypeFactor;
        LinearRegressionHypeFactor = domain.LinearRegressionHypeFactor;
        RoyaleCost = MasterGameYear.GetRoyaleGameCostFromValues(year, LinearRegressionHypeFactor);
    }

    public LocalDate Date { get; }
    public double PercentStandardGame { get; }
    public double PercentCounterPick { get; }
    public double EligiblePercentStandardGame { get; }
    public double AdjustedPercentCounterPick { get; }
    public int NumberOfBids { get; }
    public int TotalBidAmount { get; }
    public double BidPercentile { get; }
    public double AverageDraftPosition { get; }
    public double AverageWinningBid { get; }
    public double HypeFactor { get; }
    public double DateAdjustedHypeFactor { get; }
    public double PeakHypeFactor { get; }
    public double LinearRegressionHypeFactor { get; }
    public decimal RoyaleCost { get; }
}
