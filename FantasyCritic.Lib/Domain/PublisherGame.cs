using System;
using System.Collections.Generic;
using System.Text;
using CSharpFunctionalExtensions;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class PublisherGame
    {
        public PublisherGame(string gameName, Instant timestamp, bool waiver, bool antiPick, decimal? fantasyScore, Maybe<MasterGame> masterGame)
        {
            GameName = gameName;
            Timestamp = timestamp;
            Waiver = waiver;
            AntiPick = antiPick;
            FantasyScore = fantasyScore;
            MasterGame = masterGame;
        }

        public string GameName { get; }
        public Instant Timestamp { get; }
        public bool Waiver { get; }
        public bool AntiPick { get; }
        public decimal? FantasyScore { get; }
        public Maybe<MasterGame> MasterGame { get; }
    }
}
