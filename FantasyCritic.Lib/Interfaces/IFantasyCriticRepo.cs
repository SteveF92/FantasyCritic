using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.OpenCritic;
using NodaTime;

namespace FantasyCritic.Lib.Interfaces
{
    public interface IFantasyCriticRepo
    {
        Task<Maybe<League>> GetLeagueByID(Guid id);
        Task<Maybe<LeagueYear>> GetLeagueYear(League requestLeague, int requestYear);
        Task CreateLeague(League league, int initialYear, LeagueOptions options);
        Task AddNewLeagueYear(League league, int year, LeagueOptions options);
        Task EditLeagueYear(LeagueYear leagueYear);

        Task<IReadOnlyList<League>> GetAllLeagues();
        Task<IReadOnlyList<FantasyCriticUser>> GetUsersInLeague(League league);
        Task<IReadOnlyList<FantasyCriticUser>> GetActivePlayersForLeagueYear(League league, int year);
        Task SetPlayersActive(League league, int year, IReadOnlyList<FantasyCriticUser> mostRecentActivePlayers);
        Task SetPlayerActiveStatus(LeagueYear leagueYear, Dictionary<FantasyCriticUser, bool> usersToChange);
        Task<IReadOnlyList<FantasyCriticUser>> GetLeagueFollowers(League league);
        Task<IReadOnlyList<League>> GetLeaguesForUser(FantasyCriticUser user);
        Task<IReadOnlyList<LeagueYear>> GetLeagueYearsForUser(FantasyCriticUser user, int year);
        Task<IReadOnlyList<League>> GetFollowedLeagues(FantasyCriticUser user);
        Task FollowLeague(League league, FantasyCriticUser user);
        Task UnfollowLeague(League league, FantasyCriticUser user);

        Task<Maybe<LeagueInvite>> GetInvite(Guid inviteID);
        Task<IReadOnlyList<LeagueInvite>> GetLeagueInvites(FantasyCriticUser currentUser);
        Task SetAutoDraft(Publisher publisher, bool autoDraft);
        Task<IReadOnlyList<LeagueInvite>> GetOutstandingInvitees(League league);
        Task SaveInvite(LeagueInvite leagueInvite);
        Task AcceptInvite(LeagueInvite leagueInvite, FantasyCriticUser user);
        Task DeleteInvite(LeagueInvite leagueInvite);
        Task AddPlayerToLeague(League league, FantasyCriticUser inviteUser);
        Task SaveInviteLink(LeagueInviteLink inviteLink);
        Task DeactivateInviteLink(LeagueInviteLink inviteID);
        Task<IReadOnlyList<LeagueInviteLink>> GetInviteLinks(League league);
        Task<Maybe<LeagueInviteLink>> GetInviteLinkByInviteCode(Guid inviteCode);
        Task SetArchiveStatusForUser(League league, bool archive, FantasyCriticUser user);

        Task RemovePublisher(Publisher deletePublisher, IEnumerable<Publisher> publishersInLeague);
        Task RemovePlayerFromLeague(League league, FantasyCriticUser removeUser);

        Task<Maybe<Publisher>> GetPublisher(Guid publisherID);
        Task<Maybe<Publisher>> GetPublisher(LeagueYear leagueYear, FantasyCriticUser user);
        Task<Maybe<PublisherGame>> GetPublisherGame(Guid publisherGameID);
        Task CreatePublisher(Publisher publisher);
        Task<IReadOnlyList<Publisher>> GetPublishersInLeagueForYear(LeagueYear leagueYear);
        Task<IReadOnlyList<Publisher>> GetPublishersInLeagueForYear(LeagueYear leagueYear, IEnumerable<FantasyCriticUser> usersInLeague);
        Task<IReadOnlyList<Publisher>> GetAllPublishersForYear(int year);
        Task AddPublisherGame(PublisherGame publisherGame);
        Task AssociatePublisherGame(Publisher publisher, PublisherGame publisherGame, MasterGame masterGame);
        Task MergeMasterGame(MasterGame removeMasterGame, MasterGame mergeIntoMasterGame);

