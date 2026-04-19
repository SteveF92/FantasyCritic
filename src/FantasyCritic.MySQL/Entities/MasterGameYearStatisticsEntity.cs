namespace FantasyCritic.MySQL.Entities;

internal class MasterGameYearStatisticsEntity
{
    public int Year { get; }
    public Guid MasterGameID { get; }
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

    public MasterGameYearStatistics ToDomain()
    {
        return new MasterGameYearStatistics(Date, PercentStandardGame, PercentCounterPick, EligiblePercentStandardGame, AdjustedPercentCounterPick, NumberOfBids, TotalBidAmount,
            BidPercentile, AverageDraftPosition, AverageWinningBid, HypeFactor, DateAdjustedHypeFactor, PeakHypeFactor, LinearRegressionHypeFactor);
    }
}
