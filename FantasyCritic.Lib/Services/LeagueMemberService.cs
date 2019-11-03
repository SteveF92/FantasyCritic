using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using NodaTime;

namespace FantasyCritic.Lib.Services
{
    public class LeagueMemberService
    {
        private readonly FantasyCriticUserManager _userManager;
        private readonly IFantasyCriticRepo _fantasyCriticRepo;
        private readonly IClock _clock;

        public LeagueMemberService(FantasyCriticUserManager userManager, IFantasyCriticRepo fantasyCriticRepo, IClock clock)
        {
            _userManager = userManager;
            _fantasyCriticRepo = fantasyCriticRepo;
            _clock = clock;
        }

        public async Task<bool> UserIsInLeague(League league, FantasyCriticUser user)
        {
            var playersInLeague = await GetUsersInLeague(league);
            return playersInLeague.Any(x => x.UserID == user.UserID);
        }

        public Task<IReadOnlyList<FantasyCriticUser>> GetUsersInLeague(League league)
        {
            return _fantasyCriticRepo.GetUsersInLeague(league);
        }

        public async Task<Result> InviteUserByEmail(League league, string inviteEmail)
        {
            var existingInvite = await GetMatchingInvite(league, inviteEmail);
            if (existingInvite.HasValue)
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
            IReadOnlyList<LeagueInvite> outstandingInvites = await GetOutstandingInvitees(league);
            int totalPlayers = players.Count + outstandingInvites.Count;

            if (totalPlayers >= 14)
            {
                return Result.Fail("A league cannot have more than 14 players.");
            }

            LeagueInvite invite = new LeagueInvite(Guid.NewGuid(), league, inviteEmail);

            await _fantasyCriticRepo.SaveInvite(invite);

            return Result.Ok();
        }

        public async Task<Result> InviteUserByUserID(League league, FantasyCriticUser inviteUser)
        {
            var existingInvite = await GetMatchingInvite(league, inviteUser);
            if (existingInvite.HasValue)
            {
                return Result.Fail("User is already invited to this league.");
            }

            bool userInLeague = await UserIsInLeague(league, inviteUser);
            if (userInLeague)
            {
                return Result.Fail("User is already in league.");
            }

            IReadOnlyList<FantasyCriticUser> players = await GetUsersInLeague(league);
            IReadOnlyList<LeagueInvite> outstandingInvites = await GetOutstandingInvitees(league);
            int totalPlayers = players.Count + outstandingInvites.Count;

            if (totalPlayers >= 14)
            {
                return Result.Fail("A league cannot have more than 14 players.");
            }

            LeagueInvite invite = new LeagueInvite(Guid.NewGuid(), league, inviteUser);

            await _fantasyCriticRepo.SaveInvite(invite);

            return Result.Ok();
        }

        public async Task<Result> AcceptInvite(League league, FantasyCriticUser inviteUser)
        {
            bool userInLeague = await UserIsInLeague(league, inviteUser);
            if (userInLeague)
            {
                return Result.Fail("User is already in league.");
            }

            var invite = await GetMatchingInvite(league, inviteUser.EmailAddress);
            if (invite.HasNoValue)
            {
                return Result.Fail("User is not invited to this league.");
            }

            await _fantasyCriticRepo.AcceptInvite(invite.Value, inviteUser);

            return Result.Ok();
        }

        public Task<IReadOnlyList<LeagueInvite>> GetOutstandingInvitees(League league)
        {
            return _fantasyCriticRepo.GetOutstandingInvitees(league);
        }

        public Task<IReadOnlyList<League>> GetLeaguesForUser(FantasyCriticUser user)
        {
            return _fantasyCriticRepo.GetLeaguesForUser(user);
        }

        public Task<IReadOnlyList<LeagueYear>> GetLeaguesYearsForUser(FantasyCriticUser user, int year)
        {
            return _fantasyCriticRepo.GetLeagueYearsForUser(user, year);
        }

        public Task<IReadOnlyList<LeagueInvite>> GetLeagueInvites(FantasyCriticUser user)
        {
            return _fantasyCriticRepo.GetLeagueInvites(user);
        }

        public Task<Maybe<LeagueInvite>> GetInvite(Guid inviteID)
        {
            return _fantasyCriticRepo.GetInvite(inviteID);
        }

        public Task DeleteInvite(LeagueInvite invite)
        {
            return _fantasyCriticRepo.DeleteInvite(invite);
        }

        public async Task RemovePlayerFromLeague(League league, FantasyCriticUser removeUser)
        {
            foreach (var year in league.Years)
            {
                var leagueYear = await _fantasyCriticRepo.GetLeagueYear(league, year);
                var allPublishers = await _fantasyCriticRepo.GetPublishersInLeagueForYear(leagueYear.Value);
                var deletePublisher = allPublishers.SingleOrDefault(x => x.User.UserID == removeUser.UserID);
                if (deletePublisher != null)
                {
                    await _fantasyCriticRepo.RemovePublisher(deletePublisher, allPublishers);
                }
            }

            await _fantasyCriticRepo.RemovePlayerFromLeague(league, removeUser);
        }

        public Task<IReadOnlyList<FantasyCriticUser>> GetActivePlayersForLeagueYear(League league, int year)
        {
            return _fantasyCriticRepo.GetActivePlayersForLeagueYear(league, year);
        }


        private async Task<Maybe<LeagueInvite>> GetMatchingInvite(League league, string emailAddress)
        {
            IReadOnlyList<LeagueInvite> playersInvited = await GetOutstandingInvitees(league);
            var invite = playersInvited.GetMatchingInvite(emailAddress);
            return invite;
        }

        private async Task<Maybe<LeagueInvite>> GetMatchingInvite(League league, FantasyCriticUser user)
        {
            IReadOnlyList<LeagueInvite> playersInvited = await GetOutstandingInvitees(league);
            var invite = playersInvited.GetMatchingInvite(user);
            return invite;
        }
    }
}
