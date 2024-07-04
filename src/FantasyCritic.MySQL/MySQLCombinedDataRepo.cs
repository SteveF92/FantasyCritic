using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.MySQL.Entities;
using System.Data;
using FantasyCritic.Lib.SharedSerialization.Database;
using FantasyCritic.MySQL.Entities.Conferences;
using FantasyCritic.MySQL.Entities.Trades;
using FantasyCritic.MySQL.Entities.Identity;

namespace FantasyCritic.MySQL;

//This class is part of an effort to optimize API calls that are called very frequently
//Before this class, all of this data was retrieved by separate functions, with separate, wasteful SQL calls.

public class MySQLCombinedDataRepo : ICombinedDataRepo
{
    private readonly IMasterGameRepo _masterGameRepo;

    private readonly string _connectionString;

    public MySQLCombinedDataRepo(RepositoryConfiguration configuration, IMasterGameRepo masterGameRepo)
    {
        _masterGameRepo = masterGameRepo;
        _connectionString = configuration.ConnectionString;
    }

    public async Task<BasicData> GetBasicData()
    {
        await using var connection = new MySqlConnection(_connectionString);
        await using var resultSets = await connection.QueryMultipleAsync("sp_getbasicdata", commandType: CommandType.StoredProcedure);

        var systemWideSettingsEntity = resultSets.ReadSingle<SystemWideSettingsEntity>();
        var tagEntities = resultSets.Read<MasterGameTagEntity>();
        var supportedYearEntities = resultSets.Read<SupportedYearEntity>();
        await resultSets.DisposeAsync();
        await connection.DisposeAsync();

        var systemWideSettings = new SystemWideSettings(systemWideSettingsEntity.ActionProcessingMode, systemWideSettingsEntity.RefreshOpenCritic);
        var tags = tagEntities.Select(x => x.ToDomain()).ToList();
        var supportedYears = supportedYearEntities.Select(x => x.ToDomain()).ToList();

        return new BasicData(systemWideSettings, tags, supportedYears);
    }

