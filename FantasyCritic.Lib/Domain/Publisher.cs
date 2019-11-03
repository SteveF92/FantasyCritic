using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain.ScoringSystems;

namespace FantasyCritic.Lib.Domain
{
    public class Publisher : IEquatable<Publisher>
    {
        public Publisher(Guid publisherID, LeagueYear leagueYear, FantasyCriticUser user, string publisherName, int draftPosition, IEnumerable<PublisherGame> publisherGames, uint budget)
        {
            PublisherID = publisherID;
            LeagueYear = leagueYear;
            User = user;
            PublisherName = publisherName;
            DraftPosition = draftPosition;
            PublisherGames = publisherGames.ToList();
            Budget = budget;
        }

        public Guid PublisherID { get; }
        public LeagueYear LeagueYear { get; }
        public FantasyCriticUser User { get; }
        public string PublisherName { get; }
        public int DraftPosition { get; }
        public IReadOnlyList<PublisherGame> PublisherGames { get; private set; }
        public uint Budget { get; private set; }

        public decimal? AverageCriticScore
        {
            get
            {
                List<decimal> gamesWithCriticScores = PublisherGames
                    .Where(x => !x.CounterPick)
                    .Where(x => x.MasterGame.HasValue)
                    .Where(x => x.MasterGame.Value.MasterGame.CriticScore.HasValue)
                    .Select(x => x.MasterGame.Value.MasterGame.CriticScore.Value)
                    .ToList();

                if (gamesWithCriticScores.Count == 0)
                {
                    return null;
                }

                decimal average = gamesWithCriticScores.Sum(x => x) / gamesWithCriticScores.Count;
                return average;
            }
        }

        public decimal TotalFantasyPoints
        {
            get
            {
                var score = PublisherGames.Sum(x => x.FantasyPoints);
                if (!score.HasValue)
                {
                    return 0m;
                }

                return score.Value;
            }
        }

        public decimal GetProjectedFantasyPoints(LeagueOptions options, SystemWideValues systemWideValues, bool yearFinished, bool simpleProjections)
        {
            var currentGamesScore =  PublisherGames.Sum(x => x.GetProjectedOrRealFantasyPoints(options.ScoringSystem, systemWideValues, simpleProjections));
            var availableSlots = GetAvailableSlots(options, yearFinished);
            var emptySlotsScore = availableSlots * systemWideValues.AverageStandardGamePoints;
            return currentGamesScore + emptySlotsScore;
        }

        public bool HasRemainingGameSpot(int totalSpots)
        {
            if (totalSpots > PublisherGames.Count(x => !x.CounterPick))
            {
                return true;
            }

            return false;
        }

        public int GetAvailableSlots(LeagueOptions options, bool yearFinished)
        {
            if (yearFinished)
            {
                return 0;
            }

            return options.StandardGames - PublisherGames.Count(x => !x.CounterPick);
        }

        public bool Equals(Publisher other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return PublisherID.Equals(other.PublisherID);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Publisher) obj);
        }

        public override int GetHashCode()
        {
            return PublisherID.GetHashCode();
        }

        public void AcquireGame(PublisherGame game, uint bidAmount)
        {
            PublisherGames = PublisherGames.Concat(new []{ game }).ToList();
            Budget -= bidAmount;
        }
    }
}
