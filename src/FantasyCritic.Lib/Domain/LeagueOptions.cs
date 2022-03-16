using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Extensions;

namespace FantasyCritic.Lib.Domain;

public class LeagueOptions
{
    private readonly Dictionary<int, SpecialGameSlot> _specialSlotDictionary;

    public LeagueOptions(int standardGames, int gamesToDraft, int counterPicks, int counterPicksToDraft, int freeDroppableGames, int willNotReleaseDroppableGames, int willReleaseDroppableGames,
        bool dropOnlyDraftGames, bool counterPicksBlockDrops, int minimumBidAmount, IEnumerable<LeagueTagStatus> leagueTags, IEnumerable<SpecialGameSlot> specialGameSlots,
        DraftSystem draftSystem, PickupSystem pickupSystem, ScoringSystem scoringSystem, TradingSystem tradingSystem, TiebreakSystem tiebreakSystem)
    {
        StandardGames = standardGames;
        GamesToDraft = gamesToDraft;
        CounterPicks = counterPicks;
        CounterPicksToDraft = counterPicksToDraft;
        FreeDroppableGames = freeDroppableGames;
        WillNotReleaseDroppableGames = willNotReleaseDroppableGames;
        WillReleaseDroppableGames = willReleaseDroppableGames;
        DropOnlyDraftGames = dropOnlyDraftGames;
        CounterPicksBlockDrops = counterPicksBlockDrops;
        MinimumBidAmount = minimumBidAmount;
        LeagueTags = leagueTags.ToList();
        SpecialGameSlots = specialGameSlots.OrderBy(x => x.SpecialSlotPosition).ToList();
        DraftSystem = draftSystem;
        PickupSystem = pickupSystem;
        ScoringSystem = scoringSystem;
        TradingSystem = tradingSystem;
        TiebreakSystem = tiebreakSystem;

        _specialSlotDictionary = SpecialGameSlots.ToDictionary(specialGameSlot => StandardGames - SpecialGameSlots.Count + specialGameSlot.SpecialSlotPosition);
    }

    public LeagueOptions(LeagueCreationParameters parameters)
    {
        StandardGames = parameters.StandardGames;
        GamesToDraft = parameters.GamesToDraft;
        CounterPicks = parameters.CounterPicks;
        CounterPicksToDraft = parameters.CounterPicksToDraft;
        FreeDroppableGames = parameters.FreeDroppableGames;
        WillNotReleaseDroppableGames = parameters.WillNotReleaseDroppableGames;
        WillReleaseDroppableGames = parameters.WillReleaseDroppableGames;
        DropOnlyDraftGames = parameters.DropOnlyDraftGames;
        CounterPicksBlockDrops = parameters.CounterPicksBlockDrops;
        MinimumBidAmount = parameters.MinimumBidAmount;
        LeagueTags = parameters.LeagueTags;
        SpecialGameSlots = parameters.SpecialGameSlots.OrderBy(x => x.SpecialSlotPosition).ToList();
        DraftSystem = parameters.DraftSystem;
        PickupSystem = parameters.PickupSystem;
        ScoringSystem = parameters.ScoringSystem;
        TradingSystem = parameters.TradingSystem;
        TiebreakSystem = parameters.TiebreakSystem;
        PublicLeague = parameters.PublicLeague;

        _specialSlotDictionary = SpecialGameSlots.ToDictionary(specialGameSlot => StandardGames - SpecialGameSlots.Count + specialGameSlot.SpecialSlotPosition);
    }

    public LeagueOptions(EditLeagueYearParameters parameters, League league)
    {
        StandardGames = parameters.StandardGames;
        GamesToDraft = parameters.GamesToDraft;
        CounterPicks = parameters.CounterPicks;
        CounterPicksToDraft = parameters.CounterPicksToDraft;
        FreeDroppableGames = parameters.FreeDroppableGames;
        WillNotReleaseDroppableGames = parameters.WillNotReleaseDroppableGames;
        WillReleaseDroppableGames = parameters.WillReleaseDroppableGames;
        DropOnlyDraftGames = parameters.DropOnlyDraftGames;
        CounterPicksBlockDrops = parameters.CounterPicksBlockDrops;
        MinimumBidAmount = parameters.MinimumBidAmount;
        LeagueTags = parameters.LeagueTags;
        SpecialGameSlots = parameters.SpecialGameSlots.OrderBy(x => x.SpecialSlotPosition).ToList();
        DraftSystem = parameters.DraftSystem;
        PickupSystem = parameters.PickupSystem;
        ScoringSystem = parameters.ScoringSystem;
        TradingSystem = parameters.TradingSystem;
        TiebreakSystem = parameters.TiebreakSystem;

        _specialSlotDictionary = SpecialGameSlots.ToDictionary(specialGameSlot => StandardGames - SpecialGameSlots.Count + specialGameSlot.SpecialSlotPosition);
    }

