using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.FakeRepo
{
    public class FakeFantasyCriticRepo : IFantasyCriticRepo
    {
        public Task<Maybe<League>> GetLeagueByID(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Maybe<LeagueYear>> GetLeagueYear(League requestLeague, int requestYear)
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

        public Task EditLeagueYear(LeagueYear leagueYear)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<League>> GetAllLeagues()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<FantasyCriticUser>> GetUsersInLeague(League league)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<FantasyCriticUser>> GetLeagueFollowers(League league)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<League>> GetLeaguesForUser(FantasyCriticUser currentUser)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<League>> GetFollowedLeagues(FantasyCriticUser currentUser)
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

        public Task<Maybe<LeagueInvite>> GetInvite(Guid inviteID)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<LeagueInvite>> GetLeagueInvites(FantasyCriticUser currentUser)
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

        public Task RemovePublisher(Publisher deletePublisher, IEnumerable<Publisher> publishersInLeague)
        {
            throw new NotImplementedException();
        }

        public Task RemovePlayerFromLeague(League league, FantasyCriticUser removeUser)
        {
            throw new NotImplementedException();
        }

        public Task<Maybe<Publisher>> GetPublisher(Guid publisherID)
        {
            throw new NotImplementedException();
        }

        public Task<Maybe<Publisher>> GetPublisher(League league, int year, FantasyCriticUser user)
        {
            throw new NotImplementedException();
        }

        public Task<Maybe<PublisherGame>> GetPublisherGame(Guid publisherGameID)
        {
            throw new NotImplementedException();
        }

        public Task CreatePublisher(Publisher publisher)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Publisher>> GetPublishersInLeagueForYear(League league, int year)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Publisher>> GetPublishersInLeagueForYear(League league, int year, IEnumerable<FantasyCriticUser> usersInLeague)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Publisher>> GetAllPublishersForYear(int year)
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

        public Task<IReadOnlyList<SupportedYear>> GetSupportedYears()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<LeagueYear>> GetLeagueYears(int year)
        {
            throw new NotImplementedException();
        }

        public Task UpdateFantasyPoints(Dictionary<Guid, decimal?> publisherGameScores)
        {
            throw new NotImplementedException();
        }

        public Task<Result> RemovePublisherGame(Guid publisherGameID)
        {
            throw new NotImplementedException();
        }

        public Task ManuallyScoreGame(PublisherGame publisherGame, decimal? manualCriticScore)
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

        public Task<IReadOnlyList<PickupBid>> GetActivePickupBids(Publisher publisher)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>>> GetActivePickupBids(int year)
        {
            throw new NotImplementedException();
        }

        public Task<Maybe<PickupBid>> GetPickupBid(Guid bidID)
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

        public Task ChangePublisherName(Publisher publisher, string publisherName)
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

        public Task SetDraftPause(LeagueYear leagueYear, bool pause)
        {
            throw new NotImplementedException();
        }

        public Task SetDraftOrder(IEnumerable<KeyValuePair<Publisher, int>> draftPositions)
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

        public Task SetBidProcessingMode(bool modeOn)
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

        public Task<bool> LeagueHasBeenStarted(Guid leagueID)
        {
            throw new NotImplementedException();
        }

        public Task SaveProcessedBidResults(BidProcessingResults bidProcessingResults)
        {
            throw new NotImplementedException();
        }

        public Task RefreshCaches()
        {
            throw new NotImplementedException();
        }
    }
}