        Task<IReadOnlyList<SupportedYear>> GetSupportedYears();

        Task<IReadOnlyList<LeagueYear>> GetLeagueYears(int year);
        Task UpdateFantasyPoints(Dictionary<Guid, decimal?> publisherGameScores);
        Task<Result> RemovePublisherGame(Guid publisherGameID);
        Task ManuallyScoreGame(PublisherGame publisherGame, decimal? manualCriticScore);
        Task ManuallySetWillNotRelease(PublisherGame publisherGame, bool willNotRelease);

        Task CreatePickupBid(PickupBid currentBid);
        Task RemovePickupBid(PickupBid bid);
        Task<IReadOnlyList<PickupBid>> GetActivePickupBids(Publisher publisher);
        Task<IReadOnlyList<PickupBid>> GetProcessedPickupBids(int year);
        Task<IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>>> GetActivePickupBids(int year);
        Task<Maybe<PickupBid>> GetPickupBid(Guid bidID);
        Task SetBidPriorityOrder(IReadOnlyList<KeyValuePair<PickupBid, int>> bidPriorities);

        Task CreateDropRequest(DropRequest currentDropRequest);
        Task RemoveDropRequest(DropRequest dropRequest);
        Task<IReadOnlyList<DropRequest>> GetActiveDropRequests(Publisher publisher);
        Task<IReadOnlyDictionary<LeagueYear, IReadOnlyList<DropRequest>>> GetActiveDropRequests(int year);
        Task<Maybe<DropRequest>> GetDropRequest(Guid dropRequestID);

        Task<IReadOnlyList<QueuedGame>> GetQueuedGames(Publisher publisher);
        Task QueueGame(QueuedGame queuedGame);
        Task RemoveQueuedGame(QueuedGame queuedGame);
        Task SetQueueRankings(IReadOnlyList<KeyValuePair<QueuedGame, int>> queueRanks);

        Task AddLeagueAction(LeagueAction action);
        Task<IReadOnlyList<LeagueAction>> GetLeagueActions(LeagueYear leagueYear);
        Task ChangePublisherName(Publisher publisher, string publisherName);
        Task ChangeLeagueOptions(League league, string leagueName, bool publicLeague, bool testLeague);
        Task StartDraft(LeagueYear leagueYear);
        Task CompleteDraft(LeagueYear leagueYear);
        Task ResetDraft(LeagueYear leagueYear);

        Task SetDraftPause(LeagueYear leagueYear, bool pause);
        Task SetDraftOrder(IReadOnlyList<KeyValuePair<Publisher, int>> draftPositions);
        Task<IReadOnlyList<EligibilityOverride>> GetEligibilityOverrides(League league, int year);
        Task DeleteEligibilityOverride(LeagueYear leagueYear, MasterGame masterGame);
        Task SetEligibilityOverride(LeagueYear leagueYear, MasterGame masterGame, bool eligible);

        Task<SystemWideValues> GetSystemWideValues();
        Task<SystemWideSettings> GetSystemWideSettings();
        Task<SiteCounts> GetSiteCounts();
        Task SetBidProcessingMode(bool modeOn);

        Task EditPublisher(EditPublisherRequest editValues, LeagueAction leagueAction);
        Task DeletePublisher(Publisher publisher);
        Task DeleteLeagueYear(LeagueYear leagueYear);
        Task DeleteLeague(League league);
        Task DeleteLeagueActions(Publisher publisher);
        Task<bool> LeagueHasBeenStarted(Guid leagueID);
        
        Task SaveProcessedBidResults(BidProcessingResults bidProcessingResults);
        Task SaveProcessedDropResults(DropProcessingResults dropProcessingResults);
        Task UpdateSystemWideValues(SystemWideValues systemWideValues);
    }
}
