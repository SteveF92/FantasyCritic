using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Identity;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class Publisher : IEquatable<Publisher>
    {
        public Publisher(Guid publisherID, LeagueYear leagueYear, FantasyCriticUser user, string publisherName, int draftPosition, 
            IEnumerable<PublisherGame> publisherGames, uint budget, int freeGamesDropped, int willNotReleaseGamesDropped, int willReleaseGamesDropped,
            bool autoDraft)
        {
            PublisherID = publisherID;
            LeagueYear = leagueYear;
            User = user;
            PublisherName = publisherName;
            DraftPosition = draftPosition;
            PublisherGames = publisherGames.ToList();
            Budget = budget;
            FreeGamesDropped = freeGamesDropped;
            WillNotReleaseGamesDropped = willNotReleaseGamesDropped;
            WillReleaseGamesDropped = willReleaseGamesDropped;
            AutoDraft = autoDraft;
        }

        public Guid PublisherID { get; }
        public LeagueYear LeagueYear { get; }
        public FantasyCriticUser User { get; }
        public string PublisherName { get; }
        public int DraftPosition { get; }
        public IReadOnlyList<PublisherGame> PublisherGames { get; private set; }
        public uint Budget { get; private set; }
        public int FreeGamesDropped { get; private set; }
        public int WillNotReleaseGamesDropped { get; private set; }
        public int WillReleaseGamesDropped { get; private set; }
        public bool AutoDraft { get; }

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

        public decimal GetProjectedFantasyPoints(LeagueOptions options, SystemWideValues systemWideValues, bool yearFinished, bool simpleProjections, LocalDate currentDate)
        {
            var currentGamesScore =  PublisherGames.Sum(x => x.GetProjectedOrRealFantasyPoints(options.ScoringSystem, systemWideValues, simpleProjections, currentDate));
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

        public Maybe<PublisherGame> GetPublisherGame(MasterGame masterGame) => GetPublisherGameByMasterGameID(masterGame.MasterGameID);

        public Maybe<PublisherGame> GetPublisherGameByMasterGameID(Guid masterGameID)
        {
            return PublisherGames.SingleOrDefault(x => x.MasterGame.HasValue && x.MasterGame.Value.MasterGame.MasterGameID == masterGameID);
        }

        public Maybe<PublisherGame> GetPublisherGameByPublisherGameID(Guid publisherGameID)
        {
            return PublisherGames.SingleOrDefault(x => x.PublisherGameID == publisherGameID);
        }

        public HashSet<MasterGame> MyMasterGames => PublisherGames
            .Where(x => x.MasterGame.HasValue)
            .Select(x => x.MasterGame.Value.MasterGame)
            .Distinct()
            .ToHashSet();

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

        public Result CanDropGame(bool willRelease)
        {
            var leagueOptions = LeagueYear.Options;
            if (willRelease)
            {
                if (leagueOptions.WillReleaseDroppableGames == -1 || leagueOptions.WillReleaseDroppableGames > WillReleaseGamesDropped)
                {
                    return Result.Success();
                }
                if (leagueOptions.FreeDroppableGames == -1 || leagueOptions.FreeDroppableGames > FreeGamesDropped)
                {
                    return Result.Success();
                }
                return Result.Failure("Publisher cannot drop any more 'Will Release' games");
            }

            if (leagueOptions.WillNotReleaseDroppableGames == -1 || leagueOptions.WillNotReleaseDroppableGames > WillNotReleaseGamesDropped)
            {
                return Result.Success();
            }
            if (leagueOptions.FreeDroppableGames == -1 || leagueOptions.FreeDroppableGames > FreeGamesDropped)
            {
                return Result.Success();
            }
            return Result.Failure("Publisher cannot drop any more 'Will Not Release' games");
        }

        public void DropGame(PublisherGame publisherGame)
        {
            var leagueOptions = LeagueYear.Options;
            if (publisherGame.WillRelease())
            {
                if (leagueOptions.WillReleaseDroppableGames == -1 || leagueOptions.WillReleaseDroppableGames > WillReleaseGamesDropped)
                {
                    WillReleaseGamesDropped++;
                    PublisherGames = PublisherGames.Where(x => x.PublisherGameID != publisherGame.PublisherGameID).ToList();
                    return;
                }
                if (leagueOptions.FreeDroppableGames == -1 || leagueOptions.FreeDroppableGames > FreeGamesDropped)
                {
                    FreeGamesDropped++;
                    PublisherGames = PublisherGames.Where(x => x.PublisherGameID != publisherGame.PublisherGameID).ToList();
                    return;
                }
                throw new Exception("Publisher cannot drop any more 'Will Release' games");
            }

            if (leagueOptions.WillNotReleaseDroppableGames == -1 || leagueOptions.WillNotReleaseDroppableGames > WillNotReleaseGamesDropped)
            {
                WillNotReleaseGamesDropped++;
                PublisherGames = PublisherGames.Where(x => x.PublisherGameID != publisherGame.PublisherGameID).ToList();
                return;
            }
            if (leagueOptions.FreeDroppableGames == -1 || leagueOptions.FreeDroppableGames > FreeGamesDropped)
            {
                FreeGamesDropped++;
                PublisherGames = PublisherGames.Where(x => x.PublisherGameID != publisherGame.PublisherGameID).ToList();
                return;
            }
            throw new Exception("Publisher cannot drop any more 'Will Not Release' games");
        }
    }
}
