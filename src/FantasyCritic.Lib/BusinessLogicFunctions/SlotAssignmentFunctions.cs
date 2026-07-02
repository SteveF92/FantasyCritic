namespace FantasyCritic.Lib.BusinessLogicFunctions;

public static class SlotAssignmentFunctions
{
    /// <summary>
    /// Computes new slot numbers for every non-counter-pick game when the standard-game count is changing.
    /// Normal slots are compacted to 0, 1, 2 … in their current order.
    /// Games that currently occupy special slots are shifted forward by the same amount the slot count grew,
    /// so they stay inside the special-slot range in the new configuration.
    /// Returns an empty dictionary when the standard-game count is unchanged.
    /// </summary>
    public static IReadOnlyDictionary<Guid, int> GetNewSlotAssignments(
        int newStandardGames, LeagueYear currentLeagueYear, IReadOnlyList<Publisher> publishers)
    {
        var slotCountShift = newStandardGames - currentLeagueYear.Options.StandardGames;
        Dictionary<Guid, int> finalSlotAssignments = new Dictionary<Guid, int>();
        if (slotCountShift == 0)
        {
            return finalSlotAssignments;
        }

        foreach (var publisher in publishers)
        {
            Dictionary<Guid, int> slotAssignmentsForPublisher = new Dictionary<Guid, int>();
            var slots = publisher.GetPublisherSlots(currentLeagueYear);
            var filledNonCounterPickSlots = slots.Where(x => !x.CounterPick && x.PublisherGame is not null).ToList();

            int normalSlotNumber = 0;
            var normalSlots = filledNonCounterPickSlots.Where(x => x.SpecialGameSlot is null);
            foreach (var normalSlot in normalSlots)
            {
                slotAssignmentsForPublisher[normalSlot.PublisherGame!.PublisherGameID] = normalSlotNumber;
                normalSlotNumber++;
            }

            var specialSlots = filledNonCounterPickSlots.Where(x => x.SpecialGameSlot is not null);
            foreach (var specialSlot in specialSlots)
            {
                slotAssignmentsForPublisher[specialSlot.PublisherGame!.PublisherGameID] =
                    specialSlot.SlotNumber + slotCountShift;
            }

            bool invalidSlotsMade = slotAssignmentsForPublisher.GroupBy(x => x.Value).Any(x => x.Count() > 1);
            if (invalidSlotsMade)
            {
                //If we cannot do the more advanced way to preserve slots, then just do the very basic thing, and line the games up.
                slotAssignmentsForPublisher = new Dictionary<Guid, int>();
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
