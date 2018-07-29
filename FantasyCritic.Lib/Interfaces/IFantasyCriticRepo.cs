using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.OpenCritic;

namespace FantasyCritic.Lib.Interfaces
{
    public interface IFantasyCriticRepo
    {
        Task<Maybe<League>> GetLeagueByID(Guid id);
        Task<Maybe<LeagueYear>> GetLeagueYear(League requestLeague, int requestYear);
        Task<IReadOnlyList<FantasyCriticUser>> GetPlayersInLeague(League league);
        Task CreateLeague(League league, int initialYear, LeagueOptions options);
        Task SaveInvite(League league, FantasyCriticUser user);
        Task<IReadOnlyList<FantasyCriticUser>> GetOutstandingInvitees(League league);
        Task AcceptInvite(League league, FantasyCriticUser inviteUser);
        Task DeclineInvite(League league, FantasyCriticUser inviteUser);
        Task<IReadOnlyList<int>> GetOpenYears();
        Task<IReadOnlyList<League>> GetLeaguesForUser(FantasyCriticUser currentUser);
        Task<IReadOnlyList<League>> GetLeaguesInvitedTo(FantasyCriticUser currentUser);
        Task<IReadOnlyList<MasterGame>> GetMasterGames();
        Task<Maybe<MasterGame>> GetMasterGame(Guid masterGameID);
        Task UpdateCriticStats(MasterGame masterGame, OpenCriticGame openCriticGame);
        Task AddPlayerGame(League requestLeague, PlayerGame playerGame);
        Task<IReadOnlyList<PlayerGame>> GetPlayerGames(League league, FantasyCriticUser user);
        Task<bool> GameIsEligible(MasterGame masterGame, EligibilitySystem eligibilitySystem);
    }
}
