using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.Web.Models;
using FantasyCritic.Web.Models.Requests;
using FantasyCritic.Web.Models.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace FantasyCritic.Web.Controllers.API
{
    [Route("api/[controller]/[action]")]
    public class GameController : Controller
    {
        private readonly FantasyCriticUserManager _userManager;
        private readonly InterLeagueService _interLeagueService;
        private readonly IClock _clock;

        public GameController(FantasyCriticUserManager userManager, InterLeagueService interLeagueService, IClock clock)
        {
            _userManager = userManager;
            _interLeagueService = interLeagueService;
            _clock = clock;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MasterGameViewModel>> MasterGame(Guid id)
        {
            var masterGame = await _interLeagueService.GetMasterGame(id);
            if (masterGame.HasNoValue)
            {
                return NotFound();
            }

            var viewModel = new MasterGameViewModel(masterGame.Value, _clock);
            return viewModel;
        }

        [HttpGet("{id}/{year}")]
        public async Task<ActionResult<MasterGameYearViewModel>> MasterGameYear(Guid id, int year)
        {
            Maybe<MasterGameYear> masterGame = await _interLeagueService.GetMasterGameYear(id, year);
            if (masterGame.HasNoValue)
            {
                return NotFound();
            }

            var viewModel = new MasterGameYearViewModel(masterGame.Value, _clock);
            return viewModel;
        }

        public async Task<ActionResult<List<MasterGameViewModel>>> MasterGame(string gameName)
        {
            if (string.IsNullOrWhiteSpace(gameName))
            {
                return new List<MasterGameViewModel>();
            }

            IReadOnlyList<MasterGame> masterGames = await _interLeagueService.GetMasterGames();
            IReadOnlyList<MasterGame> matchingMasterGames = MasterGameSearching.SearchMasterGames(gameName, masterGames);
            List<MasterGameViewModel> viewModels = matchingMasterGames.Select(x => new MasterGameViewModel(x, _clock)).ToList();

            return viewModels;
        }

        public async Task<ActionResult<List<MasterGameYearViewModel>>> MasterGameYear(string gameName, int year)
        {
            if (string.IsNullOrWhiteSpace(gameName))
            {
                return new List<MasterGameYearViewModel>();
            }

            IReadOnlyList<MasterGameYear> masterGames = await _interLeagueService.GetMasterGameYears(year);
            IReadOnlyList<MasterGameYear> matchingMasterGames = MasterGameSearching.SearchMasterGameYears(gameName, masterGames);
            List<MasterGameYearViewModel> viewModels = matchingMasterGames.Select(x => new MasterGameYearViewModel(x, _clock)).ToList();

            return viewModels;
        }

        [HttpGet("{year}")]
        public async Task<ActionResult<List<MasterGameYearViewModel>>> MasterGameYear(int year)
        {
            IReadOnlyList<MasterGameYear> masterGames = await _interLeagueService.GetMasterGameYears(year);
            var relevantGames = masterGames.Where(x => x.MasterGame.MinimumReleaseYear >= year);

            var supportedYears = await GetSupportedYears();
            var finishedYears = supportedYears.Where(x => x.Finished);
            bool thisYearIsFinished = finishedYears.Any(x => x.Year == year);
            if (thisYearIsFinished)
            {
                var chosenGames = await _interLeagueService.GetAllSelectedMasterGameIDsForYear(year);
                relevantGames = relevantGames.Where(x => chosenGames.Contains(x.MasterGame.MasterGameID));
            }

            List<MasterGameYearViewModel> viewModels = relevantGames.Select(x => new MasterGameYearViewModel(x, _clock)).ToList();

            return viewModels;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateMasterGameRequest([FromBody] MasterGameRequestRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            EligibilityLevel eligibilityLevel = await _interLeagueService.GetEligibilityLevel(request.EligibilityLevel);
            MasterGameRequest domainRequest = request.ToDomain(currentUser, _clock.GetCurrentInstant(), eligibilityLevel);

            await _interLeagueService.CreateMasterGameRequest(domainRequest);
            return Ok();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateMasterGameChangeRequest([FromBody] MasterGameChangeRequestRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            Maybe<MasterGame> masterGame = await _interLeagueService.GetMasterGame(request.MasterGameID);
            if (masterGame.HasNoValue)
            {
                return NotFound();
            }

            MasterGameChangeRequest domainRequest = request.ToDomain(currentUser, _clock.GetCurrentInstant(), masterGame.Value);
            await _interLeagueService.CreateMasterGameChangeRequest(domainRequest);
            return Ok();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteMasterGameRequest([FromBody] MasterGameRequestDeletionRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            Maybe<MasterGameRequest> maybeRequest = await _interLeagueService.GetMasterGameRequest(request.RequestID);
            if (maybeRequest.HasNoValue)
            {
                return BadRequest("That request does not exist.");
            }

            var domainRequest = maybeRequest.Value;
            if (domainRequest.User.UserID != currentUser.UserID)
            {
                return Forbid();
            }

            await _interLeagueService.DeleteMasterGameRequest(domainRequest);
            return Ok();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteMasterGameChangeRequest([FromBody] MasterGameChangeRequestDeletionRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            Maybe<MasterGameChangeRequest> maybeRequest = await _interLeagueService.GetMasterGameChangeRequest(request.RequestID);
            if (maybeRequest.HasNoValue)
            {
                return BadRequest("That request does not exist.");
            }

            var domainRequest = maybeRequest.Value;
            if (domainRequest.User.UserID != currentUser.UserID)
            {
                return Forbid();
            }

            await _interLeagueService.DeleteMasterGameChangeRequest(domainRequest);
            return Ok();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DismissMasterGameRequest([FromBody] MasterGameRequestDismissRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            Maybe<MasterGameRequest> maybeRequest = await _interLeagueService.GetMasterGameRequest(request.RequestID);
            if (maybeRequest.HasNoValue)
            {
                return BadRequest("That request does not exist.");
            }

            var domainRequest = maybeRequest.Value;
            if (domainRequest.User.UserID != currentUser.UserID)
            {
                return Forbid();
            }

            await _interLeagueService.DismissMasterGameRequest(domainRequest);
            return Ok();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DismissMasterGameChangeRequest([FromBody] MasterGameChangeRequestDismissRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            Maybe<MasterGameChangeRequest> maybeRequest = await _interLeagueService.GetMasterGameChangeRequest(request.RequestID);
            if (maybeRequest.HasNoValue)
            {
                return BadRequest("That request does not exist.");
            }

            var domainRequest = maybeRequest.Value;
            if (domainRequest.User.UserID != currentUser.UserID)
            {
                return Forbid();
            }

            await _interLeagueService.DismissMasterGameChangeRequest(domainRequest);
            return Ok();
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<List<MasterGameRequestViewModel>>> MyMasterGameRequests()
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            IReadOnlyList<MasterGameRequest> requests = await _interLeagueService.GetMasterGameRequestsForUser(currentUser);

            var viewModels = requests.Select(x => new MasterGameRequestViewModel(x, _clock)).ToList();
            return viewModels;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<List<MasterGameChangeRequestViewModel>>> MyMasterGameChangeRequests()
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            IReadOnlyList<MasterGameRequest> requests = await _interLeagueService.GetMasterGameChangeRequestsForUser(currentUser);

            var viewModels = requests.Select(x => new MasterGameChangeRequestViewModel(x, _clock)).ToList();
            return viewModels;
        }

        public async Task<IActionResult> EligibilityLevels()
        {
            IReadOnlyList<EligibilityLevel> eligibilityLevels = await _interLeagueService.GetEligibilityLevels();
            var vms = eligibilityLevels.Select(x => new EligibilityLevelViewModel(x, true));
            return Ok(vms);
        }

        public async Task<ActionResult<List<int>>> SupportedYears()
        {
            var supportedYears = await GetSupportedYears();
            return supportedYears.Select(x => x.Year).ToList();
        }

        private async Task<IReadOnlyList<SupportedYear>> GetSupportedYears()
        {
            IReadOnlyList<SupportedYear> supportedYears = await _interLeagueService.GetSupportedYears();
            var years = supportedYears.Where(x => x.Year > 2017).OrderByDescending(x => x);
            return years.ToList();
        }
    }
}
