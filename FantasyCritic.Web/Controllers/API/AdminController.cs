using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.Web.Extensions;
using FantasyCritic.Web.Models;
using FantasyCritic.Web.Models.Requests;
using FantasyCritic.Web.Models.Requests.Admin;
using FantasyCritic.Web.Models.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoreLinq;
using NodaTime;

namespace FantasyCritic.Web.Controllers.API
{
    [Route("api/[controller]/[action]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;
        private readonly FantasyCriticService _fantasyCriticService;
        private readonly InterLeagueService _interLeagueService;
        private readonly IClock _clock;
        private readonly ILogger _logger;
        private readonly GameAcquisitionService _gameAcquisitionService;
        private readonly FantasyCriticUserManager _userManager;
        private readonly IEmailSender _emailSender;

        public AdminController(AdminService adminService, FantasyCriticService fantasyCriticService, IOpenCriticService openCriticService,
            IClock clock, InterLeagueService interLeagueService, ILogger<AdminController> logger, GameAcquisitionService gameAcquisitionService,
            FantasyCriticUserManager userManager, IEmailSender emailSender)
        {
            _adminService = adminService;
            _fantasyCriticService = fantasyCriticService;
            _clock = clock;
            _interLeagueService = interLeagueService;
            _logger = logger;
            _gameAcquisitionService = gameAcquisitionService;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMasterGame([FromBody] CreateMasterGameRequest viewModel)
        {
            Instant instant = _clock.GetCurrentInstant();

            var possibleTags = await _interLeagueService.GetMasterGameTags();
            IReadOnlyList<MasterGameTag> tags = possibleTags
                .Where(x => viewModel.GetRequestedTags().Contains(x.Name))
                .ToList();

            if (!tags.Any())
            {
                tags = new List<MasterGameTag>()
                {
                    possibleTags.Single(x => x.Name == "NewGame")
                };
            }

            MasterGame masterGame = viewModel.ToDomain(instant, tags);
            await _interLeagueService.CreateMasterGame(masterGame);
            var currentDate = _clock.GetToday();
            var vm = new MasterGameViewModel(masterGame, currentDate);

            _logger.LogInformation($"Created master game: {masterGame.MasterGameID}");
            return CreatedAtAction("MasterGame", "Game", new {id = masterGame.MasterGameID}, vm);
        }

        [HttpPost]
        public async Task<IActionResult> EditMasterGame([FromBody] EditMasterGameRequest viewModel)
        {
            Instant instant = _clock.GetCurrentInstant();

            var possibleTags = await _interLeagueService.GetMasterGameTags();
            IReadOnlyList<MasterGameTag> tags = possibleTags
                .Where(x => viewModel.GetRequestedTags().Contains(x.Name))
                .ToList();

            MasterGame masterGame = viewModel.ToDomain(instant, tags);
            await _interLeagueService.EditMasterGame(masterGame);
            var currentDate = _clock.GetToday();
            var vm = new MasterGameViewModel(masterGame, currentDate);

            _logger.LogInformation($"Edited master game: {masterGame.MasterGameID}");
            return CreatedAtAction("MasterGame", "Game", new { id = masterGame.MasterGameID }, vm);
        }

        [HttpPost]
        public async Task<IActionResult> CompleteMasterGameRequest([FromBody] CompleteMasterGameRequestRequest request)
        {
            Maybe<MasterGameRequest> maybeRequest = await _interLeagueService.GetMasterGameRequest(request.RequestID);
            if (maybeRequest.HasNoValue)
            {
                return BadRequest("That request does not exist.");
            }

            Maybe<MasterGame> masterGame = Maybe<MasterGame>.None;
            if (request.MasterGameID.HasValue)
            {
                masterGame = await _interLeagueService.GetMasterGame(request.MasterGameID.Value);
                if (masterGame.HasNoValue)
                {
                    return BadRequest("Bad master game");
                }
            }

            Instant instant = _clock.GetCurrentInstant();
            await _interLeagueService.CompleteMasterGameRequest(maybeRequest.Value, instant, request.ResponseNote, masterGame);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CompleteMasterGameChangeRequest([FromBody] CompleteMasterGameChangeRequestRequest request)
        {
            Maybe<MasterGameChangeRequest> maybeRequest = await _interLeagueService.GetMasterGameChangeRequest(request.RequestID);
            if (maybeRequest.HasNoValue)
            {
                return BadRequest("That request does not exist.");
            }

            Instant instant = _clock.GetCurrentInstant();
            await _interLeagueService.CompleteMasterGameChangeRequest(maybeRequest.Value, instant, request.ResponseNote);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> LinkGameToOpenCritic([FromBody] LinkGameToOpenCriticRequest request)
        {
            Maybe<MasterGame> masterGame = await _interLeagueService.GetMasterGame(request.MasterGameID);
            if (masterGame.HasNoValue)
            {
                return BadRequest("Bad master game");
            }

            await _interLeagueService.LinkToOpenCritic(masterGame.Value, request.OpenCriticID);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> MergeMasterGame([FromBody] MergeGameRequest request)
        {
            Maybe<MasterGame> removeMasterGame = await _interLeagueService.GetMasterGame(request.RemoveMasterGameID);
            Maybe<MasterGame> mergeIntoMasterGame = await _interLeagueService.GetMasterGame(request.MergeIntoMasterGameID);
            if (removeMasterGame.HasNoValue || mergeIntoMasterGame.HasNoValue)
            {
                return BadRequest("Bad master game");
            }

            await _interLeagueService.MergeMasterGame(removeMasterGame.Value, mergeIntoMasterGame.Value);

            return Ok();
        }

        public async Task<ActionResult<List<MasterGameRequestViewModel>>> ActiveMasterGameRequests()
        {
            IReadOnlyList<MasterGameRequest> requests = await _interLeagueService.GetAllMasterGameRequests();

            var currentDate = _clock.GetToday();
            var viewModels = requests.Select(x => new MasterGameRequestViewModel(x, currentDate)).OrderBy(x => x.GameName).ToList();
            return viewModels;
        }

        public async Task<ActionResult<List<MasterGameChangeRequestViewModel>>> ActiveMasterGameChangeRequests()
        {
            IReadOnlyList<MasterGameChangeRequest> requests = await _interLeagueService.GetAllMasterGameChangeRequests();

            var currentDate = _clock.GetToday();
            var viewModels = requests.Select(x => new MasterGameChangeRequestViewModel(x, currentDate)).OrderBy(x => x.MasterGame.GameName).ToList();
            return viewModels;
        }

        public async Task<ActionResult<MasterGameRequestViewModel>> GetMasterGameRequest(Guid requestID)
        {
            IReadOnlyList<MasterGameRequest> requests = await _interLeagueService.GetAllMasterGameRequests();

            var request = requests.SingleOrDefault(x => x.RequestID == requestID);
            var currentDate = _clock.GetToday();
            var vm = new MasterGameRequestViewModel(request, currentDate);
            return vm;
        }

        public async Task<ActionResult<MasterGameChangeRequestViewModel>> GetMasterGameChangeRequest(Guid changeRequestID)
        {
            IReadOnlyList<MasterGameChangeRequest> requests = await _interLeagueService.GetAllMasterGameChangeRequests();

            var currentDate = _clock.GetToday();
            var request = requests.SingleOrDefault(x => x.RequestID == changeRequestID);
            var vm = new MasterGameChangeRequestViewModel(request, currentDate);
            return vm;
        }

        [HttpPost]
        public async Task<IActionResult> FullDataRefresh()
        {
            await _adminService.FullDataRefresh();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RefreshCriticInfo()
        {
            await _adminService.RefreshCriticInfo();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateFantasyPoints()
        {
            await _adminService.UpdateFantasyPoints();
            return Ok();
        }

        public async Task<ActionResult<ActionedGameSet>> GetCurrentActionedGames()
        {
            var supportedYears = await _interLeagueService.GetSupportedYears();
            SystemWideValues systemWideValues = await _interLeagueService.GetSystemWideValues();
            var currentYear = supportedYears.First(x => !x.Finished && x.OpenForPlay);

            var actionResults = await _fantasyCriticService.GetActionProcessingDryRun(systemWideValues, currentYear.Year);
            IEnumerable<LeagueAction> failingActions = actionResults.LeagueActions.Where(x => x.IsFailed);
            var failingActionGames = failingActions.Select(x => x.MasterGameName).Distinct();

            List<MasterGameViewModel> pickupGames = new List<MasterGameViewModel>();
            List<MasterGameViewModel> dropGames = new List<MasterGameViewModel>();
            var currentDate = _clock.GetToday();
            foreach (var supportedYear in supportedYears)
            {
                var allBids = await _gameAcquisitionService.GetActiveAcquisitionBids(supportedYear);
                var distinctBids = allBids.SelectMany(x => x.Value).DistinctBy(x => x.MasterGame);
                var bidVMs = distinctBids.Select(x => new MasterGameViewModel(x.MasterGame, currentDate, error: failingActionGames.Contains(x.MasterGame.GameName)));
                pickupGames.AddRange(bidVMs);

                var allDrops = await _gameAcquisitionService.GetActiveDropRequests(supportedYear);
                var distinctDrops = allDrops.SelectMany(x => x.Value).DistinctBy(x => x.MasterGame);
                var dropVMs = distinctDrops.Select(x => new MasterGameViewModel(x.MasterGame, currentDate, error: failingActionGames.Contains(x.MasterGame.GameName)));
                dropGames.AddRange(dropVMs);
            }

            pickupGames = pickupGames.OrderByDescending(x => x.Error).ThenBy(x => x.MaximumReleaseDate).ToList();
            dropGames = dropGames.OrderByDescending(x => x.Error).ThenBy(x => x.MaximumReleaseDate).ToList();
            ActionedGameSet fullSet = new ActionedGameSet(pickupGames, dropGames);
            return Ok(fullSet);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessActions() 
        {
            var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
            if (!systemWideSettings.ActionProcessingMode)
            {
                return BadRequest("Turn on action processing mode first.");
            }

            var today = _clock.GetCurrentInstant().ToEasternDate();
            if (today.DayOfWeek != IsoDayOfWeek.Saturday && today.DayOfWeek != IsoDayOfWeek.Sunday)
            {
                return BadRequest($"You probably didn't mean to process pickups on a {today.DayOfWeek}");
            }

            SystemWideValues systemWideValues = await _interLeagueService.GetSystemWideValues();
            var supportedYears = await _interLeagueService.GetSupportedYears();
            foreach (var supportedYear in supportedYears)
            {
                if (supportedYear.Finished || !supportedYear.OpenForPlay)
                {
                    continue;
                }

                await _fantasyCriticService.ProcessActions(systemWideValues, supportedYear.Year);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLeague([FromBody] DeleteLeagueRequest request)
        {
            Maybe<League> league = await _fantasyCriticService.GetLeagueByID(request.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            if (!league.Value.TestLeague)
            {
                return BadRequest();
            }

            await _fantasyCriticService.DeleteLeague(league.Value);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> TurnOnActionProcessingMode()
        {
            await _interLeagueService.SetActionProcessingMode(true);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> TurnOffActionProcessingMode()
        {
            await _interLeagueService.SetActionProcessingMode(false);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RefreshCaches()
        {
            await _adminService.RefreshCaches();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SnapshotDatabase()
        {
            await _adminService.SnapshotDatabase();
            return Ok();
        }

        public async Task<IActionResult> GetRecentDatabaseSnapshots()
        {
            IReadOnlyList<DatabaseSnapshotInfo> snaps = await _adminService.GetRecentDatabaseSnapshots();

            var vms = snaps.Select(x => new DatabaseSnapshotInfoViewModel(x));
            return Ok(vms);
        }

        [HttpGet]
        public ActionResult<DateParseResponse> ParseEstimatedDate(DateParseRequest request)
        {
            var dates = TimeFunctions.ParseEstimatedReleaseDate(request.EstimatedReleaseDate, _clock);
            var response = new DateParseResponse(dates.minimumReleaseDate, dates.maximumReleaseDate);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] AdminResendConfirmationEmail request)
        {
            var user = await _userManager.FindByIdAsync(request.UserID.ToString());
            if (user is null)
            {
                return BadRequest();
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string baseURL = $"{Request.Scheme}://{Request.Host.Value}";
            await _emailSender.SendConfirmationEmail(user, code, baseURL);

            return Ok();
        }
    }
}
