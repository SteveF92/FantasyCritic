using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.SharedSerialization.Database;
using FantasyCritic.Lib.Utilities;
using NodaTime;

namespace FantasyCritic.FakeRepo.TestUtilities;
public class TestDataService
{
    private readonly string _basePath;
    private readonly bool _defaultAllowIneligible;

    public TestDataService(string basePath, bool defaultAllowIneligible)
    {
        _basePath = basePath;
        _defaultAllowIneligible = defaultAllowIneligible;
    }

    public SystemWideValues GetSystemWideValues()
    {
        using var reader = new StreamReader(Path.Combine(_basePath, "AveragePositionPoints.csv"));
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var averagePickPositionPointsEntities = csv.GetRecords<AveragePositionPointsEntity>().ToList();
        var domains = averagePickPositionPointsEntities.Select(x => x.ToDomain());
        var bidAmounts = new List<AverageBidAmountPoints>();
        return new SystemWideValues(7.873290541m, 6.921084619m, -4.054802347m, domains, bidAmounts);
    }

    public IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> GetActiveBids(IReadOnlyList<LeagueYear> leagueYears, IReadOnlyDictionary<Guid, MasterGameYear> masterGameYears)
    {
        var pickupBidEntities = GetPickupBidEntities();
        var bidLookup = pickupBidEntities.ToLookup(x => x.PublisherID);
        Dictionary<LeagueYear, List<PickupBid>> domainBidsDictionary = new Dictionary<LeagueYear, List<PickupBid>>();
        foreach (var leagueYear in leagueYears)
        {
            Dictionary<Guid, PublisherGame> publisherGamesDictionary = leagueYear.Publishers
                .SelectMany(x => x.PublisherGames)
                .Where(x => x.MasterGame is not null && !x.CounterPick)
                .ToDictionary(x => x.MasterGame!.MasterGame.MasterGameID);

            List<PickupBid> domainBidsForLeague = new List<PickupBid>();
            foreach (var publisher in leagueYear.Publishers)
            {
                var bidsForPublisher = bidLookup[publisher.PublisherID].ToList();
                foreach (var bid in bidsForPublisher)
                {
                    PublisherGame? conditionalDropPublisherGame = publisherGamesDictionary.GetValueOrDefaultNullable(bid.ConditionalDropMasterGameID);
                    var domainBid = bid.ToDomain(publisher, masterGameYears[bid.MasterGameID].MasterGame, conditionalDropPublisherGame, leagueYear);
                    domainBidsForLeague.Add(domainBid);
                }
            }
            domainBidsDictionary.Add(leagueYear, domainBidsForLeague);
        }

        return domainBidsDictionary.SealDictionary();
    }

    public IReadOnlyDictionary<LeagueYear, IReadOnlyList<DropRequest>> GetActiveDrops(IReadOnlyList<LeagueYear> leagueYears, IReadOnlyDictionary<Guid, MasterGameYear> masterGameYears)
    {
        var dropRequestEntities = GetDropRequestEntities();
        var dropLookup = dropRequestEntities.ToLookup(x => x.PublisherID);
        Dictionary<LeagueYear, List<DropRequest>> domainBidsDictionary = new Dictionary<LeagueYear, List<DropRequest>>();
        foreach (var leagueYear in leagueYears)
        {
            List<DropRequest> domainBidsForLeague = new List<DropRequest>();
            foreach (var publisher in leagueYear.Publishers)
            {
                var dropsForPublisher = dropLookup[publisher.PublisherID].ToList();
                foreach (var drop in dropsForPublisher)
                {
                    var domainDrop = drop.ToDomain(publisher, masterGameYears[drop.MasterGameID].MasterGame, leagueYear);
                    domainBidsForLeague.Add(domainDrop);
                }
            }
            domainBidsDictionary.Add(leagueYear, domainBidsForLeague);
        }

        return domainBidsDictionary.SealDictionary();
    }

