using System;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace FantasyCritic.Web.Hubs
{
    public class UpdateHub : Hub
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly FantasyCriticService _fantasyCriticService;

        public UpdateHub(FantasyCriticService fantasyCriticService)
        {
            _fantasyCriticService = fantasyCriticService;
        }

        public async Task Subscribe(string leagueID, string year)
        {
            try
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
            catch (Exception e)
            {
                _logger.Error("SignalR fail!");
                _logger.Error(e);
                throw;
            }
            
        }
    }
}
