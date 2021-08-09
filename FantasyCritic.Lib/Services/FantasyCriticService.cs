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
                return Result.Failure<League>(validateOptions.Error);
            }

            IEnumerable<int> years = new List<int>() { parameters.InitialYear };
            League newLeague = new League(Guid.NewGuid(), parameters.LeagueName, parameters.Manager, years, parameters.PublicLeague, parameters.TestLeague, false, 0);
            await _fantasyCriticRepo.CreateLeague(newLeague, parameters.InitialYear, options);
            return Result.Success(newLeague);
        }

        public async Task<Result> EditLeague(League league, EditLeagueYearParameters parameters)
        {
            LeagueOptions options = new LeagueOptions(parameters);
            var validateOptions = options.Validate();
            if (validateOptions.IsFailure)
            {
                return Result.Failure(validateOptions.Error);
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
                return Result.Failure($"Cannot reduce number of standard games to {options.StandardGames} as a publisher has {maxStandardGames} draft games currently.");
            }
            if (maxCounterPicks > options.CounterPicks)
            {
                return Result.Failure($"Cannot reduce number of counter picks to {options.CounterPicks} as a publisher has {maxCounterPicks} counter picks currently.");
            }

            int maxFreeGamesFreeDropped = publishers.Select(publisher => publisher.FreeGamesDropped).DefaultIfEmpty(0).Max();
            int maxWillNotReleaseGamesDropped = publishers.Select(publisher => publisher.WillNotReleaseGamesDropped).DefaultIfEmpty(0).Max();
            int maxWillReleaseGamesDropped = publishers.Select(publisher => publisher.WillReleaseGamesDropped).DefaultIfEmpty(0).Max();

            if (maxFreeGamesFreeDropped > options.FreeDroppableGames && options.FreeDroppableGames != -1)
            {
                return Result.Failure($"Cannot reduce number of unrestricted droppable games to {options.FreeDroppableGames} as a publisher has already dropped {maxFreeGamesFreeDropped} games.");
            }
            if (maxWillNotReleaseGamesDropped > options.WillNotReleaseDroppableGames && options.WillNotReleaseDroppableGames != -1)
            {
                return Result.Failure($"Cannot reduce number of 'will not release' droppable games to {options.WillNotReleaseDroppableGames} as a publisher has already dropped {maxWillNotReleaseGamesDropped} games.");
            }
            if (maxWillReleaseGamesDropped > options.WillReleaseDroppableGames && options.WillReleaseDroppableGames != -1)
            {
                return Result.Failure($"Cannot reduce number of 'will release' droppable games to {options.WillReleaseDroppableGames} as a publisher has already dropped {maxWillReleaseGamesDropped} games.");
            }

            var eligibilityOverrides = await GetEligibilityOverrides(league, parameters.Year);

            LeagueYear newLeagueYear = new LeagueYear(league, parameters.Year, options, leagueYear.Value.PlayStatus, eligibilityOverrides, leagueYear.Value.DraftStartedTimestamp);
            await _fantasyCriticRepo.EditLeagueYear(newLeagueYear);

            return Result.Success();
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

        public async Task UpdateLeaguePointsAndStatuses(int year)
        {
            Dictionary<Guid, PublisherGameCalculatedStats> calculatedStats = new Dictionary<Guid, PublisherGameCalculatedStats>();

            IReadOnlyList<LeagueYear> activeLeagueYears = await GetLeagueYears(year);
            Dictionary<LeagueYearKey, LeagueYear> leagueYearDictionary = activeLeagueYears.ToDictionary(x => x.Key, y => y);
            IReadOnlyList<Publisher> allPublishersForYear = await _fantasyCriticRepo.GetAllPublishersForYear(year);

            var currentDate = _clock.GetToday();
            foreach (var publisher in allPublishersForYear)
            {
                var key = new LeagueYearKey(publisher.LeagueYear.League.LeagueID, publisher.LeagueYear.Year);
                foreach (var publisherGame in publisher.PublisherGames)
                {
                    var leagueYear = leagueYearDictionary[key];
                    decimal? fantasyPoints = publisherGame.CalculateFantasyPoints(leagueYear.Options.ScoringSystem, currentDate);
                    bool? overridenEligibility = leagueYear.GetOverriddenEligibility(publisherGame.MasterGame);
                    bool currentlyIneligible = publisherGame.CalculateIsCurrentlyIneligible(leagueYear.Options, overridenEligibility);
                    var stats = new PublisherGameCalculatedStats(fantasyPoints, currentlyIneligible);
                    calculatedStats.Add(publisherGame.PublisherGameID, stats);
                }
            }

            await _fantasyCriticRepo.UpdatePublisherGameCalculatedStats(calculatedStats);
        }

        public async Task UpdatePublisherGameCalculatedStats(LeagueYear leagueYear)
        {
            Dictionary<Guid, PublisherGameCalculatedStats> calculatedStats = new Dictionary<Guid, PublisherGameCalculatedStats>();

            var currentDate = _clock.GetToday();
            var publishersInLeague = await _publisherService.GetPublishersInLeagueForYear(leagueYear);
            foreach (var publisher in publishersInLeague)
            {
                foreach (var publisherGame in publisher.PublisherGames)
                {
                    decimal? fantasyPoints = publisherGame.CalculateFantasyPoints(leagueYear.Options.ScoringSystem, currentDate);
                    bool? overridenEligibility = leagueYear.GetOverriddenEligibility(publisherGame.MasterGame);
                    bool currentlyIneligible = publisherGame.CalculateIsCurrentlyIneligible(leagueYear.Options, overridenEligibility);
                    var stats = new PublisherGameCalculatedStats(fantasyPoints, currentlyIneligible);
                    calculatedStats.Add(publisherGame.PublisherGameID, stats);
                }
            }

            await _fantasyCriticRepo.UpdatePublisherGameCalculatedStats(calculatedStats);
        }

        public Task ManuallyScoreGame(PublisherGame publisherGame, decimal? manualCriticScore)
        {
            return _fantasyCriticRepo.ManuallyScoreGame(publisherGame, manualCriticScore);
        }

        public Task ManuallySetWillNotRelease(PublisherGame publisherGame, bool willNotRelease)
        {
            return _fantasyCriticRepo.ManuallySetWillNotRelease(publisherGame, willNotRelease);
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
                return Result.Failure("League is not public");
            }

            bool userIsInLeague = await _leagueMemberService.UserIsInLeague(league, user);
            if (userIsInLeague)
            {
                return Result.Failure("Can't follow a league you are in.");
            }

            var followedLeagues = await GetFollowedLeagues(user);
            bool userIsFollowingLeague = followedLeagues.Any(x => x.LeagueID == league.LeagueID);
            if (userIsFollowingLeague)
            {
                return Result.Failure("User is already following that league.");
            }

            await _fantasyCriticRepo.FollowLeague(league, user);
            return Result.Success();
        }

        public async Task<Result> UnfollowLeague(League league, FantasyCriticUser user)
        {
            var followedLeagues = await GetFollowedLeagues(user);
            bool userIsFollowingLeague = followedLeagues.Any(x => x.LeagueID == league.LeagueID);
            if (!userIsFollowingLeague)
            {
                return Result.Failure("User is not following that league.");
            }

            await _fantasyCriticRepo.UnfollowLeague(league, user);
            return Result.Success();
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

        public async Task PostNewManagerMessage(LeagueYear leagueYear, string message, bool isPublic)
        {
            var domainMessage = new ManagerMessage(Guid.NewGuid(), message, isPublic, _clock.GetCurrentInstant());
            await _fantasyCriticRepo.PostNewManagerMessage(leagueYear, domainMessage);
        }

        public Task<IReadOnlyList<ManagerMessage>> GetManagerMessages(LeagueYear leagueYear)
        {
            return _fantasyCriticRepo.GetManagerMessages(leagueYear);
        }

        public Task DeleteManagerMessage(Guid messageID)
        {
            return _fantasyCriticRepo.DeleteManagerMessage(messageID);
        }
    }
}
