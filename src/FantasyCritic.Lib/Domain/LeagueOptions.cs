using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Extensions;

namespace FantasyCritic.Lib.Domain;

public class LeagueOptions
{
    public LeagueOptions(int unrestrictedReleaseStatusDroppableGames, int willNotReleaseDroppableGames,
        int willReleaseDroppableGames, bool dropOnlyDraftGames, bool grantSuperDrops, bool counterPicksBlockDrops, bool allowMoveIntoIneligible, int minimumBidAmount,
        bool enableBids, IEnumerable<LeagueTagStatus> leagueTags, IEnumerable<LeagueSlot> leagueSlots, PickupSystem pickupSystem,
        ScoringSystem scoringSystem, TradingSystem tradingSystem, TiebreakSystem tiebreakSystem, ReleaseSystem releaseSystem,
        IneligibleGameSystem ineligibleGameSystem,
        AnnualDate counterPickDeadline, AnnualDate? mightReleaseDroppableDate, bool bidsOnlyBeforeNextScheduledDraft)
    {
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
        LeagueSlots = leagueSlots.OrderBy(x => x.CounterPick).ThenBy(x => x.SlotNumber).ToList();
        PickupSystem = pickupSystem;
        ScoringSystem = scoringSystem;
        TradingSystem = tradingSystem;
        TiebreakSystem = tiebreakSystem;
        ReleaseSystem = releaseSystem;
        IneligibleGameSystem = ineligibleGameSystem;
        CounterPickDeadline = counterPickDeadline;
        MightReleaseDroppableDate = mightReleaseDroppableDate;
        BidsOnlyBeforeNextScheduledDraft = bidsOnlyBeforeNextScheduledDraft;

        LeagueSlotDictionary = LeagueSlots.ToDictionary(x => x.LeagueSlotID);
        StandardGames = LeagueSlots.Count(x => !x.CounterPick);
        CounterPicks = LeagueSlots.Count(x => x.CounterPick);

    }

    public LeagueOptions(LeagueYearParameters parameters)
    {
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
        LeagueSlots = parameters.LeagueSlots.OrderBy(x => x.CounterPick).ThenBy(x => x.SlotNumber).ToList();
        PickupSystem = parameters.PickupSystem;
        ScoringSystem = parameters.ScoringSystem;
        TradingSystem = parameters.TradingSystem;
        TiebreakSystem = parameters.TiebreakSystem;
        ReleaseSystem = parameters.ReleaseSystem;
        IneligibleGameSystem = parameters.IneligibleGameSystem;
        CounterPickDeadline = parameters.CounterPickDeadline;
        MightReleaseDroppableDate = parameters.MightReleaseDroppableDate;
        BidsOnlyBeforeNextScheduledDraft = parameters.BidsOnlyBeforeNextScheduledDraft;

        LeagueSlotDictionary = LeagueSlots.ToDictionary(x => x.LeagueSlotID);
        StandardGames = LeagueSlots.Count(x => !x.CounterPick);
        CounterPicks = LeagueSlots.Count(x => x.CounterPick);
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
    public IReadOnlyList<LeagueSlot> LeagueSlots { get; }
    public PickupSystem PickupSystem { get; }
    public ScoringSystem ScoringSystem { get; }
    public TradingSystem TradingSystem { get; }
    public TiebreakSystem TiebreakSystem { get; }
    public ReleaseSystem ReleaseSystem { get; }
    public IneligibleGameSystem IneligibleGameSystem { get; }
    public AnnualDate CounterPickDeadline { get; }
    public AnnualDate? MightReleaseDroppableDate { get; }
    public bool BidsOnlyBeforeNextScheduledDraft { get; }
    public IReadOnlyList<LeagueSlot> SpecialGameSlots => LeagueSlots.Where(x => x.RequiredTags.Any()).ToList();
    public bool HasSpecialSlots => SpecialGameSlots.Any();

    public IReadOnlyDictionary<Guid, LeagueSlot> LeagueSlotDictionary { get; }

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

        var requiredSpecialSlotTags = LeagueSlots.SelectMany(x => x.RequiredTags).Distinct().ToList();
        if (requiredSpecialSlotTags.Any(x => x.SystemTagOnly))
        {
            return Result.Failure("You cannot have a slot that requires a 'system only' tag. These are for behind the scenes use only.");
        }

        var counterPickSlotsWithRequiredTags = LeagueSlots.Where(x => x.CounterPick && x.RequiredTags.Any()).ToList();
        if (counterPickSlotsWithRequiredTags.Any())
        {
            return Result.Failure("Counter pick slots cannot require specific tags.");
        }

        var slotDuplicates = LeagueSlots
            .GroupBy(x => (x.CounterPick, x.SlotNumber))
            .Where(x => x.Count() > 1)
            .ToList();
        if (slotDuplicates.Any())
        {
            return Result.Failure("Some of the slots specified have duplicate positions, which is invalid.");
        }

        return Result.Success();
    }

