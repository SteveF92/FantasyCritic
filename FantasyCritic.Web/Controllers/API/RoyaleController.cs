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
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
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
    [Authorize]
    public class RoyaleController : FantasyCriticController
    {
        private readonly IClock _clock;
        private readonly ILogger<RoyaleController> _logger;
        private readonly FantasyCriticUserManager _userManager;
        private readonly RoyaleService _royaleService;
        private readonly InterLeagueService _interLeagueService;
        private static readonly SemaphoreSlim _royaleSemaphore = new SemaphoreSlim(1, 1);

        public RoyaleController(IClock clock, ILogger<RoyaleController> logger, FantasyCriticUserManager userManager,
            RoyaleService royaleService, InterLeagueService interLeagueService) : base(userManager)
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
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            if (string.IsNullOrWhiteSpace(request.PublisherName))
            {
                return BadRequest("You cannot have a blank name.");
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

        [HttpPost]
        public async Task<IActionResult> ChangePublisherName([FromBody] ChangeRoyalePublisherNameRequest request)
        {
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            if (string.IsNullOrWhiteSpace(request.PublisherName))
            {
                return BadRequest("You cannot have a blank name.");
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

            await _royaleService.ChangePublisherName(publisher.Value, request.PublisherName);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePublisherIcon([FromBody] ChangeRoyalePublisherIconRequest request)
        {
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            if (string.IsNullOrWhiteSpace(request.PublisherIcon))
            {
                return BadRequest("You cannot have a blank name.");
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

            await _royaleService.ChangePublisherIcon(publisher.Value, request.PublisherIcon.ToMaybe());
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("{year}/{quarter}")]
        public async Task<IActionResult> GetUserRoyalePublisher(int year, int quarter)
        {
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

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

            IReadOnlyList<RoyaleYearQuarter> quartersWon = await _royaleService.GetQuartersWonByUser(publisher.Value.User);
            var currentDate = _clock.GetToday();
            var masterGameTags = await _interLeagueService.GetMasterGameTags();
            var viewModel = new RoyalePublisherViewModel(publisher.Value, currentDate, null, quartersWon, masterGameTags);
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

            IReadOnlyList<RoyaleYearQuarter> quartersWon = await _royaleService.GetQuartersWonByUser(publisher.Value.User);
            var currentDate = _clock.GetToday();
            var masterGameTags = await _interLeagueService.GetMasterGameTags();
            var viewModel = new RoyalePublisherViewModel(publisher.Value, currentDate, null, quartersWon, masterGameTags);
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
            var masterGameTags = await _interLeagueService.GetMasterGameTags();
            IReadOnlyDictionary<FantasyCriticUser, IReadOnlyList<RoyaleYearQuarter>> previousWinners = await _royaleService.GetRoyaleWinners();
            foreach (var publisher in publishersToShow)
            {
                int? thisRanking = null;
                if (publisher.GetTotalFantasyPoints() > 0)
                {
                    thisRanking = ranking;
                    ranking++;
                }

                bool hasWinningQuarters = previousWinners.TryGetValue(publisher.User, out var winningQuarters);
                if (!hasWinningQuarters)
                {
                    winningQuarters = new List<RoyaleYearQuarter>();
                }

                var currentDate = _clock.GetToday();
                var vm = new RoyalePublisherViewModel(publisher, currentDate, thisRanking, winningQuarters, masterGameTags);
                viewModels.Add(vm);
            }

            return Ok(viewModels);
        }

        [HttpPost]
        public async Task<IActionResult> PurchaseGame([FromBody] PurchaseRoyaleGameRequest request)
        {
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

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

            await _royaleSemaphore.WaitAsync();
            try
            {
                var purchaseResult = await _royaleService.PurchaseGame(publisher.Value, masterGame.Value);
                var viewModel = new PlayerClaimResultViewModel(purchaseResult);
                return Ok(viewModel);
            }
            finally
            {
                _royaleSemaphore.Release(1);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SellGame([FromBody] SellRoyaleGameRequest request)
        {
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            Maybe<RoyalePublisher> publisher = await _royaleService.GetPublisher(request.PublisherID);
            if (publisher.HasNoValue)
            {
                return NotFound();
            }

            if (!publisher.Value.User.Equals(currentUser))
            {
                return Forbid();
            }

            await _royaleSemaphore.WaitAsync();
            try
            {
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
            finally
            {
                _royaleSemaphore.Release(1);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetAdvertisingMoney([FromBody] SetAdvertisingMoneyRequest request)
        {
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            Maybe<RoyalePublisher> publisher = await _royaleService.GetPublisher(request.PublisherID);
            if (publisher.HasNoValue)
            {
                return NotFound();
            }

            if (!publisher.Value.User.Equals(currentUser))
            {
                return Forbid();
            }

            await _royaleSemaphore.WaitAsync();
            try
            {
                var publisherGame = publisher.Value.PublisherGames.SingleOrDefault(x => x.MasterGame.MasterGame.MasterGameID == request.MasterGameID);
                if (publisherGame is null)
                {
                    return BadRequest();
                }

                var truncatedRequest = request.AdvertisingMoney.TruncateToPrecision(2);
                var setAdvertisingMoneyResult = await _royaleService.SetAdvertisingMoney(publisher.Value, publisherGame, truncatedRequest);
                if (setAdvertisingMoneyResult.IsFailure)
                {
                    return BadRequest(setAdvertisingMoneyResult.Error);
                }

                return Ok();
            }
            finally
            {
                _royaleSemaphore.Release(1);
            }
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

            var currentDate = _clock.GetToday();
            var masterGameTags = await _interLeagueService.GetMasterGameTags();
            var masterGames = await _royaleService.GetMasterGamesForYearQuarter(yearQuarter.Value.YearQuarter);
            if (!string.IsNullOrWhiteSpace(gameName))
            {
                masterGames = MasterGameSearching.SearchMasterGameYears(gameName, masterGames);
            }
            else
            {
                masterGames = masterGames
                    .Where(x => x.WillReleaseInQuarter(yearQuarter.Value.YearQuarter))
                    .Where(x => !x.MasterGame.IsReleased(currentDate))
                    .Where(x => !LeagueTagExtensions.GetRoyaleClaimErrors(masterGameTags, x.MasterGame, currentDate).Any())
                    .Take(1000)
                    .ToList();
            }

            var viewModels = masterGames.Select(masterGame =>
                new PossibleRoyaleMasterGameViewModel(masterGame, currentDate, yearQuarter.Value, publisher.Value.PublisherGames.Any(y =>
                    y.MasterGame.MasterGame.Equals(masterGame.MasterGame)), masterGameTags)).ToList();
            return viewModels;
        }
    }
}
