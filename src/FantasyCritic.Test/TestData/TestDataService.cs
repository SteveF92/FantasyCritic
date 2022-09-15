using System;
using System.Collections.Generic;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.LeagueActions;

namespace FantasyCritic.Test.TestData;
public static class TestDataService
{
    public static SystemWideValues GetSystemWideValues()
    {
        return new SystemWideValues(0m, 0m, 0m, new List<AveragePickPositionPoints>());
    }

    public static IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> GetActiveBids()
    {
        return new Dictionary<LeagueYear, IReadOnlyList<PickupBid>>();
    }

    public static IReadOnlyDictionary<LeagueYear, IReadOnlyList<DropRequest>> GetActiveDrops()
    {
        return new Dictionary<LeagueYear, IReadOnlyList<DropRequest>>();
    }

    public static IReadOnlyList<Publisher> GetPublishers()
    {
        return new List<Publisher>();
    }

    public static IReadOnlyDictionary<Guid, MasterGameYear> GetMasterGameYears()
    {
        return new Dictionary<Guid, MasterGameYear>();
    }
}
