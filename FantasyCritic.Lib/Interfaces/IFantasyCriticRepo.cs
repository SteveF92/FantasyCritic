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
        Task<IReadOnlyList<Guid>> GetPlayerIDsInLeague(FantasyCriticLeague league);
    }
}