    public IReadOnlyList<Publisher> GetPublishers(IReadOnlyDictionary<Guid, MasterGameYear> masterGameYears)
    {
        var publisherEntities = GetPublisherEntities();
        var publisherGameEntities = GetPublisherGamesEntities();
        var publisherGameLookup = publisherGameEntities.ToLookup(x => x.PublisherID);
        
        List<Publisher> domains = new List<Publisher>();
        foreach (var entity in publisherEntities)
        {
            var gameEntities = publisherGameLookup[entity.PublisherID];
            var domainGames = new List<PublisherGame>();
            foreach (var game in gameEntities)
            {
                MasterGameYear? masterGameYear = masterGameYears.GetValueOrDefaultNullable(game.MasterGameID);
                domainGames.Add(game.ToDomain(masterGameYear));
            }
            domains.Add(entity.ToDomain(domainGames, new List<FormerPublisherGame>()));
        }

        return domains;
    }

    public IReadOnlyDictionary<Guid, MasterGameYear> GetMasterGameYears()
    {
        using var reader = new StreamReader(Path.Combine(_basePath, "MasterGameYears.csv"));
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<MasterGameYearEntityMap>();
        var masterGameYearEntities = csv.GetRecords<MasterGameYearEntity>().ToList();
        var tagAssociations = GetTagAssociationEntities();
        var tagLookup = tagAssociations.ToLookup(x => x.MasterGameID);
        var domainTags = GetTags();

        var domains = new List<MasterGameYear>();
        foreach (var entity in masterGameYearEntities)
        {
            var tagAssociationsForGame = tagLookup[entity.MasterGameID];
            var tagNames = tagAssociationsForGame.Select(x => x.TagName).ToHashSet();
            var tagsForGame = domainTags.Where(x => tagNames.Contains(x.Name));
            domains.Add(entity.ToDomain(new List<MasterSubGame>(), tagsForGame, FantasyCriticUser.GetFakeUser().ToVeryMinimal()));
        }

        return domains.ToDictionary(x => x.MasterGame.MasterGameID);
    }

    public IReadOnlyList<LeagueYear> GetLeagueYears(IReadOnlyList<Publisher> publishers, IReadOnlyDictionary<Guid, MasterGameYear> masterGameYears, IReadOnlyList<MasterGameTag> tags)
    {
        var publisherLookup = publishers.ToLookup(x => x.LeagueYearKey);
        using var reader = new StreamReader(Path.Combine(_basePath, "LeagueYears.csv"));
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<LeagueYearEntityMap>();
        var leagueYearEntities = csv.GetRecords<LeagueYearEntity>().ToList();
        var tagDictionary = tags.ToDictionary(x => x.Name);

        var eligibilityOverrideEntities = GetEligibilityOverrideEntities();
        var tagOverrideEntities = GetTagOverrideEntities();
        var leagueTagEntities = GetLeagueTagStatusesEntities();
        var specialGameSlotEntities = GetSpecialGameSlotEntities();

        var eligibilityOverrideLookup = eligibilityOverrideEntities.ToLookup(x => new LeagueYearKey(x.LeagueID, x.Year));
        var tagOverrideEntityLookup = tagOverrideEntities.ToLookup(x => new LeagueYearKey(x.LeagueID, x.Year));
        var leagueTagEntityLookup = leagueTagEntities.ToLookup(x => new LeagueYearKey(x.LeagueID, x.Year));
        var specialGameSlotsDictionary = SpecialGameSlotEntity.ConvertSpecialGameSlotEntities(specialGameSlotEntities, tagDictionary);

        List<LeagueYear> domains = new List<LeagueYear>();
        foreach (var leagueYear in leagueYearEntities)
        {
            var leagueYearKey = new LeagueYearKey(leagueYear.LeagueID, leagueYear.Year);
            var league = new League(leagueYear.LeagueID, "LeagueName", FantasyCriticUser.GetFakeUser().ToMinimal(), null, null, new List<int>() {leagueYear.Year}, false, false, false, false, 0);
            var supportedYear = new SupportedYear(leagueYear.Year, true, true, true, new LocalDate(leagueYear.Year, 1, 1), false);

            var publishersInLeague = publisherLookup[leagueYearKey].ToList();
            var eligibilityOverrides = eligibilityOverrideLookup[leagueYearKey].Select(x => x.ToDomain(masterGameYears[x.MasterGameID].MasterGame));
            var tagOverrides = tagOverrideEntityLookup[leagueYearKey]
                .Where(x => masterGameYears.ContainsKey(x.MasterGameID))
                .GroupBy(x => x.MasterGameID)
                .Select(x => new TagOverride(masterGameYears[x.Key].MasterGame, tags.Where(t => x.Select(e => e.TagName).Contains(t.Name))));
            var leagueTags = leagueTagEntityLookup[leagueYearKey].Select(x => x.ToDomain(tagDictionary[x.Tag]));
            var specialGameSlots = specialGameSlotsDictionary[leagueYearKey];
            domains.Add(leagueYear.ToDomain(league, supportedYear, eligibilityOverrides, tagOverrides, leagueTags, specialGameSlots, null, publishersInLeague));
        }

        return domains;
    }

