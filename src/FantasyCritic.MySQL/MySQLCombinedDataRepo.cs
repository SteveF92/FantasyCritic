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

        var positionPoints = await resultSets.ReadAsync<AveragePositionPointsEntity>();
        var systemWideValuesEntity = await resultSets.ReadSingleAsync<SystemWideValuesEntity>();
        var messageEntities = await resultSets.ReadAsync<ManagerMessageEntity>();
        var dismissalEntities = await resultSets.ReadAsync<ManagerMessageDismissalEntity>();
        var previousYearWinningUserID = await resultSets.ReadSingleOrDefaultAsync<Guid?>();
        var tradeEntities = await resultSets.ReadAsync<TradeEntity>();
        var componentEntities = await resultSets.ReadAsync<TradeComponentEntity>();
        var voteEntities = await resultSets.ReadAsync<TradeVoteEntity>();
        var specialAuctionEntities = await resultSets.ReadAsync<SpecialAuctionEntity>();
        var bidEntities = await resultSets.ReadAsync<PickupBidEntity>();
        var userIsFollowingLeague = await resultSets.ReadSingleAsync<UserIsFollowingLeagueEntity>();
        var minimalPublisherEntities = await resultSets.ReadAsync<MinimalPublisherEntity>();
        var dropEntities = await resultSets.ReadAsync<DropRequestEntity>();
        var queuedEntities = await resultSets.ReadAsync<QueuedGameEntity>();

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
