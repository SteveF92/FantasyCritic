namespace FantasyCritic.Lib.BusinessLogicFunctions;

public static class LeagueSlotDifferenceFunctions
{
    // One line per change, for the league manager action log. Slots are matched by ID, so editing a slot's tags or name
    // is reported as a change to that slot, not as one slot removed and another added. Kept and added slots are named
    // by their new number, removed slots by their old one, and a kept slot whose number changed says what it was.
    public static IReadOnlyList<string> GetSlotDifferences(IReadOnlyList<LeagueSlot> existingSlots, IReadOnlyList<LeagueSlot> newSlots)
    {
        var newSlotDictionary = newSlots.ToDictionary(x => x.LeagueSlotID);

        // A slot can't change kind and keep its ID (a game's slot reference includes the counter pick flag),
        // so a slot whose flag changed counts as removed and added.
        var keptSlots = existingSlots
            .Select(x => (Existing: x, New: newSlotDictionary.GetValueOrDefault(x.LeagueSlotID)))
            .Where(x => x.New is not null && x.New.CounterPick == x.Existing.CounterPick)
            .Select(x => new KeptSlot(x.Existing, x.New!))
            .ToList();
        var keptSlotIDs = keptSlots
            .Select(x => x.Existing.LeagueSlotID)
            .ToHashSet();
        var removedSlots = existingSlots
            .Where(x => !keptSlotIDs.Contains(x.LeagueSlotID))
            .OrderBy(x => x.CounterPick)
            .ThenBy(x => x.SlotNumber)
            .ToList();
        var addedSlots = newSlots
            .Where(x => !keptSlotIDs.Contains(x.LeagueSlotID))
            .OrderBy(x => x.CounterPick)
            .ThenBy(x => x.SlotNumber)
            .ToList();

        List<string> differences = [];
        foreach (var removedSlot in removedSlots)
        {
            differences.Add($"{GetSlotLabel(removedSlot)} removed{GetRequirementSuffix(removedSlot)}.");
        }

        foreach (var keptSlot in keptSlots.OrderBy(x => x.New.CounterPick).ThenBy(x => x.New.SlotNumber))
        {
            var existing = keptSlot.Existing;
            var updated = keptSlot.New;
            var label = GetSlotLabel(updated);
            if (existing.SlotNumber != updated.SlotNumber)
            {
                label += $" (was slot {existing.SlotNumber + 1})";
            }

            if (SlotWasReordered(keptSlot, keptSlots, removedSlots, addedSlots))
            {
                differences.Add($"{GetSlotLabel(existing)} moved to slot {updated.SlotNumber + 1}.");
            }

            if (!existing.RequiredTags.ToHashSet().SetEquals(updated.RequiredTags))
            {
                differences.Add($"{label} required tags changed from {GetTagsString(existing)} to {GetTagsString(updated)}.");
            }

            if (existing.SlotName != updated.SlotName)
            {
                differences.Add($"{label} name changed from {GetNameString(existing)} to {GetNameString(updated)}.");
            }
        }

        foreach (var addedSlot in addedSlots)
        {
            differences.Add($"{GetSlotLabel(addedSlot)} added{GetRequirementSuffix(addedSlot)}.");
        }

        return differences;
    }

    // Adding or removing a slot renumbers the slots after it, which isn't worth a line each. A move is a kept slot whose
    // place among the kept slots of its kind changed. With nothing of that kind added or removed, any new number is a move.
    private static bool SlotWasReordered(KeptSlot keptSlot, IReadOnlyList<KeptSlot> keptSlots,
        IReadOnlyList<LeagueSlot> removedSlots, IReadOnlyList<LeagueSlot> addedSlots)
    {
        if (keptSlot.Existing.SlotNumber == keptSlot.New.SlotNumber)
        {
            return false;
        }

        bool counterPick = keptSlot.Existing.CounterPick;
        bool slotsOfKindAddedOrRemoved = removedSlots.Concat(addedSlots).Any(x => x.CounterPick == counterPick);
        if (!slotsOfKindAddedOrRemoved)
        {
            return true;
        }

        var keptSlotsOfKind = keptSlots.Where(x => x.Existing.CounterPick == counterPick).ToList();
        var existingRank = keptSlotsOfKind.OrderBy(x => x.Existing.SlotNumber).ToList().IndexOf(keptSlot);
        var newRank = keptSlotsOfKind.OrderBy(x => x.New.SlotNumber).ToList().IndexOf(keptSlot);
        return existingRank != newRank;
    }

    private static string GetSlotLabel(LeagueSlot slot)
    {
        var kind = slot.CounterPick ? "Counter pick slot" : "Slot";
        return $"{kind} {slot.SlotNumber + 1}";
    }

    private static string GetRequirementSuffix(LeagueSlot slot)
    {
        List<string> parts = [];
        if (slot.SlotName is not null)
        {
            parts.Add($"named '{slot.SlotName}'");
        }

        if (slot.RequiredTags.Any())
        {
            parts.Add($"requiring {GetTagsString(slot)}");
        }

        return parts.Any() ? $" ({string.Join(", ", parts)})" : "";
    }

    private static string GetTagsString(LeagueSlot slot)
    {
        if (!slot.RequiredTags.Any())
        {
            return "none";
        }

        return string.Join(" or ", slot.RequiredTags.Select(x => x.ReadableName).Order().Select(x => $"'{x}'"));
    }

    private static string GetNameString(LeagueSlot slot) => slot.SlotName is null ? "none" : $"'{slot.SlotName}'";

    private sealed record KeptSlot(LeagueSlot Existing, LeagueSlot New);
}
