namespace FantasyCritic.Web.Models.Requests.Account;

public class TokenRefreshRequest
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
}