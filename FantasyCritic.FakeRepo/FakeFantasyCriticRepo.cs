using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.FakeRepo.Factories;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;
using NodaTime;

namespace FantasyCritic.FakeRepo
{
    public class FakeFantasyCriticRepo : IFantasyCriticRepo
    {
        private readonly FakeFantasyCriticUserStore _userStore;
        private readonly FakeMasterGameRepo _fakeMasterGameRepo;
        private readonly List<League> _leagues;
        private readonly List<LeagueYear> _leagueYears;
        private readonly Dictionary<League, List<FantasyCriticUser>> _usersInLeagues;
        private readonly List<Publisher> _publishers;
        private readonly List<PublisherGame> _publisherGames;

        public FakeFantasyCriticRepo(FakeFantasyCriticUserStore userStore, FakeMasterGameRepo fakeMasterGameRepo)
        {
            _userStore = userStore;
            _fakeMasterGameRepo = fakeMasterGameRepo;
            _leagues = LeagueFactory.GetLeagues();
            _leagueYears = LeagueFactory.GetLeagueYears();
            _usersInLeagues = LeagueFactory.GetUsersInLeagues();
            _publishers = PublisherFactory.GetPublishers();
            _publisherGames = PublisherFactory.GetPublisherGames();
        }

        public Task<Maybe<League>> GetLeagueByID(Guid id)
        {
            return Task.FromResult<Maybe<League>>(_leagues.SingleOrDefault(x => x.LeagueID == id));
        }

        public Task<Maybe<LeagueYear>> GetLeagueYear(League requestLeague, int requestYear)
        {
            return Task.FromResult<Maybe<LeagueYear>>(_leagueYears.SingleOrDefault(x => x.League.LeagueID == requestLeague.LeagueID && x.Year == requestYear));
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
            return Task.FromResult<IReadOnlyList<League>>(_leagues);
        }

        public Task<IReadOnlyList<FantasyCriticUser>> GetUsersInLeague(League league)
        {
            return Task.FromResult<IReadOnlyList<FantasyCriticUser>>(_usersInLeagues.Single(x => x.Key.Equals(league)).Value);
        }

        public Task<IReadOnlyList<FantasyCriticUser>> GetLeagueFollowers(League league)
        {
            return Task.FromResult<IReadOnlyList<FantasyCriticUser>>(new List<FantasyCriticUser>());
        }

        public Task<IReadOnlyList<League>> GetLeaguesForUser(FantasyCriticUser user)
        {
            var leaguesForUser = _usersInLeagues.Where(x => x.Value.Contains(user)).Select(x => x.Key).ToList();
            return Task.FromResult<IReadOnlyList<League>>(leaguesForUser);
        }

        public Task<IReadOnlyList<LeagueYear>> GetLeagueYearsForUser(FantasyCriticUser user, int year)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<League>> GetFollowedLeagues(FantasyCriticUser user)
        {
            return Task.FromResult<IReadOnlyList<League>>(new List<League>());
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
            return Task.FromResult<IReadOnlyList<LeagueInvite>>(new List<LeagueInvite>());
        }

        public Task<IReadOnlyList<LeagueInvite>> GetOutstandingInvitees(League league)
        {
            return Task.FromResult<IReadOnlyList<LeagueInvite>>(new List<LeagueInvite>());
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
            return Task.FromResult<Maybe<Publisher>>(_publishers.SingleOrDefault(x => x.PublisherID == publisherID));
        }

        public Task<Maybe<Publisher>> GetPublisher(LeagueYear leagueYear, FantasyCriticUser user)
        {
            var publisher = _publishers
                .Where(x => x.LeagueYear.Equals(leagueYear))
                .Where(x => x.User.Equals(user))
                .SingleOrDefault();
            return Task.FromResult<Maybe<Publisher>>(publisher);
        }

