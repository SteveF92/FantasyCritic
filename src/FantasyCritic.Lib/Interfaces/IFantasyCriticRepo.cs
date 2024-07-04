using FantasyCritic.Lib.Domain.Calculations;
using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.Trades;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Interfaces;

public interface IFantasyCriticRepo
{
    Task<League?> GetLeague(Guid id);
    Task<LeagueYear?> GetLeagueYear(Guid leagueID, int year);
    Task<LeagueYearKey?> GetLeagueYearKeyForPublisherID(Guid publisherID);
    Task CreateLeague(League league, int initialYear, LeagueOptions options);
    Task AddNewLeagueYear(League league, int year, LeagueOptions options);
    Task EditLeagueYear(LeagueYear leagueYear, IReadOnlyDictionary<Guid, int> slotAssignments, LeagueManagerAction settingsChangeAction);

    Task<IReadOnlyList<FantasyCriticUser>> GetUsersInLeague(Guid leagueID);
    Task<IReadOnlyList<FantasyCriticUserRemovable>> GetUsersWithRemoveStatus(League league);
    Task<IReadOnlyList<FantasyCriticUser>> GetActivePlayersForLeagueYear(Guid leagueID, int year);
    Task<CombinedLeagueYearUserStatus> GetCombinedLeagueYearUserStatus(LeagueYear leagueYear);

    Task SetPlayersActive(League league, int year, IReadOnlyList<FantasyCriticUser> mostRecentActivePlayers);
    Task SetPlayerActiveStatus(LeagueYear leagueYear, IReadOnlyDictionary<FantasyCriticUser, bool> usersToChange);
    Task<IReadOnlyList<FantasyCriticUser>> GetLeagueFollowers(League league);
    Task<bool> UserIsFollowingLeague(FantasyCriticUser currentUser, League league);
    Task<IReadOnlyList<LeagueWithMostRecentYearStatus>> GetLeaguesForUser(FantasyCriticUser user);
    Task<IReadOnlyDictionary<FantasyCriticUser, IReadOnlyList<LeagueYearKey>>> GetUsersWithLeagueYearsWithPublisher();

    Task FollowLeague(League league, FantasyCriticUser user);
    Task UnfollowLeague(League league, FantasyCriticUser user);

    Task<LeagueInvite?> GetInvite(Guid inviteID);
    Task<IReadOnlyList<CompleteLeagueInvite>> GetCompleteLeagueInvites(FantasyCriticUser currentUser);
    Task SetAutoDraft(Publisher publisher, AutoDraftMode mode);
    Task<IReadOnlyList<LeagueInvite>> GetOutstandingInvitees(League league);
    Task SaveInvite(LeagueInvite leagueInvite);
    Task AcceptInvite(LeagueInvite leagueInvite, FantasyCriticUser user);
    Task DeleteInvite(LeagueInvite leagueInvite);
    Task AddPlayerToLeague(League league, FantasyCriticUser inviteUser);
    Task SaveInviteLink(LeagueInviteLink inviteLink);
    Task DeactivateInviteLink(LeagueInviteLink inviteID);
    Task<IReadOnlyList<LeagueInviteLink>> GetInviteLinks(League league);
    Task<LeagueInviteLink?> GetInviteLinkByInviteCode(Guid inviteCode);
    Task ReassignPublisher(LeagueYear leagueYear, Publisher publisherToReassign, FantasyCriticUser newUser);
    Task SetArchiveStatusForUser(League league, bool archive, FantasyCriticUser user);

    Task FullyRemovePublisher(LeagueYear leagueYear, Publisher deletePublisher);
    Task RemovePlayerFromLeague(League league, FantasyCriticUser removeUser);
    Task TransferLeagueManager(League league, FantasyCriticUser newManager);

    Task CreatePublisher(Publisher publisher);
    Task AddPublisherGame(PublisherGame publisherGame);
    Task AssociatePublisherGame(Publisher publisher, PublisherGame publisherGame, MasterGame masterGame);
    Task MergeMasterGame(MasterGame removeMasterGame, MasterGame mergeIntoMasterGame);
    Task ReorderPublisherGames(Publisher publisher, IReadOnlyDictionary<int, Guid?> slotStates);


    Task<IReadOnlyList<SupportedYear>> GetSupportedYears();
    Task<SupportedYear> GetSupportedYear(int year);