    public async Task<HomePageData> GetHomePageData(FantasyCriticUser currentUser)
    {
        var queryObject = new
        {
            P_UserID = currentUser.Id,
        };

        await using var connection = new MySqlConnection(_connectionString);
        await using var resultSets = await connection.QueryMultipleAsync("sp_gethomepagedata", queryObject, commandType: CommandType.StoredProcedure);

        var leagueEntities = resultSets.Read<LeagueEntity>();
        var leagueYearEntities = resultSets.Read<LeagueYearKeyEntity>();
        var inviteEntities = resultSets.Read<CompleteLeagueInviteEntity>();
        var conferenceEntities = resultSets.Read<MyConferenceEntity>();
        var yearEntities = resultSets.Read<ConferenceIDYearEntity>();
        var topBidsAndDropsEntities = resultSets.Read<TopBidsAndDropsEntity>();
        var masterGameResults = resultSets.Read<MasterGameYearEntity>();
        var tagResults = resultSets.Read<MasterGameTagEntity>();
        var masterSubGameResults = resultSets.Read<MasterSubGameEntity>();
        var masterGameTagResults = resultSets.Read<MasterGameHasTagEntity>();
        var myGameNewsEntities = resultSets.Read<MyGameNewsEntity>();
        var publicLeagueEntities = resultSets.Read<PublicLeagueYearStatsEntity>();
        var activeRoyaleYearQuarterEntity = resultSets.ReadSingle<RoyaleYearQuarterEntity>();
        var currentSupportedYearEntity = resultSets.ReadSingle<SupportedYearEntity>();
        var activeUserRoyalePublisherID = resultSets.ReadSingleOrDefault<Guid>();
        await resultSets.DisposeAsync();
        await connection.DisposeAsync();

        //MyLeagues
        var leagueYearLookup = leagueYearEntities.ToLookup(x => x.LeagueID);
        var leaguesWithStatus = new List<LeagueWithMostRecentYearStatus>();
        foreach (var leagueEntity in leagueEntities)
        {
            IEnumerable<int> years = leagueYearLookup[leagueEntity.LeagueID].Select(x => x.Year);
            League league = leagueEntity.ToDomain(years);
            leaguesWithStatus.Add(new LeagueWithMostRecentYearStatus(league, leagueEntity.UserIsInLeague, leagueEntity.UserIsFollowingLeague, leagueEntity.MostRecentYearOneShot));
        }

        //MyInvites
        var myInvites = inviteEntities.Select(x => x.ToDomain()).ToList();

        //MyConferences
        var conferenceDictionary = new Dictionary<Guid, List<int>>();
        foreach (var yearEntity in yearEntities)
        {
            if (!conferenceDictionary.TryGetValue(yearEntity.ConferenceID, out var years))
            {
                years = new List<int>();
                conferenceDictionary[yearEntity.ConferenceID] = years;
            }
            years.Add(yearEntity.Year);
        }

        var myConferences = conferenceEntities
            .Select(conference => conference.ToDomain(conferenceDictionary.GetValueOrDefault(conference.ConferenceID, new List<int>())))
            .ToList();

        //Top Bids and Drops
        var possibleTags = tagResults.Select(x => x.ToDomain()).ToDictionary(x => x.Name);
        var masterGameTagLookup = masterGameTagResults.ToLookup(x => x.MasterGameID);

        var masterSubGames = masterSubGameResults.Select(x => x.ToDomain()).ToList();
        var masterGameYears = new Dictionary<MasterGameYearKey, MasterGameYear>();
        foreach (var entity in masterGameResults)
        {
            var tags = masterGameTagLookup[entity.MasterGameID].Select(x => possibleTags[x.TagName]).ToList();
            var addedByUser = new VeryMinimalFantasyCriticUser(entity.AddedByUserID, entity.AddedByUserDisplayName);
            MasterGameYear domain = entity.ToDomain(masterSubGames.Where(sub => sub.MasterGameID == entity.MasterGameID), tags, addedByUser);
            masterGameYears.Add(domain.Key, domain);
        }

        TopBidsAndDropsData? topBidsAndDropsData = null;
        if (topBidsAndDropsEntities.Any())
        {
            var topBidsAndDrops = topBidsAndDropsEntities
                .Select(x => x.ToDomain(masterGameYears[new MasterGameYearKey(x.MasterGameID, x.Year)]))
                .ToList();
            topBidsAndDropsData = new TopBidsAndDropsData(topBidsAndDrops.First().ProcessDate, topBidsAndDrops);
        }

        //My Game News
        var myGameNews = MyGameNewsEntity.BuildMyGameNewsFromEntities(myGameNewsEntities, masterGameYears);

        //Public League Years
        var publicLeagueYears = publicLeagueEntities.Select(x => x.ToDomain()).ToList();

        //Active Royale Quarter
        var supportedYear = currentSupportedYearEntity.ToDomain();
        var activeRoyaleQuarter = activeRoyaleYearQuarterEntity.ToDomain(supportedYear);

        return new HomePageData(leaguesWithStatus, myInvites, myConferences, topBidsAndDropsData, publicLeagueYears, myGameNews, activeRoyaleQuarter, activeUserRoyalePublisherID);
    }