    public int StandardGames { get; }
    public int GamesToDraft { get; }
    public int CounterPicks { get; }
    public int CounterPicksToDraft { get; }
    public int FreeDroppableGames { get; }
    public int WillNotReleaseDroppableGames { get; }
    public int WillReleaseDroppableGames { get; }
    public bool DropOnlyDraftGames { get; }
    public bool CounterPicksBlockDrops { get; }
    public int MinimumBidAmount { get; }
    public IReadOnlyList<LeagueTagStatus> LeagueTags { get; }
    public IReadOnlyList<SpecialGameSlot> SpecialGameSlots { get; }
    public DraftSystem DraftSystem { get; }
    public PickupSystem PickupSystem { get; }
    public ScoringSystem ScoringSystem { get; }
    public TradingSystem TradingSystem { get; }
    public TiebreakSystem TiebreakSystem { get; }
    public bool PublicLeague { get; }

    public bool HasSpecialSlots() => SpecialGameSlots.Any();

    public Maybe<SpecialGameSlot> GetSpecialGameSlotByOverallSlotNumber(int slotNumber)
    {
        var hasSpecialSlot = _specialSlotDictionary.TryGetValue(slotNumber, out var specialSlot);
        if (!hasSpecialSlot)
        {
            return Maybe<SpecialGameSlot>.None;
        }

        return specialSlot;
    }

    public Result Validate()
    {
        if (GamesToDraft > StandardGames)
        {
            return Result.Failure("Can't draft more than the total number of standard games.");
        }

        if (CounterPicksToDraft > CounterPicks)
        {
            return Result.Failure("Can't draft more counter picks than the total number of counter picks.");
        }

        if (CounterPicksToDraft > GamesToDraft)
        {
            return Result.Failure("Can't have more drafted counter picks than drafted games.");
        }

        var bannedTags = LeagueTags.Where(x => x.Status.Equals(TagStatus.Banned)).ToList();
        var bannedTagNames = bannedTags.Select(x => x.Tag.Name).ToHashSet();
        if (bannedTagNames.Contains("PlannedForEarlyAccess") && !bannedTagNames.Contains("CurrentlyInEarlyAccess"))
        {
            return Result.Failure("If you ban 'Planned for Early Access' you must also ban 'Currently in Early Access'. See the FAQ page for an explanation.");
        }
        if (bannedTagNames.Contains("WillReleaseInternationallyFirst") && !bannedTagNames.Contains("ReleasedInternationally"))
        {
            return Result.Failure("If you ban 'Will Release Internationally First' you must also ban 'Released Internationally'. See the FAQ page for an explanation.");
        }

        if (bannedTags.Any(x => x.Tag.SystemTagOnly))
        {
            return Result.Failure("You cannot ban 'system only' tags. These are for behind the scenes use only.");
        }

        var requiredSpecialSlotTags = SpecialGameSlots.SelectMany(x => x.Tags).Distinct().ToList();
        if (requiredSpecialSlotTags.Any(x => x.SystemTagOnly))
        {
            return Result.Failure("You cannot have a slot that requires a 'system only' tag. These are for behind the scenes use only.");
        }

        if (SpecialGameSlots.Count > StandardGames)
        {
            return Result.Failure("You cannot have more special game slots than the number of games per player.");
        }

        if (SpecialGameSlots.Any(x => !x.Tags.Any()))
        {
            return Result.Failure("All of your special slots must list at least one tag.");
        }

        if (SpecialGameSlots.Any())
        {
            var expectedSpecialSlotNumbers = Enumerable.Range(0, SpecialGameSlots.Count);
            var actualNumbers = SpecialGameSlots.Select(x => x.SpecialSlotPosition);
            if (!actualNumbers.SequenceEqual(expectedSpecialSlotNumbers))
            {
                return Result.Failure("Your special game slots have invalid positions. You should reload the page and try again. This shouldn't be possible.");
            }
        }

        return Result.Success();
    }

