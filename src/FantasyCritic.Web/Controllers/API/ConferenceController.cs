using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.Utilities;
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
    private readonly EnvironmentConfiguration _environmentConfiguration;

    public ConferenceController(IClock clock, ILogger<ConferenceController> logger, FantasyCriticUserManager userManager, FantasyCriticService fantasyCriticService,
        InterLeagueService interLeagueService, LeagueMemberService leagueMemberService, ConferenceService conferenceService, EnvironmentConfiguration environmentConfiguration)
        : base(userManager, fantasyCriticService, interLeagueService, leagueMemberService, conferenceService)
    {
        _clock = clock;
        _logger = logger;
        _environmentConfiguration = environmentConfiguration;
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
    public async Task<IActionResult> AddLeagueToConference([FromBody] AddLeagueToConferenceRequest request)
    {
        var conferenceRecord = await GetExistingConference(request.ConferenceID, ConferenceRequiredRelationship.ConferenceManager);
        if (conferenceRecord.FailedResult is not null)
        {
            return conferenceRecord.FailedResult;
        }
        var validResult = conferenceRecord.ValidResult!;

        var supportedYears = await _interLeagueService.GetSupportedYears();
        var selectedSupportedYear = supportedYears.SingleOrDefault(x => x.Year == request.Year);
        if (selectedSupportedYear is null)
        {
            return BadRequest("That year is not supported.");
        }

        if (!selectedSupportedYear.OpenForCreation)
        {
            return BadRequest("That year is not open for play.");
        }

        var leagueManager = await _userManager.FindByIdAsync(request.LeagueManager.ToString());
        if (leagueManager is null)
        {
            return BadRequest("Desired league manager does not exist.");
        }

        var newLeagueResult = await _conferenceService.AddLeagueToConference(validResult.Conference, request.Year, request.LeagueName, leagueManager);
        if (newLeagueResult.IsFailure)
        {
            return BadRequest(newLeagueResult.Error);
        }

        return Ok();
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
    public async Task<IActionResult> EditConference([FromBody] EditConferenceRequest request)
    {
        var conferenceRecord = await GetExistingConference(request.ConferenceID, ConferenceRequiredRelationship.ConferenceManager);
        if (conferenceRecord.FailedResult is not null)
        {
            return conferenceRecord.FailedResult;
        }

        var validResult = conferenceRecord.ValidResult!;

        if (string.IsNullOrWhiteSpace(request.ConferenceName))
        {
            return BadRequest("You cannot have a blank conference name.");
        }

        bool currentlyAffectsStats = !validResult.Conference.CustomRulesConference;
        bool requestedToAffectStats = !request.CustomRulesConference;
        if (!currentlyAffectsStats && requestedToAffectStats)
        {
            return BadRequest("You cannot convert a conference from a conference that does not affect the site's stats into one that does. Contact us for assistance if you believe this is a special case.");
        }

        await _conferenceService.EditConference(validResult.Conference, request.ConferenceName, request.CustomRulesConference);
        return Ok();
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

        var conferenceYearViewModel = new ConferenceYearViewModel(conferenceViewModel, validResult.ConferenceYear, validResult.ConferenceLeagueYears, validResult.LeaguesThatUserIsIn);
        return Ok(conferenceYearViewModel);
    }

    [HttpPost]
    [Authorize("Write")]
    [Authorize("PlusUser")]
    public async Task<IActionResult> CreateInviteLink([FromBody] CreateConferenceInviteLinkRequest request)
    {
        var conferenceRecord = await GetExistingConference(request.ConferenceID, ConferenceRequiredRelationship.ConferenceManager);
        if (conferenceRecord.FailedResult is not null)
        {
            return conferenceRecord.FailedResult;
        }

        var validResult = conferenceRecord.ValidResult!;

        IReadOnlyList<ConferenceInviteLink> activeLinks = await _conferenceService.GetActiveInviteLinks(validResult.Conference);
        if (activeLinks.Count >= 2)
        {
            return BadRequest("You can't have more than 2 invite links active.");
        }

        await _conferenceService.CreateInviteLink(validResult.Conference);
        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    [Authorize("PlusUser")]
    public async Task<IActionResult> DeleteInviteLink([FromBody] DeleteConferenceInviteLinkRequest request)
    {
        var conferenceRecord = await GetExistingConference(request.ConferenceID, ConferenceRequiredRelationship.ConferenceManager);
        if (conferenceRecord.FailedResult is not null)
        {
            return conferenceRecord.FailedResult;
        }

        var validResult = conferenceRecord.ValidResult!;

        var activeLinks = await _conferenceService.GetActiveInviteLinks(validResult.Conference);
        var thisLink = activeLinks.SingleOrDefault(x => x.InviteID == request.InviteID);
        if (thisLink is null)
        {
            return BadRequest();
        }

        await _conferenceService.DeactivateInviteLink(thisLink);
        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> JoinWithInviteLink([FromBody] JoinConferenceWithInviteLinkRequest request)
    {
        var conferenceRecord = await GetExistingConference(request.ConferenceID, ConferenceRequiredRelationship.AllowAnonymous);
        if (conferenceRecord.FailedResult is not null)
        {
            return conferenceRecord.FailedResult;
        }

        var validResult = conferenceRecord.ValidResult!;
        var currentUser = validResult.CurrentUser!;

        var inviteLink = await _conferenceService.GetInviteLinkByInviteCode(request.InviteCode);
        if (inviteLink is null)
        {
            return BadRequest();
        }

        if (inviteLink.Conference.ConferenceID != request.ConferenceID)
        {
            return BadRequest();
        }

        Result result = await _conferenceService.AcceptInviteLink(inviteLink, currentUser);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpGet("{conferenceID}")]
    public async Task<IActionResult> InviteLinks(Guid conferenceID)
    {
        var conferenceRecord = await GetExistingConference(conferenceID, ConferenceRequiredRelationship.ConferenceManager);
        if (conferenceRecord.FailedResult is not null)
        {
            return conferenceRecord.FailedResult;
        }
        var validResult = conferenceRecord.ValidResult!;
        var conference = validResult.Conference;

        int currentYear = conference.Years.Max();
        IReadOnlyList<ConferenceInviteLink> activeLinks = await _conferenceService.GetActiveInviteLinks(conference);
        var viewModels = activeLinks.Select(x => new ConferenceInviteLinkViewModel(x, currentYear, _environmentConfiguration.BaseAddress));
        return Ok(viewModels);
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> PromoteNewConferenceManager([FromBody] PromoteNewConferenceManagerRequest request)
    {
        var conferenceRecord = await GetExistingConference(request.ConferenceID, ConferenceRequiredRelationship.ConferenceManager);
        if (conferenceRecord.FailedResult is not null)
        {
            return conferenceRecord.FailedResult;
        }

        var validResult = conferenceRecord.ValidResult!;
        var conference = validResult.Conference;

        var newManager = await _userManager.FindByIdAsync(request.NewManagerUserID.ToString());
        if (newManager is null)
        {
            return BadRequest("That user does not exist.");
        }

        var newManagerIsPlusUser = await _userManager.IsInRoleAsync(newManager, "PlusUser");
        if (!newManagerIsPlusUser)
        {
            return BadRequest("Conference managers must be Fantasy Critic Plus subscribers.");
        }

        var transferResult = await _conferenceService.TransferConferenceManager(conference, newManager);
        if (transferResult.IsFailure)
        {
            return BadRequest(transferResult.Error);
        }

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    [Authorize("PlusUser")]
    public async Task<IActionResult> ReassignLeagueManager([FromBody] PromoteNewLeagueManagerWithinConferenceRequest request)
    {
        var conferenceRecord = await GetExistingConference(request.ConferenceID, ConferenceRequiredRelationship.ConferenceManager);
        if (conferenceRecord.FailedResult is not null)
        {
            return conferenceRecord.FailedResult;
        }

        var validResult = conferenceRecord.ValidResult!;
        var conference = validResult.Conference;

        var newManager = await _userManager.FindByIdAsync(request.NewManagerUserID.ToString());
        if (newManager is null)
        {
            return BadRequest("That user does not exist.");
        }
        
        var transferResult = await _conferenceService.ReassignLeagueManager(conference, request.LeagueID, newManager);
        if (transferResult.IsFailure)
        {
            return BadRequest(transferResult.Error);
        }

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    [Authorize("PlusUser")]
    public async Task<IActionResult> AssignLeaguePlayers([FromBody] AssignLeaguePlayersRequest request)
    {
        var conferenceRecord = await GetExistingConference(request.ConferenceID, ConferenceRequiredRelationship.ConferenceManager);
        if (conferenceRecord.FailedResult is not null)
        {
            return conferenceRecord.FailedResult;
        }

        var validResult = conferenceRecord.ValidResult!;

        var mostRecentYear = validResult.Conference.Years.Max();
        var conferenceYear = await _conferenceService.GetConferenceYear(validResult.Conference.ConferenceID, mostRecentYear);
        if (conferenceYear is null)
        {
            throw new Exception($"Something went wrong with conference years for conference: {request.ConferenceID} and year: {mostRecentYear}");
        }

        var allUsersInConference = await _conferenceService.GetUsersInConference(validResult.Conference);
        var userDictionary = allUsersInConference.ToDictionary(x => x.Id);

        var leaguesInConference = await _conferenceService.GetLeaguesInConference(validResult.Conference);
        var leagueDictionary = leaguesInConference.ToDictionary(x => x.LeagueID);

        Dictionary<ConferenceLeague, List<FantasyCriticUser>> userAssignments = new Dictionary<ConferenceLeague, List<FantasyCriticUser>>();
        foreach (var assignment in request.LeagueAssignments)
        {
            var league = leagueDictionary.GetValueOrDefault(assignment.Key);
            if (league is null)
            {
                return BadRequest("One or more of the requested leagues is not in the conference.");
            }

            var user = userDictionary.GetValueOrDefault(assignment.Value);
            if (user is null)
            {
                return BadRequest("One or more of the requested users is not in the conference.");
            }

            if (userAssignments.ContainsKey(league))
            {
                userAssignments[league].Add(user);
            }
            else
            {
                userAssignments.Add(league, new List<FantasyCriticUser> { user });
            }
        }
        
        var assignResult = await _conferenceService.AssignLeaguePlayers(conferenceYear, userAssignments.SealDictionary());
        if (assignResult.IsFailure)
        {
            return BadRequest(assignResult.Error);
        }

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    [Authorize("PlusUser")]
    public async Task<IActionResult> EditDraftStatusForConferenceYear([FromBody] EditDraftStatusForConferenceYearRequest request)
    {
        var conferenceYearRecord = await GetExistingConferenceYear(request.ConferenceID, request.Year, ConferenceRequiredRelationship.ConferenceManager);
        if (conferenceYearRecord.FailedResult is not null)
        {
            return conferenceYearRecord.FailedResult;
        }

        var validResult = conferenceYearRecord.ValidResult!;

        if (request.OpenForDrafting && validResult.ConferenceYear.OpenForDrafting)
        {
            return BadRequest("Conference is already open for drafting.");
        }

        if (!request.OpenForDrafting && !validResult.ConferenceYear.OpenForDrafting)
        {
            return BadRequest("Conference is already closed for drafting.");
        }

        var supportedYear = await _interLeagueService.GetSupportedYear(request.Year);
        if (!supportedYear.OpenForPlay && request.OpenForDrafting)
        {
            return BadRequest($"This year is not yet open for play. It will become available on {supportedYear.StartDate}.");
        }

        await _conferenceService.EditDraftStatusForConferenceYear(validResult.ConferenceYear, request.OpenForDrafting);
        return Ok();
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
