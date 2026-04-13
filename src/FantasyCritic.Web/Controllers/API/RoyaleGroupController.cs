using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Royale;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Models.Requests.Royale;
using FantasyCritic.Web.Models.Responses.Royale;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FantasyCritic.Web.Controllers.API;

[Route("api/[controller]/[action]")]
[Authorize]
public class RoyaleGroupController : FantasyCriticController
{
    private readonly RoyaleService _royaleService;
    private readonly FantasyCriticService _fantasyCriticService;

    public RoyaleGroupController(FantasyCriticUserManager userManager, RoyaleService royaleService,
        FantasyCriticService fantasyCriticService) : base(userManager)
    {
        _royaleService = royaleService;
        _fantasyCriticService = fantasyCriticService;
    }

    [AllowAnonymous]
    [HttpGet("{groupID}")]
    public async Task<IActionResult> GetRoyaleGroup(Guid groupID)
    {
        var group = await _royaleService.GetRoyaleGroup(groupID);
        if (group is null)
        {
            return NotFound();
        }

        var members = await _royaleService.GetRoyaleGroupMembers(group);
        var vm = new RoyaleGroupViewModel(group, members.Count);
        return Ok(vm);
    }

    [AllowAnonymous]
    [HttpGet("{groupID}/{year}/{quarter}")]
    public async Task<IActionResult> GetRoyaleGroupQuarter(Guid groupID, int year, int quarter)
    {
        var group = await _royaleService.GetRoyaleGroup(groupID);
        if (group is null)
        {
            return NotFound();
        }

        var memberRows = await _royaleService.GetRoyaleGroupMemberDisplayRows(group, year, quarter);
        var vm = new RoyaleGroupQuarterViewModel(group, year, quarter, memberRows);
        return Ok(vm);
    }

    [HttpPost]
    public async Task<IActionResult> CreateManualRoyaleGroup([FromBody] CreateManualRoyaleGroupRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();

        if (string.IsNullOrWhiteSpace(request.GroupName))
        {
            return BadRequest("You cannot have a blank group name.");
        }

        var group = await _royaleService.CreateManualRoyaleGroup(currentUser.ToVeryMinimal(), request.GroupName);
        return Ok(new { group.GroupID });
    }

    [HttpPost]
    public async Task<IActionResult> CreateLeagueTiedRoyaleGroup([FromBody] CreateLeagueTiedRoyaleGroupRequest request)
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

        return Ok(new { result.Value.GroupID });
    }

    [HttpPost("{groupID}")]
    public async Task<IActionResult> CreateGroupInviteLink(Guid groupID)
    {
        var currentUser = await GetCurrentUserOrThrow();
        var result = await _royaleService.CreateGroupInviteLink(groupID, currentUser);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        var vm = new RoyaleGroupInviteLinkViewModel(result.Value);
        return Ok(vm);
    }

    [HttpPost("{inviteID}")]
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

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> SearchRoyaleGroups([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            return Ok(new List<RoyaleGroupViewModel>());
        }

        var groups = await _royaleService.SearchRoyaleGroups(query);
        var viewModels = new List<RoyaleGroupViewModel>();
        foreach (var group in groups)
        {
            var members = await _royaleService.GetRoyaleGroupMembers(group);
            viewModels.Add(new RoyaleGroupViewModel(group, members.Count));
        }
        return Ok(viewModels);
    }

    [HttpGet]
    public async Task<IActionResult> GetGroupsForUser()
    {
        var currentUser = await GetCurrentUserOrThrow();
        var groups = await _royaleService.GetRoyaleGroupsForUser(currentUser.Id);
        var viewModels = new List<RoyaleGroupViewModel>();
        foreach (var group in groups)
        {
            var members = await _royaleService.GetRoyaleGroupMembers(group);
            viewModels.Add(new RoyaleGroupViewModel(group, members.Count));
        }
        return Ok(viewModels);
    }

    [HttpGet("{groupID}")]
    public async Task<IActionResult> GetGroupInviteLinks(Guid groupID)
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
        return Ok(viewModels);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetRulesBasedGroups()
    {
        var groups = await _royaleService.GetAllRoyaleGroupsByType(RoyaleGroupType.RulesBased);
        var viewModels = new List<RoyaleGroupViewModel>();
        foreach (var group in groups)
        {
            var members = await _royaleService.GetRoyaleGroupMembers(group);
            viewModels.Add(new RoyaleGroupViewModel(group, members.Count));
        }
        return Ok(viewModels);
    }

    [AllowAnonymous]
    [HttpGet("{groupID}")]
    public async Task<IActionResult> GetRoyaleGroupMembers(Guid groupID)
    {
        var group = await _royaleService.GetRoyaleGroup(groupID);
        if (group is null)
        {
            return NotFound();
        }

        var members = await _royaleService.GetRoyaleGroupMembers(group);
        var viewModels = members.Select(m => new { m.UserID, m.DisplayName }).ToList();
        return Ok(viewModels);
    }

    [AllowAnonymous]
    [HttpGet("{leagueID}")]
    public async Task<IActionResult> GetRoyaleGroupForLeague(Guid leagueID)
    {
        var group = await _royaleService.GetRoyaleGroupForLeague(leagueID);
        if (group is null)
        {
            return Ok(new { HasRoyaleGroup = false });
        }

        var members = await _royaleService.GetRoyaleGroupMembers(group);
        var vm = new RoyaleGroupViewModel(group, members.Count);
        return Ok(new { HasRoyaleGroup = true, RoyaleGroup = vm });
    }
}
