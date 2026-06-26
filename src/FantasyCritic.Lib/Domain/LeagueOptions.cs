using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Extensions;

namespace FantasyCritic.Lib.Domain;

public class LeagueOptions
{
    private readonly Dictionary<int, SpecialGameSlot> _specialSlotDictionary;

    public LeagueOptions(int standardGames, int counterPicks, int unrestrictedReleaseStatusDroppableGames, int willNotReleaseDroppableGames,
        int willReleaseDroppableGames, bool dropOnlyDraftGames, bool grantSuperDrops, bool counterPicksBlockDrops, bool allowMoveIntoIneligible, int minimumBidAmount,
        bool enableBids, IEnumerable<LeagueTagStatus> leagueTags, IEnumerable<SpecialGameSlot> specialGameSlots, DraftSystem draftSystem, PickupSystem pickupSystem,
        ScoringSystem scoringSystem, TradingSystem tradingSystem, TiebreakSystem tiebreakSystem, ReleaseSystem releaseSystem,
        IneligibleGameSystem ineligibleGameSystem,
        AnnualDate counterPickDeadline, AnnualDate? mightReleaseDroppableDate)
    {
        StandardGames = standardGames;
        CounterPicks = counterPicks;
        UnrestrictedReleaseStatusDroppableGames = unrestrictedReleaseStatusDroppableGames;
        WillNotReleaseDroppableGames = willNotReleaseDroppableGames;
        WillReleaseDroppableGames = willReleaseDroppableGames;
        DropOnlyDraftGames = dropOnlyDraftGames;
        GrantSuperDrops = grantSuperDrops;
        CounterPicksBlockDrops = counterPicksBlockDrops;
        AllowMoveIntoIneligible = allowMoveIntoIneligible;
        MinimumBidAmount = minimumBidAmount;
        EnableBids = enableBids;
        LeagueTags = leagueTags.ToList();
        SpecialGameSlots = specialGameSlots.OrderBy(x => x.SpecialSlotPosition).ToList();
        DraftSystem = draftSystem;
        PickupSystem = pickupSystem;
        ScoringSystem = scoringSystem;
        TradingSystem = tradingSystem;
        TiebreakSystem = tiebreakSystem;
        ReleaseSystem = releaseSystem;
        IneligibleGameSystem = ineligibleGameSystem;
        CounterPickDeadline = counterPickDeadline;
        MightReleaseDroppableDate = mightReleaseDroppableDate;

        _specialSlotDictionary = SpecialGameSlots.ToDictionary(specialGameSlot => StandardGames - SpecialGameSlots.Count + specialGameSlot.SpecialSlotPosition);
    }

    public LeagueOptions(LeagueYearParameters parameters)
    {
        StandardGames = parameters.StandardGames;
        CounterPicks = parameters.CounterPicks;
        UnrestrictedReleaseStatusDroppableGames = parameters.UnrestrictedReleaseStatusDroppableGames;
        WillNotReleaseDroppableGames = parameters.WillNotReleaseDroppableGames;
        WillReleaseDroppableGames = parameters.WillReleaseDroppableGames;
        DropOnlyDraftGames = parameters.DropOnlyDraftGames;
        GrantSuperDrops = parameters.GrantSuperDrops;
        CounterPicksBlockDrops = parameters.CounterPicksBlockDrops;
        AllowMoveIntoIneligible = parameters.AllowMoveIntoIneligible;
        MinimumBidAmount = parameters.MinimumBidAmount;
        EnableBids = parameters.EnableBids;
        LeagueTags = parameters.LeagueTags;
        SpecialGameSlots = parameters.SpecialGameSlots.OrderBy(x => x.SpecialSlotPosition).ToList();
        DraftSystem = parameters.DraftSystem;
        PickupSystem = parameters.PickupSystem;
        ScoringSystem = parameters.ScoringSystem;
        TradingSystem = parameters.TradingSystem;
        TiebreakSystem = parameters.TiebreakSystem;
        ReleaseSystem = parameters.ReleaseSystem;
        IneligibleGameSystem = parameters.IneligibleGameSystem;
        CounterPickDeadline = parameters.CounterPickDeadline;
        MightReleaseDroppableDate = parameters.MightReleaseDroppableDate;

        _specialSlotDictionary = SpecialGameSlots.ToDictionary(specialGameSlot => StandardGames - SpecialGameSlots.Count + specialGameSlot.SpecialSlotPosition);
    }

