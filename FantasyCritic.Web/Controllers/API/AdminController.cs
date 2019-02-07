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
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Services;
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly FantasyCriticUserManager _userManager;
        private readonly FantasyCriticService _fantasyCriticService;
        private readonly IOpenCriticService _openCriticService;
        private readonly IClock _clock;

        public AdminController(FantasyCriticUserManager userManager, FantasyCriticService fantasyCriticService, IOpenCriticService openCriticService, IClock clock)
        {
            _userManager = userManager;
            _fantasyCriticService = fantasyCriticService;
            _openCriticService = openCriticService;
            _clock = clock;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMasterGame([FromBody] CreateMasterGameRequest viewModel)
        {
            EligibilityLevel eligibilityLevel = await _fantasyCriticService.GetEligibilityLevel(viewModel.EligibilityLevel);
            Instant instant = _clock.GetCurrentInstant();
            MasterGame masterGame = viewModel.ToDomain(eligibilityLevel, instant);
            await _fantasyCriticService.CreateMasterGame(masterGame);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> BatchCreateMasterGame([FromBody] List<MinimalCreateMasterGameRequest> request)
        {
            foreach (var singleGame in request)
            {
                EligibilityLevel eligibilityLevel = await _fantasyCriticService.GetEligibilityLevel(0);
                Instant instant = _clock.GetCurrentInstant();
                MasterGame masterGame = singleGame.ToDomain(eligibilityLevel, instant);
                await _fantasyCriticService.CreateMasterGame(masterGame);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RefreshCriticInfo()
        {
            var supportedYears = await _fantasyCriticService.GetSupportedYears();
            var masterGames = await _fantasyCriticService.GetMasterGames();
            foreach (var masterGame in masterGames)
            {
                if (!masterGame.OpenCriticID.HasValue)
                {
                    continue;
                }

                if (masterGame.DoNotRefresh)
                {
                    continue;
                }

                if (masterGame.IsReleased(_clock) && masterGame.ReleaseDate.HasValue)
                {
                    var year = masterGame.ReleaseDate.Value.Year;
                    var supportedYear = supportedYears.Single(x => x.Year == year);
                    if (supportedYear.Finished)
                    {
                        continue;
                    }
                }

                var openCriticGame = await _openCriticService.GetOpenCriticGame(masterGame.OpenCriticID.Value);
                if (openCriticGame.HasValue)
                {
                    await _fantasyCriticService.UpdateCriticStats(masterGame, openCriticGame.Value);
                }

                foreach (var subGame in masterGame.SubGames)
                {
                    if (!subGame.OpenCriticID.HasValue)
                    {
                        continue;
                    }

                    var subGameOpenCriticGame = await _openCriticService.GetOpenCriticGame(subGame.OpenCriticID.Value);

                    if (subGameOpenCriticGame.HasValue)
                    {
                        await _fantasyCriticService.UpdateCriticStats(subGame, subGameOpenCriticGame.Value);
                    }
                }
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> GetOpenCriticGames([FromBody] OpenCriticSearchRequest request)
        {
            int id = request.MinimumOpenCriticID;
            var masterGames = await _fantasyCriticService.GetMasterGames();
            while (true)
            {
                var openCriticGame = await _openCriticService.GetOpenCriticGame(id);
                if (openCriticGame.HasNoValue)
                {
                    return Ok();
                }

                if (masterGames.Any(x => x.OpenCriticID.HasValue && x.OpenCriticID.Value == id))
                {
                    id++;
                    continue;
                }

                EligibilityLevel eligibilityLevel = await _fantasyCriticService.GetEligibilityLevel(0);
                int minimumReleaseYear = 2018;
                if (openCriticGame.Value.ReleaseDate.HasValue)
                {
                    minimumReleaseYear = openCriticGame.Value.ReleaseDate.Value.Year;
                }

                MasterGame masterGame = new MasterGame(Guid.NewGuid(), openCriticGame.Value.Name, openCriticGame.Value.ReleaseDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    openCriticGame.Value.ReleaseDate, id, openCriticGame.Value.Score, minimumReleaseYear, eligibilityLevel, false, false, "", null, false, _clock.GetCurrentInstant());
                await _fantasyCriticService.CreateMasterGame(masterGame);

                id++;
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateFantasyPoints([FromBody] UpdateFantasyPointsRequest request)
        {
            await _fantasyCriticService.UpdateFantasyPoints(request.Year);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPickups([FromBody] ProcessPickupsRequest request)
        {
            await _fantasyCriticService.ProcessPickups(request.Year);

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
    }
}
