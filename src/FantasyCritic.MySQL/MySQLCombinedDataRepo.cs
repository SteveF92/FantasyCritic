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

    public async Task<LeagueYearSupplementalDataFromRepo> GetLeagueYearSupplementalData(LeagueYear leagueYear, FantasyCriticUser? currentUser)
    {
        var userPublisher = leagueYear.GetUserPublisher(currentUser);

        var queryObject = new
        {
            P_LeagueID = leagueYear.League.LeagueID,
            P_Year = leagueYear.Year,
            P_UserID = currentUser?.Id,
            P_PublisherID = userPublisher?.PublisherID
        };

        var masterGameYears = await _masterGameRepo.GetMasterGameYears(leagueYear.Year);
        var masterGameYearDictionary = masterGameYears.ToDictionary(x => x.MasterGame.MasterGameID);

        await using var connection = new MySqlConnection(_connectionString);
        //await using var resultSets = await connection.QueryMultipleAsync("sp_getleagueyearsupplementaldata", queryObject, commandType: CommandType.StoredProcedure);

        //SystemWideValues
        var positionPoints = await connection.QueryAsync<AveragePositionPointsEntity>("select * from tbl_caching_averagepositionpoints;");
        var result = await connection.QuerySingleAsync<SystemWideValuesEntity>("select * from tbl_caching_systemwidevalues;");
        var systemWideValues = result.ToDomain(positionPoints.Select(x => x.ToDomain()));

        //ManagersMessages
        const string messageSQL = "select * from tbl_league_managermessage where LeagueID = @P_LeagueID AND Year = @P_Year AND Deleted = 0;";
        const string dismissSQL = "select * from tbl_league_managermessagedismissal where MessageID in @messageIDs;";

        IEnumerable<ManagerMessageEntity> messageEntities = await connection.QueryAsync<ManagerMessageEntity>(messageSQL, queryObject);
        var messageIDs = messageEntities.Select(x => x.MessageID);
        var dismissQueryObject = new
        {
            messageIDs
        };
        IEnumerable<ManagerMessageDismissalEntity> dismissalEntities = await connection.QueryAsync<ManagerMessageDismissalEntity>(dismissSQL, dismissQueryObject);

        List<ManagerMessage> managersMessages = new List<ManagerMessage>();
        var dismissalLookup = dismissalEntities.ToLookup(x => x.MessageID);
        foreach (var messageEntity in messageEntities)
        {
            var dismissedUserIDs = dismissalLookup[messageEntity.MessageID].Select(x => x.UserID);
            managersMessages.Add(messageEntity.ToDomain(dismissedUserIDs));
        }

        //PreviousYearWinnerUserID
        const string previousYearWinnerSQL = "select WinnerUserID from tbl_league_year where LeagueID = @P_LeagueID and Year = @P_Year - 1";
        Guid? previousYearWinningUserID = await connection.QuerySingleOrDefaultAsync<Guid?>(previousYearWinnerSQL, queryObject);

        //ActiveTrades
        const string baseTableSQL = "select * from tbl_league_trade WHERE LeagueID = @P_LeagueID AND Year = @P_Year;";
        const string componentTableSQL = "select tbl_league_tradecomponent.* from tbl_league_tradecomponent " +
                                         "join tbl_league_trade ON tbl_league_tradecomponent.TradeID = tbl_league_trade.TradeID " +
                                         "WHERE LeagueID = @P_LeagueID AND Year = @P_Year;";
        const string voteTableSQL = "select tbl_league_tradevote.* from tbl_league_tradevote " +
                                    "join tbl_league_trade ON tbl_league_tradevote.TradeID = tbl_league_trade.TradeID " +
                                    "WHERE LeagueID = @P_LeagueID AND Year = @P_Year;";

        var tradeEntities = await connection.QueryAsync<TradeEntity>(baseTableSQL, queryObject);
        var componentEntities = await connection.QueryAsync<TradeComponentEntity>(componentTableSQL, queryObject);
        var voteEntities = await connection.QueryAsync<TradeVoteEntity>(voteTableSQL, queryObject);

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
                var masterGameYear = await _masterGameRepo.GetMasterGameYear(component.MasterGameID, leagueYear.Year);
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

        //Active Special Auctions
        const string activeSpecialAuctionsSQL = "select * from tbl_league_specialauction where LeagueID = @P_LeagueID AND Year = @P_Year;";
        var key = new LeagueYearKeyEntity(leagueYear.Key);

        var specialAuctionEntities = await connection.QueryAsync<SpecialAuctionEntity>(activeSpecialAuctionsSQL, key);

        List<SpecialAuction> specialAuctions = new List<SpecialAuction>();
        foreach (var entity in specialAuctionEntities)
        {
            var masterGame = await _masterGameRepo.GetMasterGameYearOrThrow(entity.MasterGameID, leagueYear.Year);
            specialAuctions.Add(entity.ToDomain(masterGame));
        }
        var activeSpecialAuctions = specialAuctions.Where(x => !x.Processed).ToList();

        //Public bidding set
        var publisherDictionary = leagueYear.Publishers.ToDictionary(x => x.PublisherID);

        var publisherGameDictionary = leagueYear.Publishers
            .SelectMany(x => x.PublisherGames)
            .Where(x => x.MasterGame is not null)
            .ToLookup(x => (x.PublisherID, x.MasterGame!.MasterGame.MasterGameID));

        var formerPublisherGameDictionary = leagueYear.Publishers
            .SelectMany(x => x.FormerPublisherGames)
            .Where(x => x.PublisherGame.MasterGame is not null)
            .ToLookup(x => (x.PublisherGame.PublisherID, x.PublisherGame.MasterGame!.MasterGame.MasterGameID));

        const string publicBiddingSQL = "select * from vw_league_pickupbid where LeagueID = @P_LeagueID and Year = @P_Year and Successful is NULL";
        var bidEntities = await connection.QueryAsync<PickupBidEntity>(publicBiddingSQL, queryObject);
        List<PickupBid> activePickupBids = new List<PickupBid>();
        foreach (var bidEntity in bidEntities)
        {
            var masterGame = await _masterGameRepo.GetMasterGameOrThrow(bidEntity.MasterGameID);
            PublisherGame? conditionalDropPublisherGame = GetConditionalDropPublisherGame(bidEntity, leagueYear.Year, publisherGameDictionary, formerPublisherGameDictionary, masterGameYearDictionary);
            var publisher = publisherDictionary[bidEntity.PublisherID];
            PickupBid domain = bidEntity.ToDomain(publisher, masterGame, conditionalDropPublisherGame, leagueYear);
            activePickupBids.Add(domain);
        }

        //User is following league
        const string userIsFollowingLeagueSQL =
            """
            SELECT EXISTS(
                select 1 from tbl_user
                join tbl_user_followingleague on (tbl_user.UserID = tbl_user_followingleague.UserID)
                where tbl_user_followingleague.LeagueID = @P_LeagueID AND tbl_user.UserID = @P_UserID;
            );
            """;

        var followingLeagueCount = await connection.QuerySingleAsync<int>(userIsFollowingLeagueSQL, queryObject);
        bool userIsFollowingLeague = followingLeagueCount > 0;

        //AllPublishersForUser
        const string minimalPublisherSQL = """
                                           SELECT PublisherID, PublisherName, l.LeagueID, LeagueName, `Year` 
                                           FROM tbl_league_publisher p 
                                           JOIN tbl_league l ON p.LeagueID = l.LeagueID 
                                           WHERE UserID = @P_UserID AND `Year` = @P_Year";
                                           """;

        PrivatePublisherData? privatePublisherData = null;
        var allPublishersForUser = new List<MinimalPublisher>();
        if (userPublisher is not null)
        {
            var minimalPublisherEntities = await connection.QueryAsync<MinimalPublisherEntity>(minimalPublisherSQL, queryObject);
            allPublishersForUser = minimalPublisherEntities.Select(p => p.ToDomain()).ToList();

            //Active Bids
            var bidsForUser = activePickupBids.Where(x => x.Publisher.PublisherID == userPublisher.PublisherID).ToList();

            //Active Drops
            var dropEntities = await connection.QueryAsync<DropRequestEntity>("select * from tbl_league_droprequest where PublisherID = @P_PublisherID and Successful is NULL", queryObject);
            List<DropRequest> domainDrops = new List<DropRequest>();
            foreach (var dropEntity in dropEntities)
            {
                var masterGame = await _masterGameRepo.GetMasterGameOrThrow(dropEntity.MasterGameID);
                DropRequest domain = dropEntity.ToDomain(userPublisher, masterGame, leagueYear);
                domainDrops.Add(domain);
            }

            //Queued Games
            var queuedEntities = await connection.QueryAsync<QueuedGameEntity>("select * from tbl_league_publisherqueue where PublisherID = @P_PublisherID", queryObject);

            List<QueuedGame> domainQueue = new List<QueuedGame>();
            foreach (var queuedEntity in queuedEntities)
            {
                var masterGame = await _masterGameRepo.GetMasterGameOrThrow(queuedEntity.MasterGameID);
                QueuedGame domain = queuedEntity.ToDomain(userPublisher, masterGame);
                domainQueue.Add(domain);
            }

            privatePublisherData = new PrivatePublisherData(bidsForUser, domainDrops, domainQueue);
        }

        return new LeagueYearSupplementalDataFromRepo(systemWideValues, managersMessages, previousYearWinningUserID, activeTrades, activeSpecialAuctions, activePickupBids,
            userIsFollowingLeague, allPublishersForUser, privatePublisherData, masterGameYearDictionary);
    }

    private static PublisherGame? GetConditionalDropPublisherGame(PickupBidEntity bidEntity, int year,
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
