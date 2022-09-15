using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.SharedSerialization;

namespace FantasyCritic.Test.TestData;
public static class TestDataService
{
    public static SystemWideValues GetSystemWideValues()
    {
        using var reader = new StreamReader("../../../TestData/AveragePositionPoints.csv");
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var averagePickPositionPointsEntities = csv.GetRecords<AveragePositionPointsEntity>().ToList();
        var domains = averagePickPositionPointsEntities.Select(x => x.ToDomain());
        return new SystemWideValues(7.873290541m, 6.921084619m, -4.054802347m, domains);
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
