using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;

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
    }
}
