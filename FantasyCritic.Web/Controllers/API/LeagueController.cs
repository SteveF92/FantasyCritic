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
    public class LeagueController : Controller
    {
        private readonly FantasyCriticUserManager _userManager;
        private readonly FantasyCriticService _fantasyCriticService;

        public LeagueController(FantasyCriticUserManager userManager, FantasyCriticService fantasyCriticService)
        {
            _userManager = userManager;
            _fantasyCriticService = fantasyCriticService;
        }

        public IActionResult LeagueOptions()
        {
            LeagueOptionsViewModel viewModel = new LeagueOptionsViewModel(EligibilitySystem.GetAllPossibleValues(), DraftSystem.GetAllPossibleValues(),
                WaiverSystem.GetAllPossibleValues(), ScoringSystem.GetAllPossibleValues());

            return Ok(viewModel);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLeague(Guid id)
        {
            Maybe<FantasyCriticLeague> league = await _fantasyCriticService.GetLeagueByID(id);
            if (league.HasNoValue)
            {
                return NotFound();
            }

            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            var playersInLeague = await _fantasyCriticService.GetPlayersInLeague(league.Value);
            bool userIsInLeague = playersInLeague.Any(x => x.UserID == currentUser.UserID);
            if (!userIsInLeague)
            {
                return Unauthorized();
            }

            var leagueViewModel = new FantasyCriticLeagueViewModel(league.Value, playersInLeague);
            return Ok(leagueViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLeague([FromBody] CreateLeagueRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            LeagueCreationParameters domainRequest = request.ToDomain(currentUser);
            var league = await _fantasyCriticService.CreateLeague(domainRequest);

            return CreatedAtRoute("GetLeague", league.LeagueID);
        }

        [HttpPost]
        public async Task<IActionResult> InvitePlayer([FromBody] InviteRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var league = await _fantasyCriticService.GetLeagueByID(request.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            if (league.Value.LeagueManager.UserID != currentUser.UserID)
            {
                return Unauthorized();
            }

            var inviteUser = await _userManager.FindByEmailAsync(request.InviteEmail);
            if (inviteUser == null)
            {
                return BadRequest();
            }

            Result result = await _fantasyCriticService.InviteUser(league.Value, inviteUser);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AcceptInvite([FromBody] AcceptInviteRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var league = await _fantasyCriticService.GetLeagueByID(request.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            Result result = await _fantasyCriticService.AcceptInvite(league.Value, currentUser);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }
    }
}
