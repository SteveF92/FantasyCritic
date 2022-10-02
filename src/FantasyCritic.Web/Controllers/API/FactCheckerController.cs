using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.SharedSerialization.API;
using FantasyCritic.Web.Models.Requests.Admin;
using FantasyCritic.Web.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Web.Controllers.API;

[Route("api/[controller]/[action]")]
[Authorize("FactChecker")]
public class FactCheckerController : FantasyCriticController
{
    private readonly AdminService _adminService;
    private readonly InterLeagueService _interLeagueService;
    private readonly IClock _clock;
    private readonly ILogger _logger;

    public FactCheckerController(AdminService adminService, IClock clock, InterLeagueService interLeagueService,
        ILogger<FactCheckerController> logger, FantasyCriticUserManager userManager)
        : base(userManager)
    {
        _adminService = adminService;
        _clock = clock;
        _interLeagueService = interLeagueService;
        _logger = logger;
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

        var currentUser = await GetCurrentUserOrThrow();
        MasterGame masterGame = viewModel.ToDomain(_clock, tags, currentUser);
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

        var user = await GetCurrentUserOrThrow();
        MasterGame editedMasterGame = viewModel.ToDomain(existingMasterGame, instant, tags);
        var result = await _interLeagueService.EditMasterGame(existingMasterGame, editedMasterGame, user);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        var currentDate = _clock.GetToday();
        var vm = new MasterGameViewModel(editedMasterGame, currentDate);

        _logger.LogInformation($"Edited master game: {editedMasterGame.MasterGameID}");
        return CreatedAtAction("MasterGame", "Game", new { id = editedMasterGame.MasterGameID }, vm);
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
        var user = await GetCurrentUserOrThrow();
        await _interLeagueService.CompleteMasterGameRequest(maybeRequest, instant, request.ResponseNote, user, masterGame);

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
        var user = await GetCurrentUserOrThrow();
        await _interLeagueService.CompleteMasterGameChangeRequest(maybeRequest, instant, request.ResponseNote, user);

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

        await _adminService.LinkToOpenCritic(masterGame, request.OpenCriticID);

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

        await _adminService.LinkToGG(masterGame, request.GGToken);

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

        await _adminService.MergeMasterGame(removeMasterGame, mergeIntoMasterGame);

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
        await _adminService.RefreshGGInfo(true);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> UpdateFantasyPoints()
    {
        await _adminService.UpdateFantasyPoints();
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
    
    [HttpGet]
    public ActionResult<DateParseResponse> ParseEstimatedDate(string estimatedReleaseDate)
    {
        var dates = TimeFunctions.ParseEstimatedReleaseDate(estimatedReleaseDate, _clock);
        var response = new DateParseResponse(dates.MinimumReleaseDate, dates.MaximumReleaseDate);
        return response;
    }
}