    Task<IReadOnlyList<LeagueYear>> GetLeagueYears(int year, bool includeDeleted = false);
    Task<IReadOnlyList<LeagueYear>> GetActiveLeagueYears(IEnumerable<Guid> leagueIDs);

    Task<IReadOnlyList<PublicLeagueYearStats>> GetPublicLeagueYears(int year, int? count);

    Task UpdatePublisherGameCalculatedStats(IReadOnlyDictionary<Guid, PublisherGameCalculatedStats> calculatedStats);
    Task UpdateLeagueWinners(IReadOnlyDictionary<LeagueYearKey, FantasyCriticUser> winningUsers, bool recalculate);

    Task FullyRemovePublisherGame(LeagueYear leagueYear, Publisher publisher, PublisherGame publisherGame);

    Task ManagerRemovePublisherGame(LeagueYear leagueYear, Publisher publisher, PublisherGame publisherGame, FormerPublisherGame formerPublisherGame, LeagueAction leagueAction);
    Task SuperDropGame(LeagueYear leagueYear, Publisher publisher, PublisherGame publisherGame, FormerPublisherGame formerPublisherGame, LeagueAction leagueAction);
    Task ManuallyScoreGame(PublisherGame publisherGame, decimal? manualCriticScore);
    Task ManuallySetWillNotRelease(PublisherGame publisherGame, bool willNotRelease);

    Task CreatePickupBid(PickupBid currentBid);
    Task RemovePickupBid(PickupBid bid);
    Task<IReadOnlyList<PickupBid>> GetActivePickupBids(LeagueYear leagueYear, Publisher publisher);
    Task<IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>>> GetActivePickupBids(int year, IReadOnlyList<LeagueYear> leagueYears);
    Task<IReadOnlyList<PickupBid>> GetActivePickupBids(LeagueYear leagueYear);
    Task<IReadOnlyList<PickupBid>> GetProcessedPickupBids(int year, IReadOnlyList<LeagueYear> allLeagueYears);
    Task<IReadOnlyList<PickupBid>> GetProcessedPickupBids(LeagueYear leagueYear);
    Task<PickupBid?> GetPickupBid(Guid bidID);
    Task SetBidPriorityOrder(IReadOnlyList<KeyValuePair<PickupBid, int>> bidPriorities);

    Task CreateDropRequest(DropRequest currentDropRequest);
    Task RemoveDropRequest(DropRequest dropRequest);
    Task<IReadOnlyList<DropRequest>> GetActiveDropRequests(LeagueYear leagueYear, Publisher publisher);
    Task<IReadOnlyDictionary<LeagueYear, IReadOnlyList<DropRequest>>> GetActiveDropRequests(int year, IReadOnlyList<LeagueYear> allLeagueYears);
    Task<IReadOnlyList<DropRequest>> GetProcessedDropRequests(LeagueYear leagueYear);
    Task<IReadOnlyList<DropRequest>> GetProcessedDropRequests(int year, IReadOnlyList<LeagueYear> allLeagueYears);
    Task<DropRequest?> GetDropRequest(Guid dropRequestID);

    Task<BidsAndDropsSet> GetPickupBidsAndDropsForProcessingSets(IEnumerable<ActionProcessingSetMetadata> processingSetsToInclude);
    Task InsertTopBidsAndDrops(IReadOnlyList<TopBidsAndDropsGame> topBidsAndDrops);

    Task<IReadOnlyList<QueuedGame>> GetQueuedGames(Publisher publisher);
    Task QueueGame(QueuedGame queuedGame);
    Task RemoveQueuedGame(QueuedGame queuedGame);
    Task SetQueueRankings(IReadOnlyList<KeyValuePair<QueuedGame, int>> queueRanks);

    Task AddLeagueAction(LeagueAction action);
    Task AddLeagueManagerAction(LeagueManagerAction action);
    Task<IReadOnlyList<LeagueAction>> GetLeagueActions(LeagueYear leagueYear);
    Task<IReadOnlyList<LeagueManagerAction>> GetLeagueManagerActions(LeagueYear leagueYear);
    Task<IReadOnlyList<LeagueAction>> GetLeagueActions(int year);
    Task ChangePublisherName(Publisher publisher, string publisherName);
    Task ChangePublisherIcon(Publisher publisher, string? publisherIcon);
    Task ChangePublisherSlogan(Publisher publisher, string? publisherSlogan);
    Task ChangeLeagueOptions(League league, string leagueName, bool publicLeague, bool testLeague, bool customRulesLeague);
    Task StartDraft(LeagueYear leagueYear);
    Task CompleteDraft(LeagueYear leagueYear);
    Task ResetDraft(LeagueYear leagueYear, Instant timestamp);

