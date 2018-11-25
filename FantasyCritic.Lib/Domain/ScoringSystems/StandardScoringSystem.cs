using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace FantasyCritic.Lib.Domain.ScoringSystems
{
    public class StandardScoringSystem : ScoringSystem
    {
        public override string Name => "Standard";

        protected override decimal? GetPointsInternal(PublisherGame publisherGame, IClock clock, LeagueWideValues leagueWideValues)
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

            decimal? possibleManualScore = publisherGame.ManualCriticScore;
            if (possibleManualScore.HasValue)
            {
                return GetPointsForScore(publisherGame, possibleManualScore.Value, leagueWideValues);
            }

            decimal? possibleCriticScore = publisherGame.MasterGame.Value.CriticScore;
            if (!possibleCriticScore.HasValue)
            {
                return 0m;
            }

            return GetPointsForScore(publisherGame, possibleCriticScore.Value, leagueWideValues);
        }

        protected override decimal GetPointsForScore(PublisherGame publisherGame, decimal? criticScore, LeagueWideValues leagueWideValues)
        {
            if (criticScore.HasValue)
            {
                decimal fantasyPoints = 0m;
                decimal criticPointsOver90 = (criticScore.Value - 90);
                if (criticPointsOver90 > 0)
                {
                    fantasyPoints += criticPointsOver90;
                }

                decimal criticPointsOver70 = (criticScore.Value - 70);
                fantasyPoints += criticPointsOver70;

                if (publisherGame.CounterPick)
                {
                    fantasyPoints *= -1;
                }

                return fantasyPoints;
            }

            if (publisherGame.CounterPick)
            {
                return leagueWideValues.AverageCounterPickPoints;
            }

            return leagueWideValues.AverageStandardGamePoints;
        }
    }
}