    public int StandardGames { get; }
    public int CounterPicks { get; }
    public int UnrestrictedReleaseStatusDroppableGames { get; }
    public int WillNotReleaseDroppableGames { get; }
    public int WillReleaseDroppableGames { get; }
    public bool DropOnlyDraftGames { get; }
    public bool GrantSuperDrops { get; }
    public bool CounterPicksBlockDrops { get; }
    public bool AllowMoveIntoIneligible { get; }
    public int MinimumBidAmount { get; }
    public bool EnableBids { get; }
    public IReadOnlyList<LeagueTagStatus> LeagueTags { get; }
    public IReadOnlyList<SpecialGameSlot> SpecialGameSlots { get; }
    public DraftSystem DraftSystem { get; }
    public PickupSystem PickupSystem { get; }
    public ScoringSystem ScoringSystem { get; }
    public TradingSystem TradingSystem { get; }
    public TiebreakSystem TiebreakSystem { get; }
    public ReleaseSystem ReleaseSystem { get; }
    public IneligibleGameSystem IneligibleGameSystem { get; }
    public AnnualDate CounterPickDeadline { get; }
    public AnnualDate? MightReleaseDroppableDate { get; }
    public bool HasSpecialSlots => SpecialGameSlots.Any();

    public SpecialGameSlot? GetSpecialGameSlotByOverallSlotNumber(int slotNumber) => _specialSlotDictionary.GetValueOrDefault(slotNumber);

