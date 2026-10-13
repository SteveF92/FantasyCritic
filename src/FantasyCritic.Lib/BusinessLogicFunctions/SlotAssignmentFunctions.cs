namespace FantasyCritic.Lib.BusinessLogicFunctions;

public static class SlotAssignmentFunctions
{
    /// <summary>
    /// Finds a new slot for every game whose slot does not survive into the new league options, i.e. its slot ID is
    /// gone (or, defensively, now has the other counter-pick flag). Games whose slot survives never move, even if the
    /// slot's tags changed.
    /// Each displaced game, taken in its current slot order, goes to an open slot of its own kind: one it is eligible
    /// in if possible, preferring a regular slot over a tag-restricted one, then the lowest slot number. If it is
    /// eligible nowhere it still takes an open slot, as the old compaction did.
    /// The caller must already have refused a roster smaller than some publisher's game count, so an open slot
    /// always exists.
    /// Returns an empty dictionary when no game is displaced.
    /// </summary>
    public static IReadOnlyDictionary<PublisherGame, LeagueSlot> GetNewSlotAssignments(LeagueYear currentLeagueYear, LeagueYear editedLeagueYear)
    {
        Dictionary<PublisherGame, LeagueSlot> slotAssignments = [];

        bool KeepsSlot(PublisherGame game)
        {
            var newSlot = editedLeagueYear.Options.LeagueSlotDictionary.GetValueOrDefault(game.LeagueSlotID);
            return newSlot is not null && newSlot.CounterPick == game.CounterPick;
        }

        foreach (var publisher in editedLeagueYear.Publishers)
        {
            var displacedGames = publisher.PublisherGames
                .Where(x => !KeepsSlot(x))
                .OrderBy(x => currentLeagueYear.Options.LeagueSlotDictionary[x.LeagueSlotID].SlotNumber)
                .ToList();
            if (!displacedGames.Any())
            {
                continue;
            }

            var filledSlotIDs = publisher.PublisherGames.Where(KeepsSlot).Select(x => x.LeagueSlotID).ToHashSet();
            var openSlots = editedLeagueYear.Options.LeagueSlots.Where(x => !filledSlotIDs.Contains(x.LeagueSlotID)).ToList();
            foreach (var game in displacedGames)
            {
                var newSlot = openSlots
                    .Where(x => x.CounterPick == game.CounterPick)
                    .OrderBy(x => !new PublisherSlot(x, game, null).SlotIsValid(editedLeagueYear))
                    .ThenBy(x => x.RequiredTags.Any())
                    .ThenBy(x => x.SlotNumber)
                    .FirstOrDefault();
                if (newSlot is null)
                {
                    throw new Exception($"No open slot for publisher game {game.PublisherGameID}. A roster smaller than a publisher's games should have been refused.");
                }

                slotAssignments[game] = newSlot;
                openSlots.Remove(newSlot);
            }
        }

        return slotAssignments;
    }
}
