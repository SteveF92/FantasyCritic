using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.Web.Models.Requests.Admin;
using FantasyCritic.Web.Models.Responses;
using FantasyCritic.Web.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Web.Controllers.API;

[Route("api/[controller]/[action]")]
[Authorize(Roles = "Admin")]
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

    public AdminController(AdminService adminService, FantasyCriticService fantasyCriticService, IClock clock, InterLeagueService interLeagueService,
        ILogger<AdminController> logger, GameAcquisitionService gameAcquisitionService, FantasyCriticUserManager userManager,
        IWebHostEnvironment webHostEnvironment, EmailSendingService emailSendingService)
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
    }

    [HttpPost]
    public async Task<IActionResult> CreateMasterGame([FromBody] CreateMasterGameRequest viewModel)
    {
        var possibleTags = await _interLeagueService.GetMasterGameTags();
        IReadOnlyList<MasterGameTag> tags = possibleTags
            .Where(x => viewModel.Tags.Contains(x.Name))
            .ToList();

        if (!tags.Any())
        {
            tags = new List<MasterGameTag>()
            {
                possibleTags.Single(x => x.Name == "NewGame")
            };
        }

        MasterGame masterGame = viewModel.ToDomain(_clock, tags);
        await _interLeagueService.CreateMasterGame(masterGame);
        var currentDate = _clock.GetToday();
        var vm = new MasterGameViewModel(masterGame, currentDate);

        _logger.LogInformation($"Created master game: {masterGame.MasterGameID}");
        return CreatedAtAction("MasterGame", "Game", new { id = masterGame.MasterGameID }, vm);
    }

    [HttpPost]
    public async Task<IActionResult> EditMasterGame([FromBody] EditMasterGameRequest viewModel)
    {
        Instant instant = _clock.GetCurrentInstant();

        var possibleTags = await _interLeagueService.GetMasterGameTags();
        IReadOnlyList<MasterGameTag> tags = possibleTags
            .Where(x => viewModel.Tags.Contains(x.Name))
            .ToList();

        var existingMasterGame = await _interLeagueService.GetMasterGame(viewModel.MasterGameID);
        if (existingMasterGame is null)
        {
            return BadRequest();
        }

        MasterGame masterGame = viewModel.ToDomain(existingMasterGame, instant, tags);
        await _interLeagueService.EditMasterGame(masterGame);
        var currentDate = _clock.GetToday();
        var vm = new MasterGameViewModel(masterGame, currentDate);

        _logger.LogInformation($"Edited master game: {masterGame.MasterGameID}");
        return CreatedAtAction("MasterGame", "Game", new { id = masterGame.MasterGameID }, vm);
    }

    [HttpPost]
    public async Task<IActionResult> CompleteMasterGameRequest([FromBody] CompleteMasterGameRequestRequest request)
    {
        MasterGameRequest? maybeRequest = await _interLeagueService.GetMasterGameRequest(request.RequestID);
        if (maybeRequest is null)
        {
            return BadRequest("That request does not exist.");
        }

        MasterGame? masterGame = null;
        if (request.MasterGameID.HasValue)
        {
            masterGame = await _interLeagueService.GetMasterGame(request.MasterGameID.Value);
            if (masterGame is null)
            {
                return BadRequest("Bad master game");
            }
        }

        Instant instant = _clock.GetCurrentInstant();
        await _interLeagueService.CompleteMasterGameRequest(maybeRequest, instant, request.ResponseNote, masterGame);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> CompleteMasterGameChangeRequest([FromBody] CompleteMasterGameChangeRequestRequest request)
    {
        MasterGameChangeRequest? maybeRequest = await _interLeagueService.GetMasterGameChangeRequest(request.RequestID);
        if (maybeRequest is null)
        {
            return BadRequest("That request does not exist.");
        }

        Instant instant = _clock.GetCurrentInstant();
        await _interLeagueService.CompleteMasterGameChangeRequest(maybeRequest, instant, request.ResponseNote);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> LinkGameToOpenCritic([FromBody] LinkGameToOpenCriticRequest request)
    {
        MasterGame? masterGame = await _interLeagueService.GetMasterGame(request.MasterGameID);
        if (masterGame is null)
        {
            return BadRequest("Bad master game");
        }

        await _interLeagueService.LinkToOpenCritic(masterGame, request.OpenCriticID);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> LinkGameToGG([FromBody] LinkGameToGGRequest request)
    {
        MasterGame? masterGame = await _interLeagueService.GetMasterGame(request.MasterGameID);
        if (masterGame is null)
        {
            return BadRequest("Bad master game");
        }

        await _interLeagueService.LinkToGG(masterGame, request.GGToken);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> MergeMasterGame([FromBody] MergeGameRequest request)
    {
        MasterGame? removeMasterGame = await _interLeagueService.GetMasterGame(request.RemoveMasterGameID);
        MasterGame? mergeIntoMasterGame = await _interLeagueService.GetMasterGame(request.MergeIntoMasterGameID);
        if (removeMasterGame is null || mergeIntoMasterGame is null)
        {
            return BadRequest("Bad master game");
        }

        await _interLeagueService.MergeMasterGame(removeMasterGame, mergeIntoMasterGame);

        return Ok();
    }

    public async Task<ActionResult<List<MasterGameRequestViewModel>>> ActiveMasterGameRequests()
    {
        IReadOnlyList<MasterGameRequest> requests = await _interLeagueService.GetAllMasterGameRequests();

        var currentDate = _clock.GetToday();
        var viewModels = requests.Select(x => new MasterGameRequestViewModel(x, currentDate)).OrderBy(x => x.GameName).ToList();
        return viewModels;
    }

    public async Task<ActionResult<List<MasterGameChangeRequestViewModel>>> ActiveMasterGameChangeRequests()
    {
        IReadOnlyList<MasterGameChangeRequest> requests = await _interLeagueService.GetAllMasterGameChangeRequests();

        var currentDate = _clock.GetToday();
        var viewModels = requests.Select(x => new MasterGameChangeRequestViewModel(x, currentDate)).OrderBy(x => x.MasterGame.GameName).ToList();
        return viewModels;
    }

    public async Task<ActionResult<MasterGameRequestViewModel>> GetMasterGameRequest(Guid requestID)
    {
        IReadOnlyList<MasterGameRequest> requests = await _interLeagueService.GetAllMasterGameRequests();

        var request = requests.SingleOrDefault(x => x.RequestID == requestID);
        if (request is null)
        {
            return NotFound();
        }

        var currentDate = _clock.GetToday();
        var vm = new MasterGameRequestViewModel(request, currentDate);
        return vm;
    }

    public async Task<ActionResult<MasterGameChangeRequestViewModel>> GetMasterGameChangeRequest(Guid changeRequestID)
    {
        IReadOnlyList<MasterGameChangeRequest> requests = await _interLeagueService.GetAllMasterGameChangeRequests();

        var currentDate = _clock.GetToday();
        var request = requests.SingleOrDefault(x => x.RequestID == changeRequestID);
        if (request is null)
        {
            return NotFound();
        }

        var vm = new MasterGameChangeRequestViewModel(request, currentDate);
        return vm;
    }

    [HttpPost]
    public async Task<IActionResult> FullDataRefresh()
    {
        await _adminService.FullDataRefresh();
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> RefreshCriticInfo()
    {
        await _adminService.RefreshCriticInfo();
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> RefreshGGInfo()
    {
        await _adminService.RefreshGGInfo();
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> UpdateFantasyPoints()
    {
        await _adminService.UpdateFantasyPoints();
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> MakePublisherSlotsConsistent()
    {
        await _adminService.MakePublisherSlotsConsistent();
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

        var leagueActionSets = actionResults.GetLeagueActionSets(true);
        var leagueActionSetViewModels = leagueActionSets.Select(x => new LeagueActionProcessingSetViewModel(x, currentDate));
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
    public async Task<IActionResult> RefreshCaches()
    {
        await _adminService.RefreshCaches();
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> RefreshPatreonInfo()
    {
        await _adminService.UpdatePatreonRoles();
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

    [HttpGet]
    public ActionResult<DateParseResponse> ParseEstimatedDate(string estimatedReleaseDate)
    {
        var dates = TimeFunctions.ParseEstimatedReleaseDate(estimatedReleaseDate, _clock);
        var response = new DateParseResponse(dates.minimumReleaseDate, dates.maximumReleaseDate);
        return response;
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
        await _emailSendingService.SendPublicBidEmails();
        return Ok();
    }
}
