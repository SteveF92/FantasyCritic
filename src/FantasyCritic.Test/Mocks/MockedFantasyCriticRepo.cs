using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Calculations;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.Trades;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using NodaTime;

namespace FantasyCritic.Test.Mocks;
public class MockedFantasyCriticRepo : IFantasyCriticRepo
{
    public Task<League?> GetLeague(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<LeagueYear?> GetLeagueYear(League requestLeague, int requestYear)
    {
        throw new NotImplementedException();
    }

    public Task<LeagueYearKey?> GetLeagueYearKeyForPublisherID(Guid publisherID)
    {
        throw new NotImplementedException();
    }

    public Task CreateLeague(League league, int initialYear, LeagueOptions options)
    {
        throw new NotImplementedException();
    }

    public Task AddNewLeagueYear(League league, int year, LeagueOptions options)
    {
        throw new NotImplementedException();
    }

    public Task EditLeagueYear(LeagueYear leagueYear, IReadOnlyDictionary<Guid, int> slotAssignments, LeagueAction settingsChangeAction)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<FantasyCriticUser>> GetUsersInLeague(Guid leagueID)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<FantasyCriticUser>> GetActivePlayersForLeagueYear(League league, int year)
    {
        throw new NotImplementedException();
    }

    public Task SetPlayersActive(League league, int year, IReadOnlyList<FantasyCriticUser> mostRecentActivePlayers)
    {
        throw new NotImplementedException();
    }

    public Task SetPlayerActiveStatus(LeagueYear leagueYear, Dictionary<FantasyCriticUser, bool> usersToChange)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<FantasyCriticUser>> GetLeagueFollowers(League league)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<League>> GetLeaguesForUser(FantasyCriticUser user)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<LeagueYear>> GetLeagueYearsForUser(FantasyCriticUser user, int year)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyDictionary<FantasyCriticUser, IReadOnlyList<LeagueYearKey>>> GetUsersWithLeagueYearsWithPublisher()
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<League>> GetFollowedLeagues(FantasyCriticUser user)
    {
        throw new NotImplementedException();
    }

    public Task FollowLeague(League league, FantasyCriticUser user)
    {
        throw new NotImplementedException();
    }

    public Task UnfollowLeague(League league, FantasyCriticUser user)
    {
        throw new NotImplementedException();
    }

    public Task<LeagueInvite?> GetInvite(Guid inviteID)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<LeagueInvite>> GetLeagueInvites(FantasyCriticUser currentUser)
    {
        throw new NotImplementedException();
    }

    public Task SetAutoDraft(Publisher publisher, bool autoDraft)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<LeagueInvite>> GetOutstandingInvitees(League league)
    {
        throw new NotImplementedException();
    }

    public Task SaveInvite(LeagueInvite leagueInvite)
    {
        throw new NotImplementedException();
    }

    public Task AcceptInvite(LeagueInvite leagueInvite, FantasyCriticUser user)
    {
        throw new NotImplementedException();
    }

    public Task DeleteInvite(LeagueInvite leagueInvite)
    {
        throw new NotImplementedException();
    }

    public Task AddPlayerToLeague(League league, FantasyCriticUser inviteUser)
    {
        throw new NotImplementedException();
    }

    public Task SaveInviteLink(LeagueInviteLink inviteLink)
    {
        throw new NotImplementedException();
    }

    public Task DeactivateInviteLink(LeagueInviteLink inviteID)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<LeagueInviteLink>> GetInviteLinks(League league)
    {
        throw new NotImplementedException();
    }

    public Task<LeagueInviteLink?> GetInviteLinkByInviteCode(Guid inviteCode)
    {
        throw new NotImplementedException();
    }

    public Task SetArchiveStatusForUser(League league, bool archive, FantasyCriticUser user)
    {
        throw new NotImplementedException();
    }

    public Task FullyRemovePublisher(LeagueYear leagueYear, Publisher deletePublisher)
    {
        throw new NotImplementedException();
    }

    public Task RemovePlayerFromLeague(League league, FantasyCriticUser removeUser)
    {
        throw new NotImplementedException();
    }

    public Task TransferLeagueManager(League league, FantasyCriticUser newManager)
    {
        throw new NotImplementedException();
    }

    public Task CreatePublisher(Publisher publisher)
    {
        throw new NotImplementedException();
    }

    public Task AddPublisherGame(PublisherGame publisherGame)
    {
        throw new NotImplementedException();
    }

    public Task AssociatePublisherGame(Publisher publisher, PublisherGame publisherGame, MasterGame masterGame)
    {
        throw new NotImplementedException();
    }

    public Task MergeMasterGame(MasterGame removeMasterGame, MasterGame mergeIntoMasterGame)
    {
        throw new NotImplementedException();
    }

    public Task ReorderPublisherGames(Publisher publisher, Dictionary<int, Guid?> slotStates)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<SupportedYear>> GetSupportedYears()
    {
        throw new NotImplementedException();
    }

    public Task<SupportedYear> GetSupportedYear(int year)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<LeagueYear>> GetLeagueYears(int year, bool includeDeleted = false)
    {
        throw new NotImplementedException();
    }

    public Task UpdatePublisherGameCalculatedStats(IReadOnlyDictionary<Guid, PublisherGameCalculatedStats> calculatedStats)
    {
        throw new NotImplementedException();
    }

    public Task UpdateLeagueWinners(IReadOnlyDictionary<LeagueYearKey, FantasyCriticUser> winningUsers)
    {
        throw new NotImplementedException();
    }

    public Task FullyRemovePublisherGame(LeagueYear leagueYear, Publisher publisher, PublisherGame publisherGame)
    {
        throw new NotImplementedException();
    }

    public Task ManagerRemovePublisherGame(LeagueYear leagueYear, Publisher publisher, PublisherGame publisherGame,
        FormerPublisherGame formerPublisherGame, LeagueAction leagueAction)
    {
        throw new NotImplementedException();
    }

    public Task SuperDropGame(LeagueYear leagueYear, Publisher publisher, PublisherGame publisherGame,
        FormerPublisherGame formerPublisherGame, LeagueAction leagueAction)
    {
        throw new NotImplementedException();
    }

    public Task ManuallyScoreGame(PublisherGame publisherGame, decimal? manualCriticScore)
    {
        throw new NotImplementedException();
    }

    public Task ManuallySetWillNotRelease(PublisherGame publisherGame, bool willNotRelease)
    {
        throw new NotImplementedException();
    }

    public Task CreatePickupBid(PickupBid currentBid)
    {
        throw new NotImplementedException();
    }

    public Task RemovePickupBid(PickupBid bid)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<PickupBid>> GetActivePickupBids(LeagueYear leagueYear, Publisher publisher)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>>> GetActivePickupBids(int year, IReadOnlyList<LeagueYear> leagueYears)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<PickupBid>> GetActivePickupBids(LeagueYear leagueYear)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<PickupBid>> GetProcessedPickupBids(int year, IReadOnlyList<LeagueYear> allLeagueYears)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<PickupBid>> GetProcessedPickupBids(LeagueYear leagueYear)
    {
        throw new NotImplementedException();
    }

    public Task<PickupBid?> GetPickupBid(Guid bidID)
    {
        throw new NotImplementedException();
    }

    public Task SetBidPriorityOrder(IReadOnlyList<KeyValuePair<PickupBid, int>> bidPriorities)
    {
        throw new NotImplementedException();
    }

    public Task CreateDropRequest(DropRequest currentDropRequest)
    {
        throw new NotImplementedException();
    }

    public Task RemoveDropRequest(DropRequest dropRequest)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<DropRequest>> GetActiveDropRequests(LeagueYear leagueYear, Publisher publisher)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyDictionary<LeagueYear, IReadOnlyList<DropRequest>>> GetActiveDropRequests(int year, IReadOnlyList<LeagueYear> allLeagueYears)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<DropRequest>> GetProcessedDropRequests(LeagueYear leagueYear)
    {
        throw new NotImplementedException();
    }

    public Task<DropRequest?> GetDropRequest(Guid dropRequestID)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<QueuedGame>> GetQueuedGames(Publisher publisher)
    {
        throw new NotImplementedException();
    }

    public Task QueueGame(QueuedGame queuedGame)
    {
        throw new NotImplementedException();
    }

    public Task RemoveQueuedGame(QueuedGame queuedGame)
    {
        throw new NotImplementedException();
    }

    public Task SetQueueRankings(IReadOnlyList<KeyValuePair<QueuedGame, int>> queueRanks)
    {
        throw new NotImplementedException();
    }

    public Task AddLeagueAction(LeagueAction action)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<LeagueAction>> GetLeagueActions(LeagueYear leagueYear)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<LeagueAction>> GetLeagueActions(int year)
    {
        throw new NotImplementedException();
    }

    public Task ChangePublisherName(Publisher publisher, string publisherName)
    {
        throw new NotImplementedException();
    }

    public Task ChangePublisherIcon(Publisher publisher, string? publisherIcon)
    {
        throw new NotImplementedException();
    }

    public Task ChangePublisherSlogan(Publisher publisher, string? publisherSlogan)
    {
        throw new NotImplementedException();
    }

    public Task ChangeLeagueOptions(League league, string leagueName, bool publicLeague, bool testLeague)
    {
        throw new NotImplementedException();
    }

    public Task StartDraft(LeagueYear leagueYear)
    {
        throw new NotImplementedException();
    }

    public Task CompleteDraft(LeagueYear leagueYear)
    {
        throw new NotImplementedException();
    }

    public Task ResetDraft(LeagueYear leagueYear, Instant timestamp)
    {
        throw new NotImplementedException();
    }

    public Task SetDraftPause(LeagueYear leagueYear, bool pause)
    {
        throw new NotImplementedException();
    }

    public Task SetDraftOrder(IReadOnlyList<KeyValuePair<Publisher, int>> draftPositions, LeagueAction draftSetAction)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<EligibilityOverride>> GetEligibilityOverrides(League league, int year)
    {
        throw new NotImplementedException();
    }

    public Task DeleteEligibilityOverride(LeagueYear leagueYear, MasterGame masterGame)
    {
        throw new NotImplementedException();
    }

    public Task SetEligibilityOverride(LeagueYear leagueYear, MasterGame masterGame, bool eligible)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TagOverride>> GetTagOverrides(League league, int year)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<MasterGameTag>> GetTagOverridesForGame(League league, int year, MasterGame masterGame)
    {
        throw new NotImplementedException();
    }

    public Task SetTagOverride(LeagueYear leagueYear, MasterGame masterGame, IEnumerable<MasterGameTag> requestedTags)
    {
        throw new NotImplementedException();
    }

    public Task<SystemWideValues> GetSystemWideValues()
    {
        throw new NotImplementedException();
    }

    public Task<SystemWideSettings> GetSystemWideSettings()
    {
        throw new NotImplementedException();
    }

    public Task<SiteCounts> GetSiteCounts()
    {
        throw new NotImplementedException();
    }

    public Task SetActionProcessingMode(bool modeOn)
    {
        throw new NotImplementedException();
    }

    public Task EditPublisher(EditPublisherRequest editValues, LeagueAction leagueAction)
    {
        throw new NotImplementedException();
    }

    public Task DeletePublisher(Publisher publisher)
    {
        throw new NotImplementedException();
    }

    public Task DeleteLeagueYear(LeagueYear leagueYear)
    {
        throw new NotImplementedException();
    }

    public Task DeleteLeague(League league)
    {
        throw new NotImplementedException();
    }

    public Task DeleteLeagueActions(Publisher publisher)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<ActionProcessingSetMetadata>> GetActionProcessingSets()
    {
        throw new NotImplementedException();
    }

    public Task SaveProcessedActionResults(FinalizedActionProcessingResults actionProcessingResults)
    {
        throw new NotImplementedException();
    }

    public Task ManualMakePublisherGameSlotsConsistent(int year)
    {
        throw new NotImplementedException();
    }

    public Task UpdateSystemWideValues(SystemWideValues systemWideValues)
    {
        throw new NotImplementedException();
    }

    public Task PostNewManagerMessage(LeagueYear leagueYear, ManagerMessage domainMessage)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<ManagerMessage>> GetManagerMessages(LeagueYear leagueYear)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteManagerMessage(LeagueYear leagueYear, Guid messageID)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DismissManagerMessage(Guid messageId, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task FinishYear(SupportedYear supportedYear)
    {
        throw new NotImplementedException();
    }

    public Task EditPickupBid(PickupBid bid, PublisherGame? conditionalDropPublisherGame, uint bidAmount)
    {
        throw new NotImplementedException();
    }

    public Task<FantasyCriticUser?> GetLeagueYearWinner(Guid leagueID, int year)
    {
        throw new NotImplementedException();
    }

    public Task CreateTrade(Trade trade)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Trade>> GetTradesForLeague(LeagueYear leagueYear)
    {
        throw new NotImplementedException();
    }

    public Task<Trade?> GetTrade(Guid tradeID)
    {
        throw new NotImplementedException();
    }

    public Task EditTradeStatus(Trade trade, TradeStatus status, Instant? acceptedTimestamp, Instant? completedTimestamp)
    {
        throw new NotImplementedException();
    }

    public Task AddTradeVote(TradeVote tradeVote)
    {
        throw new NotImplementedException();
    }

    public Task DeleteTradeVote(Trade trade, FantasyCriticUser user)
    {
        throw new NotImplementedException();
    }

    public Task ExecuteTrade(ExecutedTrade executedTrade)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<SpecialAuction>> GetAllActiveSpecialAuctions()
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<SpecialAuction>> GetSpecialAuctions(LeagueYear leagueYear)
    {
        throw new NotImplementedException();
    }

    public Task CreateSpecialAuction(SpecialAuction specialAuction, LeagueAction action)
    {
        throw new NotImplementedException();
    }

    public Task CancelSpecialAuction(SpecialAuction specialAuction, LeagueAction action)
    {
        throw new NotImplementedException();
    }

    public Task GrantSuperDrops(IEnumerable<Publisher> publishersToGrantSuperDrop, IEnumerable<LeagueAction> superDropActions)
    {
        throw new NotImplementedException();
    }
}
