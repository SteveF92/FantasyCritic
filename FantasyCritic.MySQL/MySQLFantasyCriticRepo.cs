using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.MySQL
{
    public class MySQLFantasyCriticRepo : IFantasyCriticRepo
    {
        private readonly string _connectionString;

        public MySQLFantasyCriticRepo(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Task<Maybe<FantasyCriticLeague>> GetLeagueByID(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Guid>> GetPlayerIDsInLeague(FantasyCriticLeague league)
        {
            throw new NotImplementedException();
        }
    }
}
