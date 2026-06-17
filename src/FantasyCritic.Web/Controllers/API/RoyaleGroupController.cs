using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Royale;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Models.Requests.Royale;
using FantasyCritic.Web.Models.Responses.Royale;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FantasyCritic.Web.Controllers.API;

[Route("api/[controller]/[action]")]
[Authorize]
public class RoyaleGroupController : FantasyCriticController
{
    private readonly IClock _clock;
    private readonly RoyaleService _royaleService;
    private readonly FantasyCriticService _fantasyCriticService;
    private readonly ConferenceService _conferenceService;
    private readonly InterLeagueService _interLeagueService;

    public RoyaleGroupController(IClock clock, FantasyCriticUserManager userManager, RoyaleService royaleService,
        FantasyCriticService fantasyCriticService, ConferenceService conferenceService, InterLeagueService interLeagueService) : base(userManager)
    {
        _clock = clock;
        _royaleService = royaleService;
        _fantasyCriticService = fantasyCriticService;
        _conferenceService = conferenceService;
        _interLeagueService = interLeagueService;
    }

    [HttpGet("{groupID}")]
    [AllowAnonymous]
    [ProducesResponseType<RoyaleGroupViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoyaleGroupViewModel>> GetRoyaleGroup(Guid groupID)
    {
        var group = await _royaleService.GetRoyaleGroup(groupID);
        if (group is null)
        {
            return NotFound();
        }

        var members = await _royaleService.GetRoyaleGroupMembers(group);
        var vm = new RoyaleGroupViewModel(group, members.Count);
        return vm;
    }

    [HttpGet("{groupID}/{year}/{quarter}")]
    [AllowAnonymous]
    [ProducesResponseType<RoyaleGroupQuarterViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoyaleGroupQuarterViewModel>> GetRoyaleGroupQuarter(Guid groupID, int year, int quarter)
    {
        var group = await _royaleService.GetRoyaleGroupMemberDisplayRows(groupID, year, quarter);
        if (group is null)
        {
            return NotFound();
        }

        var allMasterGameTags = await _interLeagueService.GetMasterGameTags();
        var vm = new RoyaleGroupQuarterViewModel(group.RoyaleGroup, year, quarter, group.DisplayRows, _clock.GetToday(), allMasterGameTags, _clock);
        return vm;
    }

    [HttpPost]
    [ProducesResponseType<CreatedRoyaleGroupViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreatedRoyaleGroupViewModel>> CreateManualRoyaleGroup([FromBody] CreateManualRoyaleGroupRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();

        if (string.IsNullOrWhiteSpace(request.GroupName))
        {
            return BadRequest("You cannot have a blank group name.");
        }

        var group = await _royaleService.CreateManualRoyaleGroup(currentUser.ToVeryMinimal(), request.GroupName);
        return new CreatedRoyaleGroupViewModel(group.GroupID);
    }

    [HttpPost]
    [ProducesResponseType<CreatedRoyaleGroupViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CreatedRoyaleGroupViewModel>> CreateLeagueTiedRoyaleGroup([FromBody] CreateLeagueTiedRoyaleGroupRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();

        if (string.IsNullOrWhiteSpace(request.GroupName))
        {
            return BadRequest("You cannot have a blank group name.");
        }

        var league = await _fantasyCriticService.GetLeagueByID(request.LeagueID);
        if (league is null)
        {
            return BadRequest("League does not exist.");
        }

        if (league.LeagueManager.UserID != currentUser.Id)
        {
            return StatusCode(403);
        }

        var result = await _royaleService.CreateLeagueTiedRoyaleGroup(currentUser.ToVeryMinimal(), request.GroupName, request.LeagueID);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return new CreatedRoyaleGroupViewModel(result.Value.GroupID);
    }

    [HttpPost]
    [ProducesResponseType<CreatedRoyaleGroupViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CreatedRoyaleGroupViewModel>> CreateConferenceTiedRoyaleGroup([FromBody] CreateConferenceTiedRoyaleGroupRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();

        if (string.IsNullOrWhiteSpace(request.GroupName))
        {
            return BadRequest("You cannot have a blank group name.");
        }

        var conference = await _conferenceService.GetConference(request.ConferenceID);
        if (conference is null)
        {
            return BadRequest("Conference does not exist.");
        }

        if (conference.ConferenceManager.UserID != currentUser.Id)
        {
            return StatusCode(403);
        }

        var result = await _royaleService.CreateConferenceTiedRoyaleGroup(currentUser.ToVeryMinimal(), request.GroupName, request.ConferenceID);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return new CreatedRoyaleGroupViewModel(result.Value.GroupID);
    }

    [HttpPost("{groupID}")]
    [ProducesResponseType<RoyaleGroupInviteLinkViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RoyaleGroupInviteLinkViewModel>> CreateGroupInviteLink(Guid groupID)
    {
        var currentUser = await GetCurrentUserOrThrow();
        var result = await _royaleService.CreateGroupInviteLink(groupID, currentUser);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        var vm = new RoyaleGroupInviteLinkViewModel(result.Value);
        return vm;
    }

    [HttpPost("{inviteID}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeactivateGroupInviteLink(Guid inviteID)
    {
        var currentUser = await GetCurrentUserOrThrow();
        var result = await _royaleService.DeactivateGroupInviteLink(inviteID, currentUser);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> JoinWithInviteLink([FromBody] JoinRoyaleGroupWithInviteLinkRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();
        var result = await _royaleService.JoinRoyaleGroupViaInviteLink(request.InviteCode, currentUser);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost("{groupID}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LeaveGroup(Guid groupID)
    {
        var currentUser = await GetCurrentUserOrThrow();
        var result = await _royaleService.LeaveRoyaleGroup(groupID, currentUser);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveMember([FromBody] RemoveRoyaleGroupMemberRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();
        var result = await _royaleService.RemoveMemberFromRoyaleGroup(request.GroupID, request.UserID, currentUser);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<RoyaleGroupViewModel>>> SearchRoyaleGroups([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            return new List<RoyaleGroupViewModel>();
        }

        var groups = await _royaleService.SearchRoyaleGroups(query);
        var viewModels = new List<RoyaleGroupViewModel>();
        foreach (var group in groups)
        {
            var members = await _royaleService.GetRoyaleGroupMembers(group);
            viewModels.Add(new RoyaleGroupViewModel(group, members.Count));
        }
        return viewModels;
    }

    [HttpGet]
    public async Task<ActionResult<List<RoyaleGroupViewModel>>> GetGroupsForUser()
    {
        var currentUser = await GetCurrentUserOrThrow();
        var groups = await _royaleService.GetRoyaleGroupsForUser(currentUser.Id);
        var viewModels = new List<RoyaleGroupViewModel>();
        foreach (var group in groups)
        {
            var members = await _royaleService.GetRoyaleGroupMembers(group);
            viewModels.Add(new RoyaleGroupViewModel(group, members.Count));
        }
        return viewModels;
    }

    [HttpGet("{groupID}")]
    [ProducesResponseType<List<RoyaleGroupInviteLinkViewModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<RoyaleGroupInviteLinkViewModel>>> GetGroupInviteLinks(Guid groupID)
    {
        var currentUser = await GetCurrentUserOrThrow();
        var group = await _royaleService.GetRoyaleGroup(groupID);
        if (group is null)
        {
            return NotFound();
        }

        if (group.Manager?.UserID != currentUser.Id)
        {
            return StatusCode(403);
        }

        var links = await _royaleService.GetGroupInviteLinks(groupID);
        var viewModels = links.Select(x => new RoyaleGroupInviteLinkViewModel(x)).ToList();
        return viewModels;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<RoyaleGroupViewModel>>> GetRulesBasedGroups()
    {
        var groups = await _royaleService.GetAllRoyaleGroupsByType(RoyaleGroupType.RulesBased);
        var viewModels = new List<RoyaleGroupViewModel>();
        foreach (var group in groups)
        {
            var members = await _royaleService.GetRoyaleGroupMembers(group);
            viewModels.Add(new RoyaleGroupViewModel(group, members.Count));
        }
        return viewModels;
    }

    [HttpGet("{groupID}")]
    [AllowAnonymous]
    [ProducesResponseType<RoyaleGroupDataViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoyaleGroupDataViewModel>> GetRoyaleGroupData(Guid groupID)
    {
        var stats = await _royaleService.GetRoyaleGroupMembersWithLifetimeStats(groupID);
        if (stats is null)
        {
            return NotFound();
        }

        var group = new RoyaleGroupViewModel(stats.RoyaleGroup, stats.LifetimeStats.Count);
        var members = stats.LifetimeStats.Select(x => new RoyaleGroupMemberListItemViewModel(x)).ToList();
        var activeQuarter = await _royaleService.GetActiveYearQuarter();
        var vm = new RoyaleGroupDataViewModel(group, members, new RoyaleYearQuarterViewModel(activeQuarter));
        return vm;
    }

    [HttpGet("{leagueID}")]
    [AllowAnonymous]
    public async Task<ActionResult<RoyaleGroupStatusViewModel>> GetRoyaleGroupForLeague(Guid leagueID)
    {
        var group = await _royaleService.GetRoyaleGroupForLeague(leagueID);
        if (group is null)
        {
            return new RoyaleGroupStatusViewModel(false, null);
        }

        var members = await _royaleService.GetRoyaleGroupMembers(group);
        var vm = new RoyaleGroupViewModel(group, members.Count);
        return new RoyaleGroupStatusViewModel(true, vm);
    }

    [HttpGet("{conferenceID}")]
    [AllowAnonymous]
    public async Task<ActionResult<RoyaleGroupStatusViewModel>> GetRoyaleGroupForConference(Guid conferenceID)
    {
        var group = await _royaleService.GetRoyaleGroupForConference(conferenceID);
        if (group is null)
        {
            return new RoyaleGroupStatusViewModel(false, null);
        }

        var members = await _royaleService.GetRoyaleGroupMembers(group);
        var vm = new RoyaleGroupViewModel(group, members.Count);
        return new RoyaleGroupStatusViewModel(true, vm);
    }
}
