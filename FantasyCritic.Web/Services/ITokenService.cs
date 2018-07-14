using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FantasyCritic.Web.Services
{
    public interface ITokenService
    {
         JwtSecurityToken GenerateAccessToken(IEnumerable<Claim> claims);         
         string GenerateRefreshToken();    
         ClaimsPrincipal GetPrincipalFromExpiredToken(string token);            
    }
}
