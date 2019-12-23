using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using Microsoft.AspNetCore.Identity;
using MoreLinq;
using NodaTime;

namespace FantasyCritic.Lib.Services
{
    public class FantasyCriticService
    {
        private readonly IFantasyCriticRepo _fantasyCriticRepo;
        private readonly IClock _clock;
        private readonly GameAcquisitionService _gameAcquisitionService;
        private readonly LeagueMemberService _leagueMemberService;
        private readonly PublisherService _publisherService;
        private readonly InterLeagueService _interLeagueService;
        private readonly ActionProcessingService _actionProcessingService;

        public FantasyCriticService(GameAcquisitionService gameAcquisitionService, LeagueMemberService leagueMemberService, 
            PublisherService publisherService, InterLeagueService interLeagueService, IFantasyCriticRepo fantasyCriticRepo, IClock clock, ActionProcessingService actionProcessingService)
        {
            _fantasyCriticRepo = fantasyCriticRepo;
            _clock = clock;

            _leagueMemberService = leagueMemberService;
            _publisherService = publisherService;
            _interLeagueService = interLeagueService;
            _gameAcquisitionService = gameAcquisitionService;
            _actionProcessingService = actionProcessingService;
        }

        public Task<Maybe<League>> GetLeagueByID(Guid id)
        {
            return _fantasyCriticRepo.GetLeagueByID(id);
        }

        public async Task<Maybe<LeagueYear>> GetLeagueYear(Guid id, int year)
        {
            var league = await GetLeagueByID(id);
            if (league.HasNoValue)
            {
                return Maybe<LeagueYear>.None;
            }

            var options = await _fantasyCriticRepo.GetLeagueYear(league.Value, year);
            return options;
        }

        public Task<IReadOnlyList<LeagueYear>> GetLeagueYears(int year)
        {
            return _fantasyCriticRepo.GetLeagueYears(year);
        }

        public async Task<Result<League>> CreateLeague(LeagueCreationParameters parameters)
        {
            LeagueOptions options = new LeagueOptions(parameters);

            var validateOptions = options.Validate();
            if (validateOptions.IsFailure)
            {
                return Result.Fail<League>(validateOptions.Error);
            }

            IEnumerable<int> years = new List<int>() { parameters.InitialYear };
            League newLeague = new League(Guid.NewGuid(), parameters.LeagueName, parameters.Manager, years, parameters.PublicLeague, parameters.TestLeague, 0);
            await _fantasyCriticRepo.CreateLeague(newLeague, parameters.InitialYear, options);
            return Result.Ok(newLeague);
        }

        public async Task<Result> EditLeague(League league, EditLeagueYearParameters parameters)
        {
            LeagueOptions options = new LeagueOptions(parameters);
            var validateOptions = options.Validate();
            if (validateOptions.IsFailure)
            {
                return Result.Fail(validateOptions.Error);
            }

            var leagueYear = await GetLeagueYear(league.LeagueID, parameters.Year);
            if (leagueYear.HasNoValue)
            {
                throw new Exception($"League year cannot be found: {parameters.LeagueID}|{parameters.Year}");
            }

            IReadOnlyList<Publisher> publishers = await _publisherService.GetPublishersInLeagueForYear(leagueYear.Value);

            int maxStandardGames = publishers.Select(publisher => publisher.PublisherGames.Count(x => !x.CounterPick)).DefaultIfEmpty(0).Max();
            int maxCounterPicks = publishers.Select(publisher => publisher.PublisherGames.Count(x => x.CounterPick)).DefaultIfEmpty(0).Max();

            if (maxStandardGames > options.StandardGames)
            {
                return Result.Fail($"Cannot reduce number of standard games to {options.StandardGames} as a publisher has {maxStandardGames} draft games currently.");
            }
            if (maxCounterPicks > options.CounterPicks)
            {
                return Result.Fail($"Cannot reduce number of counter picks to {options.CounterPicks} as a publisher has {maxCounterPicks} counter picks currently.");
            }

            int maxFreeGamesFreeDropped = publishers.Select(publisher => publisher.FreeGamesDropped).DefaultIfEmpty(0).Max();
            int maxWillNotReleaseGamesDropped = publishers.Select(publisher => publisher.WillNotReleaseGamesDropped).DefaultIfEmpty(0).Max();
            int maxWillReleaseGamesDropped = publishers.Select(publisher => publisher.WillReleaseGamesDropped).DefaultIfEmpty(0).Max();

            if (maxFreeGamesFreeDropped > options.FreeDroppableGames && options.FreeDroppableGames != -1)
            {
                return Result.Fail($"Cannot reduce number of unrestricted droppable games to {options.FreeDroppableGames} as a publisher has already dropped {maxFreeGamesFreeDropped} games.");
            }
            if (maxWillNotReleaseGamesDropped > options.WillNotReleaseDroppableGames && options.WillNotReleaseDroppableGames != -1)
            {
                return Result.Fail($"Cannot reduce number of 'will not release' droppable games to {options.WillNotReleaseDroppableGames} as a publisher has already dropped {maxWillNotReleaseGamesDropped} games.");
            }
            if (maxWillReleaseGamesDropped > options.WillReleaseDroppableGames && options.WillReleaseDroppableGames != -1)
            {
                return Result.Fail($"Cannot reduce number of 'will release' droppable games to {options.WillReleaseDroppableGames} as a publisher has already dropped {maxWillReleaseGamesDropped} games.");
            }

            var eligibilityOverrides = await GetEligibilityOverrides(league, parameters.Year);

            LeagueYear newLeagueYear = new LeagueYear(league, parameters.Year, options, leagueYear.Value.PlayStatus, eligibilityOverrides);
            await _fantasyCriticRepo.EditLeagueYear(newLeagueYear);

            return Result.Ok();
        }

