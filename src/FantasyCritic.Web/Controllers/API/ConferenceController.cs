using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Helpers;
using FantasyCritic.Web.Models.Requests.Conferences;
using FantasyCritic.Web.Models.Responses.Conferences;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Web.Controllers.API;

[Route("api/[controller]/[action]")]
[Authorize]
public class ConferenceController : BaseLeagueController
{
    private readonly IClock _clock;
    private readonly ILogger<ConferenceController> _logger;

    public ConferenceController(IClock clock, ILogger<ConferenceController> logger, FantasyCriticUserManager userManager, FantasyCriticService fantasyCriticService,
        InterLeagueService interLeagueService, LeagueMemberService leagueMemberService, ConferenceService conferenceService)
        : base(userManager, fantasyCriticService, interLeagueService, leagueMemberService, conferenceService)
    {
        _clock = clock;
        _logger = logger;
    }

    [HttpPost]
    [Authorize("Write")]
    [Authorize("PlusUser")]
    public async Task<IActionResult> CreateConference([FromBody] CreateConferenceRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();
        var requestValid = request.IsValid();
        if (requestValid.IsFailure)
        {
            return BadRequest(requestValid.Error);
        }

        var supportedYears = await _interLeagueService.GetSupportedYears();
        var selectedSupportedYear = supportedYears.SingleOrDefault(x => x.Year == request.LeagueYearSettings.Year);
        if (selectedSupportedYear is null)
        {
            return BadRequest("That year is not supported.");
        }

        if (!selectedSupportedYear.OpenForCreation)
        {
            return BadRequest("That year is not open for play.");
        }

        var tagDictionary = await _interLeagueService.GetMasterGameTagDictionary();
        ConferenceCreationParameters domainRequest = request.ToDomain(currentUser, tagDictionary);
        var conference = await _conferenceService.CreateConference(domainRequest);
        if (conference.IsFailure)
        {
            return BadRequest(conference.Error);
        }

        return Ok(conference.Value.ConferenceID);
    }

    [HttpPost]
    [Authorize("Write")]
    [Authorize("PlusUser")]
    public Task<IActionResult> AddNewConferenceYear([FromBody] NewConferenceYearRequest request)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Authorize("Write")]
    [Authorize("PlusUser")]
    public Task<IActionResult> EditConference([FromBody] EditConferenceRequest request)
    {
        throw new NotImplementedException();
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetConference(Guid id)
    {
        var conferenceRecord = await GetExistingConference(id, ConferenceRequiredRelationship.AllowAnonymous);
        if (conferenceRecord.FailedResult is not null)
        {
            return conferenceRecord.FailedResult;
        }

        var validResult = conferenceRecord.ValidResult!;

        var viewModel = new ConferenceViewModel(validResult.Conference, validResult.Relationship.ConferenceManager,
            validResult.Relationship.InConference, validResult.PlayersInConference, validResult.ConferenceLeagues);
        return Ok(viewModel);
    }

    [AllowAnonymous]
    public async Task<IActionResult> GetConferenceYear(Guid conferenceID, int year)
    {
        var conferenceYearRecord = await GetExistingConferenceYear(conferenceID, year, ConferenceRequiredRelationship.AllowAnonymous);
        if (conferenceYearRecord.FailedResult is not null)
        {
            return conferenceYearRecord.FailedResult;
        }

        var validResult = conferenceYearRecord.ValidResult!;
        
        var conferenceLeagues = validResult.ConferenceLeagueYears.Select(x => x.League).ToList();
        var conferenceViewModel = new ConferenceViewModel(validResult.ConferenceYear.Conference, validResult.Relationship.ConferenceManager,
            validResult.Relationship.InConference, validResult.PlayersInConference, conferenceLeagues);

        var conferenceYearViewModel = new ConferenceYearViewModel(conferenceViewModel, validResult.ConferenceYear, validResult.ConferenceLeagueYears);
        return Ok(conferenceYearViewModel);
    }

    public async Task<IActionResult> MyConferences(int? year)
    {
        var currentUser = await GetCurrentUserOrThrow();

        IReadOnlyList<Conference> myConferences = await _conferenceService.GetConferencesForUser(currentUser);

        List<ConferenceViewModel> viewModels = new List<ConferenceViewModel>();
        foreach (var conference in myConferences)
        {
            if (year.HasValue && !conference.Years.Contains(year.Value))
            {
                continue;
            }

            bool isManager = (conference.ConferenceManager.Id == currentUser.Id);
            viewModels.Add(new ConferenceViewModel(conference, isManager, true));
        }

        var sortedViewModels = viewModels.OrderBy(x => x.ConferenceName).ToList();
        return Ok(sortedViewModels);
    }

    [HttpPost]
    [Authorize("Write")]
    [Authorize("PlusUser")]
    public Task<IActionResult> CreateInviteLink([FromBody] CreateConferenceInviteLinkRequest request)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Authorize("Write")]
    [Authorize("PlusUser")]
    public Task<IActionResult> DeleteInviteLink([FromBody] DeleteConferenceInviteLinkRequest request)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Authorize("Write")]
    [Authorize("PlusUser")]
    public Task<IActionResult> JoinWithInviteLink([FromBody] JoinConferenceWithInviteLinkRequest request)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Authorize("Write")]
    [Authorize("PlusUser")]
    public Task<IActionResult> AddLeagueToConference([FromBody] AddLeagueToConferenceRequest request)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Authorize("Write")]
    public Task<IActionResult> PromoteNewConferenceManager([FromBody] PromoteNewConferenceManagerRequest request)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Authorize("Write")]
    [Authorize("PlusUser")]
    public Task<IActionResult> ReassignLeagueManager([FromBody] PromoteNewLeagueManagerWithinConferenceRequest request)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Authorize("Write")]
    [Authorize("PlusUser")]
    public Task<IActionResult> AssignLeaguePlayers([FromBody] AssignLeaguePlayersRequest request)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Authorize("Write")]
    [Authorize("PlusUser")]
    public Task<IActionResult> EditDraftStatusForConferenceYear([FromBody] EditDraftStatusForConferenceYearRequest request)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Authorize("Write")]
    [Authorize("PlusUser")]
    public Task<IActionResult> PostNewConferenceManagerMessage([FromBody] PostNewConferenceManagerMessageRequest request)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Authorize("Write")]
    [Authorize("PlusUser")]
    public Task<IActionResult> DeleteConferenceManagerMessage([FromBody] DeleteConferenceManagerMessageRequest request)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Authorize("Write")]
    public Task<IActionResult> DismissManagerMessage([FromBody] DismissConferenceManagerMessageRequest request)
    {
        throw new NotImplementedException();
    }
}
