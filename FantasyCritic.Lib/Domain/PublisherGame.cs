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

        public PublisherGame(Guid publisherID, Guid publisherGameID, string gameName, Instant timestamp, bool counterPick, decimal? manualCriticScore, decimal? fantasyPoints, 
            Maybe<MasterGameYear> masterGame, int? draftPosition, int? overallDraftPosition, int leagueYear)
        {
            PublisherID = publisherID;
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

        public Guid PublisherID { get; }
        public Guid PublisherGameID { get; }
        public string GameName { get; }
        public Instant Timestamp { get; }
        public bool CounterPick { get; }
        public decimal? ManualCriticScore { get; }
        public decimal? FantasyPoints { get; }
        public Maybe<MasterGameYear> MasterGame { get; }
        public int? DraftPosition { get; }
        public int? OverallDraftPosition { get; }

        public bool WillRelease()
        {
            if (MasterGame.HasNoValue)
            {
                return false;
            }

            if (_leagueYear < MasterGame.Value.MasterGame.MinimumReleaseDate.Year)
            {
                return false;
            }

            return true;
        }

        public decimal GetProjectedFantasyPoints(ScoringSystem scoringSystem, SystemWideValues systemWideValues)
        {
            decimal? criticScoreToUse = CalculateFantasyPoints(scoringSystem);
            if (!criticScoreToUse.HasValue)
            {
                if (MasterGame.HasValue)
                {
                    criticScoreToUse = Convert.ToDecimal(MasterGame.Value.LinearRegressionHypeFactor);
                }
                else
                {
                    return systemWideValues.GetAveragePoints(CounterPick);
                }
            }

            return scoringSystem.GetPointsForScore(criticScoreToUse.Value, CounterPick);
        }

        public decimal? CalculateFantasyPoints(ScoringSystem scoringSystem)
        {
            decimal criticScoreToUse;
            if (ManualCriticScore.HasValue)
            {
                criticScoreToUse = ManualCriticScore.Value;
            }
            else if (MasterGame.HasNoValue)
            {
                return null;
            }
            else if (MasterGame.Value.MasterGame.CriticScore.HasValue)
            {
                criticScoreToUse = MasterGame.Value.MasterGame.CriticScore.Value;
            }
            else if (!WillRelease())
            {
                return 0m;
            }
            else
            {
                return null;
            }

            return scoringSystem.GetPointsForScore(criticScoreToUse, CounterPick);
        }

        public override string ToString() => GameName;
    }
}
