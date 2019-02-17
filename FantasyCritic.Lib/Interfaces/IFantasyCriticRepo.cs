using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.OpenCritic;

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
        Task<IReadOnlyList<FantasyCriticUser>> GetLeagueFollowers(League league);
        Task<IReadOnlyList<League>> GetLeaguesForUser(FantasyCriticUser currentUser);
        Task<IReadOnlyList<League>> GetFollowedLeagues(FantasyCriticUser currentUser);
        Task FollowLeague(League league, FantasyCriticUser user);
        Task UnfollowLeague(League league, FantasyCriticUser user);

        Task<Maybe<LeagueInvite>> GetInvite(Guid inviteID);
        Task<IReadOnlyList<LeagueInvite>> GetLeagueInvites(FantasyCriticUser currentUser);
        Task<IReadOnlyList<LeagueInvite>> GetOutstandingInvitees(League league);
        Task SaveInvite(LeagueInvite leagueInvite);
        Task AcceptInvite(LeagueInvite leagueInvite, FantasyCriticUser user);
        Task DeleteInvite(LeagueInvite leagueInvite);

        Task RemovePublisher(Publisher deletePublisher, IEnumerable<Publisher> publishersInLeague);
        Task RemovePlayerFromLeague(League league, FantasyCriticUser removeUser);

        Task<Maybe<Publisher>> GetPublisher(Guid publisherID);
        Task<Maybe<Publisher>> GetPublisher(League league, int year, FantasyCriticUser user);
        Task<Maybe<PublisherGame>> GetPublisherGame(Guid publisherGameID);
        Task CreatePublisher(Publisher publisher);
        Task<IReadOnlyList<Publisher>> GetPublishersInLeagueForYear(League league, int year);
        Task<IReadOnlyList<Publisher>> GetAllPublishersForYear(int year);
        Task AddPublisherGame(PublisherGame publisherGame);
        Task AssociatePublisherGame(Publisher publisher, PublisherGame publisherGame, MasterGame masterGame);

        Task<IReadOnlyList<SupportedYear>> GetSupportedYears();

        Task<IReadOnlyList<LeagueYear>> GetLeagueYears(int year);
        Task UpdateFantasyPoints(Dictionary<Guid, decimal?> publisherGameScores);
        Task<Result> RemovePublisherGame(Guid publisherGameID);
        Task ManuallyScoreGame(PublisherGame publisherGame, decimal? manualCriticScore);

        Task CreatePickupBid(PickupBid currentBid);
        Task RemovePickupBid(PickupBid bid);
        Task<IReadOnlyList<PickupBid>> GetActivePickupBids(Publisher publisher);
        Task<IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>>> GetActivePickupBids(int year);
        Task<Maybe<PickupBid>> GetPickupBid(Guid bidID);
        Task AddLeagueAction(LeagueAction action);
        Task<IReadOnlyList<LeagueAction>> GetLeagueActions(LeagueYear leagueYear);
        Task ChangePublisherName(Publisher publisher, string publisherName);
        Task ChangeLeagueOptions(League league, string leagueName, bool publicLeague, bool testLeague);
        Task StartDraft(LeagueYear leagueYear);
        Task CompleteDraft(LeagueYear leagueYear);
        Task SetDraftPause(LeagueYear leagueYear, bool pause);
        Task SetDraftOrder(IEnumerable<KeyValuePair<Publisher, int>> draftPositions);
        Task<SystemWideValues> GetSystemWideValues();
        Task<SiteCounts> GetSiteCounts();

        Task DeletePublisher(Publisher publisher);
        Task DeleteLeagueYear(LeagueYear leagueYear);
        Task DeleteLeague(League league);
        Task DeleteLeagueActions(Publisher publisher);
        Task<bool> LeagueHasBeenStarted(Guid leagueID);

        Task SaveProcessedBidResults(BidProcessingResults bidProcessingResults);
    }
}
