using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using FantasyCritic.SharedSerialization.API;
using FantasyCritic.Web.Models.Requests.MasterGame;
using FantasyCritic.Web.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FantasyCritic.Web.Controllers.API;

[Route("api/[controller]/[action]")]
public class GameController : FantasyCriticController
{
    private readonly InterLeagueService _interLeagueService;
    private readonly FantasyCriticService _fantasyCriticService;
    private readonly GameSearchingService _gameSearchingService;
    private readonly IClock _clock;

    public GameController(FantasyCriticUserManager userManager, InterLeagueService interLeagueService,
        FantasyCriticService fantasyCriticService, GameSearchingService gameSearchingService, IClock clock)
        : base(userManager)
    {
        _interLeagueService = interLeagueService;
        _fantasyCriticService = fantasyCriticService;
        _gameSearchingService = gameSearchingService;
        _clock = clock;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MasterGameViewModel>> MasterGame(Guid id)
    {
        var masterGame = await _interLeagueService.GetMasterGame(id);
        if (masterGame is null)
        {
            return NotFound();
        }

        var numberOutstandingCorrections = await _interLeagueService.GetNumberOutstandingCorrections(masterGame);

        var currentDate = _clock.GetToday();
        var viewModel = new MasterGameViewModel(masterGame, currentDate, numberOutstandingCorrections: numberOutstandingCorrections);
        return viewModel;
    }

    public async Task<ActionResult<List<MasterGameViewModel>>> MasterGame()
    {
        IReadOnlyList<MasterGame> masterGames = await _interLeagueService.GetMasterGames();
        var currentDate = _clock.GetToday();
        var viewModels = masterGames.Select(x => new MasterGameViewModel(x, currentDate)).ToList();
        return viewModels;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<List<MasterGameChangeLogEntryViewModel>>> MasterGameChangeLog(Guid id)
    {
        var masterGame = await _interLeagueService.GetMasterGame(id);
        if (masterGame is null)
        {
            return NotFound();
        }

        var changes = await _interLeagueService.GetMasterGameChangeLog(masterGame);

        var viewModels = changes.Select(x => new MasterGameChangeLogEntryViewModel(x)).ToList();
        return viewModels;
    }

    [HttpGet("{id}/{year}")]
    public async Task<ActionResult<MasterGameYearViewModel>> MasterGameYear(Guid id, int year)
    {
        MasterGameYear? masterGame = await _interLeagueService.GetMasterGameYear(id, year);
        if (masterGame is null)
        {
            return NotFound();
        }

        var currentDate = _clock.GetToday();
        var viewModel = new MasterGameYearViewModel(masterGame, currentDate);
        return viewModel;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<List<MasterGameYearViewModel>>> MasterGameYears(Guid id)
    {
        List<MasterGameYear> masterGameYears = new List<MasterGameYear>();
        var supportedYears = await _interLeagueService.GetSupportedYears();
        foreach (var supportedYear in supportedYears)
        {
            MasterGameYear? masterGameYear = await _interLeagueService.GetMasterGameYear(id, supportedYear.Year);
            if (masterGameYear is null)
            {
                continue;
            }

            if (masterGameYear.PercentStandardGame == 0)
            {
                continue;
            }

            masterGameYears.Add(masterGameYear);
        }

        var currentDate = _clock.GetToday();
        var viewModels = masterGameYears.Select(x => new MasterGameYearViewModel(x, currentDate)).ToList();
        return viewModels;
    }

    [HttpGet("{year}")]
    public async Task<ActionResult<List<MasterGameYearViewModel>>> MasterGameYear(int year)
    {
        IReadOnlyList<MasterGameYear> masterGames = await _interLeagueService.GetMasterGameYears(year);
        var relevantGames = masterGames.Where(x => !x.MasterGame.ReleaseDate.HasValue || x.MasterGame.ReleaseDate.Value.Year >= year);

        var supportedYears = await GetSupportedYears();
        var finishedYears = supportedYears.Where(x => x.Finished);
        bool thisYearIsFinished = finishedYears.Any(x => x.Year == year);
        if (thisYearIsFinished)
        {
            var chosenGames = await _interLeagueService.GetAllSelectedMasterGameIDsForYear(year);
            relevantGames = masterGames.Where(x => chosenGames.Contains(x.MasterGame.MasterGameID));
        }

        var currentDate = _clock.GetToday();
        List<MasterGameYearViewModel> viewModels = relevantGames.Select(x => new MasterGameYearViewModel(x, currentDate)).ToList();

        return viewModels;
    }

    [HttpGet("{year}")]
    [Authorize("PlusUser")]
    public async Task<ActionResult<List<PossibleMasterGameYearViewModel>>> MasterGameYearInLeagueContext(int year, Guid leagueID)
    {
        var currentUser = await GetCurrentUserOrThrow();
        LeagueYear? leagueYear = await _fantasyCriticService.GetLeagueYear(leagueID, year);
        if (leagueYear is null)
        {
            return BadRequest();
        }

        var userPublisher = leagueYear.GetUserPublisher(currentUser);
        var possibleMasterGames = await _gameSearchingService.GetAllPossibleMasterGameYearsForLeagueYear(leagueYear, userPublisher, year);
        var currentDate = _clock.GetToday();
        var viewModels = possibleMasterGames.Select(x => new PossibleMasterGameYearViewModel(x, currentDate)).ToList();
        return viewModels;
    }

    [HttpPost]
    public async Task<IActionResult> CreateMasterGameRequest([FromBody] MasterGameRequestRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();
        MasterGameRequest domainRequest = request.ToDomain(currentUser, _clock.GetCurrentInstant());

        await _interLeagueService.CreateMasterGameRequest(domainRequest);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> CreateMasterGameChangeRequest([FromBody] MasterGameChangeRequestRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();
        MasterGame? masterGame = await _interLeagueService.GetMasterGame(request.MasterGameID);
        if (masterGame is null)
        {
            return NotFound();
        }

        MasterGameChangeRequest domainRequest = request.ToDomain(currentUser, _clock.GetCurrentInstant(), masterGame);
        await _interLeagueService.CreateMasterGameChangeRequest(domainRequest);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> DeleteMasterGameRequest([FromBody] MasterGameRequestDeletionRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();

        MasterGameRequest? domainRequest = await _interLeagueService.GetMasterGameRequest(request.RequestID);
        if (domainRequest is null)
        {
            return BadRequest("That request does not exist.");
        }

        if (domainRequest.User.Id != currentUser.Id)
        {
            return StatusCode(403);
        }

        await _interLeagueService.DeleteMasterGameRequest(domainRequest);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> DeleteMasterGameChangeRequest([FromBody] MasterGameChangeRequestDeletionRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();

        MasterGameChangeRequest? domainRequest = await _interLeagueService.GetMasterGameChangeRequest(request.RequestID);
        if (domainRequest is null)
        {
            return BadRequest("That request does not exist.");
        }

        if (domainRequest.User.Id != currentUser.Id)
        {
            return StatusCode(403);
        }

        await _interLeagueService.DeleteMasterGameChangeRequest(domainRequest);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> DismissMasterGameRequest([FromBody] MasterGameRequestDismissRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();

        MasterGameRequest? domainRequest = await _interLeagueService.GetMasterGameRequest(request.RequestID);
        if (domainRequest is null)
        {
            return BadRequest("That request does not exist.");
        }

        if (domainRequest.User.Id != currentUser.Id)
        {
            return StatusCode(403);
        }

        await _interLeagueService.DismissMasterGameRequest(domainRequest);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> DismissMasterGameChangeRequest([FromBody] MasterGameChangeRequestDismissRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();

        MasterGameChangeRequest? domainRequest = await _interLeagueService.GetMasterGameChangeRequest(request.RequestID);
        if (domainRequest is null)
        {
            return BadRequest("That request does not exist.");
        }

        if (domainRequest.User.Id != currentUser.Id)
        {
            return StatusCode(403);
        }

        await _interLeagueService.DismissMasterGameChangeRequest(domainRequest);
        return Ok();
    }

    [Authorize]
    public async Task<ActionResult<List<MasterGameRequestViewModel>>> MyMasterGameRequests()
    {
        var currentUser = await GetCurrentUserOrThrow();

        IReadOnlyList<MasterGameRequest> requests = await _interLeagueService.GetMasterGameRequestsForUser(currentUser);

        var currentDate = _clock.GetToday();
        var viewModels = requests.Select(x => new MasterGameRequestViewModel(x, currentDate)).ToList();
        return viewModels;
    }

    [Authorize]
    public async Task<ActionResult<List<MasterGameChangeRequestViewModel>>> MyMasterGameChangeRequests()
    {
        var currentUser = await GetCurrentUserOrThrow();

        IReadOnlyList<MasterGameChangeRequest> requests = await _interLeagueService.GetMasterGameChangeRequestsForUser(currentUser);

        var currentDate = _clock.GetToday();
        var viewModels = requests.Select(x => new MasterGameChangeRequestViewModel(x, currentDate)).ToList();
        return viewModels;
    }

    public async Task<ActionResult<List<SupportedYearViewModel>>> SupportedYears()
    {
        var supportedYears = await GetSupportedYears();
        return supportedYears.Select(x => new SupportedYearViewModel(x)).ToList();
    }

    private async Task<IReadOnlyList<SupportedYear>> GetSupportedYears()
    {
        IReadOnlyList<SupportedYear> supportedYears = await _interLeagueService.GetSupportedYears();
        var years = supportedYears.Where(x => x.Year > 2017).OrderByDescending(x => x);
        return years.ToList();
    }

    public async Task<ActionResult<List<MasterGameTagViewModel>>> GetMasterGameTags()
    {
        var domains = await _interLeagueService.GetMasterGameTags();
        var vms = domains.Select(x => new MasterGameTagViewModel(x)).ToList();
        return vms;
    }

    public async Task<ActionResult<List<CompleteMasterGameChangeViewModel>>> GetRecentMasterGameChanges()
    {
        IReadOnlyList<MasterGameChangeLogEntry> recentChanges = await _interLeagueService.GetRecentMasterGameChanges();
        var currentDate = _clock.GetToday();
        var vms = recentChanges.Select(x => new CompleteMasterGameChangeViewModel(x, currentDate)).ToList();
        return vms;
    }

    public async Task<ActionResult<List<LocalDate>>> GetProcessingDatesForTopBidsAndDrops()
    {
        var processingDatesWithData = await _interLeagueService.GetProcessingDatesForTopBidsAndDrops();
        return processingDatesWithData.ToList();
    }

    public async Task<ActionResult<TopBidsAndDropsSetViewModel>> GetTopBidsAndDrops(LocalDate? processDate)
    {
        LocalDate dateToUse;
        if (processDate.HasValue)
        {
            dateToUse = processDate.Value;
        }
        else
        {
            var processingDatesWithData = await _interLeagueService.GetProcessingDatesForTopBidsAndDrops();
            if (!processingDatesWithData.Any())
            {
                return NotFound();
            }

            dateToUse = processingDatesWithData.Max();
        }

        var topBidsAndDrops = await _interLeagueService.GetTopBidsAndDrops(dateToUse);
        var currentDate = _clock.GetToday();
        var viewModels = topBidsAndDrops.Select(x => new TopBidsAndDropsGameViewModel(x, currentDate)).ToList();
        return new TopBidsAndDropsSetViewModel(viewModels, dateToUse);
    }

    [HttpGet("{masterGameID}")]
    public async Task<IActionResult> LeagueYearsWithMasterGame(Guid masterGameID)
    {
        var currentUserResult = await GetCurrentUser();
        var leagueYearsWithMasterGame = await _interLeagueService.GetLeagueYearsWithMasterGame(currentUserResult.Value.Id, masterGameID);
        var viewModels = leagueYearsWithMasterGame.Select(l =>
            new LeagueYearWithMasterGameViewModel(l.LeagueID, l.LeagueName, l.Year, l.IsCounterPick)).ToList();
        return Ok(viewModels);
    }
}