        public Task<IReadOnlyList<EligibilityOverride>> GetEligibilityOverrides(League league, int year)
        {
            return _fantasyCriticRepo.GetEligibilityOverrides(league, year);
        }

        public async Task AddNewLeagueYear(League league, int year, LeagueOptions options, LeagueYear mostRecentLeagueYear)
        {
            await _fantasyCriticRepo.AddNewLeagueYear(league, year, options);
            var mostRecentActivePlayers = await _fantasyCriticRepo.GetActivePlayersForLeagueYear(league, mostRecentLeagueYear.Year);
            await _fantasyCriticRepo.SetPlayersActive(league, year, mostRecentActivePlayers);
        }

        public async Task<ClaimResult> ClaimGame(ClaimGameDomainRequest request, bool managerAction, bool draft)
        {
            Maybe<MasterGameYear> masterGameYear = Maybe<MasterGameYear>.None;
            if (request.MasterGame.HasValue)
            {
                masterGameYear = new MasterGameYear(request.MasterGame.Value, request.Publisher.LeagueYear.Year);
            }

            PublisherGame playerGame = new PublisherGame(request.Publisher.PublisherID, Guid.NewGuid(), request.GameName, _clock.GetCurrentInstant(), request.CounterPick, null, null, 
                masterGameYear, request.DraftPosition, request.OverallDraftPosition);

            var supportedYears = await _fantasyCriticRepo.GetSupportedYears();
            LeagueYear leagueYear = request.Publisher.LeagueYear;

            var publishersForYear = await _fantasyCriticRepo.GetPublishersInLeagueForYear(request.Publisher.LeagueYear);
            ClaimResult claimResult = _gameAcquisitionService.CanClaimGame(request, supportedYears, leagueYear, publishersForYear);

            if (!claimResult.Success)
            {
                return claimResult;
            }

            LeagueAction leagueAction = new LeagueAction(request, _clock.GetCurrentInstant(), managerAction, draft);
            await _fantasyCriticRepo.AddLeagueAction(leagueAction);

            await _fantasyCriticRepo.AddPublisherGame(playerGame);

            return claimResult;
        }

        public async Task<ClaimResult> AssociateGame(AssociateGameDomainRequest request)
        {
            ClaimResult claimResult = await _gameAcquisitionService.CanAssociateGame(request);

            if (!claimResult.Success)
            {
                return claimResult;
            }

            LeagueAction leagueAction = new LeagueAction(request, _clock.GetCurrentInstant());
            await _fantasyCriticRepo.AddLeagueAction(leagueAction);

            await _fantasyCriticRepo.AssociatePublisherGame(request.Publisher, request.PublisherGame, request.MasterGame);

            return claimResult;
        }

