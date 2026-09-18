using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.SharedSerialization.API;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.Web.Models.Responses;
using FantasyCritic.Web.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Web.Controllers.API;

[Route("api/[controller]/[action]")]
[Authorize("ActionRunner")]
public class ActionRunnerController : FantasyCriticController
{
    private readonly AdminService _adminService;
    private readonly FantasyCriticService _fantasyCriticService;
    private readonly InterLeagueService _interLeagueService;
    private readonly IClock _clock;
    private readonly ILogger _logger;
    private readonly GameAcquisitionService _gameAcquisitionService;
    private readonly EmailSendingService _emailSendingService;
    private readonly DiscordPushService _discordPushService;
    private readonly IJobRepo _jobRepo;

    public ActionRunnerController(AdminService adminService, FantasyCriticService fantasyCriticService, IClock clock, InterLeagueService interLeagueService,
        ILogger<ActionRunnerController> logger, GameAcquisitionService gameAcquisitionService, FantasyCriticUserManager userManager,
        EmailSendingService emailSendingService, DiscordPushService discordPushService, IJobRepo jobRepo)
        : base(userManager)
    {
        _adminService = adminService;
        _fantasyCriticService = fantasyCriticService;
        _clock = clock;
        _interLeagueService = interLeagueService;
        _logger = logger;
        _gameAcquisitionService = gameAcquisitionService;
        _emailSendingService = emailSendingService;
        _discordPushService = discordPushService;
        _jobRepo = jobRepo;
    }

    [HttpGet]
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
        return fullSet;
    }

    [HttpGet]
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
    [ProducesResponseType<FantasyCriticJobViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FantasyCriticJobViewModel>> ProcessActions()
    {
        var canProcessActions = await _adminService.CanProcessActions();
        if (canProcessActions.IsFailure)
        {
            return BadRequest(canProcessActions.Error);
        }

        //The worker runs jobs one at a time, so a second press would process actions again as soon as the first run finished.
        var incompleteJobs = await _jobRepo.GetIncompleteJobs();
        var existingJob = incompleteJobs.FirstOrDefault(x => x.Type.Equals(FantasyCriticJobType.ProcessActions));
        if (existingJob is not null)
        {
            return BadRequest($"Actions are already being processed (job is {existingJob.Status.Value}).");
        }

        var currentUser = await GetCurrentUserOrThrow();
        var result = await _jobRepo.EnqueueJob(FantasyCriticJobType.ProcessActions, currentUser, _clock.GetCurrentInstant());
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        _logger.LogInformation("{User} queued job {Job}.", currentUser.UserName, result.Value);
        return new FantasyCriticJobViewModel(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> ProcessSpecialAuctions()
    {
        await _adminService.ProcessSpecialAuctions();
        return Ok();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TurnOnActionProcessingMode()
    {
        await _interLeagueService.SetActionProcessingMode(true);
        return Ok();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
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

    [HttpGet]
    public async Task<ActionResult<List<DatabaseSnapshotInfoViewModel>>> GetRecentDatabaseSnapshots()
    {
        IReadOnlyList<DatabaseSnapshotInfo> snaps = await _adminService.GetRecentDatabaseSnapshots();

        var vms = snaps.Select(x => new DatabaseSnapshotInfoViewModel(x)).ToList();
        return vms;
    }

    [HttpPost]
    public async Task<IActionResult> UpdateTopBidsAndDrops()
    {
        await _adminService.UpdateTopBidsAndDropsForMostRecentWeek();
        return Ok();
    }
}
