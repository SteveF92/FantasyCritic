using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.SharedSerialization.API;
using FantasyCritic.Web.Models.Requests.Admin;
using FantasyCritic.Web.Models.Responses;
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

    public AdminController(AdminService adminService, FantasyCriticService fantasyCriticService, IClock clock, InterLeagueService interLeagueService,
        ILogger<AdminController> logger, GameAcquisitionService gameAcquisitionService, FantasyCriticUserManager userManager,
        IWebHostEnvironment webHostEnvironment, EmailSendingService emailSendingService, DiscordPushService discordPushService)
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
        await _adminService.RefreshCaches();
        return Ok();
    }

    public async Task<ActionResult<ActionedGameSetViewModel>> ActionProcessingDryRun()
    {
        var supportedYears = await _interLeagueService.GetSupportedYears();
        SystemWideValues systemWideValues = await _interLeagueService.GetSystemWideValues();
        var currentYear = supportedYears.First(x => !x.Finished && x.OpenForPlay);

        IReadOnlyList<LeagueYear> allLeagueYears = await _adminService.GetLeagueYears(currentYear.Year);

        var nextBidTime = _clock.GetNextBidTime();
        var actionResults = await _adminService.GetActionProcessingDryRun(systemWideValues, currentYear.Year, nextBidTime, allLeagueYears);
        IEnumerable<LeagueAction> failingActions = actionResults.Results.LeagueActions.Where(x => x.IsFailed);
        var failingActionGames = failingActions.Select(x => x.MasterGameName).Distinct();

        var currentDate = _clock.GetToday();
        var allBids = await _gameAcquisitionService.GetActiveAcquisitionBids(currentYear, allLeagueYears);
        var distinctBids = allBids.SelectMany(x => x.Value).DistinctBy(x => x.MasterGame);
        List<MasterGameViewModel> pickupGames = distinctBids
            .Select(x => new MasterGameViewModel(x.MasterGame, currentDate, failingActionGames.Contains(x.MasterGame.GameName)))
            .ToList();

        var allDrops = await _gameAcquisitionService.GetActiveDropRequests(currentYear, allLeagueYears);
        var distinctDrops = allDrops.SelectMany(x => x.Value).DistinctBy(x => x.MasterGame);
        List<MasterGameViewModel> dropGames = distinctDrops
            .Select(x => new MasterGameViewModel(x.MasterGame, currentDate, failingActionGames.Contains(x.MasterGame.GameName)))
            .ToList();

        pickupGames = pickupGames.OrderByDescending(x => x.Error).ThenBy(x => x.MaximumReleaseDate).ToList();
        dropGames = dropGames.OrderByDescending(x => x.Error).ThenBy(x => x.MaximumReleaseDate).ToList();

        var leagueYearDictionary = allLeagueYears.ToDictionary(x => x.Key);
        var leagueActionViewModels = actionResults.Results.LeagueActions.Select(x => new LeagueActionViewModel(leagueYearDictionary[x.Publisher.LeagueYearKey], x)).ToList();

        var leagueActionSets = actionResults.GetLeagueActionSets();
        var masterGameYears = await _interLeagueService.GetMasterGameYears(currentYear.Year);
        var masterGameYearDictionary = masterGameYears.ToDictionary(x => x.MasterGame.MasterGameID);
        var leagueActionSetViewModels = leagueActionSets.Select(x => new LeagueActionProcessingSetViewModel(x, currentDate, masterGameYearDictionary));
        ActionedGameSetViewModel fullSet = new ActionedGameSetViewModel(pickupGames, dropGames, leagueActionViewModels, leagueActionSetViewModels);
        return Ok(fullSet);
    }

    public async Task<FileStreamResult> ComparableActionProcessingDryRun()
    {
        var supportedYears = await _interLeagueService.GetSupportedYears();
        SystemWideValues systemWideValues = await _interLeagueService.GetSystemWideValues();
        var currentYear = supportedYears.First(x => !x.Finished && x.OpenForPlay);

        IReadOnlyList<LeagueYear> allLeagueYears = await _adminService.GetLeagueYears(currentYear.Year);

        var nextBidTime = _clock.GetNextBidTime();
        var actionResults = await _adminService.GetActionProcessingDryRun(systemWideValues, currentYear.Year, nextBidTime, allLeagueYears);
        var viewModels = actionResults.Results.LeagueActions.Select(x => new ComparableLeagueActionViewModel(x))
            .OrderBy(x => x.LeagueID).ThenBy(x => x.PublisherID).ToList();

        var csvStream = CSVUtilities.GetCSVStream(viewModels);
        return new FileStreamResult(csvStream, "text/csv") { FileDownloadName = $"ComparableActions_{nextBidTime.ToEasternDate().ToISOString()}.csv" };
    }

    [HttpPost]
    public async Task<IActionResult> ProcessActions()
    {
        var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
        if (!systemWideSettings.ActionProcessingMode)
        {
            return BadRequest("Turn on action processing mode first.");
        }

        var isProduction = string.Equals(_webHostEnvironment.EnvironmentName, "PRODUCTION", StringComparison.OrdinalIgnoreCase);
        var today = _clock.GetCurrentInstant().ToEasternDate();
        var acceptableDays = new List<IsoDayOfWeek>
        {
            IsoDayOfWeek.Saturday,
            IsoDayOfWeek.Sunday
        };
        if (!acceptableDays.Contains(today.DayOfWeek) && isProduction)
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

            await _adminService.ProcessActions(systemWideValues, supportedYear.Year);
        }

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> ProcessSpecialAuctions()
    {
        await _adminService.ProcessSpecialAuctions();
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
    public async Task<IActionResult> TurnOnActionProcessingMode()
    {
        await _interLeagueService.SetActionProcessingMode(true);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> TurnOffActionProcessingMode()
    {
        await _interLeagueService.SetActionProcessingMode(false);
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
    public async Task<IActionResult> UpdateTopBidsAndDrops()
    {
        await _adminService.UpdateTopBidsAndDrops();
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