    public LeagueOptionsDifferences GetDifferences(LeagueOptions existingOptions)
    {
        List<string> differences = [];

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

        if (BidsOnlyBeforeNextScheduledDraft != existingOptions.BidsOnlyBeforeNextScheduledDraft)
        {
            differences.Add($"'Bids Only Before Next Scheduled Draft' changed from '{existingOptions.BidsOnlyBeforeNextScheduledDraft.ToYesNoString()}' to '{BidsOnlyBeforeNextScheduledDraft.ToYesNoString()}'.");
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

        differences.AddRange(LeagueSlotDifferenceFunctions.GetSlotDifferences(existingOptions.LeagueSlots, LeagueSlots));

        return new LeagueOptionsDifferences(differences);
    }

    public LeagueOptions UpdateOptionsForYear(int requestYear)
    {
        var newScoringSystem = ScoringSystem.SupportedInYear(requestYear) ? ScoringSystem : ScoringSystem.GetDefaultScoringSystem(requestYear);
        LeagueOptions options = new LeagueOptions(UnrestrictedReleaseStatusDroppableGames,
            WillNotReleaseDroppableGames, WillReleaseDroppableGames, DropOnlyDraftGames, GrantSuperDrops, CounterPicksBlockDrops,
            AllowMoveIntoIneligible, MinimumBidAmount, EnableBids, LeagueTags, LeagueSlots,
            PickupSystem, newScoringSystem, TradingSystem, TiebreakSystem, ReleaseSystem, IneligibleGameSystem,
            CounterPickDeadline, MightReleaseDroppableDate, BidsOnlyBeforeNextScheduledDraft);
        return options;
    }

    public NewDraftLeagueSettingsChanges? WithNewDraftOptions(CreateLeagueDraftParameters newParameters, Instant currentTimestamp)
    {
        var newSlots = new List<LeagueSlot>();

        int standardSlotNumber = StandardGames;
        var newStandardSlots = newParameters.AdditionalSlots
            .Where(x => !x.CounterPick)
            .OrderBy(x => x.SlotNumberWithinNewDraft)
            .ToList();
        foreach (var newStandardSlot in newStandardSlots)
        {
            var numberedNewSlot = new LeagueSlot(Guid.NewGuid(), false, standardSlotNumber, newStandardSlot.SlotName, newStandardSlot.RequiredTags);
            newSlots.Add(numberedNewSlot);
            standardSlotNumber++;
        }

        int counterPickSlotNumber = CounterPicks;
        var newCounterPickSlots = newParameters.AdditionalSlots
            .Where(x => x.CounterPick)
            .OrderBy(x => x.SlotNumberWithinNewDraft)
            .ToList();
        foreach (var newCounterPickSlot in newCounterPickSlots)
        {
            var numberedNewSlot = new LeagueSlot(Guid.NewGuid(), true, counterPickSlotNumber, newCounterPickSlot.SlotName, newCounterPickSlot.RequiredTags);
            newSlots.Add(numberedNewSlot);
            counterPickSlotNumber++;
        }

        var allSlots = LeagueSlots.Concat(newSlots).ToList();

        LeagueOptions newOptions = new LeagueOptions(UnrestrictedReleaseStatusDroppableGames,
            WillNotReleaseDroppableGames, WillReleaseDroppableGames, DropOnlyDraftGames, GrantSuperDrops, CounterPicksBlockDrops,
            AllowMoveIntoIneligible, MinimumBidAmount, EnableBids, LeagueTags, allSlots,
            PickupSystem, ScoringSystem, TradingSystem, TiebreakSystem, ReleaseSystem, IneligibleGameSystem,
            CounterPickDeadline, MightReleaseDroppableDate, BidsOnlyBeforeNextScheduledDraft);

        var differences = newOptions.GetDifferences(this);
        if (!differences.HasChanges)
        {
            return null;
        }

        var settingsChangeAction = new LeagueManagerAction(newParameters.LeagueYearKey, currentTimestamp, "League Year Settings Changed While Adding New Draft", differences.ToString());
        var settingsChanges = new NewDraftLeagueSettingsChanges(newOptions, newSlots, settingsChangeAction);
        return settingsChanges;
    }
}
