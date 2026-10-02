using System.Diagnostics;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.SharedSerialization.API;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.Web.Hubs;
using FantasyCritic.Web.Models.Requests.Admin;
using FantasyCritic.Web.Models.Responses;
using FantasyCritic.Web.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Web.Controllers.API;

[Route("api/[controller]/[action]")]
[Authorize("Admin")]
public class AdminController : BaseJobQueuingController
{
    private readonly FantasyCriticService _fantasyCriticService;
    private readonly InterLeagueService _interLeagueService;
    private readonly ILogger _logger;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly EmailSendingService _emailSendingService;
    private readonly DiscordPushService _discordPushService;
    private readonly IMasterGameRepo _masterGameRepo;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly EnvironmentConfiguration _environmentConfiguration;
    private readonly BuildInfo _buildInfo;
    private readonly ServiceHealthClient _serviceHealthClient;
    private readonly UpdateHubConnections _updateHubConnections;
    private readonly ErrorLogCounter _errorLogCounter;

    public AdminController(FantasyCriticService fantasyCriticService, IClock clock, InterLeagueService interLeagueService,
        ILogger<AdminController> logger, FantasyCriticUserManager userManager,
        IWebHostEnvironment webHostEnvironment, EmailSendingService emailSendingService, DiscordPushService discordPushService, IMasterGameRepo masterGameRepo,
        IFantasyCriticRepo fantasyCriticRepo, EnvironmentConfiguration environmentConfiguration, BuildInfo buildInfo, IJobRepo jobRepo, ServiceHealthClient serviceHealthClient, GrafanaLogLinks logLinks,
        UpdateHubConnections updateHubConnections, ErrorLogCounter errorLogCounter)
        : base(userManager, jobRepo, clock, logLinks)
    {
        _fantasyCriticService = fantasyCriticService;
        _interLeagueService = interLeagueService;
        _logger = logger;
        _webHostEnvironment = webHostEnvironment;
        _emailSendingService = emailSendingService;
        _discordPushService = discordPushService;
        _masterGameRepo = masterGameRepo;
        _fantasyCriticRepo = fantasyCriticRepo;
        _environmentConfiguration = environmentConfiguration;
        _buildInfo = buildInfo;
        _serviceHealthClient = serviceHealthClient;
        _updateHubConnections = updateHubConnections;
        _errorLogCounter = errorLogCounter;
    }

    [HttpGet]
    public ActionResult<BuildInfoViewModel> BuildInfo()
    {
        return new BuildInfoViewModel(_buildInfo);
    }

