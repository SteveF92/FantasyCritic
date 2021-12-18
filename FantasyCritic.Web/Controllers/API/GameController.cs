using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.Web.Models;
using FantasyCritic.Web.Models.Requests;
using FantasyCritic.Web.Models.Requests.MasterGame;
using FantasyCritic.Web.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace FantasyCritic.Web.Controllers.API
{
    [Route("api/[controller]/[action]")]
    public class GameController : FantasyCriticController
    {
        private readonly FantasyCriticUserManager _userManager;
        private readonly InterLeagueService _interLeagueService;
        private readonly IClock _clock;

        public GameController(FantasyCriticUserManager userManager, InterLeagueService interLeagueService, IClock clock)
            : base(userManager)
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

            var numberOutstandingCorrections = await _interLeagueService.GetNumberOutstandingCorrections(masterGame.Value);

            var currentDate = _clock.GetToday();
            var viewModel = new MasterGameViewModel(masterGame.Value, currentDate, numberOutstandingCorrections: numberOutstandingCorrections);
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

            var currentDate = _clock.GetToday();
            var viewModel = new MasterGameYearViewModel(masterGame.Value, currentDate);
            return viewModel;
        }

        [HttpGet("{year}")]
        public async Task<ActionResult<List<MasterGameYearViewModel>>> MasterGameYear(int year)
        {
            IReadOnlyList<MasterGameYear> masterGames = await _interLeagueService.GetMasterGameYears(year);
            var relevantGames = masterGames.Where(x => !x.MasterGame.ReleaseDate.HasValue || x.MasterGame.ReleaseDate.Value.Year >= year);

            var supportedYears = await GetSupportedYears();
            var finishedYears = supportedYears.Where(x => x.Finished);
            bool thisYearIsFinished = finishedYears.Any(x => x.Year == year);
            if (thisYearIsFinished)
            {
                var chosenGames = await _interLeagueService.GetAllSelectedMasterGameIDsForYear(year);
                relevantGames = masterGames.Where(x => chosenGames.Contains(x.MasterGame.MasterGameID));
            }

            var currentDate = _clock.GetToday();
            List<MasterGameYearViewModel> viewModels = relevantGames.Select(x => new MasterGameYearViewModel(x, currentDate)).ToList();

            return viewModels;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateMasterGameRequest([FromBody] MasterGameRequestRequest request)
        {
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            MasterGameRequest domainRequest = request.ToDomain(currentUser, _clock.GetCurrentInstant());

            await _interLeagueService.CreateMasterGameRequest(domainRequest);
            return Ok();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateMasterGameChangeRequest([FromBody] MasterGameChangeRequestRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

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
        [Authorize]
        public async Task<IActionResult> DeleteMasterGameRequest([FromBody] MasterGameRequestDeletionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            Maybe<MasterGameRequest> maybeRequest = await _interLeagueService.GetMasterGameRequest(request.RequestID);
            if (maybeRequest.HasNoValue)
            {
                return BadRequest("That request does not exist.");
            }

            var domainRequest = maybeRequest.Value;
            if (domainRequest.User.Id != currentUser.Id)
            {
                return Forbid();
            }

            await _interLeagueService.DeleteMasterGameRequest(domainRequest);
            return Ok();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteMasterGameChangeRequest([FromBody] MasterGameChangeRequestDeletionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            Maybe<MasterGameChangeRequest> maybeRequest = await _interLeagueService.GetMasterGameChangeRequest(request.RequestID);
            if (maybeRequest.HasNoValue)
            {
                return BadRequest("That request does not exist.");
            }

            var domainRequest = maybeRequest.Value;
            if (domainRequest.User.Id != currentUser.Id)
            {
                return Forbid();
            }

            await _interLeagueService.DeleteMasterGameChangeRequest(domainRequest);
            return Ok();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DismissMasterGameRequest([FromBody] MasterGameRequestDismissRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            Maybe<MasterGameRequest> maybeRequest = await _interLeagueService.GetMasterGameRequest(request.RequestID);
            if (maybeRequest.HasNoValue)
            {
                return BadRequest("That request does not exist.");
            }

            var domainRequest = maybeRequest.Value;
            if (domainRequest.User.Id != currentUser.Id)
            {
                return Forbid();
            }

            await _interLeagueService.DismissMasterGameRequest(domainRequest);
            return Ok();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DismissMasterGameChangeRequest([FromBody] MasterGameChangeRequestDismissRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            Maybe<MasterGameChangeRequest> maybeRequest = await _interLeagueService.GetMasterGameChangeRequest(request.RequestID);
            if (maybeRequest.HasNoValue)
            {
                return BadRequest("That request does not exist.");
            }

            var domainRequest = maybeRequest.Value;
            if (domainRequest.User.Id != currentUser.Id)
            {
                return Forbid();
            }

            await _interLeagueService.DismissMasterGameChangeRequest(domainRequest);
            return Ok();
        }

        [Authorize]
        public async Task<ActionResult<List<MasterGameRequestViewModel>>> MyMasterGameRequests()
        {
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            IReadOnlyList<MasterGameRequest> requests = await _interLeagueService.GetMasterGameRequestsForUser(currentUser);

            var currentDate = _clock.GetToday();
            var viewModels = requests.Select(x => new MasterGameRequestViewModel(x, currentDate)).ToList();
            return viewModels;
        }

        [Authorize]
        public async Task<ActionResult<List<MasterGameChangeRequestViewModel>>> MyMasterGameChangeRequests()
        {
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            IReadOnlyList<MasterGameChangeRequest> requests = await _interLeagueService.GetMasterGameChangeRequestsForUser(currentUser);

            var currentDate = _clock.GetToday();
            var viewModels = requests.Select(x => new MasterGameChangeRequestViewModel(x, currentDate)).ToList();
            return viewModels;
        }

        public async Task<ActionResult<List<SupportedYearViewModel>>> SupportedYears()
        {
            var supportedYears = await GetSupportedYears();
            return supportedYears.Select(x => new SupportedYearViewModel(x)).ToList();
        }

        private async Task<IReadOnlyList<SupportedYear>> GetSupportedYears()
        {
            IReadOnlyList<SupportedYear> supportedYears = await _interLeagueService.GetSupportedYears();
            var years = supportedYears.Where(x => x.Year > 2017).OrderByDescending(x => x);
            return years.ToList();
        }

        public async Task<ActionResult<List<MasterGameTagViewModel>>> GetMasterGameTags()
        {
            var domains = await _interLeagueService.GetMasterGameTags();
            var vms = domains.Select(x => new MasterGameTagViewModel(x)).ToList();
            return vms;
        }
    }
}
