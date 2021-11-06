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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AccountController : ControllerBase
    {
        private readonly FantasyCriticUserManager _userManager;
        private readonly FantasyCriticRoleManager _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly IClock _clock;

        public AccountController(FantasyCriticUserManager userManager, FantasyCriticRoleManager roleManager,
            IEmailSender emailSender, ILogger<AccountController> logger, IClock clock)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _logger = logger;
            _clock = clock;
        }

        public async Task<ActionResult> CurrentUser()
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
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

            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                return BadRequest();
            }

            user.UserName = request.NewDisplayName;
            user.DisplayNumber = await _userManager.GetOpenDisplayNumber(user.UserName);
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                return BadRequest();
            }

            await _userManager.DeleteUserAccount(user);

            return Ok();
        }
    }
}