        public async Task<ClaimResult> MakePickupBid(Publisher publisher, MasterGame masterGame, uint bidAmount)
        {
            if (bidAmount > publisher.Budget)
            {
                return new ClaimResult(new List<ClaimError>(){new ClaimError("You do not have enough budget to make that bid.", false)});
            }

            IReadOnlyList<PickupBid> pickupBids = await _fantasyCriticRepo.GetActivePickupBids(publisher);
            bool alreadyBidFor = pickupBids.Select(x => x.MasterGame.MasterGameID).Contains(masterGame.MasterGameID);
            if (alreadyBidFor)
            {
                return new ClaimResult(new List<ClaimError>() { new ClaimError("You cannot have two active bids for the same game.", false) });
            }

            var claimRequest = new ClaimGameDomainRequest(publisher, masterGame.GameName, false, false, masterGame, null, null);
            var supportedYears = await _fantasyCriticRepo.GetSupportedYears();

            var leagueYear = publisher.LeagueYear;
            var publishersForYear = await _fantasyCriticRepo.GetPublishersInLeagueForYear(publisher.LeagueYear);

            var claimResult = _gameAcquisitionService.CanClaimGame(claimRequest, supportedYears, leagueYear, publishersForYear);
            if (!claimResult.Success)
            {
                return claimResult;
            }

            var nextPriority = pickupBids.Count + 1;

            PickupBid currentBid = new PickupBid(Guid.NewGuid(), publisher, leagueYear, masterGame, bidAmount, nextPriority, _clock.GetCurrentInstant(), null);
            await _fantasyCriticRepo.CreatePickupBid(currentBid);

            return claimResult;
        }

        public async Task<IReadOnlyList<QueuedGame>> GetQueuedGames(Publisher publisher)
        {
            var queuedGames = await _fantasyCriticRepo.GetQueuedGames(publisher);
            return queuedGames.OrderByDescending(x => x.Rank).ToList();
        }

        public async Task<ClaimResult> QueueGame(Publisher publisher, MasterGame masterGame)
        {
            IReadOnlyList<QueuedGame> queuedGames = await _fantasyCriticRepo.GetQueuedGames(publisher);
            bool alreadyQueued = queuedGames.Select(x => x.MasterGame.MasterGameID).Contains(masterGame.MasterGameID);
            if (alreadyQueued)
            {
                return new ClaimResult(new List<ClaimError>() { new ClaimError("You already have that game queued.", false) });
            }

            var claimRequest = new ClaimGameDomainRequest(publisher, masterGame.GameName, false, false, masterGame, null, null);
            var supportedYears = await _fantasyCriticRepo.GetSupportedYears();

            var leagueYear = publisher.LeagueYear;
            var publishersForYear = await _fantasyCriticRepo.GetPublishersInLeagueForYear(publisher.LeagueYear);

            var claimResult = _gameAcquisitionService.CanClaimGame(claimRequest, supportedYears, leagueYear, publishersForYear);
            if (!claimResult.Success)
            {
                return claimResult;
            }

            var nextRank = queuedGames.Count + 1;
            QueuedGame queuedGame = new QueuedGame(publisher, masterGame, nextRank);
            await _fantasyCriticRepo.QueueGame(queuedGame);

            return claimResult;
        }

        public Task RemoveQueuedGame(QueuedGame queuedGame)
        {
            return _fantasyCriticRepo.RemoveQueuedGame(queuedGame);
        }