    public async Task<LeagueYear?> GetLeagueYear(Guid leagueID, int year)
    {
        var param = new
        {
            P_LeagueID = leagueID,
            P_Year = year
        };

        await using var connection = new MySqlConnection(_connectionString);
        await using var resultSets = await connection.QueryMultipleAsync("sp_getleagueyear", param, commandType: CommandType.StoredProcedure);

        var supportedYearEntity = resultSets.ReadSingle<SupportedYearEntity>();

        var leagueEntity = resultSets.ReadSingleOrDefault<LeagueEntity>();
        if (leagueEntity is null)
        {
            return null;
        }

        var years = resultSets.Read<int>();
        var leagueYearEntity = resultSets.ReadSingleOrDefault<LeagueYearEntity>();
        if (leagueYearEntity is null)
        {
            return null;
        }

        var leagueTagEntities = resultSets.Read<LeagueYearTagEntity>();
        var specialGameSlotEntities = resultSets.Read<SpecialGameSlotEntity>();
        var eligibilityOverrideEntities = resultSets.Read<EligibilityOverrideEntity>();
        var tagOverrideEntities = resultSets.Read<TagOverrideEntity>();
        var usersInLeagueEntities = resultSets.Read<FantasyCriticUserEntity>();
        var publisherEntities = resultSets.Read<PublisherEntity>();
        var publisherGameEntities = resultSets.Read<PublisherGameEntity>();
        var formerPublisherGameEntities = resultSets.Read<FormerPublisherGameEntity>();

        //MasterGame Results
        var masterGameResults = resultSets.Read<MasterGameEntity>();
        var tagResults = resultSets.Read<MasterGameTagEntity>();
        var masterSubGameResults = resultSets.Read<MasterSubGameEntity>();
        var masterGameTagResults = resultSets.Read<MasterGameHasTagEntity>();
        var masterGameYearResults = resultSets.Read<MasterGameYearEntity>();

        await resultSets.DisposeAsync();
        await connection.DisposeAsync();

        var possibleTags = tagResults.Select(x => x.ToDomain()).ToDictionary(x => x.Name);
        var masterGameTagLookup = masterGameTagResults.ToLookup(x => x.MasterGameID);

        var masterSubGames = masterSubGameResults.Select(x => x.ToDomain()).ToList();
        var masterGameDictionary = new Dictionary<Guid, MasterGame>();
        foreach (var entity in masterGameResults)
        {
            var tags = masterGameTagLookup[entity.MasterGameID].Select(x => possibleTags[x.TagName]).ToList();
            var addedByUser = new VeryMinimalFantasyCriticUser(entity.AddedByUserID, entity.AddedByUserDisplayName);
            MasterGame domain = entity.ToDomain(masterSubGames.Where(sub => sub.MasterGameID == entity.MasterGameID), tags, addedByUser);
            masterGameDictionary.Add(domain.MasterGameID, domain);
        }

        var masterGameYearDictionary = new Dictionary<Guid, MasterGameYear>();
        foreach (var entity in masterGameYearResults)
        {
            var tags = masterGameTagLookup[entity.MasterGameID].Select(x => possibleTags[x.TagName]).ToList();
            var addedByUser = new VeryMinimalFantasyCriticUser(entity.AddedByUserID, entity.AddedByUserDisplayName);
            MasterGameYear domain = entity.ToDomain(masterSubGames.Where(sub => sub.MasterGameID == entity.MasterGameID), tags, addedByUser);
            masterGameYearDictionary.Add(domain.MasterGame.MasterGameID, domain);
        }

        var supportedYear = supportedYearEntity.ToDomain();
        var userDictionary = usersInLeagueEntities.ToDictionary(x => x.UserID, y => y.ToDomain());

        var winningUser = leagueYearEntity.WinningUserID.HasValue ? userDictionary.GetValueOrDefault(leagueYearEntity.WinningUserID.Value) : null;
        var manager = userDictionary[leagueEntity.LeagueManager];
        leagueEntity.ManagerDisplayName = manager.UserName;
        leagueEntity.ManagerEmailAddress = manager.UserName;

        var league = leagueEntity.ToDomain(years);
        var leagueYearKey = new LeagueYearKey(leagueID, year);
        var domainLeagueTags = leagueTagEntities.Select(x => x.ToDomain(possibleTags[x.Tag])).ToList();
        var domainSpecialGameSlots = SpecialGameSlotEntity.ConvertSpecialGameSlotEntities(specialGameSlotEntities, possibleTags);
        var specialGameSlotsForLeagueYear = domainSpecialGameSlots[leagueYearKey];
        var domainEligibilityOverrides = DomainConversionUtilities.ConvertEligibilityOverrideEntities(eligibilityOverrideEntities, masterGameDictionary);
        var domainTagOverrides = DomainConversionUtilities.ConvertTagOverrideEntities(tagOverrideEntities, masterGameDictionary, possibleTags);
        var publishers = DomainConversionUtilities.ConvertPublisherEntities(userDictionary, publisherEntities, publisherGameEntities, formerPublisherGameEntities, masterGameYearDictionary);

        LeagueYear leagueYear = leagueYearEntity.ToDomain(league, supportedYear, domainEligibilityOverrides, domainTagOverrides, domainLeagueTags, specialGameSlotsForLeagueYear,
            winningUser, publishers);
        return leagueYear;
    }

