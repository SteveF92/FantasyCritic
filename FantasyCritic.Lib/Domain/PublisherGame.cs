using System;
using System.Collections.Generic;
using System.Text;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class PublisherGame
    {
        private readonly int _leagueYear;

        public PublisherGame(Guid publisherGameID, string gameName, Instant timestamp, bool counterPick, decimal? manualCriticScore, decimal? fantasyPoints, 
            Maybe<MasterGame> masterGame, int? draftPosition, int? overallDraftPosition, int leagueYear)
        {
            PublisherGameID = publisherGameID;
            GameName = gameName;
            Timestamp = timestamp;
            CounterPick = counterPick;
            ManualCriticScore = manualCriticScore;
            FantasyPoints = fantasyPoints;
            MasterGame = masterGame;
            DraftPosition = draftPosition;
            OverallDraftPosition = overallDraftPosition;
            _leagueYear = leagueYear;
        }

        public Guid PublisherGameID { get; }
        public string GameName { get; }
        public Instant Timestamp { get; }
        public bool CounterPick { get; }
        public decimal? ManualCriticScore { get; }
        public decimal? FantasyPoints { get; }
        public Maybe<MasterGame> MasterGame { get; }
        public int? DraftPosition { get; }
        public int? OverallDraftPosition { get; }

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

        public decimal GetProjectedFantasyPoints(ScoringSystem scoringSystem, LeagueWideValues leagueWideValues)
        {
            return scoringSystem.GetProjectedPointsForGame(this, leagueWideValues);
        }

        public override string ToString() => GameName;
    }
}
