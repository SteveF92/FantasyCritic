using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class LeagueManagerController : Controller
    {
        private readonly FantasyCriticUserManager _userManager;
        private readonly FantasyCriticService _fantasyCriticService;
        private readonly IClock _clock;

        public LeagueManagerController(FantasyCriticUserManager userManager, FantasyCriticService fantasyCriticService,
            IClock clock)
        {
            _userManager = userManager;
            _fantasyCriticService = fantasyCriticService;
            _clock = clock;
        }

        [HttpPost]
        public async Task<IActionResult> CreateLeague([FromBody] CreateLeagueRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            EligibilityLevel eligibilityLevel = await _fantasyCriticService.GetEligibilityLevel(request.MaximumEligibilityLevel);
            LeagueCreationParameters domainRequest = request.ToDomain(currentUser, eligibilityLevel);
            await _fantasyCriticService.CreateLeague(domainRequest);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> EditLeagueYearSettings([FromBody] LeagueYearSettingsViewModel request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var league = await _fantasyCriticService.GetLeagueByID(request.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            var leagueYear = await _fantasyCriticService.GetLeagueYear(league.Value.LeagueID, request.Year);
            if (leagueYear.HasNoValue)
            {
                return BadRequest();
            }

            if (league.Value.LeagueManager.UserID != currentUser.UserID)
            {
                return Unauthorized();
            }

            EligibilityLevel eligibilityLevel = await _fantasyCriticService.GetEligibilityLevel(request.MaximumEligibilityLevel);
            EditLeagueYearParameters domainRequest = request.ToDomain(currentUser, eligibilityLevel);
            Result result = await _fantasyCriticService.EditLeague(league.Value, domainRequest);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            await _fantasyCriticService.UpdateFantasyPoints(leagueYear.Value);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> InvitePlayer([FromBody] InviteRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var league = await _fantasyCriticService.GetLeagueByID(request.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            if (league.Value.LeagueManager.UserID != currentUser.UserID)
            {
                return Unauthorized();
            }

            var inviteUser = await _userManager.FindByEmailAsync(request.InviteEmail);
            if (inviteUser == null)
            {
                return BadRequest();
            }

            Result result = await _fantasyCriticService.InviteUser(league.Value, inviteUser);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ManagerClaimGame([FromBody] ClaimGameRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var publisher = await _fantasyCriticService.GetPublisher(request.PublisherID);
            if (publisher.HasNoValue)
            {
                return BadRequest();
            }

            var league = await _fantasyCriticService.GetLeagueByID(publisher.Value.League.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            var leagueYear = await _fantasyCriticService.GetLeagueYear(league.Value.LeagueID, publisher.Value.Year);
            if (leagueYear.HasNoValue)
            {
                return BadRequest();
            }

            if (league.Value.LeagueManager.UserID != currentUser.UserID)
            {
                return Forbid();
            }

            var claimUser = await _userManager.FindByIdAsync(publisher.Value.User.UserID.ToString());
            if (claimUser == null)
            {
                return BadRequest();
            }

            Maybe<MasterGame> masterGame = Maybe<MasterGame>.None;
            if (request.MasterGameID.HasValue)
            {
                masterGame = await _fantasyCriticService.GetMasterGame(request.MasterGameID.Value);
            }

            ClaimGameDomainRequest domainRequest = new ClaimGameDomainRequest(publisher.Value, request.GameName,
                request.Acquisition, request.CounterPick, request.ManagerOverride, masterGame);

            ClaimResult result = await _fantasyCriticService.ClaimGame(domainRequest);
            var viewModel = new ManagerClaimResultViewModel(result);

            await _fantasyCriticService.UpdateFantasyPoints(leagueYear.Value);
            return Ok(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ManagerAssociateGame([FromBody] AssociateGameRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var publisher = await _fantasyCriticService.GetPublisher(request.PublisherID);
            if (publisher.HasNoValue)
            {
                return BadRequest();
            }

            var league = await _fantasyCriticService.GetLeagueByID(publisher.Value.League.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            var leagueYear = await _fantasyCriticService.GetLeagueYear(league.Value.LeagueID, publisher.Value.Year);
            if (leagueYear.HasNoValue)
            {
                return BadRequest();
            }

            if (league.Value.LeagueManager.UserID != currentUser.UserID)
            {
                return Forbid();
            }

            var claimUser = await _userManager.FindByIdAsync(publisher.Value.User.UserID.ToString());
            if (claimUser == null)
            {
                return BadRequest();
            }

            var publisherGame = publisher.Value.PublisherGames.SingleOrDefault(x => x.PublisherGameID == request.PublisherGameID);
            if (publisherGame == null)
            {
                return BadRequest();
            }

            Maybe<MasterGame> masterGame = await _fantasyCriticService.GetMasterGame(request.MasterGameID);
            if (masterGame.HasNoValue)
            {
                return BadRequest();
            }

            AssociateGameDomainRequest domainRequest = new AssociateGameDomainRequest(publisher.Value, publisherGame, masterGame.Value, request.ManagerOverride);

            ClaimResult result = await _fantasyCriticService.AssociateGame(domainRequest);
            var viewModel = new ManagerClaimResultViewModel(result);

            await _fantasyCriticService.UpdateFantasyPoints(leagueYear.Value);

            return Ok(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> RemovePublisherGame([FromBody] GameRemoveRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var publisher = await _fantasyCriticService.GetPublisher(request.PublisherID);
            if (publisher.HasNoValue)
            {
                return BadRequest();
            }

            var league = await _fantasyCriticService.GetLeagueByID(publisher.Value.League.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            if (league.Value.LeagueManager.UserID != currentUser.UserID)
            {
                return Forbid();
            }

            Maybe<PublisherGame> publisherGame = await _fantasyCriticService.GetPublisherGame(request.PublisherGameID);
            if (publisherGame.HasNoValue)
            {
                return BadRequest();
            }

            var leagueYear = await _fantasyCriticService.GetLeagueYear(league.Value.LeagueID, publisher.Value.Year);
            if (leagueYear.HasNoValue)
            {
                return BadRequest();
            }

            Result result = await _fantasyCriticService.RemovePublisherGame(leagueYear.Value, publisher.Value, publisherGame.Value);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            await _fantasyCriticService.UpdateFantasyPoints(leagueYear.Value);

            return Ok();
        }

        [HttpPost]
        public Task<IActionResult> ManuallyScorePublisherGame([FromBody] ManualPublisherGameScoreRequest request)
        {
            return UpdateManualCriticScore(request.PublisherID, request.PublisherGameID, request.ManualCriticScore);
        }

        [HttpPost]
        public Task<IActionResult> RemoveManualPublisherGameScore([FromBody] ManualPublisherGameScoreRequest request)
        {
            return UpdateManualCriticScore(request.PublisherID, request.PublisherGameID, null);
        }

        public async Task<IActionResult> UpdateManualCriticScore(Guid publisherID, Guid publisherGameID, decimal? manualCriticScore)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var publisher = await _fantasyCriticService.GetPublisher(publisherID);
            if (publisher.HasNoValue)
            {
                return BadRequest();
            }

            var league = await _fantasyCriticService.GetLeagueByID(publisher.Value.League.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            if (league.Value.LeagueManager.UserID != currentUser.UserID)
            {
                return Forbid();
            }

            Maybe<PublisherGame> publisherGame = await _fantasyCriticService.GetPublisherGame(publisherGameID);
            if (publisherGame.HasNoValue)
            {
                return BadRequest();
            }

            var leagueYear = await _fantasyCriticService.GetLeagueYear(league.Value.LeagueID, publisher.Value.Year);
            if (leagueYear.HasNoValue)
            {
                return BadRequest();
            }

            await _fantasyCriticService.ManuallyScoreGame(publisherGame.Value, manualCriticScore);
            await _fantasyCriticService.UpdateFantasyPoints(leagueYear.Value);

            return Ok();
        }
    }
}
