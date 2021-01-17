namespace FantasyCritic.Lib.Domain
{
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
            LinearRegressionHypeFactor = cachedMasterGameYear.LinearRegressionHypeFactor;
        }

        public MasterGameCalculatedStats(MasterGame masterGame, int year, double percentStandardGame, double percentCounterPick, double eligiblePercentStandardGame, 
            double? adjustedPercentCounterPick, int numberOfBids, int totalBidAmount, double bidPercentile, double? averageDraftPosition, double? averageWinningBid, 
            double hypeFactor, double dateAdjustedHypeFactor, double linearRegressionHypeFactor)
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
            LinearRegressionHypeFactor = linearRegressionHypeFactor;
        }

        public MasterGame MasterGame { get; }
        public int Year { get; }

        public double PercentStandardGame { get; set; }
        public double PercentCounterPick { get; set; }
        public double EligiblePercentStandardGame { get; set; }
        public double? AdjustedPercentCounterPick { get; set; }
        public int NumberOfBids { get; set; }
        public int TotalBidAmount { get; set; }
        public double BidPercentile { get; set; }
        public double? AverageDraftPosition { get; set; }
        public double? AverageWinningBid { get; set; }

        public double HypeFactor { get; }
        public double DateAdjustedHypeFactor { get; }
        public double LinearRegressionHypeFactor { get; }
        
    }
}
