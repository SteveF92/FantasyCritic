using System;
using System.Collections.Generic;
using System.Text;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class RoyalePublisherGame
    {
        public RoyalePublisherGame(Guid publisherID, Guid publisherGameID, string gameName, Instant timestamp, bool counterPick, 
            decimal? fantasyPoints, MasterGameYear masterGame)
        {
            PublisherID = publisherID;
            PublisherGameID = publisherGameID;
            GameName = gameName;
            Timestamp = timestamp;
            CounterPick = counterPick;
            FantasyPoints = fantasyPoints;
            MasterGame = masterGame;
        }

        public Guid PublisherID { get; }
        public Guid PublisherGameID { get; }
        public string GameName { get; }
        public Instant Timestamp { get; }
        public bool CounterPick { get; }
        public decimal? FantasyPoints { get; }
        public MasterGameYear MasterGame { get; }

        public bool WillRelease()
        {
            return MasterGame.WillRelease();
        }

        public decimal GetProjectedFantasyPoints(ScoringSystem scoringSystem, SystemWideValues systemWideValues, bool simpleProjections)
        {
            return MasterGame.GetProjectedFantasyPoints(scoringSystem, CounterPick, systemWideValues, simpleProjections);
        }

        public decimal? CalculateFantasyPoints(ScoringSystem scoringSystem)
        {
            return MasterGame.CalculateFantasyPoints(scoringSystem, CounterPick);
        }

        public override string ToString() => GameName;
    }
}
