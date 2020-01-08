using FantasyCritic.Lib.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Royale;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.Web.Models.Requests.Royale;
using FantasyCritic.Web.Models.Responses;
using FantasyCritic.Web.Models.Responses.Royale;
using FantasyCritic.Web.Models.RoundTrip;
using MoreLinq;

namespace FantasyCritic.Web.Controllers.API
{
    [Route("api/[controller]/[action]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RoyaleController : Controller
    {
        private readonly IClock _clock;
        private readonly ILogger<RoyaleController> _logger;
        private readonly FantasyCriticUserManager _userManager;
        private readonly RoyaleService _royaleService;
        private readonly InterLeagueService _interLeagueService;

        public RoyaleController(IClock clock, ILogger<RoyaleController> logger, FantasyCriticUserManager userManager, RoyaleService royaleService, InterLeagueService interLeagueService)
        {
            _clock = clock;
            _logger = logger;
            _userManager = userManager;
            _royaleService = royaleService;
            _interLeagueService = interLeagueService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> RoyaleQuarters()
        {
            IReadOnlyList<RoyaleYearQuarter> supportedQuarters = await _royaleService.GetYearQuarters();
            var viewModels = supportedQuarters.Select(x => new RoyaleYearQuarterViewModel(x));
            return Ok(viewModels);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ActiveRoyaleQuarter()
        {
            var activeQuarter = await _royaleService.GetActiveYearQuarter();
            var viewModel = new RoyaleYearQuarterViewModel(activeQuarter);
            return Ok(viewModel);
        }

        [AllowAnonymous]
        [HttpGet("{year}/{quarter}")]
        public async Task<IActionResult> RoyaleQuarter(int year, int quarter)
        {
            var requestedQuarter = await _royaleService.GetYearQuarter(year, quarter);
            if (requestedQuarter.HasNoValue)
            {
                return NotFound();
            }

            var viewModel = new RoyaleYearQuarterViewModel(requestedQuarter.Value);
            return Ok(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoyalePublisher([FromBody] CreateRoyalePublisherRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (currentUser is null)
            {
                return BadRequest();
            }

            IReadOnlyList<RoyaleYearQuarter> supportedQuarters = await _royaleService.GetYearQuarters();
            var selectedQuarter = supportedQuarters.Single(x => x.YearQuarter.Year == request.Year && x.YearQuarter.Quarter == request.Quarter);
            if (!selectedQuarter.OpenForPlay || selectedQuarter.Finished)
            {
                return BadRequest();
            }

            var existingPublisher = await _royaleService.GetPublisher(selectedQuarter, currentUser);
            if (existingPublisher.HasValue)
            {
                return BadRequest();
            }

            RoyalePublisher publisher = await _royaleService.CreatePublisher(selectedQuarter, currentUser, request.PublisherName);
            return Ok(publisher.PublisherID);
        }

        [AllowAnonymous]
        [HttpGet("{year}/{quarter}")]
        public async Task<IActionResult> GetUserRoyalePublisher(int year, int quarter)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            var yearQuarter = await _royaleService.GetYearQuarter(year, quarter);
            if (yearQuarter.HasNoValue)
            {
                return BadRequest();
            }

            Maybe<RoyalePublisher> publisher = await _royaleService.GetPublisher(yearQuarter.Value, currentUser);
            if (publisher.HasNoValue)
            {
                return NotFound();
            }

            var viewModel = new RoyalePublisherViewModel(publisher.Value, _clock, null);
            return Ok(viewModel);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoyalePublisher(Guid id)
        {
            Maybe<RoyalePublisher> publisher = await _royaleService.GetPublisher(id);
            if (publisher.HasNoValue)
            {
                return NotFound();
            }

            var viewModel = new RoyalePublisherViewModel(publisher.Value, _clock, null);
            return Ok(viewModel);
        }

        [AllowAnonymous]
        [HttpGet("{year}/{quarter}")]
        public async Task<IActionResult> RoyaleStandings(int year, int quarter)
        {
            IReadOnlyList<RoyalePublisher> publishers = await _royaleService.GetAllPublishers(year, quarter);
            var publishersToShow = publishers.Where(x => x.PublisherGames.Any()).OrderByDescending(x => x.GetTotalFantasyPoints());
            int ranking = 1;
            List<RoyalePublisherViewModel> viewModels = new List<RoyalePublisherViewModel>();
            foreach (var publisher in publishersToShow)
            {
                int? thisRanking = null;
                if (publisher.GetTotalFantasyPoints() > 0)
                {
                    thisRanking = ranking;
                    ranking++;
                }

                var vm = new RoyalePublisherViewModel(publisher, _clock, thisRanking);
                viewModels.Add(vm);
            }

            return Ok(viewModels);
        }

        [HttpPost]
        public async Task<IActionResult> PurchaseGame([FromBody] PurchaseRoyaleGameRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (currentUser is null)
            {
                return BadRequest();
            }

            Maybe<RoyalePublisher> publisher = await _royaleService.GetPublisher(request.PublisherID);
            if (publisher.HasNoValue)
            {
                return NotFound();
            }

            if (!publisher.Value.User.Equals(currentUser))
            {
                return Forbid();
            }

            var masterGame = await _interLeagueService.GetMasterGameYear(request.MasterGameID, publisher.Value.YearQuarter.YearQuarter.Year);
            if (masterGame.HasNoValue)
            {
                return NotFound();
            }

            var purchaseResult = await _royaleService.PurchaseGame(publisher.Value, masterGame.Value);
            var viewModel = new PlayerClaimResultViewModel(purchaseResult);
            return Ok(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SellGame([FromBody] SellRoyaleGameRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (currentUser is null)
            {
                return BadRequest();
            }

            Maybe<RoyalePublisher> publisher = await _royaleService.GetPublisher(request.PublisherID);
            if (publisher.HasNoValue)
            {
                return NotFound();
            }

            if (!publisher.Value.User.Equals(currentUser))
            {
                return Forbid();
            }

            var publisherGame = publisher.Value.PublisherGames.SingleOrDefault(x => x.MasterGame.MasterGame.MasterGameID == request.MasterGameID);
            if (publisherGame is null)
            {
                return BadRequest();
            }

            var sellResult = await _royaleService.SellGame(publisher.Value, publisherGame);
            if (sellResult.IsFailure)
            {
                return BadRequest(sellResult.Error);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SetAdvertisingMoney([FromBody] SetAdvertisingMoneyRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (currentUser is null)
            {
                return BadRequest();
            }

            Maybe<RoyalePublisher> publisher = await _royaleService.GetPublisher(request.PublisherID);
            if (publisher.HasNoValue)
            {
                return NotFound();
            }

            if (!publisher.Value.User.Equals(currentUser))
            {
                return Forbid();
            }

            var publisherGame = publisher.Value.PublisherGames.SingleOrDefault(x => x.MasterGame.MasterGame.MasterGameID == request.MasterGameID);
            if (publisherGame is null)
            {
                return BadRequest();
            }

            var setAdvertisingMoneyResult = await _royaleService.SetAdvertisingMoney(publisher.Value, publisherGame, request.AdvertisingMoney);
            if (setAdvertisingMoneyResult.IsFailure)
            {
                return BadRequest(setAdvertisingMoneyResult.Error);
            }

            return Ok();
        }

        public async Task<ActionResult<List<PossibleRoyaleMasterGameViewModel>>> PossibleMasterGames(string gameName, Guid publisherID)
        {
            Maybe<RoyalePublisher> publisher = await _royaleService.GetPublisher(publisherID);
            if (publisher.HasNoValue)
            {
                return NotFound();
            }

            var yearQuarter = await _royaleService.GetYearQuarter(publisher.Value.YearQuarter.YearQuarter.Year, publisher.Value.YearQuarter.YearQuarter.Quarter);
            if (yearQuarter.HasNoValue)
            {
                return BadRequest();
            }

            var masterGames = await _royaleService.GetMasterGamesForYearQuarter(yearQuarter.Value.YearQuarter);
            if (!string.IsNullOrWhiteSpace(gameName))
            {
                masterGames = MasterGameSearching.SearchMasterGameYears(gameName, masterGames);
            }
            else
            {
                masterGames = masterGames
                    .Where(x => x.WillReleaseInQuarter(yearQuarter.Value.YearQuarter))
                    .Where(x => !x.MasterGame.IsReleased(_clock))
                    .Where(x => !EligibilitySettings.GetRoyaleEligibilitySettings().GameIsEligible(x.MasterGame).Any())
                    .Take(100)
                    .ToList();
            }

            var viewModels = masterGames.Select(masterGame =>
                new PossibleRoyaleMasterGameViewModel(masterGame, _clock, yearQuarter.Value, publisher.Value.PublisherGames.Any(y =>
                    y.MasterGame.MasterGame.Equals(masterGame.MasterGame)))).ToList();
            return viewModels;
        }
    }
}
