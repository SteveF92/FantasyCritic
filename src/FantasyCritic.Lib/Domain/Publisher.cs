using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain;

public class Publisher : IEquatable<Publisher>
{
    public Publisher(Guid publisherID, LeagueYearKey leagueYearKey, FantasyCriticUser user, string publisherName, string? publisherIcon, int draftPosition,
        IEnumerable<PublisherGame> publisherGames, IEnumerable<FormerPublisherGame> formerPublisherGames, uint budget, int freeGamesDropped, int willNotReleaseGamesDropped, int willReleaseGamesDropped,
        bool autoDraft)
    {
        PublisherID = publisherID;
        LeagueYearKey = leagueYearKey;
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
    public LeagueYearKey LeagueYearKey { get; }
    public FantasyCriticUser User { get; }
    public string PublisherName { get; }
    public string? PublisherIcon { get; }
    public int DraftPosition { get; }
    public IReadOnlyList<PublisherGame> PublisherGames { get; }
    public IReadOnlyList<FormerPublisherGame> FormerPublisherGames { get; }
    public uint Budget { get; }
    public int FreeGamesDropped { get; }
    public int WillNotReleaseGamesDropped { get; }
    public int WillReleaseGamesDropped { get; }
    public bool AutoDraft { get; }

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
        var emptyCounterPickSlotPoints = GetEmptyCounterPickSlotPoints(year, leagueOptions) ?? 0m;
        var score = PublisherGames.Sum(x => x.FantasyPoints);
        if (!score.HasValue)
        {
            return emptyCounterPickSlotPoints;
        }

        return score.Value + emptyCounterPickSlotPoints;
    }

    public decimal GetProjectedFantasyPoints(LeagueYear leagueYear, SystemWideValues systemWideValues, LocalDate currentDate)
    {
        var leagueOptions = leagueYear.Options;
        bool ineligiblePointsShouldCount = leagueYear.Options.HasSpecialSlots;
        var slots = GetPublisherSlots(leagueOptions);
        decimal projectedScore = 0;
        foreach (var slot in slots)
        {
            bool countSlotAsValid = ineligiblePointsShouldCount || slot.SlotIsValid(leagueYear);
            var slotScore = slot.GetProjectedOrRealFantasyPoints(countSlotAsValid, leagueOptions.ScoringSystem, systemWideValues,
                leagueYear.StandardGamesTaken, leagueYear.TotalNumberOfStandardGames, currentDate);
            projectedScore += slotScore;
        }

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
        List<PublisherSlot> publisherSlots = new List<PublisherSlot>();

        int overallSlotNumber = 0;
        var standardGamesBySlot = PublisherGames.Where(x => !x.CounterPick).ToDictionary(x => x.SlotNumber);
        for (int standardGameIndex = 0; standardGameIndex < leagueOptions.StandardGames; standardGameIndex++)
        {
            PublisherGame? standardGame = null;
            if (standardGamesBySlot.TryGetValue(standardGameIndex, out var foundGame))
            {
                standardGame = foundGame;
            }
            SpecialGameSlot? specialSlot = leagueOptions.GetSpecialGameSlotByOverallSlotNumber(standardGameIndex);

            publisherSlots.Add(new PublisherSlot(standardGameIndex, overallSlotNumber, false, specialSlot, standardGame));
            overallSlotNumber++;
        }

        var counterPicksBySlot = PublisherGames.Where(x => x.CounterPick).ToDictionary(x => x.SlotNumber);
        for (int counterPickIndex = 0; counterPickIndex < leagueOptions.CounterPicks; counterPickIndex++)
        {
            PublisherGame? counterPick = null;
            if (counterPicksBySlot.TryGetValue(counterPickIndex, out var foundGame))
            {
                counterPick = foundGame;
            }

            publisherSlots.Add(new PublisherSlot(counterPickIndex, overallSlotNumber, true, null, counterPick));
            overallSlotNumber++;
        }

        return publisherSlots;
    }

    public PublisherGame? GetPublisherGame(MasterGame masterGame) => GetPublisherGameByMasterGameID(masterGame.MasterGameID);
    public PublisherGame GetPublisherGameOrThrow(MasterGame masterGame)
    {
        var publisherGame = GetPublisherGameByMasterGameID(masterGame.MasterGameID);
        if (publisherGame is null)
        {
            throw new Exception($"Publisher: {PublisherID} does not have master game: {masterGame.GameName}");
        }

        return publisherGame;
    }

    public PublisherGame? GetPublisherGameByMasterGameID(Guid masterGameID)
    {
        return PublisherGames.SingleOrDefault(x => x.MasterGame is not null && x.MasterGame.MasterGame.MasterGameID == masterGameID);
    }

    public PublisherGame? GetPublisherGameByPublisherGameID(Guid publisherGameID)
    {
        return PublisherGames.SingleOrDefault(x => x.PublisherGameID == publisherGameID);
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

    public Result CanDropGame(bool willRelease, LeagueOptions leagueOptions)
    {
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

    public static Publisher GetFakePublisher(LeagueYearKey leagueYearKey)
    {
        return new Publisher(Guid.Empty, leagueYearKey, FantasyCriticUser.GetFakeUser(), "<Unknown Publisher>",
            null, 0, new List<PublisherGame>(),
            new List<FormerPublisherGame>(), 0, 0, 0, 0, false);
    }
}
