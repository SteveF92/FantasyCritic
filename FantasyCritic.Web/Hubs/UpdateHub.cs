using System;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FantasyCritic.Web.Hubs
{
    public class UpdateHub : Hub
    {
        private readonly FantasyCriticService _fantasyCriticService;

        public UpdateHub(FantasyCriticService fantasyCriticService)
        {
            _fantasyCriticService = fantasyCriticService;
        }

        public async Task Subscribe(string leagueID, string year)
        {
            Guid leagueGUID = Guid.Parse(leagueID);
            int yearInt = int.Parse(year);
            var leagueYear = await _fantasyCriticService.GetLeagueYear(leagueGUID, yearInt);
            if (leagueYear.HasNoValue)
            {
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, leagueYear.Value.GetGroupName);
        }
    }
}
