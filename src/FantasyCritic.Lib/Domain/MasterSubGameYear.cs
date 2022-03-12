using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class MasterSubGameYear
    {
        public MasterSubGameYear(MasterSubGame masterGame, int year)
        {
            MasterGame = masterGame;
            Year = year;
        }

        public MasterSubGameYear(MasterSubGame masterGame, int year, decimal percentStandardGame, decimal percentCounterPick, decimal averageDraftPosition)
        {
            MasterGame = masterGame;
            Year = year;
            PercentStandardGame = percentStandardGame;
            PercentCounterPick = percentCounterPick;
            AverageDraftPosition = averageDraftPosition;
        }

        public MasterSubGame MasterGame { get; }
        public int Year { get; }
        public decimal PercentStandardGame { get; }
        public decimal PercentCounterPick { get; }
        public decimal AverageDraftPosition { get; }
    }
}
