using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FantasyCritic.Lib.Services
{
    public class FantasyCriticService
    {
        private readonly FantasyCriticUserManager _userManager;
        private readonly IFantasyCriticRepo _fantasyCriticRepo;

        public FantasyCriticService(FantasyCriticUserManager userManager, IFantasyCriticRepo fantasyCriticRepo)
        {
            _userManager = userManager;
            _fantasyCriticRepo = fantasyCriticRepo;
        }

        public Task<Maybe<FantasyCriticLeague>> GetLeagueByID(Guid id)
        {
            return _fantasyCriticRepo.GetLeagueByID(id);
        }

        public async Task<IReadOnlyList<FantasyCriticUser>> GetPlayersInLeague(FantasyCriticLeague league)
        {
            IReadOnlyList<Guid> ids = await _fantasyCriticRepo.GetPlayerIDsInLeague(league);

            List<FantasyCriticUser> players = new List<FantasyCriticUser>();
            foreach (var id in ids)
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                players.Add(user);
            }

            return players;
        }
    }
}
