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

        public async Task<IActionResult> LeagueOptions()
        {
            IReadOnlyList<int> openYears = await _fantasyCriticService.GetOpenYears();
            LeagueOptionsViewModel viewModel = new LeagueOptionsViewModel(openYears, EligibilitySystem.GetAllPossibleValues(), DraftSystem.GetAllPossibleValues(),
                WaiverSystem.GetAllPossibleValues(), ScoringSystem.GetAllPossibleValues());

            return Ok(viewModel);
        }

        public async Task<IActionResult> MyLeagues()
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            IReadOnlyList<League> myLeagues = await _fantasyCriticService.GetLeaguesForUser(currentUser);

            List<LeagueViewModel> viewModels = new List<LeagueViewModel>();
            foreach (var league in myLeagues)
            {
                bool isManager = (league.LeagueManager.UserID == currentUser.UserID);
                viewModels.Add(new LeagueViewModel(league, isManager));
            }

            return Ok(viewModels);
        }

        public async Task<IActionResult> MyInvites()
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            var invitedLeagues = await _fantasyCriticService.GetLeaguesInvitedTo(currentUser);
            var viewModels = invitedLeagues.Select(x => new InviteViewModel(x));
            return Ok(viewModels);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLeague(Guid id)
        {
            Maybe<League> league = await _fantasyCriticService.GetLeagueByID(id);
            if (league.HasNoValue)
            {
                return NotFound();
            }

            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            var playersInLeague = await _fantasyCriticService.GetUsersInLeague(league.Value);
            bool userIsInLeague = playersInLeague.Any(x => x.UserID == currentUser.UserID);

            var inviteesToLeague = await _fantasyCriticService.GetOutstandingInvitees(league.Value);
            bool userIsInvitedToLeague = inviteesToLeague.Any(x => x.UserID == currentUser.UserID);
            if (!userIsInLeague && !userIsInvitedToLeague)
            {
                return Unauthorized();
            }

            bool isManager = (league.Value.LeagueManager.UserID == currentUser.UserID);
            var leagueViewModel = new LeagueViewModel(league.Value, isManager, playersInLeague, inviteesToLeague, userIsInvitedToLeague);
            return Ok(leagueViewModel);
        }

        public async Task<IActionResult> GetLeagueYear(Guid leagueID, int year)
        {
            Maybe<LeagueYear> leagueYear = await _fantasyCriticService.GetLeagueYear(leagueID, year);
            if (leagueYear.HasNoValue)
            {
                throw new Exception("Something went really wrong, no options are set up for this league.");
            }

            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            var usersInLeague = await _fantasyCriticService.GetUsersInLeague(leagueYear.Value.League);
            bool userIsInLeague = usersInLeague.Any(x => x.UserID == currentUser.UserID);

            var inviteesToLeague = await _fantasyCriticService.GetOutstandingInvitees(leagueYear.Value.League);
            bool userIsInvitedToLeague = inviteesToLeague.Any(x => x.UserID == currentUser.UserID);
            if (!userIsInLeague && !userIsInvitedToLeague)
            {
                return Unauthorized();
            }

            var publishersInLeague = await _fantasyCriticService.GetPublishersInLeagueForYear(leagueYear.Value.League, leagueYear.Value.Year);

            var leagueViewModel = new LeagueYearViewModel(leagueYear.Value, publishersInLeague);
            return Ok(leagueViewModel);
        }

        public async Task<IActionResult> GetPublisher(Guid leagueID, Guid playerID, int year)
        {
            Maybe<League> league = await _fantasyCriticService.GetLeagueByID(leagueID);
            if (league.HasNoValue)
            {
                return NotFound();
            }

            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            var playersInLeague = await _fantasyCriticService.GetUsersInLeague(league.Value);
            bool userIsInLeague = playersInLeague.Any(x => x.UserID == currentUser.UserID);
            if (!userIsInLeague)
            {
                return Unauthorized();
            }

            bool leaguePlayingYear = league.Value.Years.Contains(year);
            if (!leaguePlayingYear)
            {
                return BadRequest("League is not playing that year.");
            }
            
            var requstedPlayerIsInLeague = playersInLeague.Any(x => x.UserID == playerID);
            if (!requstedPlayerIsInLeague)
            {
                return BadRequest("Requested player is not in requested league.");
            }

            var requestedPlayer = await _userManager.FindByIdAsync(playerID.ToString());
            var publisher = await _fantasyCriticService.GetPublisher(league.Value, year, requestedPlayer);
            if (publisher.HasNoValue)
            {
                return NotFound();
            }

            var publisherViewModel = new PublisherViewModel(publisher.Value);
            return Ok(publisherViewModel);
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
            await _fantasyCriticService.CreateLeague(domainRequest);

            return Ok();
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

        [HttpPost]
        public async Task<IActionResult> CreatePublisher([FromBody] CreatePublisherRequest request)
        {
            var league = await _fantasyCriticService.GetLeagueByID(request.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            bool userIsInLeague = await _fantasyCriticService.UserIsInLeague(league.Value, currentUser);
            if (!userIsInLeague)
            {
                return Forbid();
            }

            await _fantasyCriticService.CreatePublisher(league.Value, request.Year, currentUser, request.PublisherName);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeclineInvite([FromBody] DeclineInviteRequest request)
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

            Result result = await _fantasyCriticService.DeclineInvite(league.Value, currentUser);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ManagerClaimGame([FromBody] ClaimGameRequest request)
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

            if (league.Value.LeagueManager.UserID == currentUser.UserID)
            {
                return Forbid();
            }

            var claimUser = await _userManager.FindByIdAsync(request.UserID.ToString());
            if (claimUser == null)
            {
                return BadRequest();
            }

            Maybe<MasterGame> masterGame = Maybe<MasterGame>.None;
            if (request.MasterGameID.HasValue)
            {
                masterGame = await _fantasyCriticService.GetMasterGame(request.MasterGameID.Value);
            }

            var publisher = await _fantasyCriticService.GetPublisher(league.Value, request.Year, claimUser);
            if (publisher.HasNoValue)
            {
                return BadRequest();
            }

            ClaimGameDomainRequest domainRequest = new ClaimGameDomainRequest(publisher.Value, request.GameName, request.Waiver, request.AntiPick, masterGame);

            Result result = await _fantasyCriticService.ClaimGame(domainRequest);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }
    }
}
