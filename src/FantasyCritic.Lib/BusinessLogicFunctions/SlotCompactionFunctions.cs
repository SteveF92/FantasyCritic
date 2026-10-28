using FantasyCritic.Lib.Domain.Combinations;

namespace FantasyCritic.Lib.BusinessLogicFunctions;

public static class SlotCompactionFunctions
{
    /// <summary>
    /// Finds the moves that close the gaps in each publisher's roster after games are removed.
    /// A gap only means something when slots differ, so standard games are compacted only in leagues without
    /// tag-restricted slots; counter picks are always compacted. Compacted games take the lowest-numbered slots of
    /// their kind, standard games in the order they were acquired, then by current slot.
    /// Returns only the games that have to move.
    /// </summary>
    public static IReadOnlyDictionary<PublisherGame, LeagueSlot> GetCompactedSlotAssignments(IEnumerable<LeagueYearPublisherPair> publisherPairs)
    {
        Dictionary<PublisherGame, LeagueSlot> slotAssignments = [];
        foreach (var (leagueYear, publisher) in publisherPairs)
        {
            var slotDictionary = leagueYear.Options.LeagueSlotDictionary;
            if (!leagueYear.Options.HasSpecialSlots)
            {
                var standardGames = publisher.PublisherGames
                    .Where(x => !x.CounterPick)
                    .OrderBy(x => x.Timestamp).ThenBy(x => slotDictionary[x.LeagueSlotID].SlotNumber)
                    .ToList();
                AddMoves(standardGames, leagueYear.Options.LeagueSlots.Where(x => !x.CounterPick), publisher, slotAssignments);
            }

            var counterPicks = publisher.PublisherGames
                .Where(x => x.CounterPick)
                .OrderBy(x => slotDictionary[x.LeagueSlotID].SlotNumber)
                .ToList();
            AddMoves(counterPicks, leagueYear.Options.LeagueSlots.Where(x => x.CounterPick), publisher, slotAssignments);
        }

        return slotAssignments;
    }

    private static void AddMoves(IReadOnlyList<PublisherGame> orderedGames, IEnumerable<LeagueSlot> slotsOfKind, Publisher publisher,
        Dictionary<PublisherGame, LeagueSlot> slotAssignments)
    {
        var orderedSlots = slotsOfKind.OrderBy(x => x.SlotNumber).ToList();
        if (orderedGames.Count > orderedSlots.Count)
        {
            throw new Exception($"Publisher {publisher.PublisherID} has more games than slots of that kind.");
        }

        for (int i = 0; i < orderedGames.Count; i++)
        {
            if (orderedGames[i].LeagueSlotID != orderedSlots[i].LeagueSlotID)
            {
                slotAssignments[orderedGames[i]] = orderedSlots[i];
            }
        }
    }
}
