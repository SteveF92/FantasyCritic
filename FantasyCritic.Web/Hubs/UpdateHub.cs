using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace FantasyCritic.Web.Hubs
{
    public class UpdateHub : Hub
    {
        public async Task RefreshLeagueYear()
        {
            await Clients.All.SendAsync("RefreshLeagueYear");
        }
    }
}
