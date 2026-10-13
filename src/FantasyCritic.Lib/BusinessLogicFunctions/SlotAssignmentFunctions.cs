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
    /// Eligibility is judged under the new options, with the current year's eligibility and tag overrides.
    /// The caller must already have refused a roster smaller than some publisher's game count, so an open slot
    /// always exists.
    /// Returns an empty dictionary when no game is displaced.
    /// </summary>
    public static IReadOnlyDictionary<PublisherGame, LeagueSlot> GetNewSlotAssignments(LeagueYear currentLeagueYear,
        LeagueOptions newLeagueOptions)
    {
        Dictionary<PublisherGame, LeagueSlot> slotAssignments = [];

        foreach (var publisher in currentLeagueYear.Publishers)
        {
            var displacedGames = publisher.PublisherGames
                .Where(x => !SlotForGameStillExists(newLeagueOptions, x))
                .OrderBy(x => currentLeagueYear.Options.LeagueSlotDictionary[x.LeagueSlotID].SlotNumber)
                .ToList();
            if (!displacedGames.Any())
            {
                continue;
            }

            var filledSlotIDs = publisher.PublisherGames.Where(x => SlotForGameStillExists(newLeagueOptions, x))
                .Select(x => x.LeagueSlotID).ToHashSet();
            var openSlots = newLeagueOptions.LeagueSlots.Where(x => !filledSlotIDs.Contains(x.LeagueSlotID)).ToList();
            foreach (var game in displacedGames)
            {
                var newSlot = openSlots
                    .Where(x => x.CounterPick == game.CounterPick)
                    .OrderBy(x => !IsEligibleInSlot(game, x, currentLeagueYear, newLeagueOptions))
                    .ThenBy(x => x.RequiredTags.Any())
                    .ThenBy(x => x.SlotNumber)
                    .FirstOrDefault();
                if (newSlot is null)
                {
                    throw new Exception(
                        $"No open slot for publisher game {game.PublisherGameID}. A roster smaller than a publisher's games should have been refused.");
                }

                slotAssignments[game] = newSlot;
                openSlots.Remove(newSlot);
            }
        }

        return slotAssignments;
    }

    private static bool SlotForGameStillExists(LeagueOptions leagueOptions, PublisherGame game)
    {
        var newSlot = leagueOptions.LeagueSlotDictionary.GetValueOrDefault(game.LeagueSlotID);
        return newSlot is not null && newSlot.CounterPick == game.CounterPick;
    }

    private static bool IsEligibleInSlot(PublisherGame game, LeagueSlot slot, LeagueYear currentLeagueYear, LeagueOptions newLeagueOptions)
    {
        var publisherSlot = new PublisherSlot(slot, game, null);
        var eligibilityFactors = currentLeagueYear.GetEligibilityFactorsForSlot(publisherSlot, newLeagueOptions);
        return eligibilityFactors is null || !SlotEligibilityFunctions.GetClaimErrorsForSlot(publisherSlot, eligibilityFactors).Any();
    }
}
