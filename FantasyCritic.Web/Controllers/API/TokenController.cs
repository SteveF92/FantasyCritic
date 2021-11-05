using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Models;
using FantasyCritic.Web.Models.Requests;
using FantasyCritic.Web.Models.Requests.Account;
using FantasyCritic.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace FantasyCritic.Web.Controllers.API
{
    [Route("api/[controller]/[action]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TokenController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly FantasyCriticUserManager _userManager;

        public TokenController(ITokenService tokenService, FantasyCriticUserManager userManager)
        {
            _tokenService = tokenService;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Refresh([FromBody] TokenRefreshRequest request)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(request.Token);
            var emailAddress = principal.Identity.Name; //this is mapped to the Name claim by default

            var user = await _userManager.FindByNameAsync(emailAddress);

            if (user == null)
            {
                return BadRequest();
            }

            var refreshTokens = await _userManager.GetRefreshTokens(user);
            if (!refreshTokens.Contains(request.RefreshToken))
            {
                return BadRequest();
            }

            string issuedTimeString = principal.Claims.FirstOrDefault(x => x.Type == "nbf")?.Value;
            if (issuedTimeString == null)
            {
                return BadRequest("Invalid JWT.");
            }

            Instant issuedTime = Instant.FromUnixTimeSeconds(Convert.ToInt64(issuedTimeString));
            if (issuedTime < user.GetLastChangedCredentials())
            {
                return StatusCode(401);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = user.GetUserClaims(roles);
            var newJwtToken = _tokenService.GenerateAccessToken(claims);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            await _userManager.RemoveRefreshToken(user, request.RefreshToken);
            await _userManager.AddRefreshToken(user, newRefreshToken);

            var newJwtString = new JwtSecurityTokenHandler().WriteToken(newJwtToken);

            return new ObjectResult(new
            {
                token = newJwtString,
                refreshToken = newRefreshToken,
                expiration = newJwtToken.ValidTo
            });
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> Revoke()
        {
            var emailAddress = User.Identity.Name;

            var user = await _userManager.FindByNameAsync(emailAddress);
            if (user == null) return BadRequest();

            await _userManager.RemoveAllRefreshTokens(user);

            return NoContent();
        }

    }
}
