using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class MasterGameYear
    {
        public MasterGameYear(MasterGame masterGame, int year)
        {
            MasterGame = masterGame;
            Year = year;
        }

        public MasterGameYear(MasterGame masterGame, int year, decimal percentStandardGame, decimal percentCounterPick, decimal averageDraftPosition, 
            decimal? hypeFactor, decimal? dateAdjustedHypeFactor)
        {
            MasterGame = masterGame;
            Year = year;
            PercentStandardGame = percentStandardGame;
            PercentCounterPick = percentCounterPick;
            AverageDraftPosition = averageDraftPosition;
            if (AverageDraftPosition == 0m)
            {
                AverageDraftPosition = null;
            }

            HypeFactor = hypeFactor;
            DateAdjustedHypeFactor = dateAdjustedHypeFactor;
        }

        public MasterGame MasterGame { get; }
        public int Year { get; }
        public decimal PercentStandardGame { get; }
        public decimal PercentCounterPick { get; }
        public decimal? AverageDraftPosition { get; }
        public decimal? HypeFactor { get; }
        public decimal? DateAdjustedHypeFactor { get; set; }
    }
}