        public Task<Maybe<PublisherGame>> GetPublisherGame(Guid publisherGameID)
        {
            var publisherGame = _publisherGames.SingleOrDefault(x => x.PublisherGameID == publisherGameID);
            return Task.FromResult<Maybe<PublisherGame>>(publisherGame);
        }

        public Task CreatePublisher(Publisher publisher)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Publisher>> GetPublishersInLeagueForYear(LeagueYear leagueYear)
        {
            var publishers = _publishers
                .Where(x => x.LeagueYear.Equals(leagueYear))
                .ToList();
            return Task.FromResult<IReadOnlyList<Publisher>>(publishers);
        }

        public Task<IReadOnlyList<Publisher>> GetPublishersInLeagueForYear(LeagueYear leagueYear, IEnumerable<FantasyCriticUser> usersInLeague)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Publisher>> GetPublishersInLeagueForYear(League league, int year, IEnumerable<FantasyCriticUser> usersInLeague)
        {
            var publishers = _publishers
                .Where(x => x.LeagueYear.League.Equals(league))
                .Where(x => x.LeagueYear.Year == year)
                .ToList();
            return Task.FromResult<IReadOnlyList<Publisher>>(publishers);
        }

        public Task<IReadOnlyList<Publisher>> GetAllPublishersForYear(int year)
        {
            var publishers = _publishers
                .Where(x => x.LeagueYear.Year == year)
                .ToList();
            return Task.FromResult<IReadOnlyList<Publisher>>(publishers);
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
            return Task.FromResult<IReadOnlyList<SupportedYear>> (new List<SupportedYear>()
            {
                new SupportedYear(2019, true, true, new LocalDate(2018, 12, 7), false)
            });
        }

        public Task<IReadOnlyList<LeagueYear>> GetLeagueYears(int year)
        {
            var leagueYears = _leagueYears
                .Where(x => x.Year == year)
                .ToList();
            return Task.FromResult<IReadOnlyList<LeagueYear>>(leagueYears);
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
            return Task.FromResult<IReadOnlyList<PickupBid>>(new List<PickupBid>());
        }

        public Task<IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>>> GetActivePickupBids(int year)
        {
            IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> activePickupBids = new Dictionary<LeagueYear, IReadOnlyList<PickupBid>>();
            return Task.FromResult(activePickupBids);
        }

        public Task<Maybe<PickupBid>> GetPickupBid(Guid bidID)
        {
            Maybe<PickupBid> pickupBid = Maybe<PickupBid>.None;
            return Task.FromResult(pickupBid);
        }

        public Task AddLeagueAction(LeagueAction action)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<LeagueAction>> GetLeagueActions(LeagueYear leagueYear)
        {
            IReadOnlyList<LeagueAction> leagueActions = new List<LeagueAction>();
            return Task.FromResult(leagueActions);
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


        public Task SetDraftOrder(IReadOnlyList<KeyValuePair<Publisher, int>> draftPositions)
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

        public Task<SystemWideValues> GetSystemWideValues()
        {
            return Task.FromResult(new SystemWideValues(7m, -1m));
        }

        public Task<SystemWideSettings> GetSystemWideSettings()
        {
            return Task.FromResult(new SystemWideSettings(false));
        }

        public Task<SiteCounts> GetSiteCounts()
        {
            return Task.FromResult(new SiteCounts(3001, 1140, 300, 150));
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
            var leagueYears = _leagueYears.Where(x => x.League.LeagueID == leagueID);
            foreach (var leagueYear in leagueYears)
            {
                if (leagueYear.PlayStatus.PlayStarted)
                {
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }

        public Task SetBidPriorityOrder(IReadOnlyList<KeyValuePair<PickupBid, int>> bidPriorities)
        {
            throw new NotImplementedException();
        }

        public Task SaveProcessedBidResults(BidProcessingResults bidProcessingResults)
        {
            throw new NotImplementedException();
        }

        public Task UpdateSystemWideValues()
        {
            throw new NotImplementedException();
        }

        public Task<HypeConstants> GetHypeConstants()
        {
            throw new NotImplementedException();
        }
    }
}
