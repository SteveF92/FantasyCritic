namespace FantasyCritic.Lib.Domain
{
    public class MasterGameCalculatedStats
    {
        public MasterGameCalculatedStats(MasterGameYear masterGameYear)
        {
            MasterGame = masterGameYear.MasterGame;
            Year = masterGameYear.Year;

            PercentStandardGame = masterGameYear.PercentStandardGame;
            PercentCounterPick = masterGameYear.PercentCounterPick;
            EligiblePercentStandardGame = masterGameYear.EligiblePercentStandardGame;
            EligiblePercentCounterPick = masterGameYear.EligiblePercentCounterPick;
            NumberOfBids = masterGameYear.NumberOfBids;
            TotalBidAmount = masterGameYear.TotalBidAmount;
            BidPercentile = masterGameYear.BidPercentile;
            AverageDraftPosition = masterGameYear.AverageDraftPosition;
            AverageWinningBid = masterGameYear.AverageWinningBid;

            HypeFactor = masterGameYear.HypeFactor;
            DateAdjustedHypeFactor = masterGameYear.DateAdjustedHypeFactor;
            LinearRegressionHypeFactor = masterGameYear.LinearRegressionHypeFactor;
        }

        public MasterGameCalculatedStats(MasterGame masterGame, int year, double percentStandardGame, double percentCounterPick, double eligiblePercentStandardGame, 
            double eligiblePercentCounterPick, int numberOfBids, int totalBidAmount, double bidPercentile, double? averageDraftPosition, double? averageWinningBid, 
            double hypeFactor, double dateAdjustedHypeFactor, double linearRegressionHypeFactor)
        {
            MasterGame = masterGame;
            Year = year;

            PercentStandardGame = percentStandardGame;
            PercentCounterPick = percentCounterPick;
            EligiblePercentStandardGame = eligiblePercentStandardGame;
            EligiblePercentCounterPick = eligiblePercentCounterPick;
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
        public double EligiblePercentCounterPick { get; set; }
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