    Task SetDraftPause(LeagueYear leagueYear, bool pause);
    Task SetDraftOrder(IReadOnlyList<KeyValuePair<Publisher, int>> draftPositions, LeagueManagerAction draftSetAction);
    Task DeleteEligibilityOverride(LeagueYear leagueYear, MasterGame masterGame);
    Task SetEligibilityOverride(LeagueYear leagueYear, MasterGame masterGame, bool eligible);
    Task SetTagOverride(LeagueYear leagueYear, MasterGame masterGame, IEnumerable<MasterGameTag> requestedTags);

    Task<SystemWideValues> GetSystemWideValues();
    Task<SystemWideSettings> GetSystemWideSettings();
    Task<SiteCounts> GetSiteCounts();
    Task SetActionProcessingMode(bool modeOn);

    Task EditPublisher(EditPublisherRequest editValues, LeagueAction leagueAction);
    Task DeletePublisher(Publisher publisher);
    Task DeleteLeagueYear(LeagueYear leagueYear);
    Task DeleteLeague(League league);
    Task DeleteLeagueActions(Publisher publisher);

    Task<IReadOnlyList<ActionProcessingSetMetadata>> GetActionProcessingSets();
    Task SaveProcessedActionResults(FinalizedActionProcessingResults actionProcessingResults);
    Task ManualMakePublisherGameSlotsConsistent(int year);
    Task UpdateSystemWideValues(SystemWideValues systemWideValues);
    Task PostNewManagerMessage(LeagueYear leagueYear, ManagerMessage domainMessage);
    Task<IReadOnlyList<ManagerMessage>> GetManagerMessages(LeagueYear leagueYear);
    Task<Result> DeleteManagerMessage(LeagueYear leagueYear, Guid messageID);
    Task<Result> DismissManagerMessage(Guid messageId, Guid userId);
    Task FinishYear(SupportedYear supportedYear);
    Task EditPickupBid(PickupBid bid, PublisherGame? conditionalDropPublisherGame, uint bidAmount, bool allowIneligibleSlot);
    Task<FantasyCriticUser?> GetLeagueYearWinner(Guid leagueID, int year);
    Task CreateTrade(Trade trade);
    Task<IReadOnlyList<Trade>> GetTradesForLeague(LeagueYear leagueYear);
    Task<IReadOnlyList<Trade>> GetTradesForYear(int year);
    Task<Trade?> GetTrade(Guid tradeID);
    Task<Trade> EditTradeStatus(Trade trade, TradeStatus status, Instant? acceptedTimestamp, Instant? completedTimestamp);
    Task AddTradeVote(TradeVote tradeVote);
    Task DeleteTradeVote(Trade trade, FantasyCriticUser user);
    Task<Trade> ExecuteTrade(ExecutedTrade executedTrade);
    Task ExpireTrades(List<Trade> tradesToExpire, Instant expireTimestamp);
    Task<IReadOnlyList<SpecialAuction>> GetAllActiveSpecialAuctions();
    Task<IReadOnlyList<SpecialAuction>> GetSpecialAuctions(LeagueYear leagueYear);
    Task CreateSpecialAuction(SpecialAuction specialAuction, LeagueManagerAction action);
    Task CancelSpecialAuction(SpecialAuction specialAuction, LeagueManagerAction action);
    Task GrantSuperDrops(IEnumerable<Publisher> publishersToGrantSuperDrop, IEnumerable<LeagueAction> superDropActions);
    Task UpdateLeagueYearCache(IEnumerable<LeagueYear> allLeagueYears);
    Task<IReadOnlyList<MinimalPublisher>> GetMinimalPublishersForUser(Guid userID, int year);
    Task UpdateDailyPublisherStatistics(int year, LocalDate currentDate, SystemWideValues systemWideValues);
    Task<IReadOnlyList<SingleGameNews>> GetMyGameNews(FantasyCriticUser user);
    Task<bool> DraftIsActiveOrPaused(Guid leagueID, int year);
}