        public async Task<DropResult> MakeDropRequest(Publisher publisher, PublisherGame publisherGame)
        {
            if (publisherGame.CounterPick)
            {
                return new DropResult(Result.Fail("You can't drop a counterpick."), false);
            }

            if (publisherGame.MasterGame.HasNoValue)
            {
                return new DropResult(Result.Fail("You can't drop a game that is not linked to a master game."), false);
            }

            MasterGame masterGame = publisherGame.MasterGame.Value.MasterGame;
            IReadOnlyList<DropRequest> dropRequests = await _fantasyCriticRepo.GetActiveDropRequests(publisher);
            bool alreadyDropping = dropRequests.Select(x => x.MasterGame.MasterGameID).Contains(masterGame.MasterGameID);
            if (alreadyDropping)
            {
                return new DropResult(Result.Fail("You cannot have two active drop requests for the same game."), false);
            }

            DropRequest dropRequest = new DropRequest(Guid.NewGuid(), publisher, publisher.LeagueYear, masterGame, _clock.GetCurrentInstant(), null);
            var supportedYears = await _fantasyCriticRepo.GetSupportedYears();

            var dropResult = _gameAcquisitionService.CanDropGame(dropRequest, supportedYears, publisher.LeagueYear, publisher);
            if (dropResult.Result.IsFailure)
            {
                return dropResult;
            }

            await _fantasyCriticRepo.CreateDropRequest(dropRequest);

            return dropResult;
        }

        public Task<IReadOnlyList<PickupBid>> GetActiveAcquistitionBids(Publisher publisher)
        {
            return _fantasyCriticRepo.GetActivePickupBids(publisher);
        }

        public Task<IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>>> GetActiveAcquistitionBids(SupportedYear supportedYear)
        {
            return _fantasyCriticRepo.GetActivePickupBids(supportedYear.Year);
        }

        public Task<IReadOnlyList<DropRequest>> GetActiveDropRequests(Publisher publisher)
        {
            return _fantasyCriticRepo.GetActiveDropRequests(publisher);
        }

        public Task<IReadOnlyDictionary<LeagueYear, IReadOnlyList<DropRequest>>> GetActiveDropRequests(SupportedYear supportedYear)
        {
            return _fantasyCriticRepo.GetActiveDropRequests(supportedYear.Year);
        }

        public Task<Maybe<PickupBid>> GetPickupBid(Guid bidID)
        {
            return _fantasyCriticRepo.GetPickupBid(bidID);
        }

        public Task<Maybe<DropRequest>> GetDropRequest(Guid dropRequest)
        {
            return _fantasyCriticRepo.GetDropRequest(dropRequest);
        }

        public async Task<Result> RemovePickupBid(PickupBid bid)
        {
            if (bid.Successful != null)
            {
                return Result.Fail("Bid has already been processed");
            }

            await _fantasyCriticRepo.RemovePickupBid(bid);
            return Result.Ok();
        }

        public async Task<Result> RemoveDropRequest(DropRequest dropRequest)
        {
            if (dropRequest.Successful != null)
            {
                return Result.Fail("Drop request has already been processed");
            }

            await _fantasyCriticRepo.RemoveDropRequest(dropRequest);
            return Result.Ok();
        }

        public async Task UpdateFantasyPoints(int year)
        {
            Dictionary<Guid, decimal?> publisherGameScores = new Dictionary<Guid, decimal?>();

            IReadOnlyList<LeagueYear> activeLeagueYears = await GetLeagueYears(year);
            Dictionary<LeagueYearKey, LeagueYear> leagueYearDictionary = activeLeagueYears.ToDictionary(x => x.Key, y => y);
            IReadOnlyList<Publisher> allPublishersForYear = await _fantasyCriticRepo.GetAllPublishersForYear(year);

            foreach (var publisher in allPublishersForYear)
            {
                var key = new LeagueYearKey(publisher.LeagueYear.League.LeagueID, publisher.LeagueYear.Year);
                foreach (var publisherGame in publisher.PublisherGames)
                {
                    var leagueYear = leagueYearDictionary[key];
                    decimal? fantasyPoints = publisherGame.CalculateFantasyPoints(leagueYear.Options.ScoringSystem, _clock);
                    publisherGameScores.Add(publisherGame.PublisherGameID, fantasyPoints);
                }
            }

            await _fantasyCriticRepo.UpdateFantasyPoints(publisherGameScores);
        }

        public async Task UpdateFantasyPoints(LeagueYear leagueYear)
        {
            Dictionary<Guid, decimal?> publisherGameScores = new Dictionary<Guid, decimal?>();

            var publishersInLeague = await _publisherService.GetPublishersInLeagueForYear(leagueYear);
            foreach (var publisher in publishersInLeague)
            {
                foreach (var publisherGame in publisher.PublisherGames)
                {
                    decimal? fantasyPoints = publisherGame.CalculateFantasyPoints(leagueYear.Options.ScoringSystem, _clock);
                    publisherGameScores.Add(publisherGame.PublisherGameID, fantasyPoints);
                }
            }

            await _fantasyCriticRepo.UpdateFantasyPoints(publisherGameScores);
        }

