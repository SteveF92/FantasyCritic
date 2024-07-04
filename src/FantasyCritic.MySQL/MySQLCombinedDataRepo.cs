using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.MySQL.Entities;
using Serilog;
using System.Data;
using FantasyCritic.Lib.SharedSerialization.Database;
using FantasyCritic.MySQL.Entities.Conferences;
using FantasyCritic.Lib.Domain.Trades;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.MySQL.Entities.Trades;
using FantasyCritic.Lib.Domain.LeagueActions;

namespace FantasyCritic.MySQL;

//This class is part of an effort to optimize API calls that are called very frequently
//Before this class, all of this data was retrieved by separate functions, with separate, wasteful SQL calls.

public class MySQLCombinedDataRepo : ICombinedDataRepo
{
    private readonly IMasterGameRepo _masterGameRepo;
    private static readonly ILogger _logger = Log.ForContext<MySQLFantasyCriticRepo>();

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

        var tagDictionary = await _masterGameRepo.GetMasterGameTagDictionary();
        var supportedYear = supportedYearEntity.ToDomain();
        var userDictionary = usersInLeagueEntities.ToDictionary(x => x.UserID, y => y.ToDomain());

        var winningUser = leagueYearEntity.WinningUserID.HasValue ? userDictionary.GetValueOrDefault(leagueYearEntity.WinningUserID.Value) : null;
        var manager = userDictionary[leagueEntity.LeagueManager];
        leagueEntity.ManagerDisplayName = manager.UserName;
        leagueEntity.ManagerEmailAddress = manager.UserName;

        var league = leagueEntity.ToDomain(years);
        var leagueYearKey = new LeagueYearKey(leagueID, year);
        var domainLeagueTags = leagueTagEntities.Select(x => x.ToDomain(tagDictionary[x.Tag])).ToList();
        var domainSpecialGameSlots = SpecialGameSlotEntity.ConvertSpecialGameSlotEntities(specialGameSlotEntities, tagDictionary);
        var specialGameSlotsForLeagueYear = domainSpecialGameSlots[leagueYearKey];
        var domainEligibilityOverrides = await ConvertEligibilityOverrideEntities(eligibilityOverrideEntities);
        var domainTagOverrides = await ConvertTagOverrideEntities(tagOverrideEntities, tagDictionary);
        var publishers = await ConvertPublisherEntities(userDictionary, publisherEntities, publisherGameEntities, formerPublisherGameEntities, year);

