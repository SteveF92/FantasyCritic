namespace FantasyCritic.Lib.BusinessLogicFunctions;

public static class SlotAssignmentFunctions
{
    /// <summary>
    /// Computes new slot numbers for every non-counter-pick game when the standard-game count or the special-slot
    /// count is changing.
    /// Games in a special slot whose position still exists are shifted forward by the net growth in normal
    /// (non-special) slot count — i.e. (newStandardGames - oldStandardGames) minus (newSpecialSlots - oldSpecialSlots) —
    /// so they land in the same special-slot position in the new configuration.
    /// Games in a special slot that is being removed join the normal games, which are compacted to 0, 1, 2 …
    /// in their current order.
    /// If that leaves two games in one slot, or a game past the last slot, every game is compacted in order instead.
    /// Returns an empty dictionary when no game needs to move: the normal slot count is unchanged (including the
    /// case where new special slots are added in equal number to new standard games) and no special slot is removed.
    /// </summary>
    public static IReadOnlyDictionary<PublisherGame, LeagueSlot> GetNewSlotAssignments(LeagueYear currentLeagueYear,
        LeagueOptions newLeagueOptions, IReadOnlyList<Publisher> publishers)
    {
        throw new NotImplementedException("I think this can be made much cleaner now.");
    }
}
