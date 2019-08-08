using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Stats
{
    public class MasterGameYearRInput
    {
        public MasterGameYearRInput(MasterGameYear masterGameYear)
        {
            Year = masterGameYear.Year;
            MasterGameID = masterGameYear.MasterGame.MasterGameID;
            GameName = masterGameYear.MasterGame.GameName;
            DateAdjustedHypeFactor = masterGameYear.DateAdjustedHypeFactor;
            TotalBidAmount = masterGameYear.TotalBidAmount;
            EligiblePercentCounterPick = masterGameYear.EligiblePercentCounterPick;

            if (masterGameYear.MasterGame.CriticScore.HasValue)
            {
                CriticScore = masterGameYear.MasterGame.CriticScore.Value;
            }
            else
            {
                CriticScore = 70m;
            }
        }

        public int Year { get; }
        public Guid MasterGameID { get; }
        public string GameName { get; }
        public double DateAdjustedHypeFactor { get; }
        public int TotalBidAmount { get; }
        public double EligiblePercentCounterPick { get; }
        public decimal CriticScore { get; }
    }
}
