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

        public async Task<Publisher> CreatePublisher(League league, int year, FantasyCriticUser user, string publisherName, IEnumerable<Publisher> existingPublishers)
        {
            int draftPosition = 1;
            if (existingPublishers.Any())
            {
                draftPosition = existingPublishers.Max(x => x.DraftPosition) + 1;
            }

            Publisher publisher = new Publisher(Guid.NewGuid(), league, user, year, publisherName, draftPosition, new List<PublisherGame>(), 100);
            await _fantasyCriticRepo.CreatePublisher(publisher);
            return publisher;
        }

        public async Task<IReadOnlyList<Publisher>> GetPublishersInLeagueForYear(League league, int year)
        {
            var users = await _leagueMemberService.GetUsersInLeague(league);
            return await _fantasyCriticRepo.GetPublishersInLeagueForYear(league, year, users);
        }

        public Task<IReadOnlyList<Publisher>> GetPublishersInLeagueForYear(League league, int year, IEnumerable<FantasyCriticUser> users)
        {
            return _fantasyCriticRepo.GetPublishersInLeagueForYear(league, year, users);
        }

        public Task<Maybe<Publisher>> GetPublisher(League league, int year, FantasyCriticUser user)
        {
            return _fantasyCriticRepo.GetPublisher(league, year, user);
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
            IReadOnlyList<Publisher> allPublishers = await _fantasyCriticRepo.GetPublishersInLeagueForYear(leagueYear.League, leagueYear.Year);
            IReadOnlyList<Publisher> publishersForYear = allPublishers.Where(x => x.Year == leagueYear.Year).ToList();
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

        public async Task<Result> SetBidPriorityOrder(IReadOnlyList<KeyValuePair<PickupBid, int>> bidPriorities)
        {
            var requiredNumbers = Enumerable.Range(1, bidPriorities.Count).ToList();
            var requestedDraftNumbers = bidPriorities.Select(x => x.Value);
            bool allRequiredPresent = new HashSet<int>(requiredNumbers).SetEquals(requestedDraftNumbers);
            if (!allRequiredPresent)
            {
                return Result.Fail("Some of the positions are not valid.");
            }

            await _fantasyCriticRepo.SetBidPriorityOrder(bidPriorities);
            return Result.Ok();
        }
    }
}