        LeagueYear leagueYear = leagueYearEntity.ToDomain(league, supportedYear, domainEligibilityOverrides, domainTagOverrides, domainLeagueTags, specialGameSlotsForLeagueYear,
            winningUser, publishers);
        return leagueYear;
    }

    private async Task<IReadOnlyList<EligibilityOverride>> ConvertEligibilityOverrideEntities(IEnumerable<EligibilityOverrideEntity> eligibilityOverrideEntities)
    {
        List<EligibilityOverride> domainEligibilityOverrides = new List<EligibilityOverride>();
        foreach (var entity in eligibilityOverrideEntities)
        {
            var masterGame = await _masterGameRepo.GetMasterGameOrThrow(entity.MasterGameID);
            EligibilityOverride domain = entity.ToDomain(masterGame);
            domainEligibilityOverrides.Add(domain);
        }

        return domainEligibilityOverrides;
    }

    private async Task<IReadOnlyList<TagOverride>> ConvertTagOverrideEntities(IEnumerable<TagOverrideEntity> tagOverrideEntities, IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        var allMasterGames = await _masterGameRepo.GetMasterGames();
        var masterGameDictionary = allMasterGames.ToDictionary(x => x.MasterGameID);
        List<TagOverride> domainTagOverrides = new List<TagOverride>();
        var entitiesByMasterGameID = tagOverrideEntities.GroupBy(x => x.MasterGameID);
        foreach (var entitySet in entitiesByMasterGameID)
        {
            var masterGame = masterGameDictionary[entitySet.Key];
            List<MasterGameTag> tagsForMasterGame = new List<MasterGameTag>();
            foreach (var entity in entitySet)
            {
                var fullTag = tagDictionary[entity.TagName];
                tagsForMasterGame.Add(fullTag);
            }

            domainTagOverrides.Add(new TagOverride(masterGame, tagsForMasterGame));
        }

        return domainTagOverrides;
    }

    private async Task<IReadOnlyList<Publisher>> ConvertPublisherEntities(IReadOnlyDictionary<Guid, FantasyCriticUser> usersInLeague, IEnumerable<PublisherEntity> publisherEntities,
        IEnumerable<PublisherGameEntity> publisherGameEntities, IEnumerable<FormerPublisherGameEntity> formerPublisherGameEntities, int year)
    {
        IReadOnlyList<PublisherGame> domainGames = await ConvertPublisherGameEntities(publisherGameEntities, year);
        IReadOnlyList<FormerPublisherGame> domainFormerGames = await ConvertFormerPublisherGameEntities(formerPublisherGameEntities, year);

        List<Publisher> domainPublishers = new List<Publisher>();
        foreach (var entity in publisherEntities)
        {
            var gamesForPublisher = domainGames.Where(x => x.PublisherID == entity.PublisherID);
            var formerGamesForPublisher = domainFormerGames.Where(x => x.PublisherGame.PublisherID == entity.PublisherID);
            var user = usersInLeague[entity.UserID];
            var domainPublisher = entity.ToDomain(user, gamesForPublisher, formerGamesForPublisher);
            domainPublishers.Add(domainPublisher);
        }

        return domainPublishers;
    }

    private async Task<IReadOnlyList<PublisherGame>> ConvertPublisherGameEntities(IEnumerable<PublisherGameEntity> gameEntities, int year)
    {
        List<PublisherGame> domainGames = new List<PublisherGame>();
        foreach (var entity in gameEntities)
        {
            MasterGameYear? masterGame = null;
            if (entity.MasterGameID.HasValue)
            {
                masterGame = await _masterGameRepo.GetMasterGameYear(entity.MasterGameID.Value, year);
            }

            domainGames.Add(entity.ToDomain(masterGame));
        }

        return domainGames;
    }

    private async Task<IReadOnlyList<FormerPublisherGame>> ConvertFormerPublisherGameEntities(IEnumerable<FormerPublisherGameEntity> gameEntities, int year)
    {
        List<FormerPublisherGame> domainGames = new List<FormerPublisherGame>();
        foreach (var entity in gameEntities)
        {
            MasterGameYear? masterGame = null;
            if (entity.MasterGameID.HasValue)
            {
                masterGame = await _masterGameRepo.GetMasterGameYear(entity.MasterGameID.Value, year);
            }

            domainGames.Add(entity.ToDomain(masterGame));
        }

        return domainGames;
    }

    private record UserIsFollowingLeagueEntity()
    {
        public required bool UserIsFollowingLeague { get; init; }
    }


    public async Task<LeagueYearSupplementalDataFromRepo> GetLeagueYearSupplementalData(LeagueYear leagueYear, FantasyCriticUser? currentUser)
    {
        
        var userPublisher = leagueYear.GetUserPublisher(currentUser);

        var masterGames = await _masterGameRepo.GetMasterGames();
        var masterGameDictionary = masterGames.ToDictionary(x => x.MasterGameID);
        var masterGameYears = await _masterGameRepo.GetMasterGameYears(leagueYear.Year);
        var masterGameYearDictionary = masterGameYears.ToDictionary(x => x.MasterGame.MasterGameID);

        var queryObject = new
        {
            P_LeagueID = leagueYear.League.LeagueID,
            P_Year = leagueYear.Year,
            P_UserID = currentUser?.Id,
            P_PublisherID = userPublisher?.PublisherID
        };

        await using var connection = new MySqlConnection(_connectionString);
        await using var resultSets = await connection.QueryMultipleAsync("sp_getleagueyearsupplementaldata", queryObject, commandType: CommandType.StoredProcedure);

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

        //Getting domain objects
        var systemWideValues = systemWideValuesEntity.ToDomain(positionPoints.Select(x => x.ToDomain()));
        var managersMessages = GetManagersMessages(dismissalEntities, messageEntities);
        var activeTrades = GetActiveTrades(leagueYear, componentEntities, voteEntities, tradeEntities, masterGameYearDictionary);
        var activeSpecialAuctions = GetActiveSpecialAuctions(specialAuctionEntities, masterGameYearDictionary);
        var activePickupBids = GetActivePickupBids(leagueYear, bidEntities, masterGameDictionary, masterGameYearDictionary);
        var allPublishersForUser = minimalPublisherEntities.Select(p => p.ToDomain()).ToList();
        var privatePublisherData = GetPrivatePublisherData(leagueYear, userPublisher, activePickupBids, dropEntities, masterGameDictionary, queuedEntities);

        return new LeagueYearSupplementalDataFromRepo(systemWideValues, managersMessages, previousYearWinningUserID, activeTrades, activeSpecialAuctions, activePickupBids,
            userIsFollowingLeague.UserIsFollowingLeague, allPublishersForUser, privatePublisherData, masterGameYearDictionary);
    }

    private static List<ManagerMessage> GetManagersMessages(IEnumerable<ManagerMessageDismissalEntity> dismissalEntities, IEnumerable<ManagerMessageEntity> messageEntities)
    {
        List<ManagerMessage> managersMessages = new List<ManagerMessage>();
        var dismissalLookup = dismissalEntities.ToLookup(x => x.MessageID);
        foreach (var messageEntity in messageEntities)
        {
            var dismissedUserIDs = dismissalLookup[messageEntity.MessageID].Select(x => x.UserID);
            managersMessages.Add(messageEntity.ToDomain(dismissedUserIDs));
        }

        return managersMessages;
    }

    private static List<Trade> GetActiveTrades(LeagueYear leagueYear, IEnumerable<TradeComponentEntity> componentEntities, IEnumerable<TradeVoteEntity> voteEntities,
    IEnumerable<TradeEntity> tradeEntities, Dictionary<Guid, MasterGameYear> masterGameYearDictionary)
    {
        var componentLookup = componentEntities.ToLookup(x => x.TradeID);
        var voteLookup = voteEntities.ToLookup(x => x.TradeID);

        List<Trade> domainTrades = new List<Trade>();
        foreach (var tradeEntity in tradeEntities)
        {
            Publisher proposer = leagueYear.GetPublisherByIDOrFakePublisher(tradeEntity.ProposerPublisherID);
            Publisher counterParty = leagueYear.GetPublisherByIDOrFakePublisher(tradeEntity.CounterPartyPublisherID);

            var components = componentLookup[tradeEntity.TradeID];
            List<MasterGameYearWithCounterPick> proposerMasterGameYearWithCounterPicks = new List<MasterGameYearWithCounterPick>();
            List<MasterGameYearWithCounterPick> counterPartyMasterGameYearWithCounterPicks = new List<MasterGameYearWithCounterPick>();
            foreach (var component in components)
            {
                var masterGameYear = masterGameYearDictionary.GetValueOrDefault(component.MasterGameID);
                if (masterGameYear is null)
                {
                    throw new Exception($"Invalid master game when getting trade: {tradeEntity.TradeID}");
                }

                var domainComponent = new MasterGameYearWithCounterPick(masterGameYear, component.CounterPick);
                if (component.CurrentParty == TradingParty.Proposer.Value)
                {
                    proposerMasterGameYearWithCounterPicks.Add(domainComponent);
                }
                else if (component.CurrentParty == TradingParty.CounterParty.Value)
                {
                    counterPartyMasterGameYearWithCounterPicks.Add(domainComponent);
                }
                else
                {
                    throw new Exception($"Invalid party when getting trade: {tradeEntity.TradeID}");
                }
            }

            var votes = voteLookup[tradeEntity.TradeID];
            List<TradeVote> tradeVotes = new List<TradeVote>();
            foreach (var vote in votes)
            {
                var user = leagueYear.Publishers.Single(x => x.User.Id == vote.UserID).User;
                var domainVote = new TradeVote(tradeEntity.TradeID, user, vote.Approved, vote.Comment, vote.Timestamp);
                tradeVotes.Add(domainVote);
            }

            domainTrades.Add(tradeEntity.ToDomain(leagueYear, proposer, counterParty, proposerMasterGameYearWithCounterPicks, counterPartyMasterGameYearWithCounterPicks, tradeVotes));
        }

        var activeTrades = domainTrades.Where(x => x.Status.IsActive).OrderByDescending(x => x.ProposedTimestamp).ToList();
        return activeTrades;
    }

    private static List<SpecialAuction> GetActiveSpecialAuctions(IEnumerable<SpecialAuctionEntity> specialAuctionEntities, Dictionary<Guid, MasterGameYear> masterGameYearDictionary)
    {
        List<SpecialAuction> specialAuctions = new List<SpecialAuction>();
        foreach (var entity in specialAuctionEntities)
        {
            var masterGame = masterGameYearDictionary[entity.MasterGameID];
            specialAuctions.Add(entity.ToDomain(masterGame));
        }
        var activeSpecialAuctions = specialAuctions.Where(x => !x.Processed).ToList();
        return activeSpecialAuctions;
    }

    private static List<PickupBid> GetActivePickupBids(LeagueYear leagueYear, IEnumerable<PickupBidEntity> bidEntities, Dictionary<Guid, MasterGame> masterGameDictionary,
        Dictionary<Guid, MasterGameYear> masterGameYearDictionary)
    {
        var publisherDictionary = leagueYear.Publishers.ToDictionary(x => x.PublisherID);

        var publisherGameDictionary = leagueYear.Publishers
            .SelectMany(x => x.PublisherGames)
            .Where(x => x.MasterGame is not null)
            .ToLookup(x => (x.PublisherID, x.MasterGame!.MasterGame.MasterGameID));

        var formerPublisherGameDictionary = leagueYear.Publishers
            .SelectMany(x => x.FormerPublisherGames)
            .Where(x => x.PublisherGame.MasterGame is not null)
            .ToLookup(x => (x.PublisherGame.PublisherID, x.PublisherGame.MasterGame!.MasterGame.MasterGameID));

        List<PickupBid> activePickupBids = new List<PickupBid>();
        foreach (var bidEntity in bidEntities)
        {
            var masterGame = masterGameDictionary[bidEntity.MasterGameID];
            PublisherGame? conditionalDropPublisherGame = GetConditionalDropPublisherGame(bidEntity, publisherGameDictionary, formerPublisherGameDictionary, masterGameYearDictionary);
            var publisher = publisherDictionary[bidEntity.PublisherID];
            PickupBid domain = bidEntity.ToDomain(publisher, masterGame, conditionalDropPublisherGame, leagueYear);
            activePickupBids.Add(domain);
        }

        return activePickupBids;
    }

    private static PrivatePublisherData? GetPrivatePublisherData(LeagueYear leagueYear, Publisher? userPublisher,
        List<PickupBid> activePickupBids, IEnumerable<DropRequestEntity> dropEntities,
        Dictionary<Guid, MasterGame> masterGameDictionary, IEnumerable<QueuedGameEntity> queuedEntities)
    {
        if (userPublisher is null)
        {
            return null;
        }

        //Active Bids
        var bidsForUser = activePickupBids.Where(x => x.Publisher.PublisherID == userPublisher.PublisherID).ToList();

        //Active Drops
        List<DropRequest> domainDrops = new List<DropRequest>();
        foreach (var dropEntity in dropEntities)
        {
            var masterGame = masterGameDictionary[dropEntity.MasterGameID];
            DropRequest domain = dropEntity.ToDomain(userPublisher, masterGame, leagueYear);
            domainDrops.Add(domain);
        }

        //Queued Games
        List<QueuedGame> domainQueue = new List<QueuedGame>();
        foreach (var queuedEntity in queuedEntities)
        {
            var masterGame = masterGameDictionary[queuedEntity.MasterGameID];
            QueuedGame domain = queuedEntity.ToDomain(userPublisher, masterGame);
            domainQueue.Add(domain);
        }

        return new PrivatePublisherData(bidsForUser, domainDrops, domainQueue);
    }

    private static PublisherGame? GetConditionalDropPublisherGame(PickupBidEntity bidEntity,
        ILookup<(Guid PublisherID, Guid MasterGameID), PublisherGame> publisherGameLookup,
        ILookup<(Guid PublisherID, Guid MasterGameID), FormerPublisherGame> formerPublisherGameLookup,
        IReadOnlyDictionary<Guid, MasterGameYear> masterGameYearDictionary)
    {
        if (!bidEntity.ConditionalDropMasterGameID.HasValue)
        {
            return null;
        }

        var currentPublisherGames = publisherGameLookup[(bidEntity.PublisherID, bidEntity.ConditionalDropMasterGameID.Value)].ToList();
        if (currentPublisherGames.Any())
        {
            return currentPublisherGames.WhereMax(x => x.Timestamp).First();
        }

        var formerPublisherGames = formerPublisherGameLookup[(bidEntity.PublisherID, bidEntity.ConditionalDropMasterGameID.Value)].ToList();
        if (formerPublisherGames.Any())
        {
            return formerPublisherGames.WhereMax(x => x.PublisherGame.Timestamp).First().PublisherGame;
        }

        var conditionalDropGame = masterGameYearDictionary[bidEntity.ConditionalDropMasterGameID.Value];
        var fakePublisherGame = new PublisherGame(bidEntity.PublisherID, Guid.NewGuid(),
            conditionalDropGame.MasterGame.GameName, bidEntity.Timestamp,
            false, null, false, null, conditionalDropGame, 0, null, null, null, null);

        return fakePublisherGame;
    }
}
