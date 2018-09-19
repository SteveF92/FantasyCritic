using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain.ScoringSystems;
using NLog;
using NodaTime;

namespace FantasyCritic.Lib.Domain.ScoringSystems
{
    public abstract class ScoringSystem
    {
        public static ScoringSystem GetScoringSystem(string scoringSystemName)
        {
            if (scoringSystemName == "Standard")
            {
                return new StandardScoringSystem();
            }

            throw new Exception($"Scoring system not implemented: {scoringSystemName}");
        }

        public static IReadOnlyList<ScoringSystem> GetAllPossibleValues()
        {
            return new List<ScoringSystem>(){new StandardScoringSystem()};
        }

        public decimal? GetPointsForGame(PublisherGame publisherGame, IClock clock)
        {
            return GetPointsInternal(publisherGame, clock);
        }

        public decimal GetProjectedPointsForGame(PublisherGame publisherGame, decimal estimatedCriticScore)
        {
            if (publisherGame.FantasyPoints.HasValue)
            {
                return publisherGame.FantasyPoints.Value;
            }

            return GetPointsForScore(publisherGame, estimatedCriticScore);
        }

        public abstract string Name { get; }

        protected abstract decimal? GetPointsInternal(PublisherGame publisherGame, IClock clock);
        protected abstract decimal GetPointsForScore(PublisherGame publisherGame, decimal criticScore);
    }
}
