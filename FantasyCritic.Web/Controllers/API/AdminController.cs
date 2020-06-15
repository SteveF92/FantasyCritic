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
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.Utilities;
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
    public class AdminController : Controller
    {
        private readonly AdminService _adminService;
        private readonly FantasyCriticService _fantasyCriticService;
        private readonly InterLeagueService _interLeagueService;
        private readonly IOpenCriticService _openCriticService;
        private readonly IClock _clock;
        private readonly ILogger _logger;
        private readonly GameAcquisitionService _gameAcquisitionService;

        public AdminController(AdminService adminService, FantasyCriticService fantasyCriticService, IOpenCriticService openCriticService,
            IClock clock, InterLeagueService interLeagueService, ILogger<AdminController> logger, GameAcquisitionService gameAcquisitionService)
        {
            _adminService = adminService;
            _fantasyCriticService = fantasyCriticService;
            _openCriticService = openCriticService;
            _clock = clock;
            _interLeagueService = interLeagueService;
            _logger = logger;
            _gameAcquisitionService = gameAcquisitionService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMasterGame([FromBody] CreateMasterGameRequest viewModel)
        {
            EligibilityLevel eligibilityLevel = await _interLeagueService.GetEligibilityLevel(viewModel.EligibilityLevel);
            Instant instant = _clock.GetCurrentInstant();
            LocalDate tomorrow = instant.ToEasternDate().PlusDays(1);

            var currentYear = await _interLeagueService.GetCurrentYear();
            MasterGame masterGame = viewModel.ToDomain(eligibilityLevel, instant, tomorrow);
            await _interLeagueService.CreateMasterGame(masterGame);
            var vm = new MasterGameViewModel(masterGame, _clock);

            _logger.LogInformation($"Created master game: {masterGame.MasterGameID}");
            return CreatedAtAction("MasterGame", "Game", new {id = masterGame.MasterGameID}, vm);
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

        public async Task<ActionResult<List<MasterGameRequestViewModel>>> ActiveMasterGameRequests()
        {
            IReadOnlyList<MasterGameRequest> requests = await _interLeagueService.GetAllMasterGameRequests();

            var viewModels = requests.Select(x => new MasterGameRequestViewModel(x, _clock)).OrderBy(x => x.GameName).ToList();
            return viewModels;
        }

        public async Task<ActionResult<List<MasterGameChangeRequestViewModel>>> ActiveMasterGameChangeRequests()
        {
            IReadOnlyList<MasterGameChangeRequest> requests = await _interLeagueService.GetAllMasterGameChangeRequests();

            var viewModels = requests.Select(x => new MasterGameChangeRequestViewModel(x, _clock)).OrderBy(x => x.MasterGame.GameName).ToList();
            return viewModels;
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

            var bidResults = await _fantasyCriticService.GetBidProcessingDryRun(systemWideValues, currentYear.Year);
            IEnumerable<LeagueAction> failingBids = bidResults.LeagueActions.Where(x => x.IsFailed);
            var failingBidGames = failingBids.Select(x => x.MasterGameName).Distinct();

            var dropResults = await _fantasyCriticService.GetDropProcessingDryRun(currentYear.Year);
            IEnumerable<LeagueAction> failingDrops = dropResults.LeagueActions.Where(x => x.IsFailed);
            var failingDropGames = failingDrops.Select(x => x.MasterGameName).Distinct();

            List<MasterGameViewModel> pickupGames = new List<MasterGameViewModel>();
            List<MasterGameViewModel> bidGames = new List<MasterGameViewModel>();
            foreach (var supportedYear in supportedYears)
            {
                var allBids = await _gameAcquisitionService.GetActiveAcquistitionBids(supportedYear);
                var distinctBids = allBids.SelectMany(x => x.Value).DistinctBy(x => x.MasterGame);
                var bidVMs = distinctBids.Select(x => new MasterGameViewModel(x.MasterGame, _clock, failingBidGames.Contains(x.MasterGame.GameName)));
                pickupGames.AddRange(bidVMs);

                var allDrops = await _gameAcquisitionService.GetActiveDropRequests(supportedYear);
                var distinctDrops = allDrops.SelectMany(x => x.Value).DistinctBy(x => x.MasterGame);
                var dropVMs = distinctDrops.Select(x => new MasterGameViewModel(x.MasterGame, _clock, failingDropGames.Contains(x.MasterGame.GameName)));
                bidGames.AddRange(dropVMs);
            }

            pickupGames = pickupGames.OrderByDescending(x => x.Error).ThenBy(x => x.SortableEstimatedReleaseDate).ToList();
            bidGames = bidGames.OrderByDescending(x => x.Error).ThenBy(x => x.SortableEstimatedReleaseDate).ToList();
            ActionedGameSet fullSet = new ActionedGameSet(pickupGames, bidGames);
            return Ok(fullSet);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPickups()
        {
            var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
            if (!systemWideSettings.BidProcessingMode)
            {
                return BadRequest("Turn on bid processing mode first.");
            }

            var today = _clock.GetCurrentInstant().ToEasternDate();
            if (today.DayOfWeek != IsoDayOfWeek.Monday)
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

                await _fantasyCriticService.ProcessPickups(systemWideValues, supportedYear.Year);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessDropRequests()
        {
            var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
            if (!systemWideSettings.BidProcessingMode)
            {
                return BadRequest("Turn on bid processing mode first.");
            }

            var today = _clock.GetCurrentInstant().ToEasternDate();
            if (today.DayOfWeek != IsoDayOfWeek.Sunday)
            {
                return BadRequest($"You probably didn't mean to process pickups on a {today.DayOfWeek}");
            }

            var supportedYears = await _interLeagueService.GetSupportedYears();
            foreach (var supportedYear in supportedYears)
            {
                if (supportedYear.Finished || !supportedYear.OpenForPlay)
                {
                    continue;
                }

                await _fantasyCriticService.ProcessDrops(supportedYear.Year);
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
        public async Task<IActionResult> TurnOnBidProcessingMode()
        {
            await _interLeagueService.SetBidProcessingMode(true);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> TurnOffBidProcessingMode()
        {
            await _interLeagueService.SetBidProcessingMode(false);
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
    }
}