        public Task ManuallyScoreGame(PublisherGame publisherGame, decimal? manualCriticScore)
        {
            return _fantasyCriticRepo.ManuallyScoreGame(publisherGame, manualCriticScore);
        }

        public Task<IReadOnlyList<LeagueAction>> GetLeagueActions(LeagueYear leagueYear)
        {
            return _fantasyCriticRepo.GetLeagueActions(leagueYear);
        }

        public async Task<BidProcessingResults> GetBidProcessingDryRun(SystemWideValues systemWideValues, int year)
        {
            IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> leaguesAndBids = await _fantasyCriticRepo.GetActivePickupBids(year);
            IReadOnlyList<Publisher> allPublishers = await _fantasyCriticRepo.GetAllPublishersForYear(year);
            var supportedYears = await _fantasyCriticRepo.GetSupportedYears();

            IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> onlyLeaguesWithBids = leaguesAndBids.Where(x => x.Value.Any()).ToDictionary(x => x.Key, y => y.Value);
            var publishersInLeagues = allPublishers.Where(x => onlyLeaguesWithBids.ContainsKey(x.LeagueYear));
            BidProcessingResults results = _actionProcessingService.ProcessPickupsIteration(systemWideValues, onlyLeaguesWithBids, publishersInLeagues, _clock, supportedYears);

            return results;
        }

        public async Task ProcessPickups(SystemWideValues systemWideValues, int year)
        {
            var results = await GetBidProcessingDryRun(systemWideValues, year);
            await _fantasyCriticRepo.SaveProcessedBidResults(results);
        }

        public async Task<DropProcessingResults> GetDropProcessingDryRun(int year)
        {
            IReadOnlyDictionary<LeagueYear, IReadOnlyList<DropRequest>> leaguesAndDropRequests = await _fantasyCriticRepo.GetActiveDropRequests(year);
            IReadOnlyList<Publisher> allPublishers = await _fantasyCriticRepo.GetAllPublishersForYear(year);
            var supportedYears = await _fantasyCriticRepo.GetSupportedYears();

            IReadOnlyDictionary<LeagueYear, IReadOnlyList<DropRequest>> onlyLeaguesWithDrops = leaguesAndDropRequests.Where(x => x.Value.Any()).ToDictionary(x => x.Key, y => y.Value);
            var publishersInLeagues = allPublishers.Where(x => onlyLeaguesWithDrops.ContainsKey(x.LeagueYear));
            DropProcessingResults results = _actionProcessingService.ProcessDropsIteration(onlyLeaguesWithDrops, publishersInLeagues, _clock, supportedYears);

            return results;
        }

        public async Task ProcessDrops(int year)
        {
            var results = await GetDropProcessingDryRun(year);
            await _fantasyCriticRepo.SaveProcessedDropResults(results);
        }

        public Task ChangePublisherName(Publisher publisher, string publisherName)
        {
            return _fantasyCriticRepo.ChangePublisherName(publisher, publisherName);
        }

        public Task ChangeLeagueOptions(League league, string leagueName, bool publicLeague, bool testLeague)
        {
            return _fantasyCriticRepo.ChangeLeagueOptions(league, leagueName, publicLeague, testLeague);
        }

