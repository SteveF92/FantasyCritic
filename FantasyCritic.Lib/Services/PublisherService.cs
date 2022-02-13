using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
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

            Publisher publisher = new Publisher(Guid.NewGuid(), leagueYear, user, publisherName, null, draftPosition, new List<PublisherGame>(), 100, 0, 0, 0, false);
            await _fantasyCriticRepo.CreatePublisher(publisher);
            return publisher;
        }

        public Task ChangePublisherName(Publisher publisher, string publisherName)
        {
            return _fantasyCriticRepo.ChangePublisherName(publisher, publisherName);
        }

        public Task ChangePublisherIcon(Publisher publisher, Maybe<string> publisherIcon)
        {
            return _fantasyCriticRepo.ChangePublisherIcon(publisher, publisherIcon);
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

        public async Task<Result> RemovePublisherGame(Publisher publisher, PublisherGame publisherGame)
        {
            var now = _clock.GetCurrentInstant();
            var formerPublisherGame = publisherGame.GetFormerPublisherGame(now, "Removed by league manager.");
            var result = await _fantasyCriticRepo.RemovePublisherGame(publisherGame);
            if (result.IsSuccess)
            {
                RemoveGameDomainRequest removeGameRequest = new RemoveGameDomainRequest(publisher, publisherGame);
                LeagueAction leagueAction = new LeagueAction(removeGameRequest, now);
                await _fantasyCriticRepo.AddLeagueAction(leagueAction);
                await _fantasyCriticRepo.AddFormerPublisherGame(formerPublisherGame);
            }

            return result;
        }

        public async Task<Result> EditPublisher(EditPublisherRequest editValues)
        {
            if (!editValues.SomethingChanged())
            {
                return Result.Failure("You need to specify something to change.");
            }

            if (editValues.Budget.HasValue && editValues.Budget.Value > 100)
            {
                return Result.Failure("Budget cannot be set to over $100.");
            }

            if (editValues.Budget.HasValue && editValues.Budget.Value < 0)
            {
                return Result.Failure("Budget cannot be set to under $0.");
            }

            bool allowUnlimitedWillReleaseDrops = editValues.Publisher.LeagueYear.Options.WillReleaseDroppableGames == -1;
            if (editValues.WillReleaseGamesDropped.HasValue)
            {
                if (allowUnlimitedWillReleaseDrops)
                {
                    return Result.Failure("Your league allows unlimited will release drops, so there is no reason to edit this.");
                }
                if (editValues.WillReleaseGamesDropped.Value > editValues.Publisher.LeagueYear.Options.WillReleaseDroppableGames)
                {
                    return Result.Failure("Will release games dropped cannot be set to more than is allowed in the league.");
                }
            }

            bool allowUnlimitedWillNotReleaseDrops = editValues.Publisher.LeagueYear.Options.WillNotReleaseDroppableGames == -1;
            if (editValues.WillNotReleaseGamesDropped.HasValue)
            {
                if (allowUnlimitedWillNotReleaseDrops)
                {
                    return Result.Failure("Your league allows unlimited will not release drops, so there is no reason to edit this.");
                }
                if (editValues.WillNotReleaseGamesDropped.Value > editValues.Publisher.LeagueYear.Options.WillNotReleaseDroppableGames)
                {
                    return Result.Failure("Will not release games dropped cannot be set to more than is allowed in the league.");
                }
            }

            bool allowUnlimitedFreeDrops = editValues.Publisher.LeagueYear.Options.FreeDroppableGames == -1;
            if (editValues.FreeGamesDropped.HasValue)
            {
                if (allowUnlimitedFreeDrops)
                {
                    return Result.Failure("Your league allows unlimited unrestricted drops, so there is no reason to edit this.");
                }
                if (editValues.FreeGamesDropped.Value > editValues.Publisher.LeagueYear.Options.FreeDroppableGames)
                {
                    return Result.Failure("Unrestricted games dropped cannot be set to more than is allowed in the league.");
                }
            }

            LeagueAction leagueAction = new LeagueAction(editValues, _clock.GetCurrentInstant());
            await _fantasyCriticRepo.EditPublisher(editValues, leagueAction);
            return Result.Success();
        }

        public async Task FullyRemovePublisher(Publisher publisher)
        {
            var allPublishers = await _fantasyCriticRepo.GetPublishersInLeagueForYear(publisher.LeagueYear);
            await _fantasyCriticRepo.FullyRemovePublisher(publisher, allPublishers);
        }

        public async Task<Result> SetBidPriorityOrder(IReadOnlyList<KeyValuePair<PickupBid, int>> bidPriorities)
        {
            var requiredNumbers = Enumerable.Range(1, bidPriorities.Count).ToList();
            var requestedBidNumbers = bidPriorities.Select(x => x.Value);
            bool allRequiredPresent = new HashSet<int>(requiredNumbers).SetEquals(requestedBidNumbers);
            if (!allRequiredPresent)
            {
                return Result.Failure("Some of the positions are not valid.");
            }

            await _fantasyCriticRepo.SetBidPriorityOrder(bidPriorities);
            return Result.Success();
        }

        public async Task<Result> SetQueueRankings(IReadOnlyList<KeyValuePair<QueuedGame, int>> queueRanks)
        {
            var requiredNumbers = Enumerable.Range(1, queueRanks.Count).ToList();
            var requestedQueueNumbers = queueRanks.Select(x => x.Value);
            bool allRequiredPresent = new HashSet<int>(requiredNumbers).SetEquals(requestedQueueNumbers);
            if (!allRequiredPresent)
            {
                return Result.Failure("Some of the positions are not valid.");
            }

            await _fantasyCriticRepo.SetQueueRankings(queueRanks);
            return Result.Success();
        }

        public async Task<IReadOnlyList<QueuedGame>> GetQueuedGames(Publisher publisher)
        {
            var queuedGames = await _fantasyCriticRepo.GetQueuedGames(publisher);
            return queuedGames.OrderBy(x => x.Rank).ToList();
        }

        public Task ReorderPublisherGames(Publisher publisher, Dictionary<int, Guid?> slotStates)
        {
            return _fantasyCriticRepo.ReorderPublisherGames(publisher, slotStates);
        }
    }
}
