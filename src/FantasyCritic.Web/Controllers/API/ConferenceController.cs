using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.Web.Helpers;
using FantasyCritic.Web.Hubs;
using FantasyCritic.Web.Models.Requests.Conferences;
using FantasyCritic.Web.Models.Responses.Conferences;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
        InterLeagueService interLeagueService, LeagueMemberService leagueMemberService, ConferenceService conferenceService, EnvironmentConfiguration environmentConfiguration,
        DiscordPushService discordPushService, IHubContext<UpdateHub> hubContext)
        : base(userManager, fantasyCriticService, interLeagueService, leagueMemberService, conferenceService, discordPushService, hubContext)
    {
        _clock = clock;
        _logger = logger;
        _environmentConfiguration = environmentConfiguration;
    }

    public async Task<IActionResult> MyConferences()
    {
        var currentUser = await GetCurrentUserOrThrow();
        var conferences = await _conferenceService.GetConferencesForUser(currentUser);

        var viewModels = conferences
            .Select(conference => new MinimalConferenceViewModel(conference, conference.ConferenceManager.UserID == currentUser.UserID))
            .OrderBy(x => x.ConferenceName)
            .ToList();

        return Ok(viewModels);
    }

    [HttpPost]
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
        ConferenceCreationParameters domainRequest = request.ToDomain(currentUser.ToMinimal(), tagDictionary);
        var conference = await _conferenceService.CreateConference(domainRequest);
        if (conference.IsFailure)
        {
            return BadRequest(conference.Error);
        }

        return Ok(conference.Value.ConferenceID);
    }

    [HttpPost]
    [Authorize("PlusUser")]
    public async Task<IActionResult> AddLeagueToConference([FromBody] AddLeagueToConferenceRequest request)
    {
        var requestValid = request.IsValid();
        if (requestValid.IsFailure)
        {
            return BadRequest(requestValid.Error);
        }

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

    [HttpGet("{id}")]
    public async Task<IActionResult> AvailableYears(Guid id)
    {
        var leagueRecord = await GetExistingConference(id, ConferenceRequiredRelationship.ConferenceManager);
        if (leagueRecord.FailedResult is not null)
        {
            return leagueRecord.FailedResult;
        }
        var validResult = leagueRecord.ValidResult!;
        var currentUser = validResult.CurrentUser!;
        var conference = validResult.Conference;

        IReadOnlyList<SupportedYear> supportedYears = await _interLeagueService.GetSupportedYears();
        var openYears = supportedYears.Where(x => x.OpenForCreation).Select(x => x.Year);
        var availableYears = openYears.Except(conference.Years);

        var userIsBetaUser = await _userManager.IsInRoleAsync(currentUser, "BetaTester");
        if (userIsBetaUser)
        {
            var betaYears = supportedYears.Where(x => x.OpenForBetaUsers).Select(x => x.Year);
            availableYears = availableYears.Concat(betaYears).Distinct();
        }

        return Ok(availableYears);
    }

    [HttpPost]
    [Authorize("PlusUser")]
    public async Task<IActionResult> AddNewConferenceYear([FromBody] NewConferenceYearRequest request)
    {
        var conferenceRecord = await GetExistingConference(request.ConferenceID, ConferenceRequiredRelationship.ConferenceManager);
        if (conferenceRecord.FailedResult is not null)
        {
            return conferenceRecord.FailedResult;
        }
        var validResult = conferenceRecord.ValidResult!;

        if (validResult.Conference.Years.Contains(request.Year))
        {
            return BadRequest();
        }

        var supportedYears = await _interLeagueService.GetSupportedYears();
        var selectedSupportedYear = supportedYears.SingleOrDefault(x => x.Year == request.Year);
        if (selectedSupportedYear is null)
        {
            return BadRequest("That year is not supported.");
        }

        if (!selectedSupportedYear.OpenForCreation)
        {
            return BadRequest("That year is not open for creating new leagues.");
        }

        var newYearResult = await _conferenceService.AddNewConferenceYear(validResult.Conference, request.Year);
        if (newYearResult.IsFailure)
        {
            return BadRequest(newYearResult.Error);
        }

        return Ok();
    }

    [HttpPost]
    [Authorize("PlusUser")]
    public async Task<IActionResult> AddNewLeagueYear([FromBody] AddNewLeagueYearRequest request)
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

        var league = validResult.ConferenceLeagues.SingleOrDefault(x => x.LeagueID == request.LeagueID);
        if (league is null)
        {
            return BadRequest("That league is not in that conference");
        }

        var leagueRecord = await GetExistingLeague(league.LeagueID, RequiredRelationship.LoggedIn);
        if (leagueRecord.FailedResult is not null)
        {
            return leagueRecord.FailedResult;
        }

        if (leagueRecord.ValidResult!.League.Years.Contains(request.Year))
        {
            return BadRequest($"That league has already been renewed for {request.Year}");
        }

        var newLeagueResult = await _conferenceService.AddNewLeagueYear(validResult.Conference, request.Year, leagueRecord.ValidResult.League);
        if (newLeagueResult.IsFailure)
        {
            return BadRequest(newLeagueResult.Error);
        }

        return Ok();
    }

    [HttpPost]
    [Authorize("PlusUser")]
    public async Task<IActionResult> SetPlayerActiveStatus([FromBody] ConferencePlayerActiveRequest request)
    {
        var conferenceYearRecord = await GetExistingConferenceYear(request.ConferenceID, request.Year, ConferenceRequiredRelationship.ConferenceManager);
        if (conferenceYearRecord.FailedResult is not null)
        {
            return conferenceYearRecord.FailedResult;
        }

        var result = await _conferenceService.SetPlayerActiveStatus(conferenceYearRecord.ValidResult!.ConferenceYear, request.ActiveStatus);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
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
        var conferenceYearRecord = await GetExistingConferenceYearWithSupplementalData(conferenceID, year, ConferenceRequiredRelationship.AllowAnonymous);
        if (conferenceYearRecord.FailedResult is not null)
        {
            return conferenceYearRecord.FailedResult;
        }

        var validResult = conferenceYearRecord.ValidResult!;

        var conferenceLeagues = validResult.ConferenceLeagueYears.Select(x => new ConferenceLeague(x.League.LeagueID, x.League.LeagueName, x.League.LeagueManager)).ToList();
        var conferenceViewModel = new ConferenceViewModel(validResult.ConferenceYear.Conference, validResult.Relationship.ConferenceManager,
            validResult.Relationship.InConference, validResult.PlayersInConference, conferenceLeagues);

        var conferenceYearViewModel = new ConferenceYearViewModel(conferenceViewModel, validResult.ConferenceYear, validResult.ConferenceLeagueYears,
            validResult.PlayersInConference, validResult.CurrentUser, validResult.ConferenceYearStandings, validResult.ManagerMessages);
        return Ok(conferenceYearViewModel);
    }

    [HttpPost]
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
    [Authorize("PlusUser")]
    public async Task<IActionResult> RemovePlayerFromConference([FromBody] RemovePlayerFromConferenceRequest request)
    {
        var conferenceRecord = await GetExistingConference(request.ConferenceID, ConferenceRequiredRelationship.ConferenceManager);
        if (conferenceRecord.FailedResult is not null)
        {
            return conferenceRecord.FailedResult;
        }

        var validResult = conferenceRecord.ValidResult!;
        var conference = validResult.Conference;

        var removeUser = await _userManager.FindByIdAsync(request.UserID.ToString());
        if (removeUser is null)
        {
            return BadRequest("That user does not exist.");
        }

        var removeResult = await _conferenceService.RemovePlayerFromConference(conference, removeUser);
        if (removeResult.IsFailure)
        {
            return BadRequest(removeResult.Error);
        }

        return Ok();
    }

    [HttpPost]
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
    [Authorize("PlusUser")]
    public async Task<IActionResult> AssignLeaguePlayers([FromBody] AssignLeaguePlayersRequest request)
    {
        var conferenceRecord = await GetExistingConference(request.ConferenceID, ConferenceRequiredRelationship.ConferenceManager);
        if (conferenceRecord.FailedResult is not null)
        {
            return conferenceRecord.FailedResult;
        }

        var validResult = conferenceRecord.ValidResult!;

        var conferenceYear = await _conferenceService.GetConferenceYear(validResult.Conference.ConferenceID, request.Year);
        if (conferenceYear is null)
        {
            throw new Exception($"Something went wrong with conference years for conference: {request.ConferenceID} and year: {request.Year}");
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

            foreach (var userID in assignment.Value)
            {
                var user = userDictionary.GetValueOrDefault(userID);
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

        }

        var assignResult = await _conferenceService.AssignLeaguePlayers(conferenceYear, leaguesInConference, userAssignments.SealDictionary());
        if (assignResult.IsFailure)
        {
            return BadRequest(assignResult.Error);
        }

        return Ok();
    }

    [HttpPost]
    [Authorize("PlusUser")]
    public async Task<IActionResult> SetConferenceLeagueLockStatus([FromBody] SetConferenceLeagueLockStatusRequest request)
    {
        var conferenceYearRecord = await GetExistingConferenceYear(request.ConferenceID, request.Year, ConferenceRequiredRelationship.ConferenceManager);
        if (conferenceYearRecord.FailedResult is not null)
        {
            return conferenceYearRecord.FailedResult;
        }

        var validConferenceResult = conferenceYearRecord.ValidResult!;

        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Allow,
            RequiredRelationship.AllowAnonymous, RequiredYearStatus.AnyYearNotFinished);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }

        var validLeagueYearResult = leagueYearRecord.ValidResult!;
        if (validLeagueYearResult.LeagueYear.League.ConferenceID != validConferenceResult.ConferenceYear.Conference.ConferenceID)
        {
            return StatusCode(403, "That league is not in that conference.");
        }

        if (validLeagueYearResult.LeagueYear.PlayStatus.PlayStarted && !request.Locked)
        {
            return BadRequest("You cannot unlock a league that has already started their draft.");
        }

        if (validLeagueYearResult.LeagueYear.ConferenceLocked.HasValue && validLeagueYearResult.LeagueYear.ConferenceLocked.Value && request.Locked)
        {
            return BadRequest("That league is already locked.");
        }

        if (validLeagueYearResult.LeagueYear.ConferenceLocked.HasValue && !validLeagueYearResult.LeagueYear.ConferenceLocked.Value && !request.Locked)
        {
            return BadRequest("That league is already unlocked.");
        }

        await _conferenceService.SetConferenceLeagueLockStatus(validLeagueYearResult.LeagueYear, request.Locked);
        return Ok();
    }

    [HttpPost]
    [Authorize("PlusUser")]
    public async Task<IActionResult> PostNewConferenceManagerMessage([FromBody] PostNewConferenceManagerMessageRequest request)
    {
        var conferenceYearRecord = await GetExistingConferenceYear(request.ConferenceID, request.Year, ConferenceRequiredRelationship.ConferenceManager);
        if (conferenceYearRecord.FailedResult is not null)
        {
            return conferenceYearRecord.FailedResult;
        }

        var validResult = conferenceYearRecord.ValidResult!;
        var conferenceYear = validResult.ConferenceYear;

        await _conferenceService.PostNewManagerMessage(conferenceYear, request.Message, request.IsPublic);

        return Ok();
    }

    [HttpPost]
    [Authorize("PlusUser")]
    public async Task<IActionResult> DeleteConferenceManagerMessage([FromBody] DeleteConferenceManagerMessageRequest request)
    {
        var conferenceYearRecord = await GetExistingConferenceYear(request.ConferenceID, request.Year, ConferenceRequiredRelationship.ConferenceManager);
        if (conferenceYearRecord.FailedResult is not null)
        {
            return conferenceYearRecord.FailedResult;
        }

        var validResult = conferenceYearRecord.ValidResult!;
        var conferenceYear = validResult.ConferenceYear;

        Result result = await _conferenceService.DeleteManagerMessage(conferenceYear, request.MessageID);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> DismissManagerMessage([FromBody] DismissConferenceManagerMessageRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();

        Result result = await _conferenceService.DismissManagerMessage(request.MessageID, currentUser.Id);
        if (result.IsFailure)
        {
            return BadRequest();
        }

        return Ok();
    }
}
