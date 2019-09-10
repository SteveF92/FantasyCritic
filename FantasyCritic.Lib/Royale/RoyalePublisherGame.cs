using System;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using NodaTime;

namespace FantasyCritic.Lib.Royale
{
    public class RoyalePublisherGame
    {
        public RoyalePublisherGame(Guid publisherID, RoyaleYearQuarter yearQuarter, MasterGameYear masterGame, Instant timestamp, 
            decimal amountSpent, decimal advertisingMoney, decimal? fantasyPoints)
        {
            PublisherID = publisherID;
            YearQuarter = yearQuarter;
            MasterGame = masterGame;
            Timestamp = timestamp;
            AmountSpent = amountSpent;
            AdvertisingMoney = advertisingMoney;
            FantasyPoints = fantasyPoints;
        }

        public Guid PublisherID { get; }
        public RoyaleYearQuarter YearQuarter { get; }
        public MasterGameYear MasterGame { get; }
        public Instant Timestamp { get; }
        public decimal AmountSpent { get; }
        public decimal AdvertisingMoney { get; }
        public decimal? FantasyPoints { get; }
        
        public bool WillRelease()
        {
            return MasterGame.WillReleaseInQuarter(YearQuarter.YearQuarter);
        }

        public decimal GetProjectedFantasyPoints(ScoringSystem scoringSystem, SystemWideValues systemWideValues, bool simpleProjections)
        {
            return MasterGame.GetProjectedFantasyPoints(scoringSystem, false, systemWideValues, simpleProjections);
        }

        public decimal? CalculateFantasyPoints(ScoringSystem scoringSystem)
        {
            return MasterGame.CalculateFantasyPoints(scoringSystem, false);
        }

        public override string ToString() => MasterGame.MasterGame.GameName;
    }
}
