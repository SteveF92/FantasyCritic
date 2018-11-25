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
        private readonly FantasyCriticUserManager _userManager;
        private readonly IFantasyCriticRepo _fantasyCriticRepo;
        private readonly IClock _clock;

        public FantasyCriticService(FantasyCriticUserManager userManager, IFantasyCriticRepo fantasyCriticRepo, IClock clock)
        {
            _userManager = userManager;
            _fantasyCriticRepo = fantasyCriticRepo;
            _clock = clock;
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

        public Task CreateMasterGame(MasterGame masterGame)
        {
            return _fantasyCriticRepo.CreateMasterGame(masterGame);
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
            League newLeague = new League(Guid.NewGuid(), parameters.LeagueName, parameters.Manager, years);
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

            IReadOnlyList<Publisher> publishers = await GetPublishersInLeagueForYear(league, parameters.Year);

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

            LeagueYear newLeagueYear = new LeagueYear(league, parameters.Year, options, leagueYear.Value.PlayStatus);
            await _fantasyCriticRepo.EditLeagueYear(newLeagueYear);

            return Result.Ok();
        }

        public Task AddNewLeagueYear(League league, int year, LeagueOptions options)
        {
            return _fantasyCriticRepo.AddNewLeagueYear(league, year, options);
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

        public Task<IReadOnlyList<FantasyCriticUser>> GetUsersInLeague(League league)
        {
            return _fantasyCriticRepo.GetUsersInLeague(league);
        }

        public Task<IReadOnlyList<League>> GetLeaguesForUser(FantasyCriticUser user)
        {
            return _fantasyCriticRepo.GetLeaguesForUser(user);
        }

        public Task<IReadOnlyList<League>> GetLeaguesInvitedTo(FantasyCriticUser user)
        {
            return _fantasyCriticRepo.GetLeaguesInvitedTo(user);
        }

        public async Task<Result> InviteUser(League league, FantasyCriticUser inviteUser)
        {
            bool userInLeague = await UserIsInLeague(league, inviteUser);
            if (userInLeague)
            {
                return Result.Fail("User is already in league.");
            }

            bool userInvited = await UserIsInvited(league, inviteUser);
            if (userInvited)
            {
                return Result.Fail("User is already invited to this league.");
            }

            await _fantasyCriticRepo.SaveInvite(league, inviteUser);

            return Result.Ok();
        }

        public async Task<Result> AcceptInvite(League league, FantasyCriticUser inviteUser)
        {
            bool userInLeague = await UserIsInLeague(league, inviteUser);
            if (userInLeague)
            {
                return Result.Fail("User is already in league.");
            }

            bool userInvited = await UserIsInvited(league, inviteUser);
            if (!userInvited)
            {
                return Result.Fail("User is not invited to this league.");
            }

            await _fantasyCriticRepo.AcceptInvite(league, inviteUser);

            return Result.Ok();
        }

        public async Task<Result> DeclineInvite(League league, FantasyCriticUser inviteUser)
        {
            bool userInLeague = await UserIsInLeague(league, inviteUser);
            if (userInLeague)
            {
                return Result.Fail("User is already in league.");
            }

            bool userInvited = await UserIsInvited(league, inviteUser);
            if (!userInvited)
            {
                return Result.Fail("User is not invited to this league.");
            }

            await _fantasyCriticRepo.DeclineInvite(league, inviteUser);

            return Result.Ok();
        }

        public async Task RemovePlayerFromLeague(League league, FantasyCriticUser removeUser)
        {
            foreach (var year in league.Years)
            {
                var publisher = await GetPublisher(league, year, removeUser);
                if (publisher.HasValue)
                {
                    await _fantasyCriticRepo.RemovePublisher(publisher.Value);
                }
            }

            await _fantasyCriticRepo.RemovePlayerFromLeague(league, removeUser);
        }

        public Task<IReadOnlyList<FantasyCriticUser>> GetOutstandingInvitees(League league)
        {
            return _fantasyCriticRepo.GetOutstandingInvitees(league);
        }

        public Task<IReadOnlyList<Publisher>> GetPublishersInLeagueForYear(League league, int year)
        {
            return _fantasyCriticRepo.GetPublishersInLeagueForYear(league, year);
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

        public async Task<ClaimResult> ClaimGame(ClaimGameDomainRequest request)
        {
            PublisherGame playerGame = new PublisherGame(Guid.NewGuid(), request.GameName, _clock.GetCurrentInstant(), request.CounterPick, null, null, request.MasterGame, 
                request.DraftPosition, request.OverallDraftPosition, request.Publisher.Year);

            ClaimResult claimResult = await CanClaimGame(request);

            if (!claimResult.Success)
            {
                return claimResult;
            }

            LeagueAction leagueAction = new LeagueAction(request, _clock.GetCurrentInstant());
            await _fantasyCriticRepo.AddLeagueAction(leagueAction);

            await _fantasyCriticRepo.AddPublisherGame(request.Publisher, playerGame);

            return claimResult;
        }

        public async Task<ClaimResult> AssociateGame(AssociateGameDomainRequest request)
        {
            ClaimResult claimResult = await CanAssociateGame(request);

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
            var claimResult = await CanClaimGame(claimRequest);
            if (!claimResult.Success)
            {
                return claimResult;
            }

            var nextPriority = pickupBids.Count + 1;

            PickupBid currentBid = new PickupBid(Guid.NewGuid(), publisher, masterGame, bidAmount, nextPriority, _clock.GetCurrentInstant(), null);
            await _fantasyCriticRepo.CreatePickupBid(currentBid);

            return claimResult;
        }

        public Task<IReadOnlyList<PickupBid>> GetActiveAcquistitionBids(Publisher publisher)
        {
            return _fantasyCriticRepo.GetActivePickupBids(publisher);
        }

        public Task<Maybe<PickupBid>> GetPickupBid(Guid bidID)
        {
            return _fantasyCriticRepo.GetPickupBid(bidID);
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

        public async Task UpdateFantasyPoints(int year)
        {
            Dictionary<Guid, decimal?> publisherGameScores = new Dictionary<Guid, decimal?>();

            IReadOnlyList<LeagueYear> activeLeagueYears = await GetLeagueYears(year);
            foreach (var leagueYear in activeLeagueYears)
            {
                var publishersInLeague = await GetPublishersInLeagueForYear(leagueYear.League, year);
                foreach (var publisher in publishersInLeague)
                {
                    foreach (var publisherGame in publisher.PublisherGames)
                    {
                        decimal? fantasyPoints = leagueYear.Options.ScoringSystem.GetPointsForGame(publisherGame, _clock);
                        publisherGameScores.Add(publisherGame.PublisherGameID, fantasyPoints);
                    }
                }
            }

            await _fantasyCriticRepo.UpdateFantasyPoints(publisherGameScores);
        }

        public async Task UpdateFantasyPoints(LeagueYear leagueYear)
        {
            Dictionary<Guid, decimal?> publisherGameScores = new Dictionary<Guid, decimal?>();

            var publishersInLeague = await GetPublishersInLeagueForYear(leagueYear.League, leagueYear.Year);
            foreach (var publisher in publishersInLeague)
            {
                foreach (var publisherGame in publisher.PublisherGames)
                {
                    decimal? fantasyPoints = leagueYear.Options.ScoringSystem.GetPointsForGame(publisherGame, _clock);
                    publisherGameScores.Add(publisherGame.PublisherGameID, fantasyPoints);
                }
            }

            await _fantasyCriticRepo.UpdateFantasyPoints(publisherGameScores);
        }

        public  Task<IReadOnlyList<SupportedYear>> GetSupportedYears()
        {
            return _fantasyCriticRepo.GetSupportedYears();
        }

        public Task<IReadOnlyList<MasterGame>> GetMasterGames()
        {
            return _fantasyCriticRepo.GetMasterGames();
        }

        public Task<Maybe<MasterGame>> GetMasterGame(Guid masterGameID)
        {
            return _fantasyCriticRepo.GetMasterGame(masterGameID);
        }

        public Task UpdateCriticStats(MasterGame masterGame, OpenCriticGame openCriticGame)
        {
            return _fantasyCriticRepo.UpdateCriticStats(masterGame, openCriticGame);
        }

        public Task UpdateCriticStats(MasterSubGame masterSubGame, OpenCriticGame openCriticGame)
        {
            return _fantasyCriticRepo.UpdateCriticStats(masterSubGame, openCriticGame);
        }

        public async Task<bool> UserIsInLeague(League league, FantasyCriticUser user)
        {
            var playersInLeague = await GetUsersInLeague(league);
            return playersInLeague.Any(x => x.UserID == user.UserID);
        }

        public Task<EligibilityLevel> GetEligibilityLevel(int eligibilityLevel)
        {
            return _fantasyCriticRepo.GetEligibilityLevel(eligibilityLevel);
        }

        public Task<IReadOnlyList<EligibilityLevel>> GetEligibilityLevels()
        {
            return _fantasyCriticRepo.GetEligibilityLevels();
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

        public Task ManuallyScoreGame(PublisherGame publisherGame, decimal? manualCriticScore)
        {
            return _fantasyCriticRepo.ManuallyScoreGame(publisherGame, manualCriticScore);
        }

        public async Task ProcessPickups(int year)
        {
            var leagueYears = await GetLeagueYears(year);
            foreach (var leagueYear in leagueYears)
            {
                await ProcessPickupsForLeagueYear(leagueYear);
            }
        }

        public Task<IReadOnlyList<LeagueAction>> GetLeagueActions(LeagueYear leagueYear)
        {
            return _fantasyCriticRepo.GetLeagueActions(leagueYear);
        }

        private async Task<bool> UserIsInvited(League league, FantasyCriticUser inviteUser)
        {
            var playersInvited = await GetOutstandingInvitees(league);
            return playersInvited.Any(x => x.UserID == inviteUser.UserID);
        }

        private async Task<ClaimResult> CanClaimGame(ClaimGameDomainRequest request)
        {
            List<ClaimError> claimErrors = new List<ClaimError>();

            var basicErrors = await GetBasicErrors(request.Publisher.League, request.Publisher);
            claimErrors.AddRange(basicErrors);

            var leagueYear = await _fantasyCriticRepo.GetLeagueYear(request.Publisher.League, request.Publisher.Year);
            if (leagueYear.HasNoValue)
            {
                throw new Exception("Something has gone terribly wrong with league years.");
            }

            LeagueOptions yearOptions = leagueYear.Value.Options;
            if (request.MasterGame.HasValue)
            {
                var masterGameErrors = GetMasterGameErrors(leagueYear.Value.Options, request.MasterGame.Value, leagueYear.Value.Year, request.CounterPick);
                claimErrors.AddRange(masterGameErrors);
            }

            IReadOnlyList<Publisher> allPublishers = await _fantasyCriticRepo.GetPublishersInLeagueForYear(request.Publisher.League, request.Publisher.Year);
            IReadOnlyList<Publisher> otherPublishers = allPublishers.Where(x => x.User.UserID != request.Publisher.User.UserID).ToList();

            IReadOnlyList<PublisherGame> gamesForYear = allPublishers.SelectMany(x => x.PublisherGames).ToList();
            IReadOnlyList<PublisherGame> thisPlayersGames = request.Publisher.PublisherGames;
            IReadOnlyList<PublisherGame> otherPlayersGames = otherPublishers.SelectMany(x => x.PublisherGames).ToList();

            bool gameAlreadyClaimed = gamesForYear.ContainsGame(request);

            if (!request.CounterPick)
            {
                if (gameAlreadyClaimed)
                {
                    claimErrors.Add(new ClaimError("Cannot claim a game that someone already has.", false));
                }

                int leagueDraftGames = yearOptions.StandardGames;
                int userDraftGames = thisPlayersGames.Count(x => !x.CounterPick);
                if (userDraftGames == leagueDraftGames)
                {
                    claimErrors.Add(new ClaimError("User's game spaces are filled.", false));
                }
            }

            if (request.CounterPick)
            {
                bool otherPlayerHasCounterPick = otherPlayersGames.Where(x => x.CounterPick).ContainsGame(request);
                if (otherPlayerHasCounterPick)
                {
                    claimErrors.Add(new ClaimError("Cannot counter-pick a game that someone else has already counter-picked.", false));
                }

                bool otherPlayerHasDraftGame = otherPlayersGames.Where(x => !x.CounterPick).ContainsGame(request);

                int leagueCounterPicks = yearOptions.CounterPicks;
                int userCounterPicks = thisPlayersGames.Count(x => x.CounterPick);
                if (userCounterPicks == leagueCounterPicks)
                {
                    claimErrors.Add(new ClaimError("User's counter pick spaces are filled.", false));
                }

                if (!otherPlayerHasDraftGame)
                {
                    claimErrors.Add(new ClaimError("Cannot counterPick a game that no other player is publishing.", false));
                }
            }

            var result = new ClaimResult(claimErrors);
            if (result.Overridable && request.ManagerOverride)
            {
                return new ClaimResult(new List<ClaimError>());
            }

            return result;
        }

        private async Task<ClaimResult> CanAssociateGame(AssociateGameDomainRequest request)
        {
            List<ClaimError> associationErrors = new List<ClaimError>();

            var basicErrors = await GetBasicErrors(request.Publisher.League, request.Publisher);
            associationErrors.AddRange(basicErrors);

            var leagueYear = await _fantasyCriticRepo.GetLeagueYear(request.Publisher.League, request.Publisher.Year);
            IReadOnlyList<ClaimError> masterGameErrors = GetMasterGameErrors(leagueYear.Value.Options, request.MasterGame, leagueYear.Value.Year, request.PublisherGame.CounterPick);
            associationErrors.AddRange(masterGameErrors);

            IReadOnlyList<Publisher> allPublishers = await _fantasyCriticRepo.GetPublishersInLeagueForYear(request.Publisher.League, request.Publisher.Year);
            IReadOnlyList<Publisher> publishersForYear = allPublishers.Where(x => x.Year == leagueYear.Value.Year).ToList();
            IReadOnlyList<Publisher> otherPublishers = publishersForYear.Where(x => x.User.UserID != request.Publisher.User.UserID).ToList();

            IReadOnlyList<PublisherGame> gamesForYear = publishersForYear.SelectMany(x => x.PublisherGames).ToList();
            IReadOnlyList<PublisherGame> otherPlayersGames = otherPublishers.SelectMany(x => x.PublisherGames).ToList();

            bool gameAlreadyClaimed = gamesForYear.ContainsGame(request.MasterGame);

            if (!request.PublisherGame.CounterPick)
            {
                if (gameAlreadyClaimed)
                {
                    associationErrors.Add(new ClaimError("Cannot draft a game that someone already has.", false));
                }
            }

            if (request.PublisherGame.CounterPick)
            {
                bool otherPlayerHasDraftGame = otherPlayersGames.Where(x => !x.CounterPick).ContainsGame(request.MasterGame);
                if (!otherPlayerHasDraftGame)
                {
                    associationErrors.Add(new ClaimError("Cannot counterPick a game that no other player is publishing.", false));
                }
            }

            var result = new ClaimResult(associationErrors);
            if (result.Overridable && request.ManagerOverride)
            {
                return new ClaimResult(new List<ClaimError>());
            }

            return result;
        }

        private async Task<IReadOnlyList<ClaimError>> GetBasicErrors(League league, Publisher publisher)
        {
            List<ClaimError> claimErrors = new List<ClaimError>();

            bool isInLeague = await UserIsInLeague(league, publisher.User);
            if (!isInLeague)
            {
                claimErrors.Add(new ClaimError("User is not in that league.", false));
            }

            if (!league.Years.Contains(publisher.Year))
            {
                claimErrors.Add(new ClaimError("League is not active for that year.", false));
            }

            var openYears = (await GetSupportedYears()).Where(x => x.OpenForPlay).Select(x => x.Year);
            if (!openYears.Contains(publisher.Year))
            {
                claimErrors.Add(new ClaimError("That year is not open for play", false));
            }

            return claimErrors;
        }

        private IReadOnlyList<ClaimError> GetMasterGameErrors(LeagueOptions yearOptions, MasterGame masterGame, int year, bool counterPick)
        {
            List<ClaimError> claimErrors = new List<ClaimError>();

            bool eligible = masterGame.IsEligible(yearOptions.MaximumEligibilityLevel);
            if (!eligible)
            {
                claimErrors.Add(new ClaimError("That game is not eligible under this league's settings.", true));
            }

            bool earlyAccessEligible = (!masterGame.EarlyAccess || yearOptions.AllowEarlyAccess);
            if (!earlyAccessEligible)
            {
                claimErrors.Add(new ClaimError("That game is not eligible under this league's early access settings.", true));
            }

            bool yearlyInstallmentEligible = (!masterGame.YearlyInstallment || yearOptions.AllowYearlyInstallments);
            if (!yearlyInstallmentEligible)
            {
                claimErrors.Add(new ClaimError("That game is not eligible under this league's yearly installment settings.", true));
            }

            bool released = masterGame.IsReleased(_clock);
            if (released)
            {
                claimErrors.Add(new ClaimError("That game has already been released.", true));
            }

            if (masterGame.ReleaseDate.HasValue)
            {
                if (released && masterGame.ReleaseDate.Value.Year < year)
                {
                    claimErrors.Add(new ClaimError($"That game was released prior to the start of {year}.", false));
                }
                else if (!released && masterGame.ReleaseDate.Value.Year > year && !counterPick)
                {
                    claimErrors.Add(new ClaimError($"That game is not scheduled to be released in {year}.", true));
                }
            }

            bool hasScore = masterGame.CriticScore.HasValue;
            if (hasScore)
            {
                claimErrors.Add(new ClaimError("That game already has a score.", true));
            }

            return claimErrors;
        }

        private async Task ProcessPickupsForLeagueYear(LeagueYear leagueYear)
        {
            var allActiveBids = await GetActiveBids(leagueYear);
            if (!allActiveBids.Any())
            {
                return;
            }

            var insufficientFundsBids = allActiveBids.Where(x => x.BidAmount > x.Publisher.Budget);
            var winnableBids = GetWinnableBids(allActiveBids, leagueYear.Options);
            var winningBids = GetWinningBids(winnableBids);

            var takenGames = winningBids.Select(x => x.MasterGame);
            var losingBids = allActiveBids
                .Except(winningBids)
                .Except(insufficientFundsBids)
                .Where(x => takenGames.Contains(x.MasterGame))
                .Select(x => new FailedPickupBid(x, "Publisher was outbid."));

            var insufficientFundsBidFailures = insufficientFundsBids.Select(x => new FailedPickupBid(x, "Not enough budget."));
            var failedBids = losingBids.Concat(insufficientFundsBidFailures);
            await ProcessSuccessfulAndFailedBids(winningBids, failedBids);

            //When we are done, we run again.
            //This will repeat until there are no active bids.
            await Task.Delay(5);
            await ProcessPickupsForLeagueYear(leagueYear);
        }

        private async Task<IReadOnlyList<PickupBid>> GetActiveBids(LeagueYear leagueYear)
        {
            List<PickupBid> activeBids = new List<PickupBid>();
            var publishers = await GetPublishersInLeagueForYear(leagueYear.League, leagueYear.Year);
            foreach (var publisher in publishers)
            {
                var bidsForPublisher = await _fantasyCriticRepo.GetActivePickupBids(publisher);
                activeBids.AddRange(bidsForPublisher);
            }

            return activeBids;
        }

        private IReadOnlyList<PickupBid> GetWinnableBids(IEnumerable<PickupBid> activeBidsForLeagueYear, LeagueOptions options)
        {
            List<PickupBid> winnableBids = new List<PickupBid>();

            var enoughBudgetBids = activeBidsForLeagueYear.Where(x => x.BidAmount <= x.Publisher.Budget);
            var groupedByGame = enoughBudgetBids.GroupBy(x => x.MasterGame);
            foreach (var gameGroup in groupedByGame)
            {
                PickupBid bestBid;
                if (gameGroup.Count() == 1)
                {
                    bestBid = gameGroup.First();
                }
                else
                {
                    var bestBids = gameGroup.MaxBy(x => x.BidAmount);
                    var bestBidsByProjectedScore = bestBids.MinBy(x => x.Publisher.GetProjectedFantasyPoints(options.ScoringSystem, options.EstimatedCriticScore));
                    bestBid = bestBidsByProjectedScore.First();
                }

                winnableBids.Add(bestBid);
            }

            return winnableBids;
        }

        private IReadOnlyList<PickupBid> GetWinningBids(IEnumerable<PickupBid> winnableBids)
        {
            List<PickupBid> winningBids = new List<PickupBid>();
            var groupedByPublisher = winnableBids.GroupBy(x => x.Publisher);
            foreach (var publisherGroup in groupedByPublisher)
            {
                PickupBid winningBid = publisherGroup.MinBy(x => x.Priority).First();
                winningBids.Add(winningBid);
            }

            return winningBids;
        }

        private async Task ProcessSuccessfulAndFailedBids(IEnumerable<PickupBid> successBids, IEnumerable<FailedPickupBid> failedBids)
        {
            foreach (var successBid in successBids)
            {
                await _fantasyCriticRepo.MarkBidStatus(successBid, true);
                PublisherGame newPublisherGame = new PublisherGame(Guid.NewGuid(), successBid.MasterGame.GameName, _clock.GetCurrentInstant(), false, null, null, successBid.MasterGame, null, null, successBid.Publisher.Year);
                await _fantasyCriticRepo.AddPublisherGame(successBid.Publisher, newPublisherGame);
                await _fantasyCriticRepo.SpendBudget(successBid.Publisher, successBid.BidAmount);

                LeagueAction leagueAction = new LeagueAction(successBid, _clock.GetCurrentInstant());
                await _fantasyCriticRepo.AddLeagueAction(leagueAction);
            }

            foreach (var failedBid in failedBids)
            {
                await _fantasyCriticRepo.MarkBidStatus(failedBid.PickupBid, false);

                LeagueAction leagueAction = new LeagueAction(failedBid, _clock.GetCurrentInstant());
                await _fantasyCriticRepo.AddLeagueAction(leagueAction);
            }
        }

        public Task ChangePublisherName(Publisher publisher, string publisherName)
        {
            return _fantasyCriticRepo.ChangePublisherName(publisher, publisherName);
        }

        public Task ChangeLeagueName(League league, string leagueName)
        {
            return _fantasyCriticRepo.ChangeLeagueName(league, leagueName);
        }

        public async Task<StartDraftResult> GetStartDraftResult(LeagueYear leagueYear, IReadOnlyList<Publisher> publishersInLeague, IReadOnlyList<FantasyCriticUser> usersInLeague)
        {
            if (leagueYear.PlayStatus.PlayStarted)
            {
                return new StartDraftResult(true, new List<string>());
            }

            var supportedYears = await _fantasyCriticRepo.GetSupportedYears();
            var supportedYear = supportedYears.Single(x => x.Year == leagueYear.Year);

            List<string> errors = new List<string>();

            if (usersInLeague.Count() < 2)
            {
                errors.Add("You need to have at least two players in the league.");
            }

            if (publishersInLeague.Count() != usersInLeague.Count())
            {
                errors.Add("Not every player has created a publisher.");
            }

            if (!supportedYear.OpenForPlay)
            {
                errors.Add($"This year is not yet open for play. It will become available on {supportedYear.StartDate}.");
            }

            return new StartDraftResult(!errors.Any(), errors);
        }


        public bool LeagueIsReadyToSetDraftOrder(IEnumerable<Publisher> publishersInLeague, IEnumerable<FantasyCriticUser> usersInLeague)
        {
            if (publishersInLeague.Count() != usersInLeague.Count())
            {
                return false;
            }

            if (publishersInLeague.Count() < 2)
            {
                return false;
            }

            return true;
        }

        public bool LeagueIsReadyToPlay(SupportedYear supportedYear, IEnumerable<Publisher> publishersInLeague, IEnumerable<FantasyCriticUser> usersInLeague)
        {
            if (!LeagueIsReadyToSetDraftOrder(publishersInLeague, usersInLeague))
            {
                return false;
            }

            if (!supportedYear.OpenForPlay)
            {
                return false;
            }

            return true;
        }

        public Task StartDraft(LeagueYear leagueYear)
        {
            return _fantasyCriticRepo.StartDraft(leagueYear);
        }

        public Task SetDraftPause(LeagueYear leagueYear, bool pause)
        {
            return _fantasyCriticRepo.SetDraftPause(leagueYear, pause);
        }

        public async Task UndoLastDraftAction(LeagueYear leagueYear)
        {
            IReadOnlyList<Publisher> publishers = await GetPublishersInLeagueForYear(leagueYear.League, leagueYear.Year);
            var publisherGames = publishers.SelectMany(x => x.PublisherGames);
            var newestGame = publisherGames.MaxBy(x => x.Timestamp).Single();

            var publisher = publishers.Single(x => x.PublisherGames.Select(y => y.PublisherGameID).Contains(newestGame.PublisherGameID));

            await RemovePublisherGame(leagueYear, publisher, newestGame);
        }

        public async Task<Result> SetDraftOrder(LeagueYear leagueYear, IEnumerable<KeyValuePair<Publisher, int>> draftPositions)
        {
            var publishersInLeague = await GetPublishersInLeagueForYear(leagueYear.League, leagueYear.Year);
            int publishersCount = publishersInLeague.Count;
            if (publishersCount != draftPositions.Count())
            {
                return Result.Fail("Not setting all publishers.");
            }

            var requiredNumbers = Enumerable.Range(1, publishersCount).ToList();
            var requestedDraftNumbers = draftPositions.Select(x => x.Value);
            bool allRequiredPresent = new HashSet<int>(requiredNumbers).SetEquals(requestedDraftNumbers);
            if (!allRequiredPresent)
            {
                return Result.Fail("Some of the positions are not valid.");
            }

            await _fantasyCriticRepo.SetDraftOrder(draftPositions);
            return Result.Ok();
        }

        public async Task<Maybe<Publisher>> GetNextDraftPublisher(LeagueYear leagueYear)
        {
            if (!leagueYear.PlayStatus.DraftIsActive)
            {
                return Maybe<Publisher>.None;
            }

            IReadOnlyList<Publisher> publishers = await GetPublishersInLeagueForYear(leagueYear.League, leagueYear.Year);
            DraftPhase phase = GetDraftPhase(leagueYear, publishers);
            if (phase.Equals(DraftPhase.StandardGames))
            {
                var publishersWithLowestNumberOfGames = publishers.MinBy(x => x.PublisherGames.Count(y => !y.CounterPick));
                var allPlayersHaveSameNumberOfGames = publishers.Select(x => x.PublisherGames.Count(y => !y.CounterPick)).Distinct().Count() == 1;
                var maxNumberOfGames = publishers.Max(x => x.PublisherGames.Count(y => !y.CounterPick));
                var roundNumber = maxNumberOfGames;
                if (allPlayersHaveSameNumberOfGames)
                {
                    roundNumber++;
                }

                bool roundNumberIsOdd = (roundNumber % 2 != 0);
                if (roundNumberIsOdd)
                {
                    var sortedPublishersOdd = publishersWithLowestNumberOfGames.OrderBy(x => x.DraftPosition);
                    var firstPublisherOdd = sortedPublishersOdd.First();
                    return firstPublisherOdd;
                }
                //Else round is even
                var sortedPublishersEven = publishersWithLowestNumberOfGames.OrderByDescending(x => x.DraftPosition);
                var firstPublisherEven = sortedPublishersEven.First();
                return firstPublisherEven;
            }
            if (phase.Equals(DraftPhase.CounterPicks))
            {
                var publishersWithLowestNumberOfGames = publishers.MinBy(x => x.PublisherGames.Count(y => y.CounterPick));
                var allPlayersHaveSameNumberOfGames = publishers.Select(x => x.PublisherGames.Count(y => y.CounterPick)).Distinct().Count() == 1;
                var maxNumberOfGames = publishers.Max(x => x.PublisherGames.Count(y => y.CounterPick));

                var roundNumber = maxNumberOfGames;
                if (allPlayersHaveSameNumberOfGames)
                {
                    roundNumber++;
                }

                bool roundNumberIsOdd = (roundNumber % 2 != 0);
                if (roundNumberIsOdd)
                {
                    var sortedPublishersOdd = publishersWithLowestNumberOfGames.OrderByDescending(x => x.DraftPosition);
                    var firstPublisherOdd = sortedPublishersOdd.First();
                    return firstPublisherOdd;
                }
                //Else round is even
                var sortedPublishersEven = publishersWithLowestNumberOfGames.OrderBy(x => x.DraftPosition);
                var firstPublisherEven = sortedPublishersEven.First();
                return firstPublisherEven;
            }

            return Maybe<Publisher>.None;
        }

        public async Task<DraftPhase> GetDraftPhase(LeagueYear leagueYear)
        {
            IReadOnlyList<Publisher> publishers = await GetPublishersInLeagueForYear(leagueYear.League, leagueYear.Year);
            return GetDraftPhase(leagueYear, publishers);
        }

        private DraftPhase GetDraftPhase(LeagueYear leagueYear, IReadOnlyList<Publisher> publishers)
        {
            int numberOfStandardGamesToDraft = leagueYear.Options.GamesToDraft * publishers.Count;
            int standardGamesDrafted = publishers.SelectMany(x => x.PublisherGames).Count(x => !x.CounterPick);
            if (standardGamesDrafted < numberOfStandardGamesToDraft)
            {
                return DraftPhase.StandardGames;
            }

            int numberOfCounterPicksToDraft = leagueYear.Options.CounterPicks * publishers.Count;
            int counterPicksDrafted = publishers.SelectMany(x => x.PublisherGames).Count(x => x.CounterPick);
            if (counterPicksDrafted < numberOfCounterPicksToDraft)
            {
                return DraftPhase.CounterPicks;
            }

            return DraftPhase.Complete;
        }

        public async Task<IReadOnlyList<PublisherGame>> GetAvailableCounterPicks(LeagueYear leagueYear, Publisher nextDraftingPublisher)
        {
            IReadOnlyList<Publisher> allPublishers = await _fantasyCriticRepo.GetPublishersInLeagueForYear(leagueYear.League, leagueYear.Year);
            IReadOnlyList<Publisher> otherPublishers = allPublishers.Where(x => x.PublisherID != nextDraftingPublisher.PublisherID).ToList();

            IReadOnlyList<PublisherGame> gamesForYear = allPublishers.SelectMany(x => x.PublisherGames).ToList();
            IReadOnlyList<PublisherGame> otherPlayersGames = otherPublishers.SelectMany(x => x.PublisherGames).ToList();

            var alreadyCounterPicked = gamesForYear.Where(x => x.CounterPick).ToList();
            List<PublisherGame> availableCounterPicks = new List<PublisherGame>();
            foreach (var otherPlayerGame in otherPlayersGames)
            {
                bool playerHasCounterPick = alreadyCounterPicked.ContainsGame(otherPlayerGame);
                if (playerHasCounterPick)
                {
                    continue;
                }

                availableCounterPicks.Add(otherPlayerGame);
            }

            return availableCounterPicks;
        }

        public async Task<bool> CompleteDraft(LeagueYear leagueYear)
        {
            IReadOnlyList<Publisher> publishers = await GetPublishersInLeagueForYear(leagueYear.League, leagueYear.Year);

            int numberOfStandardGamesToDraft = leagueYear.Options.GamesToDraft * publishers.Count;
            int standardGamesDrafted = publishers.SelectMany(x => x.PublisherGames).Count(x => !x.CounterPick);
            if (standardGamesDrafted < numberOfStandardGamesToDraft)
            {
                return false;
            }

            int numberOfCounterPicksToDraft = leagueYear.Options.CounterPicks * publishers.Count;
            int counterPicksDrafted = publishers.SelectMany(x => x.PublisherGames).Count(x => x.CounterPick);
            if (counterPicksDrafted < numberOfCounterPicksToDraft)
            {
                return false;
            }

            await _fantasyCriticRepo.CompleteDraft(leagueYear);
            return true;
        }
    }
}
