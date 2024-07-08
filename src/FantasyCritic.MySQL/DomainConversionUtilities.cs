using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Trades;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.SharedSerialization.Database;
using FantasyCritic.MySQL.Entities;
using FantasyCritic.MySQL.Entities.Identity;
using FantasyCritic.MySQL.Entities.Trades;

namespace FantasyCritic.MySQL;
internal static class DomainConversionUtilities
{
    public static IReadOnlyList<EligibilityOverride> ConvertEligibilityOverrideEntities(IEnumerable<EligibilityOverrideEntity> eligibilityOverrideEntities,
        IReadOnlyDictionary<Guid, MasterGame> masterGameDictionary)
    {
        List<EligibilityOverride> domainEligibilityOverrides = new List<EligibilityOverride>();
        foreach (var entity in eligibilityOverrideEntities)
        {
            var masterGame = masterGameDictionary[entity.MasterGameID];
            EligibilityOverride domain = entity.ToDomain(masterGame);
            domainEligibilityOverrides.Add(domain);
        }

        return domainEligibilityOverrides;
    }

    public static IReadOnlyList<TagOverride> ConvertTagOverrideEntities(IEnumerable<TagOverrideEntity> tagOverrideEntities,
        IReadOnlyDictionary<Guid, MasterGame> masterGameDictionary, IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
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

    public static IReadOnlyList<Publisher> ConvertPublisherEntities(IReadOnlyDictionary<Guid, FantasyCriticUser> usersInLeague, IEnumerable<PublisherEntity> publisherEntities,
        IEnumerable<PublisherGameEntity> publisherGameEntities, IEnumerable<FormerPublisherGameEntity> formerPublisherGameEntities, IReadOnlyDictionary<Guid, MasterGameYear> masterGameYearDictionary)
    {
        IReadOnlyList<PublisherGame> domainGames = ConvertPublisherGameEntities(publisherGameEntities, masterGameYearDictionary);
        IReadOnlyList<FormerPublisherGame> domainFormerGames = ConvertFormerPublisherGameEntities(formerPublisherGameEntities, masterGameYearDictionary);

        var domainGameLookup = domainGames.ToLookup(x => x.PublisherID);
        var domainFormerGameLookup = domainFormerGames.ToLookup(x => x.PublisherGame.PublisherID);

        List<Publisher> domainPublishers = new List<Publisher>();
        foreach (var entity in publisherEntities)
        {
            var gamesForPublisher = domainGameLookup[entity.PublisherID];
            var formerGamesForPublisher = domainFormerGameLookup[entity.PublisherID];
            var user = usersInLeague[entity.UserID];
            var domainPublisher = entity.ToDomain(user, gamesForPublisher, formerGamesForPublisher);
            domainPublishers.Add(domainPublisher);
        }

        return domainPublishers;
    }

    public static IReadOnlyList<PublisherGame> ConvertPublisherGameEntities(IEnumerable<PublisherGameEntity> gameEntities, IReadOnlyDictionary<Guid, MasterGameYear> masterGameYearDictionary)
    {
        List<PublisherGame> domainGames = new List<PublisherGame>();
        foreach (var entity in gameEntities)
        {
            MasterGameYear? masterGame = null;
            if (entity.MasterGameID.HasValue)
            {
                masterGame = masterGameYearDictionary[entity.MasterGameID.Value];
            }

            domainGames.Add(entity.ToDomain(masterGame));
        }

        return domainGames;
    }

    public static IReadOnlyList<FormerPublisherGame> ConvertFormerPublisherGameEntities(IEnumerable<FormerPublisherGameEntity> gameEntities, IReadOnlyDictionary<Guid, MasterGameYear> masterGameYearDictionary)
    {
        List<FormerPublisherGame> domainGames = new List<FormerPublisherGame>();
        foreach (var entity in gameEntities)
        {
            MasterGameYear? masterGame = null;
            if (entity.MasterGameID.HasValue)
            {
                masterGame = masterGameYearDictionary[entity.MasterGameID.Value];
            }

            domainGames.Add(entity.ToDomain(masterGame));
        }

        return domainGames;
    }

    public static List<ManagerMessage> GetManagersMessages(IEnumerable<ManagerMessageDismissalEntity> dismissalEntities, IEnumerable<ManagerMessageEntity> messageEntities)
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

    public static List<Trade> GetActiveTrades(LeagueYear leagueYear, IEnumerable<TradeComponentEntity> componentEntities, IEnumerable<TradeVoteEntity> voteEntities,
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

    public static List<SpecialAuction> GetActiveSpecialAuctions(IEnumerable<SpecialAuctionEntity> specialAuctionEntities, Dictionary<Guid, MasterGameYear> masterGameYearDictionary)
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

    public static List<PickupBid> GetActivePickupBids(LeagueYear leagueYear, IEnumerable<PickupBidEntity> bidEntities, Dictionary<Guid, MasterGame> masterGameDictionary,
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

    public static PrivatePublisherData? GetPrivatePublisherData(LeagueYear leagueYear, Publisher? userPublisher,
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

    public static PublisherGame? GetConditionalDropPublisherGame(PickupBidEntity bidEntity,
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

    public static IReadOnlyList<FantasyCriticUserRemovable> ConvertUserRemovableEntities(League league, IEnumerable<LeagueYearUserEntity> userYears,
        IEnumerable<LeagueYearStatusEntity> playStatuses, IReadOnlyList<FantasyCriticUser> usersInLeague)
    {
        var userYearsDictionary = new Dictionary<int, HashSet<Guid>>();
        foreach (var userYear in userYears)
        {
            if (!userYearsDictionary.ContainsKey(userYear.Year))
            {
                userYearsDictionary[userYear.Year] = new HashSet<Guid>();
            }

            userYearsDictionary[userYear.Year].Add(userYear.UserID);
        }

        var startedYears = playStatuses
            .Where(x => x.PlayStatus != PlayStatus.NotStartedDraft.Value)
            .Select(x => x.Year)
            .ToList();

        List<FantasyCriticUserRemovable> usersWithStatus = new List<FantasyCriticUserRemovable>();
        foreach (var user in usersInLeague)
        {
            bool userRemovable = league.LeagueManager.UserID != user.UserID;
            foreach (var year in startedYears)
            {
                if (!userYearsDictionary.ContainsKey(year))
                {
                    continue;
                }
                var userPlayedInYear = userYearsDictionary[year].Contains(user.Id);
                if (userPlayedInYear)
                {
                    userRemovable = false;
                }
            }

            usersWithStatus.Add(new FantasyCriticUserRemovable(user, userRemovable));
        }

        return usersWithStatus;
    }
}
