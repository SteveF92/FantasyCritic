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
        private readonly IMasterGameRepo _masterGameRepo;
        private readonly IClock _clock;

        public FantasyCriticService(FantasyCriticUserManager userManager, IFantasyCriticRepo fantasyCriticRepo, IMasterGameRepo masterGameRepo, IClock clock)
        {
            _userManager = userManager;
            _fantasyCriticRepo = fantasyCriticRepo;
            _masterGameRepo = masterGameRepo;
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
            return _masterGameRepo.CreateMasterGame(masterGame);
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

        public async Task<Result> InviteUser(League league, string inviteEmail)
        {
            bool userInvited = await UserIsInvited(league, inviteEmail);
            if (userInvited)
            {
                return Result.Fail("User is already invited to this league.");
            }

            FantasyCriticUser inviteUser = await _userManager.FindByEmailAsync(inviteEmail);
            if (inviteUser != null)
            {
                bool userInLeague = await UserIsInLeague(league, inviteUser);
                if (userInLeague)
                {
                    return Result.Fail("User is already in league.");
                }
            }
            
            IReadOnlyList<FantasyCriticUser> players = await GetUsersInLeague(league);
            IReadOnlyList<string> outstandingInvites = await GetOutstandingInvitees(league);
            int totalPlayers = players.Count + outstandingInvites.Count;

            if (totalPlayers >= 14)
            {
                return Result.Fail("A league cannot have more than 14 players.");
            }

            await _fantasyCriticRepo.SaveInvite(league, inviteEmail);

            return Result.Ok();
        }

        public async Task<Result> RescindInvite(League league, string inviteEmail)
        {
            bool userInvited = await UserIsInvited(league, inviteEmail);
            if (!userInvited)
            {
                return Result.Fail("That email address has not been invited.");
            }

            await _fantasyCriticRepo.RescindInvite(league, inviteEmail);

            return Result.Ok();
        }

        public async Task<Result> AcceptInvite(League league, FantasyCriticUser inviteUser)
        {
            bool userInLeague = await UserIsInLeague(league, inviteUser);
            if (userInLeague)
            {
                return Result.Fail("User is already in league.");
            }

            bool userInvited = await UserIsInvited(league, inviteUser.EmailAddress);
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

            bool userInvited = await UserIsInvited(league, inviteUser.EmailAddress);
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

        public Task<IReadOnlyList<string>> GetOutstandingInvitees(League league)
        {
            return _fantasyCriticRepo.GetOutstandingInvitees(league);
        }

        public Task<IReadOnlyList<Publisher>> GetPublishersInLeagueForYear(League league, int year)
        {
            return _fantasyCriticRepo.GetPublishersInLeagueForYear(league, year);
        }

        private Task<IReadOnlyList<Publisher>> GetAllPublishersForYear(int year)
        {
            return _fantasyCriticRepo.GetAllPublishersForYear(year);
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

        public async Task<ClaimResult> ClaimGame(ClaimGameDomainRequest request, bool managerAction, bool draft)
        {
            Maybe<MasterGameYear> masterGameYear = Maybe<MasterGameYear>.None;
            if (request.MasterGame.HasValue)
            {
                masterGameYear = new MasterGameYear(request.MasterGame.Value, request.Publisher.Year);
            }

            PublisherGame playerGame = new PublisherGame(request.Publisher.PublisherID, Guid.NewGuid(), request.GameName, _clock.GetCurrentInstant(), request.CounterPick, null, null, 
                masterGameYear, request.DraftPosition, request.OverallDraftPosition, request.Publisher.Year);

            ClaimResult claimResult = await CanClaimGame(request);

            if (!claimResult.Success)
            {
                return claimResult;
            }

            LeagueAction leagueAction = new LeagueAction(request, _clock.GetCurrentInstant(), managerAction, draft);
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
            SystemWideValues systemWideValues = await GetLeagueWideValues();
            Dictionary<Guid, decimal?> publisherGameScores = new Dictionary<Guid, decimal?>();

            IReadOnlyList<LeagueYear> activeLeagueYears = await GetLeagueYears(year);
            Dictionary<LeagueYearKey, LeagueYear> leagueYearDictionary = activeLeagueYears.ToDictionary(x => x.Key, y => y);
            IReadOnlyList<Publisher> allPublishersForYear = await GetAllPublishersForYear(year);

            foreach (var publisher in allPublishersForYear)
            {
                var key = new LeagueYearKey(publisher.League.LeagueID, publisher.Year);
                foreach (var publisherGame in publisher.PublisherGames)
                {
                    var leagueYear = leagueYearDictionary[key];
                    decimal? fantasyPoints = leagueYear.Options.ScoringSystem.GetPointsForGame(publisherGame, _clock, systemWideValues);
                    publisherGameScores.Add(publisherGame.PublisherGameID, fantasyPoints);
                }
            }

            await _fantasyCriticRepo.UpdateFantasyPoints(publisherGameScores);
        }

        public async Task UpdateFantasyPoints(LeagueYear leagueYear)
        {
            Dictionary<Guid, decimal?> publisherGameScores = new Dictionary<Guid, decimal?>();
            SystemWideValues systemWideValues = await GetLeagueWideValues();

            var publishersInLeague = await GetPublishersInLeagueForYear(leagueYear.League, leagueYear.Year);
            foreach (var publisher in publishersInLeague)
            {
                foreach (var publisherGame in publisher.PublisherGames)
                {
                    decimal? fantasyPoints = leagueYear.Options.ScoringSystem.GetPointsForGame(publisherGame, _clock, systemWideValues);
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
            return _masterGameRepo.GetMasterGames();
        }

        public Task<IReadOnlyList<MasterGameYear>> GetMasterGameYears(int year)
        {
            return _masterGameRepo.GetMasterGameYears(year);
        }

        public Task<Maybe<MasterGame>> GetMasterGame(Guid masterGameID)
        {
            return _masterGameRepo.GetMasterGame(masterGameID);
        }

        public Task<Maybe<MasterGameYear>> GetMasterGameYear(Guid masterGameID, int year)
        {
            return _masterGameRepo.GetMasterGameYear(masterGameID, year);
        }

        public Task<IReadOnlyList<Guid>> GetAllSelectedMasterGameIDsForYear(int year)
        {
            return _masterGameRepo.GetAllSelectedMasterGameIDsForYear(year);
        }

        public Task UpdateCriticStats(MasterGame masterGame, OpenCriticGame openCriticGame)
        {
            return _masterGameRepo.UpdateCriticStats(masterGame, openCriticGame);
        }

        public Task UpdateCriticStats(MasterSubGame masterSubGame, OpenCriticGame openCriticGame)
        {
            return _masterGameRepo.UpdateCriticStats(masterSubGame, openCriticGame);
        }

        public async Task<bool> UserIsInLeague(League league, FantasyCriticUser user)
        {
            var playersInLeague = await GetUsersInLeague(league);
            return playersInLeague.Any(x => x.UserID == user.UserID);
        }

        public Task<EligibilityLevel> GetEligibilityLevel(int eligibilityLevel)
        {
            return _masterGameRepo.GetEligibilityLevel(eligibilityLevel);
        }

        public Task<IReadOnlyList<EligibilityLevel>> GetEligibilityLevels()
        {
            return _masterGameRepo.GetEligibilityLevels();
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

        public Task<IReadOnlyList<LeagueAction>> GetLeagueActions(LeagueYear leagueYear)
        {
            return _fantasyCriticRepo.GetLeagueActions(leagueYear);
        }

        private async Task<bool> UserIsInvited(League league, string inviteEmail)
        {
            var playersInvited = await GetOutstandingInvitees(league);
            return playersInvited.Any(x => string.Equals(x, inviteEmail, StringComparison.OrdinalIgnoreCase));
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
            if (request.MasterGame.HasValue && !request.CounterPick)
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
                    associationErrors.Add(new ClaimError("Cannot select a game that someone already has.", false));
                }
            }

            if (request.PublisherGame.CounterPick)
            {
                bool otherPlayerHasDraftGame = otherPlayersGames.Where(x => !x.CounterPick).ContainsGame(request.MasterGame);
                if (!otherPlayerHasDraftGame)
                {
                    associationErrors.Add(new ClaimError("Cannot counter pick a game that no other player is publishing.", false));
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

        public async Task ProcessPickups(int year)
        {
            SystemWideValues systemWideValues = await GetLeagueWideValues();
            IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> allActiveBids = await _fantasyCriticRepo.GetActivePickupBids(year);
            if (!allActiveBids.Any())
            {
                return;
            }

            IEnumerable<PickupBid> flatAllBids = allActiveBids.SelectMany(x => x.Value);

            var processedBids = new ProcessedBidSet();
            foreach (var leagueYear in allActiveBids)
            {
                if (!leagueYear.Value.Any())
                {
                    continue;
                }
                var processedBidsForLeagueYear = ProcessPickupsForLeagueYear(leagueYear.Key, leagueYear.Value, systemWideValues);
                processedBids = processedBids.AppendSet(processedBidsForLeagueYear);
            }

            await ProcessSuccessfulAndFailedBids(processedBids.SuccessBids, processedBids.FailedBids);

            var remainingBids = flatAllBids.Except(processedBids.ProcessedBids);
            if (remainingBids.Any())
            {
                await Task.Delay(1);
                await ProcessPickups(year);
            }
        }

        private ProcessedBidSet ProcessPickupsForLeagueYear(LeagueYear leagueYear, IEnumerable<PickupBid> activeBidsForLeague, SystemWideValues systemWideValues)
        {
            var noSpaceLeftBids = activeBidsForLeague.Where(x => !x.Publisher.HasRemainingGameSpot(leagueYear.Options.StandardGames));
            var insufficientFundsBids = activeBidsForLeague.Where(x => x.BidAmount > x.Publisher.Budget);

            var validBids = activeBidsForLeague.Except(noSpaceLeftBids).Except(insufficientFundsBids);
            var winnableBids = GetWinnableBids(validBids, leagueYear.Options, systemWideValues);
            var winningBids = GetWinningBids(winnableBids);

            var takenGames = winningBids.Select(x => x.MasterGame);
            var losingBids = activeBidsForLeague
                .Except(winningBids)
                .Except(noSpaceLeftBids)
                .Except(insufficientFundsBids)
                .Where(x => takenGames.Contains(x.MasterGame))
                .Select(x => new FailedPickupBid(x, "Publisher was outbid."));

            var insufficientFundsBidFailures = insufficientFundsBids.Select(x => new FailedPickupBid(x, "Not enough budget."));
            var noSpaceLeftBidFailures = noSpaceLeftBids.Select(x => new FailedPickupBid(x, "No roster spots available."));
            var failedBids = losingBids.Concat(insufficientFundsBidFailures).Concat(noSpaceLeftBidFailures);

            var processedSet = new ProcessedBidSet(winningBids, failedBids);
            return processedSet;
        }

        private IReadOnlyList<PickupBid> GetWinnableBids(IEnumerable<PickupBid> activeBidsForLeagueYear, LeagueOptions options, SystemWideValues systemWideValues)
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
                    var bestBidsByProjectedScore = bestBids.MinBy(x => x.Publisher.GetProjectedFantasyPoints(options, systemWideValues, false));
                    bestBid = bestBidsByProjectedScore.OrderByDescending(x => x.Publisher.DraftPosition).First();
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
            Dictionary<Publisher, List<PublisherGame>> gamesToAdd = new Dictionary<Publisher, List<PublisherGame>>();
            List<LeagueAction> leagueActions = new List<LeagueAction>();
            List<BudgetExpenditure> expenditures = new List<BudgetExpenditure>();
            foreach (var successBid in successBids)
            {
                PublisherGame newPublisherGame = new PublisherGame(successBid.Publisher.PublisherID, Guid.NewGuid(), successBid.MasterGame.GameName, _clock.GetCurrentInstant(), 
                    false, null, null, new MasterGameYear(successBid.MasterGame, successBid.Publisher.Year), null, null, successBid.Publisher.Year);
                if (!gamesToAdd.ContainsKey(successBid.Publisher))
                {
                    gamesToAdd[successBid.Publisher] = new List<PublisherGame>();
                }

                gamesToAdd[successBid.Publisher].Add(newPublisherGame);
                expenditures.Add(new BudgetExpenditure(successBid.Publisher, successBid.BidAmount));
                
                LeagueAction leagueAction = new LeagueAction(successBid, _clock.GetCurrentInstant());
                leagueActions.Add(leagueAction);
            }

            foreach (var failedBid in failedBids)
            {
                LeagueAction leagueAction = new LeagueAction(failedBid, _clock.GetCurrentInstant());
                leagueActions.Add(leagueAction);
            }

            var simpleFailedBids = failedBids.Select(x => x.PickupBid);

            await _fantasyCriticRepo.MarkBidStatus(successBids, true);
            await _fantasyCriticRepo.MarkBidStatus(simpleFailedBids, false);
            await _fantasyCriticRepo.AddLeagueActions(leagueActions);
            await _fantasyCriticRepo.SpendBudgets(expenditures);
        }

        public Task ChangePublisherName(Publisher publisher, string publisherName)
        {
            return _fantasyCriticRepo.ChangePublisherName(publisher, publisherName);
        }

        public Task ChangeLeagueOptions(League league, string leagueName, bool publicLeague, bool testLeague)
        {
            return _fantasyCriticRepo.ChangeLeagueOptions(league, leagueName, publicLeague, testLeague);
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

        public Maybe<Publisher> GetNextDraftPublisher(LeagueYear leagueYear, IReadOnlyList<Publisher> publishersInLeagueForYear)
        {
            if (!leagueYear.PlayStatus.DraftIsActive)
            {
                return Maybe<Publisher>.None;
            }

            DraftPhase phase = GetDraftPhase(leagueYear, publishersInLeagueForYear);
            if (phase.Equals(DraftPhase.StandardGames))
            {
                var publishersWithLowestNumberOfGames = publishersInLeagueForYear.MinBy(x => x.PublisherGames.Count(y => !y.CounterPick));
                var allPlayersHaveSameNumberOfGames = publishersInLeagueForYear.Select(x => x.PublisherGames.Count(y => !y.CounterPick)).Distinct().Count() == 1;
                var maxNumberOfGames = publishersInLeagueForYear.Max(x => x.PublisherGames.Count(y => !y.CounterPick));
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
                var publishersWithLowestNumberOfGames = publishersInLeagueForYear.MinBy(x => x.PublisherGames.Count(y => y.CounterPick));
                var allPlayersHaveSameNumberOfGames = publishersInLeagueForYear.Select(x => x.PublisherGames.Count(y => y.CounterPick)).Distinct().Count() == 1;
                var maxNumberOfGames = publishersInLeagueForYear.Max(x => x.PublisherGames.Count(y => y.CounterPick));

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

        public IReadOnlyList<PublisherGame> GetAvailableCounterPicks(LeagueYear leagueYear, Publisher nextDraftingPublisher, IReadOnlyList<Publisher> publishersInLeagueForYear)
        {
            IReadOnlyList<Publisher> otherPublishers = publishersInLeagueForYear.Where(x => x.PublisherID != nextDraftingPublisher.PublisherID).ToList();

            IReadOnlyList<PublisherGame> gamesForYear = publishersInLeagueForYear.SelectMany(x => x.PublisherGames).ToList();
            IReadOnlyList<PublisherGame> otherPlayersGames = otherPublishers.SelectMany(x => x.PublisherGames).Where(x => !x.CounterPick).ToList();

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

        public Task<SystemWideValues> GetLeagueWideValues()
        {
            return _fantasyCriticRepo.GetSystemWideValues();
        }

        public async Task DeleteLeague(League league)
        {
            var invites = await _fantasyCriticRepo.GetOutstandingInvitees(league);
            foreach (var invite in invites)
            {
                await _fantasyCriticRepo.RescindInvite(league, invite);
            }

            foreach (var year in league.Years)
            {
                var leagueYear = await _fantasyCriticRepo.GetLeagueYear(league, year);
                var publishers = await _fantasyCriticRepo.GetPublishersInLeagueForYear(league, year);
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

            bool userIsInLeague = await UserIsInLeague(league, user);
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
    }
}
