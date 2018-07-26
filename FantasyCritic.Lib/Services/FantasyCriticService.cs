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

        public async Task<FantasyCriticLeague> CreateLeague(LeagueCreationParameters parameters)
        {
            LeagueOptions newOptions = null;
            LeagueOptions options = new LeagueOptions(parameters);
            FantasyCriticLeague newLeague = new FantasyCriticLeague(Guid.NewGuid(), parameters.LeagueName, parameters.Manager, new List<int>(parameters.InitialYear), options);
            await _fantasyCriticRepo.CreateLeague(newLeague, parameters.InitialYear);
            return newLeague;
        }

        public Task<Maybe<FantasyCriticLeague>> GetLeagueByID(Guid id)
        {
            return _fantasyCriticRepo.GetLeagueByID(id);
        }

        public async Task<IReadOnlyList<LeaguePlayer>> GetPlayersInLeague(FantasyCriticLeague league)
        {
            var users = await _fantasyCriticRepo.GetPlayersInLeague(league);
            List<LeaguePlayer> leaguePlayers = new List<LeaguePlayer>();
            foreach (var user in users)
            {
                IReadOnlyList<PlayerGame> playerGames = await GetPlayerGames(league, user);
                LeaguePlayer leaguePlayer = new LeaguePlayer(league, user, playerGames);
                leaguePlayers.Add(leaguePlayer);
            }

            return leaguePlayers;
        }

        public async Task<Result> InviteUser(FantasyCriticLeague league, FantasyCriticUser inviteUser)
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

        public async Task<Result> AcceptInvite(FantasyCriticLeague league, FantasyCriticUser inviteUser)
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

        public async Task<Result> DeclineInvite(FantasyCriticLeague league, FantasyCriticUser inviteUser)
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

        public Task<IReadOnlyList<FantasyCriticUser>> GetOutstandingInvitees(FantasyCriticLeague league)
        {
            return _fantasyCriticRepo.GetOutstandingInvitees(league);
        }

        public  Task<IReadOnlyList<int>> GetOpenYears()
        {
            return _fantasyCriticRepo.GetOpenYears();
        }

        public Task<IReadOnlyList<FantasyCriticLeague>> GetLeaguesForUser(FantasyCriticUser currentUser)
        {
            return _fantasyCriticRepo.GetLeaguesForUser(currentUser);
        }

        public Task<IReadOnlyList<FantasyCriticLeague>> GetLeaguesInvitedTo(FantasyCriticUser currentUser)
        {
            return _fantasyCriticRepo.GetLeaguesInvitedTo(currentUser);
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

        public async Task<Result> ClaimGame(ClaimGameDomainRequest request)
        {
            PlayerGame playerGame = new PlayerGame(request.User, request.Year, request.GameName, _clock.GetCurrentInstant(), request.Waiver, request.AntiPick, null, request.MasterGame);

            Result claimResult = await CanClaimGame(request);

            if (claimResult.IsFailure)
            {
                return claimResult;
            }

            await _fantasyCriticRepo.AddPlayerGame(request.League, playerGame);

            return Result.Ok();
        }

        public Task<IReadOnlyList<PlayerGame>> GetPlayerGames(FantasyCriticLeague league, FantasyCriticUser user)
        {
            return _fantasyCriticRepo.GetPlayerGames(league, user);
        }

        private async Task<bool> UserIsInLeague(FantasyCriticLeague league, FantasyCriticUser user)
        {
            var playersInLeague = await GetPlayersInLeague(league);
            return playersInLeague.Any(x => x.Player.UserID == user.UserID);
        }

        private async Task<bool> UserIsInvited(FantasyCriticLeague league, FantasyCriticUser inviteUser)
        {
            var playersInvited = await GetOutstandingInvitees(league);
            return playersInvited.Any(x => x.UserID == inviteUser.UserID);
        }

        private async Task<Result> CanClaimGame(ClaimGameDomainRequest request)
        {
            bool isInLeague = await UserIsInLeague(request.League, request.User);
            if (!isInLeague)
            {
                return Result.Fail("User is not in that league.");
            }

            if (!request.League.LeagueYears.Contains(request.Year))
            {
                return Result.Fail("League is not active for that year.");
            }

            var openYears = await GetOpenYears();
            if (!openYears.Contains(request.Year))
            {
                return Result.Fail("That year is not open for play");
            }

            if (request.MasterGame.HasValue)
            {
                bool eligible = await GameIsEligible(request.MasterGame.Value, request.League.LeagueOptions.EligibilitySystem);
                if (!eligible)
                {
                    Result.Fail("That game is not eligible under this league's settings.");
                }
            }

            IReadOnlyList<PlayerGame> allGamesInLeague = await _fantasyCriticRepo.GetPlayerGames(request.League);
            var gamesForYear = allGamesInLeague.Where(x => x.Year == request.Year).ToList();

            var thisPlayersGames = gamesForYear.Where(x => x.User.UserID == request.User.UserID).ToList();
            bool gameAlreadyClaimed = gamesForYear.ContainsGame(request);

            if (!request.Waiver && !request.AntiPick)
            {
                if (gameAlreadyClaimed)
                {
                    return Result.Fail("Cannot draft a game that someone already has.");
                }

                int leagueDraftGames = request.League.LeagueOptions.DraftGames;
                int userDraftGames = thisPlayersGames.Count(x => !x.Waiver && !x.AntiPick);
                if (userDraftGames == leagueDraftGames)
                {
                    return Result.Fail("User's draft spaces are filled.");
                }
            }

            if (request.Waiver)
            {
                if (gameAlreadyClaimed)
                {
                    return Result.Fail("Cannot waiver claim a game that someone already has.");
                }

                int leagueWaiverGames = request.League.LeagueOptions.WaiverGames;
                int userWaiverGames = thisPlayersGames.Count(x => x.Waiver);
                if (userWaiverGames == leagueWaiverGames)
                {
                    return Result.Fail("User's waiver spaces are filled.");
                }
            }

            if (request.AntiPick)
            {
                var otherPlayersGames = gamesForYear.Where(x => x.User.UserID != request.User.UserID);
                bool otherPlayerHasDraftGame = otherPlayersGames.Where(x => !x.AntiPick && !x.Waiver).ContainsGame(request);

                int leagueAntiPicks = request.League.LeagueOptions.AntiPicks;
                int userAntiPicks = thisPlayersGames.Count(x => x.AntiPick);
                if (userAntiPicks == leagueAntiPicks)
                {
                    return Result.Fail("User's anti pick spaces are filled.");
                }

                if (!otherPlayerHasDraftGame)
                {
                    return Result.Fail("Cannot antipick a game that no other player has drafted.");
                }
            }

            return Result.Ok();
        }

        private async Task<bool> GameIsEligible(MasterGame masterGame, EligibilitySystem eligibilitySystem)
        {
            if (eligibilitySystem.Equals(EligibilitySystem.Unlimited))
            {
                return true;
            }

            bool eligible = await _fantasyCriticRepo.GameIsEligible(masterGame, eligibilitySystem);
            return eligible;
        }
    }
}
