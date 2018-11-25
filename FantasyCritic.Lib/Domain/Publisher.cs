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
        public Publisher(Guid publisherID, League league, FantasyCriticUser user, int year, string publisherName, int draftPosition, IEnumerable<PublisherGame> publisherGames, int budget)
        {
            PublisherID = publisherID;
            League = league;
            User = user;
            Year = year;
            PublisherName = publisherName;
            DraftPosition = draftPosition;
            PublisherGames = publisherGames.ToList();
            Budget = budget;
        }

        public Guid PublisherID { get; }
        public League League { get; }
        public FantasyCriticUser User { get; }
        public int Year { get; }
        public string PublisherName { get; }
        public int DraftPosition { get; }
        public IReadOnlyList<PublisherGame> PublisherGames { get; }
        public int Budget { get; }

        public decimal? AverageCriticScore
        {
            get
            {
                List<decimal> gamesWithCriticScores = PublisherGames
                    .Where(x => !x.CounterPick)
                    .Where(x => x.MasterGame.HasValue)
                    .Where(x => x.MasterGame.Value.CriticScore.HasValue)
                    .Select(x => x.MasterGame.Value.CriticScore.Value)
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

        public decimal GetProjectedFantasyPoints(ScoringSystem scoringSystem, LeagueWideValues leagueWideValues)
        {
            return PublisherGames.Sum(x => x.GetProjectedFantasyPoints(scoringSystem, leagueWideValues));
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
    }
}