    public async Task<LeagueYearWithUserStatus?> GetLeagueYearWithUserStatus(Guid leagueID, int year)
    {
        var param = new
        {
            P_LeagueID = leagueID,
            P_Year = year
        };

        await using var connection = new MySqlConnection(_connectionString);
        await using var resultSets = await connection.QueryMultipleAsync("sp_getleagueyearwithuserstatus", param, commandType: CommandType.StoredProcedure);

        var supportedYearEntity = resultSets.ReadSingle<SupportedYearEntity>();

        var leagueEntity = resultSets.ReadSingleOrDefault<LeagueEntity>();
        if (leagueEntity is null)
        {
            return null;
        }

        var years = resultSets.Read<int>();
        var leagueYearEntity = resultSets.ReadSingleOrDefault<LeagueYearEntity>();
        if (leagueYearEntity is null)
        {
            return null;
        }

        //League Year Results
        var leagueTagEntities = resultSets.Read<LeagueYearTagEntity>();
        var specialGameSlotEntities = resultSets.Read<SpecialGameSlotEntity>();
        var eligibilityOverrideEntities = resultSets.Read<EligibilityOverrideEntity>();
        var tagOverrideEntities = resultSets.Read<TagOverrideEntity>();
        var usersInLeagueEntities = resultSets.Read<FantasyCriticUserEntity>();
        var publisherEntities = resultSets.Read<PublisherEntity>();
        var publisherGameEntities = resultSets.Read<PublisherGameEntity>();
        var formerPublisherGameEntities = resultSets.Read<FormerPublisherGameEntity>();

        //MasterGame Results
        var masterGameResults = resultSets.Read<MasterGameEntity>();
        var tagResults = resultSets.Read<MasterGameTagEntity>();
        var masterSubGameResults = resultSets.Read<MasterSubGameEntity>();
        var masterGameTagResults = resultSets.Read<MasterGameHasTagEntity>();
        var masterGameYearResults = resultSets.Read<MasterGameYearEntity>();

        //User Status
        var userEntities = resultSets.Read<FantasyCriticUserEntity>();
        var playStatuses = resultSets.Read<LeagueYearStatusEntity>();
        var userYears = resultSets.Read<LeagueYearUserEntity>();
        var activeUserEntities = resultSets.Read<FantasyCriticUserEntity>();
        var inviteEntities = resultSets.Read<LeagueInviteEntity>();

        await resultSets.DisposeAsync();
        await connection.DisposeAsync();

        var possibleTags = tagResults.Select(x => x.ToDomain()).ToDictionary(x => x.Name);
        var masterGameTagLookup = masterGameTagResults.ToLookup(x => x.MasterGameID);

        var masterSubGames = masterSubGameResults.Select(x => x.ToDomain()).ToList();
        var masterGameDictionary = new Dictionary<Guid, MasterGame>();
        foreach (var entity in masterGameResults)
        {
            var tags = masterGameTagLookup[entity.MasterGameID].Select(x => possibleTags[x.TagName]).ToList();
            var addedByUser = new VeryMinimalFantasyCriticUser(entity.AddedByUserID, entity.AddedByUserDisplayName);
            MasterGame domain = entity.ToDomain(masterSubGames.Where(sub => sub.MasterGameID == entity.MasterGameID), tags, addedByUser);
            masterGameDictionary.Add(domain.MasterGameID, domain);
        }

        var masterGameYearDictionary = new Dictionary<Guid, MasterGameYear>();
        foreach (var entity in masterGameYearResults)
        {
            var tags = masterGameTagLookup[entity.MasterGameID].Select(x => possibleTags[x.TagName]).ToList();
            var addedByUser = new VeryMinimalFantasyCriticUser(entity.AddedByUserID, entity.AddedByUserDisplayName);
            MasterGameYear domain = entity.ToDomain(masterSubGames.Where(sub => sub.MasterGameID == entity.MasterGameID), tags, addedByUser);
            masterGameYearDictionary.Add(domain.MasterGame.MasterGameID, domain);
        }

        var supportedYear = supportedYearEntity.ToDomain();
        var userDictionary = usersInLeagueEntities.ToDictionary(x => x.UserID, y => y.ToDomain());

        var winningUser = leagueYearEntity.WinningUserID.HasValue ? userDictionary.GetValueOrDefault(leagueYearEntity.WinningUserID.Value) : null;
        var manager = userDictionary[leagueEntity.LeagueManager];
        leagueEntity.ManagerDisplayName = manager.UserName;
        leagueEntity.ManagerEmailAddress = manager.UserName;

        var league = leagueEntity.ToDomain(years);
        var leagueYearKey = new LeagueYearKey(leagueID, year);
        var domainLeagueTags = leagueTagEntities.Select(x => x.ToDomain(possibleTags[x.Tag])).ToList();
        var domainSpecialGameSlots = SpecialGameSlotEntity.ConvertSpecialGameSlotEntities(specialGameSlotEntities, possibleTags);
        var specialGameSlotsForLeagueYear = domainSpecialGameSlots[leagueYearKey];
        var domainEligibilityOverrides = DomainConversionUtilities.ConvertEligibilityOverrideEntities(eligibilityOverrideEntities, masterGameDictionary);
        var domainTagOverrides = DomainConversionUtilities.ConvertTagOverrideEntities(tagOverrideEntities, masterGameDictionary, possibleTags);
        var publishers = DomainConversionUtilities.ConvertPublisherEntities(userDictionary, publisherEntities, publisherGameEntities, formerPublisherGameEntities, masterGameYearDictionary);

        LeagueYear leagueYear = leagueYearEntity.ToDomain(league, supportedYear, domainEligibilityOverrides, domainTagOverrides, domainLeagueTags, specialGameSlotsForLeagueYear,
            winningUser, publishers);

        var usersInLeague = userEntities.Select(x => x.ToDomain()).ToList();
        var activePlayersForLeagueYear = activeUserEntities.Select(x => x.ToDomain()).ToList();
        var usersWithRemoveStatus = DomainConversionUtilities.ConvertUserRemovableEntities(leagueYear.League, userYears, playStatuses, usersInLeague);
        var leagueInvites = inviteEntities.Select(x => x.ToDomain()).ToList();

        var userStatus = new CombinedLeagueYearUserStatus(usersWithRemoveStatus, leagueInvites, activePlayersForLeagueYear);

        return new LeagueYearWithUserStatus(leagueYear, userStatus);
    }

