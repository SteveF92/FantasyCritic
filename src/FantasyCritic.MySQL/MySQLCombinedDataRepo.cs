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
using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Extensions;

namespace FantasyCritic.MySQL;

//This class is part of an effort to optimize API calls that are called very frequently
//Before this class, all of this data was retrieved by separate functions, with separate, wasteful SQL calls.

public class MySQLCombinedDataRepo : ICombinedDataRepo
{
    private readonly IReadOnlyFantasyCriticUserStore _userStore;
    private readonly string _connectionString;

    public MySQLCombinedDataRepo(RepositoryConfiguration configuration, IReadOnlyFantasyCriticUserStore _userStore)
    {
        this._userStore = _userStore;
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

    public async Task<ConferenceYearData?> GetConferenceYearData(Guid conferenceID, int year)
    {
        const string conferenceSQL = "select * from tbl_conference where ConferenceID = @conferenceID and IsDeleted = 0;";
        const string conferenceYearsSQL = "select Year from tbl_conference_year where ConferenceID = @conferenceID;";
        const string leaguesInConferenceSQL = "select LeagueID from tbl_league where ConferenceID = @conferenceID";
        const string conferenceYearSQL = "select * from tbl_conference_year where ConferenceID = @conferenceID and Year = @year;";
        const string userSQL = "select tbl_user.* from tbl_user join tbl_conference_hasuser on tbl_conference_hasuser.UserID = tbl_user.UserID where ConferenceID = @conferenceID;";
        const string leagueManagerSQL = "select LeagueID, LeagueManager from tbl_league where ConferenceID = @conferenceID;";
        const string leagueUserSQL = """
                                     select tbl_league_hasuser.LeagueID, tbl_league_hasuser.UserID
                                     from tbl_league_hasuser join tbl_league on tbl_league_hasuser.LeagueID = tbl_league.LeagueID
                                     where tbl_league.ConferenceID = @conferenceID;
                                     """;
        const string activePlayerSQL = """
                                       SELECT tbl_league_activeplayer.LeagueID, tbl_league_activeplayer.Year, tbl_league_activeplayer.UserID FROM tbl_league_activeplayer
                                       JOIN tbl_league ON tbl_league_activeplayer.LeagueID = tbl_league.LeagueID
                                       WHERE tbl_league.ConferenceID = @conferenceID;
                                       """;

        const string leagueYearSQL = """
                                     select 
                                     tbl_league.LeagueID, tbl_league.LeagueName, tbl_league.LeagueManager, 
                                     tbl_user.DisplayName as ManagerDisplayName, tbl_user.EmailAddress as ManagerEmailAddress, 
                                     tbl_league_year.Year,
                                     tbl_league_year.PlayStatus <> "NotStartedDraft" AS DraftStarted,
                                     tbl_league_year.PlayStatus = "DraftFinal" AS DraftFinished,
                                     ConferenceLocked
                                     from tbl_league_year 
                                     join tbl_league on tbl_league.LeagueID = tbl_league_year.LeagueID
                                     join tbl_user on tbl_league.LeagueManager = tbl_user.UserID
                                     where ConferenceID = @conferenceID and Year = @year;
                                     """;
        const string messageSQL = "select * from tbl_conference_managermessage where ConferenceID = @conferenceID AND Year = @year AND Deleted = 0;";
        const string dismissSQL = """
                                  SELECT *
                                  FROM tbl_conference_managermessage
                                  JOIN tbl_conference_managermessagedismissal ON tbl_conference_managermessage.MessageID = tbl_conference_managermessagedismissal.MessageID
                                  WHERE ConferenceID = @conferenceID
                                    AND YEAR = @year;
                                  """;

        var param = new
        {
            conferenceID,
            year
        };

        //Querying
        await using var connection = new MySqlConnection(_connectionString);

        ConferenceEntity? conferenceEntity = await connection.QuerySingleOrDefaultAsync<ConferenceEntity?>(conferenceSQL, param);
        if (conferenceEntity is null)
        {
            return null;
        }

        FantasyCriticUser manager = await _userStore.FindByIdOrThrowAsync(conferenceEntity.ConferenceManager, CancellationToken.None);
        IEnumerable<int> years = await connection.QueryAsync<int>(conferenceYearsSQL, param);
        IEnumerable<Guid> leagueIDs = await connection.QueryAsync<Guid>(leaguesInConferenceSQL, param);
        ConferenceYearEntity? conferenceYearEntity = await connection.QuerySingleOrDefaultAsync<ConferenceYearEntity?>(conferenceYearSQL, param);
        if (conferenceYearEntity is null)
        {
            return null;
        }

        var supportedYearEntity = await connection.QuerySingleAsync<SupportedYearEntity>("select * from tbl_meta_supportedyear where Year = @year;", param);
        var userEntities = await connection.QueryAsync<FantasyCriticUserEntity>(userSQL, param);
        var leagueManagers = await connection.QueryAsync<LeagueManagerEntity>(leagueManagerSQL, param);
        var leagueUsers = await connection.QueryAsync<LeagueUserEntity>(leagueUserSQL, param);
        var leagueActivePlayers = await connection.QueryAsync<LeagueActivePlayerEntity>(activePlayerSQL, param);
        var leagueYearEntities = (await connection.QueryAsync<ConferenceLeagueYearEntity>(leagueYearSQL, param)).ToList();
        IEnumerable<ManagerMessageEntity> messageEntities = await connection.QueryAsync<ManagerMessageEntity>(messageSQL, param);
        IEnumerable<ManagerMessageDismissalEntity> dismissalEntities = (await connection.QueryAsync<ManagerMessageDismissalEntity>(dismissSQL, param)).ToList();


        //Conversion
        Conference conference = conferenceEntity.ToDomain(manager.ToMinimal(), years, leagueIDs);
        var supportedYear = supportedYearEntity.ToDomain();
        ConferenceYear conferenceYear = conferenceYearEntity.ToDomain(conference, supportedYear);
        var usersInConference = userEntities.Select(x => x.ToDomain()).ToList();

        var leagueManagerLookup = leagueManagers.ToLookup(x => x.LeagueManager);
        var leagueUserLookup = leagueUsers.ToLookup(x => x.UserID);
        var leagueActivePlayerLookup = leagueActivePlayers.ToLookup(x => x.UserID);

        List<ConferencePlayer> conferencePlayers = new List<ConferencePlayer>();
        foreach (var user in usersInConference)
        {
            var leaguesManaged = leagueManagerLookup[user.Id].Select(x => x.LeagueID).ToHashSet();
            var leaguesIn = leagueUserLookup[user.Id].Select(x => x.LeagueID).ToHashSet();
            var leagueYearsActiveIn = leagueActivePlayerLookup[user.Id].Select(x => new LeagueYearKey(x.LeagueID, x.Year)).ToHashSet();
            var player = new ConferencePlayer(user.ToMinimal(), leaguesIn, leaguesManaged, leagueYearsActiveIn);
            conferencePlayers.Add(player);
        }


        //League Years
        List<ConferenceLeagueYear> leaguesInConference = leagueYearEntities.Select(leagueYearEntity => leagueYearEntity.ToDomain()).ToList();

        //Manager messages
        List<ManagerMessage> domainMessages = new List<ManagerMessage>();
        var dismissalLookup = dismissalEntities.ToLookup(x => x.MessageID);
        foreach (var messageEntity in messageEntities)
        {
            var dismissedUserIDs = dismissalLookup[messageEntity.MessageID].Select(x => x.UserID);
            domainMessages.Add(messageEntity.ToDomain(dismissedUserIDs));
        }

        return new ConferenceYearData(conferenceYear, conferencePlayers, leaguesInConference, new List<ConferenceYearStanding>(), domainMessages);
    }
}
