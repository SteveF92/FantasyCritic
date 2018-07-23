using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.OpenCritic;

namespace FantasyCritic.Lib.Interfaces
{
    public interface IFantasyCriticRepo
    {
        Task<Maybe<FantasyCriticLeague>> GetLeagueByID(Guid id);
        Task<IReadOnlyList<FantasyCriticUser>> GetPlayersInLeague(FantasyCriticLeague league);
        Task CreateLeague(FantasyCriticLeague league, int initialYear);
        Task SaveInvite(FantasyCriticLeague league, FantasyCriticUser user);
        Task<IReadOnlyList<FantasyCriticUser>> GetOutstandingInvitees(FantasyCriticLeague league);
        Task AcceptInvite(FantasyCriticLeague league, FantasyCriticUser inviteUser);
        Task DeclineInvite(FantasyCriticLeague league, FantasyCriticUser inviteUser);
        Task<IReadOnlyList<int>> GetOpenYears();
        Task<IReadOnlyList<FantasyCriticLeague>> GetLeaguesForUser(FantasyCriticUser currentUser);
        Task<IReadOnlyList<FantasyCriticLeague>> GetLeaguesInvitedTo(FantasyCriticUser currentUser);
        Task<IReadOnlyList<MasterGame>> GetMasterGames();
        Task<Maybe<MasterGame>> GetMasterGame(Guid masterGameID);
        Task UpdateCriticStats(MasterGame masterGame, OpenCriticGame openCriticGame);
        Task AddPlayerGame(FantasyCriticLeague requestLeague, PlayerGame playerGame);
        Task<IReadOnlyList<PlayerGame>> GetPlayerGames(FantasyCriticLeague league, FantasyCriticUser user);
        Task<IReadOnlyList<PlayerGame>> GetPlayerGames(FantasyCriticLeague league);
    }
}
