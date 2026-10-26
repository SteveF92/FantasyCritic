using System;
using System.Collections.Generic;
using System.Linq;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Test;

public static class TestSlotCreationUtilities
{
    public static IReadOnlyList<LeagueSlot> GetBasicSlots(int standardGames, int counterPicks)
    {
        var standardGameSlots = Enumerable.Range(1, standardGames).Select(x => new LeagueSlot(Guid.NewGuid(), false, x, null, [])).ToList();
        var counterPickSlots = Enumerable.Range(1, counterPicks).Select(x => new LeagueSlot(Guid.NewGuid(), true, x, null, [])).ToList();
        return [.. standardGameSlots, .. counterPickSlots];
    }
}