        public async Task DeleteLeague(League league)
        {
            var invites = await _fantasyCriticRepo.GetOutstandingInvitees(league);
            foreach (var invite in invites)
            {
                await _fantasyCriticRepo.DeleteInvite(invite);
            }

            foreach (var year in league.Years)
            {
                var leagueYear = await _fantasyCriticRepo.GetLeagueYear(league, year);
                var publishers = await _fantasyCriticRepo.GetPublishersInLeagueForYear(leagueYear.Value);
                foreach (var publisher in publishers)
                {
                    await _fantasyCriticRepo.DeleteLeagueActions(publisher);
                    foreach (var game in publisher.PublisherGames)
                    {
                        await _fantasyCriticRepo.RemovePublisherGame(game.PublisherGameID);
                    }

                    var bids = await _fantasyCriticRepo.GetActivePickupBids(publisher);
                    foreach (var bid in bids)
                    {
                        await _fantasyCriticRepo.RemovePickupBid(bid);
                    }

                    var dropRequests = await _fantasyCriticRepo.GetActiveDropRequests(publisher);
                    foreach (var dropRequest in dropRequests)
                    {
                        await _fantasyCriticRepo.RemoveDropRequest(dropRequest);
                    }

                    await _fantasyCriticRepo.DeletePublisher(publisher);
                }

                await _fantasyCriticRepo.DeleteLeagueYear(leagueYear.Value);
            }

            var users = await _fantasyCriticRepo.GetUsersInLeague(league);
            foreach (var user in users)
            {
                await _fantasyCriticRepo.RemovePlayerFromLeague(league, user);
            }

            await _fantasyCriticRepo.DeleteLeague(league);
        }

        public Task<bool> LeagueHasBeenStarted(Guid leagueID)
        {
            return _fantasyCriticRepo.LeagueHasBeenStarted(leagueID);
        }

        public Task<IReadOnlyList<League>> GetFollowedLeagues(FantasyCriticUser currentUser)
        {
            return _fantasyCriticRepo.GetFollowedLeagues(currentUser);
        }

        public Task<IReadOnlyList<FantasyCriticUser>> GetLeagueFollowers(League league)
        {
            return _fantasyCriticRepo.GetLeagueFollowers(league);
        }

        public async Task<Result> FollowLeague(League league, FantasyCriticUser user)
        {
            if (!league.PublicLeague)
            {
                return Result.Fail("League is not public");
            }

            bool userIsInLeague = await _leagueMemberService.UserIsInLeague(league, user);
            if (userIsInLeague)
            {
                return Result.Fail("Can't follow a league you are in.");
            }

            var followedLeagues = await GetFollowedLeagues(user);
            bool userIsFollowingLeague = followedLeagues.Any(x => x.LeagueID == league.LeagueID);
            if (userIsFollowingLeague)
            {
                return Result.Fail("User is already following that league.");
            }

            await _fantasyCriticRepo.FollowLeague(league, user);
            return Result.Ok();
        }

        public async Task<Result> UnfollowLeague(League league, FantasyCriticUser user)
        {
            var followedLeagues = await GetFollowedLeagues(user);
            bool userIsFollowingLeague = followedLeagues.Any(x => x.LeagueID == league.LeagueID);
            if (!userIsFollowingLeague)
            {
                return Result.Fail("User is not following that league.");
            }

            await _fantasyCriticRepo.UnfollowLeague(league, user);
            return Result.Ok();
        }

        public async Task<IReadOnlyList<LeagueYear>> GetPublicLeagueYears(int year)
        {
            var leagueYears = await GetLeagueYears(year);
            return leagueYears.Where(x => x.League.PublicLeague).Where(x => x.Year == year).OrderByDescending(x => x.League.NumberOfFollowers).ToList();
        }

        public async Task SetEligibilityOverride(LeagueYear leagueYear, MasterGame masterGame, bool? eligible)
        {
            if (!eligible.HasValue)
            {
                await _fantasyCriticRepo.DeleteEligibilityOverride(leagueYear, masterGame);
            }
            else
            {
                await _fantasyCriticRepo.SetEligibilityOverride(leagueYear, masterGame, eligible.Value);
            }

            var allPublishers = await _publisherService.GetPublishersInLeagueForYear(leagueYear);
            var managerPublisher = allPublishers.Single(x => x.User.UserID == leagueYear.League.LeagueManager.UserID);

            string description;
            if (!eligible.HasValue)
            {
                description = $"{masterGame.GameName}'s eligibility setting was reset to normal.";
            }
            else if (eligible.Value)
            {
                description = $"{masterGame.GameName} was manually set to 'Eligible'";
            }
            else
            {
                description = $"{masterGame.GameName} was manually set to 'Ineligible'";
            }

            LeagueAction eligibilityAction = new LeagueAction(managerPublisher, _clock.GetCurrentInstant(), "Eligibility Setting Changed", description, true);
            await _fantasyCriticRepo.AddLeagueAction(eligibilityAction);
        }
    }
}