    public Maybe<string> GetDifferenceString(LeagueOptions existingOptions)
    {
        List<string> differences = new List<string>();

        if (StandardGames != existingOptions.StandardGames)
        {
            differences.Add($"Number of standard games per publisher changed from {existingOptions.StandardGames} to {StandardGames}.");
        }

        if (GamesToDraft != existingOptions.GamesToDraft)
        {
            differences.Add($"Number of games to draft per publisher changed from {existingOptions.GamesToDraft} to {GamesToDraft}.");
        }

        if (CounterPicks != existingOptions.CounterPicks)
        {
            differences.Add($"Number of counter picks per publisher changed from {existingOptions.CounterPicks} to {CounterPicks}.");
        }

        if (CounterPicksToDraft != existingOptions.CounterPicksToDraft)
        {
            differences.Add($"Number of counter picks to draft per publisher changed from {existingOptions.CounterPicksToDraft} to {CounterPicksToDraft}.");
        }

        if (FreeDroppableGames != existingOptions.FreeDroppableGames)
        {
            differences.Add($"Number of 'any unreleased' droppable games per publisher changed from {existingOptions.FreeDroppableGames} to {FreeDroppableGames}.");
        }

        if (WillNotReleaseDroppableGames != existingOptions.WillNotReleaseDroppableGames)
        {
            differences.Add($"Number of 'will not release' droppable games per publisher changed from {existingOptions.WillNotReleaseDroppableGames} to {WillNotReleaseDroppableGames}.");
        }

        if (WillReleaseDroppableGames != existingOptions.WillReleaseDroppableGames)
        {
            differences.Add($"Number of 'will release' droppable games per publisher changed from {existingOptions.WillReleaseDroppableGames} to {WillReleaseDroppableGames}.");
        }

        if (DropOnlyDraftGames != existingOptions.DropOnlyDraftGames)
        {
            differences.Add($"'Drop Only Drafted Games' from '{existingOptions.DropOnlyDraftGames.ToYesNoString()}' to '{DropOnlyDraftGames.ToYesNoString()}'.");
        }

        if (CounterPicksBlockDrops != existingOptions.CounterPicksBlockDrops)
        {
            differences.Add($"'Counter Picks Block Drops' from '{existingOptions.CounterPicksBlockDrops.ToYesNoString()}' to '{CounterPicksBlockDrops.ToYesNoString()}'.");
        }

        if (MinimumBidAmount != existingOptions.MinimumBidAmount)
        {
            differences.Add($"'Minimum Bid Amount' changed from {existingOptions.MinimumBidAmount} to {MinimumBidAmount}.");
        }

        if (!DraftSystem.Equals(existingOptions.DraftSystem))
        {
            differences.Add($"Draft System changed from {existingOptions.DraftSystem} to {DraftSystem}.");
        }

        if (!PickupSystem.Equals(existingOptions.PickupSystem))
        {
            differences.Add($"Pickup System changed from {existingOptions.PickupSystem.ReadableName} to {PickupSystem.ReadableName}.");
        }

        if (!TiebreakSystem.Equals(existingOptions.TiebreakSystem))
        {
            differences.Add($"Tiebreak System changed from {existingOptions.TiebreakSystem.Value.CamelCaseToSpaces()} to {TiebreakSystem.Value.CamelCaseToSpaces()}.");
        }

        if (!ScoringSystem.Equals(existingOptions.ScoringSystem))
        {
            differences.Add($"Scoring System changed from {existingOptions.ScoringSystem} to {ScoringSystem}.");
        }

        if (!TradingSystem.Equals(existingOptions.TradingSystem))
        {
            differences.Add($"Trading System changed from {existingOptions.TradingSystem.ReadableName} to {TradingSystem.ReadableName}.");
        }

        var orderedExistingTags = existingOptions.LeagueTags.OrderBy(t => t.Tag.Name).ToList();
        var orderedNewTags = LeagueTags.OrderBy(t => t.Tag.Name).ToList();
        if (!orderedNewTags.SequenceEqual(orderedExistingTags))
        {
            differences.Add($"Banned tags changed from {string.Join(",", orderedExistingTags.Select(x => x.Tag.ReadableName))} to {string.Join(",", orderedNewTags.Select(x => x.Tag.ReadableName))}.");
        }

        var orderedExistingSpecialSlots = existingOptions.SpecialGameSlots.OrderBy(t => t.SpecialSlotPosition).ToList();
        var orderedNewSpecialSlots = SpecialGameSlots.OrderBy(t => t.SpecialSlotPosition).ToList();
        if (!orderedNewSpecialSlots.SequenceEqual(orderedExistingSpecialSlots))
        {
            differences.Add($"Special slots changed from \n {string.Join("\n", orderedExistingSpecialSlots.Select(x => x.ToString()))} \n TO \n {string.Join("\n", orderedExistingSpecialSlots.Select(x => x.ToString()))}");
        }

        if (!differences.Any())
        {
            return Maybe<string>.None;
        }

        string finalString = string.Join("\n", differences.Select(x => $"• {x}"));
        return finalString;
    }

    public LeagueOptions UpdateOptionsForYear(int requestYear)
    {
        var newScoringSystem = ScoringSystem.GetDefaultScoringSystem(requestYear);
        LeagueOptions options = new LeagueOptions(StandardGames, GamesToDraft, CounterPicks, CounterPicksToDraft, FreeDroppableGames, WillNotReleaseDroppableGames, WillReleaseDroppableGames,
            DropOnlyDraftGames, CounterPicksBlockDrops, MinimumBidAmount, LeagueTags, SpecialGameSlots, DraftSystem, PickupSystem, newScoringSystem, TradingSystem, TiebreakSystem);
        return options;
    }
}
