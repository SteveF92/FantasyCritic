using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using Microsoft.AspNetCore.Identity;
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

        public async Task<League> CreateLeague(LeagueCreationParameters parameters)
        {
            LeagueOptions options = new LeagueOptions(parameters);
            IEnumerable<int> years = new List<int>() { parameters.InitialYear };
            League newLeague = new League(Guid.NewGuid(), parameters.LeagueName, parameters.Manager, years);
            await _fantasyCriticRepo.CreateLeague(newLeague, parameters.InitialYear, options);
            return newLeague;
        }

        public Task AddNewLeagueYear(League league, int year, LeagueOptions options)
        {
            return _fantasyCriticRepo.AddNewLeagueYear(league, year, options);
        }

        public async Task<Publisher> CreatePublisher(League league, int year, FantasyCriticUser user, string publisherName)
        {
            Publisher publisher = new Publisher(Guid.NewGuid(), league, user, year, publisherName, null, new List<PublisherGame>());
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
            PublisherGame playerGame = new PublisherGame(Guid.NewGuid(), request.GameName, _clock.GetCurrentInstant(), request.Waiver, request.CounterPick, null, request.MasterGame, request.Publisher.Year);

            ClaimResult claimResult = await CanClaimGame(request);

            if (!claimResult.Success)
            {
                return claimResult;
            }

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

            await _fantasyCriticRepo.AssociatePublisherGame(request.Publisher, request.PublisherGame, request.MasterGame);

            return claimResult;
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

        public  Task<IReadOnlyList<int>> GetOpenYears()
        {
            return _fantasyCriticRepo.GetOpenYears();
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
                var masterGameErrors = GetMasterGameErrors(leagueYear.Value.Options, request.MasterGame.Value);
                claimErrors.AddRange(masterGameErrors);
            }

            IReadOnlyList<Publisher> allPublishers = await _fantasyCriticRepo.GetPublishersInLeagueForYear(request.Publisher.League, request.Publisher.Year);
            IReadOnlyList<Publisher> publishersForYear = allPublishers.Where(x => x.Year == leagueYear.Value.Year).ToList();
            IReadOnlyList<Publisher> otherPublishers = publishersForYear.Where(x => x.User.UserID != request.Publisher.User.UserID).ToList();

            IReadOnlyList<PublisherGame> gamesForYear = publishersForYear.SelectMany(x => x.PublisherGames).ToList();
            IReadOnlyList<PublisherGame> thisPlayersGames = request.Publisher.PublisherGames;
            IReadOnlyList<PublisherGame> otherPlayersGames = otherPublishers.SelectMany(x => x.PublisherGames).ToList();

            bool gameAlreadyClaimed = gamesForYear.ContainsGame(request);

            if (!request.Waiver && !request.CounterPick)
            {
                if (gameAlreadyClaimed)
                {
                    claimErrors.Add(new ClaimError("Cannot draft a game that someone already has.", false));
                }

                int leagueDraftGames = yearOptions.DraftGames;
                int userDraftGames = thisPlayersGames.Count(x => !x.Waiver && !x.CounterPick);
                if (userDraftGames == leagueDraftGames)
                {
                    claimErrors.Add(new ClaimError("User's draft spaces are filled.", false));
                }
            }

            if (request.Waiver)
            {
                if (gameAlreadyClaimed)
                {
                    claimErrors.Add(new ClaimError("Cannot waiver claim a game that someone already has.", false));
                }

                int leagueWaiverGames = yearOptions.WaiverGames;
                int userWaiverGames = thisPlayersGames.Count(x => x.Waiver);
                if (userWaiverGames == leagueWaiverGames)
                {
                    claimErrors.Add(new ClaimError("User's waiver spaces are filled.", false));
                }
            }

            if (request.CounterPick)
            {
                bool otherPlayerHasDraftGame = otherPlayersGames.Where(x => !x.CounterPick && !x.Waiver).ContainsGame(request);

                int leagueCounterPicks = yearOptions.CounterPicks;
                int userCounterPicks = thisPlayersGames.Count(x => x.CounterPick);
                if (userCounterPicks == leagueCounterPicks)
                {
                    claimErrors.Add(new ClaimError("User's counter pick spaces are filled.", false));
                }

                if (!otherPlayerHasDraftGame)
                {
                    claimErrors.Add(new ClaimError("Cannot counterpick a game that no other player is publishing.", false));
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
            IReadOnlyList<ClaimError> masterGameErrors = GetMasterGameErrors(leagueYear.Value.Options, request.MasterGame);
            associationErrors.AddRange(masterGameErrors);

            IReadOnlyList<Publisher> allPublishers = await _fantasyCriticRepo.GetPublishersInLeagueForYear(request.Publisher.League, request.Publisher.Year);
            IReadOnlyList<Publisher> publishersForYear = allPublishers.Where(x => x.Year == leagueYear.Value.Year).ToList();
            IReadOnlyList<Publisher> otherPublishers = publishersForYear.Where(x => x.User.UserID != request.Publisher.User.UserID).ToList();

            IReadOnlyList<PublisherGame> gamesForYear = publishersForYear.SelectMany(x => x.PublisherGames).ToList();
            IReadOnlyList<PublisherGame> otherPlayersGames = otherPublishers.SelectMany(x => x.PublisherGames).ToList();

            bool gameAlreadyClaimed = gamesForYear.ContainsGame(request.MasterGame);

            if (!request.PublisherGame.Waiver && !request.PublisherGame.CounterPick)
            {
                if (gameAlreadyClaimed)
                {
                    associationErrors.Add(new ClaimError("Cannot draft a game that someone already has.", false));
                }
            }

            if (request.PublisherGame.Waiver)
            {
                if (gameAlreadyClaimed)
                {
                    associationErrors.Add(new ClaimError("Cannot waiver claim a game that someone already has.", false));
                }
            }

            if (request.PublisherGame.CounterPick)
            {
                bool otherPlayerHasDraftGame = otherPlayersGames.Where(x => !x.CounterPick && !x.Waiver).ContainsGame(request.MasterGame);
                if (!otherPlayerHasDraftGame)
                {
                    associationErrors.Add(new ClaimError("Cannot counterpick a game that no other player is publishing.", false));
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

            var openYears = await GetOpenYears();
            if (!openYears.Contains(publisher.Year))
            {
                claimErrors.Add(new ClaimError("That year is not open for play", false));
            }

            return claimErrors;
        }

        private IReadOnlyList<ClaimError> GetMasterGameErrors(LeagueOptions yearOptions, MasterGame masterGame)
        {
            List<ClaimError> claimErrors = new List<ClaimError>();

            bool eligible = masterGame.IsEligible(yearOptions.MaximumEligibilityLevel);
            if (!eligible)
            {
                claimErrors.Add(new ClaimError("That game is not eligible under this league's settings.", true));
            }

            bool released = masterGame.IsReleased(_clock);
            if (released)
            {
                claimErrors.Add(new ClaimError("That game has already been released.", true));
            }

            bool hasScore = masterGame.CriticScore.HasValue;
            if (hasScore)
            {
                claimErrors.Add(new ClaimError("That game already has a score.", true));
            }

            return claimErrors;
        }

        public Task<Result> RemovePublisherGame(PublisherGame publisherGame)
        {
            return _fantasyCriticRepo.RemovePublisherGame(publisherGame.PublisherGameID);
        }
    }
}
