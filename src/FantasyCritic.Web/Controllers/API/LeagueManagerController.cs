using FantasyCritic.Lib.Domain.Draft;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Helpers;
using FantasyCritic.Web.Hubs;
using FantasyCritic.Web.Models.Requests.League.Trades;
using FantasyCritic.Web.Models.Requests.LeagueManager;
using FantasyCritic.Web.Models.Requests.Shared;
using FantasyCritic.Web.Models.Responses;
using FantasyCritic.Web.Models.RoundTrip;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace FantasyCritic.Web.Controllers.API;

[Route("api/[controller]/[action]")]
[Authorize]
public class LeagueManagerController : BaseLeagueController
{
    private readonly DraftService _draftService;
    private readonly PublisherService _publisherService;
    private readonly IClock _clock;
    private readonly IHubContext<UpdateHub> _hubContext;
    private readonly EmailSendingService _emailSendingService;
    private readonly GameAcquisitionService _gameAcquisitionService;
    private readonly TradeService _tradeService;

    public LeagueManagerController(FantasyCriticUserManager userManager, FantasyCriticService fantasyCriticService, InterLeagueService interLeagueService,
        LeagueMemberService leagueMemberService, DraftService draftService, PublisherService publisherService, IClock clock, IHubContext<UpdateHub> hubContext,
        EmailSendingService emailSendingService, GameAcquisitionService gameAcquisitionService, TradeService tradeService)
        : base(userManager, fantasyCriticService, interLeagueService, leagueMemberService)
    {
        _draftService = draftService;
        _publisherService = publisherService;
        _clock = clock;
        _hubContext = hubContext;
        _emailSendingService = emailSendingService;
        _gameAcquisitionService = gameAcquisitionService;
        _tradeService = tradeService;
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> CreateLeague([FromBody] CreateLeagueRequest request)
    {
        var currentUserResult = await GetCurrentUser();
        if (currentUserResult.IsFailure)
        {
            return BadRequest(currentUserResult.Error);
        }
        var currentUser = currentUserResult.Value;

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

        var userIsBetaUser = await _userManager.IsInRoleAsync(currentUser, "BetaTester");
        bool yearIsOpen = selectedSupportedYear.OpenForCreation || (userIsBetaUser && selectedSupportedYear.OpenForBetaUsers);
        if (!yearIsOpen)
        {
            return BadRequest("That year is not open for play.");
        }

        var tagDictionary = await _interLeagueService.GetMasterGameTagDictionary();
        LeagueCreationParameters domainRequest = request.ToDomain(currentUser, tagDictionary);
        var league = await _fantasyCriticService.CreateLeague(domainRequest);
        if (league.IsFailure)
        {
            return BadRequest(league.Error);
        }

        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> AvailableYears(Guid id)
    {
        var leagueRecord = await GetExistingLeague(id, RequiredRelationship.LeagueManager);
        if (leagueRecord.FailedResult is not null)
        {
            return leagueRecord.FailedResult;
        }
        var validResult = leagueRecord.ValidResult!;
        var currentUser = validResult.CurrentUser!;
        var league = validResult.League;

        IReadOnlyList<SupportedYear> supportedYears = await _interLeagueService.GetSupportedYears();
        var openYears = supportedYears.Where(x => x.OpenForCreation).Select(x => x.Year);
        var availableYears = openYears.Except(league.Years);

        var userIsBetaUser = await _userManager.IsInRoleAsync(currentUser, "BetaTester");
        if (userIsBetaUser)
        {
            var betaYears = supportedYears.Where(x => x.OpenForBetaUsers).Select(x => x.Year);
            availableYears = availableYears.Concat(betaYears).Distinct();
        }

        return Ok(availableYears);
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> AddNewLeagueYear([FromBody] NewLeagueYearRequest request)
    {
        var leagueRecord = await GetExistingLeague(request.LeagueID, RequiredRelationship.LeagueManager);
        if (leagueRecord.FailedResult is not null)
        {
            return leagueRecord.FailedResult;
        }
        var validResult = leagueRecord.ValidResult!;
        var currentUser = validResult.CurrentUser!;
        var league = validResult.League;

        if (league.Years.Contains(request.Year))
        {
            return BadRequest();
        }

        var supportedYears = await _interLeagueService.GetSupportedYears();
        var selectedSupportedYear = supportedYears.SingleOrDefault(x => x.Year == request.Year);
        if (selectedSupportedYear is null)
        {
            return BadRequest();
        }

        var userIsBetaUser = await _userManager.IsInRoleAsync(currentUser, "BetaTester");
        bool yearIsOpen = selectedSupportedYear.OpenForCreation || (userIsBetaUser && selectedSupportedYear.OpenForBetaUsers);
        if (!yearIsOpen)
        {
            return BadRequest();
        }

        if (!league.Years.Any())
        {
            throw new Exception("League has no initial year.");
        }

        var mostRecentYear = league.Years.Max();
        var mostRecentLeagueYear = await _fantasyCriticService.GetLeagueYear(league.LeagueID, mostRecentYear);
        if (mostRecentLeagueYear is null)
        {
            throw new Exception("Most recent league year could not be found");
        }

        var updatedOptions = mostRecentLeagueYear.Options.UpdateOptionsForYear(request.Year);
        await _fantasyCriticService.AddNewLeagueYear(league, request.Year, updatedOptions, mostRecentLeagueYear);

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> ChangeLeagueOptions([FromBody] ChangeLeagueOptionsRequest request)
    {
        var leagueRecord = await GetExistingLeague(request.LeagueID, RequiredRelationship.LeagueManager);
        if (leagueRecord.FailedResult is not null)
        {
            return leagueRecord.FailedResult;
        }
        var validResult = leagueRecord.ValidResult!;
        var league = validResult.League;

        if (string.IsNullOrWhiteSpace(request.LeagueName))
        {
            return BadRequest("You cannot have a blank league name.");
        }

        bool testLeague = request.TestLeague;
        if (league.TestLeague)
        {
            //Users can't change a test league to a non test.
            testLeague = true;
        }

        await _fantasyCriticService.ChangeLeagueOptions(league, request.LeagueName, request.PublicLeague, testLeague);
        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> EditLeagueYearSettings([FromBody] LeagueYearSettingsViewModel request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Ban, RequiredRelationship.LeagueManager, RequiredYearStatus.AnyYearNotFinished);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var validResult = leagueYearRecord.ValidResult!;
        var currentUser = validResult.CurrentUser!;
        var leagueYear = validResult.LeagueYear;

        var requestValid = request.IsValid();
        if (requestValid.IsFailure)
        {
            return BadRequest(requestValid.Error);
        }

        var tagDictionary = await _interLeagueService.GetMasterGameTagDictionary();
        LeagueYearParameters domainRequest = request.ToDomain(tagDictionary);
        Result result = await _fantasyCriticService.EditLeague(leagueYear, domainRequest);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        await _fantasyCriticService.UpdatePublisherGameCalculatedStats(leagueYear);

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> InvitePlayer([FromBody] CreateInviteRequest request)
    {
        var leagueRecord = await GetExistingLeague(request.LeagueID, RequiredRelationship.LeagueManager);
        if (leagueRecord.FailedResult is not null)
        {
            return leagueRecord.FailedResult;
        }
        var validResult = leagueRecord.ValidResult!;
        var league = validResult.League;

        string baseURL = $"{Request.Scheme}://{Request.Host.Value}";
        FantasyCriticUser inviteUser;
        if (!request.IsDisplayNameInvite())
        {
            if (request.InviteEmail is null)
            {
                return BadRequest();
            }

            string inviteEmail = request.InviteEmail.ToLower();
            inviteUser = await _userManager.FindByEmailAsync(inviteEmail);
            if (inviteUser is null)
            {
                Result userlessInviteResult = await _leagueMemberService.InviteUserByEmail(league, inviteEmail);
                if (userlessInviteResult.IsFailure)
                {
                    return BadRequest(userlessInviteResult.Error);
                }

                await _emailSendingService.SendSiteInviteEmail(inviteEmail, league, baseURL);
                return Ok();
            }
        }
        else
        {
            inviteUser = await _userManager.FindByDisplayName(request.InviteDisplayName!, request.InviteDisplayNumber!.Value);
        }

        if (inviteUser is null)
        {
            return BadRequest("No user is found with that information.");
        }

        Result userInviteResult = await _leagueMemberService.InviteUserByUserID(league, inviteUser);
        if (userInviteResult.IsFailure)
        {
            return BadRequest(userInviteResult.Error);
        }

        await _emailSendingService.SendLeagueInviteEmail(inviteUser.Email, league, baseURL);
        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> CreateInviteLink([FromBody] CreateInviteLinkRequest request)
    {
        var leagueRecord = await GetExistingLeague(request.LeagueID, RequiredRelationship.LeagueManager);
        if (leagueRecord.FailedResult is not null)
        {
            return leagueRecord.FailedResult;
        }
        var validResult = leagueRecord.ValidResult!;
        var league = validResult.League;

        IReadOnlyList<LeagueInviteLink> activeLinks = await _leagueMemberService.GetActiveInviteLinks(league);
        if (activeLinks.Count >= 2)
        {
            return BadRequest("You can't have more than 2 invite links active.");
        }

        await _leagueMemberService.CreateInviteLink(league);

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> DeleteInviteLink([FromBody] DeleteInviteLinkRequest request)
    {
        var leagueRecord = await GetExistingLeague(request.LeagueID, RequiredRelationship.LeagueManager);
        if (leagueRecord.FailedResult is not null)
        {
            return leagueRecord.FailedResult;
        }
        var validResult = leagueRecord.ValidResult!;
        var league = validResult.League;

        var activeLinks = await _leagueMemberService.GetActiveInviteLinks(league);
        var thisLink = activeLinks.SingleOrDefault(x => x.InviteID == request.InviteID);
        if (thisLink is null)
        {
            return BadRequest();
        }

        await _leagueMemberService.DeactivateInviteLink(thisLink);

        return Ok();
    }

    [HttpGet("{leagueID}")]
    public async Task<IActionResult> InviteLinks(Guid leagueID)
    {
        var leagueRecord = await GetExistingLeague(leagueID, RequiredRelationship.LeagueManager);
        if (leagueRecord.FailedResult is not null)
        {
            return leagueRecord.FailedResult;
        }
        var validResult = leagueRecord.ValidResult!;
        var league = validResult.League;

        int currentYear = league.Years.Max();
        string baseURL = $"{Request.Scheme}://{Request.Host.Value}";
        IReadOnlyList<LeagueInviteLink> activeLinks = await _leagueMemberService.GetActiveInviteLinks(league);
        var viewModels = activeLinks.Select(x => new LeagueInviteLinkViewModel(x, currentYear, baseURL));
        return Ok(viewModels);
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> RescindInvite([FromBody] DeleteInviteRequest request)
    {
        var leagueRecord = await GetExistingLeague(request.LeagueID, RequiredRelationship.LeagueManager);
        if (leagueRecord.FailedResult is not null)
        {
            return leagueRecord.FailedResult;
        }
        var validResult = leagueRecord.ValidResult!;

        LeagueInvite? invite = await _leagueMemberService.GetInvite(request.InviteID);
        if (invite is null)
        {
            return BadRequest();
        }

        if (invite.League.LeagueID != validResult.League.LeagueID)
        {
            return Forbid();
        }

        await _leagueMemberService.DeleteInvite(invite);
        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> RemovePlayer([FromBody] PlayerRemoveRequest request)
    {
        var leagueRecord = await GetExistingLeague(request.LeagueID, RequiredRelationship.LeagueManager);
        if (leagueRecord.FailedResult is not null)
        {
            return leagueRecord.FailedResult;
        }
        var validResult = leagueRecord.ValidResult!;
        var league = validResult.League;

        if (league.LeagueManager.Id == request.UserID)
        {
            return BadRequest("Can't remove the league manager.");
        }

        var removeUser = await _userManager.FindByIdAsync(request.UserID.ToString());
        if (removeUser == null)
        {
            return BadRequest();
        }

        var playersInLeague = await _leagueMemberService.GetUsersInLeague(league);
        bool userIsInLeague = playersInLeague.Any(x => x.Id == removeUser.Id);
        if (!userIsInLeague)
        {
            return BadRequest("That user is not in that league.");
        }

        await _leagueMemberService.FullyRemovePlayerFromLeague(league, removeUser);

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> CreatePublisherForUser([FromBody] CreatePublisherForUserRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Allow, RequiredRelationship.LeagueManager, RequiredYearStatus.YearNotFinishedDraftNotStarted);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var validResult = leagueYearRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;

        if (string.IsNullOrWhiteSpace(request.PublisherName))
        {
            return BadRequest("You cannot have a blank name.");
        }

        var userToCreate = await _userManager.FindByIdAsync(request.UserID.ToString());
        if (userToCreate == null)
        {
            return BadRequest();
        }

        bool userIsActive = await _leagueMemberService.UserIsActiveInLeagueYear(leagueYear.League, request.Year, userToCreate);
        if (!userIsActive)
        {
            return BadRequest();
        }

        var publisherForUser = leagueYear.GetUserPublisher(userToCreate);
        if (publisherForUser is not null)
        {
            return BadRequest("That player already has a publisher for this this league/year.");
        }

        await _publisherService.CreatePublisher(leagueYear, userToCreate, request.PublisherName);
        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> EditPublisher([FromBody] PublisherEditRequest request)
    {
        var leagueYearPublisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Ban, RequiredRelationship.LeagueManager, RequiredYearStatus.AnyYearNotFinished);
        if (leagueYearPublisherRecord.FailedResult is not null)
        {
            return leagueYearPublisherRecord.FailedResult;
        }
        var validResult = leagueYearPublisherRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var publisher = validResult.Publisher;

        var editValues = request.ToDomain(leagueYear, publisher);
        Result result = await _publisherService.EditPublisher(editValues);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> RemovePublisher([FromBody] PublisherRemoveRequest request)
    {
        var leagueYearPublisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Allow, RequiredRelationship.LeagueManager, RequiredYearStatus.YearNotFinishedDraftNotStarted);
        if (leagueYearPublisherRecord.FailedResult is not null)
        {
            return leagueYearPublisherRecord.FailedResult;
        }
        var validResult = leagueYearPublisherRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var publisher = validResult.Publisher;

        await _publisherService.FullyRemovePublisher(leagueYear, publisher);

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> SetPlayerActiveStatus([FromBody] LeaguePlayerActiveRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Allow, RequiredRelationship.LeagueManager, RequiredYearStatus.YearNotFinishedDraftNotStarted);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var validResult = leagueYearRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;

        Dictionary<FantasyCriticUser, bool> userActiveStatus = new Dictionary<FantasyCriticUser, bool>();
        foreach (var userKeyValue in request.ActiveStatus)
        {
            var domainUser = await _userManager.FindByIdAsync(userKeyValue.Key.ToString());
            if (domainUser == null)
            {
                return BadRequest();
            }

            var publisherForUser = leagueYear.GetUserPublisher(domainUser);
            if (publisherForUser is not null && !userKeyValue.Value)
            {
                return BadRequest("You must remove a player's publisher before you can set them as inactive.");
            }

            userActiveStatus.Add(domainUser, userKeyValue.Value);
        }

        var result = await _leagueMemberService.SetPlayerActiveStatus(leagueYear, userActiveStatus);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> SetAutoDraft([FromBody] ManagerSetAutoDraftRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Allow, RequiredRelationship.LeagueManager, RequiredYearStatus.YearNotFinishedDraftNotFinished);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var validResult = leagueYearRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;

        foreach (var requestPublisher in request.PublisherAutoDraft)
        {
            var publisher = leagueYear.GetPublisherByID(requestPublisher.Key);
            if (publisher is null)
            {
                return Forbid();
            }

            await _publisherService.SetAutoDraft(publisher, requestPublisher.Value);
        }

        var draftComplete = await _draftService.RunAutoDraftAndCheckIfComplete(leagueYear);
        if (draftComplete)
        {
            await _hubContext.Clients.Group(leagueYear.GetGroupName).SendAsync("DraftFinished");
        }

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> ManagerClaimGame([FromBody] ClaimGameRequest request)
    {
        var leagueYearPublisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Ban, RequiredRelationship.LeagueManager, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (leagueYearPublisherRecord.FailedResult is not null)
        {
            return leagueYearPublisherRecord.FailedResult;
        }
        var validResult = leagueYearPublisherRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var publisher = validResult.Publisher;

        MasterGame? masterGame = null;
        if (request.MasterGameID.HasValue)
        {
            masterGame = await _interLeagueService.GetMasterGame(request.MasterGameID.Value);
        }

        bool counterPickedGameIsManualWillNotRelease = PlayerGameExtensions.CounterPickedGameIsManualWillNotRelease(leagueYear, request.CounterPick, masterGame, false);
        ClaimGameDomainRequest domainRequest = new ClaimGameDomainRequest(leagueYear, publisher, request.GameName, request.CounterPick, counterPickedGameIsManualWillNotRelease, request.ManagerOverride, false, masterGame, null, null);
        ClaimResult result = await _gameAcquisitionService.ClaimGame(domainRequest, true, false, false);
        var viewModel = new ManagerClaimResultViewModel(result);

        await _fantasyCriticService.UpdatePublisherGameCalculatedStats(leagueYear);
        return Ok(viewModel);
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> ManagerAssociateGame([FromBody] AssociateGameRequest request)
    {
        var leagueYearPublisherGameRecord = await GetExistingLeagueYearAndPublisherGame(request.PublisherID, request.PublisherGameID, ActionProcessingModeBehavior.Ban, RequiredRelationship.LeagueManager, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (leagueYearPublisherGameRecord.FailedResult is not null)
        {
            return leagueYearPublisherGameRecord.FailedResult;
        }
        var validResult = leagueYearPublisherGameRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var publisher = validResult.Publisher;
        var publisherGame = validResult.PublisherGame;

        MasterGame? masterGame = await _interLeagueService.GetMasterGame(request.MasterGameID);
        if (masterGame is null)
        {
            return BadRequest();
        }

        AssociateGameDomainRequest domainRequest = new AssociateGameDomainRequest(leagueYear, publisher, publisherGame, masterGame, request.ManagerOverride);

        ClaimResult result = await _gameAcquisitionService.AssociateGame(domainRequest);
        var viewModel = new ManagerClaimResultViewModel(result);

        await _fantasyCriticService.UpdatePublisherGameCalculatedStats(leagueYear);

        return Ok(viewModel);
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> RemovePublisherGame([FromBody] GameRemoveRequest request)
    {
        var leagueYearPublisherGameRecord = await GetExistingLeagueYearAndPublisherGame(request.PublisherID, request.PublisherGameID, ActionProcessingModeBehavior.Ban, RequiredRelationship.LeagueManager, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (leagueYearPublisherGameRecord.FailedResult is not null)
        {
            return leagueYearPublisherGameRecord.FailedResult;
        }
        var validResult = leagueYearPublisherGameRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var publisher = validResult.Publisher;
        var publisherGame = validResult.PublisherGame;

        await _publisherService.RemovePublisherGame(leagueYear, publisher, publisherGame);
        await _fantasyCriticService.UpdatePublisherGameCalculatedStats(leagueYear);

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public Task<IActionResult> ManuallyScorePublisherGame([FromBody] ManualPublisherGameScoreRequest request)
    {
        return UpdateManualCriticScore(request.PublisherID, request.PublisherGameID, request.ManualCriticScore);
    }

    [HttpPost]
    [Authorize("Write")]
    public Task<IActionResult> RemoveManualPublisherGameScore([FromBody] RemoveManualPublisherGameScoreRequest request)
    {
        return UpdateManualCriticScore(request.PublisherID, request.PublisherGameID, null);
    }

    private async Task<IActionResult> UpdateManualCriticScore(Guid publisherID, Guid publisherGameID, decimal? manualCriticScore)
    {
        var leagueYearPublisherGameRecord = await GetExistingLeagueYearAndPublisherGame(publisherID, publisherGameID, ActionProcessingModeBehavior.Ban, RequiredRelationship.LeagueManager, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (leagueYearPublisherGameRecord.FailedResult is not null)
        {
            return leagueYearPublisherGameRecord.FailedResult;
        }
        var validResult = leagueYearPublisherGameRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var publisherGame = validResult.PublisherGame;

        await _fantasyCriticService.ManuallyScoreGame(publisherGame, manualCriticScore);
        await _fantasyCriticService.UpdatePublisherGameCalculatedStats(leagueYear);

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> ManuallySetWillNotRelease([FromBody] ManualPublisherGameWillNotReleaseRequest request)
    {
        var leagueYearPublisherGameRecord = await GetExistingLeagueYearAndPublisherGame(request.PublisherID, request.PublisherGameID, ActionProcessingModeBehavior.Ban, RequiredRelationship.LeagueManager, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (leagueYearPublisherGameRecord.FailedResult is not null)
        {
            return leagueYearPublisherGameRecord.FailedResult;
        }
        var validResult = leagueYearPublisherGameRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var publisherGame = validResult.PublisherGame;

        await _fantasyCriticService.ManuallySetWillNotRelease(leagueYear, publisherGame, request.WillNotRelease);

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> StartDraft([FromBody] StartDraftRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Allow, RequiredRelationship.LeagueManager, RequiredYearStatus.PlayOpenDraftNotStarted);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var validResult = leagueYearRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var activeUsers = await _leagueMemberService.GetActivePlayersForLeagueYear(leagueYear.League, request.Year);

        var completePlayStatus = new CompletePlayStatus(leagueYear, activeUsers, validResult.Relationship.LeagueManager);
        if (!completePlayStatus.ReadyToDraft)
        {
            return BadRequest();
        }

        var draftComplete = await _draftService.StartDraft(leagueYear);
        await _hubContext.Clients.Group(leagueYear.GetGroupName).SendAsync("RefreshLeagueYear");

        if (draftComplete)
        {
            await _hubContext.Clients.Group(leagueYear.GetGroupName).SendAsync("DraftFinished");
        }

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> ResetDraft([FromBody] ResetDraftRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Allow, RequiredRelationship.LeagueManager, RequiredYearStatus.DuringDraft);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var validResult = leagueYearRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;

        await _draftService.ResetDraft(leagueYear);
        await _hubContext.Clients.Group(leagueYear.GetGroupName).SendAsync("RefreshLeagueYear");

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> SetDraftOrder([FromBody] DraftOrderRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Allow, RequiredRelationship.LeagueManager, RequiredYearStatus.YearNotFinishedDraftNotStarted);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var validResult = leagueYearRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;

        var activeUsers = await _leagueMemberService.GetActivePlayersForLeagueYear(leagueYear.League, request.Year);
        var completePlayStatus = new CompletePlayStatus(leagueYear, activeUsers, validResult.Relationship.LeagueManager);
        if (!completePlayStatus.ReadyToSetDraftOrder)
        {
            return BadRequest();
        }

        var draftOrderType = DraftOrderType.TryFromValue(request.DraftOrderType);
        if (draftOrderType is null)
        {
            return BadRequest();
        }

        LeagueYear? previousLeagueYear = null;
        if (draftOrderType.Equals(DraftOrderType.InverseStandings))
        {
            previousLeagueYear = await _fantasyCriticService.GetLeagueYear(request.LeagueID, request.Year - 1);
        }

        var draftPositions = DraftFunctions.GetDraftPositions(leagueYear, draftOrderType, request.ManualPublisherDraftPositions, previousLeagueYear);
        if (draftPositions.IsFailure)
        {
            return BadRequest(draftPositions.Error);
        }

        var result = await _draftService.SetDraftOrder(leagueYear, draftOrderType, draftPositions.Value);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> ManagerDraftGame([FromBody] ManagerDraftGameRequest request)
    {
        var leagueYearPublisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Allow, RequiredRelationship.LeagueManager, RequiredYearStatus.ActiveDraft);
        if (leagueYearPublisherRecord.FailedResult is not null)
        {
            return leagueYearPublisherRecord.FailedResult;
        }
        var validResult = leagueYearPublisherRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var publisher = validResult.Publisher;

        var draftStatus = DraftFunctions.GetDraftStatus(leagueYear);
        if (draftStatus is null)
        {
            return BadRequest("Draft is not active.");
        }

        if (!draftStatus.NextDraftPublisher.Equals(publisher))
        {
            return BadRequest("That publisher is not next up for drafting.");
        }

        MasterGame? masterGame = null;
        if (request.MasterGameID.HasValue)
        {
            masterGame = await _interLeagueService.GetMasterGame(request.MasterGameID.Value);
        }

        if (draftStatus.DraftPhase.Equals(DraftPhase.StandardGames))
        {
            if (request.CounterPick)
            {
                return BadRequest("Not drafting counterPicks now.");
            }
        }

        if (draftStatus.DraftPhase.Equals(DraftPhase.CounterPicks))
        {
            if (!request.CounterPick)
            {
                return BadRequest("Not drafting standard games now.");
            }
        }

        bool counterPickedGameIsManualWillNotRelease = PlayerGameExtensions.CounterPickedGameIsManualWillNotRelease(leagueYear, request.CounterPick, masterGame, false);
        ClaimGameDomainRequest domainRequest = new ClaimGameDomainRequest(leagueYear, publisher, request.GameName, request.CounterPick, counterPickedGameIsManualWillNotRelease, request.ManagerOverride, false,
            masterGame, draftStatus.DraftPosition, draftStatus.OverallDraftPosition);

        var result = await _draftService.DraftGame(domainRequest, true);
        var viewModel = new ManagerClaimResultViewModel(result.Result);
        await _hubContext.Clients.Group(leagueYear.GetGroupName).SendAsync("RefreshLeagueYear");

        if (result.DraftComplete)
        {
            await _hubContext.Clients.Group(leagueYear.GetGroupName).SendAsync("DraftFinished");
        }

        return Ok(viewModel);
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> SetDraftPause([FromBody] DraftPauseRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Allow, RequiredRelationship.LeagueManager, RequiredYearStatus.DuringDraft);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var validResult = leagueYearRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;

        await _draftService.SetDraftPause(leagueYear, request.Pause);
        await _hubContext.Clients.Group(leagueYear.GetGroupName).SendAsync("RefreshLeagueYear");

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> UndoLastDraftAction([FromBody] UndoLastDraftActionRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Allow, RequiredRelationship.LeagueManager, RequiredYearStatus.DraftPaused);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var validResult = leagueYearRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;

        bool hasGames = leagueYear.Publishers.Any(x => x.PublisherGames.Any());
        if (!hasGames)
        {
            return BadRequest("Can't undo a drafted game if no games have been drafted.");
        }

        await _draftService.UndoLastDraftAction(leagueYear);
        await _hubContext.Clients.Group(leagueYear.GetGroupName).SendAsync("RefreshLeagueYear");

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> SetGameEligibilityOverride([FromBody] EligibilityOverrideRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Ban, RequiredRelationship.LeagueManager, RequiredYearStatus.AnyYearNotFinished);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var validResult = leagueYearRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;

        MasterGame? masterGame = await _interLeagueService.GetMasterGame(request.MasterGameID);
        if (masterGame is null)
        {
            return BadRequest();
        }

        if (masterGame.ReleaseDate.HasValue && masterGame.ReleaseDate.Value.Year < leagueYear.Year)
        {
            return BadRequest("You can't change the override setting of a game that came out in a previous year.");
        }

        await _fantasyCriticService.SetEligibilityOverride(leagueYear, masterGame, request.Eligible);
        var refreshedLeagueYear = await _fantasyCriticService.GetLeagueYear(leagueYear.League.LeagueID, request.Year);
        if (refreshedLeagueYear is null)
        {
            return BadRequest();
        }
        await _fantasyCriticService.UpdatePublisherGameCalculatedStats(refreshedLeagueYear);

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> SetGameTagOverride([FromBody] TagOverrideRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Ban, RequiredRelationship.LeagueManager, RequiredYearStatus.AnyYearNotFinished);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var validResult = leagueYearRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;

        MasterGame? masterGame = await _interLeagueService.GetMasterGame(request.MasterGameID);
        if (masterGame is null)
        {
            return BadRequest();
        }

        if (masterGame.ReleaseDate.HasValue && masterGame.ReleaseDate.Value.Year < leagueYear.Year)
        {
            return BadRequest("You can't override the tags of a game that came out in a previous year.");
        }

        IReadOnlyList<MasterGameTag> currentOverrideTags = await _fantasyCriticService.GetTagOverridesForGame(leagueYear.League, leagueYear.Year, masterGame);

        var allTags = await _interLeagueService.GetMasterGameTags();
        var requestedTags = allTags.Where(x => request.Tags.Contains(x.Name)).ToList();
        if (ListExtensions.SequencesContainSameElements(masterGame.Tags, requestedTags))
        {
            return BadRequest("That game already has those exact tags.");
        }

        if (ListExtensions.SequencesContainSameElements(currentOverrideTags, requestedTags))
        {
            return BadRequest("That game is already overriden to have those exact tags.");
        }

        await _fantasyCriticService.SetTagOverride(leagueYear, masterGame, requestedTags);
        var refreshedLeagueYear = await _fantasyCriticService.GetLeagueYear(leagueYear.League.LeagueID, request.Year);
        if (refreshedLeagueYear is null)
        {
            return BadRequest();
        }
        await _fantasyCriticService.UpdatePublisherGameCalculatedStats(refreshedLeagueYear);

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> PromoteNewLeagueManager([FromBody] PromoteNewLeagueManagerRequest request)
    {
        var leagueRecord = await GetExistingLeague(request.LeagueID, RequiredRelationship.LeagueManager);
        if (leagueRecord.FailedResult is not null)
        {
            return leagueRecord.FailedResult;
        }
        var validResult = leagueRecord.ValidResult!;
        var league = validResult.League;

        var newManager = await _userManager.FindByIdAsync(request.NewManagerUserID.ToString());
        var usersInLeague = await _leagueMemberService.GetUsersInLeague(league);
        if (!usersInLeague.Contains(newManager))
        {
            return BadRequest();
        }

        await _leagueMemberService.TransferLeagueManager(league, newManager);

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> PostNewManagerMessage([FromBody] PostNewManagerMessageRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Allow, RequiredRelationship.LeagueManager, RequiredYearStatus.Any);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var validResult = leagueYearRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;

        await _fantasyCriticService.PostNewManagerMessage(leagueYear, request.Message, request.IsPublic);

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> DeleteManagerMessage([FromBody] DeleteManagerMessageRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Allow, RequiredRelationship.LeagueManager, RequiredYearStatus.Any);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var validResult = leagueYearRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;

        Result result = await _fantasyCriticService.DeleteManagerMessage(leagueYear, request.MessageID);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> RejectTrade([FromBody] BasicTradeRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Allow, RequiredRelationship.LeagueManager, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }

        var trade = await _tradeService.GetTrade(request.TradeID);
        if (trade is null)
        {
            return BadRequest();
        }

        Result result = await _tradeService.RejectTradeByManager(trade);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> ExecuteTrade([FromBody] BasicTradeRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Ban, RequiredRelationship.LeagueManager, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }

        var trade = await _tradeService.GetTrade(request.TradeID);
        if (trade is null)
        {
            return BadRequest();
        }

        Result result = await _tradeService.ExecuteTrade(trade);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> CreateSpecialAuction([FromBody] CreateSpecialAuctionRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Ban, RequiredRelationship.LeagueManager, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }

        var masterGameYear = await _interLeagueService.GetMasterGameYear(request.MasterGameID, request.Year);
        if (masterGameYear is null)
        {
            return BadRequest();
        }

        Result result = await _gameAcquisitionService.CreateSpecialAuction(leagueYearRecord.ValidResult!.LeagueYear, masterGameYear, request.ScheduledEndTime);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> CancelSpecialAuction([FromBody] CancelSpecialAuctionRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Ban, RequiredRelationship.LeagueManager, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }

        var specialAuctionForGame = await _gameAcquisitionService.GetActiveSpecialAuctionForGame(leagueYearRecord.ValidResult!.LeagueYear, request.MasterGameID);
        if (specialAuctionForGame is null)
        {
            return BadRequest();
        }

        Result result = await _gameAcquisitionService.CancelSpecialAuction(leagueYearRecord.ValidResult!.LeagueYear, specialAuctionForGame);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }
}
