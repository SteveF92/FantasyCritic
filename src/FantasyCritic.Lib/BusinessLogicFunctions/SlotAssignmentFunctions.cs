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
    public static IReadOnlyDictionary<Guid, int> GetNewSlotAssignments(LeagueYear currentLeagueYear,
        LeagueOptions newLeagueOptions, IReadOnlyList<Publisher> publishers)
    {
        Dictionary<Guid, int> finalSlotAssignments = [];

        var newStandardGames = newLeagueOptions.StandardGames;
        var newSpecialSlotCount = newLeagueOptions.SpecialGameSlots.Count;
        var numberOfNewSpecialSlots = newSpecialSlotCount - currentLeagueYear.Options.SpecialGameSlots.Count;
        var slotCountShift = newStandardGames - currentLeagueYear.Options.StandardGames - numberOfNewSpecialSlots;

        if (slotCountShift == 0 && numberOfNewSpecialSlots >= 0)
        {
            return finalSlotAssignments;
        }

        bool KeepsSpecialSlot(PublisherSlot slot) => slot.SpecialGameSlot is not null && slot.SpecialGameSlot.SpecialSlotPosition < newSpecialSlotCount;

        foreach (var publisher in publishers)
        {
            Dictionary<Guid, int> slotAssignmentsForPublisher = [];
            var slots = publisher.GetPublisherSlots(currentLeagueYear);
            var filledNonCounterPickSlots = slots.Where(x => !x.CounterPick && x.PublisherGame is not null).ToList();

            int normalSlotNumber = 0;
            var normalSlots = filledNonCounterPickSlots.Where(x => !KeepsSpecialSlot(x));
            foreach (var normalSlot in normalSlots)
            {
                slotAssignmentsForPublisher[normalSlot.PublisherGame!.PublisherGameID] = normalSlotNumber;
                normalSlotNumber++;
            }

            var specialSlots = filledNonCounterPickSlots.Where(KeepsSpecialSlot);
            foreach (var specialSlot in specialSlots)
            {
                slotAssignmentsForPublisher[specialSlot.PublisherGame!.PublisherGameID] =
                    specialSlot.SlotNumber + slotCountShift;
            }

            bool invalidSlotsMade = slotAssignmentsForPublisher.GroupBy(x => x.Value).Any(x => x.Count() > 1)
                                    || slotAssignmentsForPublisher.Values.Any(x => x < 0 || x >= newStandardGames);
            if (invalidSlotsMade)
            {
                //If we cannot do the more advanced way to preserve slots, then just do the very basic thing, and line the games up.
                slotAssignmentsForPublisher = [];
                int allSlotNumber = 0;
                foreach (var slot in filledNonCounterPickSlots)
                {
                    slotAssignmentsForPublisher[slot.PublisherGame!.PublisherGameID] = allSlotNumber;
                    allSlotNumber++;
                }
            }

            foreach (var slot in slotAssignmentsForPublisher)
            {
                finalSlotAssignments[slot.Key] = slot.Value;
            }
        }

        return finalSlotAssignments;
    }
}
