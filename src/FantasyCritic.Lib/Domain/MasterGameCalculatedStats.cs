namespace FantasyCritic.Lib.Domain;

public class MasterGameCalculatedStats
{
    public MasterGameCalculatedStats(MasterGame masterGame, MasterGameYear cachedMasterGameYear)
    {
        MasterGame = masterGame;
        Year = cachedMasterGameYear.Year;

        PercentStandardGame = cachedMasterGameYear.PercentStandardGame;
        PercentCounterPick = cachedMasterGameYear.PercentCounterPick;
        EligiblePercentStandardGame = cachedMasterGameYear.EligiblePercentStandardGame;
        AdjustedPercentCounterPick = cachedMasterGameYear.AdjustedPercentCounterPick;
        NumberOfBids = cachedMasterGameYear.NumberOfBids;
        TotalBidAmount = cachedMasterGameYear.TotalBidAmount;
        BidPercentile = cachedMasterGameYear.BidPercentile;
        AverageDraftPosition = cachedMasterGameYear.AverageDraftPosition;
        AverageWinningBid = cachedMasterGameYear.AverageWinningBid;

        HypeFactor = cachedMasterGameYear.HypeFactor;
        DateAdjustedHypeFactor = cachedMasterGameYear.DateAdjustedHypeFactor;
        PeakHypeFactor = cachedMasterGameYear.PeakHypeFactor;
        LinearRegressionHypeFactor = cachedMasterGameYear.LinearRegressionHypeFactor;
    }

    public MasterGameCalculatedStats(MasterGame masterGame, int year, double percentStandardGame, double percentCounterPick, double eligiblePercentStandardGame,
        double? adjustedPercentCounterPick, int numberOfBids, int totalBidAmount, double bidPercentile, double? averageDraftPosition, double? averageWinningBid,
        double hypeFactor, double dateAdjustedHypeFactor, double peakHypeFactor, double linearRegressionHypeFactor)
    {
        MasterGame = masterGame;
        Year = year;

        PercentStandardGame = percentStandardGame;
        PercentCounterPick = percentCounterPick;
        EligiblePercentStandardGame = eligiblePercentStandardGame;
        AdjustedPercentCounterPick = adjustedPercentCounterPick;
        NumberOfBids = numberOfBids;
        TotalBidAmount = totalBidAmount;
        BidPercentile = bidPercentile;
        AverageDraftPosition = averageDraftPosition;
        AverageWinningBid = averageWinningBid;

        HypeFactor = hypeFactor;
        DateAdjustedHypeFactor = dateAdjustedHypeFactor;
        PeakHypeFactor = peakHypeFactor;
        LinearRegressionHypeFactor = linearRegressionHypeFactor;
    }

    public MasterGame MasterGame { get; }
    public int Year { get; }

    public double PercentStandardGame { get; }
    public double PercentCounterPick { get; }
    public double EligiblePercentStandardGame { get; }
    public double? AdjustedPercentCounterPick { get; }
    public int NumberOfBids { get; }
    public int TotalBidAmount { get; }
    public double BidPercentile { get; }
    public double? AverageDraftPosition { get; }
    public double? AverageWinningBid { get; }

    public double HypeFactor { get; }
    public double DateAdjustedHypeFactor { get; }
    public double PeakHypeFactor { get; }
    public double LinearRegressionHypeFactor { get; }

}