    public async Task<LeagueYearWithSupplementalDataFromRepo?> GetLeagueYearWithSupplementalData(Guid leagueID, int year, FantasyCriticUser? currentUser)
    {
        var param = new
        {
            P_LeagueID = leagueID,
            P_Year = year,
            P_UserID = currentUser?.Id
        };

        await using var connection = new MySqlConnection(_connectionString);
        await using var resultSets = await connection.QueryMultipleAsync("sp_getleagueyearwithsupplementaldata", param, commandType: CommandType.StoredProcedure);

        var supportedYearEntity = resultSets.ReadSingle<SupportedYearEntity>();

        var leagueEntity = resultSets.ReadSingleOrDefault<LeagueEntity>();
        if (leagueEntity is null)
        {
            return null;
        }

        var years = resultSets.Read<int>();
        var leagueYearEntity = resultSets.ReadSingleOrDefault<LeagueYearEntity>();
        if (leagueYearEntity is null)
        {
            return null;
        }

        //League Year Results
        var leagueTagEntities = resultSets.Read<LeagueYearTagEntity>();
        var specialGameSlotEntities = resultSets.Read<SpecialGameSlotEntity>();
        var eligibilityOverrideEntities = resultSets.Read<EligibilityOverrideEntity>();
        var tagOverrideEntities = resultSets.Read<TagOverrideEntity>();
        var usersInLeagueEntities = resultSets.Read<FantasyCriticUserEntity>();
        var publisherEntities = resultSets.Read<PublisherEntity>();
        var publisherGameEntities = resultSets.Read<PublisherGameEntity>();
        var formerPublisherGameEntities = resultSets.Read<FormerPublisherGameEntity>();

        //MasterGame Results
        var masterGameResults = resultSets.Read<MasterGameEntity>();
        var tagResults = resultSets.Read<MasterGameTagEntity>();
        var masterSubGameResults = resultSets.Read<MasterSubGameEntity>();
        var masterGameTagResults = resultSets.Read<MasterGameHasTagEntity>();
        var masterGameYearResults = resultSets.Read<MasterGameYearEntity>();

        //SupplementalData Results
        var positionPoints = resultSets.Read<AveragePositionPointsEntity>();
        var systemWideValuesEntity = resultSets.ReadSingle<SystemWideValuesEntity>();
        var messageEntities = resultSets.Read<ManagerMessageEntity>();
        var dismissalEntities = resultSets.Read<ManagerMessageDismissalEntity>();
        var previousYearWinningUserID = resultSets.ReadSingleOrDefault<Guid?>();
        var tradeEntities = resultSets.Read<TradeEntity>();
        var componentEntities = resultSets.Read<TradeComponentEntity>();
        var voteEntities = resultSets.Read<TradeVoteEntity>();
        var specialAuctionEntities = resultSets.Read<SpecialAuctionEntity>();
        var bidEntities = resultSets.Read<PickupBidEntity>();
        var userIsFollowingLeague = resultSets.ReadSingle<UserIsFollowingLeagueEntity>();
        var minimalPublisherEntities = resultSets.Read<MinimalPublisherEntity>();
        var dropEntities = resultSets.Read<DropRequestEntity>();
        var queuedEntities = resultSets.Read<QueuedGameEntity>();

        //User Status
        var userEntities = resultSets.Read<FantasyCriticUserEntity>();
        var playStatuses = resultSets.Read<LeagueYearStatusEntity>();
        var userYears = resultSets.Read<LeagueYearUserEntity>();
        var activeUserEntities = resultSets.Read<FantasyCriticUserEntity>();
        var inviteEntities = resultSets.Read<LeagueInviteEntity>();

        await resultSets.DisposeAsync();
        await connection.DisposeAsync();

        var possibleTags = tagResults.Select(x => x.ToDomain()).ToDictionary(x => x.Name);
        var masterGameTagLookup = masterGameTagResults.ToLookup(x => x.MasterGameID);

        var masterSubGames = masterSubGameResults.Select(x => x.ToDomain()).ToList();
        var masterGameDictionary = new Dictionary<Guid, MasterGame>();
        foreach (var entity in masterGameResults)
        {
            var tags = masterGameTagLookup[entity.MasterGameID].Select(x => possibleTags[x.TagName]).ToList();
            var addedByUser = new VeryMinimalFantasyCriticUser(entity.AddedByUserID, entity.AddedByUserDisplayName);
            MasterGame domain = entity.ToDomain(masterSubGames.Where(sub => sub.MasterGameID == entity.MasterGameID), tags, addedByUser);
            masterGameDictionary.Add(domain.MasterGameID, domain);
        }

        var masterGameYearDictionary = new Dictionary<Guid, MasterGameYear>();
        foreach (var entity in masterGameYearResults)
        {
            var tags = masterGameTagLookup[entity.MasterGameID].Select(x => possibleTags[x.TagName]).ToList();
            var addedByUser = new VeryMinimalFantasyCriticUser(entity.AddedByUserID, entity.AddedByUserDisplayName);
            MasterGameYear domain = entity.ToDomain(masterSubGames.Where(sub => sub.MasterGameID == entity.MasterGameID), tags, addedByUser);
            masterGameYearDictionary.Add(domain.MasterGame.MasterGameID, domain);
        }

        var supportedYear = supportedYearEntity.ToDomain();
        var userDictionary = usersInLeagueEntities.ToDictionary(x => x.UserID, y => y.ToDomain());

        var winningUser = leagueYearEntity.WinningUserID.HasValue ? userDictionary.GetValueOrDefault(leagueYearEntity.WinningUserID.Value) : null;
        var manager = userDictionary[leagueEntity.LeagueManager];
        leagueEntity.ManagerDisplayName = manager.UserName;
        leagueEntity.ManagerEmailAddress = manager.UserName;

        var league = leagueEntity.ToDomain(years);
        var leagueYearKey = new LeagueYearKey(leagueID, year);
        var domainLeagueTags = leagueTagEntities.Select(x => x.ToDomain(possibleTags[x.Tag])).ToList();
        var domainSpecialGameSlots = SpecialGameSlotEntity.ConvertSpecialGameSlotEntities(specialGameSlotEntities, possibleTags);
        var specialGameSlotsForLeagueYear = domainSpecialGameSlots[leagueYearKey];
        var domainEligibilityOverrides = DomainConversionUtilities.ConvertEligibilityOverrideEntities(eligibilityOverrideEntities, masterGameDictionary);
        var domainTagOverrides = DomainConversionUtilities.ConvertTagOverrideEntities(tagOverrideEntities, masterGameDictionary, possibleTags);
        var publishers = DomainConversionUtilities.ConvertPublisherEntities(userDictionary, publisherEntities, publisherGameEntities, formerPublisherGameEntities, masterGameYearDictionary);

        LeagueYear leagueYear = leagueYearEntity.ToDomain(league, supportedYear, domainEligibilityOverrides, domainTagOverrides, domainLeagueTags, specialGameSlotsForLeagueYear,
            winningUser, publishers);

        var userPublisher = leagueYear.GetUserPublisher(currentUser);
        var systemWideValues = systemWideValuesEntity.ToDomain(positionPoints.Select(x => x.ToDomain()));
        var managersMessages = DomainConversionUtilities.GetManagersMessages(dismissalEntities, messageEntities);
        var activeTrades = DomainConversionUtilities.GetActiveTrades(leagueYear, componentEntities, voteEntities, tradeEntities, masterGameYearDictionary);
        var activeSpecialAuctions = DomainConversionUtilities.GetActiveSpecialAuctions(specialAuctionEntities, masterGameYearDictionary);
        var activePickupBids = DomainConversionUtilities.GetActivePickupBids(leagueYear, bidEntities, masterGameDictionary, masterGameYearDictionary);
        var allPublishersForUser = minimalPublisherEntities.Select(p => p.ToDomain()).ToList();
        var privatePublisherData = DomainConversionUtilities.GetPrivatePublisherData(leagueYear, userPublisher, activePickupBids, dropEntities, masterGameDictionary, queuedEntities);

        var supplementalData = new LeagueYearSupplementalDataFromRepo(systemWideValues, managersMessages, previousYearWinningUserID, activeTrades, activeSpecialAuctions, activePickupBids,
            userIsFollowingLeague.UserIsFollowingLeague, allPublishersForUser, privatePublisherData, masterGameYearDictionary);

        var usersInLeague = userEntities.Select(x => x.ToDomain()).ToList();
        var activePlayersForLeagueYear = activeUserEntities.Select(x => x.ToDomain()).ToList();
        var usersWithRemoveStatus = DomainConversionUtilities.ConvertUserRemovableEntities(leagueYear.League, userYears, playStatuses, usersInLeague);
        var leagueInvites = inviteEntities.Select(x => x.ToDomain()).ToList();

        var userStatus = new CombinedLeagueYearUserStatus(usersWithRemoveStatus, leagueInvites, activePlayersForLeagueYear);

        return new LeagueYearWithSupplementalDataFromRepo(leagueYear, supplementalData, userStatus);
    }

    private record UserIsFollowingLeagueEntity()
    {
        public required bool UserIsFollowingLeague { get; init; }
    }
}
