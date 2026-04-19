namespace FantasyCritic.Lib.Domain;

public record MasterGameYearStatistics(LocalDate Date, double PercentStandardGame, double PercentCounterPick, double EligiblePercentStandardGame,
    double AdjustedPercentCounterPick, int NumberOfBids, int TotalBidAmount, double BidPercentile, double AverageDraftPosition,
    double AverageWinningBid, double HypeFactor, double DateAdjustedHypeFactor, double PeakHypeFactor, double LinearRegressionHypeFactor);
