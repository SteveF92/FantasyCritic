using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Controllers;
using FantasyCritic.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NodaTime;

namespace FantasyCritic.Web.Controllers.API
{
    [Route("api/[controller]/[action]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AuthorizationController : Controller
    {
        private readonly FantasyCriticUserManager _userManager;
        private readonly SignInManager<FantasyCriticUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ISMSSender _smsSender;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IClock _clock;

        public AuthorizationController(FantasyCriticUserManager userManager, SignInManager<FantasyCriticUser> signInManager, IEmailSender emailSender,
            ISMSSender smsSender, ILogger<AuthorizationController> logger, IConfiguration configuration, IClock clock)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _logger = logger;
            _configuration = configuration;
            _clock = clock;
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateToken([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            var passwordResult = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, false, false);
            if (passwordResult.Succeeded)
            {
                return GetToken(user);
            }
     
            return BadRequest();
        }

        private CreatedResult GetToken(FantasyCriticUser user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.EmailAddress),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
            };

            var keyString = _configuration["Tokens:Key"];
            var issuer = _configuration["Tokens:Issuer"];
            var audience = _configuration["Tokens:Audience"];

            int validMinutes = Convert.ToInt32(_configuration["Tokens:ValidMinutes"]);
            var expiration = _clock.GetCurrentInstant().Plus(Duration.FromMinutes(validMinutes)).ToDateTimeUtc();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(issuer, audience, claims, expires: expiration, signingCredentials: creds);
            token.Payload["roles"] = _userManager.GetRolesAsync(user).Result;
            var results = new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            };

            CreatedResult tokenResult = Created("", results);

            return tokenResult;
        }

        #region Helpers

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        #endregion
    }
}
