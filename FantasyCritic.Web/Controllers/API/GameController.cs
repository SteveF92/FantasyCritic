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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class GameController : Controller
    {
        private readonly FantasyCriticUserManager _userManager;
        private readonly FantasyCriticService _fantasyCriticService;
        private readonly IClock _clock;
        private static readonly int MaxDistance = 10;
        private static readonly int MaxDistanceGames = 5;

        public GameController(FantasyCriticUserManager userManager, FantasyCriticService fantasyCriticService, IClock clock)
        {
            _userManager = userManager;
            _fantasyCriticService = fantasyCriticService;
            _clock = clock;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MasterGameViewModel>> MasterGame(Guid id)
        {
            var masterGame = await _fantasyCriticService.GetMasterGame(id);
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
            Maybe<MasterGameYear> masterGame = await _fantasyCriticService.GetMasterGameYear(id, year);
            if (masterGame.HasNoValue)
            {
                return NotFound();
            }

            var viewModel = new MasterGameYearViewModel(masterGame.Value, _clock);
            return viewModel;
        }

        public async Task<ActionResult<List<MasterGameViewModel>>> MasterGame(string gameName)
        {
            IReadOnlyList<MasterGame> masterGames = await _fantasyCriticService.GetMasterGames();
            IEnumerable<MasterGame> matchingMasterGames = new List<MasterGame>();
            if (!string.IsNullOrWhiteSpace(gameName))
            {
                gameName = gameName.ToLower();
                var distances = masterGames
                    .Select(x =>
                        new Tuple<MasterGame, int>(x, Levenshtein.CalculateLevenshteinDistance(gameName, x.GameName)));

                var lowDistance = distances.Where(x => x.Item2 < MaxDistance).OrderBy(x => x.Item2).Select(x => x.Item1).Take(MaxDistanceGames);

                matchingMasterGames = masterGames
                    .Where(x => x.GameName.ToLower().Contains(gameName))
                    .Concat(lowDistance).Distinct();
            }
            List<MasterGameViewModel> viewModels = matchingMasterGames.Select(x => new MasterGameViewModel(x, _clock)).ToList();

            return viewModels;
        }

        public async Task<ActionResult<List<MasterGameYearViewModel>>> MasterGameYear(string gameName, int year)
        {
            IReadOnlyList<MasterGameYear> masterGames = await _fantasyCriticService.GetMasterGameYears(year);
            IEnumerable<MasterGameYear> matchingMasterGames = new List<MasterGameYear>();
            if (!string.IsNullOrWhiteSpace(gameName))
            {
                gameName = gameName.ToLower();
                var distances = masterGames
                    .Select(x =>
                        new Tuple<MasterGameYear, int>(x, Levenshtein.CalculateLevenshteinDistance(gameName, x.MasterGame.GameName)));

                var lowDistance = distances.Where(x => x.Item2 < MaxDistance).OrderBy(x => x.Item2).Select(x => x.Item1).Take(MaxDistanceGames);

                matchingMasterGames = masterGames
                    .Where(x => x.MasterGame.GameName.ToLower().Contains(gameName))
                    .Concat(lowDistance).Distinct();
            }
            List<MasterGameYearViewModel> viewModels = matchingMasterGames.Select(x => new MasterGameYearViewModel(x, _clock)).ToList();

            return viewModels;
        }
    }
}
