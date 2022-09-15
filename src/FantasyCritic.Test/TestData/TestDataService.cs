using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Identity;
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
        using var reader = new StreamReader("../../../TestData/MasterGameYears.csv");
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var masterGameYearEntities = csv.GetRecords<MasterGameYearEntity>().ToList();
        var tagAssociations = GetTagAssociations();
        var tagLookup = tagAssociations.ToLookup(x => x.MasterGameID);
        var domainTags = GetTags();

        var domains = new List<MasterGameYear>();
        foreach (var entity in masterGameYearEntities)
        {
            var tagAssociationsForGame = tagLookup[entity.MasterGameID];
            var tagNames = tagAssociationsForGame.Select(x => x.TagName).ToHashSet();
            var tagsForGame = domainTags.Where(x => tagNames.Contains(x.Name));
            domains.Add(entity.ToDomain(new List<MasterSubGame>(), tagsForGame, FantasyCriticUser.GetFakeUser()));
        }

        return domains.ToDictionary(x => x.MasterGame.MasterGameID);
    }

    private static IReadOnlyList<MasterGameHasTagEntity> GetTagAssociations()
    {
        return new List<MasterGameHasTagEntity>();
    }

    private static IReadOnlyList<MasterGameTag> GetTags()
    {
        return new List<MasterGameTag>();
    }
}
