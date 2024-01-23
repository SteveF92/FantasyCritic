using FantasyCritic.Lib.Domain.Calculations;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain;

public class Publisher : IEquatable<Publisher>
{
    private decimal? _cachedProjectedPoints;
    private decimal? _cachedTotalPoints;

    public Publisher(Guid publisherID, LeagueYearKey leagueYearKey, FantasyCriticUser user, string publisherName, string? publisherIcon, string? publisherSlogan,
        int draftPosition, IEnumerable<PublisherGame> publisherGames, IEnumerable<FormerPublisherGame> formerPublisherGames, uint budget,
        int freeGamesDropped, int willNotReleaseGamesDropped, int willReleaseGamesDropped, int superDropsAvailable, AutoDraftMode autoDraftMode)
    {
        PublisherID = publisherID;
        LeagueYearKey = leagueYearKey;
        User = user;
        PublisherName = publisherName;
        PublisherIcon = publisherIcon;
        PublisherSlogan = publisherSlogan;
        DraftPosition = draftPosition;
        PublisherGames = publisherGames.ToList();
        FormerPublisherGames = formerPublisherGames.ToList();
        Budget = budget;
        FreeGamesDropped = freeGamesDropped;
        WillNotReleaseGamesDropped = willNotReleaseGamesDropped;
        WillReleaseGamesDropped = willReleaseGamesDropped;
        SuperDropsAvailable = superDropsAvailable;
        AutoDraftMode = autoDraftMode;
    }

    public Guid PublisherID { get; }
    public LeagueYearKey LeagueYearKey { get; }
    public FantasyCriticUser User { get; }
    public string PublisherName { get; }
    public string? PublisherIcon { get; }
    public string? PublisherSlogan { get; }
    public int DraftPosition { get; }
    public IReadOnlyList<PublisherGame> PublisherGames { get; }
    public IReadOnlyList<FormerPublisherGame> FormerPublisherGames { get; }
    public uint Budget { get; }
    public int FreeGamesDropped { get; }
    public int WillNotReleaseGamesDropped { get; }
    public int WillReleaseGamesDropped { get; }
    public int SuperDropsAvailable { get; }
    public AutoDraftMode AutoDraftMode { get; }

    public decimal? AverageCriticScore
    {
        get
        {
            List<decimal> gamesWithCriticScores = PublisherGames
                .Where(x => !x.CounterPick)
                .Where(x => x.MasterGame is not null)
                .Where(x => x.MasterGame!.MasterGame.CriticScore.HasValue)
                .Select(x => x.MasterGame!.MasterGame.CriticScore!.Value)
                .ToList();

            if (gamesWithCriticScores.Count == 0)
            {
                return null;
            }

            decimal average = gamesWithCriticScores.Sum(x => x) / gamesWithCriticScores.Count;
            return average;
        }
    }

    public decimal GetTotalFantasyPoints(SupportedYear year, LeagueOptions leagueOptions)
    {
        if (_cachedTotalPoints.HasValue)
        {
            return _cachedTotalPoints.Value;
        }

        var emptyCounterPickSlotPoints = GetEmptyCounterPickSlotPoints(year, leagueOptions) ?? 0m;
        var score = PublisherGames.Sum(x => x.FantasyPoints);
        if (!score.HasValue)
        {
            _cachedTotalPoints = emptyCounterPickSlotPoints;
            return emptyCounterPickSlotPoints;
        }

        _cachedTotalPoints = score.Value + emptyCounterPickSlotPoints;
        return score.Value + emptyCounterPickSlotPoints;
    }

    public decimal GetProjectedFantasyPoints(LeagueYear leagueYear, SystemWideValues systemWideValues, LocalDate currentDate)
    {
        if (_cachedProjectedPoints.HasValue)
        {
            return _cachedProjectedPoints.Value;
        }

        var leagueOptions = leagueYear.Options;
        bool ineligiblePointsShouldCount = leagueYear.Options.HasSpecialSlots;
        var slots = GetPublisherSlots(leagueOptions);
        decimal projectedScore = 0;
        foreach (var slot in slots)
        {
            bool countSlotAsValid = ineligiblePointsShouldCount || slot.SlotIsValid(leagueYear);
            var slotScore = slot.GetRealUpcomingOrProjectedFantasyPoints(leagueYear.SupportedYear, countSlotAsValid, leagueOptions.ScoringSystem, systemWideValues,
                leagueYear.StandardGamesTaken, leagueYear.TotalNumberOfStandardGames, currentDate);
            projectedScore += slotScore;
        }

        _cachedProjectedPoints = projectedScore;
        return projectedScore;
    }

