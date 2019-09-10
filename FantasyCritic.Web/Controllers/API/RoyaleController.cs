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
        private readonly RoyaleService _royaleService;
        private readonly IClock _clock;
        private readonly ILogger<RoyaleController> _logger;
        private readonly FantasyCriticUserManager _userManager;

        public RoyaleController(RoyaleService royaleService, IClock clock, ILogger<RoyaleController> logger, FantasyCriticUserManager userManager)
        {
            _royaleService = royaleService;
            _clock = clock;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> RoyaleQuarters()
        {
            IReadOnlyList<RoyaleYearQuarter> supportedQuarters = await _royaleService.GetYearQuarters();
            var viewModels = supportedQuarters.Select(x => new RoyaleYearQuarterViewModel(x));
            return Ok(viewModels);
        }

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
    }
}
