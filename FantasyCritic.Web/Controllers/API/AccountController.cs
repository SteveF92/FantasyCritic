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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Web.Controllers.API
{
    [Route("api/[controller]/[action]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AccountController : Controller
    {
        private readonly FantasyCriticUserManager _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ITokenService _tokenService;

        public AccountController(FantasyCriticUserManager userManager, IEmailSender emailSender, ILogger<AccountController> logger, ITokenService tokenService)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
            _tokenService = tokenService;
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

            var user = new FantasyCriticUser(Guid.NewGuid(), model.UserName, model.UserName, model.RealName, model.EmailAddress, model.EmailAddress, false, "", "");
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

            var usersClaims = new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString())
            };

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