    private decimal? GetEmptyCounterPickSlotPoints(SupportedYear year, LeagueOptions leagueOptions)
    {
        if (!SupportedYear.Year2022FeatureSupported(year.Year))
        {
            return 0m;
        }

        if (!year.Finished)
        {
            return null;
        }

        var expectedNumberOfCounterPicks = leagueOptions.CounterPicks;
        var numberCounterPicks = PublisherGames.Count(x => x.CounterPick);
        var emptySlots = expectedNumberOfCounterPicks - numberCounterPicks;
        var points = emptySlots * -15m;
        return points;
    }

    public IReadOnlyList<PublisherSlot> GetPublisherSlots(LeagueOptions leagueOptions)
    {
        return GetPublisherSlots(leagueOptions, PublisherGames);
    }

    public static IReadOnlyList<PublisherSlot> GetPublisherSlots(LeagueOptions leagueOptions, IReadOnlyList<PublisherGame> publisherGames)
    {
        List<PublisherSlot> publisherSlots = new List<PublisherSlot>();

        int overallSlotNumber = 0;
        var standardGamesBySlot = publisherGames.Where(x => !x.CounterPick).ToDictionary(x => x.SlotNumber);
        for (int standardGameIndex = 0; standardGameIndex < leagueOptions.StandardGames; standardGameIndex++)
        {
            PublisherGame? standardGame = standardGamesBySlot.GetValueOrDefault(standardGameIndex);
            SpecialGameSlot? specialSlot = leagueOptions.GetSpecialGameSlotByOverallSlotNumber(standardGameIndex);

            publisherSlots.Add(new PublisherSlot(standardGameIndex, overallSlotNumber, false, specialSlot, standardGame));
            overallSlotNumber++;
        }

        var counterPicksBySlot = publisherGames.Where(x => x.CounterPick).ToDictionary(x => x.SlotNumber);
        for (int counterPickIndex = 0; counterPickIndex < leagueOptions.CounterPicks; counterPickIndex++)
        {
            PublisherGame? counterPick = counterPicksBySlot.GetValueOrDefault(counterPickIndex);

            publisherSlots.Add(new PublisherSlot(counterPickIndex, overallSlotNumber, true, null, counterPick));
            overallSlotNumber++;
        }

        return publisherSlots;
    }

    public PublisherGame? GetPublisherGame(MasterGame masterGame, bool counterPick) => GetPublisherGameByMasterGameID(masterGame.MasterGameID, counterPick);
    public PublisherGame GetPublisherGameOrThrow(MasterGame masterGame, bool counterPick)
    {
        var publisherGame = GetPublisherGameByMasterGameID(masterGame.MasterGameID, counterPick);
        if (publisherGame is null)
        {
            throw new Exception($"Publisher: {PublisherID} does not have master game: {masterGame.GameName}");
        }

        return publisherGame;
    }

    public PublisherGame? GetPublisherGameByMasterGameID(Guid masterGameID, bool counterPick)
    {
        return PublisherGames.SingleOrDefault(x => x.CounterPick == counterPick && x.MasterGame is not null && x.MasterGame.MasterGame.MasterGameID == masterGameID);
    }

    public PublisherGame? GetPublisherGameByPublisherGameID(Guid publisherGameID)
    {
        return PublisherGames.SingleOrDefault(x => x.PublisherGameID == publisherGameID);
    }

    public Publisher GetUpdatedPublisherWithNewScores(IReadOnlyDictionary<Guid, PublisherGameCalculatedStats> calculatedStats)
    {
        var newPublisherGames = PublisherGames.Select(x => x.GetUpdatedPublisherGameWithNewScores(calculatedStats)).ToList();
        return new Publisher(PublisherID, LeagueYearKey, User, PublisherName, PublisherIcon, PublisherSlogan, DraftPosition, newPublisherGames,
            FormerPublisherGames, Budget, FreeGamesDropped, WillNotReleaseGamesDropped, WillReleaseGamesDropped, SuperDropsAvailable, AutoDraftMode);
    }
    
