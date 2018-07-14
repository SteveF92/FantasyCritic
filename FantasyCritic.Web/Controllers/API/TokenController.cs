using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FantasyCritic.Web.Controllers.API
{
    public class TokenController : Controller
    {
        private readonly ITokenService _tokenService;
        private readonly FantasyCriticUserManager _userManager;

        public TokenController(ITokenService tokenService, FantasyCriticUserManager userManager)
        {
            _tokenService = tokenService;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Refresh(string token, string refreshToken)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(token);
            var username = principal.Identity.Name; //this is mapped to the Name claim by default

            var user = await _userManager.FindByNameAsync(username);
            if (user == null || user.RefreshToken != refreshToken)
            {
                return BadRequest();
            }

            var newJwtToken = _tokenService.GenerateAccessToken(principal.Claims);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            var newJwtString = new JwtSecurityTokenHandler().WriteToken(newJwtToken);

            return new ObjectResult(new
            {
                token = newJwtString,
                refreshToken = refreshToken,
                expiration = newJwtToken.ValidTo
            });
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> Revoke()
        {
            var username = User.Identity.Name;

            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return BadRequest();

            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);

            return NoContent();
        }

    }
}
