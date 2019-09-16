﻿using System;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using NodaTime;

namespace FantasyCritic.Lib.Royale
{
    public class RoyalePublisherGame : IEquatable<RoyalePublisherGame>
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
        
        public bool WillRelease() => MasterGame.WillReleaseInQuarter(YearQuarter.YearQuarter);

        public decimal GetProjectedFantasyPoints() => MasterGame.GetProjectedFantasyPoints(ScoringSystem.GetRoyaleScoringSystem(), false);

        public decimal? CalculateFantasyPoints() => MasterGame.CalculateFantasyPoints(ScoringSystem.GetRoyaleScoringSystem(), false);

        public override string ToString() => MasterGame.MasterGame.GameName;

        public bool Equals(RoyalePublisherGame other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return PublisherID.Equals(other.PublisherID) && Equals(MasterGame, other.MasterGame);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RoyalePublisherGame) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (PublisherID.GetHashCode() * 397) ^ (MasterGame != null ? MasterGame.GetHashCode() : 0);
            }
        }
    }
}
