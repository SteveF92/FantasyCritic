using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Cache;
using System.Security.Claims;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Extensions;
using FantasyCritic.Web.Models;
using FantasyCritic.Web.Models.Requests;
using FantasyCritic.Web.Models.Requests.Account;
using FantasyCritic.Web.Models.Responses;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace FantasyCritic.Web.Controllers.API
{
    [Route("api/[controller]/[action]")]
    [Authorize]
    public class AccountController : FantasyCriticController
    {
        private readonly FantasyCriticUserManager _userManager;
        private readonly FantasyCriticRoleManager _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly IClock _clock;

        public AccountController(FantasyCriticUserManager userManager, FantasyCriticRoleManager roleManager,
            IEmailSender emailSender, ILogger<AccountController> logger, IClock clock) :
            base(userManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _logger = logger;
            _clock = clock;
        }

        public async Task<ActionResult> CurrentUser()
        {
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            var roles = await _userManager.GetRolesAsync(currentUser);
            FantasyCriticUserViewModel vm = new FantasyCriticUserViewModel(currentUser, roles);
            return Ok(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeDisplayName([FromBody] ChangeDisplayNameRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            currentUser.UserName = request.NewDisplayName;
            currentUser.DisplayNumber = await _userManager.GetOpenDisplayNumber(currentUser.UserName);
            var result = await _userManager.UpdateAsync(currentUser);
            if (!result.Succeeded)
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount()
        {
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            await _userManager.DeleteUserAccount(currentUser);

            return Ok();
        }
    }
}
