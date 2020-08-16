using System;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Lib.Statistics
{
    public class MasterGameYearScriptInput
    {
        public MasterGameYearScriptInput(MasterGameYear masterGameYear)
        {
            Year = masterGameYear.Year;
            MasterGameID = masterGameYear.MasterGame.MasterGameID;
            GameName = masterGameYear.MasterGame.GameName;

            EligiblePercentStandardGame = masterGameYear.EligiblePercentStandardGame;
            EligiblePercentCounterPick = masterGameYear.EligiblePercentCounterPick;
            AverageDraftPosition = masterGameYear.AverageDraftPosition;
            DateAdjustedHypeFactor = masterGameYear.DateAdjustedHypeFactor;
            TotalBidAmount = masterGameYear.TotalBidAmount;
            BidPercentile = masterGameYear.BidPercentile;

            if (masterGameYear.MasterGame.CriticScore.HasValue)
            {
                CriticScore = masterGameYear.MasterGame.CriticScore.Value;
            }
            else if (!masterGameYear.WillRelease())
            {
                CriticScore = 70m;
            }
        }

        public int Year { get; }
        public Guid MasterGameID { get; }
        public string GameName { get; }
        public double EligiblePercentStandardGame { get; }
        public double EligiblePercentCounterPick { get; }
        public double DateAdjustedHypeFactor { get; }
        public double? AverageDraftPosition { get; }
        public int TotalBidAmount { get; }
        public double BidPercentile { get; }
        public decimal? CriticScore { get; }
    }
}