    public HashSet<MasterGame> MyMasterGames => PublisherGames
        .Where(x => x.MasterGame is not null)
        .Select(x => x.MasterGame!.MasterGame)
        .Distinct()
        .ToHashSet();

    public bool Equals(Publisher? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return PublisherID.Equals(other.PublisherID);
    }

    public override bool Equals(object? obj)
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

    public Result CanDropGame(bool couldRelease, LeagueOptions leagueOptions, bool superDrop)
    {
        if (superDrop)
        {
            if (SuperDropsAvailable > 0)
            {
                return Result.Success();
            }
            return Result.Failure("Publisher does not have any super drops.");
        }
        if (couldRelease)
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

    public static Publisher GetFakePublisher(LeagueYearKey leagueYearKey)
    {
        return new Publisher(Guid.Empty, leagueYearKey, FantasyCriticUser.GetFakeUser(), "<Unknown Publisher>",
            null,null, 0, new List<PublisherGame>(),
            new List<FormerPublisherGame>(), 0, 0, 0, 0, 0, AutoDraftMode.Off);
    }

    public string GetPublisherAndUserDisplayName()
    {
        return $"{PublisherName} ({User.UserName})";
    }

    public PublisherStatistics GetPublisherStatistics(LocalDate date, LeagueYear leagueYear, SystemWideValues systemWideValues)
    {
        byte numberOfStandardGames = 0, numberOfStandardGamesReleased = 0, numberOfStandardGamesExpectedToRelease = 0, numberOfStandardGamesNotExpectedToRelease = 0,
            numberOfCounterPicks = 0, numberOfCounterPicksReleased = 0, numberOfCounterPicksExpectedToRelease = 0, numberOfCounterPicksNotExpectedToRelease = 0;

        foreach (var game in PublisherGames)
        {
            if (!game.CounterPick)
            {
                numberOfStandardGames++;

                if (game.MasterGame is not null && game.MasterGame.MasterGame.IsReleased(date))
                {
                    numberOfStandardGamesReleased++;
                }

                if (game.WillRelease().CountAsWillRelease)
                {
                    numberOfStandardGamesExpectedToRelease++;
                }

                if (!game.WillRelease().CountAsWillRelease)
                {
                    numberOfStandardGamesNotExpectedToRelease++;
                }
            }
            else
            {
                numberOfCounterPicks++;

                if (game.MasterGame is not null && game.MasterGame.MasterGame.IsReleased(date))
                {
                    numberOfCounterPicksReleased++;
                }

                if (game.WillRelease().CountAsWillRelease)
                {
                    numberOfCounterPicksExpectedToRelease++;
                }

                if (!game.WillRelease().CountAsWillRelease)
                {
                    numberOfCounterPicksNotExpectedToRelease++;
                }
            }
        }

        return new PublisherStatistics(PublisherID, date)
        {
            FantasyPoints = GetTotalFantasyPoints(leagueYear.SupportedYear, leagueYear.Options),
            ProjectedPoints = GetProjectedFantasyPoints(leagueYear, systemWideValues, date),
            RemainingBudget = (ushort) Budget,
            NumberOfStandardGames = numberOfStandardGames,
            NumberOfStandardGamesReleased = numberOfStandardGamesReleased,
            NumberOfStandardGamesExpectedToRelease = numberOfStandardGamesExpectedToRelease,
            NumberOfStandardGamesNotExpectedToRelease = numberOfStandardGamesNotExpectedToRelease,
            NumberOfCounterPicks = numberOfCounterPicks,
            NumberOfCounterPicksReleased = numberOfCounterPicksReleased,
            NumberOfCounterPicksExpectedToRelease = numberOfCounterPicksExpectedToRelease,
            NumberOfCounterPicksNotExpectedToRelease = numberOfCounterPicksNotExpectedToRelease
        };
    }
}