    public Result Validate()
    {
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

    public LeagueOptionsDifferences GetDifferences(LeagueOptions existingOptions)
    {
        List<string> differences = new List<string>();

        if (StandardGames != existingOptions.StandardGames)
        {
            differences.Add($"Number of standard games per publisher changed from {existingOptions.StandardGames} to {StandardGames}.");
        }

        if (CounterPicks != existingOptions.CounterPicks)
        {
            differences.Add($"Number of counter picks per publisher changed from {existingOptions.CounterPicks} to {CounterPicks}.");
        }

        if (EnableBids != existingOptions.EnableBids)
        {
            differences.Add($"'Enable Bids' changed from '{existingOptions.EnableBids.ToYesNoString()}' to '{EnableBids.ToYesNoString()}'.");
        }

        if (UnrestrictedReleaseStatusDroppableGames != existingOptions.UnrestrictedReleaseStatusDroppableGames)
        {
            differences.Add($"Number of 'any unreleased' droppable games per publisher changed from {existingOptions.UnrestrictedReleaseStatusDroppableGames.ToDroppableString()} to {UnrestrictedReleaseStatusDroppableGames.ToDroppableString()}.");
        }

        if (WillNotReleaseDroppableGames != existingOptions.WillNotReleaseDroppableGames)
        {
            differences.Add($"Number of 'will not release' droppable games per publisher changed from {existingOptions.WillNotReleaseDroppableGames.ToDroppableString()} to {WillNotReleaseDroppableGames.ToDroppableString()}.");
        }

        if (WillReleaseDroppableGames != existingOptions.WillReleaseDroppableGames)
        {
            differences.Add($"Number of 'will release' droppable games per publisher changed from {existingOptions.WillReleaseDroppableGames.ToDroppableString()} to {WillReleaseDroppableGames.ToDroppableString()}.");
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

        if (!ReleaseSystem.Equals(existingOptions.ReleaseSystem))
        {
            differences.Add($"Game Release Rule changed from {existingOptions.ReleaseSystem.ReadableName} to {ReleaseSystem.ReadableName}.");
        }

        if (!IneligibleGameSystem.Equals(existingOptions.IneligibleGameSystem))
        {
            differences.Add($"Ineligible Game Rule changed from {existingOptions.IneligibleGameSystem.ReadableName} to {IneligibleGameSystem.ReadableName}.");
        }

        if (!CounterPickDeadline.Equals(existingOptions.CounterPickDeadline))
        {
            differences.Add($"Counter pick deadline changed from {existingOptions.CounterPickDeadline.ToReadableString()} to {CounterPickDeadline.ToReadableString()}.");
        }

        if (!Equals(MightReleaseDroppableDate, existingOptions.MightReleaseDroppableDate))
        {
            differences.Add($"'Might Release Droppable Date' changed from {existingOptions.MightReleaseDroppableDate.ToReadableString()} to {MightReleaseDroppableDate.ToReadableString()}.");
        }

        if (GrantSuperDrops != existingOptions.GrantSuperDrops)
        {
            differences.Add($"'Grant Super Drops' changed from {existingOptions.GrantSuperDrops.ToYesNoString()} to {GrantSuperDrops.ToYesNoString()}.");
        }

        if (AllowMoveIntoIneligible != existingOptions.AllowMoveIntoIneligible)
        {
            differences.Add($"'Allow Move Into Ineligible' changed from {existingOptions.AllowMoveIntoIneligible.ToYesNoString()} to {AllowMoveIntoIneligible.ToYesNoString()}.");
        }

        var orderedExistingTags = existingOptions.LeagueTags.OrderBy(t => t.Tag.Name).ToList();
        var orderedNewTags = LeagueTags.OrderBy(t => t.Tag.Name).ToList();
        if (!orderedNewTags.SequenceEqual(orderedExistingTags))
        {
            differences.Add($"Banned tags changed from {string.Join(",", orderedExistingTags.Select(x => x.Tag.ReadableName))} to {string.Join(",", orderedNewTags.Select(x => x.Tag.ReadableName))}.");
        }

        var orderedExistingSpecialSlots = existingOptions.SpecialGameSlots.OrderBy(t => t.SpecialSlotPosition).ToList();
        var orderedNewSpecialGameSlots = SpecialGameSlots.OrderBy(t => t.SpecialSlotPosition).ToList();
        if (!orderedNewSpecialGameSlots.SequenceEqual(orderedExistingSpecialSlots))
        {
            differences.Add($"Special slots changed from \n {string.Join("\n", orderedExistingSpecialSlots.Select(x => x.ToString()))} \n TO \n {string.Join("\n", orderedNewSpecialGameSlots.Select(x => x.ToString()))}");
        }

        return new LeagueOptionsDifferences(differences);
    }

    public LeagueOptions UpdateOptionsForYear(int requestYear)
    {
        var newScoringSystem = ScoringSystem.SupportedInYear(requestYear) ? ScoringSystem : ScoringSystem.GetDefaultScoringSystem(requestYear);
        LeagueOptions options = new LeagueOptions(StandardGames, CounterPicks, UnrestrictedReleaseStatusDroppableGames,
            WillNotReleaseDroppableGames, WillReleaseDroppableGames, DropOnlyDraftGames, GrantSuperDrops, CounterPicksBlockDrops,
            AllowMoveIntoIneligible, MinimumBidAmount, EnableBids, LeagueTags, SpecialGameSlots, DraftSystem,
            PickupSystem, newScoringSystem, TradingSystem, TiebreakSystem, ReleaseSystem, IneligibleGameSystem,
            CounterPickDeadline, MightReleaseDroppableDate);
        return options;
    }

    public NewDraftLeagueSettingsChanges? WithNewDraftOptions(CreateLeagueDraftParameters newParameters, Instant currentTimestamp)
    {
        var totalStandardGames = StandardGames + newParameters.AdditionalStandardGames;
        var totalCounterPicks = CounterPicks + newParameters.AdditionalCounterPicks;

        // The incoming NewSpecialGameSlots carry positions relative to their own sub-list (0-based within
        // the selector component). Re-number them so they follow the existing slots.
        var existingSlotCount = SpecialGameSlots.Count;
        var renumberedNewSlots = newParameters.NewSpecialGameSlots
            .OrderBy(s => s.SpecialSlotPosition)
            .Select((s, i) => new SpecialGameSlot(existingSlotCount + i, s.Tags))
            .ToList();
        var totalSpecialGameSlots = SpecialGameSlots.Concat(renumberedNewSlots).ToList();

        LeagueOptions newOptions = new LeagueOptions(totalStandardGames, totalCounterPicks, UnrestrictedReleaseStatusDroppableGames,
            WillNotReleaseDroppableGames, WillReleaseDroppableGames, DropOnlyDraftGames, GrantSuperDrops, CounterPicksBlockDrops,
            AllowMoveIntoIneligible, MinimumBidAmount, EnableBids, LeagueTags, totalSpecialGameSlots, DraftSystem,
            PickupSystem, ScoringSystem, TradingSystem, TiebreakSystem, ReleaseSystem, IneligibleGameSystem,
            CounterPickDeadline, MightReleaseDroppableDate);

        var differences = newOptions.GetDifferences(this);
        if (!differences.HasChanges)
        {
            return null;
        }

        var settingsChangeAction = new LeagueManagerAction(newParameters.LeagueYearKey, currentTimestamp, "League Year Settings Changed While Adding New Draft", differences.ToString());
        var settingsChanges = new NewDraftLeagueSettingsChanges(totalStandardGames, totalCounterPicks, renumberedNewSlots, settingsChangeAction);
        return settingsChanges;
    }
}
