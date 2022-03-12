using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain
{
    public class Publisher : IEquatable<Publisher>
    {
        public Publisher(Guid publisherID, LeagueYear leagueYear, FantasyCriticUser user, string publisherName, Maybe<string> publisherIcon, int draftPosition,
            IEnumerable<PublisherGame> publisherGames, IEnumerable<FormerPublisherGame> formerPublisherGames, uint budget, int freeGamesDropped, int willNotReleaseGamesDropped, int willReleaseGamesDropped,
            bool autoDraft)
        {
            PublisherID = publisherID;
            LeagueYear = leagueYear;
            User = user;
            PublisherName = publisherName;
            PublisherIcon = publisherIcon;
            DraftPosition = draftPosition;
            PublisherGames = publisherGames.ToList();
            FormerPublisherGames = formerPublisherGames.ToList();
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
        public Maybe<string> PublisherIcon { get; }
        public int DraftPosition { get; }
        public IReadOnlyList<PublisherGame> PublisherGames { get; private set; }
        public IReadOnlyList<FormerPublisherGame> FormerPublisherGames { get; }
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
                var emptyCounterPickSlotPoints = GetEmptyCounterPickSlotPoints() ?? 0m;
                var score = PublisherGames.Sum(x => x.FantasyPoints);
                if (!score.HasValue)
                {
                    return emptyCounterPickSlotPoints;
                }

                return score.Value + emptyCounterPickSlotPoints;
            }
        }

        public decimal GetProjectedFantasyPoints(SystemWideValues systemWideValues, bool simpleProjections, LocalDate currentDate, bool ineligiblePointsShouldCount)
        {
            var score = GetPublisherSlots()
                .Sum(x => x.GetProjectedOrRealFantasyPoints(ineligiblePointsShouldCount || x.SlotIsValid(LeagueYear),
                    LeagueYear.Options.ScoringSystem, systemWideValues, simpleProjections, currentDate));

            return score;
        }

        private decimal? GetEmptyCounterPickSlotPoints()
        {
            if (!SupportedYear.Year2022FeatureSupported(LeagueYear.Year))
            {
                return 0m;
            }

            if (!LeagueYear.SupportedYear.Finished)
            {
                return null;
            }

            var expectedNumberOfCounterPicks = LeagueYear.Options.CounterPicks;
            var numberCounterPicks = PublisherGames.Count(x => x.CounterPick);
            var emptySlots = expectedNumberOfCounterPicks - numberCounterPicks;
            var points = emptySlots * -15m;
            return points;
        }

        public IReadOnlyList<PublisherSlot> GetPublisherSlots()
        {
            List<PublisherSlot> publisherSlots = new List<PublisherSlot>();

            int overallSlotNumber = 0;
            var standardGamesBySlot = PublisherGames.Where(x => !x.CounterPick).ToDictionary(x => x.SlotNumber);
            for (int standardGameIndex = 0; standardGameIndex < LeagueYear.Options.StandardGames; standardGameIndex++)
            {
                Maybe<PublisherGame> standardGame = Maybe<PublisherGame>.None;
                if (standardGamesBySlot.TryGetValue(standardGameIndex, out var foundGame))
                {
                    standardGame = foundGame;
                }
                Maybe<SpecialGameSlot> specialSlot = LeagueYear.Options.GetSpecialGameSlotByOverallSlotNumber(standardGameIndex);

                publisherSlots.Add(new PublisherSlot(standardGameIndex, overallSlotNumber, false, specialSlot, standardGame));
                overallSlotNumber++;
            }

            var counterPicksBySlot = PublisherGames.Where(x => x.CounterPick).ToDictionary(x => x.SlotNumber);
            for (int counterPickIndex = 0; counterPickIndex < LeagueYear.Options.CounterPicks; counterPickIndex++)
            {
                Maybe<PublisherGame> counterPick = Maybe<PublisherGame>.None;
                if (counterPicksBySlot.TryGetValue(counterPickIndex, out var foundGame))
                {
                    counterPick = foundGame;
                }

                publisherSlots.Add(new PublisherSlot(counterPickIndex, overallSlotNumber, true, Maybe<SpecialGameSlot>.None, counterPick));
                overallSlotNumber++;
            }

            return publisherSlots;
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
            return Equals((Publisher)obj);
        }

        public override int GetHashCode()
        {
            return PublisherID.GetHashCode();
        }

        public override string ToString() => $"{PublisherID}|{PublisherName}";

        public void AcquireGame(PublisherGame game, uint bidAmount)
        {
            PublisherGames = PublisherGames.Concat(new[] { game }).ToList();
            Budget -= bidAmount;
        }

        public void SpendBudget(uint budget)
        {
            Budget -= budget;
        }

        public void ObtainBudget(uint budget)
        {
            Budget += budget;
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

        public static Publisher GetFakePublisher(LeagueYear leagueYear)
        {
            return new Publisher(Guid.Empty, leagueYear, FantasyCriticUser.GetFakeUser(), "<Unknown Publisher>",
                Maybe<string>.None, 0, new List<PublisherGame>(),
                new List<FormerPublisherGame>(), 0, 0, 0, 0, false);
        }
    }
}
