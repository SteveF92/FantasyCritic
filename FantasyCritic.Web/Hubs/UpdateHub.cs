using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NodaTime;

namespace FantasyCritic.Web.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UpdateHub : Hub
    {
        private readonly FantasyCriticUserManager _userManager;
        private readonly FantasyCriticService _fantasyCriticService;

        public UpdateHub(FantasyCriticUserManager userManager, FantasyCriticService fantasyCriticService)
        {
            _userManager = userManager;
            _fantasyCriticService = fantasyCriticService;
        }

        public async Task RefreshLeagueYear(LeagueYear leagueYear)
        {
            await Clients.Group(GetGroupName(leagueYear)).SendAsync("RefreshLeagueYear");
        }

        public override async Task OnConnectedAsync()
        {
            var currentUser = await _userManager.FindByNameAsync(Context.User.Identity.Name);
            var leagues = await _fantasyCriticService.GetLeaguesForUser(currentUser);
            List<LeagueYear> draftingLeagueYears = new List<LeagueYear>();
            foreach (var league in leagues)
            {
                foreach (var year in league.Years)
                {
                    var leagueYear = await _fantasyCriticService.GetLeagueYear(league.LeagueID, year);
                    if (leagueYear.HasValue && leagueYear.Value.PlayStatus.DraftIsActive)
                    {
                        draftingLeagueYears.Add(leagueYear.Value);
                    }
                }
            }

            foreach (var leagueYear in draftingLeagueYears)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(leagueYear));
            }
        }

        private static string GetGroupName(LeagueYear leagueYear) => $"{leagueYear.League.LeagueID}|{leagueYear.Year}";
    }
}