    private IReadOnlyList<PickupBidEntity> GetPickupBidEntities()
    {
        using var reader = new StreamReader(Path.Combine(_basePath, "PickupBids.csv"));
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        if (_defaultAllowIneligible)
        {
            csv.Context.RegisterClassMap<PreAllowIneligibleSlotPickupBidEntityMap>();
        }
        else
        {
            csv.Context.RegisterClassMap<PickupBidEntityMap>();
        }
        var pickupBids = csv.GetRecords<PickupBidEntity>().ToList();
        return pickupBids;
    }

    private IReadOnlyList<DropRequestEntity> GetDropRequestEntities()
    {
        using var reader = new StreamReader(Path.Combine(_basePath, "DropRequests.csv"));
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var dropRequests = csv.GetRecords<DropRequestEntity>().ToList();
        return dropRequests;
    }

    private IReadOnlyList<TestPublisherEntity> GetPublisherEntities()
    {
        using var reader = new StreamReader(Path.Combine(_basePath, "Publishers.csv"));
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var publishers = csv.GetRecords<TestPublisherEntity>().ToList();
        return publishers;
    }

    private IReadOnlyList<PublisherGameEntity> GetPublisherGamesEntities()
    {
        using var reader = new StreamReader(Path.Combine(_basePath, "PublisherGames.csv"));
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var publisherGames = csv.GetRecords<PublisherGameEntity>().ToList();
        return publisherGames;
    }

    private IReadOnlyList<MasterGameHasTagEntity> GetTagAssociationEntities()
    {
        using var reader = new StreamReader(Path.Combine(_basePath, "MasterGameHasTags.csv"));
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var tagAssociations = csv.GetRecords<MasterGameHasTagEntity>().ToList();
        return tagAssociations;
    }

    private IReadOnlyList<EligibilityOverrideEntity> GetEligibilityOverrideEntities()
    {
        using var reader = new StreamReader(Path.Combine(_basePath, "EligibilityOverrides.csv"));
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var eligibilityOverrides = csv.GetRecords<EligibilityOverrideEntity>().ToList();
        return eligibilityOverrides;
    }

    private IReadOnlyList<TagOverrideEntity> GetTagOverrideEntities()
    {
        using var reader = new StreamReader(Path.Combine(_basePath, "TagOverrides.csv"));
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var tagOverrides = csv.GetRecords<TagOverrideEntity>().ToList();
        return tagOverrides;
    }

    private IReadOnlyList<LeagueYearTagEntity> GetLeagueTagStatusesEntities()
    {
        using var reader = new StreamReader(Path.Combine(_basePath, "LeagueUsesTag.csv"));
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var tagStatuses = csv.GetRecords<LeagueYearTagEntity>().ToList();
        return tagStatuses;
    }

    private IReadOnlyList<SpecialGameSlotEntity> GetSpecialGameSlotEntities()
    {
        using var reader = new StreamReader(Path.Combine(_basePath, "SpecialGameSlots.csv"));
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var specialGameSlots = csv.GetRecords<SpecialGameSlotEntity>().ToList();
        return specialGameSlots;
    }

    public IReadOnlyList<MasterGameTag> GetTags()
    {
        using var reader = new StreamReader(Path.Combine(_basePath, "MasterGameTags.csv"));
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var tags = csv.GetRecords<MasterGameTagEntity>().ToList();
        var domains = tags.Select(x => x.ToDomain()).ToList();
        return domains;
    }
}
