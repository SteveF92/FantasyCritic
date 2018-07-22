using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Enums;
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
    public class GameController : Controller
    {
        private readonly FantasyCriticUserManager _userManager;
        private readonly FantasyCriticService _fantasyCriticService;

        public GameController(FantasyCriticUserManager userManager, FantasyCriticService fantasyCriticService)
        {
            _userManager = userManager;
            _fantasyCriticService = fantasyCriticService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MasterGameViewModel>> MasterGame(Guid id)
        {
            var masterGame = await _fantasyCriticService.GetMasterGame(id);
            if (masterGame.HasNoValue)
            {
                return NotFound();
            }

            var viewModel = new MasterGameViewModel(masterGame.Value);
            return viewModel;
        }

        public async Task<ActionResult<List<MasterGameViewModel>>> MasterGame()
        {
            var masterGames = await _fantasyCriticService.GetMasterGames();
            List<MasterGameViewModel> viewModels = masterGames.Select(x => new MasterGameViewModel(x)).ToList();
            return viewModels;
        }
    }
}
