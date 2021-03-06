﻿using System;
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

            if (scoringSystemName == "Diminishing")
            {
                return new DiminishingScoringSystem();
            }

            throw new Exception($"Scoring system not implemented: {scoringSystemName}");
        }

        public static ScoringSystem GetDefaultScoringSystem(int year)
        {
            if (year < 2021)
            {
                return new StandardScoringSystem();
            }

            return new DiminishingScoringSystem();
        }

        public static IReadOnlyList<ScoringSystem> GetAllPossibleValues()
        {
            return new List<ScoringSystem>(){new StandardScoringSystem(), new DiminishingScoringSystem()};
        }

        public abstract string Name { get; }

        public abstract decimal GetPointsForScore(decimal criticScore, bool counterPick);
    }
}
