using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Cache;
using System.Security.Claims;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Extensions;
using FantasyCritic.Web.Models;
using FantasyCritic.Web.Models.Requests;
using FantasyCritic.Web.Models.Responses;
using FantasyCritic.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Pages.Account.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace FantasyCritic.Web.Controllers.API
{
    [Route("api/[controller]/[action]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AccountController : Controller
    {
        private readonly FantasyCriticUserManager _userManager;
        private readonly FantasyCriticRoleManager _roleManager;
        private readonly SignInManager<FantasyCriticUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ITokenService _tokenService;
        private readonly IClock _clock;

        public AccountController(FantasyCriticUserManager userManager, FantasyCriticRoleManager roleManager, SignInManager<FantasyCriticUser> signInManager, 
            IEmailSender emailSender, ILogger<AccountController> logger, ITokenService tokenService, IClock clock)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _tokenService = tokenService;
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
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest();
            }

            var user = new FantasyCriticUser(Guid.NewGuid(), model.UserName, model.UserName, model.EmailAddress, model.EmailAddress, false, "", "", _clock.GetCurrentInstant());
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest();
            }

            _logger.LogInformation("User created a new account with password.");

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string baseURL = $"{Request.Scheme}://{Request.Host.Value}";
            await _emailSender.SendConfirmationEmail(user, code, baseURL);

            return Created("", user.UserID.ToString());
        }

        [HttpPost]
        public async Task<IActionResult> ResendConfirmationEmail()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string baseURL = $"{Request.Scheme}://{Request.Host.Value}";
            await _emailSender.SendConfirmationEmail(user, code, baseURL);

            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByEmailAsync(request.EmailAddress);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                return Ok();
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            string baseURL = $"{Request.Scheme}://{Request.Host.Value}";
            await _emailSender.SendForgotPasswordEmail(user, code, baseURL);

            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SendChangeEmail([FromBody] SendChangeEmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByEmailAsync(request.OldEmailAddress);
            if (user == null)
            {
                return BadRequest();
            }

            var code = await _userManager.GenerateChangeEmailTokenAsync(user, request.NewEmailAddress);
            string baseURL = $"{Request.Scheme}://{Request.Host.Value}";
            await _emailSender.SendChangeEmail(user, code, baseURL);

            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByEmailAsync(request.EmailAddress);
            if (user == null)
            {
                return Ok();
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Code, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByEmailAsync(request.OldEmailAddress);
            if (user == null)
            {
                return Ok();
            }

            var result = await _userManager.ChangeEmailAsync(user, request.NewEmailAddress, request.Code);
            if (!result.Succeeded)
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserID);
            if (user == null)
            {
                return BadRequest();
            }

            var result = await _userManager.ConfirmEmailAsync(user, request.Code);
            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.EmailAddress);
            if (user == null)
            {
                return BadRequest();
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, false, false);
            if (!result.Succeeded)
            {
                return BadRequest();
            }

            return await GetToken(user);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUserName([FromBody] ChangeUserNameRequest request)
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

            user.UserName = request.NewUserName;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest();
            }

            return await GetToken(user);
        }

        private async Task<ObjectResult> GetToken(FantasyCriticUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var usersClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            };

            foreach (var role in roles)
            {
                usersClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var jwtToken = _tokenService.GenerateAccessToken(usersClaims);
            var refreshToken = _tokenService.GenerateRefreshToken();
            await _userManager.AddRefreshToken(user, refreshToken);
            var jwtString = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            return new ObjectResult(new
            {
                token = jwtString,
                refreshToken = refreshToken,
                expiration = jwtToken.ValidTo
            });
        }
    }
}
