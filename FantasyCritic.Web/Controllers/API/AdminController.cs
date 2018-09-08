using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Models;
using FantasyCritic.Web.Models.Requests;
using FantasyCritic.Web.Models.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FantasyCritic.Web.Controllers.API
{
    [Route("api/[controller]/[action]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AdminController : Controller
    {
        private readonly FantasyCriticUserManager _userManager;
        private readonly FantasyCriticService _fantasyCriticService;
        private readonly IOpenCriticService _openCriticService;

        public AdminController(FantasyCriticUserManager userManager, FantasyCriticService fantasyCriticService, IOpenCriticService openCriticService)
        {
            _userManager = userManager;
            _fantasyCriticService = fantasyCriticService;
            _openCriticService = openCriticService;
        }

        [HttpPost]
        public async Task<IActionResult> RefreshCriticInfo()
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            bool isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            if (!isAdmin)
            {
                return StatusCode(403);
            }

            var masterGames = await _fantasyCriticService.GetMasterGames();
            foreach (var masterGame in masterGames)
            {
                if (!masterGame.OpenCriticID.HasValue)
                {
                    continue;
                }

                var openCriticGame = await _openCriticService.GetOpenCriticGame(masterGame.OpenCriticID.Value);
                if (openCriticGame.HasValue)
                {
                    await _fantasyCriticService.UpdateCriticStats(masterGame, openCriticGame.Value);
                }
            }

            return Ok();
        }
    }
}
