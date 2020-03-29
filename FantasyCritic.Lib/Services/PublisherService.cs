using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using NodaTime;

namespace FantasyCritic.Lib.Services
{
    public class PublisherService
    {
        private readonly IFantasyCriticRepo _fantasyCriticRepo;
        private readonly LeagueMemberService _leagueMemberService;
        private readonly IClock _clock;


        public PublisherService(IFantasyCriticRepo fantasyCriticRepo, LeagueMemberService leagueMemberService, IClock clock)
        {
            _fantasyCriticRepo = fantasyCriticRepo;
            _leagueMemberService = leagueMemberService;
            _clock = clock;
        }

        public async Task<Publisher> CreatePublisher(LeagueYear leagueYear, FantasyCriticUser user, string publisherName, IEnumerable<Publisher> existingPublishers)
        {
            int draftPosition = 1;
            if (existingPublishers.Any())
            {
                draftPosition = existingPublishers.Max(x => x.DraftPosition) + 1;
            }

            Publisher publisher = new Publisher(Guid.NewGuid(), leagueYear, user, publisherName, draftPosition, new List<PublisherGame>(), 100, 0, 0, 0, false);
            await _fantasyCriticRepo.CreatePublisher(publisher);
            return publisher;
        }

        public Task ChangePublisherName(Publisher publisher, string publisherName)
        {
            return _fantasyCriticRepo.ChangePublisherName(publisher, publisherName);
        }

        public Task SetAutoDraft(Publisher publisher, bool autoDraft)
        {
            return _fantasyCriticRepo.SetAutoDraft(publisher, autoDraft);
        }

        public async Task<IReadOnlyList<Publisher>> GetPublishersInLeagueForYear(LeagueYear leagueYear)
        {
            var users = await _leagueMemberService.GetUsersInLeague(leagueYear.League);
            return await _fantasyCriticRepo.GetPublishersInLeagueForYear(leagueYear, users);
        }

        public Task<IReadOnlyList<Publisher>> GetPublishersInLeagueForYear(LeagueYear leagueYear, IEnumerable<FantasyCriticUser> users)
        {
            return _fantasyCriticRepo.GetPublishersInLeagueForYear(leagueYear, users);
        }

        public Task<Maybe<Publisher>> GetPublisher(LeagueYear leagueYear, FantasyCriticUser user)
        {
            return _fantasyCriticRepo.GetPublisher(leagueYear, user);
        }

        public Task<Maybe<Publisher>> GetPublisher(Guid publisherID)
        {
            return _fantasyCriticRepo.GetPublisher(publisherID);
        }

        public Task<Maybe<PublisherGame>> GetPublisherGame(Guid publisherGameID)
        {
            return _fantasyCriticRepo.GetPublisherGame(publisherGameID);
        }

        public async Task<Result> RemovePublisherGame(LeagueYear leagueYear, Publisher publisher, PublisherGame publisherGame)
        {
            IReadOnlyList<Publisher> allPublishers = await _fantasyCriticRepo.GetPublishersInLeagueForYear(leagueYear);
            IReadOnlyList<Publisher> publishersForYear = allPublishers.Where(x => x.LeagueYear.Year == leagueYear.Year).ToList();
            IReadOnlyList<Publisher> otherPublishers = publishersForYear.Where(x => x.User.UserID != publisher.User.UserID).ToList();
            IReadOnlyList<PublisherGame> otherPlayersGames = otherPublishers.SelectMany(x => x.PublisherGames).ToList();

            bool otherPlayerHasCounterPick = otherPlayersGames.Where(x => x.CounterPick).ContainsGame(publisherGame);
            if (otherPlayerHasCounterPick)
            {
                return Result.Fail("Can't remove a publisher game that another player has as a counterPick.");
            }

            var result = await _fantasyCriticRepo.RemovePublisherGame(publisherGame.PublisherGameID);
            if (result.IsSuccess)
            {
                RemoveGameDomainRequest removeGameRequest = new RemoveGameDomainRequest(publisher, publisherGame);
                LeagueAction leagueAction = new LeagueAction(removeGameRequest, _clock.GetCurrentInstant());
                await _fantasyCriticRepo.AddLeagueAction(leagueAction);
            }

            return result;
        }

        public Task EditPublisher(EditPublisherRequest editValues)
        {
            LeagueAction leagueAction = new LeagueAction(editValues, _clock.GetCurrentInstant());
            return _fantasyCriticRepo.EditPublisher(editValues, leagueAction);
        }

        public async Task RemovePublisher(Publisher publisher)
        {
            var allPublishers = await _fantasyCriticRepo.GetPublishersInLeagueForYear(publisher.LeagueYear);
            await _fantasyCriticRepo.RemovePublisher(publisher, allPublishers);
        }

        public async Task<Result> SetBidPriorityOrder(IReadOnlyList<KeyValuePair<PickupBid, int>> bidPriorities)
        {
            var requiredNumbers = Enumerable.Range(1, bidPriorities.Count).ToList();
            var requestedBidNumbers = bidPriorities.Select(x => x.Value);
            bool allRequiredPresent = new HashSet<int>(requiredNumbers).SetEquals(requestedBidNumbers);
            if (!allRequiredPresent)
            {
                return Result.Fail("Some of the positions are not valid.");
            }

            await _fantasyCriticRepo.SetBidPriorityOrder(bidPriorities);
            return Result.Ok();
        }

        public async Task<Result> SetQueueRankings(IReadOnlyList<KeyValuePair<QueuedGame, int>> queueRanks)
        {
            var requiredNumbers = Enumerable.Range(1, queueRanks.Count).ToList();
            var requestedQueueNumbers = queueRanks.Select(x => x.Value);
            bool allRequiredPresent = new HashSet<int>(requiredNumbers).SetEquals(requestedQueueNumbers);
            if (!allRequiredPresent)
            {
                return Result.Fail("Some of the positions are not valid.");
            }

            await _fantasyCriticRepo.SetQueueRankings(queueRanks);
            return Result.Ok();
        }

        public async Task<IReadOnlyList<QueuedGame>> GetQueuedGames(Publisher publisher)
        {
            var queuedGames = await _fantasyCriticRepo.GetQueuedGames(publisher);
            return queuedGames.OrderBy(x => x.Rank).ToList();
        }
    }
}
