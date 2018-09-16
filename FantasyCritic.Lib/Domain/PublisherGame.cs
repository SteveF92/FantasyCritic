using System;
using System.Collections.Generic;
using System.Text;
using CSharpFunctionalExtensions;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class PublisherGame
    {
        private readonly int _leagueYear;

        public PublisherGame(Guid publisherGameID, string gameName, Instant timestamp, bool waiver, bool counterPick, decimal? fantasyScore, Maybe<MasterGame> masterGame, int leagueYear)
        {
            PublisherGameID = publisherGameID;
            GameName = gameName;
            Timestamp = timestamp;
            Waiver = waiver;
            CounterPick = counterPick;
            FantasyScore = fantasyScore;
            MasterGame = masterGame;
            _leagueYear = leagueYear;
        }

        public Guid PublisherGameID { get; }
        public string GameName { get; }
        public Instant Timestamp { get; }
        public bool Waiver { get; }
        public bool CounterPick { get; }
        public decimal? FantasyScore { get; }
        public Maybe<MasterGame> MasterGame { get; }

        public bool WillRelease()
        {
            if (MasterGame.HasNoValue)
            {
                return false;
            }

            if (_leagueYear < MasterGame.Value.MinimumReleaseYear)
            {
                return false;
            }

            return true;
        }

        public override string ToString() => GameName;
    }
}
