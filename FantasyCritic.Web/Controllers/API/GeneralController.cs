using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Hubs;
using FantasyCritic.Web.Models;
using FantasyCritic.Web.Models.Requests;
using FantasyCritic.Web.Models.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NodaTime;

namespace FantasyCritic.Web.Controllers.API
{
    [Route("api/[controller]/[action]")]
    public class GeneralController : ControllerBase
    {
        private readonly FantasyCriticService _fantasyCriticService;
        private readonly InterLeagueService _interLeagueService;

        public GeneralController(FantasyCriticService fantasyCriticService, InterLeagueService interLeagueService)
        {
            _fantasyCriticService = fantasyCriticService;
            _interLeagueService = interLeagueService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> SiteCounts()
        {
            var counts = await _interLeagueService.GetSiteCounts();
            return Ok(new SiteCountsViewModel(counts));
        }
    }
}