    [HttpGet]
    [ProducesResponseType<ServiceMonitorViewModel>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceMonitorViewModel>> GetServiceMonitor()
    {
        var workerHealthTask = _serviceHealthClient.GetWorkerHealth();
        var discordBotHealthTask = _serviceHealthClient.GetDiscordBotHealth();
        var workerHealth = await workerHealthTask;
        var discordBotHealth = await discordBotHealthTask;

        //Both read from the database rather than taken from the worker's answer, so that the state shown here
        //is the one a deploy's drain acts on, and is still known when the worker cannot be reached.
        //Timed as well, for the web row: one small row, so this is about as close to the database's round trip as a query gets.
        var databaseStopwatch = Stopwatch.StartNew();
        var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
        databaseStopwatch.Stop();
        var incompleteJobs = await _jobRepo.GetIncompleteJobs();

        var workerReachable = workerHealth.Status != ServiceHealthClient.UnreachableStatus;
        var workerHealthy = workerHealth.Status != ServiceHealthClient.UnhealthyStatus;
        var workerState = WorkerState.Determine(workerReachable, workerHealthy, systemWideSettings.WorkerShouldPullNewJobs, incompleteJobs);

        //Available is the container's memory limit where it has one, and the machine's memory otherwise.
        using var process = Process.GetCurrentProcess();
        var availableMemory = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;

        //Nothing asks the web app how it is: an unhealthy one could not have answered this request, so it can only be healthy.
        var webDetails = new List<ServiceHealthDetail>
        {
            ServiceHealthDetail.FromText("Database round trip", $"{databaseStopwatch.ElapsedMilliseconds} ms"),
            ServiceHealthDetail.FromText("Live draft connections", _updateHubConnections.Count.ToString()),
            ServiceHealthDetail.FromText("Working set", $"{Megabytes(process.WorkingSet64)} of {Megabytes(availableMemory)} available"),
            ServiceHealthDetail.FromText("GC heap", Megabytes(GC.GetTotalMemory(false)))
        };

        var errorLogSummary = _errorLogCounter.GetSummary();
        webDetails.Add(ServiceHealthDetail.FromText("Errors since start", errorLogSummary.Count.ToString()));
        if (errorLogSummary.LastErrorAt.HasValue)
        {
            webDetails.Add(ServiceHealthDetail.FromTime("Last error", errorLogSummary.LastErrorAt.Value));
        }

        var webHealth = new ServiceHealthReport(nameof(HealthStatus.Healthy), "Responding to requests.", webDetails);

        return new ServiceMonitorViewModel(_clock.GetCurrentInstant(), systemWideSettings.WorkerShouldPullNewJobs, workerState, webHealth, workerHealth, discordBotHealth, _logLinks);
    }

    private static string Megabytes(long bytes) => $"{bytes / (1024 * 1024):N0} MB";

    //Turning the worker off does not stop its container. It stops pulling new jobs, finishes the one it has, and idles.
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TurnOffWorker()
    {
        await _interLeagueService.SetWorkerShouldPullNewJobs(false);
        return Ok();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TurnOnWorker()
    {
        await _interLeagueService.SetWorkerShouldPullNewJobs(true);
        return Ok();
    }

    [HttpPost]
    [ProducesResponseType<FantasyCriticJobViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<ActionResult<FantasyCriticJobViewModel>> MakePublisherSlotsConsistent() => EnqueueJob(FantasyCriticJobType.MakeSlotsConsistent);

    [HttpPost]
    [ProducesResponseType<FantasyCriticJobViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<ActionResult<FantasyCriticJobViewModel>> RecalculateWinners() => EnqueueJob(FantasyCriticJobType.RecalculateLastSeasonWinners);

    [HttpPost]
    [ProducesResponseType<FantasyCriticJobViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<ActionResult<FantasyCriticJobViewModel>> RecalculateRoyaleWinners() => EnqueueJob(FantasyCriticJobType.RecalculateRoyaleWinners);

    [HttpPost]
    [ProducesResponseType<FantasyCriticJobViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<ActionResult<FantasyCriticJobViewModel>> RecomputeRulesBasedRoyaleGroups() => EnqueueJob(FantasyCriticJobType.RecomputeRulesBasedRoyaleGroups);

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
    public async Task<ActionResult<List<SupportUserSearchMatchViewModel>>> SearchSupportUsers([FromBody] SupportUserSearchRequest request)
    {
        string searchValue = request.SearchValue ?? "";
        if (request.SearchKind == SupportUserSearchKind.Email)
        {
            searchValue = _userManager.NormalizeEmail(searchValue) ?? "";
        }

        IReadOnlyList<FantasyCriticUser> users = await _userManager.SearchUsersForSupport(request.SearchKind, searchValue);
        if (users.Count == 0)
        {
            return Ok(Array.Empty<SupportUserSearchMatchViewModel>());
        }

        IReadOnlyList<LeaguePublisherRowForUser> leagueRows = await _fantasyCriticRepo.GetLeaguePublisherRowsForUsers(users.Select(u => u.Id));
        ILookup<Guid, LeaguePublisherRowForUser> rowsByUser = leagueRows.ToLookup(x => x.UserID);

        List<SupportUserSearchMatchViewModel> viewModels = new List<SupportUserSearchMatchViewModel>(users.Count);
        foreach (FantasyCriticUser user in users)
        {
            viewModels.Add(new SupportUserSearchMatchViewModel(user, rowsByUser[user.Id].ToList()));
        }

        return viewModels;
    }

    [HttpPost]
    public async Task<IActionResult> OpenSupportTicket([FromBody] OpenSupportTicketRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserID.ToString());
        if (user is null)
        {
            return BadRequest("Cannot open support ticket for a non-existent user.");
        }

        try
        {
            SupportTicket supportTicket = await _userManager.OpenSupportTicket(user, request.IssueDescription, openedByUser: false);
            return Ok(new SupportTicketViewModel(supportTicket));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<SupportTicketViewModel>> CloseSupportTicket([FromBody] CloseSupportTicketRequest request)
    {
        var existingTicket = await _userManager.GetSupportTicket(request.SupportTicketID);
        if (existingTicket is null)
        {
            return BadRequest("Support ticket does not exist.");
        }

        if (!existingTicket.Active)
        {
            return BadRequest("Support ticket is already closed.");
        }

        var closedTicket = await _userManager.CloseSupportTicket(existingTicket, request.ResolutionNotes);
        var vm = new SupportTicketViewModel(closedTicket);
        return vm;
    }

    [HttpGet]
    public async Task<ActionResult<List<SupportTicketAdminListEntryViewModel>>> GetActiveSupportTickets()
    {
        var tickets = await _userManager.GetAllActiveSupportTickets();
        var viewModels = tickets.Select(t => new SupportTicketAdminListEntryViewModel(t)).ToList();
        return viewModels;
    }

    [HttpPost]
    [ProducesResponseType<FantasyCriticJobViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<ActionResult<FantasyCriticJobViewModel>> SendPublicBiddingEmails() => EnqueueJob(FantasyCriticJobType.SendPublicBiddingEmails);

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

        await _discordPushService.SendMasterGameUpdatesImmediately([], [new GameCriticScoreUpdateMessage(Guid.NewGuid(), masterGame, 80m, 85m)], []);

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

        var editMessage = new MasterGameEditMessage(Guid.NewGuid(), masterGameYear.MasterGame, masterGameYear.MasterGame, masterGameYear.Year, new List<string>() { "The Test Game Was Changed" });
        await _discordPushService.SendMasterGameUpdatesImmediately([], [], [editMessage]);
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
        await _discordPushService.SendMasterGameUpdatesImmediately([new NewMasterGameMessage(Guid.NewGuid(), masterGame)], [], []);

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
    [ProducesResponseType<FantasyCriticJobViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<ActionResult<FantasyCriticJobViewModel>> SendReleasingThisWeekUpdate() => EnqueueJob(FantasyCriticJobType.SendReleasingThisWeekUpdate);

    [HttpPost]
    [ProducesResponseType<FantasyCriticJobViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<ActionResult<FantasyCriticJobViewModel>> GrantSuperDrops() => EnqueueJob(FantasyCriticJobType.GrantSuperDrops);

    [HttpPost]
    [ProducesResponseType<FantasyCriticJobViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<ActionResult<FantasyCriticJobViewModel>> ExpireTrades() => EnqueueJob(FantasyCriticJobType.ExpireTrades);

    [HttpPost]
    [ProducesResponseType<FantasyCriticJobViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<ActionResult<FantasyCriticJobViewModel>> EndOfYearRollover() => EnqueueJob(FantasyCriticJobType.EndOfYearRollover);

    [HttpPost]
    [ProducesResponseType<FantasyCriticJobViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<ActionResult<FantasyCriticJobViewModel>> SendFinalYearStandings() => EnqueueJob(FantasyCriticJobType.SendFinalYearStandings);

    [HttpPost]
    [ProducesResponseType<FantasyCriticJobViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<ActionResult<FantasyCriticJobViewModel>> RefreshPatreonInfo() => EnqueueJob(FantasyCriticJobType.RefreshPatreonInfo);

    [HttpPost]
    [ProducesResponseType<FantasyCriticJobViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<ActionResult<FantasyCriticJobViewModel>> UpdateDailyPublisherStatistics() => EnqueueJob(FantasyCriticJobType.UpdateDailyPublisherStatistics);

    [HttpPost]
    [ProducesResponseType<FantasyCriticJobViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<ActionResult<FantasyCriticJobViewModel>> PushPublicBiddingDiscordMessages() => EnqueueJob(FantasyCriticJobType.SendPublicBiddingDiscordMessages);

    [HttpGet]
    [ProducesResponseType<FantasyCriticUserViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FantasyCriticUserViewModel>> GetUserInfo([FromQuery] Guid userID)
    {
        var user = await _userManager.FindByIdAsync(userID.ToString());
        if (user is null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);
        return new FantasyCriticUserViewModel(user, roles);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GrantRole([FromBody] UserRoleRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserID.ToString());
        if (user is null)
        {
            return NotFound();
        }

        try
        {
            await _userManager.AddToRoleAsync(user, request.RoleName);
        }
        catch (InvalidOperationException)
        {
            return BadRequest($"Role '{request.RoleName}' does not exist.");
        }

        return Ok();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveRole([FromBody] UserRoleRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserID.ToString());
        if (user is null)
        {
            return NotFound();
        }

        await _userManager.RemoveFromRoleAsync(user, request.RoleName);
        return Ok();
    }

    [HttpGet]
    [ProducesResponseType<List<SiteAnnouncementViewModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SiteAnnouncementViewModel>>> GetSiteAnnouncements()
    {
        var announcements = await _interLeagueService.GetSiteAnnouncements();
        return announcements.Select(x => new SiteAnnouncementViewModel(x)).ToList();
    }

    [HttpPost]
    [ProducesResponseType<SiteAnnouncementViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SiteAnnouncementViewModel>> CreateSiteAnnouncement([FromBody] CreateSiteAnnouncementRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Announcements must have a title.");
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest("Announcements must have a body.");
        }

        var announcement = request.ToDomain(_clock);
        await _interLeagueService.CreateSiteAnnouncement(announcement);
        return new SiteAnnouncementViewModel(announcement);
    }

    [HttpPost]
    [ProducesResponseType<SiteAnnouncementViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SiteAnnouncementViewModel>> EditSiteAnnouncement([FromBody] EditSiteAnnouncementRequest request)
    {
        var existingAnnouncement = await _interLeagueService.GetSiteAnnouncement(request.AnnouncementID);
        if (existingAnnouncement is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Announcements must have a title.");
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest("Announcements must have a body.");
        }

        var announcement = request.ToDomain();
        await _interLeagueService.EditSiteAnnouncement(announcement);
        return new SiteAnnouncementViewModel(announcement);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSiteAnnouncement([FromBody] DeleteSiteAnnouncementRequest request)
    {
        var existingAnnouncement = await _interLeagueService.GetSiteAnnouncement(request.AnnouncementID);
        if (existingAnnouncement is null)
        {
            return NotFound();
        }

        await _interLeagueService.DeleteSiteAnnouncement(request.AnnouncementID);
        return Ok();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult SetInitialTime([FromBody] SetTimeRequest request)
    {
        if (!_environmentConfiguration.IntegrationTestMode)
        {
            return NotFound();
        }

        if (_clock is not AdjustableClock adjustableClock)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                "IntegrationTestMode is enabled but the registered IClock is not an AdjustableClock.");
        }

        adjustableClock.SetInitialTime(Instant.FromDateTimeOffset(request.NewTime));
        return Ok();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult SetTime([FromBody] SetTimeRequest request)
    {
        if (!_environmentConfiguration.IntegrationTestMode)
        {
            return NotFound();
        }

        if (_clock is not AdjustableClock adjustableClock)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                "IntegrationTestMode is enabled but the registered IClock is not an AdjustableClock.");
        }

        var result = adjustableClock.SetTime(Instant.FromDateTimeOffset(request.NewTime));
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ResetTime()
    {
        if (!_environmentConfiguration.IntegrationTestMode)
        {
            return NotFound();
        }

        if (_clock is not AdjustableClock adjustableClock)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                "IntegrationTestMode is enabled but the registered IClock is not an AdjustableClock.");
        }

        adjustableClock.ResetTime();
        return Ok();
    }
}
