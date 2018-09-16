using System;
using System.Collections.Generic;
using System.Text;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Lib.Enums
{
    public class ScoringSystem : TypeSafeEnum<ScoringSystem>
    {
        private readonly Func<PublisherGame, IClock, decimal?> _scoringFunction;
        
        // Define values here.
        public static readonly ScoringSystem Manual = new ScoringSystem("Manual", ManualScore);
        public static readonly ScoringSystem Standard = new ScoringSystem("Standard", StandardScore);

        // Constructor is private: values are defined within this class only!
        private ScoringSystem(string value, Func<PublisherGame, IClock, decimal?> scoringFunction)
            : base(value)
        {
            _scoringFunction = scoringFunction;
        }

        public decimal? ScoreGame(PublisherGame publisherGame, IClock clock)
        {
            return _scoringFunction.Invoke(publisherGame, clock);
        }

        public static decimal? ManualScore(PublisherGame publisherGame, IClock clock)
        {
            return null;
        }

        public static decimal? StandardScore(PublisherGame publisherGame, IClock clock)
        {
            if (publisherGame.MasterGame.HasNoValue)
            {
                return null;
            }

            if (!publisherGame.WillRelease())
            {
                return 0m;
            }

            if (!publisherGame.MasterGame.Value.IsReleased(clock))
            {
                return null;
            }

            decimal? possibleCriticScore = publisherGame.MasterGame.Value.CriticScore;
            if (!possibleCriticScore.HasValue)
            {
                return 0m;
            }

            decimal criticScore = possibleCriticScore.Value;

            decimal fantasyPoints = 0m;
            decimal criticPointsOver90 = (criticScore - 90);
            if (criticPointsOver90 > 0)
            {
                fantasyPoints += criticPointsOver90;
            }

            decimal criticPointsOver70 = (criticScore - 70);
            fantasyPoints += criticPointsOver70;

            if (publisherGame.CounterPick)
            {
                fantasyPoints *= -1;
            }

            return fantasyPoints;
        }

        public override string ToString() => Value;
    }
}
