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
using FantasyCritic.Lib.Royale;
using FantasyCritic.Web.Models.Requests.Royale;
using FantasyCritic.Web.Models.Responses.Royale;
using FantasyCritic.Web.Models.RoundTrip;

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

        [HttpPost]
        public async Task<IActionResult> CreatePublisher([FromBody] CreateRoyalePublisherRequest request)
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
            return Ok();
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

            var viewModel = new RoyalePublisherViewModel(publisher.Value, _clock);
            return Ok(viewModel);
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
            if (purchaseResult.IsFailure)
            {
                return BadRequest(purchaseResult.Error);
            }

            return Ok();
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
    }
}
