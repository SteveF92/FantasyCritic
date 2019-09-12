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

        public static ScoringSystem GetRoyaleScoringSystem() => new StandardScoringSystem();

        public static IReadOnlyList<ScoringSystem> GetAllPossibleValues()
        {
            return new List<ScoringSystem>(){new StandardScoringSystem()};
        }

        public abstract string Name { get; }

        public abstract decimal GetPointsForScore(decimal criticScore, bool counterPick);
    }
}
