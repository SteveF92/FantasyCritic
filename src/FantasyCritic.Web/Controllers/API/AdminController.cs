using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Models.Requests.Admin;
using FantasyCritic.Web.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Web.Controllers.API;

[Route("api/[controller]/[action]")]
[Authorize("Admin")]
public class AdminController : FantasyCriticController
{
    private readonly AdminService _adminService;
    private readonly FantasyCriticService _fantasyCriticService;
    private readonly InterLeagueService _interLeagueService;
    private readonly IClock _clock;
    private readonly ILogger _logger;
    private readonly GameAcquisitionService _gameAcquisitionService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly EmailSendingService _emailSendingService;
    private readonly DiscordPushService _discordPushService;
    private readonly IMasterGameRepo _masterGameRepo;

    public AdminController(AdminService adminService, FantasyCriticService fantasyCriticService, IClock clock, InterLeagueService interLeagueService,
        ILogger<AdminController> logger, GameAcquisitionService gameAcquisitionService, FantasyCriticUserManager userManager,
        IWebHostEnvironment webHostEnvironment, EmailSendingService emailSendingService, DiscordPushService discordPushService, IMasterGameRepo masterGameRepo)
        : base(userManager)
    {
        _adminService = adminService;
        _fantasyCriticService = fantasyCriticService;
        _clock = clock;
        _interLeagueService = interLeagueService;
        _logger = logger;
        _gameAcquisitionService = gameAcquisitionService;
        _webHostEnvironment = webHostEnvironment;
        _emailSendingService = emailSendingService;
        _discordPushService = discordPushService;
        _masterGameRepo = masterGameRepo;
    }

    [HttpPost]
    public async Task<IActionResult> MakePublisherSlotsConsistent()
    {
        await _adminService.MakePublisherSlotsConsistent();
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> RecalculateWinners()
    {
        await _adminService.RecalculateWinners();
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> RecalculateRoyaleWinners()
    {
        await _adminService.RecalculateRoyaleWinners();
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> DeleteLeague([FromBody] DeleteLeagueRequest request)
    {
        League? league = await _fantasyCriticService.GetLeagueByID(request.LeagueID);
        if (league is null)
        {
            return BadRequest();
        }

        if (!league.TestLeague)
        {
            return BadRequest();
        }

        await _fantasyCriticService.DeleteLeague(league);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> ResendConfirmationEmail([FromBody] AdminResendConfirmationEmail request)
    {
        var user = await _userManager.FindByIdAsync(request.UserID.ToString());
        if (user is null)
        {
            return BadRequest();
        }

        var confirmLink = await LinkBuilder.GetConfirmEmailLink(_userManager, user, Request);
        await _emailSendingService.SendConfirmationEmail(user, confirmLink);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> SendPublicBiddingEmails()
    {
        var supportedYears = await _interLeagueService.GetSupportedYears();
        var activeYears = supportedYears.Where(x => x.OpenForPlay && !x.Finished);

        var publicBiddingSets = new List<LeagueYearPublicBiddingSet>();
        foreach (var year in activeYears)
        {
            var publicBiddingSetsForYear = await _gameAcquisitionService.GetPublicBiddingGames(year.Year);
            publicBiddingSets.AddRange(publicBiddingSetsForYear);
        }

        await _emailSendingService.SendPublicBidEmails(publicBiddingSets);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> SendSpoofScoreUpdate()
    {
        var isProduction = string.Equals(_webHostEnvironment.EnvironmentName, "PRODUCTION", StringComparison.OrdinalIgnoreCase);
        if (isProduction)
        {
            return BadRequest("This is a test endpoint. Do not use in production.");
        }
        var currentSupportedYear = (await _interLeagueService.GetSupportedYears())
            .Where(x => x.OpenForPlay && !x.Finished)
            .MaxBy(x => x.Year);

        var masterGame = await _masterGameRepo.GetTestMasterGame(currentSupportedYear!.Year);

        _discordPushService.QueueGameCriticScoreUpdateMessage(masterGame, 80m, 85m);
        await _discordPushService.SendBatchedMasterGameUpdates();

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> SendSpoofEditUpdate()
    {
        var isProduction = string.Equals(_webHostEnvironment.EnvironmentName, "PRODUCTION", StringComparison.OrdinalIgnoreCase);
        if (isProduction)
        {
            return BadRequest("This is a test endpoint. Do not use in production.");
        }
        var currentSupportedYear = (await _interLeagueService.GetSupportedYears())
            .Where(x => x.OpenForPlay && !x.Finished)
            .MaxBy(x => x.Year);

        var masterGameYear = await _masterGameRepo.GetTestMasterGameYear(currentSupportedYear!.Year);

        _discordPushService.QueueMasterGameEditMessage(masterGameYear, masterGameYear, new List<string>() { "The Test Game Was Changed" });
        await _discordPushService.SendBatchedMasterGameUpdates();
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> SendSpoofNewUpdate()
    {
        var isProduction = string.Equals(_webHostEnvironment.EnvironmentName, "PRODUCTION", StringComparison.OrdinalIgnoreCase);
        if (isProduction)
        {
            return BadRequest("This is a test endpoint. Do not use in production.");
        }

        var currentSupportedYear = (await _interLeagueService.GetSupportedYears())
            .Where(x => x.OpenForPlay && !x.Finished)
            .MaxBy(x => x.Year);

        var masterGame = await _masterGameRepo.GetTestMasterGame(currentSupportedYear!.Year);
        _discordPushService.QueueNewMasterGameMessage(masterGame);
        await _discordPushService.SendBatchedMasterGameUpdates();

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> SendSpoofReleasedUpdate()
    {
        var isProduction = string.Equals(_webHostEnvironment.EnvironmentName, "PRODUCTION", StringComparison.OrdinalIgnoreCase);
        if (isProduction)
        {
            return BadRequest("This is a test endpoint. Do not use in production.");
        }
        var currentSupportedYear = (await _interLeagueService.GetSupportedYears())
            .Where(x => x.OpenForPlay && !x.Finished)
            .MaxBy(x => x.Year);

        var masterGameYear = await _masterGameRepo.GetTestMasterGameYear(currentSupportedYear!.Year);
        var masterGameList = new List<MasterGameYear>() { masterGameYear };
        await _discordPushService.SendGameReleaseUpdates(masterGameList);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> GrantSuperDrops()
    {
        await _adminService.GrantSuperDrops();
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> ExpireTrades()
    {
        await _adminService.ExpireTrades();
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> PushYearEndDiscordMessages()
    {
        var currentDate = _clock.GetToday();
        var leagueYears = await _fantasyCriticService.GetLeagueYears(currentDate.Year);
        await _discordPushService.SendFinalYearStandings(leagueYears, currentDate);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> RefreshPatreonInfo()
    {
        await _adminService.UpdatePatreonRoles();
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> UpdateDailyPublisherStatistics()
    {
        await _adminService.UpdateDailyPublisherStatistics();
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> PushPublicBiddingDiscordMessages()
    {
        var supportedYears = await _interLeagueService.GetSupportedYears();
        var activeYears = supportedYears.Where(x => x.OpenForPlay && !x.Finished);

        var publicBiddingSets = new List<LeagueYearPublicBiddingSet>();
        foreach (var year in activeYears)
        {
            var publicBiddingSetsForYear = await _gameAcquisitionService.GetPublicBiddingGames(year.Year);
            publicBiddingSets.AddRange(publicBiddingSetsForYear);
        }

        await _discordPushService.SendPublicBiddingSummary(publicBiddingSets);
        return Ok();
    }
}
